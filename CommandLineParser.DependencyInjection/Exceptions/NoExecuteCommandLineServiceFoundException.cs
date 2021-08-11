using System;

namespace CommandLineParser.DependencyInjection.Exceptions
{
    /// <summary>
    /// Exception thrown when the Command Line Parser was able to get the Options but there was no service found to handle it.
    /// </summary>
    public class NoExecuteCommandLineServiceFoundException : Exception
    {
        /// <summary>
        /// Create new Exception
        /// </summary>
        /// <param name="optionsType">Options Type</param>
        /// <param name="resultType">Result Type</param>
        /// <param name="isSynchronous">Was <see cref="ICommandLineParser{TResult}"/> run synchronously?</param>
        /// <param name="allowFallback">Was the sync/async allowed to fallback to async/sync?</param>
        public NoExecuteCommandLineServiceFoundException(Type optionsType, Type resultType, bool isSynchronous, bool allowFallback)
        {
            OptionsType = optionsType;
            ResultType = resultType;
            IsSynchronous = isSynchronous;
            AllowFallback = allowFallback;
        }

        /// <summary>
        /// Options Type
        /// </summary>
        public Type OptionsType { get; }

        /// <summary>
        /// Result Type
        /// </summary>
        public Type ResultType { get; }

        /// <summary>
        /// Was <see cref="ICommandLineParser{TResult}"/> run synchronously?
        /// </summary>
        public bool IsSynchronous { get; }

        /// <summary>
        /// Was the sync/async allowed to fallback to async/sync?
        /// </summary>
        public bool AllowFallback { get; }
    }
}
