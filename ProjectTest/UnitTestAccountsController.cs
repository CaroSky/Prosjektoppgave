using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using SharedModels.Entities;
using WebAPI.Controllers;
using WebAPI.Models.Entities;
using WebAPI.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using TestProject1;
using WebAPI.Models.ViewModels;

namespace ProjectTest
{
    [TestClass]
    public class AccountsControllerTests
    {


        [TestMethod]
        public void Logout_ReturnsSuccessfulMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AccountsController>>();
            var controller = new AccountsController(null, null, null, loggerMock.Object);

            // Act
            var result = controller.Logout();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Register_ValidModel_ReturnsOkWithToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AccountsController>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var controller = new AccountsController(null, null, null, loggerMock.Object);

            var validModel = new RegisterModel
            {
                Email = "test@example.com",
                Password = "TestPassword123"
            };

            userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.Register(validModel) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.Equals(200, result.StatusCode);

            var registerResult = result.Value as RegisterResult;
            Assert.IsNotNull(registerResult);
            Assert.IsTrue(registerResult.Successful);
            Assert.IsNotNull(registerResult.Token);
        }

        [TestMethod]
        public async Task Register_InvalidModel_ReturnsOkWithErrors()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AccountsController>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var controller = new AccountsController(null, null, null, loggerMock.Object);

            var invalidModel = new RegisterModel
            {
                // Omitted required fields intentionally to make it invalid
            };

            var identityErrors = new List<IdentityError>
        {
            new IdentityError { Description = "Error 1" },
            new IdentityError { Description = "Error 2" }
        };
            var model = new RegisterModel() 
                {ConfirmPassword = "1", 
                   Email = "userManagerMock@email.com", 
                   Password = "password" };
            var newUser = new IdentityUser { UserName = model.Email, Email = model.Email };
            userManagerMock.Setup(x => x.CreateAsync(newUser, "password"))
                           .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await controller.Register(invalidModel) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.Equals(200, result.StatusCode);

            var registerResult = result.Value as RegisterResult;
            Assert.IsNotNull(registerResult);
            Assert.IsFalse(registerResult.Successful);
            Assert.IsNotNull(registerResult.Errors);
            Assert.Equals(identityErrors.Select(e => e.Description), registerResult.Errors);
        }
    }
}