namespace TaskFlow.Services.Auth.Domain;

public class RefreshTokenData
{

    #region Properties

    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    #endregion

}
