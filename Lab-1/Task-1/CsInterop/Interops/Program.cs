// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;

[DllImport("Functions.dll")]
static extern int SumTwoIntsInterop(int a, int b);

Console.WriteLine(SumTwoIntsInterop(5, 10));