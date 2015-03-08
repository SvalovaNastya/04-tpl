using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
    public enum CellStatus
    {
        Unknown,
        Fill,
        Empty
    }

    public class Crossword
    {
        public readonly int rowsCount;
        public readonly int columnsCount;
        public readonly List<List<int>> numbersInRows;
        public readonly List<List<int>> numbersInColumns;
        public readonly CellStatus[,] field;

        public Crossword(int rowsCount, int columnsCount, List<List<int>> numbersInRows, List<List<int>> numbersInColumns)
        {
            this.rowsCount = rowsCount;
            this.columnsCount = columnsCount;
            this.numbersInRows = new List<List<int>>();
            this.numbersInColumns = new List<List<int>>();
            for (int i = 0; i < numbersInRows.Count(); i++)
            {
                this.numbersInRows.Add(new List<int>(numbersInRows[i]));
            }
            for (int i = 0; i < numbersInColumns.Count(); i++)
            {
                this.numbersInColumns.Add(new List<int>(numbersInColumns[i]));
            }
            field = new CellStatus[rowsCount, columnsCount];
        }
    }
}
