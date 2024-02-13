using System;
namespace X_Programming_Language.Constants
{
    public class SyntaxIdentifier
    {
        public static string DIGITS = "0123456789";
        public static string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string PERMITTED_IDENTIFIER_OR_COND_OP = LETTERS + "&_|" + DIGITS;
        public static char[] OPERATORS = { '+', '-', '*', '/', '^', '%', '(', ')', '[', ']', '{', '}', '=', '<', '>', '!', '&', '|', ':', ',', '"', '.' };
        public static string[] COND_OPERATORS = { ">=", "<=", "!=", "==" };
        public static string[] BOOLEAN_VALUES = { "true", "false" };
        public static string[] KEYWORDS = {
            "assign",
            "mut",
            "set",
            "if",
            "then",
            "else",
            "for",
            "foreach",
            "while",
            "switch",
            "case",
            "default",
            "return",
            "continue",
            "break",
            "step",
            "from",
            "func",
            "void",
            "instof",
            "end"
        };
    }
}

