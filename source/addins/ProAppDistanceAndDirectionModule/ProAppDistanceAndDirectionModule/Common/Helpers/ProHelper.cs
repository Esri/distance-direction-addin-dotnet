using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ProAppDistanceAndDirectionModule.Common.Helpers
{
    internal class ProHelper
    {
        private const string NonAwaitExceptionString = "Exception while waiting for a Task to complete. Caller: {0}, File: {1}, Line: {2}. See the inner exception for additional information.";
        /// <summary>
        /// The function wraps the async function in a try catch. In the catch it throws a new exception that provides 
        /// the Name, file and line number of the function call this NonAwait function. It add the caught exception 
        /// in the inner exception. When specified, the afterwards action is called with a finally clause.
        /// </summary>
        /// <param name="function">Function returning a Task.</param>
        /// <param name="afterwards">[Optional] Action called within finally clause, null or unspecified means no finally action</param>
        /// <param name="callerName">Do not provide. Compiler provided name of method calling this method.</param>
        /// <param name="callerFile">Do not provide. Compiler provided name of file calling this method.</param>
        /// <param name="callerLine">Do not provide. Compiler provided name of line calling this method.</param>
        public static async void NonAwaitCall(Func<Task> function, Action afterwards = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            try
            {
                if (function != null)
                {
                    var task = function();
                    Debug.Assert(task != null);
                    if (task != null)
                        await task;
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format(NonAwaitExceptionString, callerName, callerFile, callerLine), e);
            }
            finally
            {
                if (afterwards != null)
                    afterwards();
            }
        }
        /// <summary>
        /// The function wraps the async function in a try catch. In the catch it throws a new exception that provides 
        /// the Name, file and line number of the function call this NonAwait function. It add the caught exception 
        /// in the inner exception.
        /// </summary>
        /// <param name="task">Task that will not be awaited.</param>
        /// <param name="afterwards">[Optional] Action called within finally clause, null or unspecified means no finally action</param>
        /// <param name="callerName">Do not provide. Compiler provided name of method calling this method.</param>
        /// <param name="callerFile">Do not provide. Compiler provided name of file calling this method.</param>
        /// <param name="callerLine">Do not provide. Compiler provided name of line calling this method.</param>
        public static async void NonAwaitCall(Task task, Action afterwards = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            try
            {
                Debug.Assert(task != null);
                if (task != null)
                    await task;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format(NonAwaitExceptionString, callerName, callerFile, callerLine), e);
            }
            finally
            {
                if (afterwards != null)
                    afterwards();
            }
        }
    }
}
