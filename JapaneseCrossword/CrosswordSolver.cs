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
            var possibleFill = new bool[crossword.columnsCount];
            var possibleEmpty = new bool[crossword.columnsCount];
            CanArrangeBlock(-1, -1, rowNumber, possibleFill, possibleEmpty);
            for (int i = 0; i < crossword.columnsCount; i++)
            {
                if (possibleEmpty[i] != possibleFill[i])
                    if (possibleEmpty[i])
                        crossword.field[rowNumber, i] = CellStatus.Empty;
                    else
                        crossword.field[rowNumber, i] = CellStatus.Fill;
            }
        }

        public bool CanArrangeBlock(int blockIndex, int startIndex, int rowNumber, bool[] possibleFill, bool[] possibleEmpty)
        {
            var blockLength = 0;
            if (blockIndex != -1)
            {
                blockLength = crossword.numbersInRows[rowNumber][blockIndex];
                for (int i = startIndex; i < startIndex + blockLength; i++)
                {
                    if (crossword.field[rowNumber, i] == CellStatus.Empty)
                        return false;
                }
            }
            if (blockIndex < crossword.numbersInRows[rowNumber].Count - 1)
            {
                bool res = false;
                for (int startNext = startIndex + blockLength + 1;
                    startNext < crossword.columnsCount - crossword.numbersInRows[rowNumber][blockIndex + 1] + 1;
                    startNext++)
                {
                    if(startNext != 0 && crossword.field[rowNumber, startNext - 1] == CellStatus.Fill)
                        break;
                    if (CanArrangeBlock(blockIndex + 1, startNext, rowNumber, possibleFill, possibleEmpty))
                    {
                        res = true;
                        if (blockIndex != -1)
                        {
                            for (int i = startIndex; i < startIndex + blockLength; i++)
                                possibleFill[i] = true;
                            for (int i = startIndex + blockLength; i < startNext; i++)
                                possibleEmpty[i] = true;
                        }
                        else
                        {
                            for (int i = 0; i < startNext; i++)
                                possibleEmpty[i] = true;
                        }
                    }
                }
                return res;
            }
            for(int i = startIndex + blockLength; 
                i < crossword.columnsCount; 
                i++)
                if (crossword.field[rowNumber, i] == CellStatus.Fill)
                    return false;
            for (int i = startIndex; i < startIndex + blockLength; i++)
                possibleFill[i] = true;
            for (int i = startIndex + blockLength; i < crossword.columnsCount; i++)
                possibleEmpty[i] = true;
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