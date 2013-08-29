using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Elmah.Io
{
    static class WebClientExtensions
    {
        public static Task<string> DownloadStringAsTask(this WebClient client, Uri address) 
        {
            if (client == null) throw new ArgumentNullException("client");
            if (address == null) throw new ArgumentNullException("address");

            var task = client.EventAsync
                (
                    (EventHandler<DownloadStringCompletedEventArgs> handler) => new DownloadStringCompletedEventHandler((sender, args) => handler(sender, args)), 
                    (wc, handler) => wc.DownloadStringCompleted += handler, 
                    (wc, handler) => wc.DownloadStringCompleted -= handler, 
                    args => args.Result
                );
            
            client.DownloadStringAsync(address);
            
            return task;
        }        

        public static Task<string> UploadStringAsTask(this WebClient client, Uri address, string data) 
        {
            if (client == null) throw new ArgumentNullException("client");
            if (address == null) throw new ArgumentNullException("address");

            var task = client.EventAsync
                (
                    (EventHandler<UploadStringCompletedEventArgs> handler) => new UploadStringCompletedEventHandler((sender, args) => handler(sender, args)), 
                    (wc, handler) => wc.UploadStringCompleted += handler, 
                    (wc, handler) => wc.UploadStringCompleted -= handler, 
                    args => args.Result
                );

            client.UploadStringAsync(address, data);

            return task;
        }

        static Task<TResult> EventAsync<TEventSource, THandler, TEventArgs, TResult>(
            this TEventSource source, 
            Func<EventHandler<TEventArgs>, THandler> handlerMapper, 
            Action<TEventSource, THandler> handlerAdder, 
            Action<TEventSource, THandler> handlerRemover, 
            Func<TEventArgs, TResult> resultSelector) 
            where TEventArgs : AsyncCompletedEventArgs
        {
            if (source == null) throw new ArgumentNullException("source");
            if (handlerMapper == null) throw new ArgumentNullException("handlerMapper");
            if (handlerAdder == null) throw new ArgumentNullException("handlerAdder");
            if (resultSelector == null) throw new ArgumentNullException("resultSelector");

            var tcs = new TaskCompletionSource<TResult>();
            var mappedHandler = new THandler[1];
            var handler = new EventHandler<TEventArgs>((_, args) => 
            {
                if (handlerRemover != null)
                    handlerRemover((TEventSource) _, mappedHandler[0]);
                if (args.Error != null) tcs.SetException(args.Error);
                else if (args.Cancelled) tcs.SetCanceled();
                else tcs.SetResult(resultSelector(args));
            });
            mappedHandler[0] = handlerMapper(handler);
            handlerAdder(source, mappedHandler[0]);
            return tcs.Task;
        }
    }
}