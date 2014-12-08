using System;
using System.Collections;
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
            Assert.That(errorLog.Url, Is.EqualTo(configUri));
        }
    }
}
