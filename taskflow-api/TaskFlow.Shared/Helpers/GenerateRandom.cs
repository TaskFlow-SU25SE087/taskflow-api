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
    }
}
