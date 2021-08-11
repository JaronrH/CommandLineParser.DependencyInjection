using System.Threading.Tasks;

namespace CommandLineParser.DependencyInjection.Interfaces
{
    /// <summary>
    /// Execute Command Line Asynchronously.
    /// </summary>
    /// <typeparam name="TCommandLineOptions">Command Line Options this executor handles.</typeparam>
    /// <typeparam name="TResult">Results</typeparam>
    public interface IExecuteCommandLineOptionsAsync<in TCommandLineOptions, TResult> where TCommandLineOptions : ICommandLineOptions
    {
        /// <summary>
        /// Execute Command Asynchronously.
        /// </summary>
        /// <param name="options">Command Line Options</param>
        /// <returns>Result</returns>
        Task<TResult> ExecuteAsync(TCommandLineOptions options);
    }
}