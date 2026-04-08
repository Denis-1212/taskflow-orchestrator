namespace TaskFlow.Services.Project.Domain;

using Shared.Kernel;

public class Project
{

    #region Fields

    private readonly List<ProjectMember> _members;

    #endregion

    #region Properties

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid OwnerId { get; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    #endregion

    #region Constructors

    public Project(string name, string? description, Guid ownerId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description ?? string.Empty;
        OwnerId = ownerId;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        _members = new List<ProjectMember>();

        _members.Add(new ProjectMember(Id, ownerId, ProjectRole.Owner));
    }

    private Project()
    {
        Name = string.Empty;
        Description = string.Empty;
        _members = [];
    }

    #endregion

    #region Methods

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public Result AddMember(Guid userId, ProjectRole role, Guid addedBy)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            return Result.Failure(Error.Conflict("User is already a member of this project"));
        }

        ProjectRole? currentUserRole = GetMemberRole(addedBy);

        if (currentUserRole != ProjectRole.Owner)
        {
            return Result.Failure(Error.Forbidden("Only project owner can add members"));
        }

        _members.Add(new ProjectMember(Id, userId, role));
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveMember(Guid userId, Guid removedBy)
    {
        if (userId == OwnerId)
        {
            return Result.Failure(Error.Forbidden("Cannot remove project owner"));
        }

        ProjectRole? currentUserRole = GetMemberRole(removedBy);

        if (currentUserRole != ProjectRole.Owner)
        {
            return Result.Failure(Error.Forbidden("Only project owner can remove members"));
        }

        ProjectMember? member = _members.FirstOrDefault(m => m.UserId == userId);

        if (member == null)
        {
            return Result.Failure(Error.NotFound("Member", userId));
        }

        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateMemberRole(Guid userId, ProjectRole newRole, Guid updatedBy)
    {
        if (userId == OwnerId)
        {
            return Result.Failure(Error.Forbidden("Cannot change owner role"));
        }

        ProjectRole? currentUserRole = GetMemberRole(updatedBy);

        if (currentUserRole != ProjectRole.Owner)
        {
            return Result.Failure(Error.Forbidden("Only project owner can change member roles"));
        }

        ProjectMember? member = _members.FirstOrDefault(m => m.UserId == userId);

        if (member == null)
        {
            return Result.Failure(Error.NotFound("Member", userId));
        }

        member.UpdateRole(newRole);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public ProjectRole? GetMemberRole(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId)?.Role;
    }

    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    #endregion

}
