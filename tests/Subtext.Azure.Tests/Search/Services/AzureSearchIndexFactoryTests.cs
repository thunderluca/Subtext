using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subtext.Azure.Search.Services;
using System;

namespace Subtext.Azure.Tests.Search.Services
{
    [TestClass]
    public class AzureSearchIndexFactoryTests
    {
        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_ApiKey_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory(null, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("apiKey", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_ApiKey_Is_Empty()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory(string.Empty, null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("apiKey", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_ApiKey_Is_Blank()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("    ", null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("apiKey", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Empty()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", string.Empty));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Blank()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", "    "));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Is_Not_A_Valid_Url()
        {
            new AzureSearchIndexFactory("123", "test.net");
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Throw_If_Endpoint_Does_Not_Use_Https_Protocol()
        {
            var exception = Assert.ThrowsException<ArgumentException>(() => new AzureSearchIndexFactory("123", "http://localhost"));

            Assert.IsNotNull(exception);
            Assert.AreEqual("endpoint", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchIndexFactory_Constructor_Should_Not_Throw_If_All_Parameters_Are_Valid()
        {
            var indexFactory = new AzureSearchIndexFactory("123", "https://localhost");

            Assert.IsNotNull(indexFactory);
        }
    }
}
