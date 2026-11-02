using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        int n = 2000;
        int numThreads = 4;
        string mode = "dynamic";

        if (args.Length >= 1) int.TryParse(args[0], out n);
        if (args.Length >= 2) int.TryParse(args[1], out numThreads);
        if (args.Length >= 3) mode = args[2].ToLower();

        Console.WriteLine($"Запуск: N={n}, Потоков={numThreads}, Режим={mode}");

        long[][] a;
        long[,] b;

        using (Process generator = new Process())
        {
            generator.StartInfo.FileName = "dotnet";
            generator.StartInfo.Arguments = $"run --project ../Generator -- {n}";
            generator.StartInfo.UseShellExecute = false;
            generator.StartInfo.RedirectStandardOutput = true; 
            
            a = GenerateLowerTriangularMatrix(n);
            b = GenerateMatrix(n, n);
        }

        long[,] c = new long[n, n];
        int completedRows = 0;
        int nextRowIndex = 0; 

        var sw = Stopwatch.StartNew();

        var progressThread = new Thread(() =>
        {
            while (Volatile.Read(ref completedRows) < n)
            {
                int current = Volatile.Read(ref completedRows);
                double pct = (double)current / n * 100;
                Console.Write($"\rПрогресс: {pct:F1}%");
                Thread.Sleep(200);
            }
            Console.Write("\rПрогресс: 100.0%\n");
        });
        progressThread.Start();

        Thread[] threads = new Thread[numThreads];

        if (mode == "static")
        {
            int chunkSize = n / numThreads;
            int remainder = n % numThreads;
            int startRow = 0;

            for (int t = 0; t < numThreads; t++)
            {
                int rows = chunkSize + (t < remainder ? 1 : 0);
                int start = startRow;
                int end = start + rows;

                threads[t] = new Thread(() =>
                {
                    for (int i = start; i < end; i++)
                    {
                        ComputeRow(i, a, b, c, n);
                        Interlocked.Increment(ref completedRows);
                    }
                });
                threads[t].Start();
                startRow = end;
            }
        }
        else
        {
            for (int t = 0; t < numThreads; t++)
            {
                threads[t] = new Thread(() =>
                {
                    while (true)
                    {
                        int i = Interlocked.Increment(ref nextRowIndex) - 1;
                        if (i >= n) break;

                        ComputeRow(i, a, b, c, n);
                        Interlocked.Increment(ref completedRows);
                    }
                });
                threads[t].Start();
            }
        }

        foreach (var t in threads) t.Join();
        progressThread.Join();
        sw.Stop();

        long checksum = CalculateChecksum(c, n, n);
        Console.WriteLine($"Завершено за: {sw.ElapsedMilliseconds} ms. Контрольная сумма: {checksum}");
    }

    static void ComputeRow(int i, long[][] a, long[,] b, long[,] c, int n)
    {
        int rowLength = a[i].Length; 
        for (int tIdx = 0; tIdx < rowLength; tIdx++)
        {
            long valA = a[i][tIdx];
            for (int j = 0; j < n; j++)
            {
                c[i, j] += valA * b[tIdx, j];
            }
        }
    }

    static long[][] GenerateLowerTriangularMatrix(int n)
    {
        long[][] m = new long[n][];
        Random rnd = new Random(42);
        for (int i = 0; i < n; i++)
        {
            m[i] = new long[i + 1];
            for (int j = 0; j <= i; j++)
                m[i][j] = rnd.Next(0, 10);
        }
        return m;
    }

    static long[,] GenerateMatrix(int rows, int cols)
    {
        long[,] m = new long[rows, cols];
        Random rnd = new Random(42);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                m[i, j] = rnd.Next(0, 10);
        return m;
    }

    static long CalculateChecksum(long[,] matrix, int rows, int cols)
    {
        long sum = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                sum += matrix[i, j];
        return sum;
    }
}