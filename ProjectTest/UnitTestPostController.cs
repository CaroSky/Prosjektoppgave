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
    public class PostControllerTests
    {
        private PostController _postController;
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<PostController>> _mockLogger;
        private IAuthorizationService _authorizationService;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockLogger = new Mock<ILogger<PostController>>();
            _postController = new PostController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetPosts_ReturnsPostIndexViewModel()
        {
            var id = 1;
            // Set up claims identity for the user
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            // Set up UserManager to return a user when FindByNameAsync is called
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });


            // Set up repository to return dummy data
            _mockRepository.Setup(repo => repo.GetBlogById(id))
                .ReturnsAsync(new Blog { BlogId = id, Title = "Test Blog", IsPostAllowed = true });
            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(id))
                .ReturnsAsync(new List<Post>());
            _mockRepository.Setup(repo => repo.GetAllLikesForUser(It.IsAny<string>()))
                .ReturnsAsync(new List<Like> { new Like { PostId = 1 } });

            // Act
            var result = await _postController.GetPosts(id);

            // Assert
            Assert.IsNotNull(result);

            // You can also add assertions to verify that the UserManager and Repository methods were called with the expected parameters
            _mockRepository.Verify(repo => repo.GetBlogById(id), Times.Once);
            _mockRepository.Verify(repo => repo.GetAllPostByBlogId(id), Times.Once);
        }

        [TestMethod]
        public async Task Post_ValidData_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var postCreateViewModel = new PostCreateViewModel
            {
                Title = "Test Title",
                Content = "Test Content",
                BlogId = 1,
            };
   
            var username = "testuser";


            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            // Set up UserManager to return a user when FindByNameAsync is called
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser {  });

            // Set up repository to return dummy data and mock necessary methods
            _mockRepository.Setup(repo => repo.GetBlogById(postCreateViewModel.BlogId))
                .ReturnsAsync(new Blog { BlogId = postCreateViewModel.BlogId, Title = "Test Blog", IsPostAllowed = true });

            // Act
            var result = await _postController.Post(postCreateViewModel);

            // Assert
            var createdAtActionResult = result as CreatedAtActionResult; ;
            Assert.IsNotNull(createdAtActionResult);
            // Add more assertions based on your expected behavior

            // You can also add assertions to verify that the UserManager and Repository methods were called with the expected parameters
            _mockRepository.Verify(repo => repo.GetBlogById(postCreateViewModel.BlogId), Times.Once);

        }

        [TestMethod]
        public async Task Post_ValidData_VerifiesTagOperations()
        {
            // Arrange
            var postCreateViewModel = new PostCreateViewModel
            {
                Title = "Test Title",
                Content = "Test #Content",
                BlogId = 1,
            };

            var username = "testuser";


            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            // Set up UserManager to return a user when FindByNameAsync is called
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            // Set up repository to return dummy data and mock necessary methods
            _mockRepository.Setup(repo => repo.GetBlogById(postCreateViewModel.BlogId))
                .ReturnsAsync(new Blog
                    { BlogId = postCreateViewModel.BlogId, Title = "Test Blog", IsPostAllowed = true });

            _mockRepository.Setup(repo => repo.GetTagByName("Content"))
                .ReturnsAsync((Tag)null); // Assume tag does not exist by default

            // Act
            var result = await _postController.Post(postCreateViewModel);

            // Assert
            // You can add assertions based on the expected behavior of your code.
            // For example, you might want to verify that GetTagByName, SaveTag, and SavePostTag are called the expected number of times.
            _mockRepository.Verify(repo => repo.GetTagByName("Content"), Times.Once);
            _mockRepository.Verify(repo => repo.SaveTag(It.IsAny<Tag>()), Times.Once);
            _mockRepository.Verify(repo => repo.SavePostTag(It.IsAny<PostTag>()), Times.Once);
        }

        [TestMethod]
        public async Task Put_ValidData_ReturnsOkResult()
        {
            // Arrange
            var postId = 1; 
            var postEditViewModel = new PostEditViewModel
            {
                PostId = postId,
                Title = "Updated Title",
                Content = "#Updated Content",
                Created = DateTime.Now,
                IsCommentAllowed = true,
                BlogId = 1, 
                OwnerId = "testOwnerId", 
                OwnerUsername = "testOwnerUsername" 
            };
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            // Set up repository to return dummy data and mock necessary methods
            _mockRepository.Setup(repo => repo.GetBlogById(postEditViewModel.BlogId))
                .ReturnsAsync(new Blog { BlogId = postEditViewModel.BlogId, Title = "Test Blog", IsPostAllowed = true }); 
            _mockRepository.Setup(repo => repo.RemovePostTags(postId))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(repo => repo.GetTagByName(It.IsAny<string>()))
                .ReturnsAsync((Tag)null);

            _mockRepository.Setup(repo => repo.RemoveOrphanedTags())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _postController.Put(postId, postEditViewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Get_ValidData_ReturnsPostIndexViewModel()
        {
            // Arrange
            var postId = 1; 
            var blogId = 2; 
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };


            // Set up repository to return dummy data and mock necessary methods
            _mockRepository.Setup(repo => repo.GetPostById(postId))
                .ReturnsAsync(new Post { /* replace with your Post data */ }); // replace with your Post data

            // Act
            var result = await _postController.Get(postId, blogId);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Delete_ExistingPost_ReturnsOkResult()
        {
            // Arrange
            var postId = 1; // replace with your test data
            var blogId = 2; // replace with your test data

            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            // Set up repository to return dummy data and mock necessary methods
            _mockRepository.Setup(repo => repo.GetPostById(postId))
                .ReturnsAsync(new Post { /* replace with your Post data */ }); // replace with your Post data

            // Act
            var result = await _postController.Delete(postId, blogId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        }

        [TestMethod]
        public async Task Delete_NonExistingPost_ReturnsNotFoundResult()
        {
            // Arrange
            var postId = 1; // replace with your test data
            var blogId = 2; // replace with your test data

            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _postController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            // Set up repository to return null for the post
            _mockRepository.Setup(repo => repo.GetPostById(postId))
                .ReturnsAsync((Post)null);

            // Act
            var result = await _postController.Delete(postId, blogId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

        }

    }

    }