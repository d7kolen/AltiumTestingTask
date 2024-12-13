using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Altium.Core;

public class SwappingBufferReadingStream : Stream
{
    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _inner.Length;

    public override long Position
    {
        get => 0;
        set => throw new NotSupportedException();
    }

    private readonly Stream _inner;

    private byte[] _bufferData;
    private int _bufferLength;
    private int _bufferPosition;

    private Channel<(byte[] Data, int Length)> _channel;
    private readonly CancellationTokenSource _stopDataReading = new();
    private Task? _dataReader = null;

    public SwappingBufferReadingStream(Stream inner, int bufferSize)
    {
        _inner = inner;

        if (bufferSize < 2)
            bufferSize = 2;

        InitChannel(bufferSize);
    }

    private void InitChannel(int bufferSize)
    {
        _channel = Channel.CreateBounded<(byte[] Data, int Length)>(1);

        var buffers = new Queue<byte[]>();
        for (int i = 0; i < 3; i++)
            buffers.Enqueue(new byte[bufferSize]);

        var cancel = _stopDataReading.Token;

        _dataReader = Task.Run(async () =>
        {
            while (!_stopDataReading.IsCancellationRequested)
            {
                var tBuffer = buffers.Dequeue();

                var realLength = await _inner.ReadAsync(tBuffer, 0, tBuffer.Length, cancel);
                if (realLength == 0)
                    break;

                await _channel.Writer.WriteAsync((Data: tBuffer, Length: realLength), cancel);
                buffers.Enqueue(tBuffer);
            }

            _channel.Writer.Complete();
        }, cancel);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int alreadyRead = 0;

        while (alreadyRead < count)
        {
            if (_bufferPosition == _bufferLength)
            {
                if (!await _channel.Reader.WaitToReadAsync())
                    return alreadyRead;

                var readData = await _channel.Reader.ReadAsync();
                _bufferData = readData.Data;
                _bufferLength = readData.Length;
                _bufferPosition = 0;
            }

            var readingLength = Math.Min(_bufferLength - _bufferPosition, count - alreadyRead);
            Array.Copy(_bufferData, _bufferPosition, buffer, alreadyRead, readingLength);
            _bufferPosition += readingLength;

            alreadyRead += readingLength;
        }

        return alreadyRead;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    #region NotSupport

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    #endregion

    public override async ValueTask DisposeAsync()
    {
        if (_dataReader != null)
        {
            _stopDataReading.Cancel();
            try
            {
                await _dataReader;
            }
            catch (OperationCanceledException) { }

            _dataReader = null;
        }

        await base.DisposeAsync();
    }
}