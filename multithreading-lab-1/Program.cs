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

const int N = 3;

Lab1();

static void Lab1()
{
    Console.WriteLine("Натисніть будь-яку клавішу, щоб почати виконання програми\n");
    Console.ReadKey(true);

    Console.WriteLine("Потік Lab1 почав виконання");
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
    Console.WriteLine($"Потік Lab1 закінчив виконання після {elapsed:0.###} секунд");
}


class T1() : Data.ALabThread(1)
{

    // Введення
    protected override void ReadInput()
    {
        
    }

    // Обчислення
    protected override void ComputeFunction()
    {
        // A = B * (MA * MD) * d
    }

    // Виведення
    protected override void WriteOutput()
    {

    }
}

class T2() : Data.ALabThread(2)
{

    // Введення
    protected override void ReadInput()
    {

    }

    // Обчислення
    protected override void ComputeFunction()
    {
        // MF = MAX(MG) * (MH * MK)
    }

    // Виведення
    protected override void WriteOutput()
    {

    }
}

class T3() : Data.ALabThread(3)
{

    // Введення
    protected override void ReadInput()
    {

    }

    // Обчислення
    protected override void ComputeFunction()
    {
        // T = P * MO + (S * (MR * MS))
    }

    // Виведення
    protected override void WriteOutput()
    {

    }
}


static class Data
{
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
            Console.WriteLine($"Потік T{ID} почав виконання");

            Console.WriteLine($"Потік T{ID} почав ввід даних");
            ReadInput();
            Console.WriteLine($"Потік T{ID} завершив ввід даних");

            Console.WriteLine($"Потік T{ID} почав обчислення функції F{ID}");
            ComputeFunction();
            Console.WriteLine($"Потік T{ID} завершив обчислення функції F{ID}");

            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.WriteLine($"Потік T{ID} почав вивід даних");
            WriteOutput();
            Console.WriteLine($"Потік T{ID} завершив вивід даних");

            Console.WriteLine($"Потік T{ID} завершив виконання");
        }

        public void Start() => thread.Start();

        public void Join() => thread.Join();
    }


    public static Matrix<T> ParseMatrixFromConsole<T>(int rows, int columns, string name)
        where T : INumber<T>, IParsable<T>
    {
        Console.WriteLine($"Введіть матрицю {name} ({rows}x{columns}):");

        Matrix<T> matrix = new(rows, columns);

        int i = 0;
        while (i < rows)
        {
            Console.Write($"{name}[{i + 1}]: ");
            string? input = Console.ReadLine();
            if (input is null)
            {
                break;
            }

            string[] values = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != columns)
            {
                Console.WriteLine($"Очікувалось {columns} значень, але отримано {values.Length}.");
                continue;
            }

            Span<T> row = matrix.GetRowSpan(i);
            for (int j = 0; j < columns; j++)
            {
                if (!T.TryParse(values[j], null, out T? value))
                {
                    Console.WriteLine($"Невірний {nameof(T)} '{values[j]}'.");
                    continue;
                }
                row[j] = value;
            }
        }

        return matrix;
    }

    public class Matrix<T>(int rows, int columns)
        where T : INumber<T>
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
            int elementCount = left.Rows * left.Columns;

            for (int i = 0; i < elementCount; i++)
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

        public static Matrix<T> Max(Matrix<T> left, Matrix<T> right)
        {
            if (left.Rows != right.Rows || left.Columns != right.Columns)
            {
                throw new ArgumentException("Matrices must have the same dimensions.");
            }

            Matrix<T> result = new(left.Rows, left.Columns);
            int elementCount = left.Rows * left.Columns;

            for (int i = 0; i < elementCount; i++)
            {
                result.Elements[i] = T.Max(left.Elements[i], right.Elements[i]);
            }

            return result;
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

            return sb.ToString();
        }
    }
}
