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
    }
}