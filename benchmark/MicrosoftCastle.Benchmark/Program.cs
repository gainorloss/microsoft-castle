using BenchmarkDotNet.Running;

namespace MicrosoftCastle.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run(typeof(ServiceProviderBuildBenchmark));
            var summary = BenchmarkRunner.Run(typeof(ServiceResolveBenchmark));
        }
    }
}
