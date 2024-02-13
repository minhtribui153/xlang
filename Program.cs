// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using X_Programming_Language.Constants;
using X_Programming_Language.Errors;
using X_Programming_Language.Library.BuiltIn;
using X_Programming_Language.Utilities;
using X_Programming_Language.Values;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace X_Programming_Language
{
    internal class Program
    {
        public static string Version = "1.1.5";
        public static string ReleaseType = "alpha";

        public static SymbolTable GlobalSymbolTable = new();
        public static List<string> UserInputs = new();

        public static void SetBuiltInVariables()
        {
            // Built-in Variables
            GlobalSymbolTable.Set("nil", new NullValue());

            // Built-in Functions
            GlobalSymbolTable.SetFunction("printLn", new BuiltInFunctionValue("printLn", new() { new("value", "object") }, BuiltInFunctions.PrintLn));
            GlobalSymbolTable.SetFunction("input", new BuiltInFunctionValue("input", new() { new("prompt", "string") }, BuiltInFunctions.Input));

            // Built-in Type Conversions
            GlobalSymbolTable.SetFunction("parseString", new BuiltInFunctionValue("parseString", new() { new("value", "object") }, TypeConversions.ParseString));

            // Built-in Types
            GlobalSymbolTable.SetType("int", new TypeValue("int", typeof(IntValue)));
            GlobalSymbolTable.SetType("float", new TypeValue("float", typeof(FloatValue)));
            GlobalSymbolTable.SetType("string", new TypeValue("string", typeof(StringValue)));
            GlobalSymbolTable.SetType("bool", new TypeValue("bool", typeof(BoolValue)));
            GlobalSymbolTable.SetType("function", new TypeValue("function", typeof(FunctionValue)));
            GlobalSymbolTable.SetType("object", new TypeValue("object", typeof(Value)));
        }
        
        public static void Main(string[] localArgs)
        {
            SetBuiltInVariables();

            var arguments = localArgs.ToList();

            if (arguments.Count > 0)
	        {
                try
                {
                    if (arguments.ElementAtOrDefault(0) != null)
                    {
                        var command = arguments[0]!;

                        if (command == "run")
                        {
                            if (arguments.ElementAtOrDefault(1) == null) throw new Exception("Please provide a file to run");
                            var callback = UtilFunctions.ReadFile(arguments[1], "x");
                            var fileName = callback.Item1;
                            var execute = UtilFunctions.Execute(callback.Item2, GlobalSymbolTable, UserInputs, false, fileName);
                            if (execute.Item2 != null) Console.WriteLine(execute.Item2.AsString());
                        }
                        else throw new Exception($"Command '{command}' not found");
                    }
                }
                catch (Exception e) {
                    Console.WriteLine($"{TerminalColours.FAIL}Error:{TerminalColours.RESET} {e.Message}");
                }
            } else InitializeInterpreter();
            
        }



        public static void InitializeInterpreter()
        {
            LanguageScreen();
            bool running = true;
            while (running)
            {
                Console.Write(">>> ");
                string query = UtilFunctions.Input(UserInputs);
                if (string.IsNullOrWhiteSpace(query)) continue;
                if (query.StartsWith(":"))
                {
                    InterpreterCommandHandler(query);
                    continue;
                }
                var callback = UtilFunctions.Execute(query, GlobalSymbolTable, UserInputs, true);
                
                var result = callback.Item1;
                var error = callback.Item2;

                if (error != null) Console.WriteLine(error.AsString());
                else if (result != null)
                {
                    if (result.Elements.Count == 1)
                        Console.WriteLine(result.Elements[0]);
                    else
                        Console.WriteLine(result);
                }
            }
        }

        public static void InterpreterCommandHandler(string query)
        {
            string[] data = query[1..].Split(" ");
            string command = data[0]!;
            string[] args = data.Length > 1 ? data[1..] : Array.Empty<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(command)) throw new Exception("Please enter a command");

                if (command == "exit")
                {
                    Environment.Exit(0);
                }
                else if (command == "help")
                {
                    string res = "Here are the available commands:\n";
                    res += "help - Displays this information\n";
                    res += "exit - Exits the program\n";
                    Console.WriteLine(res);
                }
                else throw new Exception($"No such command '{command}'");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{TerminalColours.FAIL}CommandError:{TerminalColours.RESET} {e.Message}");
            }
        }

        public static void LanguageScreen()
        {
            var print = "";
            if (Console.WindowWidth < 130)
            {
                print +=   "▁▁▁▁      ▁▁▁▁\n";
                print += "\\   \\    /   /\n";
                print += " \\   \\  /   / \n";
                print += "  \\   \\/   /  \n";
                print += $"   \\      /   \n";
                print += $"   /      \\   \n";
                print += "  /   /\\   \\  \n";
                print += " /   /  \\   \\ \n";
                print += "/   /    \\   \\\n";
                print +=   "▔▔▔▔      ▔▔▔▔\n";
                print += "\n";

                print += "© NoCli Technologies 2022-2023\n";
                print += $"{TerminalColours.INFO}v{Version}{TerminalColours.RESET} {TerminalColours.HIGHLIGHTED_LIGHTRED}{TerminalColours.BLACK}({ReleaseType}){TerminalColours.RESET}\n";
                print += $"Welcome to the X Programming Language interpreter\n";
                print += $"Type {TerminalColours.HIGHLIGHTED_LIGHTBLUE}{TerminalColours.BLACK}:help{TerminalColours.RESET} for more information\n";
                print += $"CTRL-C or type {TerminalColours.HIGHLIGHTED_LIGHTBLUE}{TerminalColours.BLACK}:exit{TerminalColours.RESET} to exit the interpreter\n";

            } else
            {
                print +=    "____      ____    │ © NoCli Technologies 2022-2023\n";
                print += $"\\   \\    /   /    │ {TerminalColours.INFO}v{Version}{TerminalColours.RESET} {TerminalColours.HIGHLIGHTED_LIGHTRED}{TerminalColours.BLACK}({ReleaseType}){TerminalColours.RESET}\n";
                print += $" \\   \\  /   /     │ Welcome to the X Programming Language interpreter\n";
                print += $"  \\   \\/   /      │ Type {TerminalColours.HIGHLIGHTED_LIGHTBLUE}{TerminalColours.BLACK}:help{TerminalColours.RESET} for more information\n";
                print +=  $"   \\      /       │\n";
                print +=  $"   /      \\       │\n";
                print += $"  /   /\\   \\      │\n";
                print += $" /   /  \\   \\     │\n";
                print += $"/   /    \\   \\    │\n";
                print +=   $"▔▔▔▔      ▔▔▔▔    │ CTRL-C or type {TerminalColours.HIGHLIGHTED_LIGHTBLUE}{TerminalColours.BLACK}:exit{TerminalColours.RESET} to exit the interpreter\n";
            }
            Console.WriteLine(print);
        }
    }
}
