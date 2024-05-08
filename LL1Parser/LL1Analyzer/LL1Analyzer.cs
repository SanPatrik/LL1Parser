
namespace LL1Parser;

public class LL1Analyzer
{
    public const string Epsilon = "\"\"";
    public Dictionary<string, HashSet<string>> FirstSets { get; private set; }
    public Dictionary<string, HashSet<string>> FollowSets { get; private set; }
    public Dictionary<(string, string), string> ParsingTable { get; private set; }
    
    public LL1Analyzer(List<Grammar> grammars)
    {
        FirstSets = CalculateFirstSets(grammars);
        FollowSets = CalculateFollowSets(grammars);
        ParsingTable = BuildParsingTable(grammars);
    }
    private Dictionary<string, HashSet<string>> CalculateFirstSets(List<Grammar> grammars)
    {
        Dictionary<string, HashSet<string>> first = new Dictionary<string, HashSet<string>>();
        HashSet<string> nonTerminals = new HashSet<string>(grammars.Select(g => g.Set.Item1));

        foreach (var nonTerminal in nonTerminals)
            first[nonTerminal] = new HashSet<string>();
        

        bool changed;
        do
        {
            changed = false;
            foreach (var grammar in grammars)
            {
                string nonTerminal = grammar.Set.Item1;
                string[] parts = grammar.Set.Item2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts[0] == Epsilon)
                {
                    if (first[nonTerminal].Add(Epsilon))
                        changed = true;
                }
                else
                {
                    foreach (var part in parts)
                    {
                        if (nonTerminals.Contains(part))
                        {
                            int beforeAdd = first[nonTerminal].Count;
                            foreach (var symbol in first[part].Where(x => x != Epsilon))
                                first[nonTerminal].Add(symbol);
                            

                            if (!first[part].Contains(Epsilon))
                                break;

                            if (beforeAdd != first[nonTerminal].Count)
                                changed = true;
                        }
                        else
                        {
                            if (first[nonTerminal].Add(part))
                                changed = true;
                            break;
                        }
                    }
                }
            }
        } while (changed);

        return first;
    }

    private Dictionary<string, HashSet<string>> CalculateFollowSets(List<Grammar> grammars)
    {
        Dictionary<string, HashSet<string>> follow = new Dictionary<string, HashSet<string>>();
        HashSet<string> nonTerminals = new HashSet<string>(grammars.Select(g => g.Set.Item1));

        foreach (var grammar in grammars)
            follow[grammar.Set.Item1] = new HashSet<string>();

        follow[grammars[0].Set.Item1].Add("\"end\"");  // Assuming '"end"' as EOF symbol

        bool changed;
        do
        {
            changed = false;
            foreach (var grammar in grammars)
            {
                string nonTerminal = grammar.Set.Item1;
                string[] parts = grammar.Set.Item2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                HashSet<string> trailer = new HashSet<string>(follow[nonTerminal]);
                for (int i = parts.Length - 1; i >= 0; i--)
                {
                    string part = parts[i];
                    if (nonTerminals.Contains(part))
                    {
                        if (!follow.ContainsKey(part))
                            follow[part] = new HashSet<string>();

                        int beforeAdd = follow[part].Count;
                        follow[part].UnionWith(trailer);
                        if (beforeAdd != follow[part].Count)
                            changed = true;

                        if (FirstSets.ContainsKey(part) && !FirstSets[part].Contains(Epsilon))
                            trailer.Clear();

                        if (FirstSets.ContainsKey(part))
                            trailer.UnionWith(FirstSets[part].Where(sym => sym != Epsilon));
                    }
                    else
                    {
                        trailer.Clear();
                        trailer.Add(part);
                    }
                }
            }
        } while (changed);

        return follow;
    }

    private Dictionary<(string, string), string> BuildParsingTable(List<Grammar> grammars)
    {
        var table = new Dictionary<(string, string), string>();

        foreach (var grammar in grammars)
        {
            string nonTerminal = grammar.Set.Item1;
            string production = grammar.Set.Item2;
            var firstOfProduction = GetFirstOfProduction(production);

            foreach (var terminal in firstOfProduction)
            {
                if (terminal != Epsilon)
                {
                    var key = (nonTerminal, terminal);
                    if (table.ContainsKey(key))
                        table[key] += " | " + production;
                    else
                        table[key] = production;
                }
            }

            if (firstOfProduction.Contains(Epsilon))
            {
                foreach (var followSymbol in FollowSets[nonTerminal])
                {
                    var key = (nonTerminal, followSymbol);
                    if (table.ContainsKey(key))
                        table[key] += " | " + production;
                    else
                        table[key] = production;
                }
            }
        }

        return table;
    }

    private HashSet<string> GetFirstOfProduction(string production)
    {
        var symbols = production.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var firstSet = new HashSet<string>();

        bool containsEpsilon = true;
        foreach (var symbol in symbols)
        {
            if (FirstSets.ContainsKey(symbol))
            {
                foreach (var item in FirstSets[symbol])
                {
                    if (item != Epsilon)
                        firstSet.Add(item);
                }
                containsEpsilon &= FirstSets[symbol].Contains(Epsilon);
            }
            else
            {
                firstSet.Add(symbol);
                containsEpsilon = false;
                break;
            }
            if (!containsEpsilon)
                break;
        }

        if (containsEpsilon)
            firstSet.Add(Epsilon);

        return firstSet;
    }

    public bool IsLL1()
    {
        foreach (var entry in ParsingTable)
        {
            if (entry.Value.Contains('|'))
                return false;
        }
        return true;
    }
}

