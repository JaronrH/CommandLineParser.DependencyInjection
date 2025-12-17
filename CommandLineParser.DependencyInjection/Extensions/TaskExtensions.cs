using System.Reflection;
using System.Threading.Tasks;

namespace CommandLineParser.DependencyInjection.Extensions;

public static class TaskExtensions
{
    /// <summary>
    /// Invoke Async Method.
    /// </summary>
    /// <typeparam name="TResult">Result Type</typeparam>
    /// <param name="this">Method Info</param>
    /// <param name="obj">Class method is being invoked on</param>
    /// <param name="parameters">Method Parameters to use</param>
    /// <returns>Result</returns>
    public static async Task<TResult> InvokeAsync<TResult>(this MethodInfo @this, object obj, params object[] parameters)
    {
        var awaitable = (Task<TResult>)@this.Invoke(obj, parameters);
        await awaitable;
        return awaitable.GetAwaiter().GetResult();
    }
}