using Domain.Entities.DbEntities;
using Infrastracture.Context;
using Infrastracture.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InfrastractureTests
{
    public class RepositoryTests
    {
        private ApplicationContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationContext(options);
        }
        #region GenericRepo
        [Fact]
        public async Task AddAsync_AddsUserToDatabase()
        {
            var context = GetInMemoryContext();
            var repo = new UserRepository(context);
            var user = new User { Id = Guid.NewGuid(),
                Email = "test@test.com", UserName = "testuser" };

            await repo.AddAsync(user);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(user.Id);
            Assert.NotNull(result);
            Assert.Equal("testuser", result.UserName);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            var context = GetInMemoryContext();
            var repo = new UserRepository(context);
            var user1 = new User { Id = Guid.NewGuid(), 
                Email = "a@a.com", UserName = "user1" };
            var user2 = new User { Id = Guid.NewGuid(),
                Email = "b@b.com", UserName = "user2" };

            await repo.AddAsync(user1);
            await repo.AddAsync(user2);
            await context.SaveChangesAsync();

            var users = await repo.GetAllAsync();
            Assert.Equal(2, System.Linq.Enumerable.Count(users));
        }

        [Fact]
        public async Task Delete_RemovesUserFromDatabase()
        {
            var context = GetInMemoryContext();
            var repo = new UserRepository(context);
            var user = new User { Id = Guid.NewGuid(), 
                Email = "delete@me.com", UserName = "deleteuser" };

            await repo.AddAsync(user);
            await context.SaveChangesAsync();

            repo.Delete(user);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(user.Id);
            Assert.Null(result);
        }
        #endregion

        #region UserRepository
        [Fact]
        public async Task GetByEmailAsync_ReturnsCorrectUser()
        {
            var context = GetInMemoryContext();
            var repo = new UserRepository(context);
            var user = new User { Id = Guid.NewGuid(), 
                Email = "unique@email.com", UserName = "emailuser" };

            await repo.AddAsync(user);
            await context.SaveChangesAsync();

            var result = await repo.GetByEmailAsync("unique@email.com");
            Assert.NotNull(result);
            Assert.Equal("emailuser", result.UserName);
        }

        [Fact]
        public async Task GetByUserNameAsync_ReturnsCorrectUser()
        {
            var context = GetInMemoryContext();
            var repo = new UserRepository(context);
            var user = new User { Id = Guid.NewGuid(), 
                Email = "user@name.com", UserName = "specialuser" };

            await repo.AddAsync(user);
            await context.SaveChangesAsync();

            var result = await repo.GetByUserNameAsync("specialuser");
            Assert.NotNull(result);
            Assert.Equal("user@name.com", result.Email);
        }
        #endregion
        #region ProjectRepository
        [Fact]
        public async Task AddAsync_AddsProjectToDatabase()
        {
            var context = GetInMemoryContext();
            var repo = new ProjectRepository(context);
            var project = new Project { Id = Guid.NewGuid(), 
                Title = "Test Project", 
                StartDate = DateOnly.FromDateTime(DateTime.Today) };

            await repo.AddAsync(project);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(project.Id);
            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Title);
        }

        [Fact]
        public async Task GetByEndDateAsync_ReturnsCorrectProject()
        {
            var context = GetInMemoryContext();
            var repo = new ProjectRepository(context);
            var project = new Project { Id = Guid.NewGuid(), 
                Title = "EndDate Project", 
                EndDate = DateOnly.FromDateTime(DateTime.Today) };

            await repo.AddAsync(project);
            await context.SaveChangesAsync();

            var result = await repo.GetByEndDateAsync(DateOnly.FromDateTime(DateTime.Today));
            Assert.NotNull(result);
            Assert.Equal("EndDate Project", result.Title);
        }

        [Fact]
        public async Task GetByStartDateAsync_ReturnsCorrectProject()
        {
            var context = GetInMemoryContext();
            var repo = new ProjectRepository(context);
            var project = new Project { Id = Guid.NewGuid(), 
                Title = "StartDate Project", 
                StartDate = DateOnly.FromDateTime(DateTime.Today) };

            await repo.AddAsync(project);
            await context.SaveChangesAsync();

            var result = await repo.GetByStartDateAsync(DateOnly.FromDateTime(DateTime.Today));
            Assert.NotNull(result);
            Assert.Equal("StartDate Project", result.Title);
        }

        [Fact]
        public async Task GetByTitleAsync_ReturnsCorrectProject()
        {
            var context = GetInMemoryContext();
            var repo = new ProjectRepository(context);
            var project = new Project { Id = Guid.NewGuid(), Title = "UniqueTitle" };

            await repo.AddAsync(project);
            await context.SaveChangesAsync();

            var result = await repo.GetByTitleAsync("UniqueTitle");
            Assert.NotNull(result);
            Assert.Equal("UniqueTitle", result.Title);
        }

        [Fact]
        public async Task GetAllWithParticipantsAsync_ReturnsProjectsWithParticipants()
        {
            var context = GetInMemoryContext();
            var repo = new ProjectRepository(context);
            var user = new User { Id = Guid.NewGuid(), UserName = "participant" };
            var project = new Project { Id = Guid.NewGuid(), 
                Title = "WithParticipants", Participants = new List<User> { user } };

            await context.Users.AddAsync(user);
            await repo.AddAsync(project);
            await context.SaveChangesAsync();

            var result = await repo.GetAllWithParticipantsAsync();
            Assert.Single(result);
            Assert.Single(result.First().Participants);
        }
        #endregion

        #region StageRepository
        [Fact]
        public async Task AddAsync_AddsStageToDatabase()
        {
            var context = GetInMemoryContext();
            var repo = new StageRepository(context);
            var stage = new Stage { Id = Guid.NewGuid(), Title = "Stage1" };

            await repo.AddAsync(stage);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(stage.Id);
            Assert.NotNull(result);
            Assert.Equal("Stage1", result.Title);
        }
        #endregion

        #region StageAssignmentRepository
        [Fact]
        public async Task AddAsync_AddsStageAssignmentToDatabase()
        {
            var context = GetInMemoryContext();
            var repo = new StageAssignmentRepository(context);
            var assignment = new StageAssignment { StageId = Guid.NewGuid(), 
                UserId = Guid.NewGuid(), StageTitle = "Assignment1" };

            await repo.AddAsync(assignment);
            await context.SaveChangesAsync();

            var result = await context.StageAssignments
            .FirstOrDefaultAsync(sa => sa.StageId == assignment.StageId 
            && sa.UserId == assignment.UserId);
            Assert.NotNull(result);
            Assert.Equal("Assignment1", result.StageTitle);
        }

        [Fact]
        public async Task AddRangeAsync_AddsMultipleStageAssignments()
        {
            var context = GetInMemoryContext();
            var repo = new StageAssignmentRepository(context);
            var assignments = new List<StageAssignment>
            {
                new StageAssignment { StageId = Guid.NewGuid(), 
                    UserId = Guid.NewGuid(), StageTitle = "A1" },
                new StageAssignment { StageId = Guid.NewGuid(), 
                    UserId = Guid.NewGuid(), StageTitle = "A2" }
            };

            await repo.AddRangeAsync(assignments);
            await context.SaveChangesAsync();

            var all = await repo.GetAllAsync();
            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task GetByStageTitleAsync_ReturnsCorrectAssignment()
        {
            var context = GetInMemoryContext();
            var repo = new StageAssignmentRepository(context);
            var assignment = new StageAssignment { StageId = Guid.NewGuid(), 
                UserId = Guid.NewGuid(), StageTitle = "SpecialTitle" };

            await repo.AddAsync(assignment);
            await context.SaveChangesAsync();

            var result = await repo.GetByStageTitleAsync("SpecialTitle");
            Assert.NotNull(result);
            Assert.Equal("SpecialTitle", result.StageTitle);
        }
        #endregion

        #region CommitRepository
        [Fact]
        public async Task AddAsync_AddsCommitToDatabase()
        {
            var context = GetInMemoryContext();
            var repo = new CommitRepository(context);
            var commit = new Commit { Id = Guid.NewGuid(), Message = "Initial commit", 
                CommitDate = DateOnly.FromDateTime(DateTime.Today) };

            await repo.AddAsync(commit);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(commit.Id);
            Assert.NotNull(result);
            Assert.Equal("Initial commit", result.Message);
        }
        #endregion
    }
}