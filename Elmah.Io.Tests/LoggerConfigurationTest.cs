using System;
using Elmah.Io.Client;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class LoggerConfigurationTest
    {
        [Test]
        public void CanCreateANewLogger()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            var logger = loggerConfiguration.UseLog(Guid.NewGuid()).CreateLogger();

            // Assert
            Assert.That(logger, Is.Not.Null);
            Assert.That(logger.Options, Is.Not.Null);
            Assert.That(logger.Options.Url, Is.Not.Null);
            Assert.That(logger.Options.WebClient, Is.Not.Null);
        }

        [Test]
        public void CanCreateLoggerWithOptions()
        {
            // Arrange
            var fixture = new Fixture();
            var loggerOptions = new LoggerOptions {Durable = true, Url = fixture.Create<Uri>()};
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            var loggerWithOptions = loggerConfiguration.UseLog(Guid.NewGuid()).WithOptions(loggerOptions).CreateLogger();

            // Assert
            Assert.That(loggerWithOptions, Is.Not.Null);
            Assert.That(loggerWithOptions.Options, Is.EqualTo(loggerOptions));
        }

        [Test]
        public void CannotCreateLoggerWithoutLogId()
        {
            // Arrange
            var loggerConfiguration = new LoggerConfiguration();

            // Act
            TestDelegate createDelegate = () => loggerConfiguration.CreateLogger();

            // Assert
            Assert.Throws<ArgumentException>(createDelegate);
        }
    }
}