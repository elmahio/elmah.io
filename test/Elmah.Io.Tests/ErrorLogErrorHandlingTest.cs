using System;
using System.Collections;
using NUnit.Framework;

namespace Elmah.Io.Tests
{
    public class ErrorLogErrorHandlingTest
    {
        [SetUp]
        public void SetUp()
        {
            ErrorLog.Api = null;
        }

        [Test]
        public void AssertThrowsArgumentNullExceptionOnMissingConfig()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ErrorLog((IDictionary)null));
            Assert.That(exception.ParamName, Is.EqualTo("config"));
        }

        [Test]
        public void CanCreateErrorLogWithValidLogIdAndApiKey()
        {
            var errorLog = new ErrorLog(new Hashtable
            {
                {"logId", Guid.NewGuid().ToString()},
                {"apiKey", "ApiKey"}
            });
            Assert.That(errorLog, Is.Not.Null);
        }

        [Test]
        public void CanCreateErrorLogWithValidLogIdKeyAndApiKeyKey()
        {
            var errorLog = new ErrorLog(new Hashtable
            {
                {"LogIdKey", "MyValidLogId"},
                {"apiKeyKey", "MyValidApiKey"}
            });
            Assert.That(errorLog, Is.Not.Null);
        }

        [Test]
        public void CanCreateErrorLogWithApplicationName()
        {
            var errorLog = new ErrorLog(new Hashtable
            {
                {"logId", Guid.NewGuid().ToString()},
                {"apiKey", "ApiKey"},
                {"applicationName", "MyApp"}
            });
            Assert.That(errorLog, Is.Not.Null);
            Assert.That(errorLog.ApplicationName, Is.EqualTo("MyApp"));
        }
    }
}