
using System.Text.RegularExpressions;

namespace LL1Parser;

public class Grammar
{
    public Tuple<string, string> Set { get; set; }

    public Grammar(string left, string right)
    {
        Set = new Tuple<string, string>(left, right);
    }
    
    public static List<Grammar> FilterDerivableGrammars(List<Grammar> allGrammars)
    {
        List<Grammar> terminalGrammars = allGrammars.Where(g => IsTerminalOnly(g.Set.Item2)).ToList();
        HashSet<string> derivableNonTerminals = new HashSet<string>(terminalGrammars.Select(g => g.Set.Item1));
        List<Grammar> derivableGrammars = new List<Grammar>();
        bool addedNew;

        do
        {
            addedNew = false;
            foreach (var grammar in allGrammars.Except(derivableGrammars))
            {
                var nonTerminalTokens = Regex.Matches(grammar.Set.Item2, @"<[^>]+>")
                    .Cast<Match>()
                    .Select(match => match.Value)
                    .Distinct();

                if (nonTerminalTokens.All(nt => derivableNonTerminals.Contains(nt)) && nonTerminalTokens.Any())
                {
                    derivableGrammars.Add(grammar);
                    derivableNonTerminals.Add(grammar.Set.Item1);
                    addedNew = true;
                }
            }
        } while (addedNew);
        
        derivableGrammars.AddRange(terminalGrammars);
        return derivableGrammars;
    }

    public static List<Grammar> FilterReachableGrammars(List<Grammar> derivableGrammars, string startSymbol)
    {
        HashSet<string> visited = new HashSet<string>();
        List<Grammar> reachableGrammars = new List<Grammar>();
        Traverse(startSymbol, derivableGrammars, visited, reachableGrammars);
        return reachableGrammars;
    }

    private static void Traverse(string symbol, List<Grammar> grammars, HashSet<string> visited, List<Grammar> reachableGrammars)
    {
        if (!visited.Contains(symbol))
        {
            visited.Add(symbol);
            var relevantGrammars = grammars.Where(g => g.Set.Item1 == symbol).ToList();

            foreach (var grammar in relevantGrammars)
            {
                reachableGrammars.Add(grammar);
                var nonTerminals = Regex.Matches(grammar.Set.Item2, @"<[^>]+>")
                                        .Cast<Match>()
                                        .Select(match => match.Value)
                                        .Distinct();
                foreach (var nt in nonTerminals)
                {
                    Traverse(nt, grammars, visited, reachableGrammars);
                }
            }
        }
    }

    private static bool IsTerminalOnly(string production)
    {
        return !Regex.IsMatch(production, @"<[^>]+>");
    }
}

