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
    public static long defaultSortMemory = 0;
    public static long mergeSortMemory = 0;

    @Setup(Level.Invocation)
    public static void Setup() {
        Random r = new Random();
        arrayForDefaultSort = new int[2000000];
        arrayForMergeSort = new int[2000000];
        for (int i=0;i<2000000;i++) {
            arrayForDefaultSort[i] = r.nextInt();
            arrayForMergeSort[i] = r.nextInt();
        }

        defaultSortMemory = (Runtime.getRuntime().totalMemory()-Runtime.getRuntime().freeMemory());
        mergeSortMemory = (Runtime.getRuntime().totalMemory()-Runtime.getRuntime().freeMemory());
    }

    @Benchmark
    public void DefaultSort(Blackhole blackhole) {
        Arrays.sort(arrayForDefaultSort);
        defaultSortMemory = (Runtime.getRuntime().totalMemory()-Runtime.getRuntime().freeMemory()) - defaultSortMemory;
        blackhole.consume(arrayForDefaultSort);
    }

    @Benchmark
    public void MergeSort(Blackhole blackhole) {
        MergeSort.RecursiveMergeSort(arrayForMergeSort, 2000000);
        mergeSortMemory = (Runtime.getRuntime().totalMemory()-Runtime.getRuntime().freeMemory()) - mergeSortMemory;
        blackhole.consume(arrayForMergeSort);
    }

    @TearDown(Level.Iteration)
    public static void TearDown() {
        System.out.println("Default sort took "+defaultSortMemory+" B memory");
        System.out.println("Merge sort took "+mergeSortMemory+" B memory");
    }
}
