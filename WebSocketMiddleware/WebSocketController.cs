using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketMiddleware
{
    public class WebSocketController
    {
        public ConcurrentDictionary<Guid, WebSocketClient> Clients { get; } = new ConcurrentDictionary<Guid, WebSocketClient>();

        public virtual Task OnBinaryMessage(Stream stream)
        {
            return Task.CompletedTask;
        }
    }

    internal class WebSocketBinaryStream : Stream
    {
        private WebSocketClient Client { get; }
        private ArraySegment<byte>? InitialSegment { get; set; }
        internal WebSocketReceiveResult Result { get; private set; }

        public override bool CanRead => true;

        internal WebSocketBinaryStream(WebSocketClient client, ArraySegment<byte> initialSegment, WebSocketReceiveResult initialResult)
        {
            Client = client;
            InitialSegment = initialSegment;
            Result = initialResult;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var segment = new ArraySegment<byte>(buffer, offset, count);
            if (InitialSegment.HasValue)
            {
                if (InitialSegment.Value.Count <= count)
                {
                    InitialSegment.Value.CopyTo(segment);
                    int read = InitialSegment.Value.Count;
                    InitialSegment = null;
                    return read;
                }
                else
                {
                    var partialInitialSegment = new ArraySegment<byte>(InitialSegment.Value.Array, InitialSegment.Value.Offset, count);
                    partialInitialSegment.CopyTo(segment);
                    InitialSegment = new ArraySegment<byte>(InitialSegment.Value.Array, InitialSegment.Value.Offset + count, InitialSegment.Value.Count - count);
                    return count;
                }
            }

            if (Result.CloseStatus.HasValue || Result.EndOfMessage)
                return 0;

            Result = Client.Socket.ReceiveAsync(segment, CancellationToken.None).Result;
            return Result.Count;
        }

        #region NotSupported
        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

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
    }
}
