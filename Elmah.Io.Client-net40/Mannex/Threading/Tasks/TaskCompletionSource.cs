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
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mannex.Threading.Tasks
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Extension methods for <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>

    static partial class TaskCompletionSourceExtensions
    {
        /// <summary>
        /// Attempts to conclude <see cref="TaskCompletionSource{TResult}"/>
        /// as being canceled, faulted or having completed successfully
        /// based on the corresponding status of the given 
        /// <see cref="Task{T}"/>.
        /// </summary>

        public static bool TryConcludeFrom<T>(this TaskCompletionSource<T> source, Task<T> task)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (task == null) throw new ArgumentNullException("task");

            if (task.IsCanceled)
            {
                source.TrySetCanceled();
            }
            else if (task.IsFaulted)
            {
                var aggregate = task.Exception;
                Debug.Assert(aggregate != null);
                source.TrySetException(aggregate.InnerExceptions);
            }
            else if (TaskStatus.RanToCompletion == task.Status)
            {
                source.TrySetResult(task.Result);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
