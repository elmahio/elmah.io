using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using Moq;
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
            Uri actualUri = null;
            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Post(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, string, Func<WebHeaderCollection, string, string>>((headers, uri, data, resultor) => { actualUri = uri; })
                .Returns(Task.FromResult<string>(null));
            var errorLog = new ErrorLog(new Hashtable { {"LogId", _fixture.Create<Guid>().ToString()}, {"Url", configUri} }, webClientMock.Object);

            // Act
            errorLog.Log(new Error(new System.ApplicationException()));

            // Assert
            Assert.That(actualUri.ToString(), Is.StringStarting(configUri.ToString()));
        }

        [Test]
        public void DoDefaultToAzureWhenNoUrlSpecified()
        {
            // Arrange
            Uri actualUri = null;
            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Post(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, string, Func<WebHeaderCollection, string, string>>((headers, uri, data, resultor) => { actualUri = uri; })
                .Returns(Task.FromResult<string>(null));
            var errorLog = new ErrorLog(new Hashtable { { "LogId", _fixture.Create<Guid>().ToString() } }, webClientMock.Object);

            // Act
            errorLog.Log(new Error(new System.ApplicationException()));

            // Assert
            Assert.That(actualUri.ToString(), Is.StringStarting("http://elmahio.azurewebsites.net/"));
        }
    }
}
