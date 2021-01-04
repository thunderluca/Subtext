using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.IO;

namespace Subtext.Azure.Storage
{
    public class BlobFile : IBlob
    {
        private readonly BlobClient _client;

        public string Name
        {
            get
            {
                return _client.Name;
            }
        }

        public BlobFile(BlobClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public void CreateIfNotExists()
        {
            if (_client.Exists())
                return;

            _client.Upload(new MemoryStream());
        }

        public void DeleteIfExists()
        {
            _client.DeleteIfExists();
        }

        public void DownloadTo(Stream stream)
        {
            _client.DownloadTo(stream);
        }

        public bool Exists()
        {
            return _client.Exists();
        }

        public BlobLeaseClient GetBlobLeaseClient(string leaseId)
        {
            return _client.GetBlobLeaseClient(leaseId);
        }

        public long GetFileSizeInBytes()
        {
            if (!this.TryGetProperties(out BlobProperties properties))
                throw new ArgumentException($"Cannot retrieve properties from blob with name '{_client.Name}'", nameof(properties));

            return properties.ContentLength;
        }

        public long GetLastModifiedDateInMilliseconds()
        {
            if (!_client.Exists())
                return DateTimeOffset.MinValue.ToUnixTimeMilliseconds();

            if (!TryGetProperties(out BlobProperties properties))
                throw new ArgumentException($"Cannot retrieve properties from blob with name '{_client.Name}'", nameof(properties));

            return properties.LastModified.ToUnixTimeMilliseconds();
        }

        public LeaseState GetLeaseState()
        {
            if (!this.TryGetProperties(out BlobProperties properties))
                throw new ArgumentException($"Cannot retrieve properties from blob with name '{_client.Name}'", nameof(properties));

            return properties.LeaseState;
        }

        public Stream OpenRead(long position)
        {
            return _client.OpenRead(position);
        }

        public void Upload(Stream stream, bool overwrite)
        {
            _client.Upload(stream, overwrite);
        }

        private bool TryGetProperties(out BlobProperties properties)
        {
            properties = null;

            var blobProperties = _client.GetProperties();
            if (blobProperties == null || blobProperties.Value == null)
                return false;

            properties = blobProperties.Value;
            return true;
        }
    }
}
