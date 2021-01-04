using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.IO;

namespace Subtext.Azure.Storage
{
    public interface IBlob
    {
        void CreateIfNotExists();

        void DeleteIfExists();

        void DownloadTo(Stream stream);

        bool Exists();

        BlobLeaseClient GetBlobLeaseClient(string leaseId);

        long GetFileSizeInBytes();

        long GetLastModifiedDateInMilliseconds();

        LeaseState GetLeaseState();

        string Name { get; }

        Stream OpenRead(long position);

        void Upload(Stream stream, bool overwrite);
    }
}
