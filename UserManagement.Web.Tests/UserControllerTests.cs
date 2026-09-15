using System;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void List_WhenFilteredByActive_MustUseFilteredUsers(bool isActive)
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var matching = SetupUsers(email: "match@example.com", isActive: isActive);
        _userService.Setup(s => s.FilterByActive(isActive)).Returns(matching);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List(isActive);

        // Assert: Verifies that the action of the method under test behaves as expected.
        var model = result.Model.Should().BeOfType<UserListViewModel>().Subject;
        model.Items.Should().ContainSingle().Which.Email.Should().Be("match@example.com");
        model.IsActive.Should().Be(isActive);

        _userService.Verify(s => s.GetAll(), Times.Never);
    }

    [Fact]
    public void List_WhenServiceReturnsUsers_MustMapDateOfBirth()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();
        users[0].DateOfBirth = new DateOnly(1975, 3, 14);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model.Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().ContainSingle()
            .Which.DateOfBirth.Should().Be(new DateOnly(1975, 3, 14));
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private UsersController CreateController() => new(_userService.Object);
}
