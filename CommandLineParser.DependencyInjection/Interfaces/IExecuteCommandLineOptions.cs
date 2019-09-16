namespace CommandLineParser.DependencyInjection.Interfaces
{
    /// <summary>
    /// Execute Command Line Synchronously.
    /// </summary>
    /// <typeparam name="TCommandLineOptions">Command Line Options this executor handles.</typeparam>
    /// <typeparam name="TResult">Results</typeparam>
    public interface IExecuteCommandLineOptions<in TCommandLineOptions, out TResult> where TCommandLineOptions : ICommandLineOptions
    {
        /// <summary>
        /// Execute Command Synchronously.
        /// </summary>
        /// <param name="options">Command Line Options</param>
        /// <returns>Result</returns>
        TResult Execute(TCommandLineOptions options);
    }
}