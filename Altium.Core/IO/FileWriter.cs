﻿using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Altium.Core
{
    public class FileWriter : IDisposable
    {
        const int _fileBufferSize = 1_000_000;
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

        public void WriteRandomRows(int count, ILogger logger)
        {
            var random = new Random(new Guid().GetHashCode());

            for (int i = 0; i < count; i++)
            {
                WriteRow(
                    random.Next(_maxNumber),
                    _alphabet.RandomString(random, _minStringSize, _maxStringSize));

                if (i % 1000000 == 0)
                    logger.Information("Wrote {count} random lines", i);
            }
        }

        public void WriteRows(List<RowDto> rows)
        {
            foreach (var t in rows)
                _writer.WriteLine(t.OriginLine);
        }

        private void WriteRow(int number, string stringValue)
        {
            _writer.Write(number.ToString());
            _writer.Write(". ");
            _writer.Write(stringValue);
            _writer.WriteLine();
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