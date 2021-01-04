using Lucene.Net.Store;
using System;
using System.IO;

namespace Subtext.Azure.Storage
{
    public class BlobIndexOutput : BufferedIndexOutput
    {
        private readonly IBlob _blob;
        private long _position;

        public BlobIndexOutput(IBlob blob)
        {
            _blob = blob ?? throw new ArgumentNullException(nameof(blob));
            _blob.CreateIfNotExists();

            _position = 0;
        }

        public override long Length
        {
            get
            {
                return _blob.GetFileSizeInBytes();
            }
        }

        public override void FlushBuffer(byte[] b, int offset, int len)
        {
            using (var stream = this.GetBlobStream(_position))
            {
                stream.Write(b, offset, len);
                stream.Flush();

                this.UploadData(stream, overwrite: true);
            }
        }

        public override void Seek(long pos)
        {
            base.Seek(pos);
            _position = pos;
        }

        private Stream GetBlobStream(long position)
        {
            var stream = new MemoryStream();

            _blob.DownloadTo(stream);

            stream.Seek(position, SeekOrigin.Begin);

            return stream;
        }

        private void UploadData(Stream stream, bool overwrite)
        {
            stream.Seek(0, SeekOrigin.Begin);

            _blob.Upload(stream, overwrite);
        }
    }
}
