using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using oblig2.Controllers;
using oblig2.Models.Entities;
using oblig2.Models.Repositories;
using oblig2.Models.ViewModels;
using System.Security.Claims;
using TestProject1;

namespace TestOblig2
{
    [TestClass]
    public class UnitTestBlog
    {
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private BlogController _controller;
        private IdentityUser _user;


        [TestMethod]
        public void Index_ReturnsViewResultWithViewModel()
        {
            // Arrange
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);

            // Mock the repository to return some sample data
            var blogs = new List<Blog>();
            _mockRepository.Setup(repo => repo.GetAllBlogs()).Returns(blogs);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(List<Blog>));

            var model = result.Model as List<Blog>;
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);

            _mockRepository.Setup(repo => repo.GetBlogCreateViewModel()).Returns(new BlogCreateViewModel());

            // Act
            var result = _controller.Create() as ViewResult;

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

            
            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            var blogCreateViewModel = new BlogCreateViewModel
            {
                Title = "Test Blog",
                Content = "Test Content",
            };
            //var owner = _mockUserManager.Object.FindByNameAsync(_user.UserName);
            //_controller.ModelState.Clear(); // Ensure ModelState is valid

            // Act
            var result = await _controller.Create(blogCreateViewModel) as RedirectToActionResult;

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
            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);

            var blogCreateViewModel = new BlogCreateViewModel();
            _controller.ModelState.AddModelError("Title", "Title is required.");

            // Act
            var result = await _controller.Create(blogCreateViewModel) as ViewResult;

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


            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            var blogCreateViewModel = new BlogCreateViewModel
            {
                Title = "Test Blog",
                Content = "This is a test blog",
            };

            //_mockUserManager.Setup(manager => manager.FindByNameAsync(It.IsAny<string>()))
             //   .ReturnsAsync(new IdentityUser());

            // Act
            var result = await _controller.Create(blogCreateViewModel) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            //Assert.AreEqual("Test Blog has been created", result.RouteValues["message"]);
        }

        [TestMethod]
        public void Create_InvalidModel_ReturnsView()
        {
            // Arrange
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockRepository = new Mock<IBlogRepository>();
            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);

            _controller.ModelState.AddModelError("Title", "Title is required");

            var blogCreateViewModel = new BlogCreateViewModel();

            // Act
            var result = _controller.Create(blogCreateViewModel);

            // Assert
            Assert.IsNotNull(result);
        }

        // Helper method to create a mock UserManager
        //private Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        //{
        //    var store = new Mock<IUserStore<TUser>>();
        //    return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        //}

        [TestMethod]
        public async Task Edit_Get_ReturnsViewResult()
        {
            // Arrange
            int id = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            _mockRepository.Setup(repo => repo.GetBlogEditViewModel()).Returns(new BlogEditViewModel());

            var blog = new Blog
            {
                BlogId = id,
                Owner = new IdentityUser { UserName = "cla040@uit.no" }
            };

            var blogEditViewModel = new BlogEditViewModel { BlogId = id };
            //Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "cla040@uit.no"),
                 new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };

            _mockRepository.Setup(repo => repo.GetAllBlogs()).Returns(new List<Blog> { blog });
            _mockRepository.Setup(repo => repo.GetBlogById(id)).Returns(blog);
            _mockRepository.Setup(repo => repo.GetBlogEditViewModelById(id)).Returns(blogEditViewModel);

            _mockUserManager
                .Setup(manager => manager.FindByNameAsync("cla040@uit.no"))
                .ReturnsAsync(new IdentityUser { UserName = "cla040@uit.no" });


            // Act
            var result = await _controller.Edit(id) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task Edit_ValidId_ReturnsView()
        {
            // Arrange
            int id = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            // Mock data for repository
            var blog = new Blog(); 
            var blogEdit = new BlogEditViewModel();

            //Create a fake ClaimsPrincipal to simulate a logged-in user
            var _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "cla040@uit.no"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
            _controller.TempData = tempData;

            // Set the user as the User of the ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };


            //var currentUser = new ApplicationUser(); // Replace with your user model
            _mockRepository.Setup(r => r.GetAllBlogs()).Returns(new List<Blog> { blog });
            _mockRepository.Setup(r => r.GetBlogById(id)).Returns(blog);
            _mockRepository.Setup(r => r.GetBlogEditViewModelById(id)).Returns(blogEdit);
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(this._user);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Edit_UserNotOwner_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            int id = 1;
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var blog = new Blog
            {
                BlogId = id,
                Owner = new IdentityUser { UserName = "owner" } // Set a different owner username
            };

            var blogEditViewModel = new BlogEditViewModel { BlogId = id };
            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllBlogs()).Returns(new List<Blog> { blog });
            _mockRepository.Setup(repo => repo.GetBlogById(id)).Returns(blog);
            _mockRepository.Setup(repo => repo.GetBlogEditViewModelById(id)).Returns(blogEditViewModel);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Edit(id) as RedirectToActionResult;

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

            var blogEditViewModel = new BlogEditViewModel
            {
                BlogId = 1,
                Title = "Updated Title",
                Content = "Updated Content",
                Created = DateTime.UtcNow,
                IsPostAllowed = true
            };

            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Edit(blogEditViewModel) as RedirectToActionResult;

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

            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Edit(new BlogEditViewModel()) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Delete_ExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var id = 1;
            var blog = new Blog
            {
                BlogId = id,
                Title = "Deleted Blog Title"
            };

            var user = new IdentityUser { UserName = "cla040@uit.no" };

            _mockRepository.Setup(repo => repo.GetAllBlogs()).Returns(new List<Blog> { blog });
            _mockRepository.Setup(repo => repo.GetBlogById(id)).Returns(blog);

            _mockUserManager.Setup(manager => manager.FindByNameAsync("cla040@uit.no")).ReturnsAsync(user);

            _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
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
            var result = await _controller.Delete(id) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Deleted Blog Title has been deleted", tempData["message"]);
        }

        [TestMethod]
        public async Task Delete_NonExistingId_ReturnsRedirectToActionWithMessage()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();

            var id = 1; // Non-existing ID

            _mockRepository.Setup(repo => repo.GetAllBlogs()).Returns(new List<Blog>());

           _controller = new BlogController(_mockUserManager.Object, _mockRepository.Object);
           _controller.TempData = tempData;

            // Act
            var result = await _controller.Delete(id) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Item not found", tempData["message"]);
        }
    }

}