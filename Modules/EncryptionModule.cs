using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Diagnostics;

namespace Passomnemo.Modules
{
    class EncryptionModule
    {
        public static byte[] Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
                return [];
            byte[] salt = Encoding.UTF8.GetBytes($"Argon2Salt{text}");
            using (var argon2 = new Argon2id(salt))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 4; // потоки
                argon2.MemorySize = 65536; // в кілобайтах
                argon2.Iterations = 3;

                return argon2.GetBytes(32); // 256-бітний ключ
            }
        }

        public static byte[] EncryptData(byte[] data, byte[] masterkey)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = masterkey;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plaintextBytes = data;
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                    Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                    return result;
                }
            }
        }

        public static byte[] DecryptData(byte[] data, byte[] masterkey)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = masterkey;

                    byte[] iv = new byte[16];
                    Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    byte[] encryptedBytes = new byte[data.Length - iv.Length];
                    Buffer.BlockCopy(data, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return decryptedBytes;
                    }
                }
            }
            catch (Exception e) { Debug.WriteLine(e.Message); return []; }
        }

        public static byte[] DeriveKeyFromPassphrase(string passphrase, string userPassword)
        {
            byte[] seed = AuthModule.GetMnemonicSeed(passphrase);
            byte[] salt = Encoding.UTF8.GetBytes($"Mnemonic{userPassword}");
            using (var argon2 = new Argon2id(seed))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 4; // потоки
                argon2.MemorySize = 65536; // в кілобайтах
                argon2.Iterations = 3;

                return argon2.GetBytes(32); // 256-бітний ключ
            }
        }

        public static byte[] GenerateKey(int size)
        {
            byte[] masterKey = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(masterKey);
            }
            return masterKey;
        }
    }
}
