using FppCompilerLib;

namespace FppCompiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var path = args[0];
            var program = File.ReadAllText(path);

            var compiler = new Compiler();
            var bluprint = compiler.Compile(program);

            var output = args[1];
            File.WriteAllText(output, bluprint);
        }
    }
}