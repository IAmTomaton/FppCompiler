using FppCompilerLib.SemanticAnalysis.Nodes;
using FppCompilerLib.SemanticAnalysis.Nodes.AssignNodes;
using FppCompilerLib.SemanticAnalysis.Nodes.ExpressionNodes;
using FppCompilerLib.SemanticAnalysis.Nodes.LoopNodes;
using FppCompilerLib.SemanticAnalysis.Nodes.TypeNodes;
using FppCompilerLib.SyntacticalAnalysis;

namespace FppCompilerLib
{
    internal class FppLanguageGrammar
    {
        public readonly Grammar Grammar;
        public readonly RuleToNodeParseTable ParseTable;

        public FppLanguageGrammar()
        {
            Grammar = new Grammar(body);
            ParseTable = new RuleToNodeParseTable();

            GenerateGrammar();
        }

        private void GenerateGrammar()
        {
            AddRule(body, bodyAdd, BodyNode.Parse);
            AddRule(bodyAdd, new Token[] { bodyLine, new Terminal(";"), bodyAdd });
            AddRule(bodyAdd, new Token[] { fork, bodyAdd });
            AddRule(bodyAdd, new Token[] { loopWhile, bodyAdd });
            AddRule(bodyAdd, new Token[] { loopFor, bodyAdd });
            AddLambdaRule(bodyAdd);

            AddRule(bodyLine, expression, PassParse(0));
            AddRule(bodyLine, createVariableLine, PassParse(0));
            AddRule(bodyLine, assignVariableLine, PassParse(0));
            AddRule(bodyLine, continueCmd, PassParse(0));
            AddRule(bodyLine, breakCmd, PassParse(0));

            AddRule(createVariableLine, new Token[] { typeExpression, Terminal.Word, assignVariableS }, CreateVariableNode.Parse);
            AddRule(assignVariableS, new Token[] { new Terminal("="), expression });
            AddLambdaRule(assignVariableS);

            AddRule(assignVariableLine, new Token[] { assignVariableLineArg, new Terminal("="), expression }, AssignNode.Parse);
            AddRule(assignVariableLineArg, Terminal.Word, AssignableVariableNode.Parse);
            //AddRule(assignVariableLineArg, new Token[] { new Terminal("*"), assignVariableLineArg }));

            // Math expression
            AddRule(expression, new Token[] { expressionArg, expressionS }, BinaryOperatorNode.Parse);

            AddRule(expressionArg, new Token[] { expressionArgS, postfixOperator }, UnaryOperatorNode.ParsePostfix);
            AddRule(postfixOperator, Terminal.UnaryOperator);
            AddLambdaRule(postfixOperator);

            AddRule(expressionArg, new Token[] { new Terminal("-"), expressionArg }, UnaryOperatorNode.ParsePrefix);
            AddRule(expressionArg, new Token[] { new Terminal("&"), expressionArg }, UnaryOperatorNode.ParsePrefix);
            AddRule(expressionArg, new Token[] { new Terminal("*"), expressionArg }, UnaryOperatorNode.ParsePrefix);
            AddRule(expressionArg, new Token[] { Terminal.UnaryOperator, expressionArg }, UnaryOperatorNode.ParsePrefix);

            AddRule(expressionArgS, Terminal.Const, ConstantNode.Parse);
            AddRule(expressionArgS, Terminal.Word, VariableNode.Parse);
            AddRule(expressionArgS, functionCall, PassParse(0));
            AddRule(expressionArgS, new Token[] { openingBrace, expression, closingBrace }, PassParse(1));

            AddRule(expressionS, new Token[] { new Terminal("-"), expressionArg, expressionS });
            AddRule(expressionS, new Token[] { new Terminal("&"), expressionArg, expressionS });
            AddRule(expressionS, new Token[] { new Terminal("*"), expressionArg, expressionS });
            AddRule(expressionS, new Token[] { Terminal.BinaryOperator, expressionArg, expressionS });
            AddLambdaRule(expressionS);

            // Type expression
            AddRule(typeExpression, new Token[] { simpleType, additionType }, TypeNode.Parse);
            AddRule(simpleType, Terminal.Type, SimpleTypeNode.Parse);
            AddLambdaRule(additionType);

            // Fork
            AddRule(fork,
                new Token[]
                {
                    new Terminal("if"), openingBrace, expression, closingBrace,
                    openingCurlyBrace, body, closingCurlyBrace,
                    forkAdd
                },
                ForkNode.Parce);
            AddRule(forkAdd,
                new Token[]
                {
                    new Terminal("else"), new Terminal("if"), openingBrace, expression, closingBrace,
                    openingCurlyBrace, body, closingCurlyBrace,
                    forkAdd
                });
            AddRule(forkAdd, new Token[] { new Terminal("else"), openingCurlyBrace, body, closingCurlyBrace });
            AddLambdaRule(forkAdd);

            // Loop
            // While
            AddRule(loopWhile,
                new Token[]
                {
                    new Terminal("while"), openingBrace, expression, closingBrace,
                    openingCurlyBrace, body, closingCurlyBrace
                },
                WhileNode.Parse);
            // For
            AddRule(loopFor,
                new Token[]
                {
                    new Terminal("for"), openingBrace, bodyLine, new Terminal(";"), expression, new Terminal(";"), bodyLine, closingBrace,
                    openingCurlyBrace, body, closingCurlyBrace
                },
                ForNode.Parse);
            AddRule(continueCmd, new Terminal("continue"), ContinueNode.Parse);
            AddRule(breakCmd, new Terminal("break"), BreakNode.Parse);

            // Function call
            AddRule(functionCall, new Token[] { Terminal.Word, openingBrace, functionCallArg, closingBrace }, FunctionCallNode.Parse);
            AddLambdaRule(functionCallArg);
            AddRule(functionCallArg, expression);
            AddRule(functionCallArg, new Token[] { expression, new Terminal(","), functionCallArg });
        }

        private void AddRule(Rule rule)
        {
            Grammar.AddRule(rule);
        }

        private void AddRule(NonTerminal source, Token[] tokens)
        {
            AddRule(new Rule(source, tokens));
        }

        private void AddRule(NonTerminal source, Token token)
        {
            AddRule(new Rule(source, token));
        }

        private void AddLambdaRule(NonTerminal source)
        {
            Grammar.AddRule(new Rule(source));
        }

        private void AddRule(Rule rule, Func<NonTerminalNode, RuleToNodeParseTable, SemanticNode> parser)
        {
            Grammar.AddRule(rule);
            ParseTable.Add(rule, parser);
        }

        private void AddRule(NonTerminal source, Token[] tokens, Func<NonTerminalNode, RuleToNodeParseTable, SemanticNode> parser)
        {
            AddRule(new Rule(source, tokens), parser);
        }

        private void AddRule(NonTerminal source, Token token, Func<NonTerminalNode, RuleToNodeParseTable, SemanticNode> parser)
        {
            AddRule(new Rule(source, token), parser);
        }

        private static Func<NonTerminalNode, RuleToNodeParseTable, SemanticNode> PassParse(int i) => (node, parseTable) => parseTable.Parse((NonTerminalNode)node.childs[i]);

        private static readonly NonTerminal body = new("body");
        private static readonly NonTerminal bodyLine = new("body_line");
        private static readonly NonTerminal bodyAdd = new("body_add");

        private static readonly NonTerminal createVariableLine = new("create_variable_line");
        private static readonly NonTerminal createConstantLine = new("create_constant_line");

        private static readonly NonTerminal typeExpression = new("type_expression");
        private static readonly NonTerminal additionType = new("addition_type");
        private static readonly NonTerminal simpleType = new("simple_type");
        private static readonly NonTerminal pointerType = new("pointer_type");
        private static readonly NonTerminal arrayType = new("array_type");

        private static readonly NonTerminal assignVariableLine = new("assign_variable_line");
        private static readonly NonTerminal assignVariableLineArg = new("assign_variable_line_arg");
        private static readonly NonTerminal assignVariableS = new("assign_variable_s");
        private static readonly NonTerminal expression = new("expression");
        private static readonly NonTerminal expressionArg = new("expression_arg");
        private static readonly NonTerminal expressionArgS = new("expression_arg_s");
        private static readonly NonTerminal variable = new("variable");
        private static readonly NonTerminal array = new("array");
        private static readonly NonTerminal arrayAdd = new("array_add");
        private static readonly NonTerminal postfixOperator = new("postfix_operator");
        private static readonly NonTerminal expressionS = new("expression_s");
        private static readonly NonTerminal fork = new("fork");
        private static readonly NonTerminal forkAdd = new("fork_add");
        private static readonly NonTerminal loopFor = new("loop_for");
        private static readonly NonTerminal loopWhile = new("loop_while");
        private static readonly NonTerminal continueCmd = new("continueCmd");
        private static readonly NonTerminal breakCmd = new("breakCmd");
        private static readonly NonTerminal functionCall = new("function_call");
        private static readonly NonTerminal functionCallArg = new("function_call_arg");

        private static readonly Terminal openingBrace = new("(");
        private static readonly Terminal closingBrace = new(")");
        private static readonly Terminal openingCurlyBrace = new("{");
        private static readonly Terminal closingCurlyBrace = new("}");
    }
}
