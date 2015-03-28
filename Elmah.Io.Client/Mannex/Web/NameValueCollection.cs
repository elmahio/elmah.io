#region License, Terms and Author(s)
//
// Mannex - Extension methods for .NET
// Copyright (c) 2009 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Specialized;
using System.Text;

namespace Mannex.Web
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Extension methods for <see cref="NameValueCollection"/>.
    /// </summary>

    static partial class NameValueCollectionExtensions
    {
        /// <summary>
        /// Creates a query string from the key and value pairs found
        /// in the collection.
        /// </summary>
        /// <remarks>
        /// A question mark (?) is prepended if the resulting query string
        /// is not empty.
        /// </remarks>

        public static string ToQueryString(this NameValueCollection collection)
        {
            return W3FormEncode(collection, "?", null);
        }

        /// <summary>
        /// Encodes the content of the collection to a string
        /// suitably formatted per the <c>application/x-www-form-urlencoded</c>
        /// MIME media type.
        /// </summary>
        /// <remarks>
        /// Each value is escaped using <see cref="Uri.EscapeDataString"/> 
        /// but which can throw <see cref="UriFormatException"/> for very 
        /// large values.
        /// </remarks>

        public static string ToW3FormEncoded(this NameValueCollection collection)
        {
            return collection.ToW3FormEncoded(null);
        }

        /// <summary>
        /// Encodes the content of the collection to a string
        /// suitably formatted per the <c>application/x-www-form-urlencoded</c>
        /// MIME media type. An additional parameter specifies a function 
        /// used to encode the value into the URI.
        /// </summary>
        /// <remarks>
        /// A null reference is permitted for <paramref name="encoder"/> and 
        /// in which case <see cref="Uri.EscapeDataString"/> is used by 
        /// default. However, <see cref="Uri.EscapeDataString"/> may throw 
        /// <see cref="UriFormatException"/> for very large values.
        /// </remarks>

        public static string ToW3FormEncoded(this NameValueCollection collection, Func<string, string> encoder)
        {
            return W3FormEncode(collection, null, encoder);
        }

        static readonly Func<string, string> UriEscapeDataString = Uri.EscapeDataString;

        static string W3FormEncode(NameValueCollection collection, string prefix, Func<string, string> encoder)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            if (collection.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            var names = collection.AllKeys;
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var values = collection.GetValues(i);

                if (values == null)
                    continue;
                
                foreach (var value in values)
                {
                    if (sb.Length > 0)
                        sb.Append('&');

                    if (!string.IsNullOrEmpty(name))
                        sb.Append(name).Append('=');

                    sb.Append(string.IsNullOrEmpty(value) 
                              ? string.Empty
                              : (encoder ?? UriEscapeDataString)(value));
                }
            }

            if (sb.Length > 0 && !string.IsNullOrEmpty(prefix))
                sb.Insert(0, prefix);

            return sb.ToString();
        }
    }
}
