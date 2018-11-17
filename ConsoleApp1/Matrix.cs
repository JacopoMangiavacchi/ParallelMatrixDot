using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public interface IMatrix
    {
        int Rows { get; }
        int Cols { get; }
        int this[int r, int c] { get; set; }
        string Description { get; }

        SubMatrix Sub(int fromRow, int rows, int fromCol, int cols);
    }

    public class SubMatrix : IMatrix
    {
        private readonly Matrix matrix;
        private readonly int fromRow;
        private readonly int fromCol;

        public int Rows { get; }
        public int Cols { get; }

        public int this[int r, int c] {
            get
            {
                return matrix.Buffer[((r + fromRow) * matrix.Cols) + c + fromCol];
            }
            set
            {
                matrix.Buffer[((r + fromRow) * matrix.Cols) + c + fromCol] = value;
            }
        }

        public string Description { 
            get
            {
                var descr = new StringBuilder();

                for (int r = fromRow; r < fromRow + Rows; r++)
                {
                    var row = new StringBuilder();
                    for (int c = fromCol; c < fromCol + Cols; c++)
                    {
                        row.Append($"{matrix.Buffer[r * matrix.Cols + c]} ");
                    }

                    descr.Append($"{row} \r\n");
                }

                return descr.ToString();
            }
        }


        public SubMatrix(Matrix matrix, int fromRow, int rows, int fromCol, int cols)
        {
            this.fromRow = fromRow;
            this.fromCol = fromCol;
            this.Rows = rows;
            this.Cols = cols;
            this.matrix = matrix;
        }

        public SubMatrix Sub(int fromRow, int rows, int fromCol, int cols)
        {
            //TODO GUARD from & to in the range
            return new SubMatrix(this.matrix, fromRow, rows, fromCol, cols);
        }

        internal void Multiply(List<Task> tasks, SubMatrix op, SubMatrix result)
        {
            if (Rows > 1)
            {
                var half = Rows / 2;
                this.Sub(fromRow, half, fromCol, Cols).Multiply(tasks, op, result.Sub(result.fromRow, half, result.fromCol, result.Cols));
                this.Sub(fromRow + half, Rows - half, fromCol, Cols).Multiply(tasks, op, result.Sub(result.fromRow + half, result.Rows - half, result.fromCol, result.Cols));
            }
            else if (op.Cols > 1)
            {
                var half = op.Cols / 2;
                this.Multiply(tasks, op.Sub(op.fromRow, op.Rows, op.fromCol, half), result.Sub(result.fromRow, result.Rows, result.fromCol, half));
                this.Multiply(tasks, op.Sub(op.fromRow, op.Rows, op.fromCol + half, op.Cols - half), result.Sub(result.fromRow, result.Rows, result.fromCol + half, result.Cols - half));
            }
            else {
                tasks.Add(Task.Factory.StartNew(() => {
                    for (int row = 0; row < this.Rows; row++)
                    {
                        for (int col = 0; col < op.Cols; col++)
                        {
                            result[row, col] = 0;
                            for (int col2 = 0; col2 < this.Cols; col2++)
                            {
                                result[row, col] += this[row, col2] * op[col2, col];
                            }
                        }
                    }
                }));
            }
        }
    }



    public class Matrix : IMatrix
    {
        internal readonly int[] Buffer;

        public int Rows { get; }
        public int Cols { get; }

        public Matrix(int rows, int cols)
        {
            this.Rows = rows;
            this.Cols = cols;
            this.Buffer = new int[rows * cols];
        }

        public int this[int r, int c]
        {
            get
            {
                return Buffer[r * Cols + c];
            }
            set
            {
                Buffer[r * Cols + c] = value;
            }
        }

        public string Description
        {
            get
            {
                return this.Sub(0, Rows, 0, Cols).Description;
            }
        }

        public SubMatrix Sub(int fromRow, int rows, int fromCol, int cols)
        {
            //TODO GUARD from & to in the range
            return new SubMatrix(this, fromRow, rows, fromCol, cols);
        }

        public void Randomize(Func<int> randomizer)
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Buffer[r * Cols + c] = randomizer();
                }
            }
        }

        public Matrix Multiply(Matrix op)
        {
            if (this.Cols != op.Rows)
            {
                throw new System.Exception("Incompatible Matixes");
            }

            var result = new Matrix(this.Rows, op.Cols);

            var tasks = new List<Task>();

            this.Sub(0, Rows, 0, Cols).Multiply(tasks,
                                                op.Sub(0, op.Rows, 0, op.Cols), 
                                                result.Sub(0, this.Rows, 0, op.Cols));

            Task.WaitAll(tasks.ToArray());

            return result;
        }
    }

    //// NO SUPPORT FOR INumeric or similar in C# Generic
    //// https://stackoverflow.com/questions/32664/is-there-a-constraint-that-restricts-my-generic-method-to-numeric-types
    //// like in C++ https://stackoverflow.com/questions/44848011/c-limit-template-type-to-numbers
    //public class Matrix<T>
    //    where T : INumeric<T> ????  ..... like  IComparable<T>, IEquatable<T>
    //{
    //    int Rows { get; }
    //    int Cols { get; }

    //    private readonly T[] Buffer;

    //    public Matrix(int rows, int cols)
    //    {
    //        this.Rows = rows;
    //        this.Cols = cols;
    //        this.Buffer = new T[rows * cols];
    //    }

    //    public T this[int r, int c]
    //    {
    //        get
    //        {
    //            return Buffer[r * Cols + c];
    //        }
    //        set
    //        {
    //            Buffer[r * Cols + c] = value;
    //        }
    //    }

    //    public void Randomize(Func<T> randomizer)
    //    {
    //        for (int r=0; r<Rows; r++)
    //        {
    //            for (int c=0; c<Cols; c++)
    //            {
    //                Buffer[r * Cols + c] = randomizer();
    //            }
    //        }
    //    }

    //    public string Description()
    //    {
    //        var descr = new StringBuilder();

    //        for (int r=0; r<Rows; r++)
    //        {
    //            var row = new StringBuilder();
    //            for (int c=0; c<Cols; c++)
    //            {
    //                row.Append($"{Buffer[r * Cols + c]} ");
    //            }

    //            descr.Append($"{row} \r\n");
    //        }

    //        return descr.ToString();
    //    }

    //    public Matrix<T> Multiply(Matrix<T> op, T zero)
    //    {
    //        if (this.Cols != op.Rows)
    //        {
    //            throw new System.Exception("Incompatible Matixes");
    //        }

    //        var c = new Matrix<T>(this.Rows, op.Cols);

    //        for (int row=0; row<this.Rows; row++)
    //        {
    //            for (int col=0; col<op.Cols; col++)
    //            {
    //                c[row, col] = zero;
    //                for (int col2 = 0; col2 < this.Cols; col2++)
    //                {
    //                    c[row, col] += this[row, col2] * op[col2, col];
    //                }
    //            }
    //        }

    //        return c;
    //    }
    //}
}
