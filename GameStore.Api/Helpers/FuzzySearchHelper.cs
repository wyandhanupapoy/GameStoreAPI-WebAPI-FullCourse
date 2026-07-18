namespace GameStore.Api.Helpers;

/// <summary>
/// Helper class untuk typo-tolerant fuzzy search.
/// Menggunakan kombinasi Levenshtein Distance + Token Matching.
/// 
/// Kenapa Levenshtein?
/// - Menghitung "edit distance" (jumlah operasi insert/delete/substitute) antara 2 string
/// - Sangat akurat untuk mendeteksi typo: "Winbing" → "Winning" = distance 2
/// - Kompleksitas O(m*n) dimana m dan n adalah panjang string
/// - Untuk 300 data dengan nama game rata-rata 20 karakter, ini sangat cepat
/// 
/// Kenapa Token Matching?
/// - Memecah query multi-kata menjadi token
/// - Mencocokkan setiap token query dengan best match di candidate
/// - "Winbing Eleven" → "Winbing" fuzzy match "Winning", "Eleven" exact match "Eleven"
/// - Menghasilkan score rata-rata dari semua token
/// </summary>
public static class FuzzySearchHelper
{
    /// <summary>
    /// Menghitung Levenshtein Distance antara dua string.
    /// Levenshtein Distance = jumlah minimum operasi (insert, delete, substitute)
    /// yang diperlukan untuk mengubah satu string menjadi string lain.
    /// 
    /// Contoh:
    /// - "kitten" → "sitting" = 3 (s/k, i→i, e→i, n→n, →g = sub k→s, sub e→i, insert g)
    /// - "Winbing" → "Winning" = 2 (b→n, g→g = sub b→n, sub g→n... actually let me recalc)
    /// 
    /// Menggunakan Wagner-Fischer algorithm dengan optimasi 2 baris (hemat memory).
    /// Space complexity: O(min(m,n)) instead of O(m*n)
    /// Time complexity: O(m*n)
    /// </summary>
    public static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        // Optimasi: gunakan string yang lebih pendek sebagai "kolom" untuk hemat memory
        if (source.Length > target.Length)
            (source, target) = (target, source);

        int sourceLength = source.Length;
        int targetLength = target.Length;

        // Hanya butuh 2 baris, bukan full matrix
        var previousRow = new int[sourceLength + 1];
        var currentRow = new int[sourceLength + 1];

        // Inisialisasi baris pertama: cost transformasi string kosong → source[0..i]
        for (int i = 0; i <= sourceLength; i++)
            previousRow[i] = i;

        for (int j = 1; j <= targetLength; j++)
        {
            currentRow[0] = j; // Cost transformasi target[0..j] → string kosong

            for (int i = 1; i <= sourceLength; i++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1; // 0 jika karakter sama

                currentRow[i] = Math.Min(
                    Math.Min(
                        currentRow[i - 1] + 1,      // Insert
                        previousRow[i] + 1),         // Delete
                    previousRow[i - 1] + cost);      // Substitute (atau match jika cost=0)
            }

            // Swap baris: previousRow = currentRow untuk iterasi berikutnya
            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[sourceLength];
    }

    /// <summary>
    /// Mengkonversi Levenshtein Distance menjadi similarity score (0.0 - 1.0).
    /// 
    /// Formula: 1.0 - (distance / maxLength)
    /// 
    /// Contoh:
    /// - "Winning" vs "Winbing" → distance=1, maxLen=7, score = 1 - 1/7 = 0.857
    /// - "FIFA" vs "FITA" → distance=1, maxLen=4, score = 1 - 1/4 = 0.75
    /// - "abc" vs "xyz" → distance=3, maxLen=3, score = 0.0
    /// </summary>
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

    /// <summary>
    /// Token-based fuzzy matching.
    /// 
    /// Memecah query dan candidate menjadi token (kata), lalu mencocokkan 
    /// setiap token query dengan token candidate yang paling mirip.
    /// 
    /// Contoh:
    /// Query: "Winbing Eleven"
    /// Candidate: "Winning Eleven 2024"
    /// 
    /// Token query: ["winbing", "eleven"]
    /// Token candidate: ["winning", "eleven", "2024"]
    /// 
    /// Matching:
    /// - "winbing" best match → "winning" (similarity = 0.857)
    /// - "eleven" best match → "eleven" (similarity = 1.0)
    /// 
    /// Score = average(0.857, 1.0) = 0.928
    /// 
    /// Bonus: jika candidate mengandung query sebagai substring (case-insensitive),
    /// kita boost score ke minimum 0.9 karena itu pasti relevan.
    /// </summary>
    public static double TokenMatch(string query, string candidate)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(candidate))
            return 0.0;

        string queryLower = query.ToLowerInvariant();
        string candidateLower = candidate.ToLowerInvariant();

        // Quick check: exact substring match mendapat bonus tinggi
        if (candidateLower.Contains(queryLower))
            return 1.0;

        string[] queryTokens = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] candidateTokens = candidateLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (queryTokens.Length == 0 || candidateTokens.Length == 0)
            return 0.0;

        double totalScore = 0.0;

        // Untuk setiap token query, cari best match di candidate tokens
        foreach (string queryToken in queryTokens)
        {
            double bestTokenScore = 0.0;

            foreach (string candidateToken in candidateTokens)
            {
                // Cek substring match per token (e.g. "war" matches "warcraft")
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

        // Score = rata-rata similarity semua token query
        return totalScore / queryTokens.Length;
    }

    /// <summary>
    /// Melakukan fuzzy search terhadap collection of candidates.
    /// 
    /// Parameter:
    /// - query: search keyword dari user (bisa mengandung typo)
    /// - candidates: list of (Id, Name) dari database
    /// - threshold: minimum similarity score (default 0.6 = 60%)
    /// 
    /// Return: List of (Id, Name, Score) sorted by score descending
    /// 
    /// Contoh usage:
    /// var results = FuzzySearchHelper.FuzzySearch(
    ///     "Winbing Eleven",
    ///     games.Select(g => (g.Id, g.Name)),
    ///     threshold: 0.6
    /// );
    /// // Returns: [(41, "Winning Eleven 2024", 0.928), (42, "Winning Eleven 2023", 0.928), ...]
    /// </summary>
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
            .ThenBy(r => r.Name) // Secondary sort: alphabetical untuk score yang sama
            .ToList();
    }
}
