using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using MicrosoftCastle.ConsoleApp;

namespace MicrosoftCastle.Samples.Common
{
    public class SampleService : ISampleService
    {
        private readonly ILogger<SampleService> _logger;

        public SampleService(ILogger<SampleService> logger)
        {
            _logger = logger;
        }

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
        [CatchLoggingInterceptor]
        Task<string> ShowAsync();
    }
}
