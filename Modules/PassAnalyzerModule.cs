using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Specialized;

namespace Passomnemo.Modules
{
    public class PassAnalyzerModule
    {
        public static PasswordSecurityReport AnalyzePasswords(ObservableCollection<AccountEntry> passwords)
        {
            var weakPasswords = GetWeakPasswords(passwords);
            var duplicatePasswords = GetDuplicatePasswords(passwords);
            var expiredPasswords = GetExpiredPasswords(passwords);

            return new PasswordSecurityReport(weakPasswords, duplicatePasswords, expiredPasswords);
        }
        public static double CalculateEntropy(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;
            var uniqueChars = password.Distinct().Count();
            return password.Length * Math.Log2(uniqueChars);
        }

        public static string EvaluateStrength(string password)
        {
            double entropy = CalculateEntropy(password);
            return entropy switch
            {
                < 28 => "Дуже слабкий",
                < 36 => "Слабкий",
                < 60 => "Середній",
                < 100 => "Сильний",
                _ => "Дуже сильний"
            };
        }

        public static ObservableCollection<AccountEntry> GetWeakPasswords(ObservableCollection<AccountEntry> accounts)
        {
            return new ObservableCollection<AccountEntry>(accounts
                .Where(p => EvaluateStrength(p.Password) != "Сильний"
                && EvaluateStrength(p.Password) != "Дуже сильний")
                .ToList());
        }

        public static ObservableCollection<AccountEntry> GetDuplicatePasswords(ObservableCollection<AccountEntry> accounts)
        {
            return new ObservableCollection<AccountEntry>(accounts
                .GroupBy(e => e.Password)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToList());
        }

        public static ObservableCollection<AccountEntry> GetExpiredPasswords(ObservableCollection<AccountEntry> accounts)
        {
            return new ObservableCollection<AccountEntry>(accounts.Where(x =>
            DateTime.Now - x.LastModified > TimeSpan.FromDays(StorageModule.SettingEntries.OldPasswordRemindTime)).ToList());
        }
    }

    public partial class PasswordSecurityReport : ObservableObject
    {
        [ObservableProperty]
        private int _issueCount;
        public ObservableCollection<AccountEntry> WeakPasswords { get; }
        public ObservableCollection<AccountEntry> DuplicatePasswords { get; }
        public ObservableCollection<AccountEntry> ExpiredPasswords { get; }

        public PasswordSecurityReport(ObservableCollection<AccountEntry> weak, 
            ObservableCollection<AccountEntry> duplicates, 
            ObservableCollection<AccountEntry> expired)
        {
            WeakPasswords = weak;
            DuplicatePasswords = duplicates;
            ExpiredPasswords = expired;

            WeakPasswords.CollectionChanged += OnCollectionChanged;
            DuplicatePasswords.CollectionChanged += OnCollectionChanged;
            ExpiredPasswords.CollectionChanged += OnCollectionChanged;
            UpdateIssueCount();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateIssueCount();
        }

        private void UpdateIssueCount()
        {
            IssueCount = WeakPasswords.Count + DuplicatePasswords.Count + ExpiredPasswords.Count;
        }
    }
}
