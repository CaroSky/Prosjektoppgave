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
using System.Text;
using System;
using Microsoft.Extensions.Configuration;

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
            var userManagerMock = MockHelpers.MockUserManager<IdentityUser>();
            var configuration = new Mock<IConfiguration>();
            var controller = new AccountsController(userManagerMock.Object, null, configuration.Object, loggerMock.Object);

            var validModel = new RegisterModel
            {
                Email = "test@example.com",
                Password = "TestPassword123"
            };

            userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.Register(validModel) as OkObjectResult; ;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

        }

        [TestMethod]
        public async Task Register_InvalidModel_ReturnsOkWithErrors()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AccountsController>>();
            var userManagerMock = MockHelpers.MockUserManager<IdentityUser>();
            var configuration = new Mock<IConfiguration>();
            var controller = new AccountsController(userManagerMock.Object, null, configuration.Object, loggerMock.Object);

            var invalidModel = new RegisterModel
            {
                // Omitted required fields intentionally to make it invalid
            };

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            };

            userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await controller.Register(invalidModel) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            var registerResult = result.Value as RegisterResult;
            Assert.IsNotNull(registerResult);
            Assert.IsFalse(registerResult.Successful);
            Assert.IsNotNull(registerResult.Errors);
        }

 
    }
}