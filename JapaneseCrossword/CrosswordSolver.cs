using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        public Crossword crossword;
        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
            this.crossword = ReadCrosswordFromFile(inputFilePath);
            FillUnknownCells(0);
            throw new NotImplementedException();
        }

        public void FillUnknownCells(int rowNumber)
        {
            for (int i = 0; i < crossword.columnsCount; i++)
            {
                var possibleFill = new bool[crossword.columnsCount];
                var possibleEmpty = new bool[crossword.columnsCount];
                GetAllPossibleArrangements(rowNumber, ref possibleFill, ref possibleEmpty);
                if (possibleEmpty[i] != possibleFill[i])
                    if(possibleEmpty[i])
                        crossword.field[rowNumber, i] = CellStatus.Empty;
                    else
                        crossword.field[rowNumber, i] = CellStatus.Fill;
            }
        }

        public void GetAllPossibleArrangements(int rowNumber, ref bool[] fill, ref bool[] empty)
        {
            for (int i = 0; i < crossword.columnsCount - crossword.numbersInRows[rowNumber][0]; i++)
                CanArrangeBlock(0, i, rowNumber, ref fill, ref empty);
        }

        public bool CanArrangeBlock(int blockIndex, int startIndex, int rowNumber, ref bool[] fill, ref bool[] empty)
        {
            for (int i = startIndex; i < startIndex + crossword.numbersInRows[rowNumber][blockIndex]; i++)
            {
                if (crossword.field[rowNumber, i] == CellStatus.Empty)
                    return false;
            }
            if (blockIndex < crossword.numbersInRows[rowNumber].Count - 1)
            {
                bool res = false;
                for (int startNext = startIndex + crossword.numbersInRows[rowNumber][blockIndex] + 1;
                    startNext < crossword.columnsCount - crossword.numbersInRows[rowNumber][blockIndex + 1] + 1;
                    startNext++)
                {
                    if(crossword.field[rowNumber, startNext - 1] == CellStatus.Fill)
                        break;
                    if (CanArrangeBlock(blockIndex + 1, startNext, rowNumber, ref fill, ref empty))
                    {
                        res = true;
                        for (int i = startIndex; i < startIndex + crossword.numbersInRows[rowNumber][blockIndex]; i++)
                            fill[i] = true;
                        for (int i = startIndex + crossword.numbersInRows[rowNumber][blockIndex]; i < startNext; i++)
                            empty[i] = true;
                    }
                }
                return res;
            }
            for(int i = startIndex + crossword.numbersInRows[rowNumber][blockIndex]; 
                i < crossword.numbersInRows[rowNumber].Count; 
                i++)
                if (crossword.field[rowNumber, i] == CellStatus.Fill)
                    return false;
            for ( int i = startIndex; i < startIndex + crossword.numbersInRows[rowNumber][blockIndex]; i++)
                fill[i] = true;
            for (int i = startIndex + crossword.numbersInRows[rowNumber][blockIndex]; i < crossword.columnsCount; i++)
                empty[i] = true;
            return true;
        }

        public Crossword ReadCrosswordFromFile(string inputFilePath)
        {
            var discriberDict = new Dictionary<string, List<List<int>>>();
            var currentKey = "";
            var listsCount = 0;
            foreach (var line in File.ReadAllLines(inputFilePath))
            {
                var splettedLine = line.Split(' ').ToArray();
                int temp;
                if (!int.TryParse(splettedLine[0], out temp))
                {
                    splettedLine = line.Split(':').ToArray();
                    currentKey = splettedLine[0];
                    discriberDict.Add(currentKey, new List<List<int>>());
                    listsCount = 0;
                }
                else
                {
                    discriberDict[currentKey].Add(new List<int>());
                    listsCount++;
                    foreach (var numb in splettedLine)
                    {
                        discriberDict[currentKey][listsCount - 1].Add(int.Parse(numb));
                    }
                }
            }
            return new Crossword(discriberDict["rows"].Count, discriberDict["columns"].Count, discriberDict["rows"], discriberDict["columns"]);
        }
    }
}