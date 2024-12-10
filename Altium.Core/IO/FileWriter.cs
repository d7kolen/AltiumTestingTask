using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altium.Core
{
    public class FileWriter
    {
        const int _fileBufferSize = 1_000_000;
        const int _maxNumber = 9_999;

        const int _minStringSize = 3;
        const int _maxStringSize = 25;

        private readonly string _fileName;

        public FileWriter(string fileName)
        {
            _fileName = fileName;
        }

        public async Task CreateRandomFileAsync(int count)
        {
            await using var stream = new FileStream(_fileName, FileMode.CreateNew, FileAccess.Write);
            await using var writer = new StreamWriter(stream, Encoding.UTF8, _fileBufferSize);

            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                await WriteRowAsync(
                    writer,
                    random.Next(_maxNumber),
                    RandomString(random, _alphabet, _alphabetLen));
            }
        }

        public async Task CreateFileAsync(List<RowDto> rows)
        {
            await using var stream = new FileStream(_fileName, FileMode.CreateNew, FileAccess.Write);
            await using var writer = new StreamWriter(stream, Encoding.UTF8, _fileBufferSize);

            foreach (var t in rows)
                await WriteRowAsync(writer, t.Number, t.StringValue);
        }

        private async Task WriteRowAsync(StreamWriter writer, int number, string stringValue)
        {
            await writer.WriteAsync(number.ToString());
            await writer.WriteAsync(". ");
            await writer.WriteAsync(stringValue);
            await writer.WriteLineAsync();
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
    }
}