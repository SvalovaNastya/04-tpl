﻿using System;

namespace JapaneseCrossword
{
    class Program
    {
        static void Main(string[] args)
        {
            var crosswordSolver = new CrosswordSolver();
            crosswordSolver.Solve(@"TestFiles\Flower.txt", "output.txt");
        }
    }
}
