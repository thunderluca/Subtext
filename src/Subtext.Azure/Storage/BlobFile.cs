using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using log4net;
using System;
using System.IO;

namespace Subtext.Azure.Storage
{
    public class BlobFile : IBlob
    {
        private readonly BlobClient _client;
        private readonly ILog _logger;

        public string Name
        {
            get
            {
                return _client.Name;
            }
        }

        public BlobFile(BlobClient client, ILog logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            _logger = logger;
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

        public long GetFileSizeInBytes()
        {
            if (!_client.Exists())
                return 0;

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

        public string ObtainLock(TimeSpan duration, string previousLeaseId)
        {
            var leaseState = this.GetLeaseState();

            switch (leaseState)
            {
                case LeaseState.Leased:
                    throw new InvalidOperationException($"Requested release operation on unavailable blob ({_client.Name}, state: {leaseState})");
                case LeaseState.Breaking:
                case LeaseState.Broken:
                    this.ReleaseLock(previousLeaseId);
                    break;
            }

            var client = _client.GetBlobLeaseClient(previousLeaseId);

            global::Azure.Response<BlobLease> leaseResponse;

            if (!string.IsNullOrWhiteSpace(previousLeaseId) && leaseState == LeaseState.Expired)
            {
                leaseResponse = client.Renew();
            }
            else
            {
                leaseResponse = client.Acquire(duration);
            }

            return leaseResponse?.Value?.LeaseId;
        }

        public Stream OpenRead(long position)
        {
            return _client.OpenRead(position);
        }

        public void ReleaseLock(string leaseId)
        {
            var leaseState = this.GetLeaseState();

            if (leaseState == LeaseState.Available)
            {
                _logger?.Warn($"Requested release operation on available blob ({_client.Name}), skipping it");
                return;
            }

            var client = _client.GetBlobLeaseClient(leaseId);

            client.Release();
        }

        public void Upload(Stream stream, bool overwrite)
        {
            _client.Upload(stream, overwrite);
        }

        private LeaseState GetLeaseState()
        {
            if (!_client.Exists())
                throw new InvalidOperationException($"Blob with name '{_client.Name}' doesn't exist, cannot retrieve lease state");

            if (!this.TryGetProperties(out BlobProperties properties))
                throw new ArgumentException($"Cannot retrieve properties from blob with name '{_client.Name}'", nameof(properties));

            return properties.LeaseState;
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
