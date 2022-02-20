using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace CsSortBenchmark;

public static class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<Benchmark>();
        Console.WriteLine(summary);
    }
}