using System;
using System.Collections;
using System.Configuration;

namespace Elmah.Io
{
    internal static class DictionaryExtensions
    {
        public static string ApplicationName(this IDictionary config)
        {
            return config.Contains("applicationName") ? config["applicationName"].ToString() : string.Empty;
        }

        public static bool Durable(this IDictionary config)
        {
            if (!config.Contains("Durable"))
            {
                return false;
            }

            bool durable;
            return bool.TryParse(config["Durable"].ToString(), out durable) && durable;
        }

        public static string FailedRequestPath(this IDictionary config)
        {
            return config.Contains("FailedRequestPath") ? config["FailedRequestPath"].ToString() : null;
        }

        public static Uri Url(this IDictionary config)
        {
            if (!config.Contains("Url"))
            {
                return null;
            }

            Uri uri;
            if (!Uri.TryCreate(config["Url"].ToString(), UriKind.Absolute, out uri))
            {
                throw new ApplicationException(
                    "Invalid URL. Please specify a valid absolute url. In fact you don't even need to specify an url, which will make the error logger use the elmah.io backend.");
            }

            return new Uri(config["Url"].ToString());
        }

        public static Guid LogId(this IDictionary config)
        {
            if (config.Contains("LogId"))
            {
                Guid result;
                if (!Guid.TryParse(config["LogId"].ToString(), out result))
                {
                    throw new ApplicationException(
                        "Invalid LogId. Please specify a valid LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
                }

                return result;
            }
            else
            {
                var appSettingsKey = config["LogIdKey"].ToString();
                var value = ConfigurationManager.AppSettings.Get(appSettingsKey);
                if (value == null)
                {
                    throw new ApplicationException(
                        "You are trying to reference a AppSetting which is not found (key = '" + appSettingsKey + "'");
                }

                Guid result;
                if (!Guid.TryParse(value, out result))
                {
                    throw new ApplicationException(
                        "Invalid LogId. Please specify a valid LogId in your web.config like this: <appSettings><add key=\""
                        + appSettingsKey + "\" value=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
                }

                return result;
            }
        }
    }
}