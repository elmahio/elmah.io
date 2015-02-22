using System;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.Io.Client
{
    public static class ExceptionExtensions
    {
        public static List<Item> ToDataList(this Exception exception)
        {
            if (exception == null || exception.Data.Count == 0) return null;

            return (from object key in exception.Data.Keys select new Item {Key = key.ToString(), Value = exception.Data[key].ToString()}).ToList();
        }
         
    }
}