using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public interface IMatrix
    {
        int Rows { get; }
        int Cols { get; }
        int this[int r, int c] { get; set; }
        string Description();
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

        public SubMatrix(Matrix matrix, int fromRow, int rows, int fromCol, int cols)
        {
            this.fromRow = fromRow;
            this.fromCol = fromCol;
            this.Rows = rows;
            this.Cols = cols;
            this.matrix = matrix;
        }

        public string Description()
        {
            var descr = new StringBuilder();

            for (int r = fromRow; r < fromRow+Rows; r++)
            {
                var row = new StringBuilder();
                for (int c = fromCol; c < fromCol+Cols; c++)
                {
                    row.Append($"{matrix.Buffer[r * matrix.Cols + c]} ");
                }

                descr.Append($"{row} \r\n");
            }

            return descr.ToString();
        }

        public void Multiply(SubMatrix op, SubMatrix result)
        {
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

        public SubMatrix SubMatrix(int fromRow, int rows, int fromCol, int cols)
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

        public string Description()
        {
            return new SubMatrix(this, 0, Rows, 0, Cols).Description();
        }

        public Matrix Multiply(Matrix op)
        {
            if (this.Cols != op.Rows)
            {
                throw new System.Exception("Incompatible Matixes");
            }

            var result = new Matrix(this.Rows, op.Cols);
            var subResult = result.SubMatrix(0, this.Rows, 0, op.Cols);

            new SubMatrix(this, 0, Rows, 0, Cols).Multiply(op.SubMatrix(0, op.Rows, 0, op.Cols), subResult);

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
