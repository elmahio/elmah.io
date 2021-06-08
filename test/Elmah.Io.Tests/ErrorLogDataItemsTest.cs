using AutoFixture;
using Elmah.Io.Client;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Elmah.Io.Tests
{
    public class ErrorLogDataItemsTest
    {
        private Fixture _fixture;
        private ErrorLog _errorLog;
        private IMessagesClient _messagesMock;
        private Guid _logId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _logId = _fixture.Create<Guid>();
            ErrorLog.Api = null;
            var clientMock = Substitute.For<IElmahioAPI>();
            _messagesMock = Substitute.For<IMessagesClient>();
            clientMock.Messages.Returns(_messagesMock);
            _errorLog = new ErrorLog(clientMock, _logId);
        }

        [Test]
        public void CanLogData()
        {
            // Arrange
            var id = _fixture.Create<string>();
            CreateMessage actualMessage = null;
            Guid? actualLogId = null;

            _messagesMock
                .CreateAndNotifyAsync(Arg.Any<Guid>(), Arg.Any<CreateMessage>())
                .Returns(Task.FromResult(new Message { Id = id }))
                .AndDoes(x =>
                {
                    actualLogId = x.Arg<Guid>();
                    actualMessage = x.Arg<CreateMessage>();
                });

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
