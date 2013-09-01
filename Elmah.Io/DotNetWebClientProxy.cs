using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Mannex.Net;
using Mannex.Net.Mime;
using Mannex.Threading.Tasks;

namespace Elmah.Io
{
    internal class DotNetWebClientProxy : IWebClient
    {
        public Task<T> Post<T>(WebHeaderCollection headers, Uri address, string data,
            Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (data == null) throw new ArgumentNullException("data");
            if (resultor == null) throw new ArgumentNullException("resultor");

            var request = (HttpWebRequest) WebRequest.Create(address);
            request.Method = "POST";

            if (headers != null)
            {
                // Some headers like Content-Type cannot be added directly
                // and therefore must be treated specially and go over 
                // the corresponding property on the HttpWebRequest object.

                var headerz = new WebHeaderCollection {headers};
                var contentType = headerz[HttpRequestHeader.ContentType];
                if (contentType != null)
                {
                    request.ContentType = contentType;
                    headerz.Remove(HttpRequestHeader.ContentType);
                }

                request.Headers.Add(headerz);
            }

            var encoding = Encoding.UTF8; // TODO parameterize?
            var bytes = encoding.GetBytes(data);
            request.ContentLength = bytes.Length;

            var tcs = new TaskCompletionSource<T>();
            Spawn(Post(request, bytes, tcs, resultor), tcs);
            return tcs.Task;
        }

        static IEnumerable<Task> Post<T>(WebRequest request, byte[] bytes, TaskCompletionSource<T> tcs, Func<WebHeaderCollection, string, T> resultor)
        {
            Debug.Assert(request != null);
            Debug.Assert(bytes != null);
            Debug.Assert(tcs != null);
            Debug.Assert(resultor != null);

            var getRequestStreamTask = request.GetRequestStreamAsync();
            yield return getRequestStreamTask;

            using (var stream = getRequestStreamTask.Result)
            {
                var writeTask = stream.WriteAsync(bytes, 0, bytes.Length);
                yield return writeTask;
            }

            foreach (var task in Get(request, tcs, resultor))
                yield return task;
        }

        public Task<T> Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (resultor == null) throw new ArgumentNullException("resultor");

            var request = (HttpWebRequest) WebRequest.Create(address);
            
            if (headers != null)
                request.Headers.Add(headers);

            var tcs = new TaskCompletionSource<T>();
            Spawn(Get(request, tcs, resultor), tcs);            
            return tcs.Task;
        }

        static IEnumerable<Task> Get<T>(WebRequest request, TaskCompletionSource<T> tcs, Func<WebHeaderCollection, string, T> resultor)
        {
            Debug.Assert(request != null);
            Debug.Assert(tcs != null);
            Debug.Assert(resultor != null);

            var getResponseTask = request.GetResponseAsync();
            yield return getResponseTask;

            using (var response = getResponseTask.Result)
            using (var stream = response.GetResponseStream())
            {
                var contentType = response.Headers.Map(HttpResponseHeader.ContentType, h => new ContentType(h));
                var encoding = contentType != null
                    ? contentType.EncodingFromCharSet(Encoding.Default)
                    : Encoding.Default;

                var bytes = new byte[4096];
                var chars = (char[]) null;
                var decoder = encoding.GetDecoder();
                var sb = new StringBuilder();
                while (true)
                {
                    var readTask = stream.ReadAsync(bytes, 0, bytes.Length);
                    yield return readTask;
                    var readCount = readTask.Result;
                    if (readCount == 0)
                        break;
                    var charCount = decoder.GetCharCount(bytes, 0, readCount);
                    if (chars == null || charCount > chars.Length)
                        chars = new char[charCount];
                    var decodedCharCount = decoder.GetChars(bytes, 0, readCount, chars, 0, false);
                    sb.Append(chars, 0, decodedCharCount);
                }

                tcs.SetResult(resultor(response.Headers, sb.ToString()));
            }
        }

        static void Spawn<T>(IEnumerable<Task> job, TaskCompletionSource<T> tcs)
        {
            Debug.Assert(job != null);
            Debug.Assert(tcs != null);

            Task.Factory
                .StartNew(job)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else if (t.IsFaulted)
                    {
                        var aggregate = t.Exception;
                        Debug.Assert(aggregate != null);
                        tcs.TrySetException(aggregate.InnerExceptions);
                    }
                    else
                    {
                        Debug.Assert(t.Status == TaskStatus.RanToCompletion);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}