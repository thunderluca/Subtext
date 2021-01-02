using Azure.Storage.Blobs.Models;
using log4net;
using Lucene.Net.Store;
using System;
using RequestFailedException = Azure.RequestFailedException;

namespace Subtext.Azure.Storage
{
    public class BlobLock : Lock
    {
        private readonly IBlob _blob;
        private readonly TimeSpan _duration;
        private readonly ILog _logger;

        public BlobLock(IBlob blob, TimeSpan duration, ILog logger = null)
        {
            _blob = blob ?? throw new ArgumentNullException(nameof(blob));

            _blob.CreateIfNotExists();

            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration), $"{nameof(duration)} cannot be less or equal to {nameof(TimeSpan)}.{nameof(TimeSpan.Zero)} value");

            _duration = duration;

            _logger = logger;
        }

        public string LeaseId { get; private set; }

        public override bool IsLocked()
        {
            return !string.IsNullOrWhiteSpace(this.LeaseId);
        }

        public override bool Obtain()
        {
            try
            {
                var result = _blob.TryGetProperties(out BlobProperties properties);
                if (!result)
                    throw new ArgumentException($"Cannot retrieve properties from blob with name '{_blob.Name}'", nameof(properties));

                switch (properties.LeaseState)
                {
                    case LeaseState.Leased:
                        _logger.Error($"Requested release operation on unavailable blob ({_blob.Name}, state: {properties.LeaseState})");
                        return false;
                    case LeaseState.Breaking:
                    case LeaseState.Broken:
                        this.Release();
                        break;
                }

                var client = _blob.GetBlobLeaseClient(this.LeaseId);

                global::Azure.Response<BlobLease> leaseResponse;

                if (!string.IsNullOrWhiteSpace(this.LeaseId) && properties.LeaseState == LeaseState.Expired)
                {
                    leaseResponse = client.Renew();
                }
                else
                {
                    leaseResponse = client.Acquire(_duration);
                }

                this.LeaseId = leaseResponse?.Value?.LeaseId;

                return this.IsLocked();
            }
            catch (RequestFailedException ex)
            {
                _logger?.Error("An Azure SDK error occurred while acquiring blob's lease: " + ex.Message, ex);

                return false;
            }
            catch (Exception ex)
            {
                _logger?.Error("A generic error occurred while acquiring blob's lease: " + ex.Message, ex);

                return false;
            }
        }

        public override void Release()
        {
            try
            {
                var result = _blob.TryGetProperties(out BlobProperties properties);
                if (!result)
                    throw new ArgumentException($"Cannot retrieve properties from blob with name '{_blob.Name}'", nameof(properties));

                if (properties.LeaseState == LeaseState.Available)
                {
                    _logger.Warn($"Requested release operation on available blob ({_blob.Name}), skipping it");
                    return;
                }

                var client = _blob.GetBlobLeaseClient(this.LeaseId);

                client.Release();
            }
            catch (RequestFailedException ex)
            {
                _logger?.Error("An Azure SDK error occurred while releasing blob's lease: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger?.Error("A generic error occurred while releasing blob's lease: " + ex.Message, ex);
            }
        }
    }
}
