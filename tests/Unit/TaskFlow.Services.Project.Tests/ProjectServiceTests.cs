namespace TaskFlow.Services.Project.Tests;

using Application.Services;

using Auth;

using Clients;

using Domain;

using FluentAssertions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using RabbitMQ.Module.Contracts;

using Shared.Kernel;

using Project = Domain.Project;
using Task = Task;

public class ProjectServiceTests : IDisposable
{

    #region Fields

    private readonly ProjectDbContext _context;
    private readonly ProjectService _projectService;
    private readonly Guid _memberId;

    #endregion

    #region Constructors

    public ProjectServiceTests()
    {
        _context = TestDatabase.Create();
        var loggerMock = new Mock<ILogger<ProjectService>>();
        var authGrpcClientMock = new Mock<IAuthGrpcClient>();
        var publisher = new Mock<IPublisher>();
        _memberId = Guid.NewGuid();
        authGrpcClientMock.Setup(r => r.GetUserAsync(_memberId)).ReturnsAsync(new GetUserResponse());
        _projectService = new ProjectService(_context, authGrpcClientMock.Object, publisher.Object, loggerMock.Object);
    }

    #endregion

    #region Methods

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProject()
    {
        // Arrange
        string name = "Test Project";
        string description = "Test Description";
        var ownerId = Guid.NewGuid();

        // Act
        Result<ProjectResult> result = await _projectService.CreateAsync(name, description, ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.OwnerId.Should().Be(ownerId);

        Project? projectInDb = await _context.Projects
                                   .Include(p => p.Members)
                                   .FirstOrDefaultAsync(p => p.Id == result.Value.Id);

        projectInDb.Should().NotBeNull();
        projectInDb!.Members.Should().HaveCount(1);
        projectInDb.Members.First().UserId.Should().Be(ownerId);
        projectInDb.Members.First().Role.Should().Be(ProjectRole.Owner);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidUser_ShouldReturnProject()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<ProjectResult> result = await _projectService.GetByIdAsync(project.Id, ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(project.Id);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonMember_ShouldReturnForbidden()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var nonMemberId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<ProjectResult> result = await _projectService.GetByIdAsync(project.Id, nonMemberId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedProject_ShouldReturnNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        project.SoftDelete();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<ProjectResult> result = await _projectService.GetByIdAsync(project.Id, ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_ByOwner_ShouldSucceed()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var project = new Project("Old Name", "Old Description", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<ProjectResult> result = await _projectService.UpdateAsync(project.Id, "New Name", "New Description", ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task UpdateAsync_ByNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var nonOwnerId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<ProjectResult> result = await _projectService.UpdateAsync(project.Id, "New Name", "New Description", nonOwnerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task DeleteAsync_ByOwner_ShouldSoftDelete()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _projectService.DeleteAsync(project.Id, ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Project? deletedProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);
        deletedProject.Should().NotBeNull();
        deletedProject!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task AddMemberAsync_ByOwner_ShouldSucceed()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        // var newMemberId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _projectService.AddMemberAsync(project.Id, _memberId, "Member", ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Project? updatedProject = await _context.Projects
                                      .Include(p => p.Members)
                                      .FirstOrDefaultAsync(p => p.Id == project.Id);

        updatedProject!.Members.Should().HaveCount(2);
        updatedProject.Members.Any(m => m.UserId == _memberId).Should().BeTrue();
    }

    [Fact]
    public async Task AddMemberAsync_WithDuplicate_ShouldReturnConflict()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act - first add
        await _projectService.AddMemberAsync(project.Id, ownerId, "Member", ownerId);

        // Act - second add (duplicate)
        Result result = await _projectService.AddMemberAsync(project.Id, ownerId, "Member", ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task RemoveMemberAsync_ByOwner_ShouldSucceed()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var project = new Project("Test", "Description", ownerId);
        project.AddMember(memberId, ProjectRole.Member, ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _projectService.RemoveMemberAsync(project.Id, memberId, ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Project? updatedProject = await _context.Projects
                                      .Include(p => p.Members)
                                      .FirstOrDefaultAsync(p => p.Id == project.Id);

        updatedProject!.Members.Should().HaveCount(1);
        updatedProject.Members.Any(m => m.UserId == memberId).Should().BeFalse();
    }

    [Fact]
    public async Task GetUserProjectsAsync_ShouldReturnUserProjects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var project1 = new Project("Project 1", "Desc", userId);
        var project2 = new Project("Project 2", "Desc", userId);

        _context.Projects.Add(project1);
        _context.Projects.Add(project2);
        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<ProjectResult>> result = await _projectService.GetUserProjectsAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task ValidateMemberAsync_ForExistingMember_ShouldReturnTrue()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var project = new Project("Test", "Desc", ownerId);
        project.AddMember(memberId, ProjectRole.Member, ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<MemberValidationResult> result = await _projectService.ValidateMemberAsync(project.Id, memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsMember.Should().BeTrue();
        result.Value.Role.Should().Be("Member");
    }

    [Fact]
    public async Task ValidateMemberAsync_ForNonMember_ShouldReturnFalse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var nonMemberId = Guid.NewGuid();
        var project = new Project("Test", "Desc", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<MemberValidationResult> result = await _projectService.ValidateMemberAsync(project.Id, nonMemberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsMember.Should().BeFalse();
        result.Value.Role.Should().BeEmpty();
    }

    [Fact]
    public async Task ProjectExistsAsync_ForExistingProject_ShouldReturnTrue()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var project = new Project("Test", "Desc", ownerId);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<bool> result = await _projectService.ProjectExistsAsync(project.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ProjectExistsAsync_ForDeletedProject_ShouldReturnFalse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var project = new Project("Test", "Desc", ownerId);
        project.SoftDelete();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        Result<bool> result = await _projectService.ProjectExistsAsync(project.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

}
