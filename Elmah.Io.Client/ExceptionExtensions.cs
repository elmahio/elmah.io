using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.Io.Client
{
    public static class ExceptionExtensions
    {
        public static List<Item> ToDataList(this Exception exception)
        {
            if (exception == null || exception.Data.Count == 0) return null;

            return exception
                .Data
                .Keys
                .Cast<object>()
                .Where(k => !string.IsNullOrWhiteSpace(k.ToString()))
                .Select(k => new Item {Key = k.ToString(), Value = Value(exception.Data, k)})
                .ToList();
        }

        private static string Value(IDictionary data, object key)
        {
            var value = data[key];
            if (value == null) return string.Empty;
            return value.ToString();
        }
    }
}