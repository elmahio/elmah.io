using System;
using System.Linq;
using Elmah.Io.Client;
using NUnit.Framework;

namespace Elmah.Io.Tests
{
    public class ExceptionExtensionsTest
    {
        [TestCase(true, 42, true, "42", "True")]
        [TestCase(true, "key", "value", "key", "value")]
        [TestCase(true, "key", "", "key", "")]
        [TestCase(true, "key", null, "key", "")]
        [TestCase(false, "", "value", null, null)]
        public void CanMapDataDictionary(bool shouldMap, object key, object value, string expectedKey, string expectedValue)
        {
            // Arrange
            var exception = new Exception();
            exception.Data.Add(key, value);

            // Act
            var dataList = exception.ToDataList();
            
            // Assert
            Assert.That(dataList != null);
            Assert.That(dataList.Count, Is.EqualTo(shouldMap ? 1 : 0));
            if (!shouldMap) return;
            var keyAndValue = dataList.First();
            Assert.That(keyAndValue.Key, Is.EqualTo(expectedKey));
            Assert.That(keyAndValue.Value, Is.EqualTo(expectedValue));
        }
    }
}