using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace AcademiaDoZe.Application.Security;

// Hash de senha com Argon2id (Konscious). Formato: ARGON2ID:{t}:{mKb}:{p}:{saltBase64}:{hashBase64}
public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int DefaultMemorySizeKb = 65536; // 64 MB
    private const int DefaultIterations = 3;

    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        int p = Math.Clamp(Environment.ProcessorCount, 1, 4);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = p,
            MemorySize = DefaultMemorySizeKb,
            Iterations = DefaultIterations
        };

        byte[] hash = argon2.GetBytes(HashSize);

        return $"ARGON2ID:{DefaultIterations}:{DefaultMemorySizeKb}:{p}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        var parts = passwordHash.Split(':');
        if (parts.Length != 6) return false;
        if (!parts[0].Equals("ARGON2ID", StringComparison.Ordinal)) return false;

        if (!int.TryParse(parts[1], out int t)) return false;
        if (!int.TryParse(parts[2], out int mKb)) return false;
        if (!int.TryParse(parts[3], out int p)) return false;

        if (t is < 1 or > 10 || mKb is < 8192 or > 262144 || p is < 1 or > 64) return false;

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[4]);
            expected = Convert.FromBase64String(parts[5]);
        }
        catch
        {
            return false;
        }

        if (salt.Length < 8 || salt.Length > 64 || expected.Length != HashSize) return false;

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = p,
            MemorySize = mKb,
            Iterations = Math.Max(1, t)
        };

        byte[] actual = argon2.GetBytes(expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}