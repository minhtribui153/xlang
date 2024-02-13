using System;
using X_Programming_Language.Values;

namespace X_Programming_Language.Utilities
{
    public class Position
    {
        public int Index { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public string FileName { get; private set; }
        public string FileText { get; private set; }

        public Position(int index, int line, int column, string fileName, string fileText)
        {
            Index = index;
            Line = line;
            Column = column;

            FileName = fileName;
            FileText = fileText;
        }

        public Position Advance(char? currentChar = null)
        {
            Index += 1;
            Column += 1;

            if (currentChar != null && currentChar == '\n')
            {
                Line += 1;
                Column = 0;
            }

            return this;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Position _obj)
                return (
                    Index == _obj.Index ||
                    Line == _obj.Line ||
                    Column == _obj.Column
                );
            return false;
        }

        public Position Copy()
        {
            return new Position(Index, Line, Column, FileName, FileText);
        }
    }
}

