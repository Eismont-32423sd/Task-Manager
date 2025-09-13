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
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests
{
    public class ServicesTests
    {
        #region AuthenticationService

        [Fact]
        public async Task RegisterAsync_ReturnsConflict_WhenEmailExists()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { Email = "test@test.com" });
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var service = new AuthenticationService(unitOfWorkMock.Object, 
                passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var request = new RegisterRequest { Email = "test@test.com", UserName = "user", Password = "pass" };
            var result = await service.RegisterAsync(request, "http://confirm");

            Assert.False(result.isSucceded);
            // Replace this line:
            // emailFactoryMock.Setup(e => e.SendAsync()).ThrowsAsync(new Exception("SMTP error"));

            // With this:
            emailFactoryMock.Setup(e => e.SendAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP error"));
            Assert.Contains("User with such email already exists", result.errors);
            Assert.Equal("Conflict", result.message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsConflict_WhenUserNameExists()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserName = "user" });

            var service = new AuthenticationService(unitOfWorkMock.Object, 
                passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var request = new RegisterRequest { Email = "new@test.com", UserName = "user", Password = "pass" };
            var result = await service.RegisterAsync(request, "http://confirm");

            Assert.False(result.isSucceded);
            Assert.Contains("User with such user name already exists", result.errors);
            Assert.Equal("Conflict", result.message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsError_WhenEmailSendFails()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
            jwtGenMock.Setup(j => j.CreateJwtToken(It.IsAny<User>())).Returns("token");
            userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            emailFactoryMock.Setup(e => e.To(It.IsAny<string>())).Returns(emailFactoryMock.Object);
            emailFactoryMock.Setup(e => e.Subject(It.IsAny<string>())).Returns(emailFactoryMock.Object);
            emailFactoryMock.Setup(e => e.Body(It.IsAny<string>(), true)).Returns(emailFactoryMock.Object);
            emailFactoryMock.Setup(e => e.SendAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("SMTP error"));

            var service = new AuthenticationService(unitOfWorkMock.Object, passwordHasherMock.Object, 
                jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var request = new RegisterRequest { Email = "new@test.com", UserName = "user", Password = "pass" };
            var result = await service.RegisterAsync(request, "http://confirm");

            Assert.False(result.isSucceded);
            Assert.Contains("Failed to send confirmation email.", result.errors);
            Assert.Contains("User registered, but failed to send confirmation email.", result.message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsSuccess_WhenRegistrationIsValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
            jwtGenMock.Setup(j => j.CreateJwtToken(It.IsAny<User>())).Returns("token");
            userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            emailFactoryMock.Setup(e => e.To(It.IsAny<string>())).Returns(emailFactoryMock.Object);
            emailFactoryMock.Setup(e => e.Subject(It.IsAny<string>())).Returns(emailFactoryMock.Object);
            emailFactoryMock.Setup(e => e.Body(It.IsAny<string>(), true)).Returns(emailFactoryMock.Object);
            // Replace this line:
            // emailFactoryMock.Setup(static e => e.SendAsync()).ReturnsAsync(new FluentEmail.Core.Models.SendResponse { Success = true });

            // With this:
           

            var service = new AuthenticationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var request = new RegisterRequest { Email = "new@test.com", UserName = "user", Password = "pass" };
            var result = await service.RegisterAsync(request, "http://confirm");

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("User registered succesfully", result.message);
            Assert.NotNull(result.ConfiramationToken);
            Assert.NotNull(result.UserId);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsBadRequest_WhenRequestOrTokenIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();

            var service = new AuthenticationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var result = await service.ConfirmEmailAsync(null, null);

            Assert.False(result.isSucceded);
            Assert.Contains("Invalid confirmation data", result.errors);
            Assert.Equal("Bad Request", result.message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsBadRequest_WhenUserNotFound()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var service = new AuthenticationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var confirmRequest = new ConfirmRequest { Email = "notfound@test.com" };
            var result = await service.ConfirmEmailAsync(confirmRequest, "token");

            Assert.False(result.isSucceded);
            Assert.Contains("User with such email doesn`t exists", result.errors);
            Assert.Equal("Bad request", result.message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { Email = "test@test.com", ConfirmationToken = "validtoken" });

            var service = new AuthenticationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var confirmRequest = new ConfirmRequest { Email = "test@test.com" };
            var result = await service.ConfirmEmailAsync(confirmRequest, "invalidtoken");

            Assert.False(result.isSucceded);
            Assert.Contains("Invalid or expired reset token", result.errors);
            Assert.Equal("Unauthorized", result.message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsError_WhenExceptionThrown()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            var user = new User { Email = "test@test.com", ConfirmationToken = "token" };
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userRepoMock.Setup(r => r.Update(It.IsAny<User>())).Throws(new Exception("DB error"));

            var service = new AuthenticationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var confirmRequest = new ConfirmRequest { Email = "test@test.com" };
            var result = await service.ConfirmEmailAsync(confirmRequest, "token");

            Assert.False(result.isSucceded);
            Assert.Contains("Couldn`t verify email, DB error", result.errors.First());
            Assert.Equal("Error", result.message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ReturnsSuccess_WhenConfirmationIsValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var emailFactoryMock = new Mock<IFluentEmail>();
            var loggerMock = new Mock<ILogger<AuthenticationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            var user = new User { Email = "test@test.com", ConfirmationToken = "token" };
            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userRepoMock.Setup(r => r.Update(It.IsAny<User>()));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AuthenticationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, emailFactoryMock.Object, loggerMock.Object);

            var confirmRequest = new ConfirmRequest { Email = "test@test.com" };
            var result = await service.ConfirmEmailAsync(confirmRequest, "token");

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Email confirmed succesfully", result.message);
        }

        #endregion
        #region AuthorizationService

        [Fact]
        public async Task LoginAsync_ReturnsConflict_WhenUserNotFound()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var service = new AuthorizationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, loggerMock.Object);

            var loginRequest = new LoginRequest { UserName = "notfound", Password = "pass" };
            var result = await service.LoginAsync(loginRequest);

            Assert.False(result.isSucceded);
            Assert.Contains("User with such username doesn`t exist", result.errors);
            Assert.Equal("Conflict", result.message);
            Assert.Null(result.token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsConflict_WhenPasswordDoesNotMatch()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            var user = new User { UserName = "user", PasswordHash = "hashed", IsConfirmed = true };
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var service = new AuthorizationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, loggerMock.Object);

            var loginRequest = new LoginRequest { UserName = "user", Password = "wrongpass" };
            var result = await service.LoginAsync(loginRequest);

            Assert.False(result.isSucceded);
            Assert.Contains("Password doesn`t match", result.errors);
            Assert.Equal("Conflict", result.message);
            Assert.Null(result.token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsConflict_WhenUserNotConfirmed()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            var user = new User { UserName = "user", PasswordHash = "hashed", IsConfirmed = false };
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var service = new AuthorizationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, loggerMock.Object);

            var loginRequest = new LoginRequest { UserName = "user", Password = "pass" };
            var result = await service.LoginAsync(loginRequest);

            Assert.False(result.isSucceded);
            Assert.Contains("You did`nt confirm your data, please check your email box", result.errors);
            Assert.Equal("Conflict", result.message);
            Assert.Null(result.token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsSuccess_WhenCredentialsAreValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var passwordHasherMock = new Mock<IPasswordHasher>();
            var jwtGenMock = new Mock<IJwtGenerator>();
            var loggerMock = new Mock<ILogger<AuthorizationService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            var user = new User { UserName = "user", PasswordHash = "hashed", IsConfirmed = true };
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            jwtGenMock.Setup(j => j.CreateJwtToken(It.IsAny<User>())).Returns("token");

            var service = new AuthorizationService(unitOfWorkMock.Object, passwordHasherMock.Object, jwtGenMock.Object, loggerMock.Object);

            var loginRequest = new LoginRequest { UserName = "user", Password = "pass" };
            var result = await service.LoginAsync(loginRequest);

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Logged in succesfully", result.message);
            Assert.Equal("token", result.token);
        }

        #endregion
        #region AdminProjectService

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsConflict_WhenNoProjects()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetAllWithParticipantsAsync()).ReturnsAsync(new List<Project>());

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllProjectsAsync();

            Assert.False(result.isSucceded);
            Assert.Contains("There are no available projects", result.errors);
            Assert.Equal("Conflict", result.message);
            Assert.Null(result.projects);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsSuccess_WhenProjectsExist()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();

            var user = new User { Id = Guid.NewGuid(), UserName = "user" };
            var project = new Project { Id = Guid.NewGuid(), Title = "Project", Participants = new List<User> { user } };
            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetAllWithParticipantsAsync()).ReturnsAsync(new List<Project> { project });

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllProjectsAsync();

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Retrieved projects", result.message);
            Assert.Single(result.projects);
            Assert.Equal("Project", result.projects[0].Title);
            Assert.Single(result.projects[0].Participants);
        }

        [Fact]
        public async Task CreateProjectAsync_ReturnsError_WhenRequestIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.CreateProjectAsync(null);

            Assert.False(result.isSucceded);
            Assert.Contains("Error occured while creating new project", result.errors);
            Assert.Equal("Error", result.message);
        }

        [Fact]
        public async Task CreateProjectAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.AddAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new CreateProjectRequest { Title = "New Project" };
            var result = await service.CreateProjectAsync(request);

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Project added succesfully", result.message);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsError_WhenIdIsEmpty()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetProjectByIdAsync(Guid.Empty);

            Assert.False(result.isSucceded);
            Assert.Contains("Provide valid project id", result.errors);
            Assert.Equal("Error", result.message);
            Assert.Null(result.project);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsError_WhenProjectNotFound()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Project)null);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetProjectByIdAsync(Guid.NewGuid());

            Assert.False(result.isSucceded);
            Assert.Contains("Unable to find project with id =", result.errors.First());
            Assert.Equal("Error", result.message);
            Assert.Null(result.project);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsSuccess_WhenProjectFound()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();
            var project = new Project { Id = Guid.NewGuid(), Title = "Project" };

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetProjectByIdAsync(project.Id);

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Project retrieved succesfully", result.message);
            Assert.Equal(project, result.project);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsNotFound_WhenTitleIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.UpdateProjectAsync(null, new UpdateProjectRequest());

            Assert.False(result.isSucceded);
            Assert.Contains("Please provide project title", result.errors);
            Assert.Equal("Not Found", result.message);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsConflict_WhenRequestIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.UpdateProjectAsync("title", null);

            Assert.False(result.isSucceded);
            Assert.Contains("Invalid update data", result.errors);
            Assert.Equal("Conflict", result.message);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsNotFound_WhenProjectNotFound()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetByTitleAsync(It.IsAny<string>())).ReturnsAsync((Project)null);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.UpdateProjectAsync("title", new UpdateProjectRequest());

            Assert.False(result.isSucceded);
            Assert.Contains("Couldn`t find porject with title title", result.errors);
            Assert.Equal("Not Found", result.message);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();
            var project = new Project { Id = Guid.NewGuid(), Title = "title" };

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetByTitleAsync(It.IsAny<string>())).ReturnsAsync(project);
            projectRepoMock.Setup(r => r.Update(It.IsAny<Project>()));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new UpdateProjectRequest { Title = "newtitle" };
            var result = await service.UpdateProjectAsync("title", request);

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Project data changed succesfully", result.message);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsNotFound_WhenTitleIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.DeleteProjectAsync(null);

            Assert.False(result.isSucceded);
            Assert.Contains("Could`nt find project with title:", result.errors.First());
            Assert.Equal("Not Found", result.message);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsNotFound_WhenProjectNotFound()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetByTitleAsync(It.IsAny<string>())).ReturnsAsync((Project)null);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.DeleteProjectAsync("title");

            Assert.False(result.isSucceded);
            Assert.Contains("Could`nt find project with title:title", result.errors.First());
            Assert.Equal("Not Found", result.message);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();
            var project = new Project { Id = Guid.NewGuid(), Title = "title" };

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            projectRepoMock.Setup(r => r.GetByTitleAsync(It.IsAny<string>())).ReturnsAsync(project);
            projectRepoMock.Setup(r => r.Delete(It.IsAny<Project>()));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.DeleteProjectAsync("title");

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Project titled title deleted succesfully", result.message);
        }

        [Fact]
        public async Task AddStagesToProjectAsync_ReturnsBadRequest_WhenRequestIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.AddStagesToProjectAsync(null);

            Assert.False(result.isSucceded);
            Assert.Contains("Provide valid data", result.errors);
            Assert.Equal("Bad Request", result.message);
        }

        [Fact]
        public async Task AddStagesToProjectAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminProjectService>>();
            var projectRepoMock = new Mock<IProjectRepository>();
            var stageRepoMock = new Mock<IStageRepository>();
            var stageAssignmentRepoMock = new Mock<IStageAssignmentRepository>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            unitOfWorkMock.Setup(u => u.StageRepository).Returns(stageRepoMock.Object);
            unitOfWorkMock.Setup(u => u.StageAssignmentRepository).Returns(stageAssignmentRepoMock.Object);
            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);

            projectRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Project { Id = Guid.NewGuid() });
            stageRepoMock.Setup(r => r.AddAsync(It.IsAny<Stage>())).Returns(Task.CompletedTask);
            stageAssignmentRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<List<StageAssignment>>())).Returns(Task.CompletedTask);
            userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new User());

            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AdminProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new AddStageRequest
            {
                ProjectId = Guid.NewGuid(),
                Title = "Stage",
                AssignedUserIds = new List<Guid> { Guid.NewGuid() }
            };
            var result = await service.AddStagesToProjectAsync(request);

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Stage added succesfully", result.message);
        }

        #endregion
        #region AdminUserService

        [Fact]
        public async Task GetAllUsersAsync_ReturnsConflict_WhenNoUsers()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync((List<User>)null);

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllUsersAsync();

            Assert.False(result.isSucceded);
            Assert.Contains("There are no availvable users", result.errors);
            Assert.Equal("Conflict", result.message);
            Assert.Null(result.users);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsSuccess_WhenUsersExist()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();
            var userRepoMock = new Mock<IUserRepository>();
            var users = new List<User> { new User { Id = Guid.NewGuid(), UserName = "user" } };

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllUsersAsync();

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Retrieved users", result.message);
            Assert.Single(result.users);
        }

        [Fact]
        public async Task AssignOnProjectAsync_ReturnsError_WhenRequestIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.AssignOnProjectAsync(null);

            Assert.False(result.isSucceded);
            Assert.Contains("Invalid data", result.errors);
            Assert.Equal("Error", result.message);
        }

        [Fact]
        public async Task AssignOnProjectAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();
            var projectRepoMock = new Mock<IProjectRepository>();
            var userRepoMock = new Mock<IUserRepository>();
            var project = new Project { Id = Guid.NewGuid(), Title = "Project", Participants = new List<User>() };
            var user = new User { Id = Guid.NewGuid(), UserName = "user" };

            unitOfWorkMock.Setup(u => u.ProjectRepository).Returns(projectRepoMock.Object);
            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            projectRepoMock.Setup(r => r.GetByTitleAsync(It.IsAny<string>())).ReturnsAsync(project);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            projectRepoMock.Setup(r => r.Update(It.IsAny<Project>()));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new AssignRequest { Titile = "Project", UserNames = new List<string> { "user" } };
            var result = await service.AssignOnProjectAsync(request);

            Assert.True(result.isSucceded);
            Assert.Contains("User assigned to project succesfully", result.errors);
            Assert.Equal("Success", result.message);
        }

        [Fact]
        public async Task AssignRoleAsync_ReturnsError_WhenRequestIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.AssignRoleAsync(null);

            Assert.False(result.isSucceded);
            Assert.Contains("Invalid data", result.errors);
            Assert.Equal("Error", result.message);
        }

        [Fact]
        public async Task AssignRoleAsync_ReturnsConflict_WhenUserNotFound()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();
            var userRepoMock = new Mock<IUserRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new AssignRoleRequest { UserName = "user", Role = Role.TeamLead };
            var result = await service.AssignRoleAsync(request);

            Assert.False(result.isSucceded);
            Assert.Contains("Couldn`t find user with username:user", result.errors);
            Assert.Equal("Conflict", result.message);
        }

        [Fact]
        public async Task AssignRoleAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();
            var userRepoMock = new Mock<IUserRepository>();
            var user = new User { Id = Guid.NewGuid(), UserName = "user" };

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            userRepoMock.Setup(r => r.Update(It.IsAny<User>()));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new AssignRoleRequest { UserName = "user", Role = Role.TeamLead };
            var result = await service.AssignRoleAsync(request);

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Role assigned to user succesfully", result.message);
        }

        [Fact]
        public async Task DeleteUserAsync_ReturnsNotFound_WhenUserNameIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.DeleteUserAsync(null);

            Assert.False(result.isSucceded);
            Assert.Contains("Couldn`t find user with username:", result.errors.First());
            Assert.Equal("Not Found", result.message);
        }

        [Fact]
        public async Task DeleteUserAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AdminUserService>>();
            var userRepoMock = new Mock<IUserRepository>();
            var user = new User { Id = Guid.NewGuid(), UserName = "user" };

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            userRepoMock.Setup(r => r.Delete(It.IsAny<User>()));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new AdminUserService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.DeleteUserAsync("user");

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("User removed succesfully", result.message);
        }

        #endregion
        #region UserProjectService

        [Fact]
        public async Task AddCommitAsync_ReturnsError_WhenRequestIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.AddCommitAsync(null);

            Assert.False(result.isSucceded);
            Assert.Contains("Please provide valid data", result.errors);
            Assert.Equal("Error", result.message);
        }

        [Fact]
        public async Task AddCommitAsync_ReturnsError_WhenStageAssignmentIsNull()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var userRepoMock = new Mock<IUserRepository>();
            var stageAssignmentRepoMock = new Mock<IStageAssignmentRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            unitOfWorkMock.Setup(u => u.StageAssignmentRepository).Returns(stageAssignmentRepoMock.Object);

            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserName = "user" });
            stageAssignmentRepoMock.Setup(r => r.GetByStageTitleAsync(It.IsAny<string>()))
                .ReturnsAsync((StageAssignment)null);

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new CommitRequest { UserName = "user", StageTitle = "stage", CommitMessage = "msg" };
            var result = await service.AddCommitAsync(request);

            Assert.False(result.isSucceded);
            Assert.Contains("User is not assigned to this stage.", result.errors);
            Assert.Equal("Error", result.message);
        }

        [Fact]
        public async Task AddCommitAsync_ReturnsSuccess_WhenValid()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var userRepoMock = new Mock<IUserRepository>();
            var stageAssignmentRepoMock = new Mock<IStageAssignmentRepository>();
            var commitRepoMock = new Mock<ICommitRepository>();

            unitOfWorkMock.Setup(u => u.UserRepository).Returns(userRepoMock.Object);
            unitOfWorkMock.Setup(u => u.StageAssignmentRepository).Returns(stageAssignmentRepoMock.Object);
            unitOfWorkMock.Setup(u => u.CommitRepository).Returns(commitRepoMock.Object);

            userRepoMock.Setup(r => r.GetByUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserName = "user" });
            stageAssignmentRepoMock.Setup(r => r.GetByStageTitleAsync(It.IsAny<string>()))
                .ReturnsAsync(new StageAssignment { StageId = Guid.NewGuid(), UserId = Guid.NewGuid(), StageTitle = "stage" });
            commitRepoMock.Setup(r => r.AddAsync(It.IsAny<Commit>())).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var request = new CommitRequest { UserName = "user", StageTitle = "stage", CommitMessage = "msg" };
            var result = await service.AddCommitAsync(request);

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Commit added successfully.", result.message);
        }

        [Fact]
        public async Task GetAllCommitsAsync_ReturnsError_WhenNoCommits()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var commitRepoMock = new Mock<ICommitRepository>();

            unitOfWorkMock.Setup(u => u.CommitRepository).Returns(commitRepoMock.Object);
            commitRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync((List<Commit>)null);

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllCommitsAsync();

            Assert.False(result.isSucceded);
            Assert.Contains("Couldn`t find any commits", result.errors);
            Assert.Equal("Errors", result.message);
            Assert.Null(result.commits);
        }

        [Fact]
        public async Task GetAllCommitsAsync_ReturnsSuccess_WhenCommitsExist()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UserProjectService>>();
            var commitRepoMock = new Mock<ICommitRepository>();
            var commits = new List<Commit> { new Commit { Message = "msg" } };

            unitOfWorkMock.Setup(u => u.CommitRepository).Returns(commitRepoMock.Object);
            commitRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(commits);

            var service = new UserProjectService(unitOfWorkMock.Object, loggerMock.Object);

            var result = await service.GetAllCommitsAsync();

            Assert.True(result.isSucceded);
            Assert.Null(result.errors);
            Assert.Equal("Commits retrieved succesfully", result.message);
            Assert.Single(result.commits);
        }

        #endregion
    }
}