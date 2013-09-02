using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

                headers = new WebHeaderCollection { headers };
                var contentType = headers[HttpRequestHeader.ContentType];
                if (contentType != null)
                {
                    request.ContentType = contentType;
                    headers.Remove(HttpRequestHeader.ContentType);
                }

                request.Headers.Add(headers);
            }

            var encoding = Encoding.UTF8; // TODO parameterize?
            var bytes = encoding.GetBytes(data);
            request.ContentLength = bytes.Length;

            return Spawn<T>(tcs => Post(request, bytes, tcs, resultor));
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

            foreach (var task in GetResponse(request, tcs, resultor))
                yield return task;
        }

        public Task<T> Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (resultor == null) throw new ArgumentNullException("resultor");

            var request = (HttpWebRequest) WebRequest.Create(address);
            
            if (headers != null)
                request.Headers.Add(headers);

            return Spawn<T>(tcs => GetResponse(request, tcs, resultor));            
        }

        static IEnumerable<Task> GetResponse<T>(WebRequest request, TaskCompletionSource<T> tcs, Func<WebHeaderCollection, string, T> resultor)
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

                var sb = new StringBuilder();
                foreach (var task in ReadAllText(stream, encoding, sb)) 
                    yield return task;

                tcs.SetResult(resultor(response.Headers, sb.ToString()));
            }
        }

        static IEnumerable<Task> ReadAllText(Stream stream, Encoding encoding, StringBuilder output)
        {
            Debug.Assert(stream != null);
            Debug.Assert(encoding != null);
            Debug.Assert(output != null);

            var bytes = new byte[4096];
            var chars = (char[]) null;
            var decoder = encoding.GetDecoder();
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
                output.Append(chars, 0, decodedCharCount);
            }
        }

        static Task<T> Spawn<T>(Func<TaskCompletionSource<T>, IEnumerable<Task>> jobFunc)
        {
            Debug.Assert(jobFunc != null);

            var tcs = new TaskCompletionSource<T>();
            
            Task.Factory
                .StartNew(jobFunc(tcs))
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

            return tcs.Task;
        }
    }
}