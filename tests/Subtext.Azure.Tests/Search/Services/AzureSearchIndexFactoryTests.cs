using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subtext.Azure.Search.Services;
using System;

namespace Subtext.Azure.Tests.Search.Services
{
    [TestClass]
    public class AzureSearchIndexFactoryTests
    {
        Mock<IHttpService> _httpServiceMock;
        Mock<ISerializationService> _serializationService;

        [TestInitialize]
        public void SetUp()
        {
            _httpServiceMock = new Mock<IHttpService>();
            _serializationService = new Mock<ISerializationService>();
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_ApiKey_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory(null, null, null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("apiKey", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_ApiKey_Is_Empty()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory(string.Empty, null, null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("apiKey", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_ApiKey_Is_Blank()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("    ", null, null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("apiKey", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", null, null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Empty()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", string.Empty, null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Blank()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", "    ", null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_HttpService_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new AzureSearchIndexFactory("123", "http://localhost", null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("httpService", exception.ParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Not_A_Valid_Url()
        {
            new AzureSearchIndexFactory("123", "test.net", _httpServiceMock.Object, null);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Does_Not_Use_Https_Protocol()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", "http://localhost", _httpServiceMock.Object, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_SerializationService_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new AzureSearchIndexFactory("123", "https://localhost", _httpServiceMock.Object, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("serializationService", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Not_Throw_If_All_Parameters_Are_Valid()
        {
            var indexFactory = new AzureSearchIndexFactory("123", "https://localhost", _httpServiceMock.Object, _serializationService.Object);

            Assert.IsNotNull(indexFactory);
        }
    }
}
