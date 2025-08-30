using System.Security.Cryptography;

namespace taskflow_api.TaskFlow.Shared.Helpers
{
    public static class GenerateRandom
    {
        public static string GenerateRandomToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            var base64 = Convert.ToBase64String(randomNumber);
            var urlSafe = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');

            return urlSafe;
        }

        public static string GenerateRandomNumber()
        {
            int code = RandomNumberGenerator.GetInt32(100000, 1000000);
            return code.ToString();
        }

        public static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var randomNumber = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            var password = new char[8];
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[randomNumber[i] % chars.Length];
            }
            return new string(password);
        }
    }
}
