using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using X_Programming_Language.Errors;
using X_Programming_Language.Values;

namespace X_Programming_Language.Utilities
{
    public class UtilFunctions
    {
        public static string Input(List<string> userInputs)
        {
            List<string> queryFindResult = new();
            string queryFind = "";
            string result = "";
            string writeResult = "";
            int initialPosition = Console.GetCursorPosition().Left;
            int position = initialPosition;
            int resultTopPosition = 0; // Reference History
            int resultLeftPosition = 0;
            bool changeHistorySelect = true;
            ConsoleKeyInfo currentKey = new ConsoleKeyInfo();

            while (true)
            {
                currentKey = Console.ReadKey(true);

                if (currentKey.Key == ConsoleKey.Enter) break;

                if (currentKey.Key == ConsoleKey.LeftArrow)
                {
                    position = position > initialPosition ? Console.GetCursorPosition().Left - 1 : initialPosition;
                    resultLeftPosition = resultLeftPosition > 0 ? resultLeftPosition - 1 : 0;
                    Console.SetCursorPosition(position, Console.GetCursorPosition().Top);
                }
                else if (currentKey.Key == ConsoleKey.RightArrow)
                {
                    position = position < initialPosition + result.Length ? Console.GetCursorPosition().Left + 1 : initialPosition + result.Length;
                    resultLeftPosition = resultLeftPosition < result.Length ? resultLeftPosition + 1 : result.Length;
                    Console.SetCursorPosition(position, Console.GetCursorPosition().Top);
                }
                else if (currentKey.Key == ConsoleKey.UpArrow)
                {
                    if (resultTopPosition >= userInputs.Count)
                    {
                        resultTopPosition = userInputs.Count;
                        continue;
                    }
                    if (changeHistorySelect) resultTopPosition += 1;
                    queryFind = "" + result;

                    if (queryFindResult.Count == 0)
                    {
                        queryFindResult = userInputs.FindAll(q => q.StartsWith(queryFind));
                        resultTopPosition = 1;
                    }

                    if (queryFindResult.Count != 0)
                    {
                        var storage = queryFindResult[^resultTopPosition];

                        

                        Console.CursorVisible = false;
                        Console.SetCursorPosition(initialPosition, Console.GetCursorPosition().Top);
                        Console.Write(new string(' ', initialPosition + result.Length));
                        Console.SetCursorPosition(initialPosition, Console.GetCursorPosition().Top);
                        result = storage;
                        resultLeftPosition = result.Length;
                        Console.Write(result);
                        position = initialPosition + result.Length;
                        Console.CursorVisible = true;
                    }

                    if (!changeHistorySelect) changeHistorySelect = true;
                }
                else if (currentKey.Key == ConsoleKey.DownArrow)
                {
                    if (resultTopPosition <= 1)
                    {
                        var beforeResultLength = result.Length;
                        resultTopPosition = 1;
                        resultLeftPosition = writeResult.Length;
                        position = initialPosition;
                        if (Console.GetCursorPosition().Left != initialPosition)
                        {
                            Console.CursorVisible = false;
                            Console.SetCursorPosition(initialPosition, Console.GetCursorPosition().Top);
                            Console.Write(new string(' ', initialPosition + beforeResultLength));
                            Console.SetCursorPosition(initialPosition, Console.GetCursorPosition().Top);
                            result = writeResult;
                            position = initialPosition + result.Length;
                            Console.Write(result, Console.GetCursorPosition().Top);
                            Console.CursorVisible = true;
                            changeHistorySelect = false;
                        }
                        continue;
                    }
                    resultTopPosition -= 1;
                    queryFind = "" + result;

                    if (queryFindResult.Count == 0) queryFindResult = userInputs.FindAll(q => q.StartsWith(queryFind));
                    if (queryFindResult.Count != 0)
                    {
                        var beforeResultLength = result.Length;
                        result = queryFindResult[^resultTopPosition];

                        resultLeftPosition = result.Length;

                        Console.CursorVisible = false;
                        Console.SetCursorPosition(initialPosition, Console.GetCursorPosition().Top);
                        Console.Write(new string(' ', initialPosition + beforeResultLength));
                        Console.SetCursorPosition(initialPosition, Console.GetCursorPosition().Top);
                        Console.Write(result);
                        position = initialPosition + result.Length;
                        Console.CursorVisible = true;
                    }
                }
                else if (currentKey.Key == ConsoleKey.Backspace || currentKey.Key == ConsoleKey.Delete)
                {
                    if (result.Length != 0 && resultLeftPosition > 0)
                    {
                        result = string.Concat(result.AsSpan(0, resultLeftPosition - 1), result.AsSpan(resultLeftPosition));
                        Console.CursorVisible = false;
                        position = position > initialPosition ? Console.GetCursorPosition().Left - 1 : initialPosition;
                        resultLeftPosition -= 1;
                        Console.SetCursorPosition(position, Console.GetCursorPosition().Top);
                        Console.Write(result.Substring(resultLeftPosition) + new string(' ', position - resultLeftPosition));
                        Console.SetCursorPosition(position, Console.GetCursorPosition().Top);
                        Console.CursorVisible = true;
                    }
                }
                else
                {
                    if (result.Length <= resultLeftPosition) result += currentKey.KeyChar;
                    else result = result[..resultLeftPosition] + currentKey.KeyChar + result.Substring(resultLeftPosition);

                    Console.CursorVisible = false;
                    Console.Write(currentKey.KeyChar + result.Substring(resultLeftPosition + 1));
                    if (resultLeftPosition < result.Length - 1) Console.SetCursorPosition(position + 1, Console.GetCursorPosition().Top);
                    Console.CursorVisible = true;
                    writeResult = result;
                    position += 1;
                    resultLeftPosition += 1;
                    if (queryFindResult.Count != 0) queryFindResult.RemoveAll(_ => true);
                }
            }
            Console.WriteLine();
            if (result != "") userInputs.Add(result.TrimEnd());
            return result.TrimEnd();
        }

        public static string ListToString<T>(List<T> list)
        {
            return string.Join(", ", list);
        }

        public static Tuple<string, string> ReadFile(string filePath, string correctFileFormat)
        {
            if (IsADirectory(filePath)) throw new Exception($"\"{filePath}\" Must be a file path, not a directory");
            string fileName = filePath.Split(Path.DirectorySeparatorChar)[^1];
            
            if (fileName.Split(".").Length < 1 || fileName.Split(".")[^1] != correctFileFormat) throw new Exception($"Invalid file format");

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            
            return new(fileName, string.Join('\n', lines));
        }

        public static bool IsADirectory(string filePath)
        {
            FileAttributes attr = File.GetAttributes(filePath);

            return attr.HasFlag(FileAttributes.Directory);
        }

        public static Tuple<ListValue?, Error?> Execute(string query, SymbolTable globalSymbolTable, List<string> userInputs, bool isOnInterpreter, string fileName = "<stdin>")
        {
            Lexer lexer = new Lexer(fileName, query);
            var callback = lexer.Tokenize();
            if (callback.Item2 != null) return new(null, callback.Item2);

            if (isOnInterpreter)
            {
                if (callback.Item1.Count < 2) return new(new(null, new()), null);
                //if (callback.Item1[^2].Type == "")
            }

            Console.WriteLine(ListToString(callback.Item1));

            // Generate AST
            Parser parser = new Parser(callback.Item1, isOnInterpreter);
            var ast = parser.Parse();
            if (ast.Error != null) return new(null, ast.Error);

            // Execute program
            Interpreter interpreter = new Interpreter();
            Context context = new Context("<program>");
            context.SymbolTable = globalSymbolTable;
            var result = interpreter.Visit(ast.Node!, context);

            return new(result.Value as ListValue, result.Error);
        }
    }
}

