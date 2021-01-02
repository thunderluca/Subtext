using Azure.Storage.Blobs.Models;
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
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), $"{nameof(chunkSize)} cannot be less or equal to zero");

            _blob = blob ?? throw new ArgumentNullException(nameof(blob));
            _blob.CreateIfNotExists();
            _chunkSize = chunkSize;
            _position = 0;
        }

        public override long Length()
        {
            var result = _blob.TryGetProperties(out BlobProperties properties);
            if (!result)
                throw new ArgumentException($"Cannot retrieve properties from blob with name '{_blob.Name}'", nameof(properties));

            return properties.ContentLength;
        }

        protected override void Dispose(bool disposing)
        {
            _blob = null;
        }

        public override void ReadInternal(byte[] buffer, int offset, int bufferLength)
        {
            System.IO.Stream stream;
            int length = 0, byteCount, readerPosition;

            var position = this.FilePointer;

            if (_position != position)
            {
                _position = position;
            }

            try
            {
                do
                {
                    byteCount = _chunkSize + length > bufferLength ? bufferLength - length : _chunkSize;

                    using (stream = _blob.OpenRead(position))
                    {
                        readerPosition = stream.Read(buffer, length + offset, byteCount);
                    }

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

        public long GetLastModifiedDateInMilliseconds()
        {
            if (!_blob.Exists())
                return DateTimeOffset.MinValue.ToUnixTimeMilliseconds();

            var result = _blob.TryGetProperties(out BlobProperties properties);
            if (!result)
                throw new ArgumentException($"Cannot retrieve properties from blob with name '{_blob.Name}'", nameof(properties));

            return properties.LastModified.ToUnixTimeMilliseconds();
        }
    }
}
