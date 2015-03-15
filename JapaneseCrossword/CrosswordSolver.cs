using System;
using System.Collections.Generic;

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        private Crossword crossword;
        private Queue<Tuple<int, bool>> tasksQueue; 

        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
            try { crossword = Crossword.ReadCrosswordFromFile(inputFilePath); }
            catch { return SolutionStatus.BadInputFilePath; }
            tasksQueue = new Queue<Tuple<int, bool>>();
            try
            {
                for (int i = 0; i < crossword.RowsCount; i++)
                    SetNewPosition(i, false);
                while (tasksQueue.Count != 0)
                {
                    var t = tasksQueue.Dequeue();
                    SetNewPosition(t.Item1, t.Item2);
                }
            }
            catch
            {
                return SolutionStatus.IncorrectCrossword;
            }
            try { crossword.WriteCrosswordToFile(outputFilePath); }
            catch { return SolutionStatus.BadOutputFilePath; }
            return SolutionStatus.Solved;
        }

        private bool ShouldRefreshPerpendicularLine(int lineIdx, int i, CellStatus linei, bool isColumn)
        {
            var cell = isColumn ? crossword.Field[i, lineIdx] : crossword.Field[lineIdx, i];
            return cell == CellStatus.Unknown && cell != linei;
        }

        private void SetNewPosition(int lineIdx, bool isColumn)
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
                    if (!tasksQueue.Contains(t))
                        tasksQueue.Enqueue(t);
                }
            }
        }

        private void FillUnknownCells(CellStatus[] row, int[] rowNumbers)
        {
            var possibleFill = new bool[row.Length];
            var possibleEmpty = new bool[row.Length];
//            CanArrangeBlock(-1, -1, possibleFill, possibleEmpty, row, rowNumbers);
            for(int i = 0; i < row.Length - rowNumbers[0]; i++)
                CanArrangeBlock(0, i, -1, possibleFill, possibleEmpty, row, rowNumbers);
            for (int i = 0; i < row.Length; i++)
            {
                if (possibleEmpty[i] != possibleFill[i])
                    if (possibleEmpty[i])
                        row[i] = CellStatus.Empty;
                    else
                        row[i] = CellStatus.Fill;
            }
        }

        private bool Check(int left , int right , CellStatus badCelltype, CellStatus[] row)
        {
            for (int i = left; i < right; i++)
            {
                if (row[i] == badCelltype)
                    return false;
            }
            return true;
        }

        private void Draw(int left, int right, bool[] possibleCelltype)
        {
            for (int i = left; i < right; i++)
                possibleCelltype[i] = true;
        }

        private bool CanArrangeBlock(int blockIndex, int startIndex, int previousEndIndex, bool[] possibleFill, bool[] possibleEmpty, CellStatus[] row, int[] rowNumbers)
        {
            var blockLength = rowNumbers[blockIndex];
            if (!Check(startIndex, startIndex + blockLength, CellStatus.Empty, row))
                return false;
            if (!Check(previousEndIndex + 1, startIndex, CellStatus.Fill, row))
                return false;
            if (blockIndex < rowNumbers.Length - 1)
            {
                bool res = false;
                int afterBlockIdx = startIndex + blockLength + 1;
                int lastNextBlockFirstPosition = row.Length - rowNumbers[blockIndex + 1] + 1;
                for (int nextStart = afterBlockIdx; nextStart < lastNextBlockFirstPosition; nextStart++)
                {
                    if (CanArrangeBlock(blockIndex + 1, nextStart, startIndex + blockLength - 1, possibleFill, possibleEmpty, row, rowNumbers))
                    {
                        res = true;
                        Draw(startIndex, startIndex + blockLength, possibleFill);
                        Draw(previousEndIndex + 1, startIndex, possibleEmpty);
                    }
                }
                return res;
            }
            if (!Check(startIndex + blockLength, row.Length, CellStatus.Fill, row))
                return false;
            Draw(startIndex + blockLength, row.Length, possibleEmpty);
            Draw(startIndex, startIndex + blockLength, possibleFill);
            Draw(previousEndIndex + 1, startIndex, possibleEmpty);
            return true;

//            var blockLength = 0;
//            if (blockIndex != -1)
//            {
//                blockLength = rowNumbers[blockIndex];
//                if (!NotHaveSmthCells(startIndex, row, startIndex + blockLength, CellStatus.Empty))
//                    return false;
//            }
//            if (blockIndex < rowNumbers.Length - 1)
//            {
//                bool res = false;
//                int afterBlockIdx = startIndex + blockLength + 1;
//                int lastNextBlockFirstPosition = row.Length - rowNumbers[blockIndex + 1] + 1;
//                for (int nextStart = afterBlockIdx; nextStart < lastNextBlockFirstPosition; nextStart++)
//                {
//                    if(nextStart != 0 && row[nextStart - 1] == CellStatus.Fill)
//                        break;
//                    if (CanArrangeBlock(blockIndex + 1, nextStart, possibleFill, possibleEmpty, row, rowNumbers))
//                    {
//                        res = true;
//                        if (blockIndex != -1)
//                        {
//                            RefreshState(startIndex, possibleFill, startIndex + blockLength);
//                            RefreshState(startIndex + blockLength, possibleEmpty, nextStart);
//                        }
//                        else
//                            RefreshState(0, possibleEmpty, nextStart);
//                    }
//                }
//                return res;
//            }
//            if (!NotHaveSmthCells(startIndex + blockLength, row, row.Length, CellStatus.Fill))
//                    return false;
//            RefreshState(startIndex, possibleFill, startIndex + blockLength);
//            RefreshState(startIndex + blockLength, possibleEmpty, row.Length);
//            return true;
        }

        private static bool NotHaveSmthCells(int startIndex, CellStatus[] row, int length, CellStatus status)
        {
            for (int i = startIndex; i < length; i++)
            {
                if (row[i] == status)
                    return false;
            }
            return true;
        }

        private static void RefreshState(int startIndex, bool[] possibleSmth, int length)
        {
            for (int i = startIndex; i < length; i++)
                possibleSmth[i] = true;
        }
    }
}