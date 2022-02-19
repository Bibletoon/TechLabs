package com.bibletoon.sortBenchmark;

import org.openjdk.jmh.annotations.*;
import org.openjdk.jmh.infra.Blackhole;

import java.util.Arrays;
import java.util.Random;
import java.util.concurrent.TimeUnit;

import com.bibletoon.sort.MergeSort;

@BenchmarkMode(Mode.AverageTime)
@Warmup(iterations = 0)
@Fork(value = 1, warmups = 0)
@Measurement(iterations = 1)
@OutputTimeUnit(TimeUnit.NANOSECONDS)
@State(Scope.Benchmark)
public class SortBenchmarks {
    public static int[] arrayForDefaultSort;
    public static int[] arrayForMergeSort;

    @Setup(Level.Iteration)
    public static void Setup() {
        Random r = new Random();
        arrayForDefaultSort = new int[2000000];
        arrayForMergeSort = new int[2000000];
        for (int i=0;i<2000000;i++) {
            arrayForDefaultSort[i] = r.nextInt();
            arrayForMergeSort[i] = r.nextInt();
        }
    }

    @Benchmark
    public void DefaultSort(Blackhole blackhole) {
        Arrays.sort(arrayForDefaultSort);
        blackhole.consume(arrayForDefaultSort);
    }

    @Benchmark
    public void MergeSort(Blackhole blackhole) {
        MergeSort.RecursiveMergeSort(arrayForMergeSort, 2000000);
        blackhole.consume(arrayForMergeSort);
    }
}
