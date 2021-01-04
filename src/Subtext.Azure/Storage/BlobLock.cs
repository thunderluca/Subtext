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

        public BlobLock(IBlob blob, TimeSpan duration, ILog logger)
        {
            _blob = blob ?? throw new ArgumentNullException(nameof(blob));

            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration), $"{nameof(duration)} cannot be less or equal to {nameof(TimeSpan)}.{nameof(TimeSpan.Zero)} value");

            _blob.CreateIfNotExists();

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
                this.LeaseId = _blob.ObtainLock(_duration, this.LeaseId);

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
                _blob.ReleaseLock(this.LeaseId);
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
