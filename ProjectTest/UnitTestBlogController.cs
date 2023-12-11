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
    public class BlogControllerTests
    {
        private BlogController _blogController;
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<BlogController>> _mockLogger;
        private Mock<SignInManager<IdentityUser>> _mockSignInManager;
        private IAuthorizationService _authorizationService;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockLogger = new Mock<ILogger<BlogController>>();
            _mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                _mockUserManager.Object,
                new HttpContextAccessor(),
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                null, null, null, null);

            _blogController = new BlogController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object, _mockSignInManager.Object);
        }

        [TestMethod]
        public async Task GetBlogs_ReturnsOkResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));

            _blogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { /* configure the user properties as needed */ });

            _mockRepository.Setup(r => r.GetAllBlogs())
                .ReturnsAsync(new List<Blog>()); // You may need to replace 'Blog' with your actual blog class

            // Act
            var result = await _blogController.GetBlogs() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task Post_ValidBlog_ReturnsCreatedAtAction()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "http://example.com:testuser");
            var user = new IdentityUser { Id = "1", UserName = "testuser" };
            var blog = new Blog { BlogId = 1, Title = "Test Blog" };

            _blogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { userIdClaim }))
                }
            };

            _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);

            // Act
            var result = await _blogController.Post(blog);
            var createdAtActionResult = result as CreatedAtActionResult;

            // Assert
            _mockUserManager.Verify(x => x.FindByNameAsync("testuser"), Times.Once);
            _mockRepository.Verify(x => x.SaveBlog(blog, _blogController.User), Times.Once);

            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual("Get", createdAtActionResult.ActionName);
            Assert.AreEqual(201, createdAtActionResult.StatusCode);
            Assert.AreEqual(blog, createdAtActionResult.Value);
        }

 

        [TestMethod]
        public async Task Put_ReturnsOkForValidBlogUpdate()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "http://example.com:testuser");
            var user = new IdentityUser { Id = "1", UserName = "testuser" };
            var blogId = 1;
            var blog = new Blog { BlogId = blogId, OwnerId = user.Id };

            _blogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { userIdClaim }))
                }
            };

            _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
            _mockRepository.Setup(x => x.UpdateBlog(It.IsAny<Blog>())).Returns(Task.CompletedTask);

            // Act
            var result = await _blogController.Put(blogId, blog);
            var okResult = result as OkObjectResult;

            // Assert
            _mockUserManager.Verify(x => x.FindByNameAsync("testuser"), Times.Once);
            _mockRepository.Verify(x => x.UpdateBlog(blog), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(blog, okResult.Value);
        }


        [TestMethod]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            // Arrange
            var blogId = 1;
            var blog = new Blog { BlogId = blogId, Title = "Test Blog" };

            _mockRepository.Setup(x => x.GetBlogById(blogId)).ReturnsAsync(blog);
            _mockRepository.Setup(x => x.DeleteBlog(blog, It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            // Act
            var result = await _blogController.Delete(blogId);
            var okResult = result as OkObjectResult;

            // Assert
           // _mockLogger.Verify(x => x.LogInformation("Handling DELETE request for blog with ID: 1"), Times.Once);
            _mockRepository.Verify(x => x.GetBlogById(blogId), Times.Once);
            _mockRepository.Verify(x => x.DeleteBlog(blog, It.IsAny<ClaimsPrincipal>()), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(blog, okResult.Value);
        }


        [TestMethod]
        public async Task Subscribe_ValidBlogId_ReturnsOkResult()
        {
            // Arrange
            var blogId = 1;
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "http://example.com:testuser");
            var user = new IdentityUser { Id = "1", UserName = "testuser" };

            _blogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { userIdClaim }))
                }
            };

            _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
            _mockRepository.Setup(x => x.SubscribeToBlog(user.Id, blogId)).Returns(Task.CompletedTask);

            // Act
            var result = await _blogController.Subscribe(blogId);
            var okResult = result as OkResult;

            // Assert
            _mockUserManager.Verify(x => x.FindByNameAsync("testuser"), Times.Once);
            _mockRepository.Verify(x => x.SubscribeToBlog(user.Id, blogId), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public async Task Unsubscribe_ReturnsOkResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "http://example.com:testuser");
            var user = new IdentityUser { Id = "1", UserName = "testuser" };

            _blogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { userIdClaim }))
                }
            };

            _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
            _mockRepository.Setup(x => x.UnsubscribeFromBlog(user.Id, It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            var result = await _blogController.Unsubscribe(1);
            var okResult = result as OkResult;

            // Assert
            _mockRepository.Verify(x => x.UnsubscribeFromBlog(user.Id, 1), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public async Task GetAllSubscriptionStatuses_ReturnsOkResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "http://example.com:testuser");
            var user = new IdentityUser { Id = "1", UserName = "testuser" };
            var subscriptionStatuses = new Dictionary<int, bool>();
            subscriptionStatuses[1] = true;

            _blogController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { userIdClaim }))
                }
            };

            _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
            _mockRepository.Setup(x => x.GetAllSubscriptionStatuses(user.Id)).ReturnsAsync(subscriptionStatuses);

            // Act
            var result = await _blogController.GetAllSubscriptionStatuses();
            var okResult = result as OkObjectResult;

            // Assert
            _mockRepository.Verify(x => x.GetAllSubscriptionStatuses(user.Id), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(subscriptionStatuses, okResult.Value);
        }


    }
}