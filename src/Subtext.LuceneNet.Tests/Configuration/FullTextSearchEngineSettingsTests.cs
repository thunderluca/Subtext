using Lucene.Net.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subtext.LuceneNet.Configuration;
using System.Linq;

namespace Subtext.LuceneNet.Tests.Configuration
{
    [TestClass]
    public class FullTextSearchEngineSettingsTests
    {
        [TestMethod]
        public void FullTextSearchEngineSettings_WithEmptyConfig_LanguageIsEnglish()
        {
            FullTextSearchEngineSettings settings = FullTextSearchEngineSettings.Settings;
            Assert.AreEqual("English", settings.Language);
        }

        [TestMethod]
        public void FullTextSearchEngineSettings_WithEmptyConfig_StopWordsIsDefaultOne()
        {
            FullTextSearchEngineSettings settings = FullTextSearchEngineSettings.Settings;

            Assert.AreEqual(StopAnalyzer.ENGLISH_STOP_WORDS_SET.Count, settings.StopWords.Count);

            for (var i = 0; i < StopAnalyzer.ENGLISH_STOP_WORDS_SET.Count; i++)
            {
                Assert.AreEqual(StopAnalyzer.ENGLISH_STOP_WORDS_SET.ElementAt(i), settings.StopWords.ElementAt(i));
            }
        }

        [TestMethod]
        public void FullTextSearchEngineSettings_WithStopWordsSpecified_StopWordsAreTheCorrectOnes()
        {
            FullTextSearchEngineSettings settings = FullTextSearchEngineSettings.Settings;
            settings.StopWordsString = "e,a,in,che";
            Assert.IsTrue(settings.StopWords.Contains("e"));
            Assert.IsTrue(settings.StopWords.Contains("a"));
            Assert.IsTrue(settings.StopWords.Contains("in"));
            Assert.IsTrue(settings.StopWords.Contains("che"));
            Assert.IsFalse(settings.StopWords.Contains("ch"));
        }

        [TestMethod]
        public void FullTextSearchEngineSettings_WithEmptyConfig_TuningParametersAreDefault()
        {
            FullTextSearchEngineSettings settings = FullTextSearchEngineSettings.Settings;
            Assert.IsNotNull(settings.Parameters);
            Assert.AreEqual(1f, settings.Parameters.EntryNameBoost);
        }

        [TestMethod]
        public void FullTextSearchEngineSettings_WithConfig_MinimumDocumentFrequencyIsAsSet()
        {
            FullTextSearchEngineSettings settings = FullTextSearchEngineSettings.Settings;
            Assert.IsNotNull(settings.Parameters);
            Assert.AreEqual(10, settings.Parameters.MinimumDocumentFrequency);
        }

        [TestMethod]
        public void FullTextSearchEngineSettings_WithEmptyConfig_IndexFolderLocationIsAdd_Data()
        {
            FullTextSearchEngineSettings settings = FullTextSearchEngineSettings.Settings;
            Assert.AreEqual("~/App_Data", settings.IndexFolderLocation);
        }

        [TestMethod]
        public void FullTextSearchEngineSettings_WithEmptyConfig_IsEnabledIsTrue()
        {
            FullTextSearchEngineSettings settings = FullTextSearchEngineSettings.Settings;
            Assert.IsTrue(settings.IsEnabled);
        }
        
    }
}
