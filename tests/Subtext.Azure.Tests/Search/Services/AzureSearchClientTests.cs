using Azure;
using Azure.Search.Documents;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subtext.Azure.Search.Services;
using System;

namespace Subtext.Azure.Tests.Search.Services
{
    [TestClass]
    public class AzureSearchClientTests
    {
        SearchClient _client;
        Mock<ILog> _loggerMock;

        [TestInitialize]
        public void SetUp()
        {
            _client = new SearchClient(new Uri("https://localhost"), "blog-1", new AzureKeyCredential("123"));
            _loggerMock = new Mock<ILog>();
        }

        [TestMethod]
        public void AzureSearchClient_Constructor_Should_Throw_If_Client_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new AzureSearchClient(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("client", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchClient_Constructor_Should_Not_Throw_If_Logger_Is_Null()
        {
            var searchClient = new AzureSearchClient(_client);

            Assert.IsNotNull(searchClient);
        }

        [TestMethod]
        public void AzureSearchClient_Constructor_Should_Not_Throw_If_Logger_Is_Not_Null()
        {
            var searchClient = new AzureSearchClient(_client, _loggerMock.Object);

            Assert.IsNotNull(searchClient);
        }
    }
}
