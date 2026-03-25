namespace TaskFlow.Services.Project.Domain;

public class ProjectMember
{

    #region Properties

    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public ProjectRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    #endregion

    #region Constructors

    public ProjectMember(Guid projectId, Guid userId, ProjectRole role)
    {
        ProjectId = projectId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    private ProjectMember()
    {
    }

    #endregion

    #region Methods

    public void UpdateRole(ProjectRole newRole)
    {
        Role = newRole;
    }

    #endregion

}
