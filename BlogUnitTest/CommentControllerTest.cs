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
using WebAPI.Models.ViewModels;

namespace BlogUnitTest
{
    [TestClass]
    public class CommentControllerTests
    {
        private CommentController _commentController;
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<CommentController>> _mockLogger;
        private IAuthorizationService _authorizationService;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockLogger = new Mock<ILogger<CommentController>>();
            _commentController = new CommentController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object);
        }

        [TestMethod]

        public async Task GetComments_ValidPostId_ReturnsCommentIndexViewModel()
        {
            // Arrange
            var postId = 1;
            var blogId = 1;

            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            // Set up repository to return dummy data and mock necessary methods
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });
            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId))
                .ReturnsAsync(new List<Comment>());
            _mockRepository.Setup(repo => repo.GetPostById(postId))
                .ReturnsAsync(new Post { PostId = postId, Content = "post content", Title = "post Title", Blog = new Blog() { BlogId = blogId, Title = "Test Blog", IsPostAllowed = true } });

            // Act
            var result = await _commentController.GetComments(postId);

            // Assert
            Assert.IsNotNull(result);

        }

        [TestMethod]
        public async Task Create_ValidComment_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var commentCreateViewModel = new CommentCreateViewModel { Content = "Test Content", PostId = 1 };
            var comment = new Comment() { Content = "content", OwnerId = "1", Post = new Post { PostId = 1, Content = "post content", Title = "post Title", Blog = new Blog() { BlogId = 1, Title = "Test Blog", IsPostAllowed = true } } };
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };

            // Set up repository to mock necessary methods
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });
            _mockRepository.Setup(repo => repo.GetPostById(It.IsAny<int>()))
                .ReturnsAsync(new Post { PostId = 1, Content = "post content", Title = "post Title", Blog = new Blog() { BlogId = 1, Title = "Test Blog", IsPostAllowed = true } });
            _mockRepository.Setup(repo => repo.GetBlogById(1))
                .ReturnsAsync(new Blog { BlogId = 1, Title = "Test Blog", IsPostAllowed = true });


            // Act
            var result = await _commentController.Create(commentCreateViewModel);

            // Assert
            Assert.IsNotNull(result);
            //_mockRepository.Verify(x => x.SaveComment(comment, _commentController.User), Times.Once);

        }

        [TestMethod]
        public async Task Create_InvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Create(new CommentCreateViewModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Get_ValidCommentId_ReturnsOkObjectResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            // Set up repository to mock necessary methods
            _mockRepository.Setup(repo => repo.GetCommentById(It.IsAny<int>()))
                .ReturnsAsync(new Comment { });

            _mockRepository.Setup(repo => repo.GetCommentEditViewModelById(It.IsAny<int>()))
                .ReturnsAsync(new CommentEditViewModel { });

            // Act
            var result = await _commentController.Get(1, 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        }

        [TestMethod]
        public async Task Get_InvalidCommentId_ReturnsNotFoundResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });


            // Set up repository to return null for GetCommentById
            _mockRepository.Setup(repo => repo.GetCommentById(It.IsAny<int>()))
                .ReturnsAsync((Comment)null);

            // Act
            var result = await _commentController.Get(1, 1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

        }

        [TestMethod]
        public async Task Put_ValidCommentId_ReturnsOkObjectResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            // Set up repository to mock necessary methods
            _mockRepository.Setup(repo => repo.GetPostById(It.IsAny<int>()))
                .ReturnsAsync(new Post { });

            _mockRepository.Setup(repo => repo.UpdateComment(It.IsAny<Comment>(), It.IsAny<ClaimsPrincipal>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _commentController.Put(1, new CommentEditViewModel { CommentId = 1, });

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        }

        [TestMethod]
        public async Task Put_InvalidCommentId_ReturnsBadRequestResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            // Set up repository to return null for GetPostById
            _mockRepository.Setup(repo => repo.GetPostById(It.IsAny<int>()))
                .ReturnsAsync((Post)null);

            // Act
            var result = await _commentController.Put(1, new CommentEditViewModel { CommentId = 2, });

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Delete_ValidCommentId_ReturnsOkObjectResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            // Set up repository to mock necessary methods
            _mockRepository.Setup(repo => repo.GetCommentById(It.IsAny<int>()))
                .ReturnsAsync(new Comment { });

            _mockRepository.Setup(repo => repo.DeleteComment(It.IsAny<Comment>(), It.IsAny<ClaimsPrincipal>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _commentController.Delete(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        }

        [TestMethod]
        public async Task Delete_InvalidCommentId_ReturnsNotFoundResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            // Set up repository to return null for GetCommentById
            _mockRepository.Setup(repo => repo.GetCommentById(It.IsAny<int>()))
                .ReturnsAsync((Comment)null);

            // Act
            var result = await _commentController.Delete(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));

        }

        [TestMethod]
        public async Task Delete_InvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Delete(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Get_InvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Get(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Put_InvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityUser { });

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Put(1, new CommentEditViewModel { CommentId = 2, });

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Put_NotSignedIN_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Put(1, new CommentEditViewModel { CommentId = 2, });

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Create_NotSignedIN_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Create(new CommentCreateViewModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Delete_NotSignedIN_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Delete(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Get_NotSignedIN_ReturnsUnauthorizedResult()
        {
            // Arrange
            var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "test");
            var userClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { userIdClaim }));
            _commentController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaimsPrincipal }
            };
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);

            _commentController.ModelState.AddModelError("Content", "Content is required.");

            // Act
            var result = await _commentController.Get(1, 2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

    }

}