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
    public class SearchControllerTests
    {
        private SearchController _searchController;
        private Mock<IBlogRepository> _mockRepository;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<ILogger<SearchController>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IBlogRepository>();
            _mockUserManager = MockHelpers.MockUserManager<IdentityUser>();
            _mockLogger = new Mock<ILogger<SearchController>>();
            _searchController = new SearchController(_mockUserManager.Object, _mockRepository.Object, _mockLogger.Object);
        }


        [TestMethod]
        public async Task SearchPosts_ValidQuery_ReturnsOkResult()
        {
            // Arrange
            var searchQuery = "yourQuery";
            var expectedPosts = new List<Post>(); // Replace with your expected data
            _mockRepository.Setup(repo => repo.SearchPostByTagOrUsername(searchQuery))
                           .ReturnsAsync(expectedPosts);

            // Act
            var result = await _searchController.SearchPosts(searchQuery);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var actualPosts = okResult.Value as List<Post>; 
            Assert.IsNotNull(actualPosts);
        }

        [TestMethod]
        public async Task SearchPosts_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var searchQuery = "yourQuery";
            _mockRepository.Setup(repo => repo.SearchPostByTagOrUsername(searchQuery))
                           .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            Func<Task> act = async () => await _searchController.SearchPosts(searchQuery);

            // Assert
            await Assert.ThrowsExceptionAsync<Exception>(act);
        }

        [TestMethod]
        public async Task SearchSuggestions_ValidQuery_ReturnsOkResult()
        {
            // Arrange
            var searchQuery = "yourQuery";
            var expectedSuggestions = new List<String>(); 
            _mockRepository.Setup(repo => repo.SearchSuggestions(searchQuery))
                           .ReturnsAsync(expectedSuggestions);

            // Act
            var result = await _searchController.SearchSuggestions(searchQuery);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var actualSuggestions = okResult.Value as List<String>;
            Assert.IsNotNull(actualSuggestions);
        }

        [TestMethod]
        public async Task SearchSuggestions_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var searchQuery = "yourQuery";
            _mockRepository.Setup(repo => repo.SearchSuggestions(searchQuery))
                           .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            Func<Task> act = async () => await _searchController.SearchSuggestions(searchQuery);

            // Assert
            await Assert.ThrowsExceptionAsync<Exception>(act);
        }

    }

    }