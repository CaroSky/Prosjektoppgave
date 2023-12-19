using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using SharedModels.Entities;
using System.Security.Claims;
using WebAPI.Controllers;
using WebAPI.Hubs;
using WebAPI.Models.Repositories;

namespace BlogControllerTests
{
    [TestClass]
    public class BlogControllerTests
    {
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<BlogController>> _mockLogger;
        private Mock<SignInManager<IdentityUser>> _mockSignInManager;
        private Mock<HttpContext> mockHttpContext;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize Mock objects here
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<BlogController>>();
            _mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                null, null, null, null);

            mockHttpContext = new Mock<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId"),
                new Claim(ClaimTypes.Name, "testUserName")
            }));

            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);
        }
        [TestMethod]
        public async Task GetBlogs_ReturnsBlogs()
        {
            // Arrange
            var controller = new BlogController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockSignInManager.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            _mockRepository.Setup(repo => repo.GetAllBlogs()).ReturnsAsync(new List<Blog>());

            // Act
            var result = await controller.GetBlogs();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task TestCreateBlog()
        {
            // Arrange
            var blog = new Blog
            {
                Title = "Test Blog",
                Content = "This is test blog content",
                Created = DateTime.Now,
                IsPostAllowed = true
                // OwnerId blir satt i kontrolleren basert på den innloggede brukeren
            };

            var mockHttpContext = new Mock<HttpContext>();
            var controller = new BlogController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockSignInManager.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "testUserId"),
        new Claim(ClaimTypes.Name, "testUserName")
            }));
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });
            _mockRepository.Setup(repo => repo.SaveBlog(It.IsAny<Blog>(), It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.Post(blog);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
            _mockRepository.Verify(repo => repo.SaveBlog(It.Is<Blog>(b => b.Title == blog.Title && b.Content == blog.Content && b.IsPostAllowed == blog.IsPostAllowed), It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
        [TestMethod]
        public async Task TestEditBlog()
        {
            // Arrange
            var blog = new Blog { BlogId = 1, Title = "Updated Blog", Content = "Updated Content", OwnerId = "testUserId" };
            var mockHttpContext = new Mock<HttpContext>();
            var controller = new BlogController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockSignInManager.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "testUserId"),
        new Claim(ClaimTypes.Name, "testUserName")
            }));
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });
            _mockRepository.Setup(repo => repo.GetBlogById(blog.BlogId)).ReturnsAsync(blog);
            _mockRepository.Setup(repo => repo.UpdateBlog(It.IsAny<Blog>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.Put(blog.BlogId, blog);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }
        [TestMethod]
        public async Task TestDeleteBlog()
        {
            // Arrange
            var blogId = 1;
            var controller = new BlogController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object, _mockSignInManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object }
            };

            _mockRepository.Setup(repo => repo.GetBlogById(blogId)).ReturnsAsync(new Blog { BlogId = blogId });
            _mockRepository.Setup(repo => repo.DeleteBlog(It.IsAny<Blog>(), It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.Delete(blogId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task TestBlogSubscription()
        {
            // Arrange
            var blogId = 1;
            var mockHttpContext = new Mock<HttpContext>();
            var controller = new BlogController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object, _mockSignInManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object }
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "testUserId"),
            new Claim(ClaimTypes.Name, "testUserName")
            }));
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });
            _mockRepository.Setup(repo => repo.SubscribeToBlog(It.IsAny<string>(), blogId)).Returns(Task.CompletedTask);
            _mockRepository.Setup(repo => repo.UnsubscribeFromBlog(It.IsAny<string>(), blogId)).Returns(Task.CompletedTask);

            // Act
            var subscribeResult = await controller.Subscribe(blogId);
            var unsubscribeResult = await controller.Unsubscribe(blogId);

            // Assert
            Assert.IsInstanceOfType(subscribeResult, typeof(OkResult));
            Assert.IsInstanceOfType(unsubscribeResult, typeof(OkResult));
        }

        [TestMethod]
        public async Task TestGetAllSubscriptionStatuses()
        {
            // Arrange
            var userId = "testUserId";
            var userName = "testUserName";
            var mockHttpContext = new Mock<HttpContext>();

            var controller = new BlogController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object, _mockSignInManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object }
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
             {
            new Claim(ClaimTypes.NameIdentifier, "testUserId"),
            new Claim(ClaimTypes.Name, "testUserName")
             }));
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);



            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });
            _mockRepository.Setup(repo => repo.GetAllSubscriptionStatuses(userId)).ReturnsAsync(new Dictionary<int, bool>());

            // Act
            var result = await controller.GetAllSubscriptionStatuses();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }


    }
}