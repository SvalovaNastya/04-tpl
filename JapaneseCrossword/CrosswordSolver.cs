using System;
using System.Collections.Generic;

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        private Crossword crossword;
        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
            crossword = Crossword.ReadCrosswordFromFile(inputFilePath);
            var queue = new Queue<Tuple<int, bool>>();
            for (int i = 0; i < crossword.RowsCount; i++)
                SetNewPosition(i, false, queue);
            while (queue.Count != 0)
            {
                var t = queue.Dequeue();
                SetNewPosition(t.Item1, t.Item2, queue);
            }
            crossword.WriteCrosswordToFile(outputFilePath);
            return SolutionStatus.BadInputFilePath;
        }

        private bool ShouldRefreshPerpendicularLine(int lineIdx, int i, CellStatus linei, bool isColumn)
        {
            var cell = isColumn ? crossword.Field[i, lineIdx] : crossword.Field[lineIdx, i];
            return cell == CellStatus.Unknown && cell != linei;
        }

        private void SetNewPosition(int lineIdx, bool isColumn, Queue<Tuple<int, bool>> queue)
        {
            var line = isColumn ? crossword.GetColumnCells(lineIdx) : crossword.GetRowCells(lineIdx);
            var numbersIsLine = isColumn
                ? crossword.NumbersInColumns[lineIdx].ToArray()
                : crossword.NumbersInRows[lineIdx].ToArray();
            FillUnknownCells(line, numbersIsLine);
            for (int i = 0; i < line.Length; i++)
            {
                if (ShouldRefreshPerpendicularLine(lineIdx, i, line[i], isColumn))
                {
                    Tuple<int, bool> t;
                    if (isColumn)
                    {
                        t = Tuple.Create(i, false);
                        crossword.Field[i, lineIdx] = line[i];
                    }
                    else
                    {
                        t = Tuple.Create(i, true);
                        crossword.Field[lineIdx, i] = line[i];
                    }
                    if (!queue.Contains(t))
                        queue.Enqueue(t);
                }
            }
        }

        private void FillUnknownCells(CellStatus[] row, int[] rowNumbers)
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

        private bool CanArrangeBlock(int blockIndex, int startIndex, bool[] possibleFill, bool[] possibleEmpty, CellStatus[] row, int[] rowNumbers)
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
                            RefreshState(startIndex, possibleFill, startIndex + blockLength);
                            RefreshState(startIndex + blockLength, possibleEmpty, startNext);
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
            RefreshState(startIndex, possibleFill, startIndex + blockLength);
            RefreshState(startIndex + blockLength, possibleEmpty, row.Length);
            return true;
        }

        private static void RefreshState(int startIndex, bool[] possibleSmth, int length)
        {
            for (int i = startIndex; i < length; i++)
                possibleSmth[i] = true;
        }
    }
}