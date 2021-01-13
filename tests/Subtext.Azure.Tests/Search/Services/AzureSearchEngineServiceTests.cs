using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subtext.Azure.Search.Models;
using Subtext.Azure.Search.Services;
using Subtext.Framework.Services.SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subtext.Azure.Tests.Search.Services
{
    [TestClass]
    public class AzureSearchEngineServiceTests
    {
        Mock<IIndexFactory> _indexFactoryMock;
        Mock<ILog> _loggerMock;
        Mock<ISearchClient> _searchClientMock;

        [TestInitialize]
        public void SetUp()
        {
            _indexFactoryMock = new Mock<IIndexFactory>();
            _loggerMock = new Mock<ILog>();
            _searchClientMock = new Mock<ISearchClient>();

            _searchClientMock.Setup(sc => sc.ContainsEntry(15)).Returns(true);
            _searchClientMock.Setup(sc => sc.CountEntries(-1)).Returns(20);
            _searchClientMock.Setup(sc => sc.CountEntries(1)).Returns(20);
            _searchClientMock.Setup(sc => sc.UploadEntry(It.Is<SearchEngineEntry>(q => q.EntryId == 15))).Returns(new IndexingError[0]);

            _indexFactoryMock.Setup(i => i.GetIndexNames()).Returns(new[] { "blog-1" });
            _indexFactoryMock.Setup(i => i.GetSearchClient()).Returns(_searchClientMock.Object);
        }

        [TestMethod]
        public void AzureSearchEngineService_Constructor_Should_Throw_If_IndexFactory_Is_Null()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new AzureSearchEngineService(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("indexFactory", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchEngineService_Constructor_Should_Not_Throw_If_Logger_Is_Null()
        {
            var azureSearchEngineService = GetSearchEngine(useMockedLogger: false);

            Assert.IsNotNull(azureSearchEngineService);
        }

        [TestMethod]
        public void AzureSearchEngineService_Constructor_Should_Not_Throw_If_Logger_Is_Not_Null()
        {
            var azureSearchEngineService = GetSearchEngine(useMockedLogger: true);

            Assert.IsNotNull(azureSearchEngineService);
        }

        [TestMethod]
        public void AzureSearchEngineService_AddPost_Should_Throw_If_Post_Is_Null()
        {
            var azureSearchEngineService = GetSearchEngine();

            var exception = Assert.ThrowsException<ArgumentNullException>(() => azureSearchEngineService.AddPost(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("post", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchEngineService_AddPost_Should_Not_Throw_If_Indexing_Service_Does_Not_Contain_Any_Index_And_An_Exception_Is_Thrown_During_Upload_Process()
        {
            object warningMessageObject = null;

            _loggerMock.Setup(l => l.Warn(It.IsAny<object>())).Callback<object>(msg => warningMessageObject = msg);

            _indexFactoryMock.Setup(i => i.GetIndexNames()).Returns((IEnumerable<string>)null);

            var azureSearchEngineService = GetSearchEngine();

            var entry = GetSearchEngineEntry();

            var errors = azureSearchEngineService.AddPost(entry);

            Assert.IsNotNull(warningMessageObject);
            Assert.IsInstanceOfType(warningMessageObject, typeof(string));

            var warningMessage = warningMessageObject as string;
            Assert.AreEqual("The indexing service didn't find any index", warningMessage);

            Assert.IsNotNull(errors);
            Assert.IsFalse(errors.Any());

            _searchClientMock.Verify(sc => sc.UploadEntry(It.IsAny<SearchEngineEntry>()), Times.Once());
        }

        [TestMethod]
        public void AzureSearchEngineService_AddPost_Should_Not_Throw_If_Indexing_Service_Does_Not_Contain_Any_Index_And_No_Exceptions_Are_Thrown_During_Upload_Process()
        {
            object warningMessageObject = null;

            _loggerMock.Setup(l => l.Warn(It.IsAny<object>())).Callback<object>(msg => warningMessageObject = msg);

            _indexFactoryMock.Setup(i => i.GetIndexNames()).Returns(new string[0]);

            _searchClientMock.Setup(sc => sc.UploadEntry(It.Is<SearchEngineEntry>(q => q.EntryId == 15))).Throws(new InvalidOperationException("You should not upload!"));

            var azureSearchEngineService = GetSearchEngine();

            var entry = GetSearchEngineEntry();

            var errors = azureSearchEngineService.AddPost(entry);

            Assert.IsNotNull(warningMessageObject);
            Assert.IsInstanceOfType(warningMessageObject, typeof(string));

            var warningMessage = warningMessageObject as string;
            Assert.AreEqual("The indexing service didn't find any index", warningMessage);

            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Any());

            var error = errors.ElementAt(0);
            Assert.IsNotNull(error.Entry);
            Assert.AreEqual(15, error.Entry.EntryId);
            Assert.IsNotNull(error.Exception);
            Assert.IsInstanceOfType(error.Exception, typeof(InvalidOperationException));
            Assert.AreEqual("You should not upload!", error.Exception.Message);

            _searchClientMock.Verify(sc => sc.UploadEntry(It.IsAny<SearchEngineEntry>()), Times.Once());
        }

        [TestMethod]
        public void AzureSearchEngineService_AddPosts_Should_Throw_If_Posts_Is_Null()
        {
            var azureSearchEngineService = GetSearchEngine();

            var exception = Assert.ThrowsException<ArgumentNullException>(() => azureSearchEngineService.AddPosts(null));

            Assert.IsNotNull(exception);
            Assert.AreEqual("posts", exception.ParamName);
        }

        [TestMethod]
        public void AzureSearchEngineService_AddPosts_Should_Not_Throw_And_Returns_Empty_Error_Collection_If_Posts_Is_Empty()
        {
            var azureSearchEngineService = GetSearchEngine();

            var errors = azureSearchEngineService.AddPosts(new SearchEngineEntry[0]);

            Assert.IsNotNull(errors);
            Assert.IsFalse(errors.Any());
        }

        [TestMethod]
        public void AzureSearchEngineService_AddPosts_Should_Not_Throw_And_Returns_Not_Empty_Error_Collection_If_Posts_Is_Not_Empty_But_Upload_Process_Returns_Multiple_Errors()
        {
            var entry1 = GetSearchEngineEntry(e => e.EntryId = 15);
            var entry2 = GetSearchEngineEntry(e => e.EntryId = 17);

            _searchClientMock.Setup(sc => sc.UploadEntry(It.Is<SearchEngineEntry>(q => q.EntryId == 15)))
                             .Returns(new[] { new IndexingError(entry1, new NullReferenceException("First error")) });
            _searchClientMock.Setup(sc => sc.UploadEntry(It.Is<SearchEngineEntry>(q => q.EntryId == 17)))
                             .Returns(new[] { new IndexingError(entry2, new OutOfMemoryException("Second error")) });

            var azureSearchEngineService = GetSearchEngine();

            var errors = azureSearchEngineService.AddPosts(new[] { entry1, entry2 });

            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Any());
            Assert.AreEqual(2, errors.Count());

            var error1 = errors.ElementAt(0);
            Assert.IsNotNull(error1.Entry);
            Assert.AreEqual(15, error1.Entry.EntryId);
            Assert.IsNotNull(error1.Exception);
            Assert.IsInstanceOfType(error1.Exception, typeof(NullReferenceException));
            Assert.AreEqual("First error", error1.Exception.Message);

            var error2 = errors.ElementAt(1);
            Assert.IsNotNull(error2.Entry);
            Assert.AreEqual(17, error2.Entry.EntryId);
            Assert.IsNotNull(error2.Exception);
            Assert.IsInstanceOfType(error2.Exception, typeof(OutOfMemoryException));
            Assert.AreEqual("Second error", error2.Exception.Message);
        }

        [TestMethod]
        public void AzureSearchEngineService_Dispose_Should_Not_Throw()
        {
            var azureSearchEngineService = GetSearchEngine();

            azureSearchEngineService.Dispose();
        }

        [TestMethod]
        public void AzureSearchEngineService_GetIndexedEntryCount_Should_Not_Throw()
        {
            var azureSearchEngineService = GetSearchEngine();

            var count = azureSearchEngineService.GetIndexedEntryCount(1);

            Assert.AreEqual(20, count);
        }

        [TestMethod]
        public void AzureSearchEngineService_GetTotalIndexedEntryCount_Should_Not_Throw()
        {
            var azureSearchEngineService = GetSearchEngine();

            var count = azureSearchEngineService.GetTotalIndexedEntryCount();

            Assert.AreEqual(20, count);
        }

        [TestMethod]
        public void AzureSearchEngineService_RemovePost_Should_Not_Throw_If_Indexing_Service_Returns_Null_Index_Name_Collection()
        {
            object warningMessageObject = null;

            _loggerMock.Setup(l => l.Warn(It.IsAny<object>())).Callback<object>(msg => warningMessageObject = msg);

            _indexFactoryMock.Setup(i => i.GetIndexNames()).Returns((IEnumerable<string>)null);

            var azureSearchEngineService = GetSearchEngine();

            azureSearchEngineService.RemovePost(15);

            Assert.IsNotNull(warningMessageObject);
            Assert.IsInstanceOfType(warningMessageObject, typeof(string));

            var warningMessage = warningMessageObject as string;
            Assert.AreEqual("The indexing service didn't find any index", warningMessage);

            _indexFactoryMock.Verify(i => i.GetSearchClient(), Times.Never());
        }

        [TestMethod]
        public void AzureSearchEngineService_RemovePost_Should_Not_Throw_If_Indexing_Service_Returns_Empty_Index_Name_Collection()
        {
            object warningMessageObject = null;

            _loggerMock.Setup(l => l.Warn(It.IsAny<object>())).Callback<object>(msg => warningMessageObject = msg);

            _indexFactoryMock.Setup(i => i.GetIndexNames()).Returns(new string[0]);

            var azureSearchEngineService = GetSearchEngine();

            azureSearchEngineService.RemovePost(15);

            Assert.IsNotNull(warningMessageObject);
            Assert.IsInstanceOfType(warningMessageObject, typeof(string));

            var warningMessage = warningMessageObject as string;
            Assert.AreEqual("The indexing service didn't find any index", warningMessage);

            _indexFactoryMock.Verify(i => i.GetSearchClient(), Times.Never());
        }

        [TestMethod]
        public void AzureSearchEngineService_RemovePost_Should_Not_Throw_If_Indexing_Service_Does_Not_Have_Any_Index_That_Contains_PostId_To_Remove()
        {
            object warningMessageObject = null;

            _loggerMock.Setup(l => l.Warn(It.IsAny<object>())).Callback<object>(msg => warningMessageObject = msg);

            _indexFactoryMock.Setup(i => i.GetIndexNames()).Returns(new[] { "blog-1" });

            var azureSearchEngineService = GetSearchEngine();

            azureSearchEngineService.RemovePost(13);

            Assert.IsNotNull(warningMessageObject);
            Assert.IsInstanceOfType(warningMessageObject, typeof(string));

            var warningMessage = warningMessageObject as string;
            Assert.AreEqual("Didn't find any index that contains post with id '13'", warningMessage);

            _searchClientMock.Verify(i => i.DeleteEntries(nameof(Entry.Id), It.IsAny<string[]>(), It.IsAny<bool>()), Times.Never());
        }

        [TestMethod]
        public void AzureSearchEngineService_RemovePost_Should_Not_Throw_If_Indexing_Service_Has_Index_That_Contains_PostId_To_Remove()
        {
            object warningMessageObject = null;

            _loggerMock.Setup(l => l.Warn(It.IsAny<object>())).Callback<object>(msg => warningMessageObject = msg);

            _indexFactoryMock.Setup(i => i.GetIndexNames()).Returns(new[] { "blog-1" });

            var azureSearchEngineService = GetSearchEngine();

            azureSearchEngineService.RemovePost(15);

            Assert.IsNull(warningMessageObject);

            _searchClientMock.Verify(i => i.DeleteEntries(nameof(Entry.Id), new[] { "15" }, true), Times.Once());
        }

        private ISearchEngineService GetSearchEngine(bool useMockedLogger = true)
        {
            var azureSearchEngineService = new AzureSearchEngineService(_indexFactoryMock.Object, useMockedLogger ? _loggerMock.Object : null);

            return azureSearchEngineService;
        }

        [TestMethod]
        public void AzureSearchEngineService_Search_Should_Not_Throw()
        {
            _searchClientMock.Setup(s => s.Search("test", 1, 10, -1))
                             .Returns(new[]
                             {
                                GetSearchEngineResult(),
                                GetSearchEngineResult(r => r.EntryId = 16)
                             });

            var azureSearchEngineService = GetSearchEngine();

            var results = azureSearchEngineService.Search("test", 10, 1);

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(2, results.Count());
            Assert.AreEqual(15, results.ElementAt(0).EntryId);
            Assert.AreEqual(16, results.ElementAt(1).EntryId);
        }

        private SearchEngineEntry GetSearchEngineEntry(Action<SearchEngineEntry> builder = null)
        {
            var entry = new SearchEngineEntry
            {
                BlogId = 1,
                BlogName = "Test blog",
                Body = "An interesting useless body",
                EntryId = 15,
                EntryName = "Fifteenth entry in my blog",
                GroupId = 3,
                IsPublished = true,
                PublishDate = new DateTime(2021, 1, 5, 12, 0, 0, DateTimeKind.Utc),
                Tags = "test,blog,subtext,azure",
                Title = "Fifteenth entry in my blog"
            };

            builder?.Invoke(entry);

            return entry;
        }

        private SearchEngineResult GetSearchEngineResult(Action<SearchEngineResult> builder = null)
        {
            var result = new SearchEngineResult
            {
                BlogName = "Test blog",
                EntryId = 15,
                EntryName = "Fifteenth entry in my blog",
                PublishDate = new DateTime(2021, 1, 5, 12, 0, 0, DateTimeKind.Utc),
                Score = 0.5f,
                Title = "Fifteenth entry in my blog"
            };

            builder?.Invoke(result);

            return result;
        }
    }
}
