using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SharedModels.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAPI.Controllers;
using WebAPI.Models.Repositories;

namespace YourProject.Tests
{
    [TestClass]
    public class SearchControllerTests
    {
        private SearchController _searchController;
        private Mock<IBlogRepository> _repositoryMock;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<ILogger<SearchController>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IBlogRepository>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            _loggerMock = new Mock<ILogger<SearchController>>();

            _searchController = new SearchController(_userManagerMock.Object, _repositoryMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task SearchPosts_ReturnsOkResult()
        {
            // Arrange
            var searchQuery = "testQuery";
            var expectedPosts = new List<Post>(); // Replace YourPostModel with the actual type returned by your repository method.
            _repositoryMock.Setup(repo => repo.SearchPostByTagOrUsername(searchQuery)).ReturnsAsync(expectedPosts);

            // Act
            var result = await _searchController.SearchPosts(searchQuery) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expectedPosts, result.Value);
        }

        [TestMethod]
        public async Task SearchPosts_ReturnsInternalServerErrorOnException()
        {
            // Arrange
            var searchQuery = "testQuery";
            _repositoryMock.Setup(repo => repo.SearchPostByTagOrUsername(searchQuery)).Throws(new Exception("Test exception"));

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _searchController.SearchPosts(searchQuery);
            });
        }
        [TestMethod]
        public async Task SearchSuggestions_ReturnsOkResult()
        {
            // Arrange
            var searchQuery = "testQuery";
            var expectedSuggestions = new List<String>(); // Replace YourSuggestionModel with the actual type returned by your repository method.
            _repositoryMock.Setup(repo => repo.SearchSuggestions(searchQuery)).ReturnsAsync(expectedSuggestions);

            // Act
            var result = await _searchController.SearchSuggestions(searchQuery) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expectedSuggestions, result.Value);
        }

        [TestMethod]
        public async Task SearchSuggestions_ReturnsInternalServerErrorOnException()
        {
            // Arrange
            var searchQuery = "testQuery";
            _repositoryMock.Setup(repo => repo.SearchSuggestions(searchQuery)).Throws(new Exception("Test exception"));

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _searchController.SearchSuggestions(searchQuery);
            });
        }

    }
}
