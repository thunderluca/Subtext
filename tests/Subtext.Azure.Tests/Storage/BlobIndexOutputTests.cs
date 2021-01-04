using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subtext.Azure.Storage;
using System;
using System.IO;

namespace Subtext.Azure.Tests.Storage
{
    [TestClass]
    public class BlobIndexOutputTests
    {
        Mock<IBlob> _blobMock;

        [TestInitialize]
        public void SetUp()
        {
            _blobMock = new Mock<IBlob>();
            _blobMock.Setup(b => b.GetFileSizeInBytes()).Returns(3072);
        }

        [TestMethod]
        public void BlobIndexOutput_Constructor_Should_Throw_If_Blob_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new BlobIndexOutput(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("blob", exception.ParamName);
        }

        [TestMethod]
        public void BlobIndexOutput_Constructor_Should_Not_Throw_If_All_Parameters_Are_Valid()
        {
            var indexOutput = new BlobIndexOutput(_blobMock.Object);

            Assert.IsNotNull(indexOutput);

            var privateObject = new PrivateObject(indexOutput);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(0, (long)positionObj);
        }

        [TestMethod]
        public void BlobIndexOutput_Length_Should_Not_Throw()
        {
            var indexOutput = GetBlobIndexOutput();

            Assert.AreEqual(3072, indexOutput.Length);
        }

        [TestMethod]
        public void BlobIndexOutput_FlushBuffer_Should_Not_Throw()
        {
            Stream uploadStreamClone = null;
            bool? overwriteFlag = null;

            _blobMock.Setup(b => b.Upload(It.IsAny<Stream>(), It.IsAny<bool>()))
                     .Callback<Stream, bool>((str, b) =>
                     {
                         uploadStreamClone = new MemoryStream();
                         str.CopyTo(uploadStreamClone);
                         overwriteFlag = b;
                     });

            var indexOutput = GetBlobIndexOutput();

            indexOutput.FlushBuffer(new byte[] { 0xA, 0xB, 0xC }, 0, 3);

            Assert.IsNotNull(uploadStreamClone);
            Assert.AreEqual(3, uploadStreamClone.Length);
            Assert.IsInstanceOfType(uploadStreamClone, typeof(MemoryStream));
            
            var bytes = (uploadStreamClone as MemoryStream).ToArray();
            Assert.IsNotNull(bytes);
            Assert.AreEqual(3, bytes.Length);
            Assert.AreEqual((byte)0xA, bytes[0]);
            Assert.AreEqual((byte)0xB, bytes[1]);
            Assert.AreEqual((byte)0xC, bytes[2]);

            Assert.IsNotNull(overwriteFlag);
            Assert.IsTrue(overwriteFlag.Value);
        }

        [TestMethod]
        public void BlobIndexOutput_Seek_Should_Not_Throw_If_Pos_Is_Less_Than_Zero()
        {
            var indexOutput = GetBlobIndexOutput();

            indexOutput.Seek(-1);

            var privateObject = new PrivateObject(indexOutput);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(-1, (long)positionObj);
        }

        [TestMethod]
        public void BlobIndexOutput_Seek_Should_Not_Throw_If_Pos_Equals_To_Zero()
        {
            var indexOutput = GetBlobIndexOutput();

            indexOutput.Seek(0);

            var privateObject = new PrivateObject(indexOutput);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(0, (long)positionObj);
        }

        [TestMethod]
        public void BlobIndexOutput_Seek_Should_Not_Throw_If_Pos_Is_Greater_Than_Zero()
        {
            var indexOutput = GetBlobIndexOutput();

            indexOutput.Seek(1);

            var privateObject = new PrivateObject(indexOutput);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(1, (long)positionObj);
        }

        private BlobIndexOutput GetBlobIndexOutput()
        {
            var indexOutput = new BlobIndexOutput(_blobMock.Object);

            return indexOutput;
        }
    }
}
