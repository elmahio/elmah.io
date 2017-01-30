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
            ErrorLog.Api = null;
        }

        [Test]
        public void CanSpecifyCustomUrlOnErrorLog()
        {
            // Arrange
            var configUri = _fixture.Create<Uri>();
            var errorLog =
                new ErrorLog(new Hashtable
                {
                    {"logId", _fixture.Create<Guid>().ToString()},
                    {"apiKey", "MyKey"},
                    {"url", configUri.ToString()}
                });

            // Act
            var uri = ErrorLog.Api.BaseUri;

            // Assert
            Assert.That(uri, Is.EqualTo(configUri));
        }
    }
}
