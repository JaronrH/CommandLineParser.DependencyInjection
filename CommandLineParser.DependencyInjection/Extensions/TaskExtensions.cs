using System.Reflection;
using System.Threading.Tasks;

namespace CommandLineParser.DependencyInjection.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<TResult> InvokeAsync<TResult>(this MethodInfo @this, object obj, params object[] parameters)
        {
            var awaitable = (Task<TResult>)@this.Invoke(obj, parameters);
            await awaitable;
            return awaitable.GetAwaiter().GetResult();
        }
    }
}
