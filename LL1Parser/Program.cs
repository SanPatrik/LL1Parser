
namespace LL1Parser;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: dotnet run <input1_file> <input2_file>");
            return;
        }

        var inputFilePath1 = args[0];
        var inputFilePath2 = args[1];

        BnfGrammarReducer reducer = new BnfGrammarReducer(inputFilePath1);
        LL1Analyzer analyzer = new LL1Analyzer(reducer.ReducedGrammar);
        if (!analyzer.IsLL1())
        {
            Console.WriteLine("Gramatika nie je LL(1)-gramatikou.\n");
            return;
        }
        Console.WriteLine("Gramatika je LL(1)-gramatikou.\n");
        
        DerivationProcessor processor = new DerivationProcessor(analyzer, inputFilePath2, reducer.StartSymbol);
        processor.ProcessStrings();
        return;
    }
}
