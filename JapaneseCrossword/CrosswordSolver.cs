using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            if (isFullSolve && !CheckCrossword())
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

        private bool CheckCrossword()
        {
            return CheckForCorrectAnswer(true, crossword.RowsCount, crossword.ColumnsCount)
                && CheckForCorrectAnswer(false, crossword.ColumnsCount, crossword.RowsCount);
        }

        private bool CheckForCorrectAnswer(bool isHorisontal, int dimensionalOne, int dimensionalTwo)
        {
            for (int i = 0; i < dimensionalOne; i++)
            {
                var firstPoint = getPoint(i, 0, isHorisontal);
                var currentCell = crossword.Field[firstPoint.Item1, firstPoint.Item2];
                int count = 1;
                int fillBlockIndex = 0;
                for (int j = 1; j < dimensionalTwo; j++)
                {
                    var a = getPoint(i, j, isHorisontal);
                    var cell = crossword.Field[a.Item1, a.Item2];
                    if (cell == currentCell)
                        count++;
                    else
                    {
                        if (currentCell == CellStatus.Fill)
                        {
                            if (count != GetNumberOfFillCell(isHorisontal, i, fillBlockIndex))
                                return false;
                            fillBlockIndex++;
                        }
                        count = 1;
                    }
                    currentCell = cell;
                }
                if (currentCell == CellStatus.Fill)
                    if (count != GetNumberOfFillCell(isHorisontal, i, fillBlockIndex))
                        return false;
            }
            return true;
        }

        private int GetNumberOfFillCell(bool isHorisontal, int i, int fillBlockIndex)
        {
            if (isHorisontal)
                return crossword.NumbersInRows[i][fillBlockIndex];
            return crossword.NumbersInColumns[i][fillBlockIndex];
        }

        private Tuple<int, int> getPoint(int x, int y, bool isHorisontal)
        {
            if (isHorisontal)
                return new Tuple<int, int>(x, y);
            return new Tuple<int, int>(y, x);
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

        private bool CheckBadCellsAbscence(int left, int right, CellStatus badCelltype, CellStatus[] row)
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