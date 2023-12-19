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
using System.Security.Principal;

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
        [TestMethod]
        public async Task Put_Post_ValidModel_UpdatesPost()
        {
            // Arrange
            var postId = 1;
            var postEditViewModel = new PostEditViewModel
            {
                PostId = postId,
                Title = "Updated Post Title",
                Content = "Updated content",
                BlogId = 1,
                IsCommentAllowed = true
            };

            var post = new Post { PostId = postId, Title = "Original Post Title" };
            _mockRepository.Setup(repo => repo.GetPostById(postId))
                           .ReturnsAsync(post);

            bool updatePostCalled = false;
            _mockRepository.Setup(repo => repo.UpdatePost(It.IsAny<Post>(), It.IsAny<IPrincipal>()))
                           .Callback<Post, IPrincipal>((p, u) => updatePostCalled = true)
                           .Returns(Task.CompletedTask);

            var user = new IdentityUser { UserName = "testuser", Id = "userid" };
            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            var controller = new PostController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockHubContext.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object }
            };

            // Act
            var result = await controller.Put(postId, postEditViewModel);

            // Assert
            Assert.IsTrue(updatePostCalled, "UpdatePost was not called.");
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // ...

        [TestMethod]
        public async Task UpdatePost_WhenPostExists_UpdatesAndReturnsNoContent()
        {
            // Arrange
            var postId = 1;
            var postToUpdate = new PostEditViewModel
            {
                PostId = postId,
                Title = "Updated Title",
                Content = "Updated Content"
               
            };

            _mockRepository.Setup(repo => repo.GetPostById(postId))
                           .ReturnsAsync(new Post { PostId = postId });
            _mockRepository.Setup(repo => repo.UpdatePost(It.IsAny<Post>(), It.IsAny<ClaimsPrincipal>()))
                           .Returns(Task.CompletedTask);

            var user = new IdentityUser { UserName = "testuser", Id = "userid" };
            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName)
    };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            var controller = new PostController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockHubContext.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object }
            };

            // Act
            var result = await controller.Put(postId, postToUpdate);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }


        [TestMethod]
        public async Task DeletePost_WhenPostExists_DeletesAndReturnsOk()
        {
            // Arrange
            var postId = 1;
            _mockRepository.Setup(repo => repo.GetPostById(postId))
                           .ReturnsAsync(new Post { PostId = postId });
            _mockRepository.Setup(repo => repo.DeletePost(It.IsAny<Post>(), It.IsAny<ClaimsPrincipal>()))
                           .Returns(Task.CompletedTask);

            var user = new IdentityUser { UserName = "testuser", Id = "userid" };
            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);

            var controller = new PostController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockHubContext.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object }
            };

            // Act
            var result = await controller.Delete(postId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Post_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var postCreateViewModel = new PostCreateViewModel
            {
                Title = "Test Title",
                Content = "Test Content",
                BlogId = 1 // Anta en gyldig blogg-ID
            };

            var mockHttpContext = new Mock<HttpContext>();
            var controller = new PostController(
                _mockUserManager.Object,
                _mockRepository.Object,
                _mockLogger.Object,
                _mockHubContext.Object)
            {
                ControllerContext = new ControllerContext() { HttpContext = mockHttpContext.Object }
            };

            // Set User to an empty ClaimsPrincipal to simulate unauthenticated access
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(new ClaimsPrincipal());

            // Act
            var result = await controller.Post(postCreateViewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }



    }
}
