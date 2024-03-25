using FppCompilerLib.CodeGeneration;
using FppCompilerLib.LexicalAnalysis;
using FppCompilerLib.SemanticAnalysis;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib
{
    public class Compiler
    {
        public Compiler()
        {

        }

        public string Compile(string program)
        {
            var lexicalAnalyzer = new LexicalAnalyzer();
            var tokens = lexicalAnalyzer.Parse(program);

            var fppLanguageGrammar = new FppLanguageGrammar();

            var syntacticalAnalyzer = new SyntacticalAnalyzer(fppLanguageGrammar.Grammar, 3);
            var rootParseNode = syntacticalAnalyzer.Parse(tokens);

            var rootNode = fppLanguageGrammar.ParseTable.Parse(rootParseNode);

            var semanticAnalyzer = new SemanticAnalyzer();
            rootNode = semanticAnalyzer.Parse(rootNode);

            var codeGenerator = new CodeGenerator();
            var machineCommands = codeGenerator.ToMachineCommands(rootNode);

            var programPackager = new ProgramPackager();
            var blueprint = programPackager.PackProgram(machineCommands);

            return blueprint;
        }
    }
}
