using System;
using System.Linq;
using System.Threading;
using Elmah.Io.Client;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class LoggerTest
    {
        private Fixture _fixture;
        private string _apiKey;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _apiKey = Environment.GetEnvironmentVariable("ELMAH_IO_API_KEY", EnvironmentVariableTarget.User);
        }

        [Test]
        public void CanLogMessage()
        {
            // Arrange
            var logger = new Logger(new Guid("494cd9d0-3fab-4412-913b-2b2aa109dff5"), _apiKey);
            var message = _fixture.Create<Message>();

            // Act
            var result = logger.Log(message);

            // Assert
            Assert.That(result, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void CanLogMessageThroughHelpers()
        {
            // Arrange
            var logger = new Logger(new Guid("494cd9d0-3fab-4412-913b-2b2aa109dff5"), _apiKey);
            var value = _fixture.Create<string>();

            // Act
            logger.Verbose("{0}", value);

            // Assert
        }

        [Test]
        public void CanGetMessage()
        {
            // Arrange
            var logger = new Logger(new Guid("494cd9d0-3fab-4412-913b-2b2aa109dff5"), _apiKey);
            var message = _fixture.Create<Message>();
            var location = logger.Log(message);

            Thread.Sleep(10000);

            var id = location.Query.TrimStart('?').Split('&').Select(parameter => parameter.Split('='))
                        .Where(parameterSplitted => parameterSplitted.Length == 2 && parameterSplitted[0] == "id")
                        .Select(parameterSplitted => parameterSplitted[1])
                        .FirstOrDefault();

            // Act
            var result = logger.GetMessage(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo(message.Title));
        }



        [Test]
        public void CanGetMessages()
        {
            // Arrange
            var logger = new Logger(new Guid("494cd9d0-3fab-4412-913b-2b2aa109dff5"), _apiKey);
            var message = _fixture.Create<Message>();
            logger.Log(message);
            logger.Log(message);

            // Act
            var result = logger.GetMessages(0, 10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Messages.Count, Is.AtLeast(2));
        }

        [Test]
        public void CanRegisterForMessageFailEvent()
        {
            // Arrange
            var logger = new Logger(new Guid("494cd9d0-3fab-4412-913b-2b2aa109dff5"), _apiKey, new Uri("http://localhost"));
            var eventHandlerWasCalled = false;
            string reason = null;
            Message message = null;
            logger.OnMessageFail += (sender, args) =>
            {
                eventHandlerWasCalled = true;
                reason = args.Reason;
                message = args.Message;
            };

            // Act
            logger.Log(_fixture.Create<Message>());

            // Assert
            Assert.That(eventHandlerWasCalled);
            Assert.That(reason, Is.Not.Null);
            Assert.That(message, Is.Not.Null);
        }

        [Test]
        public void CanRegisterForMessageEvent()
        {
            // Arrange
            var logger = new Logger(new Guid("494cd9d0-3fab-4412-913b-2b2aa109dff5"), _apiKey);

            var eventHandlerWasCalled = false;
            Message message = null;

            logger.OnMessage += (sender, args) =>
            {
                eventHandlerWasCalled = true;
                message = args.Message;
            };

            // Act
            logger.Log(_fixture.Create<Message>());

            // Assert
            Assert.That(eventHandlerWasCalled);
            Assert.That(message, Is.Not.Null);
        }
    }
}