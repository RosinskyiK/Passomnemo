using Bitcoin.BIP39;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Passomnemo.Modules
{
    class AuthModule
    {
        public async static Task<string> CreateMnemonicPhrase()
        {
            RandomNumberGenerator r = RandomNumberGenerator.Create();
            BIP39 bip = new();
            bip = await BIP39.GetBIP39Async(entropySize: 256); //128 - 12 words # 256 - 24 words
            return bip.MnemonicSentence;
        }

        public static byte[] GetMnemonicSeed(string mnemonic)
        {
            BIP39 bip = new(mnemonic);
            return bip.SeedBytes;
        }

        public static bool CheckFileMnemonic(string mnemonic)
        {
            byte[] encryptedData = StorageModule.ReadVaultFile();
            try
            {
                byte[] currentMnemonicHash = EncryptionModule.Hash(mnemonic);

                byte[] decryptedData = EncryptionModule.DecryptData(encryptedData, currentMnemonicHash);

                byte[] storedHash = new byte[32];
                Buffer.BlockCopy(decryptedData, 0, storedHash, 0, storedHash.Length);

                return CryptographicOperations.FixedTimeEquals(storedHash, currentMnemonicHash);
            }
            catch
            {
                return false;
            }
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredRead(string target, uint type, uint flags, out IntPtr pCredential);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr buffer);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredDelete(string target, uint type, uint flags);

        public static void InvalidateSession()
        {
            bool result = CredDelete("Passomnemo", 1, 0);
            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                if (error == 1168)
                    return;
                throw new System.ComponentModel.Win32Exception(error);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public uint Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        public static void Save(string password)
        {
            string mnemonic = Convert.ToHexString(EncryptionModule.EncryptData(Encoding.UTF8.GetBytes(StorageModule.Mnemonic), EncryptionModule.Hash(password)));
            string combined = $"{StorageModule.FilePath}|{mnemonic}|{DateTime.Now.AddDays(StorageModule.SettingEntries.SessionTokenLifetime).Ticks}";
            byte[] blob = Encoding.UTF8.GetBytes(combined);
            IntPtr blobPtr = Marshal.AllocHGlobal(blob.Length);
            Marshal.Copy(blob, 0, blobPtr, blob.Length);

            var credential = new CREDENTIAL
            {
                Type = 1,
                TargetName = "Passomnemo",
                CredentialBlobSize = (uint)blob.Length,
                CredentialBlob = blobPtr,
                Persist = 2
            };

            if (!CredWrite(ref credential, 0))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            Marshal.FreeHGlobal(blobPtr);
        }

        public static (bool isValid, bool isExpired, string? filePath, string? mnemonic) Load(string password = "")
        {
            if (!CredRead("Passomnemo", 1, 0, out var credPtr))
                return (false, true, null, null);

            var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
            byte[] blob = new byte[cred.CredentialBlobSize];
            Marshal.Copy(cred.CredentialBlob, blob, 0, (int)cred.CredentialBlobSize);
            string combined = Encoding.UTF8.GetString(blob);
            CredFree(credPtr);

            var parts = combined.Split('|');
            if (parts.Length != 3)
                return (false, true, null, null);

            long ticks = long.Parse(parts[2]);
            DateTime expiry = new DateTime(ticks, DateTimeKind.Local);
            if (expiry < DateTime.Now)
                return (true, true, null, null);
            if (string.IsNullOrEmpty(password))
                return (true, false, null, null);
            string filePath = parts[0];
            byte[] passwordHash = EncryptionModule.Hash(password);
            string mnemonic = Encoding.UTF8.GetString(EncryptionModule.DecryptData(Convert.FromHexString(parts[1]), passwordHash));
            return (true, false, filePath, mnemonic);
        }
    }
}
