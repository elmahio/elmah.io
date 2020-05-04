using AutoFixture;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Elmah.Io.Tests
{
    public class ErrorLogDataItemsTest
    {
        private Fixture _fixture;
        private ErrorLog _errorLog;
        Mock<IMessages> _messagesMock;
        private Guid _logId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _logId = _fixture.Create<Guid>();
            ErrorLog.Api = null;
            var clientMock = new Mock<IElmahioAPI>();
            _messagesMock = new Mock<IMessages>();
            clientMock.Setup(x => x.Messages).Returns(_messagesMock.Object);
            _errorLog = new ErrorLog(clientMock.Object, _logId);
        }

        [Test]
        public void CanLogData()
        {
            // Arrange
            var id = _fixture.Create<string>();
            CreateMessage actualMessage = null;
            Guid? actualLogId = null;

            _messagesMock
                .Setup(x => x.CreateAndNotifyAsync(It.IsAny<Guid>(), It.IsAny<CreateMessage>()))
                .Callback<Guid, CreateMessage>((logId, msg) =>
                {
                    actualLogId = logId;
                    actualMessage = msg;
                })
                .Returns(Task.FromResult(new Message { Id = id }));

            var exception = new HttpParseException("message", new Exception(), "virtualPath", "sourceCode", 42);

            // Act
            var result = _errorLog.Log(new Error(exception));

            // Assert
            Assert.That(actualMessage, Is.Not.Null);
            Assert.That(actualMessage.Data, Is.Not.Null);
            Assert.That(actualMessage.Data.Any(d => d.Key == "HttpParseException.VirtualPath" && d.Value == "virtualPath"));
            Assert.That(actualMessage.Data.Any(d => d.Key == "HttpParseException.Line" && d.Value == "42"));
        }
    }
}
