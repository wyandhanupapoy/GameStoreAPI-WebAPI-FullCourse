namespace GameStore.Api.Helpers;

public static class FuzzySearchHelper
{
    public static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        if (source.Length > target.Length)
            (source, target) = (target, source);

        int sourceLength = source.Length;
        int targetLength = target.Length;

        var previousRow = new int[sourceLength + 1];
        var currentRow = new int[sourceLength + 1];

        for (int i = 0; i <= sourceLength; i++)
            previousRow[i] = i;

        for (int j = 1; j <= targetLength; j++)
        {
            currentRow[0] = j;

            for (int i = 1; i <= sourceLength; i++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                currentRow[i] = Math.Min(
                    Math.Min(
                        currentRow[i - 1] + 1,
                        previousRow[i] + 1),
                    previousRow[i - 1] + cost);
            }

            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[sourceLength];
    }

    public static double CalculateSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target)) return 1.0;
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;

        // Case-insensitive comparison
        source = source.ToLowerInvariant();
        target = target.ToLowerInvariant();

        int distance = LevenshteinDistance(source, target);
        int maxLength = Math.Max(source.Length, target.Length);

        return 1.0 - ((double)distance / maxLength);
    }

    public static double TokenMatch(string query, string candidate)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(candidate))
            return 0.0;

        string queryLower = query.ToLowerInvariant();
        string candidateLower = candidate.ToLowerInvariant();

        if (candidateLower.Contains(queryLower))
            return 1.0;

        string[] queryTokens = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] candidateTokens = candidateLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (queryTokens.Length == 0 || candidateTokens.Length == 0)
            return 0.0;

        double totalScore = 0.0;

        foreach (string queryToken in queryTokens)
        {
            double bestTokenScore = 0.0;

            foreach (string candidateToken in candidateTokens)
            {
                if (candidateToken.Contains(queryToken) || queryToken.Contains(candidateToken))
                {
                    double substringScore = (double)Math.Min(queryToken.Length, candidateToken.Length)
                                          / Math.Max(queryToken.Length, candidateToken.Length);
                    bestTokenScore = Math.Max(bestTokenScore, Math.Max(substringScore, 0.85));
                }

                double similarity = CalculateSimilarity(queryToken, candidateToken);
                bestTokenScore = Math.Max(bestTokenScore, similarity);
            }

            totalScore += bestTokenScore;
        }

        return totalScore / queryTokens.Length;
    }

    public static List<(int Id, string Name, double Score)> FuzzySearch(
        string query,
        IEnumerable<(int Id, string Name)> candidates,
        double threshold = 0.6)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        return candidates
            .Select(c => (c.Id, c.Name, Score: TokenMatch(query, c.Name)))
            .Where(r => r.Score >= threshold)
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Name)
            .ToList();
    }
}
