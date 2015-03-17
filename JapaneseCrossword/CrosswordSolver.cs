using System;
using System.Collections.Generic;
using NUnit.Framework;

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
            var isFullSolve = CheckForFullAnswer();
            if (isFullSolve && !CheckForCorrectAnswer())
                return SolutionStatus.IncorrectCrossword;
            if (isFullSolve && !CheckForCorrectAnswer1())
                return SolutionStatus.IncorrectCrossword;
            try { crossword.WriteCrosswordToFile(outputFilePath); }
            catch { return SolutionStatus.BadOutputFilePath; }
            if (!isFullSolve)
                return SolutionStatus.PartiallySolved;
            return SolutionStatus.Solved;
        }

        private bool CheckForFullAnswer()
        {
            foreach (var e in crossword.Field)
            {
                if (e == CellStatus.Unknown)
                    return false;
            }
            return true;
        }

        private bool CheckForCorrectAnswer()
        {
            for (int i = 0; i < crossword.RowsCount; i++)
            {
                int j = 0;
                while (crossword.Field[i, j] == CellStatus.Empty)
                    j++;
                for (int numberIdx = 0; numberIdx < crossword.NumbersInRows[i].Count; numberIdx++)
                {
                    int count = AddCounter(i, ref j, CellStatus.Fill);
                    if (crossword.NumbersInRows[i][numberIdx] != count)
                        return false;
                    count = AddCounter(i, ref j, CellStatus.Empty);
                    if (count < 1 && numberIdx != crossword.NumbersInRows[i].Count - 1)
                        return false;
                }
            }
            return true;
        }

        private int AddCounter(int i, ref int j, CellStatus status)
        {
            int count = 0;
            while (j < crossword.ColumnsCount && crossword.Field[i, j] == status)
            {
                count++;
                j++;
            }
            return count;
        }

        private bool CheckForCorrectAnswer1()
        {
            for (int i = 0; i < crossword.ColumnsCount; i++)
            {
                int j = 0;
                while (crossword.Field[j, i] == CellStatus.Empty)
                    j++;
                for (int numberIdx = 0; numberIdx < crossword.NumbersInColumns[i].Count; numberIdx++)
                {
                    int count = AddCounter1(i, ref j, CellStatus.Fill);
                    if (crossword.NumbersInColumns[i][numberIdx] != count)
                        return false;
                    count = AddCounter1(i, ref j, CellStatus.Empty);
                    if (count < 1 && numberIdx != crossword.NumbersInColumns[i].Count - 1)
                        return false;
                }
            }
            return true;
        }

        private int AddCounter1(int i, ref int j, CellStatus status)
        {
            int count = 0;
            while (j < crossword.RowsCount && crossword.Field[j, i] == status)
            {
                count++;
                j++;
            }
            return count;
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
            for (int i = 0; i < row.Length - rowNumbers[0] + 1; i++)
                CanArrangeBlock(0, i, 0, possibleFill, possibleEmpty, row, rowNumbers);
            for (int i = 0; i < row.Length; i++)
            {
                if (possibleEmpty[i] != possibleFill[i])
                    if (possibleEmpty[i])
                        row[i] = CellStatus.Empty;
                    else
                        row[i] = CellStatus.Fill;
            }
        }

        private bool CheckBadCellsAbscence(int left , int right , CellStatus badCelltype, CellStatus[] row)
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
            var endIndex = startIndex + blockLength;
            if (!CheckBadCellsAbscence(startIndex, endIndex, CellStatus.Empty, row))
                return false;
            if (!CheckBadCellsAbscence(previousEndIndex, startIndex, CellStatus.Fill, row))
                return false;
            if (blockIndex == rowNumbers.Length - 1)
            {
                if (!CheckBadCellsAbscence(endIndex, row.Length, CellStatus.Fill, row))
                    return false;
                Draw(endIndex, row.Length, possibleEmpty);
                Draw(startIndex, endIndex, possibleFill);
                Draw(previousEndIndex, startIndex, possibleEmpty);
                return true;
            }
            bool res = false;
            int afterBlockIdx = endIndex + 1;
            int lastNextBlockFirstPosition = row.Length - rowNumbers[blockIndex + 1] + 1;
            for (int nextStart = afterBlockIdx; nextStart < lastNextBlockFirstPosition; nextStart++)
            {
                if (CanArrangeBlock(blockIndex + 1, nextStart, endIndex, possibleFill, possibleEmpty, row, rowNumbers))
                {
                    res = true;
                    Draw(startIndex, endIndex, possibleFill);
                    Draw(previousEndIndex, startIndex, possibleEmpty);
                }
            }
            return res;
        }
    }
}