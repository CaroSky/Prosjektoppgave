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
using SharedModels.ViewModels;

namespace ProjectTest
{
    [TestClass]
    public class LikeControllerTests
    {
        private LikeController _likeController;
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<LikeController>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockLogger = new Mock<ILogger<LikeController>>();
            _likeController = new LikeController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object);
        }


        [TestMethod]
        public async Task GetLikes_ReturnsOkResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));

            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _mockRepository.Setup(r => r.GetAllLikes())
                .ReturnsAsync(new List<Like>());
            // Act
            var result = await _likeController.GetLikes();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Post_ReturnsOkResult()
        {
            // Arrange

            // Set up claims identity for the user
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            // Set up UserManager to return a user when FindByNameAsync is called
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            // Act
            var result = await _likeController.Post(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [TestMethod]
        public async Task Delete_ReturnsOkResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            // Set up UserManager to return a user when FindByNameAsync is called
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _mockRepository.Setup(repo => repo.GetLike(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Like { PostId = 1, UserId = "userId" });

            // Act
            var result = await _likeController.Delete(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [TestMethod]
        public async Task Post_NotSignedIN_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);

            // Act
            var result = await _likeController.Post(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Delete_NotSignedIN_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);

            // Act
            var result = await _likeController.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Post_ModelInvalid_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _likeController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _likeController.Post(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Delete_ModelInvalid_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _likeController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _likeController.Post(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Delete_likeNull_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _likeController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });
            _mockRepository.Setup(repo => repo.GetLike(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((Like)null);

            // Act
            var result = await _likeController.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
