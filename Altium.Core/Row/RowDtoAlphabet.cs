using System;
using System.Collections.Generic;
using System.Linq;

namespace Altium.Core
{
    public class RowDtoAlphabet
    {
        private const string _alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private Dictionary<char, int> _alphabetIndexes = new();

        private const int _weightAnalysisLevel = 4;

        const int _minStringSize = 3;
        const int _maxStringSize = 25;

        public RowDtoAlphabet()
        {
            int index = 0;
            foreach (var t in _alphabet)
                _alphabetIndexes[t] = index++;
        }

        public string RandomString(Random random)
        {
            var lenght = random.Next(_minStringSize, _maxStringSize);

            var symbols = Enumerable.Repeat("", lenght)
                .Select(_ => _alphabet[random.Next() % _alphabet.Length]);

            return string.Concat(symbols);
        }

        public long StringValueWeight(ReadOnlyMemory<char> stringValue)
        {
            long sum = 0;

            var stringValueSpan = stringValue.Span;
            for (int i = 0; i < _weightAnalysisLevel; i++)
            {
                sum *= _alphabet.Length;

                if (stringValue.Length > i && _alphabetIndexes.TryGetValue(stringValueSpan[i], out var index))
                    sum += index;
            }

            return sum;
        }
    }
}