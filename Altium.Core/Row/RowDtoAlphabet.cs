using System;
using System.Collections.Generic;
using System.Linq;

namespace Altium.Core
{
    public class RowDtoAlphabet
    {
        private const string _alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private Dictionary<char, int> _alphabetIndexes = new();

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

            foreach (var t in stringValue)
            {
                var previousSum = sum;

                sum *= _alphabet.Length;

                if (!_alphabetIndexes.TryGetValue(t, out var index))
                    return null;

                sum += index;

                if (sum < previousSum)
                    return previousSum;
            }

            return sum;
        }
    }
}