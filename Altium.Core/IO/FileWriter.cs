using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Altium.Core
{
    public class FileWriter : IDisposable
    {
        const int _fileBufferSize = 10_000_000;
        const int _maxNumber = 9_999;

        const int _minStringSize = 3;
        const int _maxStringSize = 25;

        private StreamWriter _writer;
        private RowDtoAlphabet _alphabet = new();

        public FileWriter(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write);

            //writer will close the 'stream' implicitly
            _writer = new StreamWriter(stream, Encoding.UTF8, _fileBufferSize);
        }

        public async Task WriteRandomRowsAsync(int count)
        {
            var random = new Random(new Guid().GetHashCode());

            for (int i = 0; i < count; i++)
            {
                await WriteRowAsync(
                    random.Next(_maxNumber),
                    _alphabet.RandomString(random, _minStringSize, _maxStringSize));
            }
        }

        public async Task WriteRowsAsync(List<RowDto> rows)
        {
            foreach (var t in rows)
                _writer.WriteLine(t.OriginLine);
        }

        private async Task WriteRowAsync(int number, string stringValue)
        {
            await _writer.WriteAsync(number.ToString());
            await _writer.WriteAsync(". ");
            await _writer.WriteAsync(stringValue);
            await _writer.WriteLineAsync();
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