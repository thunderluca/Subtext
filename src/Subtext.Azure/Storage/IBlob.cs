using System;
using System.IO;

namespace Subtext.Azure.Storage
{
    public interface IBlob
    {
        void CreateIfNotExists();

        void DeleteIfExists();

        void DownloadTo(Stream stream);

        bool Exists();

        long GetFileSizeInBytes();

        long GetLastModifiedDateInMilliseconds();

        string Name { get; }

        string ObtainLock(TimeSpan duration, string previousLeaseId);

        Stream OpenRead(long position);

        void ReleaseLock(string leaseId);

        void Upload(Stream stream, bool overwrite);
    }
}
