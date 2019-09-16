
namespace CommandLineParser.DependencyInjection.Tests.Services
{
    class DoYouLikeService
    {
        public string DoILikeThis(string thing, bool like)
        {
            return like
                ? $"Yes, I do like {thing}! Thank you, Thank you Sam I Am!"
                : $"I do not like them, Sam I Am! I do not like {thing}.";
        }
    }
}
