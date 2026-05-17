using System;
using System.Linq;
using System.Security.Cryptography;

namespace TourDulich.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;
        private const string Prefix = "PBKDF2";

        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = Pbkdf2(password, salt, Iterations);
            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string storedPassword, string password)
        {
            if (string.IsNullOrEmpty(storedPassword) || password == null) return false;

            if (!IsHashed(storedPassword))
            {
                return storedPassword == password;
            }

            var parts = storedPassword.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations)) return false;

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Pbkdf2(password, salt, iterations);
                return FixedTimeEquals(expectedHash, actualHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool NeedsRehash(string storedPassword)
        {
            return !IsHashed(storedPassword);
        }

        private static bool IsHashed(string storedPassword)
        {
            return storedPassword != null && storedPassword.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return deriveBytes.GetBytes(HashSize);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;

            var diff = left.Length ^ right.Length;
            for (var i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
