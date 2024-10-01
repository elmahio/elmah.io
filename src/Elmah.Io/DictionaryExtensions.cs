using System;
using System.Collections;
using System.Configuration;
using System.Linq;

namespace Elmah.Io
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S112:General or reserved exceptions should never be thrown",
            Justification = "Ignoring use of ApplicationException for now. We may want to consider changing the exception type on a major version upgrade")]
        internal static Guid LogId(this IDictionary dictionary)
        {
            const string logid = "logId";
            const string logidkey = "logIdKey";

            if (dictionary.ContainsCaseInsensitive(logid))
            {
                if (!Guid.TryParse(dictionary.ValueByKeyCaseInsensitive(logid), out Guid result))
                {
                    throw new System.ApplicationException(
                        "Invalid log ID. Please specify a valid log ID in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" logId=\"98895825-2516-43DE-B514-FFB39EA89A65\" apiKey=\"5ac68b71ddca4201bbf21991964ac29e\" />");
                }

                return result;
            }
            if (dictionary.ContainsCaseInsensitive(logidkey))
            {
                var appSettingsKey = dictionary.ValueByKeyCaseInsensitive(logidkey);
                var value = ConfigurationManager.AppSettings.Get(appSettingsKey) ?? throw new System.ApplicationException("You are trying to reference an AppSetting which is not found (key = '" + appSettingsKey + "'");

                if (!Guid.TryParse(value, out Guid result))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S112:General or reserved exceptions should never be thrown",
            Justification = "Ignoring use of ApplicationException for now. We may want to consider changing the exception type on a major version upgrade")]
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
                var value = ConfigurationManager.AppSettings.Get(appSettingsKey) ?? throw new System.ApplicationException("You are trying to reference a AppSetting which is not found (key = '" + appSettingsKey + "'");
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

        internal static string ProxyHost(this IDictionary dictionary)
        {
            const string proxyHost = "proxyHost";
            return dictionary.ContainsCaseInsensitive(proxyHost) ? dictionary.ValueByKeyCaseInsensitive(proxyHost) : string.Empty;
        }

        internal static int? ProxyPort(this IDictionary dictionary)
        {
            const string proxyPort = "proxyPort";
            var value = dictionary.ContainsCaseInsensitive(proxyPort) ? dictionary.ValueByKeyCaseInsensitive(proxyPort) : null;
            return !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var port) ? port : null;
        }
    }
}