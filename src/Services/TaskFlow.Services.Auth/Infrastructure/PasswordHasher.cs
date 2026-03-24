namespace TaskFlow.Services.Auth.Infrastructure;

using System.Security.Cryptography;

public class PasswordHasher : IPasswordHasher
{

    #region Constants

    private const int SALT_SIZE = 16;
    private const int HASH_SIZE = 32;
    private const int ITERATIONS = 100000;

    #endregion

    #region Methods

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            ITERATIONS,
            HashAlgorithmName.SHA256,
            HASH_SIZE);

        byte[] result = new byte[SALT_SIZE + HASH_SIZE];
        Array.Copy(salt, 0, result, 0, SALT_SIZE);
        Array.Copy(hash, 0, result, SALT_SIZE, HASH_SIZE);

        return Convert.ToBase64String(result);
    }

    public bool Verify(string hash, string password)
    {
        byte[] hashBytes = Convert.FromBase64String(hash);
        byte[] salt = new byte[SALT_SIZE];
        Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);

        byte[] expectedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            ITERATIONS,
            HashAlgorithmName.SHA256,
            HASH_SIZE);

        byte[] actualHash = new byte[HASH_SIZE];
        Array.Copy(hashBytes, SALT_SIZE, actualHash, 0, HASH_SIZE);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }

    #endregion

}
