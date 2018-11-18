using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public interface IMatrix<T>
    {
        int Rows { get; }
        int Cols { get; }
        T this[int r, int c] { get; set; }
        string Description { get; }

        SubMatrix<T> Sub(int fromRow, int rows, int fromCol, int cols);
    }

    /// <summary>
    /// Need to pass Randomizer, Multiplier, Adder and Zero lambda to constuctor 
    /// as there is NO SUPPORT FOR INumeric or similar in C# Generic
    /// https://stackoverflow.com/questions/32664/is-there-a-constraint-that-restricts-my-generic-method-to-numeric-types
    /// like in C++ https://stackoverflow.com/questions/44848011/c-limit-template-type-to-numbers or other languages
    ///  i.e.   public class Matrix<T>
    ///           where T : INumeric<T> ????  ..... like  IComparable<T>, IEquatable<T>
    /// </summary>
    public class Matrix<T> : IMatrix<T>
    {
        internal readonly T[] buffer;
        internal readonly Func<T, T, T> multiplier;
        internal readonly Func<T, T, T> adder;
        internal readonly T Zero;

        public int Rows { get; }
        public int Cols { get; }

        public Matrix(int rows, int cols, Func<T, T, T> multiplier, Func<T, T, T> adder, T zero, Func<T> randomizer = null)
        {
            Rows = rows;
            Cols = cols;
            buffer = new T[rows * cols];
            this.multiplier = multiplier;
            this.adder = adder;
            Zero = zero;

            if (randomizer != null)
            {
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Cols; c++)
                    {
                        buffer[r * Cols + c] = randomizer();
                    }
                }
            }
        }

        public T this[int r, int c]
        {
            get
            {
                return buffer[r * Cols + c];
            }
            set
            {
                buffer[r * Cols + c] = value;
            }
        }

        public string Description
        {
            get
            {
                return Sub(0, Rows, 0, Cols).Description;
            }
        }

        public SubMatrix<T> Sub(int fromRow, int rows, int fromCol, int cols)
        {
            //TODO GUARD from & to in the range
            return new SubMatrix<T>(this, fromRow, rows, fromCol, cols);
        }

        public Matrix<T> MultiplyIterative(Matrix<T> op)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (Cols != op.Rows)
            {
                throw new System.Exception("Incompatible Matixes");
            }

            var result = new Matrix<T>(Rows, op.Cols, multiplier, adder, Zero);

            var tasks = new List<Task>();

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < op.Cols; col++)
                {
                    int c = col;
                    int r = row;
                    tasks.Add(Task.Factory.StartNew(() => {
                        T t = Zero;
                        for (int i = 0; i < Cols; i++)
                        {
                            t = adder(t, multiplier(this[r, i], op[i, c])); //t += this[r, i] * op[i, c];
                        }
                        result[r, c] = t;
                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"MultiplyIterative tasks: {tasks.Count} time: {elapsedMs}");

            return result;
        }

        public Matrix<T> MultiplyRecursiveVector(Matrix<T> op)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (Cols != op.Rows)
            {
                throw new System.Exception("Incompatible Matixes");
            }

            var result = new Matrix<T>(Rows, op.Cols, multiplier, adder, Zero);

            var tasks = new List<Task>();

            Sub(0, Rows, 0, Cols).MultiplyRecursiveVector(tasks,
                                                op.Sub(0, op.Rows, 0, op.Cols),
                                                result.Sub(0, Rows, 0, op.Cols));

            Task.WaitAll(tasks.ToArray());
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"MultiplyRecursiveVector tasks: {tasks.Count} time: {elapsedMs}");

            return result;
        }

        public Matrix<T> MultiplyRecursiveBox(Matrix<T> op, int rows = 2, int cols = 2)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (Cols != op.Rows)
            {
                throw new System.Exception("Incompatible Matixes");
            }

            var result = new Matrix<T>(Rows, op.Cols, multiplier, adder, Zero);

            var tasks = new List<Task>();

            Sub(0, Rows, 0, Cols).MultiplyRecursiveBox(tasks,
                                                op.Sub(0, op.Rows, 0, op.Cols),
                                                result.Sub(0, Rows, 0, op.Cols),
                                                rows, cols);

            Task.WaitAll(tasks.ToArray());
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"MultiplyRecursiveBox({rows}x{cols}) tasks: {tasks.Count} time: {elapsedMs}");

            return result;
        }
    }

    public class SubMatrix<T> : IMatrix<T>
    {
        private readonly Matrix<T> matrix;
        private readonly int fromRow;
        private readonly int fromCol;

        public int Rows { get; }
        public int Cols { get; }

        public T this[int r, int c]
        {
            get
            {
                return matrix.buffer[((r + fromRow) * matrix.Cols) + c + fromCol];
            }
            set
            {
                matrix.buffer[((r + fromRow) * matrix.Cols) + c + fromCol] = value;
            }
        }

        public string Description
        {
            get
            {
                var descr = new StringBuilder();

                for (int r = fromRow; r < fromRow + Rows; r++)
                {
                    var row = new StringBuilder();
                    for (int c = fromCol; c < fromCol + Cols; c++)
                    {
                        row.Append($"{matrix.buffer[r * matrix.Cols + c]} ");
                    }

                    descr.Append($"{row} \r\n");
                }

                return descr.ToString();
            }
        }


        public SubMatrix(Matrix<T> matrix, int fromRow, int rows, int fromCol, int cols)
        {
            this.fromRow = fromRow;
            this.fromCol = fromCol;
            this.Rows = rows;
            this.Cols = cols;
            this.matrix = matrix;
        }

        public SubMatrix<T> Sub(int fromRow, int rows, int fromCol, int cols)
        {
            //TODO GUARD from & to in the range
            return new SubMatrix<T>(matrix, fromRow, rows, fromCol, cols);
        }

        internal void MultiplyRecursiveVector(List<Task> tasks, SubMatrix<T> op, SubMatrix<T> result)
        {
            if (Rows > 1)
            {
                var half = Rows / 2;
                Sub(fromRow, half, fromCol, Cols).MultiplyRecursiveVector(tasks, op, result.Sub(result.fromRow, half, result.fromCol, result.Cols));
                Sub(fromRow + half, Rows - half, fromCol, Cols).MultiplyRecursiveVector(tasks, op, result.Sub(result.fromRow + half, result.Rows - half, result.fromCol, result.Cols));
            }
            else if (op.Cols > 1)
            {
                var half = op.Cols / 2;
                MultiplyRecursiveVector(tasks, op.Sub(op.fromRow, op.Rows, op.fromCol, half), result.Sub(result.fromRow, result.Rows, result.fromCol, half));
                MultiplyRecursiveVector(tasks, op.Sub(op.fromRow, op.Rows, op.fromCol + half, op.Cols - half), result.Sub(result.fromRow, result.Rows, result.fromCol + half, result.Cols - half));
            }
            else
            {
                tasks.Add(Task.Factory.StartNew(() => {
                    T t = matrix.Zero;
                    for (int i = 0; i < Cols; i++)
                    {
                        t = matrix.adder(t, matrix.multiplier(this[0, i], op[i, 0])); //t += this[0, i] * op[i, 0];
                    }
                    result[0, 0] = t;
                }));
            }
        }

        internal void MultiplyRecursiveBox(List<Task> tasks, SubMatrix<T> op, SubMatrix<T> result, int rows, int cols)
        {
            if (Rows > rows)
            {
                var half = Rows / 2;
                Sub(fromRow, half, fromCol, Cols).MultiplyRecursiveBox(tasks, op, result.Sub(result.fromRow, half, result.fromCol, result.Cols), rows, cols);
                Sub(fromRow + half, Rows - half, fromCol, Cols).MultiplyRecursiveBox(tasks, op, result.Sub(result.fromRow + half, result.Rows - half, result.fromCol, result.Cols), rows, cols);
            }
            else if (op.Cols > cols)
            {
                var half = op.Cols / 2;
                MultiplyRecursiveBox(tasks, op.Sub(op.fromRow, op.Rows, op.fromCol, half), result.Sub(result.fromRow, result.Rows, result.fromCol, half), rows, cols);
                MultiplyRecursiveBox(tasks, op.Sub(op.fromRow, op.Rows, op.fromCol + half, op.Cols - half), result.Sub(result.fromRow, result.Rows, result.fromCol + half, result.Cols - half), rows, cols);
            }
            else
            {
                tasks.Add(Task.Factory.StartNew(() => {
                    for (int row = 0; row < Rows; row++)
                    {
                        for (int col = 0; col < op.Cols; col++)
                        {
                            result[row, col] = matrix.Zero;
                            for (int col2 = 0; col2 < Cols; col2++)
                            {
                                result[row, col] = matrix.adder(result[row, col], matrix.multiplier(this[row, col2], op[col2, col])); //result[row, col] += this[row, col2] * op[col2, col];
                            }
                        }
                    }
                }));
            }
        }
    }
}
