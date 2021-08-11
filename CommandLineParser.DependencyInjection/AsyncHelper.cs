using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommandLineParser.DependencyInjection
{
    /// <summary>
    /// Async Helper to run Async Task's Synchronously.
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory TaskFactory = new
            TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// Run an Async task Synchronously and return the result.
        /// </summary>
        /// <typeparam name="TResult">Result Type</typeparam>
        /// <param name="func">Async Task</param>
        /// <returns>Async Task's Result</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return TaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Run an Async task Synchronously.
        /// </summary>
        /// <param name="func">Async Task</param>
        public static void RunSync(Func<Task> func)
        {
            TaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}