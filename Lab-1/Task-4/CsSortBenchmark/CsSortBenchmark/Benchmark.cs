using BenchmarkDotNet.Attributes;

namespace CsSortBenchmark;

public class Benchmark
{
    private int[] _arrayForDefaultSort;
    private int[] _arrayForMergeSort;
    
    [IterationSetup]
    public void Setup()
    {
        _arrayForDefaultSort = new int[2000000];
        _arrayForMergeSort = new int[2000000];
        Random r = new Random();

        for (int i = 0; i < 2000000; i++)
        {
            _arrayForDefaultSort[i] = r.Next();
            _arrayForMergeSort[i] = r.Next();
        }
    }

    [Benchmark]
    public void DefaultSort()
    {
        Array.Sort(_arrayForDefaultSort);
    }

    [Benchmark]
    public void MergeSort()
    {
        _arrayForMergeSort = MergeSortAlgorithm.MergeSort(_arrayForMergeSort);
    }
}