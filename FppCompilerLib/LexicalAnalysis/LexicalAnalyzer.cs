using FppCompilerLib.SyntacticalAnalysis;
using System.Text.RegularExpressions;

namespace FppCompilerLib.LexicalAnalysis
{
    internal class LexicalAnalyzer
    {
        private readonly string[] binaryOperators = { "<<", ">>", "**", "!=", "==", "<=", ">=", "||", "&&", ">", "<", "+", "/", "%", "|", "&", "^" };
        private readonly string[] unaryOperators = { "~", "!", "--", "++" };
        private readonly string[] specialCharacters = { "=", ";", "{", "}", "(", ")", "[", "]", ".", ",", "-", "*", "&" };
        private readonly string[] specialWords = { "if", "else", "for", "while", "const", "continue", "break"};
        private readonly string[] types = { "int", "bool" };
        private readonly string wordRegEx = "[a-zA-Z_][0-9a-zA-Z_]*";
        private readonly string constRegEx = "[0-9]+\\.[0-9]*|[0-9]+|true|false";

        public TerminalWithValue[] Parse(string programString)
        {
            var r = GetRegEx();
            var tokens = Regex.Matches(programString, GetRegEx(), RegexOptions.IgnoreCase)
                .Select(match => match.Value)
                .Select(str => StringToTerminal(str))
                .ToArray();
            return tokens;
        }

        private string GetRegEx()
        {
            string regex = constRegEx + '|' + wordRegEx;
            foreach (string str in unaryOperators.Concat(binaryOperators).Concat(specialCharacters))
                regex += "|\\" + string.Join("\\", str.ToArray());
            foreach (string word in specialWords)
                regex += '|' + word;
            return regex;
        }

        private TerminalWithValue StringToTerminal(string str)
        {
            if (types.Contains(str))
                return TerminalWithValue.Type(str);
            if (binaryOperators.Contains(str))
                return TerminalWithValue.BinaryOperator(str);
            if (unaryOperators.Contains(str))
                return TerminalWithValue.UnaryOperator(str);
            if (specialWords.Contains(str))
                return new TerminalWithValue(str, str);
            if (Regex.Match(str, "^(" + constRegEx + ")$", RegexOptions.IgnoreCase).Success)
                return TerminalWithValue.Const(str);
            if (Regex.Match(str, "^(" + wordRegEx + ")$", RegexOptions.IgnoreCase).Success)
                return TerminalWithValue.Word(str);
            return new TerminalWithValue(str, str);
        }
    }
}
