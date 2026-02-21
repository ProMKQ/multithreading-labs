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


static class Data
{
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

        public static T Max(Matrix<T> matrix)
        {
            T max = matrix.Elements[0];
            for (int i = 1; i < matrix.Rows * matrix.Columns; i++)
            {
                if (matrix.Elements[i] > max)
                {
                    max = matrix.Elements[i];
                }
            }

            return max;
        }

        public override string ToString()
        {
            StringBuilder sb = new(Rows * Columns * 4);

            for (int y = 0; y < Rows; y++)
            {
                ReadOnlySpan<T> row = GetRowSpan(y);

                for (int x = 0; x < Columns; x++)
                {
                    sb.Append(row[x]);
                    if (x < Columns - 1)
                    {
                        sb.Append('\t');
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
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

    public static Matrix<T> ParseMatrixFromConsole<T>(int rows, int columns, string name) where T : INumber<T>, IParsable<T>
    {
        if (rows == 1)
        {
            Console.WriteLine($"Введіть вектор {name} ({columns} елементів):");
        }
        else
        {
            Console.WriteLine($"Введіть матрицю {name} ({rows} рядків по {columns} елементів):");
        }

        Matrix<T> matrix = new(rows, columns);

        int i = 0;
        while (i < rows)
        {
            Console.Write($"{name}[{i + 1}]: ");
            string? input = Console.ReadLine();
            if (input is null)
            {
                continue;
            }

            Span<T> row = matrix.GetRowSpan(i);
            if (TryParseRow(input, row))
            {
                i++;
            }
        }

        return matrix;
    }

    public static T ParseScalarFromConsole<T>(string name) where T : INumber<T>, IParsable<T>
    {
        while (true)
        {
            Console.Write($"Введіть скаляр {name}: ");
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
            Priority = ThreadPriority.Highest
        };
    }

    private void Main()
    {
        Console.WriteLine($"[T{ID}] почав виконання");

        Console.WriteLine($"[T{ID}] почав ввід даних");
        ReadInput();
        Console.WriteLine($"[T{ID}] завершив ввід даних");

        Console.WriteLine($"[T{ID}] почав обчислення функції F{ID}");
        ComputeFunction();
        Thread.Sleep(1000);
        Console.WriteLine($"[T{ID}] завершив обчислення функції F{ID}");

        Console.WriteLine($"[T{ID}] почав вивід даних");
        WriteOutput();
        Console.WriteLine($"[T{ID}] завершив вивід даних");

        Console.WriteLine($"[T{ID}] завершив виконання");
    }

    public void Start() => thread.Start();

    public void Join() => thread.Join();
}


internal class Program
{
    const int N = 3;

    class T1() : ALabThread(1)
    {
        private Data.Matrix<int>? A;
        private Data.Matrix<int>? B;
        private Data.Matrix<int>? MA;
        private Data.Matrix<int>? MD;
        private int d;

        // Введення
        protected override void ReadInput()
        {
            B = Data.ParseMatrixFromConsole<int>(1, N, nameof(B));
            MA = Data.ParseMatrixFromConsole<int>(N, N, nameof(MA));
            MD = Data.ParseMatrixFromConsole<int>(N, N, nameof(MD));
            d = Data.ParseScalarFromConsole<int>(nameof(d));
        }

        // Обчислення
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

    class T2() : ALabThread(2)
    {
        private Data.Matrix<int>? MF;
        private Data.Matrix<int>? MG;
        private Data.Matrix<int>? MH;
        private Data.Matrix<int>? MK;

        // Введення
        protected override void ReadInput()
        {
            MG = Data.ParseMatrixFromConsole<int>(N, N, nameof(MG));
            MH = Data.ParseMatrixFromConsole<int>(N, N, nameof(MH));
            MK = Data.ParseMatrixFromConsole<int>(N, N, nameof(MK));
        }

        // Обчислення
        protected override void ComputeFunction()
        {
            MF = Data.Matrix<int>.Max(MG!) * (MH! * MK!);
        }

        // Виведення
        protected override void WriteOutput()
        {
            Console.WriteLine($"[T{ID}] Результат: MF =\n{MF}");
        }
    }

    class T3() : ALabThread(3)
    {
        private Data.Matrix<int>? T;
        private Data.Matrix<int>? P;
        private Data.Matrix<int>? MO;
        private Data.Matrix<int>? S;
        private Data.Matrix<int>? MR;
        private Data.Matrix<int>? MS;

        // Введення
        protected override void ReadInput()
        {
            P = Data.ParseMatrixFromConsole<int>(1, N, nameof(P));
            MO = Data.ParseMatrixFromConsole<int>(N, N, nameof(MO));
            S = Data.ParseMatrixFromConsole<int>(1, N, nameof(S));
            MR = Data.ParseMatrixFromConsole<int>(N, N, nameof(MR));
            MS = Data.ParseMatrixFromConsole<int>(N, N, nameof(MS));
        }

        // Обчислення
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

    static void Lab1()
    {
        Console.OutputEncoding = new UTF8Encoding();

        Console.WriteLine("Натисніть будь-яку клавішу, щоб почати виконання програми\n");
        Console.ReadKey(true);

        Console.WriteLine("[Lab1] почав виконання");
        Stopwatch stopwatch = Stopwatch.StartNew();

        T1 t1 = new();
        T2 t2 = new();
        T3 t3 = new();

        t1.Start();
        t2.Start();
        t3.Start();

        t1.Join();
        t2.Join();
        t3.Join();

        double elapsed = stopwatch.Elapsed.TotalSeconds;
        Console.WriteLine($"[Lab1] закінчив виконання після {elapsed:0.###} секунд");
    }

    private static void Main()
    {
        Lab1();
    }
}
