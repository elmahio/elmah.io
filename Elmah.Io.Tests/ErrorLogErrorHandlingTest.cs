using System;
using System.Collections;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class ErrorLogErrorHandlingTest
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            ErrorLog.Client = null;
            _fixture = new Fixture();
        }

        [Test]
        public void AssertThrowsArgumentNullExceptionOnMissingConfig()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ErrorLog((IDictionary)null));
            Assert.That(exception.ParamName, Is.EqualTo("config"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnMissingLogId()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable()));
            Assert.That(exception.Message, Is.StringContaining("Missing LogId"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnMissingApiKey()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable {{"LogId", Guid.NewGuid().ToString()}}));
            Assert.That(exception.Message, Is.StringContaining("Missing API key"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnInvalidLogId()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogId", "NoGuid" }, {"ApiKey", _fixture.Create<string>()} }));
            Assert.That(exception.Message, Is.StringContaining("Invalid LogId"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnInvalidUrl()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogId", Guid.NewGuid().ToString() }, { "Url", "NoUrl" }, {"ApiKey", _fixture.Create<string>()} }));
            Assert.That(exception.Message, Is.StringContaining("Invalid URL"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnMissingAppSettings()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogIdKey", "NoKey" }, {"ApiKey", _fixture.Create<string>()} }));
            Assert.That(exception.Message, Is.StringContaining("You are trying to reference a AppSetting which is not found"));
        }

        [Test]
        public void AssertThrowsExplainingApplicationExceptionOnInvalidLogIdInAppSettings()
        {
            var exception = Assert.Throws<ApplicationException>(() => new ErrorLog(new Hashtable { { "LogIdKey", "MyInvalidLogId" }, {"ApiKey", _fixture.Create<string>()} }));
            Assert.That(exception.Message, Is.StringContaining("Invalid LogId"));
        }

        [Test]
        public void CanCreateErrorLogWithValidLogId()
        {
            var logId = new Guid();
            var errorLog = new ErrorLog(new Hashtable { { "LogId", logId }, {"ApiKey", _fixture.Create<string>()} });
            Assert.That(errorLog, Is.Not.Null);
        }

        [Test]
        public void CanCreateErrorLogWithValidLogIdKey()
        {
            var errorLog = new ErrorLog(new Hashtable { { "LogIdKey", "MyValidLogId" }, {"ApiKey", _fixture.Create<string>()} });
            Assert.That(errorLog, Is.Not.Null);
        }
    }
}