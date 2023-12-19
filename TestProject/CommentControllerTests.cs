using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SharedModels.Entities;
using System.Security.Claims;
using WebAPI.Controllers;
using WebAPI.Models.Repositories;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedModels.ViewModels;

namespace CommentControllerTests
{
    [TestClass]
    public class CommentControllerTests
    {
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<CommentController>> _mockLogger;
        private Mock<HttpContext> mockHttpContext;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize Mock objects here
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<CommentController>>();
            mockHttpContext = new Mock<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId"),
                new Claim(ClaimTypes.Name, "testUserName")
            }));
            mockHttpContext.SetupGet(ctx => ctx.User).Returns(claimsPrincipal);
        }

        [TestMethod]
        public async Task TestCreateComment()
        {
            // Arrange
            var commentViewModel = new CommentCreateViewModel
            {
                Content = "created content",
                PostId = 1
            };

            var controller = new CommentController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object }
            };

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                            .ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });
            _mockRepository.Setup(repo => repo.SaveComment(It.IsAny<Comment>(), It.IsAny<ClaimsPrincipal>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await controller.Create(commentViewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
          
        }

        [TestMethod]
        public async Task TestEditComment()
        {
            // Arrange
            var commentEditViewModel = new CommentEditViewModel
            {
                CommentId = 1,
                Content = "Updated content",
                //Created = DateTime,
                PostId= 1

    };

            var controller = new CommentController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object }
            };
            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                            .ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });
            _mockRepository.Setup(repo => repo.UpdateComment(It.IsAny<Comment>(), It.IsAny<ClaimsPrincipal>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await controller.Put(commentEditViewModel.CommentId, commentEditViewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task TestDeleteComment()
        {
            // Arrange
            int commentId = 1;
            int postId = 1;
            var mockComment = new Comment
            {
                CommentId = commentId,
                Post = new Post { PostId = postId },
                OwnerId = "testUserId",
                Created = DateTime.UtcNow,
                Content = "DeleteUse"

            };


            var controller = new CommentController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object }
            };

            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                            .ReturnsAsync(new IdentityUser { Id = "testUserId", UserName = "testUserName" });
            _mockRepository.Setup(repo => repo.GetCommentById(commentId))
                           .ReturnsAsync(mockComment);
            _mockRepository.Setup(repo => repo.DeleteComment(It.IsAny<Comment>(), It.IsAny<ClaimsPrincipal>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await controller.Delete(commentId, postId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }



    }
}

