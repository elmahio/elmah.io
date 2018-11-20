using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Rest;
using Moq;
using NUnit.Framework;
using AutoFixture;

namespace Elmah.Io.Tests
{
    public class ErrorLogCoreElmahMethodsTest
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
        public void CanLogError()
        {
            // Arrange
            var id = _fixture.Create<string>();
            var logMessage = _fixture.Create<string>();
            CreateMessage actualMessage = null;
            Guid? actualLogId = null;

            _messagesMock
                .Setup(x => x.CreateAndNotifyAsync(It.IsAny<Guid>(), It.IsAny<CreateMessage>()))
                .Callback<Guid, CreateMessage>((logId, msg) =>
                {
                    actualLogId = logId;
                    actualMessage = msg;
                })
                .Returns(Task.FromResult(new Message {Id = id}));

            // Act
            var result = _errorLog.Log(new Error(new System.ApplicationException(logMessage)));

            // Assert
            Assert.That(result, Is.EqualTo(id));
            Assert.That(actualLogId, Is.EqualTo(_logId));
            Assert.That(actualMessage, Is.Not.Null);
            Assert.That(actualMessage.Title, Is.EqualTo(logMessage));
        }

        [Test]
        public void CanGetError()
        {
            // Arrange
            var id = _fixture.Create<string>();
            var logMessage = _fixture.Create<string>();

            _messagesMock
                .Setup(x => x.GetWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new HttpOperationResponse<Message> {Body = new Message { Id = id, Title = logMessage, DateTime = DateTime.UtcNow }}));

            // Act
            var result = _errorLog.GetError(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Message, Is.EqualTo(logMessage));
        }

        [Test]
        public void CanGetErrors()
        {
            // Arrange
            var message1 = _fixture.Create<MessageOverview>();
            var message2 = _fixture.Create<MessageOverview>();
            var pageIndex = _fixture.Create<int>();
            var pageSize = _fixture.Create<int>();
            var messages = new MessagesResult
            {
                Total = 3,
                Messages = new List<MessageOverview> { message1, message2, }
            };
            var results = new ArrayList();

            var taskCompletionSource = new TaskCompletionSource<HttpOperationResponse<MessagesResult>>(results);
            taskCompletionSource.SetResult(new HttpOperationResponse<MessagesResult> {Body = messages});
            _messagesMock
                .Setup(
                    x =>
                        x.GetAllWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<bool?>(), It.IsAny<Dictionary<string,List<string>>>(),
                            It.IsAny<CancellationToken>()))
                .Returns(taskCompletionSource.Task);

            // Act
            var count = _errorLog.GetErrors(pageIndex, pageSize, results);

            // Assert
            Assert.That(count, Is.EqualTo(3));
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
        }
    }
}