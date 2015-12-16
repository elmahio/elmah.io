using System;
using System.Collections;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class ErrorLogApplicationNameTest
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            ErrorLog.Client = null;
        }

        [Test]
        public void CanSetApplicationName()
        {
            var applicationName = _fixture.Create<string>();
            var errorLog = new ErrorLog(new Hashtable {{"applicationName", applicationName}, {"LogId", Guid.NewGuid().ToString()}, {"ApiKey", Guid.NewGuid()}});
            Assert.That(errorLog.ApplicationName, Is.EqualTo(applicationName));
        }

        [Test]
        public void CanExcludeApplicationName()
        {
            var errorLog = new ErrorLog(new Hashtable {{"LogId", Guid.NewGuid().ToString()}, {"ApiKey", Guid.NewGuid()}});
            Assert.That(errorLog.ApplicationName, Is.EqualTo(string.Empty));
        }
    }
}