using System;
using System.Collections;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace Elmah.Io
{
    public class ErrorLog : Elmah.ErrorLog
    {
        private readonly string _logId;
        private readonly Uri _url = new Uri("http://elmahio.azurewebsites.net/");
        private readonly IWebClientFactory _webClientFactory;

        public ErrorLog(IDictionary config) : this(config, new DotNetWebClientFactory())
        {
        }

        public ErrorLog(IDictionary config, IWebClientFactory webClientFactory)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (!config.Contains("LogId"))
            {
                throw new ApplicationException("Missing LogId. Please specify a LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
            }

            Guid result;
            if (!Guid.TryParse(config["LogId"].ToString(), out result))
            {
                throw new ApplicationException("Invalid LogId. Please specify a valid LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
            }

            _logId = result.ToString();
            _webClientFactory = webClientFactory;
        }

        public override string Log(Error error)
        {
            var url = new Uri(_url, string.Format("api/logs?logId={0}", _logId));
            using (var webClient = _webClientFactory.Create())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var xml = ErrorXml.EncodeString(error);

                return webClient.Post(url, "=" + HttpUtility.UrlEncode(xml));
            }
        }

        public override ErrorLogEntry GetError(string id)
        {
            var url = new Uri(_url, string.Format("api/logs/{0}&logId={1}", id, _logId));
            string response;
            using (var webClient = _webClientFactory.Create())
            {
                response = webClient.Get(url);
            }

            dynamic d = JsonConvert.DeserializeObject(response);
            return MapErrorLogEntry(d.Id, d.ErrorXml);
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            var url = new Uri(_url, string.Format("api/logs?logId={0}&pageindex={1}&pagesize={2}", _logId, pageIndex, pageSize));
            string response;
            using (var webClient = _webClientFactory.Create())
            {
                response = webClient.Get(url);
            }

            dynamic d = JsonConvert.DeserializeObject(response);
            foreach (dynamic error in d)
            {
                errorEntryList.Add(MapErrorLogEntry(error.Id, error.ErrorXml));
            }

            return errorEntryList.Count;
        }

        private ErrorLogEntry MapErrorLogEntry(dynamic id, dynamic xml)
        {
            return new ErrorLogEntry(this, (string)id, ErrorXml.DecodeString((string)xml));
        }
    }
}