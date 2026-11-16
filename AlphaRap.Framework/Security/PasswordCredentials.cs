using System;
using System.Security.Cryptography;
using System.Text;

namespace AlphaRap.Security
{
    /// <summary>版本化 PBKDF2-SHA256 密码凭据，不保存可还原密码。</summary>
    public static class PasswordCredentials
    {
        private const int Iterations = 600000;
        private const string Prefix = "np1$";
        /// <summary>每份密码使用独立随机盐，记录算法与成本以便后续迁移。</summary>
        public static string Hash(string password)
        {
            byte[] salt = new byte[16];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(salt);
            return Prefix + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(Derive(password, salt, Iterations));
        }

        /// <summary>只接受已知版本和有界成本，固定时间比较派生结果。</summary>
        public static bool Verify(string password, string encoded)
        {
            if (encoded == null || encoded.Length > 256 || !encoded.StartsWith(Prefix, StringComparison.Ordinal)) return false;
            string[] fields = encoded.Split('$');
            int cost;
            if (fields.Length != 4 || !int.TryParse(fields[1], out cost) || cost < 100000 || cost > 2000000) return false;
            try
            {
                byte[] salt = Convert.FromBase64String(fields[2]), expected = Convert.FromBase64String(fields[3]);
                if (salt.Length != 16 || expected.Length != 32) return false;
                return Equal(Derive(password, salt, cost), expected);
            }
            catch (FormatException) { return false; }
        }

        /// <summary>旧凭据验证只用于成功登录迁移，不作为新账号存储格式。</summary>
        internal static bool VerifyLegacy(string password, string legacy)
        {
            using (var digest = SHA256.Create()) return Equal(digest.ComputeHash(Encoding.UTF8.GetBytes(password ?? "")), digest.ComputeHash(Encoding.UTF8.GetBytes(legacy ?? "")));
        }

        /// <summary>使用明确的 SHA256 派生算法，不依赖旧构造函数的默认算法。</summary>
        private static byte[] Derive(string password, byte[] salt, int cost)
        {
            using (var derive = new Rfc2898DeriveBytes(password ?? "", salt, cost, HashAlgorithmName.SHA256)) return derive.GetBytes(32);
        }

        /// <summary>比较固定长度的散列结果，不因首个不同字节提前返回。</summary>
        private static bool Equal(byte[] one, byte[] two)
        {
            if (one.Length != two.Length) return false;
            int difference = 0;
            for (int i = 0; i < one.Length; i++) difference |= one[i] ^ two[i];
            return difference == 0;
        }
    }
}
