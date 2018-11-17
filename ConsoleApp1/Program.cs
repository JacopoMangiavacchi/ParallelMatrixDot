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
            //var m1 = new Matrix(4, 2);
            //var m2 = new Matrix(2, 3);
            var m1 = new Matrix(47, 23);
            var m2 = new Matrix(23, 32);

            m1.Randomize(() => random.Next(1, 9));
            m2.Randomize(() => random.Next(1, 9));

            Console.WriteLine(m1.Description);

            Console.WriteLine("=======");
            Console.WriteLine(m2.Description);

            Console.WriteLine("=======");
            Console.WriteLine(m1.MultiplyIterative(m2).Description);

            Console.WriteLine("=======");
            Console.WriteLine(m1.MultiplyRecursiveBox(m2).Description);

            Console.WriteLine("=======");
            Console.WriteLine(m1.MultiplyRecursiveVector(m2).Description);
        }
    }
}
