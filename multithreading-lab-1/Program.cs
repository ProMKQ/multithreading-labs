using System.Numerics;
using System.Text;

static void Lab1()
{

}

static class Data
{
    public class Matrix<T>(int rows, int columns)
        where T : INumber<T>
    {
        private readonly T[] Elements = (rows > 0 && columns > 0) ? new T[rows * columns]
            : throw new ArgumentOutOfRangeException("Matrix dimensions must be positive.");

        public int Rows { get; } = rows;
        public int Columns { get; } = columns;

        public T this[int row, int col]
        {
            get => Elements[row * Columns + col];
            set => Elements[row * Columns + col] = value;
        }

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
                throw new ArgumentException("Left columns must equal right rows.");
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
