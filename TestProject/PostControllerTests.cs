using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebAPI.Controllers;
using System.Security.Claims; 
using SharedModels.Entities;
using SharedModels.ViewModels;
using WebAPI.Models.Repositories;
using WebAPI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PostControllerTests
{
    [TestClass]
    public class PostControllerTests
    {
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<PostController>> _mockLogger;
        private Mock<IHubContext<NotificationHub>> _mockHubContext;
        private object mockHttpContext;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialiser Mock-objekter her
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<PostController>>();
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            // Mock HttpContext for controlleren
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
    {
        new Claim(ClaimTypes.NameIdentifier, "testUserId"),
        new Claim(ClaimTypes.Name, "testUserName")
    }));

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                            .ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            // Du skal ikke bruke PostController.ControllerContext her
            // Fjern denne linjen: PostController.ControllerContext = new ControllerContext() ...

            // Opprette en ny instans av PostController og tilordne ControllerContext til den instansen
            var controller = new PostController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockHubContext.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };

        }

        [TestMethod]
        public async Task GetPosts_ReturnsPostIndexViewModel()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var controller = new PostController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockHubContext.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Set up claims principal for the user context
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "testUserId"),
        new Claim(ClaimTypes.Name, "testUserName")
            }));

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                            .ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });

            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            int testBlogId = 1; // Tester for blogg id 1
            _mockRepository.Setup(repo => repo.GetBlogById(testBlogId))
                           .ReturnsAsync(new Blog()); // Returner en mock Blog objekt her

            // Act
            var result = await controller.GetPosts(testBlogId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PostIndexViewModel));
        }
       
        [TestMethod]
        public async Task Post_ValidModel_CreatesPostAndReturnsCreatedAtAction()
        {
            // Arrange
            var postCreateViewModel = new PostCreateViewModel
            {
                Title = "New Post Title",
                Content = "New post content",
                BlogId = 1
            };
            var user = new IdentityUser { UserName = "testuser", Id = "userid" };

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockRepository.Setup(repo => repo.GetBlogById(It.IsAny<int>())).ReturnsAsync(new Blog { BlogId = 1, Title = "Test Blog" });
            _mockRepository.Setup(repo => repo.SavePost(It.IsAny<Post>(), It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            var mockHttpContext = new Mock<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName)
            }));

            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            var controller = new PostController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object, _mockHubContext.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var result = await controller.Post(postCreateViewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult), "Result is not of type CreatedAtActionResult");
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult, "CreatedAtActionResult is null");
            Assert.IsInstanceOfType(createdAtActionResult.Value, typeof(Post), "CreatedAtActionResult.Value is not of type Post");
            var postResult = createdAtActionResult.Value as Post;
        }

    }
}
