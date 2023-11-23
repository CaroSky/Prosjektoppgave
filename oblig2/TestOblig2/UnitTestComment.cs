using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Protocol.Core.Types;
using oblig2.Controllers;
using oblig2.Models.Entities;
using oblig2.Models.Repositories;
using oblig2.Models.ViewModels;
using System.Security.Claims;
using TestProject1;

namespace TestOblig2
{
    [TestClass]
    public class UnitTestComment
    {
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private CommentController _controller;
        private IdentityUser _user;


        [TestMethod]
        public void Index_ValidId_ReturnsViewResultWithModel()
        {
            // Arrange
            var commentId = 1;
            var postId = 1;
            var post = new Post
            {
                PostId = postId,
                Title = "Sample post Title",
                IsCommentAllowed = true,
                Blog = new Blog{BlogId = 1}
            };

            var comments = new List<Comment>
        {
            new Comment { CommentId = 1, Content = "post1", Post = post},
            new Comment { CommentId = 2, Content = "post2" , Post = post},
        };

           
            var commentIndexViewModel = new CommentIndexViewModel
            {
                Comments = comments,
                PostId = postId,
                PostTitle = post.Title,
                IsCommentAllowed = post.IsCommentAllowed
            };

            _mockRepository = new Mock<IBlogRepository>();
            _mockRepository.Setup(repo => repo.GetPostById(postId)).Returns(post);
            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId)).Returns(comments);
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);

            // Act
            var result = _controller.Index(postId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as CommentIndexViewModel;
            Assert.IsNotNull(model);
            // You can add more specific assertions about the model's properties here
            Assert.AreEqual(commentIndexViewModel.Comments, model.Comments);
            Assert.AreEqual(commentIndexViewModel.IsCommentAllowed, model.IsCommentAllowed);
        }


        [TestMethod]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var commentId = 1;
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);

            _mockRepository.Setup(repo => repo.GetPostCreateViewModel(commentId)).Returns(new PostCreateViewModel());

            // Act
            var result = _controller.Create(commentId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            // Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));


            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            var commentCreateViewModel = new CommentCreateViewModel
            {
                Content = "Test Content",
            };


            // Act
            var result = await _controller.Create(commentCreateViewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestMethod]
        public async Task Create_Post_WithInvalidModel_ReturnsViewResult()
        {
            // Arrange
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);

            var commentCreateViewModel = new CommentCreateViewModel();
            _controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = await _controller.Create(commentCreateViewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Create_ValidModel_ReturnsRedirectToAction()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            // Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));


            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            var commentCreateViewModel = new CommentCreateViewModel
            {
                Content = "This is a test blog",
            };


            // Act
            var result = await _controller.Create(commentCreateViewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestMethod]
        public void Create_InvalidModel_ReturnsView()
        {
            // Arrange
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockRepository = new Mock<IBlogRepository>();
            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);

            _controller.ModelState.AddModelError("Title", "Title is required");

            var commentCreateViewModel = new CommentCreateViewModel();

            // Act
            var result = _controller.Create(commentCreateViewModel);

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task Edit_Get_ReturnsViewResult()
        {
            // Arrange
            int commentId = 1;
            int postId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var comment = new Comment
            {
                CommentId = commentId,
                Author = new IdentityUser { UserName = "cla040@uit.no" },
                Post = new Post { PostId = 1 }

            };

            var commentEditViewModel = new CommentEditViewModel { CommentId = commentId };
            //Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "cla040@uit.no"),
                 new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };
            //_mockRepository.Setup(repo => repo.GetPostById(id)).Returns(new PostEditViewModel());
            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId)).Returns(new List<Comment> { comment });
            _mockRepository.Setup(repo => repo.GetCommentById(commentId)).Returns(comment);
            _mockRepository.Setup(repo => repo.GetCommentEditViewModelById(commentId)).Returns(commentEditViewModel);

            _mockUserManager
                .Setup(manager => manager.FindByNameAsync("cla040@uit.no"))
                .ReturnsAsync(new IdentityUser { UserName = "cla040@uit.no" });


            // Act
            var result = await _controller.Edit(commentId, postId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task Edit_ValidId_ReturnsView()
        {
            // Arrange
            int commentId = 1;
            int postId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            // Mock data for repository
            var comment = new Comment
            {
                CommentId = commentId,
                Author = new IdentityUser { UserName = "cla040@uit.no" },
                Post = new Post { PostId = 1 }

            };
            var commentEdit = new CommentEditViewModel();

            //Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };


            //var currentUser = new ApplicationUser(); // Replace with your user model
            _mockRepository.Setup(r => r.GetAllCommentsByPostId(postId)).Returns(new List<Comment> { comment });
            _mockRepository.Setup(r => r.GetCommentById(commentId)).Returns(comment);
            _mockRepository.Setup(r => r.GetCommentEditViewModelById(commentId)).Returns(commentEdit);
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(this._user);

            _mockUserManager
                .Setup(manager => manager.FindByNameAsync("cla040@uit.no"))
                .ReturnsAsync(new IdentityUser { UserName = "cla040@uit.no" });
            // Act
            var result = await _controller.Edit(commentId, postId);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Edit_UserNotOwner_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            int commentId = 1;
            int postId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var comment = new Comment
            {
                CommentId = commentId,
                Author = new IdentityUser { UserName = "owner" },  // Set a different owner username
                Post = new Post { PostId = 1 }
            };

            var commentEditViewModel = new CommentEditViewModel { PostId = commentId };
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId)).Returns(new List<Comment> { comment });
            _mockRepository.Setup(repo => repo.GetCommentById(commentId)).Returns(comment);
            _mockRepository.Setup(repo => repo.GetCommentEditViewModelById(commentId)).Returns(commentEditViewModel);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await _controller.Edit(commentId, postId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("You cannot edit this item", tempData["message"]);

        }

        [TestMethod]
        public async Task Edit_ValidModel_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var commentEditViewModel = new CommentEditViewModel
            {
                CommentId = 1,
                Content = "Updated Content",
                Created = DateTime.UtcNow,
            };

            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "cla040@uit.no"),
            new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await _controller.Edit(commentEditViewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("The comment has been updated", tempData["message"]);
        }

        [TestMethod]
        public async Task Edit_InvalidModel_ReturnsView()
        {
            // Arrange
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Edit(new CommentEditViewModel()) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task Edit_NonExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var commentId = 99;
            int postId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            Comment? comment = null;
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId)).Returns(new List<Comment>());
            _mockRepository.Setup(repo => repo.GetCommentById(commentId)).Returns(comment);
            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await _controller.Edit(commentId, postId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Item not found", tempData["message"]);
        }

        [TestMethod]
        public async Task Delete_ExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var commentId = 1;
            int postId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var comment = new Comment
            {
                CommentId = commentId,
                Content = "content",
                Post = new Post { PostId = 1 },
                Author = new IdentityUser { UserName = "cla040@uit.no" }
            };

            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId)).Returns(new List<Comment> { comment });
            _mockRepository.Setup(repo => repo.GetCommentById(commentId)).Returns(comment);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "cla040@uit.no"),
            new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await _controller.Delete(commentId, postId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("The comment has been deleted", tempData["message"]);
        }

        [TestMethod]
        public async Task Delete_NonExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var commentId = 99;
            int postId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            Comment? comment = null;
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId)).Returns(new List<Comment>());
            _mockRepository.Setup(repo => repo.GetCommentById(commentId)).Returns(comment);
            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await _controller.Delete(commentId, postId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Item not found", tempData["message"]);
        }

        [TestMethod]
        public async Task Delete_UserNotOwner_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            int commentId = 1;
            int postId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var comment = new Comment
            {
                CommentId = commentId,
                Author = new IdentityUser { UserName = "owner" },  // Set a different owner username
                Post = new Post { PostId = 1 }
            };

            var commentEditViewModel = new CommentEditViewModel { CommentId = commentId };
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllCommentsByPostId(postId)).Returns(new List<Comment> { comment });
            _mockRepository.Setup(repo => repo.GetCommentById(commentId)).Returns(comment);
            _mockRepository.Setup(repo => repo.GetCommentEditViewModelById(commentId)).Returns(commentEditViewModel);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new CommentController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await _controller.Delete(commentId, postId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("You cannot delete this item", tempData["message"]);

        }
    }

}