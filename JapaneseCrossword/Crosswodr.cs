using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        public int RowsCount { get { return Field.GetLength(0); } }
        public int ColumnsCount { get { return Field.GetLength(1); } }
        public readonly List<List<int>> NumbersInRows;
        public readonly List<List<int>> NumbersInColumns;
        public readonly CellStatus[,] Field;

        public Crossword(int rowsCount, int columnsCount, List<List<int>> numbersInRows, List<List<int>> numbersInColumns)
        {
            NumbersInRows = new List<List<int>>();
            NumbersInColumns = new List<List<int>>();
            for (int i = 0; i < numbersInRows.Count(); i++)
            {
                NumbersInRows.Add(new List<int>(numbersInRows[i]));
            }
            for (int i = 0; i < numbersInColumns.Count(); i++)
            {
                NumbersInColumns.Add(new List<int>(numbersInColumns[i]));
            }
            Field = new CellStatus[rowsCount, columnsCount];
        }

        public CellStatus[] GetColumnCells(int columnIdx)
        {
            var column = new CellStatus[RowsCount];
            for (int i = 0; i < RowsCount; i++)
                column[i] = Field[i, columnIdx];
            return column;
        }

        public CellStatus[] GetRowCells(int rowIdx)
        {
            var row = new CellStatus[ColumnsCount];
            for (int i = 0; i < ColumnsCount; i++)
                row[i] = Field[rowIdx, i];
            return row;
        }

        static public Crossword ReadCrosswordFromFile(string inputFilePath)
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

        public void WriteCrosswordToFile(string fileName)
        {
            var lines = new List<string>();
            for (int i = 0; i < Field.GetLength(0); i++)
            {
                var line = new StringBuilder();
                for (int j = 0; j < Field.GetLength(1); j++)
                    if (Field[i, j] == CellStatus.Fill)
                        line.Append('*');//('█');
                    else if (Field[i, j] == CellStatus.Empty)
                        line.Append('.');
                    else
                        line.Append('?');
                lines.Add(line.ToString());
            }
            File.WriteAllLines(fileName, lines);
        }
    }
}
