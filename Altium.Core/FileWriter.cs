using System;
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

        private readonly int _count;

        public FileWriter(int count)
        {
            _count = count;
        }

        public async Task CreateRandomFileAsync(string fileName)
        {
            await using var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write);
            await using var writer = new StreamWriter(stream, Encoding.UTF8, _fileBufferSize);

            var random = new Random();

            for (int i = 0; i < _count; i++)
            {
                await writer.WriteAsync(random.Next(_maxNumber).ToString());
                await writer.WriteAsync(". ");
                await writer.WriteAsync(RandomString(random, _alphabet, _alphabetLen));
                await writer.WriteLineAsync();
            }
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