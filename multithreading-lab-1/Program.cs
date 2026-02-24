/*
 * Лабораторна робота ЛР1 Варіант 8
 * F1: A  = B * (MA * MD) * d
 * F2: MF = MAX(MG) * (MH * MK)
 * F3: T  = P * MO + (S * (MR * MS))

 * Комаров Максим ІМ-31
 * Дата 18.02.2026
*/

using System.Diagnostics;
using System.Numerics;
using System.Text;


internal static class Data
{
    public static bool AutomaticFill;
    public static bool OutputResults;
    private static readonly Random random = new();

    public class Matrix<T>(int rows, int columns) where T : INumber<T>
    {
        private readonly T[] Elements = (rows > 0 && columns > 0) ? new T[rows * columns]
            : throw new ArgumentOutOfRangeException("Matrix dimensions must be positive.");

        public int Rows { get; } = rows;
        public int Columns { get; } = columns;

        public Span<T> GetRowSpan(int row) => Elements.AsSpan(row * Columns, Columns);

        public static Matrix<T> operator +(Matrix<T> left, Matrix<T> right)
        {
            if (left.Rows != right.Rows || left.Columns != right.Columns)
            {
                throw new ArgumentException("Matrices must have the same dimensions.");
            }

            Matrix<T> result = new(left.Rows, left.Columns);
            for (int i = 0; i < left.Rows * left.Columns; i++)
            {
                result.Elements[i] = left.Elements[i] + right.Elements[i];
            }

            return result;
        }

        public static Matrix<T> operator *(Matrix<T> left, Matrix<T> right)
        {
            if (left.Columns != right.Rows)
            {
                throw new ArgumentException("Left column count must equal right row count.");
            }

            Matrix<T> result = new(left.Rows, right.Columns);

            for (int i = 0; i < left.Rows; i++)
            {
                Span<T> resultRow = result.GetRowSpan(i);
                ReadOnlySpan<T> leftRow = left.GetRowSpan(i);

                for (int k = 0; k < left.Columns; k++)
                {
                    T scalar = leftRow[k];
                    if (scalar == T.Zero)
                    {
                        continue;
                    }

                    ReadOnlySpan<T> rightRow = right.GetRowSpan(k);

                    for (int j = 0; j < right.Columns; j++)
                    {
                        resultRow[j] += scalar * rightRow[j];
                    }
                }
            }

            return result;
        }

        public static Matrix<T> operator *(Matrix<T> matrix, T scalar)
        {
            Matrix<T> result = new(matrix.Rows, matrix.Columns);
            for (int i = 0; i < matrix.Rows * matrix.Columns; i++)
            {
                result.Elements[i] = matrix.Elements[i] * scalar;
            }

            return result;
        }

        public static Matrix<T> operator *(T scalar, Matrix<T> matrix) => matrix * scalar;

        public T Max()
        {
            T max = Elements[0];
            for (int i = 1; i < Rows * Columns; i++)
            {
                if (Elements[i] > max)
                {
                    max = Elements[i];
                }
            }

            return max;
        }

        public static Matrix<T> Random(int rows, int columns, int min, int max)
        {
            Matrix<T> result = new(rows, columns);
            for (int i = 1; i < rows * columns; i++)
            {
                result.Elements[i] = T.CreateChecked(random.Next(min, max));
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new(Rows * Columns * 4);

            for (int y = 0; y < Rows; y++)
            {
                ReadOnlySpan<T> row = GetRowSpan(y);

                if (y > 0)
                {
                    sb.AppendLine();
                }

                for (int x = 0; x < Columns; x++)
                {
                    if (x > 0)
                    {
                        sb.Append('\t');
                    }
                    sb.Append(row[x]);
                }
            }

            return sb.ToString();
        }
    }

    private static bool TryParseRow<T>(string input, Span<T> row) where T : INumber<T>, IParsable<T>
    {
        string[] values = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (values.Length != row.Length)
        {
            Console.WriteLine($"Очікувалось {row.Length} значень, але отримано {values.Length}.");
            return false;
        }

        for (int i = 0; i < row.Length; i++)
        {
            if (!T.TryParse(values[i], null, out T? value))
            {
                Console.WriteLine($"Невірне значення '{values[i]}'.");
                return false;
            }

            row[i] = value;
        }

        return true;
    }

    public static Matrix<T> FillMatrix<T>(int rows, int columns, string name) where T : INumber<T>, IParsable<T>
    {
        if (AutomaticFill)
        {
            return Matrix<T>.Random(rows, columns, -20, 20);
        }

        if (rows == 1)
        {
            Console.WriteLine($"Введіть вектор {name} ({columns} елементів):");
        }
        else
        {
            Console.WriteLine($"Введіть матрицю {name} ({rows} рядків по {columns} елементів):");
        }

        Matrix<T> matrix = new(rows, columns);

        int row = 0;
        while (row < rows)
        {
            Console.Write($"{name}[{row}]: ");
            string? input = Console.ReadLine();

            if (input is null)
            {
                continue;
            }

            Span<T> rowSpan = matrix.GetRowSpan(row);
            if (TryParseRow(input, rowSpan))
            {
                row++;
            }
        }

        return matrix;
    }

    public static T ParseValue<T>(string name) where T : INumber<T>, IParsable<T>
    {
        while (true)
        {
            Console.Write($"Введіть значення {name}: ");
            string? input = Console.ReadLine();

            if (input is null)
            {
                continue;
            }

            if (T.TryParse(input, null, out T? value))
            {
                return value;
            }

            Console.WriteLine($"Невірне значення '{input}'.");
        }
    }

    public static T FillValue<T>(string name) where T : INumber<T>, IParsable<T>
    {
        if (AutomaticFill)
        {
            return T.CreateChecked(random.Next(-20, 20));
        }

        return ParseValue<T>(name);
    }
}


public abstract class ALabThread
{
    protected readonly int ID;

    protected readonly Thread thread;

    protected abstract void ReadInput();
    protected abstract void ComputeFunction();
    protected abstract void WriteOutput();

    protected ALabThread(int id)
    {
        ID = id;
        thread = new(Main)
        {
            Priority = ThreadPriority.AboveNormal,
        };
    }

    private void Main()
    {
        Console.WriteLine($"[T{ID}] почав виконання");

        Console.WriteLine($"[T{ID}] почав ввід даних");
        ReadInput();
        Console.WriteLine($"[T{ID}] закінчив ввід даних");

        Console.WriteLine($"[T{ID}] почав обчислення функції F{ID}");
        ComputeFunction();
        Console.WriteLine($"[T{ID}] закінчив обчислення функції F{ID}");

        if (Data.OutputResults)
        {
            Console.WriteLine($"[T{ID}] почав вивід даних");
            WriteOutput();
            Console.WriteLine($"[T{ID}] закінчив вивід даних");
        }

        Console.WriteLine($"[T{ID}] закінчив виконання");
    }

    public void Start() => thread.Start();

    public void Join() => thread.Join();
}


internal class Program
{
    private static int N;

    private class T1() : ALabThread(1)
    {
        private Data.Matrix<int>? A, B, MA, MD;
        private int d;

        // Введення
        protected override void ReadInput()
        {
            B = Data.FillMatrix<int>(1, N, nameof(B));
            MA = Data.FillMatrix<int>(N, N, nameof(MA));
            MD = Data.FillMatrix<int>(N, N, nameof(MD));
            d = Data.FillValue<int>(nameof(d));
        }

        // Обчислення F1
        protected override void ComputeFunction()
        {
            A = B! * (MA! * MD!) * d;
        }

        // Виведення
        protected override void WriteOutput()
        {
            Console.WriteLine($"[T{ID}] Результат: A =\n{A}");
        }
    }

    private class T2() : ALabThread(2)
    {
        private Data.Matrix<int>? MF, MG, MH, MK;

        // Введення
        protected override void ReadInput()
        {
            MG = Data.FillMatrix<int>(N, N, nameof(MG));
            MH = Data.FillMatrix<int>(N, N, nameof(MH));
            MK = Data.FillMatrix<int>(N, N, nameof(MK));
        }

        // Обчислення F2
        protected override void ComputeFunction()
        {
            MF = MG!.Max() * (MH! * MK!);
        }

        // Виведення
        protected override void WriteOutput()
        {
            Console.WriteLine($"[T{ID}] Результат: MF =\n{MF}");
        }
    }

    private class T3() : ALabThread(3)
    {
        private Data.Matrix<int>? T, P, MO, S, MR, MS;

        // Введення
        protected override void ReadInput()
        {
            P = Data.FillMatrix<int>(1, N, nameof(P));
            MO = Data.FillMatrix<int>(N, N, nameof(MO));
            S = Data.FillMatrix<int>(1, N, nameof(S));
            MR = Data.FillMatrix<int>(N, N, nameof(MR));
            MS = Data.FillMatrix<int>(N, N, nameof(MS));
        }

        // Обчислення F3
        protected override void ComputeFunction()
        {
            T = P! * MO! + (S! * (MR! * MS!));
        }

        // Виведення
        protected override void WriteOutput()
        {
            Console.WriteLine($"[T{ID}] Результат: T =\n{T}");
        }
    }

    private static void Lab1()
    {
        Console.OutputEncoding = new UTF8Encoding();

        N = (int)Data.ParseValue<uint>(nameof(N));
        Data.AutomaticFill = N > 5;
        Data.OutputResults = N <= 15;

        Console.WriteLine("\nНатисніть будь-яку клавішу, щоб почати виконання програми\n");
        Console.ReadKey(true);

        Console.WriteLine("[Lab1] почав виконання");
        Stopwatch stopwatch = Stopwatch.StartNew();

        ALabThread[] threads = [new T1(), new T2(), new T3()];

        foreach (ALabThread thread in threads)
        {
            thread.Start();
        }
        foreach (ALabThread thread in threads)
        {
            thread.Join(); 
        }

        double elapsed = stopwatch.Elapsed.TotalSeconds;
        Console.WriteLine($"[Lab1] закінчив виконання");

        if (Data.AutomaticFill)
        {
            Console.WriteLine($"Час виконання: {elapsed:0.###} секунд");
        }
    }

    private static void Main()
    {
        Lab1();
    }
}
