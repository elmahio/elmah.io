using System;
using System.Collections;
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
    }
}