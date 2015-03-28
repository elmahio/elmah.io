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

using System.Diagnostics;
using System.Globalization;

namespace Mannex
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Extension methods for <see cref="int"/>.
    /// </summary>

    static partial class Int32Extensions
    {
        /// <summary>
        /// Converts <see cref="int"/> to its string representation in the
        /// invariant culture.
        /// </summary>

        [DebuggerStepThrough]
        public static string ToInvariantString(this int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        /*
        /// <summary>
        /// Calculates the quotient and remainder from dividing two numbers 
        /// and returns a user-defined result.
        /// </summary>

        [DebuggerStepThrough]
        public static T DivRem<T>(this int dividend, int divisor, Func<int, int, T> resultFunc)
        {
            if (resultFunc == null) throw new ArgumentNullException("resultFunc");
            var quotient = dividend / divisor;
            var remainder = dividend % divisor;
            return resultFunc(quotient, remainder);
        }

        /// <summary>
        /// Returns the digits of the integer encoded in another digital 
        /// system given as a list of ordered digits. The number of digits 
        /// in the list also determines the radix.
        /// </summary>
        
        public static IEnumerable<T> LsDigits<T>(this int number, IList<T> digits)
        {                                                    // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var d in number.LsDigits(digits.Count)) // ReSharper restore LoopCanBeConvertedToQuery
                yield return digits[d];
        }

        /// <summary>
        /// Returns a sequence of the least significant base 10 digits in 
        /// the integer.
        /// </summary>

        public static IEnumerable<int> LsDigits(this int number)
        {
            return LsDigits(number, 10);
        }

        /// <summary>
        /// Returns a sequence of the least significant digits in the 
        /// integer in a given radix.
        /// </summary>
        
        public static IEnumerable<int> LsDigits(this int number, int radix)
        {
            do
            {
                yield return number % radix;
                number = number / radix;
            }
            while (number > 0);
        }

        /// <summary>
        /// Gets the digit at a specific position of the number (assuming 
        /// base 10) with the first position being zero.
        /// </summary>

        public static int GetDigit(this int number, int position)
        {
            return GetDigit(number, position, 10);
        }

        /// <summary>
        /// Gets the digit at a specific position of the number with
        /// the first position being zero. An additional parameter specifies 
        /// the radix to assume.
        /// </summary>

        public static int GetDigit(this int number, int position, int radix)
        {
            return (number / checked((int) Math.Pow(radix, position))) % radix;
        }*/
    }
}