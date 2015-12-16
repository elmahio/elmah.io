using System;
using System.Collections;
using Elmah.Io.Client;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class ErrorLogCustomUrlTest
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            ErrorLog.Client = null;
        }

        [Test]
        public void CanSpecifyCustomUrlOnErrorLog()
        {
            // Arrange
            var configUri = _fixture.Create<Uri>();
            var errorLog = new ErrorLog(new Hashtable { { "LogId", _fixture.Create<Guid>().ToString() }, { "Url", configUri } });

            // Act
            errorLog.Log(new Error(new System.ApplicationException()));

            // Assert
            var client = ErrorLog.Client as Logger;
            Assert.That(client != null);
            Assert.That(client.Url, Is.EqualTo(configUri));
        }
    }
}
