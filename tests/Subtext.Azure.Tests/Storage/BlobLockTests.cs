using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subtext.Azure.Storage;
using System;

namespace Subtext.Azure.Tests.Storage
{
    [TestClass]
    public class BlobLockTests
    {
        Mock<IBlob> _blobMock;
        Mock<ILog> _loggerMock;

        [TestInitialize]
        public void SetUp()
        {
            _blobMock = new Mock<IBlob>();
            _loggerMock = new Mock<ILog>();
        }

        [TestMethod]
        public void BlobLock_Constructor_Should_Throw_If_Blob_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new BlobLock(null, TimeSpan.Zero, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("blob", exception.ParamName);
        }

        [TestMethod]
        public void BlobLock_Constructor_Should_Throw_If_Duration_Is_Less_Than_Zero()
        {
            var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BlobLock(_blobMock.Object, TimeSpan.FromMilliseconds(1).Negate(), null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("duration", exception.ParamName);
        }

        [TestMethod]
        public void BlobLock_Constructor_Should_Throw_If_Duration_Equals_To_Zero()
        {
            var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BlobLock(_blobMock.Object, TimeSpan.Zero, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("duration", exception.ParamName);
        }

        [TestMethod]
        public void BlobLock_Constructor_Should_Not_Throw_If_Logger_Is_Null()
        {
            var blobLock = new BlobLock(_blobMock.Object, TimeSpan.FromMilliseconds(1), null);

            Assert.IsNotNull(blobLock);
            Assert.IsNull(blobLock.LeaseId);
        }

        [TestMethod]
        public void BlobLock_Constructor_Should_Not_Throw_If_Logger_Is_Not_Null()
        {
            var blobLock = new BlobLock(_blobMock.Object, TimeSpan.FromMilliseconds(1), _loggerMock.Object);

            Assert.IsNotNull(blobLock);
            Assert.IsNull(blobLock.LeaseId);
        }

        [TestMethod]
        public void BlobLock_IsLocked_Should_Return_False_If_LeaseId_Is_Null()
        {
            var blobLock = GetBlobLock();

            var result = blobLock.IsLocked();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BlobLock_IsLocked_Should_Return_False_If_LeaseId_Is_Empty()
        {
            var blobLock = GetBlobLock();

            TestHelper.SetProperty(blobLock, b => b.LeaseId, string.Empty);

            var result = blobLock.IsLocked();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BlobLock_IsLocked_Should_Return_False_If_LeaseId_Is_Blank()
        {
            var blobLock = GetBlobLock();

            TestHelper.SetProperty(blobLock, b => b.LeaseId, "    ");

            var result = blobLock.IsLocked();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BlobLock_IsLocked_Should_Return_True_If_LeaseId_Is_Not_Null_Empty_Or_Blank()
        {
            var blobLock = GetBlobLock();

            TestHelper.SetProperty(blobLock, b => b.LeaseId, "12345");

            var result = blobLock.IsLocked();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void BlobLock_Obtain_Should_Return_False_If_An_Azure_RequestFailedException_Is_Thrown_By_Blob_ObtainLock_Method()
        {
            object errorMessageObject = null;
            Exception errorException = null;

            _blobMock.Setup(b => b.ObtainLock(It.IsAny<TimeSpan>(), It.IsAny<string>()))
                     .Throws(new global::Azure.RequestFailedException("Mocked exception"));

            _loggerMock.Setup(l => l.Error(It.IsAny<object>(), It.IsAny<Exception>()))
                       .Callback<object, Exception>((msg, ex) =>
                       {
                           errorMessageObject = msg;
                           errorException = ex;
                       });

            var blobLock = GetBlobLock();

            var result = blobLock.Obtain();

            Assert.IsFalse(result);

            Assert.IsNotNull(errorMessageObject);
            Assert.IsInstanceOfType(errorMessageObject, typeof(string));

            var errorMessage = errorMessageObject as string;
            Assert.AreEqual("An Azure SDK error occurred while acquiring blob's lease: Mocked exception", errorMessage);

            Assert.IsNotNull(errorException);
            Assert.IsInstanceOfType(errorException, typeof(global::Azure.RequestFailedException));
        }

        [TestMethod]
        public void BlobLock_Obtain_Should_Return_False_If_A_Generic_Exception_Is_Thrown_By_Blob_ObtainLock_Method()
        {
            object errorMessageObject = null;
            Exception errorException = null;

            _blobMock.Setup(b => b.ObtainLock(It.IsAny<TimeSpan>(), It.IsAny<string>()))
                     .Throws(new Exception("Mocked generic exception"));

            _loggerMock.Setup(l => l.Error(It.IsAny<object>(), It.IsAny<Exception>()))
                       .Callback<object, Exception>((msg, ex) =>
                       {
                           errorMessageObject = msg;
                           errorException = ex;
                       });

            var blobLock = GetBlobLock();

            var result = blobLock.Obtain();

            Assert.IsFalse(result);

            Assert.IsNotNull(errorMessageObject);
            Assert.IsInstanceOfType(errorMessageObject, typeof(string));

            var errorMessage = errorMessageObject as string;
            Assert.AreEqual("A generic error occurred while acquiring blob's lease: Mocked generic exception", errorMessage);

            Assert.IsNotNull(errorException);
            Assert.IsInstanceOfType(errorException, typeof(Exception));
        }

        [TestMethod]
        public void BlobLock_Obtain_Should_Return_False_If_A_Custom_Exception_Is_Thrown_By_Blob_ObtainLock_Method()
        {
            object errorMessageObject = null;
            Exception errorException = null;

            _blobMock.Setup(b => b.ObtainLock(It.IsAny<TimeSpan>(), It.IsAny<string>()))
                     .Throws(new ArgumentException("Mocked argument exception"));

            _loggerMock.Setup(l => l.Error(It.IsAny<object>(), It.IsAny<Exception>()))
                       .Callback<object, Exception>((msg, ex) =>
                       {
                           errorMessageObject = msg;
                           errorException = ex;
                       });

            var blobLock = GetBlobLock();

            var result = blobLock.Obtain();

            Assert.IsFalse(result);

            Assert.IsNotNull(errorMessageObject);
            Assert.IsInstanceOfType(errorMessageObject, typeof(string));

            var errorMessage = errorMessageObject as string;
            Assert.AreEqual("A generic error occurred while acquiring blob's lease: Mocked argument exception", errorMessage);

            Assert.IsNotNull(errorException);
            Assert.IsInstanceOfType(errorException, typeof(ArgumentException));
        }

        [TestMethod]
        public void BlobLock_Obtain_Should_Return_False_If_Blob_ObtainLock_Method_Does_Not_Throw_But_Return_Null_LeaseId()
        {
            _blobMock.Setup(b => b.ObtainLock(It.IsAny<TimeSpan>(), It.IsAny<string>())).Returns((string)null);

            var blobLock = GetBlobLock();

            var result = blobLock.Obtain();

            Assert.IsFalse(result);

            Assert.IsNull(blobLock.LeaseId);
        }

        [TestMethod]
        public void BlobLock_Obtain_Should_Return_False_If_Blob_ObtainLock_Method_Does_Not_Throw_But_Return_Empty_LeaseId()
        {
            _blobMock.Setup(b => b.ObtainLock(It.IsAny<TimeSpan>(), It.IsAny<string>())).Returns(string.Empty);

            var blobLock = GetBlobLock();

            var result = blobLock.Obtain();

            Assert.IsFalse(result);

            Assert.AreEqual(string.Empty, blobLock.LeaseId);
        }

        [TestMethod]
        public void BlobLock_Obtain_Should_Return_False_If_Blob_ObtainLock_Method_Does_Not_Throw_But_Return_Blank_LeaseId()
        {
            _blobMock.Setup(b => b.ObtainLock(It.IsAny<TimeSpan>(), It.IsAny<string>())).Returns("    ");

            var blobLock = GetBlobLock();

            var result = blobLock.Obtain();

            Assert.IsFalse(result);

            Assert.AreEqual("    ", blobLock.LeaseId);
        }

        [TestMethod]
        public void BlobLock_Obtain_Should_Return_True_If_Blob_ObtainLock_Method_Does_Not_Throw_And_Return_Not_Null_Empty_Or_Blank_LeaseId()
        {
            _blobMock.Setup(b => b.ObtainLock(It.IsAny<TimeSpan>(), It.IsAny<string>())).Returns("12345");

            var blobLock = GetBlobLock();

            var result = blobLock.Obtain();

            Assert.IsTrue(result);

            Assert.AreEqual("12345", blobLock.LeaseId);
        }

        [TestMethod]
        public void BlobLock_Release_Should_Not_Throw_If_An_Azure_RequestFailedException_Is_Thrown_By_Blob_ReleaseLock_Method()
        {
            object errorMessageObject = null;
            Exception errorException = null;

            _blobMock.Setup(b => b.ReleaseLock(It.IsAny<string>()))
                     .Throws(new global::Azure.RequestFailedException("Mocked exception"));

            _loggerMock.Setup(l => l.Error(It.IsAny<object>(), It.IsAny<Exception>()))
                       .Callback<object, Exception>((msg, ex) =>
                       {
                           errorMessageObject = msg;
                           errorException = ex;
                       });

            var blobLock = GetBlobLock();

            blobLock.Release();

            Assert.IsNotNull(errorMessageObject);
            Assert.IsInstanceOfType(errorMessageObject, typeof(string));

            var errorMessage = errorMessageObject as string;
            Assert.AreEqual("An Azure SDK error occurred while releasing blob's lease: Mocked exception", errorMessage);

            Assert.IsNotNull(errorException);
            Assert.IsInstanceOfType(errorException, typeof(global::Azure.RequestFailedException));
        }

        [TestMethod]
        public void BlobLock_Release_Should_Not_Throw_If_A_Generic_Exception_Is_Thrown_By_Blob_ReleaseLock_Method()
        {
            object errorMessageObject = null;
            Exception errorException = null;

            _blobMock.Setup(b => b.ReleaseLock(It.IsAny<string>()))
                     .Throws(new Exception("Mocked generic exception"));

            _loggerMock.Setup(l => l.Error(It.IsAny<object>(), It.IsAny<Exception>()))
                       .Callback<object, Exception>((msg, ex) =>
                       {
                           errorMessageObject = msg;
                           errorException = ex;
                       });

            var blobLock = GetBlobLock();

            blobLock.Release();

            Assert.IsNotNull(errorMessageObject);
            Assert.IsInstanceOfType(errorMessageObject, typeof(string));

            var errorMessage = errorMessageObject as string;
            Assert.AreEqual("A generic error occurred while releasing blob's lease: Mocked generic exception", errorMessage);

            Assert.IsNotNull(errorException);
            Assert.IsInstanceOfType(errorException, typeof(Exception));
        }

        [TestMethod]
        public void BlobLock_Release_Should_Not_Throw_If_A_Custom_Exception_Is_Thrown_By_Blob_ReleaseLock_Method()
        {
            object errorMessageObject = null;
            Exception errorException = null;

            _blobMock.Setup(b => b.ReleaseLock(It.IsAny<string>()))
                     .Throws(new ArgumentException("Mocked argument exception"));

            _loggerMock.Setup(l => l.Error(It.IsAny<object>(), It.IsAny<Exception>()))
                       .Callback<object, Exception>((msg, ex) =>
                       {
                           errorMessageObject = msg;
                           errorException = ex;
                       });

            var blobLock = GetBlobLock();

            blobLock.Release();

            Assert.IsNotNull(errorMessageObject);
            Assert.IsInstanceOfType(errorMessageObject, typeof(string));

            var errorMessage = errorMessageObject as string;
            Assert.AreEqual("A generic error occurred while releasing blob's lease: Mocked argument exception", errorMessage);

            Assert.IsNotNull(errorException);
            Assert.IsInstanceOfType(errorException, typeof(ArgumentException));
        }

        private BlobLock GetBlobLock(bool useMockedLogger = true)
        {
            var blobLock = new BlobLock(_blobMock.Object, TimeSpan.FromMilliseconds(1), useMockedLogger ? _loggerMock.Object : null);

            return blobLock;
        }
    }
}
