namespace TaskFlow.Services.Auth.Domain;

using Infrastructure;

public class User
{

    #region Properties

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; }
    public string FullName { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public List<string> Roles { get; }

    #endregion

    #region Constructors

    public User(string email, string passwordHash, string fullName)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        IsEmailConfirmed = false;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        Roles = new List<string>
        {
            "User"
        };
    }

    private User()
    {
        // Required for EF Core
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
        Roles = new List<string>();
    }

    #endregion

    #region Methods

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRole(string role)
    {
        if (!Roles.Contains(role))
        {
            Roles.Add(role);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool VerifyPassword(string password, IPasswordHasher hasher)
    {
        return hasher.Verify(PasswordHash, password);
    }

    #endregion

}
