using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Elmah.Io.Tests
{
    public class ErrorLogErrorHandlingTest
    {
        [Test]
        public void AssertThrowsArgumentNullExceptionOnMissingConfig()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ErrorLog(null));
            Assert.That(exception.ParamName, Is.EqualTo("config"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnMissingLogId()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable()));
            Assert.That(exception.Message, Is.StringContaining("Missing LogId"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnInvalidLogId()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogId", "NoGuid" } }));
            Assert.That(exception.Message, Is.StringContaining("Invalid LogId"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnInvalidUrl()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogId", Guid.NewGuid().ToString() }, { "Url", "NoUrl" } }));
            Assert.That(exception.Message, Is.StringContaining("Invalid URL"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnMissingAppSettings()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogIdKey", "NoKey" } }));
            Assert.That(exception.Message, Is.StringContaining("You are trying to reference a AppSetting which is not found"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnInvalidLogIdInAppSettings()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogIdKey", "MyInvalidLogId" } }));
            Assert.That(exception.Message, Is.StringContaining("Invalid LogId"));
        }

        [Test]
        public void CanCreateErrorLogWithValidLogId()
        {
            var logId = new Guid();
            var errorLog = new ErrorLog(new Hashtable { { "LogId", logId } });
            Assert.That(errorLog, Is.Not.Null);
        }

        [Test]
        public void CanCreateErrorLogWithValidLogIdKey()
        {
            var errorLog = new ErrorLog(new Hashtable { { "LogIdKey", "MyValidLogId" } });
            Assert.That(errorLog, Is.Not.Null);
        }

        [Test]
        public void CanLogErrorXmlToAppDataOnError()
        {
            var appData = Path.Combine(AssemblyDirectory, ErrorLog.FailedMessagesDirectory);
            var guid = new Guid();
            if (Directory.Exists(appData)) Directory.Delete(appData, true);
            Directory.CreateDirectory(appData);

            var errorLog =
                new ErrorLog(new Hashtable {{"LogId", guid}, {"Url", string.Format("http://{0}.com", guid)}});
            errorLog.EndLog(errorLog.BeginLog(new Error(), null, null));

            var errorFiles = Directory.GetFiles(appData);
            Assert.That(errorFiles.Length, Is.EqualTo(1));
            Assert.That(errorFiles[0].Contains("error-") && errorFiles[0].EndsWith(".xml"));
        }

        private static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}