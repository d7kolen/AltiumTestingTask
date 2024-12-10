using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altium.Core
{
    public class FileWriter : IDisposable
    {
        const int _fileBufferSize = 1_000_000;
        const int _maxNumber = 9_999;

        const int _minStringSize = 3;
        const int _maxStringSize = 25;

        private StreamWriter _writer;

        public FileWriter(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write);

            //writer will close the 'stream' implicitly
            _writer = new StreamWriter(stream, Encoding.UTF8, _fileBufferSize);
        }

        public async Task WriteRandomRowsAsync(int count)
        {
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                await WriteRowAsync(
                    random.Next(_maxNumber),
                    RandomString(random, _alphabet, _alphabetLen));
            }
        }

        public async Task WriteRowsAsync(List<RowDto> rows)
        {
            foreach (var t in rows)
                await WriteRowAsync(t.Number, t.StringValue);
        }

        private async Task WriteRowAsync(int number, string stringValue)
        {
            await _writer.WriteAsync(number.ToString());
            await _writer.WriteAsync(". ");
            await _writer.WriteAsync(stringValue);
            await _writer.WriteLineAsync();
        }

        private const string _alphabet = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int _alphabetLen = _alphabet.Length;

        private static string RandomString(Random random, string alphabet, int alphabetLen)
        {
            var symbols = Enumerable
                .Repeat("", random.Next(_minStringSize, _maxStringSize))
                .Select(_ => alphabet[random.Next(alphabetLen)]);

            return string.Concat(symbols);
        }

        public void Dispose()
        {
            if (_writer == null)
                return;

            _writer.Dispose();
            _writer = null;
        }
    }
}