using System;

namespace JapaneseCrossword
{
    class Program
    {
        static void Main(string[] args)
        {
            var crosswordSolver = new CrosswordSolver();
            crosswordSolver.Solve(@"TestFiles\Flower.txt", "uuu");
//            crosswordSolver.Solve(@"TestFiles\SampleInput.txt", "uuu");
//            var a = crosswordSolver.ReadCrosswordFromFile(@"TestFiles\Car.txt");
//            var c = crosswordSolver.ReadCrosswordFromFile(@"TestFiles\Car.txt");
//            Console.ReadKey();
        }
    }
}
