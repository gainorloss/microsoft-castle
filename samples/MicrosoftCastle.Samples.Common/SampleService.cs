using Castle.DynamicProxy;
using MicrosoftCastle.ConsoleApp;

namespace MicrosoftCastle.Samples.Common
{
    public class SampleService : ISampleService
    {
        [CatchLoggingInterceptor]
        [Interceptor(typeof(LoggingInterceptor))]
        public virtual Task<string> ShowAsync()
        {
            Console.WriteLine(nameof(ShowAsync));
            return Task.FromResult(nameof(ShowAsync));
        }
    }

    public interface ISampleService
    {
        //[CatchLoggingInterceptor]
        Task<string> ShowAsync();
    }
}
