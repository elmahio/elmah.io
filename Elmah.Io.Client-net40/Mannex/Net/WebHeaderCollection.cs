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
using System.Net;

namespace Mannex.Net
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Extension methods for <see cref="WebHeaderCollection"/>.
    /// </summary>

    static partial class WebHeaderCollectionExtensions
    {
        /// <summary>
        /// Applies a projection to response header if the response header 
        /// is contained in the collection is non-empty. Otherwise it 
        /// returns the default value for the type <typeparamref name="T"/>.
        /// </summary>

        public static T Map<T>(this WebHeaderCollection headers, HttpResponseHeader header, Func<string, T> mapper)
        {
            return Map(headers, header, default(T), mapper);
        }

        /// <summary>
        /// Applies a projection to response header if the response header 
        /// is contained in the collection is non-empty. Otherwise it 
        /// returns a given default of type <typeparamref name="T"/>.
        /// </summary>

        public static T Map<T>(this WebHeaderCollection headers, HttpResponseHeader header, T defaultValue, Func<string, T> mapper)
        {
            if (headers == null) throw new ArgumentNullException("headers");
            if (mapper == null) throw new ArgumentNullException("mapper");
            var value = headers[header];
            return string.IsNullOrEmpty(value) ? defaultValue : mapper(value);
        }
    }
}
