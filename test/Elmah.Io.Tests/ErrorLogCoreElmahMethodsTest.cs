using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Rest;
using NSubstitute;
using NUnit.Framework;
using AutoFixture;

namespace Elmah.Io.Tests
{
    public class ErrorLogCoreElmahMethodsTest
    {
        private Fixture _fixture;
        private ErrorLog _errorLog;
        IMessages _messagesMock;
        private Guid _logId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _logId = _fixture.Create<Guid>();
            ErrorLog.Api = null;
            var clientMock = Substitute.For<IElmahioAPI>();
            _messagesMock = Substitute.For<IMessages>();
            clientMock.Messages.Returns(_messagesMock);
            _errorLog = new ErrorLog(clientMock, _logId);
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
                .CreateAndNotifyAsync(Arg.Any<Guid>(), Arg.Any<CreateMessage>())
                .Returns(Task.FromResult(new Message { Id = id }))
                .AndDoes(x =>
                {
                    actualLogId = x.Arg<Guid>();
                    actualMessage = x.Arg<CreateMessage>();
                });

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
                .GetWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, List<string>>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new HttpOperationResponse<Message> { Body = new Message { Id = id, Title = logMessage, DateTime = DateTime.UtcNow } }));

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
                .GetAllWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(),
                    Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<bool?>(), Arg.Any<Dictionary<string, List<string>>>(),
                    Arg.Any<CancellationToken>())
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