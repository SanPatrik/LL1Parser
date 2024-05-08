
namespace LL1Parser;

public class BnfGrammarReducer
{
    public List<Grammar> ReducedGrammar { get; private set; }
    public string StartSymbol { get; private set; }
    public BnfGrammarReducer(string inputFilePath1)
    {
        try
        {
            var grammars = Parser.ReadFromGrammar(inputFilePath1);
            if (grammars.Count == 0)
            {
                Console.WriteLine("No grammars parsed from the file.");
                return;
            }

            var startSymbol = grammars.First().Set.Item1;
            var nT = Grammar.FilterDerivableGrammars(grammars);
            var vD = Grammar.FilterReachableGrammars(nT, startSymbol);
            
            // Parser.WriteFormattedGrammar(outputFilePath, grammars, vD);
            ReducedGrammar = vD;
            StartSymbol = startSymbol;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

