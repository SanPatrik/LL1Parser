
namespace LL1Parser;

public class DerivationProcessor
{
    private readonly LL1Analyzer Analyzer;
    private readonly string InputFilePath;
    private readonly string StartSymbol;

    public DerivationProcessor(LL1Analyzer analyzer, string inputFilePath, string startSymbol)
    {
        Analyzer = analyzer;
        InputFilePath = inputFilePath;
        StartSymbol = startSymbol;
    }

    public void ProcessStrings()
    {
        var lines = File.ReadAllLines(InputFilePath).Skip(1); // Skip first line
        foreach (var line in lines)
        {
            Console.WriteLine("Retazec: " + line);
            var tokens = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var output = ParseString(tokens);
            Console.WriteLine(output);
            Console.WriteLine("*****");
        }
    }

    private string ParseString(string[] tokens)
    {
        Stack<string> stack = new Stack<string>();
        stack.Push("$");
        stack.Push(StartSymbol);

        int tokenIndex = 0;
        List<string> derivationRules = new List<string>();

        while (stack.Count > 1)
        {
            string top = stack.Peek();
            string token = tokenIndex < tokens.Length ? $"\"{tokens[tokenIndex]}\"" : "$";

            if (top.Equals(token, StringComparison.InvariantCultureIgnoreCase))
            {
                stack.Pop();
                tokenIndex++;
            }
            else if (Analyzer.ParsingTable.TryGetValue((top, token), out string production))
            {
                stack.Pop();
                // Console.WriteLine($"{top} ::= {production}");

                var symbols = production.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Reverse();
                foreach (var symbol in symbols)
                {
                    if (symbol != LL1Analyzer.Epsilon)
                        stack.Push(symbol);
                }
                derivationRules.Add($"{top} ::= {production}");
            }
            else
                return $"Retazec nema derivaciu.";
            
        }
        if (stack.Count == 1 && stack.Peek() == "$" && tokenIndex == tokens.Length)
            return $"Retazec ma derivaciu.\nPostupnost pravidiel pre lavu derivaciu:\n{string.Join("\n", derivationRules)}";
        
        else
            return $"Retazec nema derivaciu.";
    }


}

