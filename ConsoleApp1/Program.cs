using System;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            Func<int, int, int> multiplier = (a, b) => a*b;
            Func<int, int, int> adder = (a, b) => a + b;
            Func<int> randomizer = () => random.Next(1, 9);

            var m1 = new Matrix<int>(471, 236, multiplier, adder, 0, randomizer);
            var m2 = new Matrix<int>(236, 862, multiplier, adder, 0, randomizer);

            Matrix<int> r;

            //Console.WriteLine(m1.Description);
            //Console.WriteLine("=======");
            //Console.WriteLine(m2.Description);

            //Console.WriteLine("=======");
            r = m1.MultiplyIterative(m2);
            //Console.WriteLine(r.Description);

            //Console.WriteLine("=======");
            r = m1.MultiplyRecursiveBox(m2, 2, 2);
            r = m1.MultiplyRecursiveBox(m2, 4, 4);
            r = m1.MultiplyRecursiveBox(m2, 10, 10);
            r = m1.MultiplyRecursiveBox(m2, 100, 100);
            r = m1.MultiplyRecursiveBox(m2, 200, 200);
            r = m1.MultiplyRecursiveBox(m2, 1000, 1000);
            //Console.WriteLine(r.Description);

            //Console.WriteLine("=======");
            r = m1.MultiplyRecursiveVector(m2);
            //Console.WriteLine(r.Description);
        }
    }
}
