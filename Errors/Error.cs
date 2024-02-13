using System;
using System.Reflection.Metadata;
using X_Programming_Language.Constants;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class Error
    {
        public string ErrorName { get; private set; }
        public string Details { get; private set; }
        public Position PositionStart { get; private set; }
        public Position PositionEnd { get; private set; }
        public Context? Context { get; private set; }
        public bool Overwritable { get; private set; }

        public Error(Position positionStart, Position positionEnd, string errorName, string details, Context? context = null)
        {
            PositionStart = positionStart;
            PositionEnd = positionEnd;
            ErrorName = errorName;
            Details = details;
            Context = context;
            Overwritable = true;
        }

        public Error SetOverwritable(bool overwritable)
        {
            Overwritable = overwritable;
            return this;
        }

        public virtual string AsString()
        {
            string result = "";
            if (Context != null && Context.Parent != null)
            {
                result += GenerateTraceback();
                result += $"{TerminalColours.INFO}{TerminalColours.BOLD}{PositionStart.FileName}:{PositionStart.Line + 1}:{PositionStart.Column + 1}:{TerminalColours.RESET}{TerminalColours.FAIL} {ErrorName}:{TerminalColours.RESET} {Details}\n";
                result += StackTrace(PositionStart.FileText, PositionStart, PositionEnd);
            }
            else
            {
                result += $"{TerminalColours.INFO}{TerminalColours.BOLD}{PositionStart.FileName}:{PositionStart.Line + 1}:{PositionStart.Column + 1}:{TerminalColours.RESET}{TerminalColours.FAIL} {ErrorName}:{TerminalColours.RESET} {Details}\n";
                result += StackTrace(PositionStart.FileText, PositionStart, PositionEnd);
            }
            return result;
        }

        public string StackTrace(string fileTxt, Position positionStart, Position positionEnd)
        {
            var fileText = fileTxt + "  ";
            var fileData = fileText.Split('\n');
            var result = "";


            List<int> erroredLineNums = new();

            for (int i = positionStart.Line + 1; i < positionEnd.Line + 2; i++) erroredLineNums.Add(i);

            foreach (var i in GetLineNumbersToCheck(positionEnd.Line + 1, fileData.Length, erroredLineNums.Count > 0 ? erroredLineNums.Count + 2 : 2))
            {
                string line = fileData[i - 1] ;
                if (erroredLineNums.Contains(i))
                {
                    int columnStart = i == positionStart.Line + 1 ? positionStart.Column : 0;
                    int columnEnd = i == positionEnd.Line + 1 ? positionEnd.Column : line.Length - 1;


                    if (columnStart < line.Length && columnEnd < line.Length)
                    {
                        var lineFromPosition = line.Substring(columnStart, columnEnd - columnStart);
                        var beforeHighlight = line.Substring(0, columnStart);
                        var afterHighlight = line.Substring(columnEnd);
                        var highlightedLine = beforeHighlight + TerminalColours.HIGHLIGHTED_LIGHTRED + lineFromPosition + TerminalColours.RESET + afterHighlight;

                        if (string.IsNullOrWhiteSpace(lineFromPosition))
                            highlightedLine = beforeHighlight + lineFromPosition;


                        result += TerminalColours.FAIL + "▸" + TerminalColours.RESET + GetLineNumber(i) + " │ " + highlightedLine + "\n";
                    }
                    else result += TerminalColours.FAIL + "▸" + TerminalColours.RESET + GetLineNumber(i) + " │ " + line + "\n";
                    result += "       │ " + (columnStart < 1 ? "" : new string(' ', columnStart)) + TerminalColours.FAIL + ((columnEnd - columnStart == 1) ? "▴" : new string('~', columnEnd - columnStart)) + TerminalColours.RESET + "\n";
                }
                else
                {
                    result += " " + GetLineNumber(i) + " │ " + line + "\n";
                }
            }

            return result.Replace("\t", "");
        }

        public string GetLineNumber(int number)
        {
            if (number > 0 && number < 10) return $"    {number}";
            else if (number >= 10 && number < 100) return $"   {number}";
            else if (number >= 100 && number < 1000) return $"  {number}";
            else if (number >= 1000 && number < 10000) return $" {number}";
            return $"{number}";
        }

        public string MultiplyString(string text, int times)
        {
            var res = text;

            for (int i = 0; i < times; i++) res += text;

            return res;
        }

        public string GenerateTraceback()
        {
            var result = "";
            var position = PositionStart;
            Context? ctx = Context;

            while (ctx != null)
            {
                result += $"    At {ctx.DisplayName} -> File {TerminalColours.UNDERLINE}{position!.FileName}{TerminalColours.RESET} ({position.Line + 1}:{position.Column + 1})\n";
                position = ctx.ParentEntryPosition;
                ctx = ctx.Parent;
            }

            return $"{TerminalColours.FAIL}-------- Traceback (most recent call last){TerminalColours.RESET}\n" + result + $"{TerminalColours.FAIL}-------- End of Traceback{TerminalColours.RESET}\n\n";
        }

        public int[] GetLineNumbersToCheck(int lineNum, int lineCount, int firstLast)
        {
            if (lineCount == 1) return new int[] { 1 };
            List<int> lineNums = new();

            for (int i = lineNum - firstLast; i < lineNum + firstLast; i++)
            {
                if (i <= 0 || i > lineCount) continue;
                lineNums.Add(i);
            }

            return lineNums.ToArray();
        }
    }
}

