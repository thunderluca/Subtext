using Azure.Storage.Blobs;
using log4net;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Subtext.Azure.Storage
{
    public class BlobDirectory : Directory
    {
        private readonly TimeSpan _leaseDuration;
        private readonly Dictionary<string, BlobLock> _locksDictionary;
        private readonly ILog _logger;
        private BlobContainerClient _container;

        public BlobDirectory(string connectionString, string containerName, TimeSpan leaseDuration, ILog logger = null, bool useDevelopmentStorage = false)
        {
            if (useDevelopmentStorage) //Use local Storage Emulator
                connectionString = "UseDevelopmentStorage=true";

            if (string.IsNullOrWhiteSpace(connectionString) && !useDevelopmentStorage)
                throw new ArgumentException($"{nameof(connectionString)} cannot be null, empty or blank", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException($"{nameof(containerName)} cannot be null, empty or blank", nameof(containerName));

            if (leaseDuration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(leaseDuration), $"{nameof(leaseDuration)} cannot be less or equal to {nameof(TimeSpan)}.{nameof(TimeSpan.Zero)} value");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var client = new BlobServiceClient(connectionString);

            _container = client.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists();

            _leaseDuration = leaseDuration;

            _locksDictionary = new Dictionary<string, BlobLock>();

            _logger = logger;
        }

        public override void ClearLock(string name)
        {
            if (!_locksDictionary.ContainsKey(name))
                return;

            var blobLock = _locksDictionary[name];

            blobLock.Release();

            _locksDictionary.Remove(name);
        }

        public override IndexOutput CreateOutput(string name)
        {
            var blobClient = _container.GetBlobClient(name);
            if (blobClient == null)
                throw new ArgumentNullException(nameof(blobClient));

            var blobFile = new BlobFile(blobClient);

            blobFile.DeleteIfExists();

            return new BlobIndexOutput(blobFile);
        }

        public override void DeleteFile(string name)
        {
            var blobClient = _container.GetBlobClient(name);
            if (blobClient == null)
                throw new ArgumentNullException(nameof(blobClient));

            blobClient.DeleteIfExists();
        }

        public override bool FileExists(string name)
        {
            var blobClient = _container.GetBlobClient(name);
            if (blobClient == null)
                throw new ArgumentNullException(nameof(blobClient));

            return blobClient.Exists();
        }

        public override long FileLength(string name)
        {
            var blobClient = _container.GetBlobClient(name);
            if (blobClient == null)
                throw new ArgumentNullException(nameof(blobClient));

            var blobFile = new BlobFile(blobClient);

            var indexInput = new BlobIndexInput(blobFile);
            if (indexInput == null)
                throw new ArgumentNullException(nameof(indexInput));

            return indexInput.Length();
        }

        public override long FileModified(string name)
        {
            var blobClient = _container.GetBlobClient(name);
            if (blobClient == null)
                throw new ArgumentNullException(nameof(blobClient));

            var blobFile = new BlobFile(blobClient);

            var indexInput = new BlobIndexInput(blobFile);
            if (indexInput == null)
                throw new ArgumentNullException(nameof(indexInput));

            return indexInput.GetLastModifiedDateInMilliseconds();
        }

        public override string[] ListAll()
        {
            var blobList = _container.GetBlobs();
            if (blobList == null || blobList.Count() == 0)
                return new string[0];

            return blobList.Select(b => b.Name).ToArray();
        }

        public override Lock MakeLock(string name)
        {
            if (_locksDictionary.ContainsKey(name))
                return _locksDictionary[name];

            var blobClient = _container.GetBlobClient(name);
            if (blobClient == null)
                throw new ArgumentNullException(nameof(blobClient));

            var blobFile = new BlobFile(blobClient);

            var blobLock = new BlobLock(blobFile, _leaseDuration, _logger);

            _locksDictionary[name] = blobLock;

            return blobLock;
        }

        public override IndexInput OpenInput(string name)
        {
            var blobClient = _container.GetBlobClient(name);
            if (blobClient == null)
                throw new ArgumentNullException(nameof(blobClient));

            var blobFile = new BlobFile(blobClient);

            return new BlobIndexInput(blobFile);
        }

        public override void TouchFile(string name)
        {
            //Azure blob last modified date is always updated on each change made
        }

        protected override void Dispose(bool disposing)
        {
            _container = null;
        }
    }
}
