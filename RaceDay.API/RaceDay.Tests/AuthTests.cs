using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Dtos;

namespace RaceDay.Tests
{
    public class AuthTests
    {
        private static RegisterRequest NewUser(string email = "eli@test.com", string role = "Organiser") => new()
        {
            FullName = "Test User",
            Email = email,
            Password = "Password123",
            Role = role
        };

        [Fact]
        public async Task Register_ValidRequest_Returns201()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession();

            var result = await controller.Register(NewUser());

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession();
            await controller.Register(NewUser());

            var result = await controller.Register(NewUser());

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession();

            var result = await controller.Register(NewUser(role: "Admin"));

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Users);
        }

        [Fact]
        public async Task Register_StoresHashedPassword_NotPlainText()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession();

            await controller.Register(NewUser());

            var user = context.Users.Single();
            Assert.False(string.IsNullOrEmpty(user.PasswordHash));
            Assert.NotEqual("Password123", user.PasswordHash);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkAndStartsSession()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession();
            await controller.Register(NewUser());

            var result = await controller.Login(new LoginRequest { Email = "eli@test.com", Password = "Password123" });

            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(controller.HttpContext.Session.GetInt32("UserId"));
            Assert.Equal("Organiser", controller.HttpContext.Session.GetString("Role"));
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            using var context = TestHelpers.CreateContext();
            var controller = new AuthController(context).WithSession();
            await controller.Register(NewUser());

            var result = await controller.Login(new LoginRequest { Email = "eli@test.com", Password = "WrongPassword" });

            Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Null(controller.HttpContext.Session.GetInt32("UserId"));
        }
    }
}