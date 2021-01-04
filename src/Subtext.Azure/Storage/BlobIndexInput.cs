using Lucene.Net.Store;
using System;

namespace Subtext.Azure.Storage
{
    public class BlobIndexInput : BufferedIndexInput
    {
        private readonly int _chunkSize;
        private IBlob _blob;
        private long _position;

        public BlobIndexInput(IBlob blob, int chunkSize = 4096)
        {
            _blob = blob ?? throw new ArgumentNullException(nameof(blob));

            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), $"{nameof(chunkSize)} cannot be less or equal to zero");

            _blob.CreateIfNotExists();
            _chunkSize = chunkSize;
            _position = 0;
        }

        protected override void Dispose(bool disposing)
        {
            _blob = null;
        }

        public override long Length()
        {
            return _blob.GetFileSizeInBytes();
        }

        public override void ReadInternal(byte[] buffer, int offset, int bufferLength)
        {
            int length = 0, byteCount, readerPosition;

            var position = this.FilePointer;

            if (_position != position)
            {
                _position = position;
            }

            try
            {
                var stream = new System.IO.MemoryStream();

                _blob.DownloadTo(stream);

                do
                {
                    byteCount = _chunkSize + length > bufferLength ? bufferLength - length : _chunkSize;

                    stream.Seek(position, System.IO.SeekOrigin.Begin);

                    readerPosition = stream.Read(buffer, length + offset, byteCount);

                    if (readerPosition <= -1)
                        throw new ArgumentOutOfRangeException(nameof(readerPosition), "Reader position went after end of stream");

                    position += readerPosition;
                    length += readerPosition;
                }
                while (bufferLength > length);
            }
            finally
            {
                //Code or nothing
            }
        }

        public override void SeekInternal(long pos)
        {
            _position = pos;
        }
    }
}
