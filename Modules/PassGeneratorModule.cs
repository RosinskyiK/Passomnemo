using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Passomnemo.Modules
{
    public class PassGeneratorModule
    {
        public const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        private static Dictionary<string, string> Wordlist { get; } = new();
        static PassGeneratorModule()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"Passomnemo.Assets.diceware.txt";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException("Embedded Diceware dictionary not found.");

            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;

                var parts = line.Split('\t');
                if (parts.Length == 2)
                    Wordlist[parts[0]] = parts[1];
            }
        }

        public static string GenerateSequential(int length)
        {
            return new string(Enumerable.Range(0, length)
                .Select(_ => RandomProvider.GetRandomElement(alphabet))
                .ToArray());
        }

        public static string GenerateDiceware(int wordsCount)
        {
            return string.Join("", Enumerable.Range(0, wordsCount)
                .Select(_ => CapitalizeFirstLetter(GetWordByKey(RandomProvider.RollDice(5)))));
        }

        public static string GetWordByKey(string key)
        {
            return Wordlist.TryGetValue(key, out var word) ? word : "???";
        }

        private static string CapitalizeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word)) return word;
            return char.ToUpper(word[0]) + word.Substring(1);
        }
    }

    public class RandomProvider
    {
        public static int GetRandomInt(int min, int max) => RandomNumberGenerator.GetInt32(min, max + 1);

        public static char GetRandomElement(string source) => source[GetRandomInt(0, source.Length - 1)];

        public static string RollDice(int rolls)
        {
            return string.Concat(Enumerable.Range(0, rolls).
                Select(_ => GetRandomInt(1, 6)
                .ToString()));
        }
    }
}
