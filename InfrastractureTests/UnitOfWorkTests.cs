using Domain.Abstractions;
using Infrastracture.Context;
using Infrastracture.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InfrastractureTests
{
    public class UnitOfWorkTests
    {
        private readonly Mock<ApplicationContext> _contextMock;
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _contextMock = new Mock<ApplicationContext>(options);

            _unitOfWork = new Infrastracture.UnitOfWork.UnitOfWork(_contextMock.Object);
        }

        [Fact]
        public void UserRepository_ShouldReturn_UserRepositoryInstance()
        {
            var repo1 = _unitOfWork.UserRepository;
            var repo2 = _unitOfWork.UserRepository;

            Assert.NotNull(repo1);
            Assert.IsType<UserRepository>(repo1);
            Assert.Same(repo1, repo2); 
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldCall_ContextSaveChangesAsync()
        {
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            await _unitOfWork.SaveChangesAsync();

            _contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }
    }
}
