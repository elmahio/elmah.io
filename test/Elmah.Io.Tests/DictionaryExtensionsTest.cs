using System;
using System.Collections;
using NUnit.Framework;

namespace Elmah.Io.Tests
{
    public class DictionaryExtensionsTest
    {
        [TestCase(null, "applicationName", null)]
        [TestCase("", "applicationName", "")]
        [TestCase("value", "applicationName", "value")]
        [TestCase("value", "ApplicationName", "value")]
        [TestCase("", null, null)]
        public void CanGetApplicationName(string expectedValue, string key, string value)
        {
            var dictionary = new Hashtable();
            if (!string.IsNullOrWhiteSpace(key))
            {
                dictionary.Add(key, value);
            }

            Assert.That(dictionary.ApplicationName(), Is.EqualTo(expectedValue));
        }

        [TestCase(null, "proxyHost", null)]
        [TestCase("", "proxyHost", "")]
        [TestCase("value", "proxyHost", "value")]
        [TestCase("value", "proxyHost", "value")]
        [TestCase("", null, null)]
        public void CanGetProxyHost(string expectedValue, string key, string value)
        {
            var dictionary = new Hashtable();
            if (!string.IsNullOrWhiteSpace(key))
            {
                dictionary.Add(key, value);
            }

            Assert.That(dictionary.ProxyHost(), Is.EqualTo(expectedValue));
        }

        [TestCase(null, "proxyPort", null)]
        [TestCase(null, "proxyPort", "")]
        [TestCase(5555, "proxyPort", "5555")]
        [TestCase(6666, "proxyPort", "6666")]
        public void CanGetProxyPort(int? expectedValue, string key, string value)
        {
            var dictionary = new Hashtable();
            if (!string.IsNullOrWhiteSpace(key))
            {
                dictionary.Add(key, value);
            }

            Assert.That(dictionary.ProxyPort(), Is.EqualTo(expectedValue));
        }

        [TestCase(false, "DC14639C-B930-4960-9A3A-BD73C6CA6375", "logId", "DC14639C-B930-4960-9A3A-BD73C6CA6375")]
        [TestCase(false, "E70628F4-EEED-4C06-B570-663F8AFA80E5", "LogId", "E70628F4-EEED-4C06-B570-663F8AFA80E5")]
        [TestCase(true, null, "logId", "No guid")]
        [TestCase(true, null, null, null)]
        [TestCase(false, "98895825-2516-43DE-B514-FFB39EA89A65", "logIdKey", "MyValidLogId")]
        [TestCase(false, "98895825-2516-43DE-B514-FFB39EA89A65", "LogIdKey", "MyValidLogId")]
        [TestCase(true, null, "logIdKey", "MyInvalidLogId")]
        [TestCase(true, null, "logIdKey", "NonExistingKey")]
        public void CanGetLogId(bool shouldThrowException, string expected, string key, string value)
        {
            var dictionary = new Hashtable();
            if (!string.IsNullOrWhiteSpace(key))
            {
                dictionary.Add(key, value);
            }

            if (shouldThrowException)
            {
                Assert.Throws<System.ApplicationException>(() => dictionary.LogId());
            }
            else
            {
                Assert.That(dictionary.LogId(), Is.EqualTo(new Guid(expected)));
            }
        }

        [TestCase(false, "ValidKey", "apiKey", "ValidKey")]
        [TestCase(false, "ValidKey", "ApiKey", "ValidKey")]
        [TestCase(true, null, null, null)]
        [TestCase(false, "ApiKey", "apiKeyKey", "MyValidApiKey")]
        [TestCase(false, "ApiKey", "ApiKeyKey", "MyValidApiKey")]
        [TestCase(true, null, "apiKeyKey", "NonExistingKey")]
        public void CanGetApiKey(bool shouldThrowException, string expected, string key, string value)
        {
            var dictionary = new Hashtable();
            if (!string.IsNullOrWhiteSpace(key))
            {
                dictionary.Add(key, value);
            }

            if (shouldThrowException)
            {
                Assert.Throws<System.ApplicationException>(() => dictionary.ApiKey());
            }
            else
            {
                Assert.That(dictionary.ApiKey(), Is.EqualTo(expected));
            }
        }
    }
}
