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
using SharedModels.ViewModels;
using TestProject1;
using WebAPI.Models.ViewModels;

namespace ProjectTest
{
    [TestClass]
    public class NotificationControllerTests
    {
        private NotificationController _notificationController;
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<NotificationController>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockLogger = new Mock<ILogger<NotificationController>>();
            _notificationController = new NotificationController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object);
        }


        [TestMethod]
        public async Task GetNotificationPosts_ReturnsOkWithPostList()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));

            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            var NotificationList = new List<Notification>(); 

            _mockRepository.Setup(r => r.GetAllNotificationsForUser("a")).ReturnsAsync(NotificationList);
            _mockRepository.Setup(r => r.GetAllLikesForUser("a")).ReturnsAsync(new List<Like>()); 
            _mockRepository.Setup(r => r.GetPostById(It.IsAny<int>())).ReturnsAsync(new Post());

            // Act
            var result = await _notificationController.GetNotificationPosts() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        [TestMethod]
        public async Task GetNotificationsCount_ReturnsOkWithCount()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));

            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });


            _mockRepository.Setup(r => r.GetNotificationsCountForUser("a")).ReturnsAsync(2);

            // Act
            var result = await _notificationController.GetNotificationsCount() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        [TestMethod]
        public async Task DeleteOne_ReturnsOkWithDeletedNotification()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));

            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });
            var notification = new Notification() {PostId = 1, UserId = "a" };
            _mockRepository.Setup(r => r.GetNotification(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new Notification());
            _mockRepository.Setup(repo => repo.DeleteNotification(notification))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _notificationController.Delete(1) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode); ;
        }



        [TestMethod]
        public async Task DeleteOne_NotSignedIN_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));

            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);
            var notification = new Notification() { PostId = 1, UserId = "a" };
            _mockRepository.Setup(r => r.GetNotification(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new Notification());
            _mockRepository.Setup(repo => repo.DeleteNotification(notification))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _notificationController.Delete(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Delete_ReturnsOkWithDeletedNotification()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));

            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _mockRepository.Setup(r => r.GetNotification(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new Notification());
            _mockRepository.Setup(repo => repo.DeleteAllNotificationsForUser("a"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _notificationController.Delete() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode); ;
        }



        [TestMethod]
        public async Task Delete_ReturnsUnauthorizedWhenUserNotAuthenticated()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _notificationController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);
            _mockRepository.Setup(r => r.GetNotification(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new Notification());



            // Act
            var result = await _notificationController.Delete();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

    }

}