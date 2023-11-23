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
    public class UnitTestPost
    {
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private PostController _controller;
        private IdentityUser _user;


        [TestMethod]
        public void Index_ValidId_ReturnsViewResultWithModel()
        {
            // Arrange
            var blogId = 1;
            var blog = new Blog
            {
                BlogId = blogId,
                Title = "Sample Blog Title",
                IsPostAllowed = true
            };

            var posts = new List<Post>
        {
            new Post { PostId = 1, Title = "post1"},
            new Post { PostId = 2, Title = "post2" },
        };

            var postIndexViewModel = new PostIndexViewModel
            {
                Posts = posts,
                BlogId = blogId,
                BlogTitle = blog.Title,
                IsPostAllowed = blog.IsPostAllowed
            };

            _mockRepository = new Mock<IBlogRepository>();
            _mockRepository.Setup(repo => repo.GetBlogById(blogId)).Returns(blog);
            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(blogId)).Returns(posts);
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);

            // Act
            var result = _controller.Index(blogId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as PostIndexViewModel;
            Assert.IsNotNull(model);
            // You can add more specific assertions about the model's properties here
            Assert.AreEqual(postIndexViewModel.BlogTitle, model.BlogTitle);
            Assert.AreEqual(postIndexViewModel.IsPostAllowed, model.IsPostAllowed);
        }


        [TestMethod]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var postId = 1;
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);

            _mockRepository.Setup(repo => repo.GetPostCreateViewModel(postId)).Returns(new PostCreateViewModel());

            // Act
            var result = _controller.Create(postId) as ViewResult;

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


            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            var postCreateViewModel = new PostCreateViewModel
            {
                Title = "Test Blog",
                Content = "Test Content",
            };
            

            // Act
            var result = await _controller.Create(postCreateViewModel) as RedirectToActionResult;

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
            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);

            var postCreateViewModel = new PostCreateViewModel();
            _controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = await _controller.Create(postCreateViewModel) as ViewResult;

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


            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            var postCreateViewModel = new PostCreateViewModel
            {
                Title = "Test Blog",
                Content = "This is a test blog",
            };


            // Act
            var result = await _controller.Create(postCreateViewModel) as RedirectToActionResult;

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
            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);

            _controller.ModelState.AddModelError("Title", "Title is required");

            var postCreateViewModel = new PostCreateViewModel();

            // Act
            var result = _controller.Create(postCreateViewModel);

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task Edit_Get_ReturnsViewResult()
        {
            // Arrange
            int postId = 1;
            int blogId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var post = new Post
            {
                PostId = postId,
                Author = new IdentityUser { UserName = "cla040@uit.no" },
                Blog = new Blog { BlogId = 1}
                
            };

            var postEditViewModel = new PostEditViewModel { BlogId = postId };
            //Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "cla040@uit.no"),
                 new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };
            //_mockRepository.Setup(repo => repo.GetPostById(id)).Returns(new PostEditViewModel());
            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(blogId)).Returns(new List<Post> { post });
            _mockRepository.Setup(repo => repo.GetPostById(postId)).Returns(post);
            _mockRepository.Setup(repo => repo.GetPostEditViewModelById(postId)).Returns(postEditViewModel);

            _mockUserManager
                .Setup(manager => manager.FindByNameAsync("cla040@uit.no"))
                .ReturnsAsync(new IdentityUser { UserName = "cla040@uit.no" });


            // Act
            var result = await _controller.Edit(postId, blogId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task Edit_ValidId_ReturnsView()
        {
            // Arrange
            int postId = 1;
            int blogId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            // Mock data for repository
            var post = new Post
            {
                PostId = postId,
                Author = new IdentityUser { UserName = "cla040@uit.no" },
                Blog = new Blog { BlogId = 1 }

            };
            var postEdit = new PostEditViewModel();

            //Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };


            //var currentUser = new ApplicationUser(); // Replace with your user model
            _mockRepository.Setup(r => r.GetAllPostByBlogId(blogId)).Returns(new List<Post> { post });
            _mockRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockRepository.Setup(r => r.GetPostEditViewModelById(postId)).Returns(postEdit);
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(this._user);

            _mockUserManager
                .Setup(manager => manager.FindByNameAsync("cla040@uit.no"))
                .ReturnsAsync(new IdentityUser { UserName = "cla040@uit.no" });
            // Act
            var result = await _controller.Edit(postId, blogId);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Edit_UserNotOwner_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            int postId = 1;
            int blogId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var post = new Post
            {
                PostId = postId,
                Author = new IdentityUser { UserName = "owner"},  // Set a different owner username
                Blog = new Blog { BlogId = 1 }
            };

            var postEditViewModel = new PostEditViewModel { PostId = postId };
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(blogId)).Returns(new List<Post> { post });
            _mockRepository.Setup(repo => repo.GetPostById(postId)).Returns(post);
            _mockRepository.Setup(repo => repo.GetPostEditViewModelById(postId)).Returns(postEditViewModel);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Edit(postId, blogId) as RedirectToActionResult;

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

            var postEditViewModel = new PostEditViewModel
            {
                PostId = 1,
                Title = "Updated Title",
                Content = "Updated Content",
                Created = DateTime.UtcNow,
                IsCommentAllowed = true
            };

            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Edit(postEditViewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Updated Title has been updated", tempData["message"]);
        }

        [TestMethod]
        public async Task Edit_InvalidModel_ReturnsView()
        {
            // Arrange
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Edit(new PostEditViewModel()) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public async Task Edit_NonExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var postId = 99;
            int blogId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            Post? post = null;
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(blogId)).Returns(new List<Post>());
            _mockRepository.Setup(repo => repo.GetPostById(postId)).Returns(post);
            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Edit(postId, blogId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Item not found", tempData["message"]);
        }

        [TestMethod]
        public async Task Delete_ExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var postId = 1;
            int blogId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var post = new Post
            {
                PostId = postId,
                Title = "Post",
                Blog = new Blog {BlogId = 1},
                Author = new IdentityUser { UserName = "cla040@uit.no" }
            };

            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(blogId)).Returns(new List<Post> { post });
            _mockRepository.Setup(repo => repo.GetPostById(postId)).Returns(post);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Delete(postId,blogId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("The post has been deleted", tempData["message"]);
        }

        [TestMethod]
        public async Task Delete_NonExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var postId = 99;
            int blogId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            Post? post = null;
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(blogId)).Returns(new List<Post>());
            _mockRepository.Setup(repo => repo.GetPostById(postId)).Returns(post);
            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Delete(postId,blogId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Item not found", tempData["message"]);
        }

        [TestMethod]
        public async Task Delete_UserNotOwner_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            int postId = 1;
            int blogId = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var post = new Post
            {
                PostId = postId,
                Author = new IdentityUser { UserName = "owner" },  // Set a different owner username
                Blog = new Blog { BlogId = 1 }
            };

            var postEditViewModel = new PostEditViewModel { PostId = postId };
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllPostByBlogId(blogId)).Returns(new List<Post> { post });
            _mockRepository.Setup(repo => repo.GetPostById(postId)).Returns(post);
            _mockRepository.Setup(repo => repo.GetPostEditViewModelById(postId)).Returns(postEditViewModel);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new PostController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Delete(postId, blogId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("You cannot delete this item", tempData["message"]);

        }
    }

}