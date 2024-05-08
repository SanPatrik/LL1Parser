namespace LL1Parser;

public class Parser
{
    private const string EmptyLanguage = "PRAZDNY JAZYK";

    public static List<Grammar> ReadFromGrammar(string filePath)
    {
        var grammars = new List<Grammar>();
        string fileContent = File.ReadAllText(filePath);
        string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(new[] { "::=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new FormatException("Invalid production rule format.");

            string left = parts[0].Trim();
            string[] alternatives = parts[1].Trim().Split('|');

            grammars.AddRange(alternatives.Select(alt => new Grammar(left, alt.Trim())));
        }

        return grammars;
    }
    
    public static void WriteFormattedGrammar(string filePath, List<Grammar> originalGrammars, List<Grammar> filteredGrammars)
    {
        if (filteredGrammars.Count == 0)
        {
            File.WriteAllText(filePath, EmptyLanguage);
            return;
        }

        var grammarGroups = filteredGrammars
            .GroupBy(g => g.Set.Item1)
            .ToDictionary(group => group.Key, group => group.ToList());

        using (var writer = new StreamWriter(filePath))
        {
            foreach (var originalGrammar in originalGrammars)
            {
                if (grammarGroups.TryGetValue(originalGrammar.Set.Item1, out var groupedGrammars))
                {
                    string combinedProductions = string.Join(" | ", groupedGrammars.Select(g => g.Set.Item2).Distinct());
                    writer.WriteLine($"{originalGrammar.Set.Item1} ::= {combinedProductions}");
                    grammarGroups.Remove(originalGrammar.Set.Item1);
                }
            }
        }
    }
}