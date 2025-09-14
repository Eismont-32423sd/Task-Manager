using Application.Features.Admin;
using Application.Features.Authentication;
using Application.Features.Authorization;
using Application.Features.User;
using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.DTOs.PorjectDTOs;
using Application.Services.Interfaces;
using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Domain.Entities.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests
{
    public class ServicesTests
    {
        private readonly Mock<IUnitOfWork> unitOfWorkMock;
        private readonly Mock<IPasswordHasher> passwordHasherMock;
        private readonly Mock<ILogger<AuthenticationService>> loggerMock;
        private readonly Mock<IJwtGenerator> jwtGeneratorMock;

        public ServicesTests()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            passwordHasherMock = new Mock<IPasswordHasher>();
            loggerMock = new Mock<ILogger<AuthenticationService>>();
            jwtGeneratorMock = new Mock<IJwtGenerator>();
        }

        #region Authentication
        [Fact]
        public async Task RegisterAsync_UserExistsByEmail_ReturnsConflict()
        {
            unitOfWorkMock.Reset();
            passwordHasherMock.Reset();
            loggerMock.Reset();
            jwtGeneratorMock.Reset();

            var existingUser = new User { Email = "test@example.com" };
            unitOfWorkMock.Setup(u => u.UserRepository.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingUser);
            unitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var service = new AuthenticationService(
                unitOfWorkMock.Object,
                passwordHasherMock.Object,
                loggerMock.Object,
                jwtGeneratorMock.Object);

            var request = new RegisterRequest
            {
                Email = "test@example.com",
                UserName = "newuser",
                Password = "password"
            };

            var result = await service.RegisterAsync(request, "http://localhost");

            Assert.False(result.IsSucceded);
            Assert.Contains(result.Errors, e => e.Contains("User with such email already exists"));
            Assert.Equal("Conflict", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_UserExistsByUserName_ReturnsConflict()
        {
            unitOfWorkMock.Reset();
            passwordHasherMock.Reset();
            loggerMock.Reset();
            jwtGeneratorMock.Reset();

            unitOfWorkMock.Setup(u => u.UserRepository.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            var existingUser = new User { UserName = "existinguser" };
            unitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync(existingUser);

            var service = new AuthenticationService(
                unitOfWorkMock.Object,
                passwordHasherMock.Object,
                loggerMock.Object,
                jwtGeneratorMock.Object);

            var request = new RegisterRequest
            {
                Email = "new@example.com",
                UserName = "existinguser",
                Password = "password"
            };

            var result = await service.RegisterAsync(request, "http://localhost");

            Assert.False(result.IsSucceded);
            Assert.Contains(result.Errors, e => e.Contains("User with such username already exists"));
            Assert.Equal("Conflict", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_NewUser_SuccessfulRegistration()
        {
            unitOfWorkMock.Reset();
            passwordHasherMock.Reset();
            loggerMock.Reset();
            jwtGeneratorMock.Reset();

            unitOfWorkMock.Setup(u => u.UserRepository.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            unitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            unitOfWorkMock.Setup(u => u.UserRepository.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            passwordHasherMock.Setup(p => p.Hash(It.IsAny<string>()))
                .Returns("hashedpassword");
            jwtGeneratorMock.Setup(j => j.CreateJwtToken(It.IsAny<User>()))
                .Returns("token");

            var service = new AuthenticationService(
                unitOfWorkMock.Object,
                passwordHasherMock.Object,
                loggerMock.Object,
                jwtGeneratorMock.Object);

            var request = new RegisterRequest
            {
                Email = "new@example.com",
                UserName = "newuser",
                Password = "password"
            };

            var result = await service.RegisterAsync(request, "http://localhost");

            Assert.True(result.IsSucceded);
            Assert.Null(result.Errors);
            Assert.Equal("User registered succesfully", result.Message);
        }
        #endregion
        #region AdminProjectService

        [Fact]
        public async Task GetAllProjectsAsync_NoProjects_ReturnsConflict()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminProjectService>>();
            adminUnitOfWorkMock.Setup(u => u.ProjectRepository.GetAllWithParticipantsAsync())
                .ReturnsAsync((IEnumerable<Project>)null);

            var service = new AdminProjectService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.GetAllProjectsAsync();

            Assert.False(result.IsSucceded);
            Assert.Equal("Conflict", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("There are no available projects"));
        }

        [Fact]
        public async Task GetAllProjectsAsync_ProjectsExist_ReturnsSuccess()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminProjectService>>();
            var projects = new List<Project>
            {
                new Project { Id = Guid.NewGuid(), Title = "Test", Description = "Desc", Participants = new List<User> { new User { Id = Guid.NewGuid(), UserName = "user1" } } }
            };
            adminUnitOfWorkMock.Setup(u => u.ProjectRepository.GetAllWithParticipantsAsync())
                .ReturnsAsync(projects);

            var service = new AdminProjectService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.GetAllProjectsAsync();

            Assert.True(result.IsSucceded);
            Assert.Equal("Retrieved projects", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task CreateProjectAsync_NullRequest_ReturnsError()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminProjectService>>();
            var service = new AdminProjectService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.CreateProjectAsync(null);

            Assert.False(result.IsSucceded);
            Assert.Equal("Error", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Error occured while creating new project"));
        }

        [Fact]
        public async Task CreateProjectAsync_ValidRequest_ReturnsSuccess()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminProjectService>>();
            var request = new CreateProjectRequest
            {
                Title = "New Project",
                Description = "Desc",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                ProjectType = Domain.Entities.Enums.ProjectType.PetProject,
                Status = Domain.Entities.Enums.ProjectStatus.Development
            };
            adminUnitOfWorkMock.Setup(u => u.ProjectRepository.AddAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);
            adminUnitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new AdminProjectService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.CreateProjectAsync(request);

            Assert.True(result.IsSucceded);
            Assert.Equal("Project added succesfully", result.Message);
        }

        [Fact]
        public async Task GetProjectByIdAsync_EmptyId_ReturnsError()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminProjectService>>();
            var service = new AdminProjectService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.GetProjectByIdAsync(Guid.Empty);

            Assert.False(result.IsSucceded);
            Assert.Equal("Provide valid project id", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetProjectByIdAsync_NotFound_ReturnsError()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminProjectService>>();
            var id = Guid.NewGuid();
            adminUnitOfWorkMock.Setup(u => u.ProjectRepository.GetByIdAsync(id))
                .ReturnsAsync((Project)null);

            var service = new AdminProjectService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.GetProjectByIdAsync(id);

            Assert.False(result.IsSucceded);
            Assert.Equal($"Unable to find project with id = {id}", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetProjectByIdAsync_Found_ReturnsSuccess()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminProjectService>>();
            var id = Guid.NewGuid();
            var project = new Project { Id = id, Title = "Test" };
            adminUnitOfWorkMock.Setup(u => u.ProjectRepository.GetByIdAsync(id))
                .ReturnsAsync(project);

            var service = new AdminProjectService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.GetProjectByIdAsync(id);

            Assert.True(result.IsSucceded);
            Assert.Equal("Project retrieved succesfully", result.Message);
            Assert.Equal(project, result.Data);
        }

        #endregion
        #region AdminUserService

        [Fact]
        public async Task GetAllUsersAsync_NoUsers_ReturnsConflict()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            adminUnitOfWorkMock.Setup(u => u.UserRepository.GetAllAsync())
                .ReturnsAsync((IEnumerable<User>)null);

            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.GetAllUsersAsync();

            Assert.False(result.IsSucceded);
            Assert.Equal("Conflict", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("There are no availvable users"));
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAllUsersAsync_UsersExist_ReturnsSuccess()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), UserName = "user1", Email = "user1@example.com" }
            };
            adminUnitOfWorkMock.Setup(u => u.UserRepository.GetAllAsync())
                .ReturnsAsync(users);

            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.GetAllUsersAsync();

            Assert.True(result.IsSucceded);
            Assert.Equal("Users retrieved succesfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task AssignOnProjectAsync_NullRequest_ReturnsError()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.AssignOnProjectAsync(null);

            Assert.False(result.IsSucceded);
            Assert.Equal("Error", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Invalid data"));
        }

        [Fact]
        public async Task AssignOnProjectAsync_ValidRequest_ReturnsSuccess()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var project = new Project { Id = Guid.NewGuid(), Title = "Test", Participants = new List<User>() };
            var user = new User { Id = Guid.NewGuid(), UserName = "user1" };
            var request = new AssignRequest { Titile = "Test", UserNames = new List<string> { "user1" } };

            adminUnitOfWorkMock.Setup(u => u.ProjectRepository.GetByTitleAsync("Test"))
                .ReturnsAsync(project);
            adminUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            adminUnitOfWorkMock.Setup(u => u.ProjectRepository.Update(project));
            adminUnitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.AssignOnProjectAsync(request);

            Assert.True(result.IsSucceded);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public async Task AssignRoleAsync_NullRequest_ReturnsError()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.AssignRoleAsync(null);

            Assert.False(result.IsSucceded);
            Assert.Equal("Error", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Invalid data"));
        }

        [Fact]
        public async Task AssignRoleAsync_UserNotFound_ReturnsConflict()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var request = new AssignRoleRequest { UserName = "user1", Role = Role.Tester };

            adminUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync((User)null);

            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.AssignRoleAsync(request);

            Assert.False(result.IsSucceded);
            Assert.Equal("Conflict", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Couldn`t find user with username:user1"));
        }

        [Fact]
        public async Task AssignRoleAsync_ValidRequest_ReturnsSuccess()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var user = new User { Id = Guid.NewGuid(), UserName = "user1" };
            var request = new AssignRoleRequest { UserName = "user1", Role = Role.TeamLead };

            adminUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            adminUnitOfWorkMock.Setup(u => u.UserRepository.Update(user));
            adminUnitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.AssignRoleAsync(request);

            Assert.True(result.IsSucceded);
            Assert.Equal("Role assigned to user succesfully", result.Message);
        }

        [Fact]
        public async Task DeleteUserAsync_NullUserName_ReturnsNotFound()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.DeleteUserAsync(null);

            Assert.False(result.IsSucceded);
            Assert.Equal("Not Found", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Couldn`t find user with username:"));
        }

        [Fact]
        public async Task DeleteUserAsync_ValidUserName_ReturnsSuccess()
        {
            var adminUnitOfWorkMock = new Mock<IUnitOfWork>();
            var adminLoggerMock = new Mock<ILogger<AdminUserService>>();
            var user = new User { Id = Guid.NewGuid(), UserName = "user1" };

            adminUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            adminUnitOfWorkMock.Setup(u => u.UserRepository.Delete(user));
            adminUnitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new AdminUserService(adminUnitOfWorkMock.Object, adminLoggerMock.Object);

            var result = await service.DeleteUserAsync("user1");

            Assert.True(result.IsSucceded);
            Assert.Equal("User removed succesfully", result.Message);
        }

        #endregion
        #region AuthorizationService

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsConflict()
        {
            var authUnitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGeneratorMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();

            authUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var service = new AuthorizationService(
                authUnitOfWorkMock.Object,
                passwordHasherMock.Object,
                jwtGeneratorMock.Object,
                loggerMock.Object);

            var request = new LoginRequest
            {
                UserName = "notfound",
                Password = "password"
            };

            var result = await service.LoginAsync(request);

            Assert.False(result.IsSucceded);
            Assert.Equal("Conflict", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("User with such username doesn`t exist"));
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task LoginAsync_PasswordDoesNotMatch_ReturnsConflict()
        {
            var authUnitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGeneratorMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();

            var user = new User { UserName = "user1", PasswordHash = "hashed" };
            authUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            passwordHasherMock.Setup(p => p.Verify("wrongpassword", "hashed"))
                .Returns(false);

            var service = new AuthorizationService(
                authUnitOfWorkMock.Object,
                passwordHasherMock.Object,
                jwtGeneratorMock.Object,
                loggerMock.Object);

            var request = new LoginRequest
            {
                UserName = "user1",
                Password = "wrongpassword"
            };

            var result = await service.LoginAsync(request);

            Assert.False(result.IsSucceded);
            Assert.Equal("Conflict", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Password doesn`t match"));
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task LoginAsync_UserNotConfirmed_ReturnsConflict()
        {
            var authUnitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGeneratorMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();

            var user = new User { UserName = "user1", PasswordHash = "hashed", IsConfirmed = false };
            authUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            passwordHasherMock.Setup(p => p.Verify("password", "hashed"))
                .Returns(true);

            var service = new AuthorizationService(
                authUnitOfWorkMock.Object,
                passwordHasherMock.Object,
                jwtGeneratorMock.Object,
                loggerMock.Object);

            var request = new LoginRequest
            {
                UserName = "user1",
                Password = "password"
            };

            var result = await service.LoginAsync(request);

            Assert.False(result.IsSucceded);
            Assert.Equal("Conflict", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Some error ocured"));
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task LoginAsync_Success_ReturnsToken()
        {
            var authUnitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGeneratorMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();

            var user = new User { UserName = "user1", PasswordHash = "hashed", IsConfirmed = true };
            authUnitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            passwordHasherMock.Setup(p => p.Verify("password", "hashed"))
                .Returns(true);
            jwtGeneratorMock.Setup(j => j.CreateJwtToken(user))
                .Returns("token");

            authUnitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new AuthorizationService(
                authUnitOfWorkMock.Object,
                passwordHasherMock.Object,
                jwtGeneratorMock.Object,
                loggerMock.Object);

            var request = new LoginRequest
            {
                UserName = "user1",
                Password = "password"
            };

            var result = await service.LoginAsync(request);

            Assert.True(result.IsSucceded);
            Assert.Equal("User logged in succesfully", result.Message);
            Assert.Equal("token", result.Data);
            Assert.Null(result.Errors);
        }

        #endregion
        #region UserProjectService

        [Fact]
        public async Task AddCommitAsync_NullRequest_ReturnsError()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.AddCommitAsync(null);

            Assert.False(result.IsSucceded);
            Assert.Equal("Error", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Please provide valid data"));
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task AddCommitAsync_StageAssignmentNotFound_ReturnsError()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var user = new User { UserName = "user1" };
            var request = new CommitRequest
            {
                UserName = "user1",
                StageTitle = "Stage1",
                CommitMessage = "Initial commit"
            };

            unitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            unitOfWorkMock.Setup(u => u.StageAssignmentRepository.GetByStageTitleAsync("Stage1"))
                .ReturnsAsync((StageAssignment)null);

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.AddCommitAsync(request);

            Assert.False(result.IsSucceded);
            Assert.Equal("Error", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Error, user user1 is not assigned to the stage"));
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task AddCommitAsync_ValidRequest_ReturnsSuccess()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var user = new User { UserName = "user1" };
            var stageAssignment = new StageAssignment { StageId = Guid.NewGuid(), UserId = Guid.NewGuid(), StageTitle = "Stage1" };
            var request = new CommitRequest
            {
                UserName = "user1",
                StageTitle = "Stage1",
                CommitMessage = "Initial commit"
            };

            unitOfWorkMock.Setup(u => u.UserRepository.GetByUserNameAsync("user1"))
                .ReturnsAsync(user);
            unitOfWorkMock.Setup(u => u.StageAssignmentRepository.GetByStageTitleAsync("Stage1"))
                .ReturnsAsync(stageAssignment);
            unitOfWorkMock.Setup(u => u.CommitRepository.AddAsync(It.IsAny<Commit>()))
                .Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.AddCommitAsync(request);

            Assert.True(result.IsSucceded);
            Assert.Equal("Commit created successfully", result.Message);
            Assert.Null(result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAllCommitsAsync_NoCommits_ReturnsError()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var userId = Guid.NewGuid();

            unitOfWorkMock.Setup(u => u.CommitRepository.GetUserCommitsAsync(userId))
                .ReturnsAsync(new List<Commit>());

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllCommitsAsync(userId);

            Assert.False(result.IsSucceded);
            Assert.Equal("Error", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("No commits found for this user"));
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAllCommitsAsync_CommitsExist_ReturnsSuccess()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var userId = Guid.NewGuid();
            var commits = new List<Commit>
            {
                new Commit { Message = "Commit1", CommitDate = DateOnly.FromDateTime(DateTime.UtcNow) }
            };

            unitOfWorkMock.Setup(u => u.CommitRepository.GetUserCommitsAsync(userId))
                .ReturnsAsync(commits);

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllCommitsAsync(userId);

            Assert.True(result.IsSucceded);
            Assert.Equal("Commits retrieved successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Null(result.Errors);
        }

        #endregion
    }
}