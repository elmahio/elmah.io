using System;
using System.Collections;
using System.Configuration;
using System.Linq;

namespace Elmah.Io.Elmah
{
    internal static class DictionaryExtensions
    {
        internal static bool ContainsCaseInsensitive(this IDictionary dictionary, string key)
        {
            return dictionary.Keys.Cast<string>().Any(existingKey => existingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        internal static string ValueByKeyCaseInsensitive(this IDictionary dictionary, string key)
        {
            var k =
                dictionary.Keys.Cast<string>()
                    .FirstOrDefault(existingKey => existingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            return (string) (k == null ? null : dictionary[k]);
        }

        internal static Uri Url(this IDictionary dictionary)
        {
            const string url = "url";

            if (!dictionary.ContainsCaseInsensitive(url))
            {
                return null;
            }

            Uri uri;
            if (!Uri.TryCreate(dictionary.ValueByKeyCaseInsensitive(url), UriKind.Absolute, out uri))
            {
                throw new System.ApplicationException(
                    "Invalid URL. Please specify a valid absolute url. In fact you don't even need to specify an url, which will make the error logger use the elmah.io backend.");
            }

            return new Uri(dictionary.ValueByKeyCaseInsensitive(url));
        }

        internal static Guid LogId(this IDictionary dictionary)
        {
            const string logid = "logId";
            const string logidkey = "logIdKey";

            if (dictionary.ContainsCaseInsensitive(logid))
            {
                Guid result;
                if (!Guid.TryParse(dictionary.ValueByKeyCaseInsensitive(logid), out result))
                {
                    throw new System.ApplicationException(
                        "Invalid log ID. Please specify a valid log ID in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" logId=\"98895825-2516-43DE-B514-FFB39EA89A65\" apiKey=\"5ac68b71ddca4201bbf21991964ac29e\" />");
                }

                return result;
            }
            if (dictionary.ContainsCaseInsensitive(logidkey))
            {
                var appSettingsKey = dictionary.ValueByKeyCaseInsensitive(logidkey);
                var value = ConfigurationManager.AppSettings.Get(appSettingsKey);
                if (value == null)
                {
                    throw new System.ApplicationException(
                        "You are trying to reference a AppSetting which is not found (key = '" + appSettingsKey + "'");
                }

                Guid result;
                if (!Guid.TryParse(value, out result))
                {
                    throw new System.ApplicationException(
                        "Invalid LogId. Please specify a valid LogId in your web.config like this: <appSettings><add key=\""
                        + appSettingsKey + "\" value=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
                }

                return result;
            }

            throw new System.ApplicationException(
                "Missing LogId or LogIdKey. Please specify a LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" apiKey=\"bc9c62543e35450495223f9934ed75cd\" />.");
        }

        internal static string ApiKey(this IDictionary dictionary)
        {
            const string apikey = "apiKey";
            const string apikeykey = "apiKeyKey";

            if (dictionary.ContainsCaseInsensitive(apikey))
            {
                return dictionary.ValueByKeyCaseInsensitive(apikey);
            }

            if (dictionary.ContainsCaseInsensitive(apikeykey))
            {

                var appSettingsKey = dictionary.ValueByKeyCaseInsensitive(apikeykey);
                var value = ConfigurationManager.AppSettings.Get(appSettingsKey);
                if (value == null)
                {
                    throw new System.ApplicationException(
                        "You are trying to reference a AppSetting which is not found (key = '" + appSettingsKey + "'");
                }

                return value;
            }

            throw new System.ApplicationException(
                "Missing API key. Please specify an API key in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" apiKey=\"bc9c62543e35450495223f9934ed75cd\" />.");
        }

        internal static string ApplicationName(this IDictionary dictionary)
        {
            const string applicationname = "applicationName";
            return dictionary.ContainsCaseInsensitive(applicationname) ? dictionary.ValueByKeyCaseInsensitive(applicationname) : string.Empty;
        }
    }
}