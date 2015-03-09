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
            for (int i = 0; i < crossword.columnsCount; i++)
                SetNewPosition(i, false);
            for (int i = 0; i < crossword.field.GetLength(0); i++)
            {
                for (int j = 0; j < crossword.field.GetLength(1); j++)
                    if (crossword.field[i, j] == CellStatus.Fill)
                        Console.Write('█');
                    else if (crossword.field[i, j] == CellStatus.Empty)
                        Console.Write('*');
                    else
                        Console.Write('.')
                ;
                Console.WriteLine();
            }
            throw new NotImplementedException();
        }

        public void SetNewPosition(int rowNumber, bool isColumn)
        {
            if (isColumn)
            {
                var row = new CellStatus[crossword.rowsCount];
                for (int i = 0; i < crossword.rowsCount; i++)
                    row[i] = crossword.field[i, rowNumber];
                var rowNumbers = new int[crossword.numbersInColumns[rowNumber].Count];
                for (int i = 0; i < crossword.numbersInColumns[rowNumber].Count; i++)
                    rowNumbers[i] = crossword.numbersInColumns[rowNumber][i];
                FillUnknownCells(row, rowNumbers);
                for (int i = 0; i < crossword.rowsCount; i++)
                    if (crossword.field[i, rowNumber] == CellStatus.Unknown && crossword.field[i, rowNumber] != row[i])
                    {
                        crossword.field[i, rowNumber] = row[i];
                        //запустить рекурсию
                    }
            }
            else
            {
                var row = new CellStatus[crossword.columnsCount];
                for (int i = 0; i < crossword.columnsCount; i++)
                    row[i] = crossword.field[rowNumber, i];
                var rowNumbers = new int[crossword.numbersInRows[rowNumber].Count];
                for (int i = 0; i < crossword.numbersInRows[rowNumber].Count; i++)
                    rowNumbers[i] = crossword.numbersInRows[rowNumber][i];
                FillUnknownCells(row, rowNumbers);
                for (int i = 0; i < crossword.columnsCount; i++)
                    if (crossword.field[rowNumber, i] == CellStatus.Unknown && crossword.field[rowNumber, i] != row[i])
                    {
                        crossword.field[rowNumber, i] = row[i];
                        //запустить рекурсию
                    }
            }
        }

        public void FillUnknownCells(CellStatus[] row, int[] rowNumbers)
        {
            var possibleFill = new bool[row.Length];
            var possibleEmpty = new bool[row.Length];
            CanArrangeBlock(-1, -1, possibleFill, possibleEmpty, row, rowNumbers);
            for (int i = 0; i < row.Length; i++)
            {
                if (possibleEmpty[i] != possibleFill[i])
                    if (possibleEmpty[i])
                        row[i] = CellStatus.Empty;
                    else
                        row[i] = CellStatus.Fill;
            }
        }

        public bool CanArrangeBlock(int blockIndex, int startIndex, bool[] possibleFill, bool[] possibleEmpty, CellStatus[] row, int[] rowNumbers)
        {
            var blockLength = 0;
            if (blockIndex != -1)
            {
                blockLength = rowNumbers[blockIndex];
                for (int i = startIndex; i < startIndex + blockLength; i++)
                {
                    if (row[i] == CellStatus.Empty)
                        return false;
                }
            }
            if (blockIndex < rowNumbers.Length - 1)
            {
                bool res = false;
                for (int startNext = startIndex + blockLength + 1;
                    startNext < row.Length - rowNumbers[blockIndex + 1] + 1;
                    startNext++)
                {
                    if(startNext != 0 && row[startNext - 1] == CellStatus.Fill)
                        break;
                    if (CanArrangeBlock(blockIndex + 1, startNext, possibleFill, possibleEmpty, row, rowNumbers))
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
                i < row.Length; 
                i++)
                if (row[i] == CellStatus.Fill)
                    return false;
            for (int i = startIndex; i < startIndex + blockLength; i++)
                possibleFill[i] = true;
            for (int i = startIndex + blockLength; i < row.Length; i++)
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