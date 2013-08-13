using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Elmah.Io.Tests
{
    public class ErrorLogCustomUrlTest
    {
        private Fixture _fixture;
        private Mock<IWebClientFactory> _webClientFactoryMock;
        private Mock<IWebClient> _webClientMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _webClientFactoryMock = new Mock<IWebClientFactory>();
            _webClientMock = new Mock<IWebClient>();
            _webClientFactoryMock.Setup(x => x.Create()).Returns(_webClientMock.Object);
        }

        [Test]
        public void CanSpecifyCustomUrlOnErrorLog()
        {
            // Arrange
            var configUri = _fixture.Create<Uri>();
            var errorLog = new ErrorLog(new Hashtable { {"LogId", _fixture.Create<Guid>().ToString()}, {"Url", configUri} }, _webClientFactoryMock.Object);
            Uri actualUri = null;
            var webHeaderCollection = new WebHeaderCollection();
            _webClientMock.SetupGet(x => x.Headers).Returns(webHeaderCollection);
            _webClientMock
                .Setup(x => x.Post(It.IsAny<Uri>(), It.IsAny<string>()))
                .Callback<Uri, string>((uri, data) => { actualUri = uri; });

            // Act
            errorLog.Log(new Error(new System.ApplicationException()));

            // Assert
            Assert.That(actualUri.ToString(), Is.StringStarting(configUri.ToString()));
        }

        [Test]
        public void DoDefaultToAzureWhenNoUrlSpecified()
        {
            // Arrange
            var errorLog = new ErrorLog(new Hashtable { { "LogId", _fixture.Create<Guid>().ToString() } }, _webClientFactoryMock.Object);
            Uri actualUri = null;
            var webHeaderCollection = new WebHeaderCollection();
            _webClientMock.SetupGet(x => x.Headers).Returns(webHeaderCollection);
            _webClientMock
                .Setup(x => x.Post(It.IsAny<Uri>(), It.IsAny<string>()))
                .Callback<Uri, string>((uri, data) => { actualUri = uri; });

            // Act
            errorLog.Log(new Error(new System.ApplicationException()));

            // Assert
            Assert.That(actualUri.ToString(), Is.StringStarting("http://elmahio.azurewebsites.net/"));
        }
    }
}
