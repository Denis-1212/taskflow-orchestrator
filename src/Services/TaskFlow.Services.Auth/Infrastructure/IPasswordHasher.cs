namespace TaskFlow.Services.Auth.Infrastructure;

public interface IPasswordHasher
{

    #region Methods

    string Hash(string password);
    bool Verify(string hash, string password);

    #endregion

}
