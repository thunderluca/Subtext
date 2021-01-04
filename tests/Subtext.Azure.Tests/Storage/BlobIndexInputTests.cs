using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subtext.Azure.Storage;
using System;

namespace Subtext.Azure.Tests.Storage
{
    [TestClass]
    public class BlobIndexInputTests
    {
        Mock<IBlob> _blobMock;

        [TestInitialize]
        public void SetUp()
        {
            _blobMock = new Mock<IBlob>();
            _blobMock.Setup(b => b.GetFileSizeInBytes()).Returns(2048);
        }

        [TestMethod]
        public void BlobIndexInput_Constructor_Should_Throw_If_Blob_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new BlobIndexInput(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("blob", exception.ParamName);
        }

        [TestMethod]
        public void BlobIndexInput_Constructor_Should_Throw_If_ChunkSize_Is_Less_Than_Zero()
        {
            var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BlobIndexInput(_blobMock.Object, -1));

            Assert.IsNotNull(exception);
            Assert.AreEqual("chunkSize", exception.ParamName);
        }

        [TestMethod]
        public void BlobIndexInput_Constructor_Should_Throw_If_ChunkSize_Equals_To_Zero()
        {
            var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BlobIndexInput(_blobMock.Object, 0));

            Assert.IsNotNull(exception);
            Assert.AreEqual("chunkSize", exception.ParamName);
        }

        [TestMethod]
        public void BlobIndexInput_Constructor_Should_Not_Throw_If_All_Parameters_Are_Valid()
        {
            var indexInput = new BlobIndexInput(_blobMock.Object, 1024);

            Assert.IsNotNull(indexInput);

            var privateObject = new PrivateObject(indexInput);

            var chuckSizeObj = privateObject.GetField("_chunkSize");
            Assert.IsNotNull(chuckSizeObj);
            Assert.IsInstanceOfType(chuckSizeObj, typeof(int));
            Assert.AreEqual(1024, (int)chuckSizeObj);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(0, (long)positionObj);
        }

        [TestMethod]
        public void BlobIndexInput_Dispose_Should_Not_Throw()
        {
            var indexInput = GetBlobIndexInput();

            indexInput.Dispose();

            var privateObject = new PrivateObject(indexInput);

            var blobObj = privateObject.GetField("_blob");
            Assert.IsNull(blobObj);
        }

        [TestMethod]
        public void BlobIndexInput_Length_Should_Not_Throw()
        {
            var indexInput = GetBlobIndexInput();

            var length = indexInput.Length();

            Assert.AreEqual(2048, length);
        }

        [TestMethod]
        public void BlobIndexInput_SeekInternal_Should_Not_Throw_If_Pos_Is_Less_Than_Zero()
        {
            var indexInput = GetBlobIndexInput();

            indexInput.SeekInternal(-1);

            var privateObject = new PrivateObject(indexInput);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(-1, (long)positionObj);
        }

        [TestMethod]
        public void BlobIndexInput_SeekInternal_Should_Not_Throw_If_Pos_Equals_To_Zero()
        {
            var indexInput = GetBlobIndexInput();

            indexInput.SeekInternal(0);

            var privateObject = new PrivateObject(indexInput);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(0, (long)positionObj);
        }

        [TestMethod]
        public void BlobIndexInput_SeekInternal_Should_Not_Throw_If_Pos_Is_Greater_Than_Zero()
        {
            var indexInput = GetBlobIndexInput();

            indexInput.SeekInternal(1);

            var privateObject = new PrivateObject(indexInput);

            var positionObj = privateObject.GetField("_position");
            Assert.IsNotNull(positionObj);
            Assert.IsInstanceOfType(positionObj, typeof(long));
            Assert.AreEqual(1, (long)positionObj);
        }

        private BlobIndexInput GetBlobIndexInput()
        {
            var indexInput = new BlobIndexInput(_blobMock.Object);

            return indexInput;
        }
    }
}
