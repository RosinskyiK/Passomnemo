using System.Collections.Generic;
using System.Formats.Cbor;
using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;

namespace Passomnemo.Modules
{
    public class StorageModule
    {
        public static string Mnemonic { get; set; } = "";
        public static string FilePath { get; set; } = "";
        public static ObservableCollection<AccountEntry> AccountEntries { get; private set; } = new();
        public static SettingsEntry SettingEntries { get; set; } = new();
        //vault file
        public static void CreateVaultFile()
        {
            File.Create(FilePath).Close();
        }

        private static void UpdateVaultFile(byte[] data)
        {
            if (!File.Exists(FilePath))
                CreateVaultFile();
            File.WriteAllBytes(FilePath, data);
        }

        public static byte[] ReadVaultFile()
        {
            return File.ReadAllBytes(FilePath);
        }
        //account entries
        public static void AddAccountEntry(AccountEntry entry)
        {
            AccountEntries.Add(entry);
        }

        public static void RemoveAccountEntry(string name)
        {
            AccountEntries.Remove(GetAccountEntry(name));
        }

        public static AccountEntry GetAccountEntry(string name)
        {
            return AccountEntries.First(x => x.ServiceName == name);
        }

        public static void UpdateAccountEntry(AccountEntry entry, string name)
        {
            AccountEntries.Replace(GetAccountEntry(name), entry);
        }

        //data
        public static void FlushAppData()
        {
            Mnemonic = "";
            FilePath = "";
            AccountEntries = new();
            SettingEntries = new();
        }

        public static bool SaveData(string password)
        {
            try
            {
                byte[] cborData = SerializeData();
                byte[] masterKey = EncryptionModule.DeriveKeyFromPassphrase(Mnemonic, password);
                byte[] encryptedData = EncryptionModule.EncryptData(cborData, masterKey);

                byte[] hashMnemonic = EncryptionModule.Hash(Mnemonic);
                Console.WriteLine($"[LOAD DATA] HashMnemonic size: {hashMnemonic.Length}");
                byte[] combinedData = new byte[hashMnemonic.Length + encryptedData.Length];

                Buffer.BlockCopy(hashMnemonic, 0, combinedData, 0, hashMnemonic.Length);
                Buffer.BlockCopy(encryptedData, 0, combinedData, hashMnemonic.Length, encryptedData.Length);

                byte[] doubleEncryptedData = EncryptionModule.EncryptData(combinedData, hashMnemonic);

                UpdateVaultFile(doubleEncryptedData);
                return true;
            }
            catch { return false; }
        }

        public static bool LoadData(string password)
        {
            try
            {
                byte[] encryptedData = ReadVaultFile();
                byte[] hashMnemonic = EncryptionModule.Hash(Mnemonic);
                byte[] decryptedData = EncryptionModule.DecryptData(encryptedData, hashMnemonic);

                byte[] storedHash = new byte[32];
                Buffer.BlockCopy(decryptedData, 0, storedHash, 0, storedHash.Length);
                byte[] combinedData = new byte[decryptedData.Length - storedHash.Length];
                Buffer.BlockCopy(decryptedData, storedHash.Length, combinedData, 0, combinedData.Length);

                byte[] masterKey = EncryptionModule.DeriveKeyFromPassphrase(Mnemonic, password);
                byte[] doubleDecryptedData = EncryptionModule.DecryptData(combinedData, masterKey);

                DeserializeData(doubleDecryptedData);
                return true;
            }
            catch { return false; }
        }

        private static byte[] SerializeData()
        {
            var writer = new CborWriter();

            writer.WriteStartArray(2);

            // accounts
            writer.WriteStartArray(AccountEntries.Count);
            foreach (var account in AccountEntries)
            {
                writer.WriteStartMap(6);
                writer.WriteTextString("ServiceName");
                writer.WriteTextString(account.ServiceName);
                writer.WriteTextString("Website");
                writer.WriteTextString(account.Website);
                writer.WriteTextString("Login");
                writer.WriteTextString(account.Login);
                writer.WriteTextString("Password");
                writer.WriteTextString(account.Password);
                writer.WriteTextString("LastModified");
                writer.WriteInt64(account.LastModified.ToBinary());
                writer.WriteTextString("Tags");

                writer.WriteStartArray(account.Tags.Count);
                foreach (var tag in account.Tags)
                {
                    writer.WriteTextString(tag);
                }
                writer.WriteEndArray();

                writer.WriteEndMap();
            }
            writer.WriteEndArray();

            // settings
            writer.WriteStartMap(5);
            writer.WriteTextString("OldPasswordRemindTime");
            writer.WriteInt32(SettingEntries.OldPasswordRemindTime);
            writer.WriteTextString("SessionTokenLifetime");
            writer.WriteInt32(SettingEntries.SessionTokenLifetime);
            writer.WriteTextString("BackupRemindTime");
            writer.WriteInt32(SettingEntries.BackupRemindTime);
            writer.WriteTextString("LastBackupDate");
            writer.WriteInt64(SettingEntries.LastBackupDate.ToBinary());
            writer.WriteTextString("PasswordHash");
            writer.WriteByteString(SettingEntries.PasswordHash);
            writer.WriteEndMap();

            writer.WriteEndArray();

            return writer.Encode();
        }

        private static void DeserializeData(byte[] data)
        {
            var reader = new CborReader(data);

            int mainArrayLength = (int)reader.ReadStartArray();
            if (mainArrayLength != 2)
                throw new InvalidOperationException("CBOR structure is invalid");

            // accounts
            int accountCount = (int)reader.ReadStartArray();
            var accounts = new List<AccountEntry>(accountCount);
            for (int i = 0; i < accountCount; i++)
            {
                var account = new AccountEntry();
                reader.ReadStartMap();

                for (int j = 0; j < 6; j++)
                {
                    string key = reader.ReadTextString();
                    switch (key)
                    {
                        case "ServiceName":
                            account.ServiceName = reader.ReadTextString();
                            break;
                        case "Website":
                            account.Website = reader.ReadTextString();
                            break;
                        case "Login":
                            account.Login = reader.ReadTextString();
                            break;
                        case "Password":
                            account.Password = reader.ReadTextString();
                            break;
                        case "LastModified":
                            account.LastModified = DateTime.FromBinary(reader.ReadInt64());
                            break;
                        case "Tags":
                            int tagCount = (int)reader.ReadStartArray();
                            account.Tags = new List<string>(tagCount);
                            for (int k = 0; k < tagCount; k++)
                                account.Tags.Add(reader.ReadTextString());
                            reader.ReadEndArray();
                            break;
                    }
                }

                reader.ReadEndMap();
                accounts.Add(account);
            }
            reader.ReadEndArray();
            AccountEntries = new ObservableCollection<AccountEntry>(accounts);

            // settings
            reader.ReadStartMap();
            var settings = new SettingsEntry();
            for (int i = 0; i < 5; i++)
            {
                string key = reader.ReadTextString();
                switch (key)
                {
                    case "OldPasswordRemindTime":
                        settings.OldPasswordRemindTime = reader.ReadInt32();
                        break;
                    case "SessionTokenLifetime":
                        settings.SessionTokenLifetime = reader.ReadInt32();
                        break;
                    case "BackupRemindTime":
                        settings.BackupRemindTime = reader.ReadInt32();
                        break;
                    case "LastBackupDate":
                        settings.LastBackupDate = DateTime.FromBinary(reader.ReadInt64());
                        break;
                    case "PasswordHash":
                        settings.PasswordHash = reader.ReadByteString();
                        break;
                }
            }
            reader.ReadEndMap();
            SettingEntries = settings;

            reader.ReadEndArray();
        }
    }

    public partial class AccountEntry : ObservableObject
    {
        [ObservableProperty] private string _serviceName = "";
        [ObservableProperty] private string _website = "";
        [ObservableProperty] private string _login = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private DateTime _lastModified = DateTime.Now;
        [ObservableProperty] private List<string> _tags = [];

        public AccountEntry() { }
        public AccountEntry(AccountEntry entry) 
        {
            ServiceName = entry.ServiceName;
            Website = entry.Website;
            Login = entry.Login;
            Password = entry.Password;
            LastModified = entry.LastModified;
            Tags = entry.Tags;
        }

        public AccountEntry(string serviceName, string website, string login, string password, DateTime lastModified, List<string> tags)
        {
            ServiceName = serviceName;
            Website = website;
            Login = login;
            Password = password;
            LastModified = lastModified;
            Tags = tags;
        }

        public override string ToString()
        {
            return $"Service: {ServiceName}, Website: {Website}, Login: {Login}, Password: {Password}, Date: {LastModified}, " +
                    $"Tags: {GetCombineString(Tags)}";
        }
        public static string GetCombineString(List<string> Tags)
        {
            return string.Join(" ", Tags);
        }
    }

    public partial class SettingsEntry : ObservableObject
    {
        [ObservableProperty] private int _oldPasswordRemindTime; //in days
        [ObservableProperty] private int _sessionTokenLifetime; //in days
        [ObservableProperty] private int _backupRemindTime; //in days
        [ObservableProperty] private DateTime _lastBackupDate;
        [ObservableProperty] private byte[] _passwordHash;
        public SettingsEntry()
        {
            OldPasswordRemindTime = 90;
            SessionTokenLifetime = 14;
            BackupRemindTime = 30;
            LastBackupDate = DateTime.Now;
            PasswordHash = [];
        }
        public SettingsEntry(SettingsEntry entry)
        {
            OldPasswordRemindTime = entry.OldPasswordRemindTime;
            SessionTokenLifetime = entry.SessionTokenLifetime;
            BackupRemindTime = entry.BackupRemindTime;
            LastBackupDate = entry.LastBackupDate;
            PasswordHash = entry.PasswordHash;
        }
    }
}
