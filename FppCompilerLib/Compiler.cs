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

        /// <summary>
        /// Compiles the program to a blueprint string
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public string Compile(string program)
        {
            var lexicalAnalyzer = new LexicalAnalyzer();
            var tokens = lexicalAnalyzer.Parse(program);

            var fppLanguageGrammar = new FppLanguageGrammar();

            var syntacticalAnalyzer = new SyntacticalAnalyzer(fppLanguageGrammar.Grammar, 3);
            var rootParseNode = syntacticalAnalyzer.Parse(tokens);

            var rootNode = fppLanguageGrammar.ParseTable.Parse(rootParseNode);

            var semanticAnalyzer = new SemanticAnalyzer();
            var updatedRootNode = semanticAnalyzer.Parse(rootNode);

            var codeGenerator = new CodeGenerator();
            var machineCommands = codeGenerator.ToMachineCommands(updatedRootNode);

            var programPackager = new ProgramPackager();
            var blueprint = programPackager.PackProgram(machineCommands);

            return blueprint;
        }
    }
}
