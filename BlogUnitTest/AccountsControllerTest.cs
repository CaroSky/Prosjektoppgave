using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using WebAPI.Controllers;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BlogUnitTest
{
    [TestClass]
    public class AccountsControllerTests
    {
        private Mock<UserManager<IdentityUser>> userManagerMock;
        private FakeSignInManager signInManager;
        private Mock<IConfiguration> configurationMock;
        private Mock<ILogger<AccountsController>> loggerMock;
        private AccountsController controller;

        [TestInitialize]
        public void Initialize()
        {
            // Mock the required dependencies for UserManager
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Mock the required dependencies for SignInManager
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
            var loggerSignInManagerMock = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();

            signInManager = new FakeSignInManager(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsAccessorMock.Object,
                loggerSignInManagerMock.Object,
                schemesMock.Object,
                confirmationMock.Object);

            // Mock the IConfiguration and ILogger for AccountsController
            configurationMock = new Mock<IConfiguration>();
            loggerMock = new Mock<ILogger<AccountsController>>();

            // Setup configuration for JWT
            configurationMock.SetupGet(c => c["Jwt:Key"]).Returns("RANDOM_KEY_MUST_NOT_BE_SHARED");
            configurationMock.SetupGet(c => c["Jwt:Issuer"]).Returns("https://localhost");
            configurationMock.SetupGet(c => c["Jwt:Audience"]).Returns("https://localhost");

            // Instantiate the AccountsController with mocked dependencies
            controller = new AccountsController(
                userManagerMock.Object,
                signInManager,
                configurationMock.Object,
                loggerMock.Object);
        }

        [TestMethod]
        public async Task Login_ReturnsJwtToken_WhenCredentialsAreValid()
        {
            // Arrange
            var testUser = new IdentityUser { Email = "test@example.com", UserName = "testUser" };
            userManagerMock.Setup(um => um.FindByEmailAsync("test@example.com")).ReturnsAsync(testUser);

            var loginModel = new LoginModel { Email = "test@example.com", Password = "Password123" };

            // Act
            var result = await controller.Login(loginModel) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var loginResult = result.Value as LoginResult;
            Assert.IsTrue(loginResult.Successful);
            Assert.IsFalse(string.IsNullOrEmpty(loginResult.Token));
        }

        [TestMethod]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            signInManager.SetSignInResult(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var loginModel = new LoginModel { Email = "test@example.com", Password = "WrongPassword" };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }
        [TestMethod]
        public async Task Register_ReturnsSuccess_WhenUserIsSuccessfullyCreated()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Success);

            var controller = new AccountsController(userManagerMock.Object, null, null, null);

            var registerModel = new RegisterModel { Email = "test@example.com", Password = "Password123" };

            // Act
            var result = await controller.Register(registerModel) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var registerResult = result.Value as RegisterResult;
            Assert.IsTrue(registerResult.Successful);
        }

        private Mock<UserManager<IdentityUser>> CreateUserManagerMock()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }
        [TestMethod]
        public async Task Register_ReturnsFailure_WhenUserCreationFails()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                           .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Test Error" }));

            var controller = new AccountsController(userManagerMock.Object, null, null, null);

            var registerModel = new RegisterModel { Email = "test@example.com", Password = "Password123" };

            // Act
            var result = await controller.Register(registerModel) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var registerResult = result.Value as RegisterResult;
            Assert.IsFalse(registerResult.Successful);
            Assert.IsTrue(registerResult.Errors.Contains("Test Error"));
        }

        [TestMethod]
        public void Logout_ReturnsSuccessfulMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AccountsController>>();
            var controller = new AccountsController(null, null, null, loggerMock.Object);

            // Act
            var result = controller.Logout() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);

            // Serialize the result to JSON and check if it contains the expected message
            var jsonResult = JsonConvert.SerializeObject(result.Value);
            Assert.IsTrue(jsonResult.Contains("Logout successful. Please clear the token on the client side."));
        }
    }

    public class FakeSignInManager : SignInManager<IdentityUser>
    {
        private Microsoft.AspNetCore.Identity.SignInResult signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;

        public FakeSignInManager(UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<IdentityUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<IdentityUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<IdentityUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public void SetSignInResult(Microsoft.AspNetCore.Identity.SignInResult result)
        {
            signInResult = result;
        }

        public override Task<Microsoft.AspNetCore.Identity.SignInResult> PasswordSignInAsync(IdentityUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return Task.FromResult(signInResult);
        }

    }
}
