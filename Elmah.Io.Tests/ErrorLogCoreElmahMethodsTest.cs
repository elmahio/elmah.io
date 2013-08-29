﻿using System;
using System.Collections;
using System.Globalization;
using System.Net;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class ErrorLogCoreElmahMethodsTest
    {
        private const string ErrorXml = "<error host=\"localhost\" type=\"System.ApplicationException\" message=\"Error in the application.\" detail=\"System.ApplicationException: Error in the application.\" time=\"2013-07-13T06:16:03.9957581Z\" />";
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void CanLogError()
        {
            var id = _fixture.Create<int>().ToString(CultureInfo.InvariantCulture);
            var logId = _fixture.Create<Guid>().ToString();
            Uri actualUri = null;
            string actualData = null;

            var requestHeaders = new WebHeaderCollection();
            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Post(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, string, Func<WebHeaderCollection, string, string>>((headers, uri, data, resultor) => { requestHeaders = headers; actualUri = uri; actualData = data; })
                .Returns(id);

            var errorLog = new ErrorLog(new Hashtable { { "LogId", logId } }, webClientMock.Object);

            var result = errorLog.Log(new Error(new System.ApplicationException()));

            Assert.That(result, Is.EqualTo(id));
            Assert.That(requestHeaders[HttpRequestHeader.ContentType], Is.EqualTo("application/x-www-form-urlencoded"));
            Assert.That(actualUri.AbsoluteUri, Is.Not.Null.And.StringEnding(string.Format("api/logs2?logId={0}", logId)));
            Assert.That(actualData, Is.Not.Null.And.StringStarting("=").And.StringContaining("ApplicationException"));
        }

        [Test]
        public void CanGetError()
        {
            var id = _fixture.Create<string>();
            var logId = _fixture.Create<Guid>().ToString();
            var error = new { Id = id, ErrorXml };
            Uri actualUri = null;

            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Get(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, Func<WebHeaderCollection, string, string>>((headers, uri, resultor) => { actualUri = uri; })
                .Returns(JsonConvert.SerializeObject(error));
            
            var errorLog = new ErrorLog(new Hashtable { { "LogId", logId } }, webClientMock.Object);

            var result = errorLog.GetError(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(actualUri.AbsoluteUri, Is.Not.Null.And.StringEnding(string.Format("api/logs2?logId={1}&id={0}", id, logId)));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error.Type, Is.EqualTo("System.ApplicationException"));
        }

        [Test]
        public void CanGetErrors()
        {
            var pageIndex = _fixture.Create<int>();
            var pageSize = _fixture.Create<int>();
            var logId = _fixture.Create<Guid>().ToString();
            var errors = new {
                Total = 3,
                Errors = new[]
                {
                    new { Id = _fixture.Create<string>(), ErrorXml },
                    new { Id = _fixture.Create<string>(), ErrorXml },
                    new { Id = _fixture.Create<string>(), ErrorXml }
                }
            };
            Uri actualUri = null;

            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Get(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, Func<WebHeaderCollection, string, string>>((headers, uri, resultor) => { actualUri = uri; })
                .Returns(JsonConvert.SerializeObject(errors));

            var errorLog = new ErrorLog(new Hashtable { { "LogId", logId } }, webClientMock.Object);

            var results = new ArrayList();
            var count = errorLog.GetErrors(pageIndex, pageSize, results);

            Assert.That(actualUri.AbsoluteUri, Is.Not.Null.And.StringEnding(string.Format("api/logs2?logId={0}&pageindex={1}&pagesize={2}", logId, pageIndex, pageSize)));
            Assert.That(count, Is.EqualTo(3));
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(3));
        }
    }
}