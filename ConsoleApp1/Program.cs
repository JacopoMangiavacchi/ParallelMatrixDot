using System;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();

            //var m = new Matrix<int>(4, 2);
            var m1 = new Matrix(4, 2);
            m1.Randomize(() => random.Next(1, 9));
            Console.WriteLine(m1.Description());

            Console.WriteLine("=======");
            var m2 = new Matrix(2, 3);
            m2.Randomize(() => random.Next(1, 9));
            Console.WriteLine(m2.Description());

            Console.WriteLine("=======");
            var m3 = m1.Multiply(m2);
            Console.WriteLine(m3.Description());

            Console.WriteLine("=======");
            var m4 = m3.SubMatrix(1, 2, 1, 2);
            Console.WriteLine(m4.Description());
        }
    }
}
