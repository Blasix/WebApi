using System.Security.Claims;
using LU2.Controllers;
using LU2.Models;
using LU2.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LU2Tests;

public class Environment2DTests
{
        private Mock<IEnvironment2DRepository> _environment2DRepositoryMock;
        private Mock<IObject2DRepository> _object2DRepositoryMock;
        private Environment2DController _controller;
        private string _userId;
        private Guid _environmentId;

        [SetUp]
        public void Setup()
        {
            _environment2DRepositoryMock = new Mock<IEnvironment2DRepository>();
            _object2DRepositoryMock = new Mock<IObject2DRepository>();
            _controller = new Environment2DController(_environment2DRepositoryMock.Object, _object2DRepositoryMock.Object);
            _userId = Guid.NewGuid().ToString();
            _environmentId = Guid.NewGuid();

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Test]
        public async Task Get_ReturnsUserEnvironments()
        {
            var environments = new List<Environment2D>
            {
                new() { Id = Guid.NewGuid().ToString(), Name = "Env1", UserId = _userId },
                new() { Id = Guid.NewGuid().ToString(), Name = "Env2", UserId = _userId }
            };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);

       
            var result = await _controller.Get();
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var returnedEnvironments = okResult.Value as IEnumerable<Environment2D>;
            Assert.That(returnedEnvironments, Is.EquivalentTo(environments));
        }

        [Test]
        public async Task Create_CreatesEnvironment_WhenValid()
        {
            var environment = new Environment2D { Name = "NewEnv", MaxHeight = 10, MaxLength = 20 };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(new List<Environment2D>());
            _environment2DRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Environment2D>())).Returns(Task.CompletedTask);
            
            var result = await _controller.Post(environment);
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtActionResult, Is.Not.Null);
            Assert.That(createdAtActionResult.Value, Is.TypeOf<Environment2D>());
            Assert.That((createdAtActionResult.Value as Environment2D).UserId, Is.EqualTo(_userId));
            _environment2DRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Environment2D>()), Times.Once);
        }

        [Test]
        public async Task Create_ReturnsBadRequest_WhenUserHasMaxEnvironments()
        {
            var environment = new Environment2D { Name = "NewEnv", MaxHeight = 10, MaxLength = 20 };
            var existingEnvironments = Enumerable.Range(0, 5).Select(i => new Environment2D()).ToList();
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(existingEnvironments);
            
            var result = await _controller.Post(environment);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }
        
        [Test]
        public async Task Create_ReturnsBadRequest_WhenNameIsInvalid()
        {
            var environment1 = new Environment2D { Name = "a", MaxHeight = 10, MaxLength = 20 };
            var result1 = await _controller.Post(environment1);
            Assert.That(result1.Result, Is.TypeOf<BadRequestObjectResult>());
            
            var environment2 = new Environment2D { Name = "abcdefghijklmnopqrstuvwxyz", MaxHeight = 10, MaxLength = 20 };
            var result2 = await _controller.Post(environment2);
            Assert.That(result2.Result, Is.TypeOf<BadRequestObjectResult>());
        }
        
        [Test]
        public async Task Create_ReturnsBadRequest_WhenNameIsDuplicated()
        {
            var environment = new Environment2D { Name = "NewEnv", MaxHeight = 10, MaxLength = 20 };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(new List<Environment2D> { environment });
            var result = await _controller.Post(environment);
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Delete_DeletesEnvironment_WhenUserOwnsEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = _environmentId.ToString(), UserId = _userId } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            _environment2DRepositoryMock.Setup(repo => repo.DeleteAsync(_environmentId)).Returns(Task.CompletedTask);
            
            var result = await _controller.Delete(_environmentId);
            Assert.That(result, Is.TypeOf<NoContentResult>());
            _environment2DRepositoryMock.Verify(repo => repo.DeleteAsync(_environmentId), Times.Once);
        }

        [Test]
        public async Task Delete_ReturnsUnauthorized_WhenUserDoesNotOwnEnvironment()
        {
            var environments = new List<Environment2D> { new Environment2D { Id = Guid.NewGuid().ToString(), UserId = Guid.NewGuid().ToString() } };
            _environment2DRepositoryMock.Setup(repo => repo.GetByUserIdAsync(_userId)).ReturnsAsync(environments);
            
            var result = await _controller.Delete(_environmentId);
            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }
}