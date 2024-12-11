using System;
using System.Collections.Generic;
using System.Linq;

namespace Altium.Core
{
    public class RowDtoAlphabet
    {
        private const string _alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private Dictionary<char, int> _alphabetIndexes = new();

        private const int _weightAnalysisLevel = 4;

        public RowDtoAlphabet()
        {
            int index = 0;
            foreach (var t in _alphabet)
                _alphabetIndexes[t] = index++;

        }

        public string RandomString(Random random, int minStringSize, int maxStringSize)
        {
            var symbols = Enumerable
                .Repeat("", random.Next(minStringSize, maxStringSize))
                .Select(_ => _alphabet[random.Next(_alphabet.Length)]);

            return string.Concat(symbols);
        }

        public long? StringValueWeight(string stringValue)
        {
            long sum = 0;

            for (int i = 0; i < _weightAnalysisLevel; i++)
            {
                sum *= _alphabet.Length;

                if (stringValue.Length > i && _alphabetIndexes.TryGetValue(stringValue[i], out var index))
                    sum += index;
            }

            return sum;
        }
    }
}