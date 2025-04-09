using System.Security.Claims;
using LU2.Controllers;
using LU2.Models;
using LU2.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LU2Tests;

public class Object2DTests
{
        private Mock<IEnvironment2DRepository> _environment2DRepositoryMock;
        private Mock<IObject2DRepository> _object2DRepositoryMock;
        private Environment2DController _controller;
        private string _userId;
        private Guid _environmentId;
        private Guid _objectId;

        [SetUp]
        public void Setup()
        {
            _environment2DRepositoryMock = new Mock<IEnvironment2DRepository>();
            _object2DRepositoryMock = new Mock<IObject2DRepository>();
            _controller = new Environment2DController(_environment2DRepositoryMock.Object, _object2DRepositoryMock.Object);
            _userId = Guid.NewGuid().ToString();
            _environmentId = Guid.NewGuid();
            _objectId = Guid.NewGuid();

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }
        
        [Test]
        public async Task Get_ReturnsObjects_WhenUserOwnsEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = _environmentId.ToString(), UserId = _userId } };
            var objects = new List<Object2D> { new Object2D { Id = Guid.NewGuid().ToString(), EnvironmentId = _environmentId.ToString() } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            _object2DRepositoryMock.Setup(repo => repo.GetByEnvironmentIdAsync(_environmentId)).ReturnsAsync(objects);
            
            var result = await _controller.GetObjects(_environmentId);
            
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EquivalentTo(objects));
        }

        [Test]
        public async Task Create_CreatesObject_WhenUserOwnsEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = _environmentId.ToString(), UserId = _userId } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            var object2D = new Object2D { PrefabId = 1, PositionX = 1, PositionY = 1, ScaleX = 1, ScaleY = 1, RotationZ = 0, SortingLayer = 0 };
            _object2DRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Object2D>())).Returns(Task.CompletedTask);

            var result = await _controller.PostObject(_environmentId, object2D);
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtActionResult, Is.Not.Null);
            Assert.That(createdAtActionResult.Value, Is.TypeOf<Object2D>());
            Assert.That((createdAtActionResult.Value as Object2D).EnvironmentId, Is.EqualTo(_environmentId.ToString()));
            _object2DRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Object2D>()), Times.Once);
        }

        [Test]
        public async Task Create_ReturnsUnauthorized_WhenUserDoesNotOwnEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = Guid.NewGuid().ToString(), UserId = Guid.NewGuid().ToString() } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            var object2D = new Object2D();

            var result = await _controller.PostObject(_environmentId, object2D);
            Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Delete_DeletesObject_WhenUserOwnsEnvironmentAndObjectExists()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = _environmentId.ToString(), UserId = _userId } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            var object2D = new Object2D { Id = _objectId.ToString(), EnvironmentId = _environmentId.ToString() };
            _object2DRepositoryMock.Setup(repo => repo.GetByIdAsync(_objectId)).ReturnsAsync(object2D);
            _object2DRepositoryMock.Setup(repo => repo.DeleteAsync(_objectId)).Returns(Task.CompletedTask);
            
            var result = await _controller.DeleteObject(_environmentId, _objectId);
            Assert.That(result, Is.TypeOf<NoContentResult>());
            _object2DRepositoryMock.Verify(repo => repo.DeleteAsync(_objectId), Times.Once);
        }

        [Test]
        public async Task Delete_ReturnsUnauthorized_WhenUserDoesNotOwnEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = Guid.NewGuid().ToString(), UserId = Guid.NewGuid().ToString() } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);

            var result = await _controller.DeleteObject(_environmentId, _objectId);
            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Delete_ReturnsNotFound_WhenObjectDoesNotExistOrBelongsToDifferentEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = _environmentId.ToString(), UserId = _userId } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            _object2DRepositoryMock.Setup(repo => repo.GetByIdAsync(_objectId)).ReturnsAsync((Object2D)null);

            var result = await _controller.DeleteObject(_environmentId, _objectId);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
            
            _object2DRepositoryMock.Setup(repo => repo.GetByIdAsync(_objectId)).ReturnsAsync(new Object2D{Id = _objectId.ToString(), EnvironmentId = Guid.NewGuid().ToString()});
            
            var result2 = await _controller.DeleteObject(_environmentId, _objectId);
            Assert.That(result2, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteMultiple_DeletesObjects_WhenUserOwnsEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = _environmentId.ToString(), UserId = _userId } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            _object2DRepositoryMock.Setup(repo => repo.DeleteByEnvironmentIdAsync(_environmentId)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteObjects(_environmentId);
            Assert.That(result, Is.TypeOf<NoContentResult>());
            _object2DRepositoryMock.Verify(repo => repo.DeleteByEnvironmentIdAsync(_environmentId), Times.Once);
        }

        [Test]
        public async Task DeleteMultiple_ReturnsUnauthorized_WhenUserDoesNotOwnEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = Guid.NewGuid().ToString(), UserId = Guid.NewGuid().ToString() } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);

            var result = await _controller.DeleteObjects(_environmentId);
            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }
}