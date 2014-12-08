using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elmah.Io.Client;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class ErrorLogCoreElmahMethodsTest
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void CanLogError()
        {
            // Arrange
            var id = _fixture.Create<string>();
            var logMessage = _fixture.Create<string>();
            Message actualMessage = null;

            var loggerMock = new Mock<ILogger>();
            loggerMock
                .Setup(x => x.BeginLog(It.IsAny<Message>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Callback<Message, AsyncCallback, object>((message, callback, state) => { actualMessage = message; })
                .Returns(Task.FromResult(id));

            var errorLog = new ErrorLog(loggerMock.Object);

            // Act
            var result = errorLog.Log(new Error(new System.ApplicationException(logMessage)));

            // Assert
            Assert.That(result, Is.EqualTo(id));
            Assert.That(actualMessage, Is.Not.Null);
            Assert.That(actualMessage.Title, Is.EqualTo(logMessage));
        }

        [Test]
        public void CanGetError()
        {
            // Arrange
            var id = _fixture.Create<string>();
            var logMessage = _fixture.Create<string>();

            var loggerMock = new Mock<ILogger>();
            loggerMock
                .Setup(x => x.BeginGetMessage(It.IsAny<string>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Returns(Task.FromResult(new Message(logMessage) {Id = id}));

            var errorLog = new ErrorLog(loggerMock.Object);

            // Act
            var result = errorLog.GetError(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Message, Is.EqualTo(logMessage));
        }

        [Test]
        public void CanGetErrors()
        {
            // Arrange
            var message1 = _fixture.Create<Message>();
            var message2 = _fixture.Create<Message>();
            var pageIndex = _fixture.Create<int>();
            var pageSize = _fixture.Create<int>();
            var messages = new MessagesResult()
            {
                Total = 3,
                Messages = new List<Message> { message1, message2, }
            };
            var results = new ArrayList();

            var loggerMock = new Mock<ILogger>();

            var taskCompletionSource = new TaskCompletionSource<MessagesResult>(results);
            taskCompletionSource.SetResult(messages);
            loggerMock
                .Setup(x => x.BeginGetMessages(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Returns(taskCompletionSource.Task);

            var errorLog = new ErrorLog(loggerMock.Object);

            // Act
            var count = errorLog.GetErrors(pageIndex, pageSize, results);

            // Assert
            Assert.That(count, Is.EqualTo(3));
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
        }
    }
}