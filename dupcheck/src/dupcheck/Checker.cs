namespace dupcheck;

/// <summary>
/// ファイル照合ロジック（純粋関数・副作用なし）。
/// target ディレクトリ直下のファイルが history に既に存在するかをチェックする。
/// </summary>
public static class Checker
{
    /// <summary>除外する実行ファイル名（大文字小文字を区別）</summary>
    public const string ExeName = "dupcheck.exe";

    /// <summary>
    /// target ディレクトリ直下のファイルを history のファイル名セットと照合し、
    /// 重複判定結果のリストを返す。
    /// </summary>
    /// <param name="targetDir">チェック対象ディレクトリのフルパス</param>
    /// <param name="historyDir">history ディレクトリのフルパス</param>
    /// <param name="excludedFileNames">照合対象から除外するファイル名の集合（例: ログファイル名）。Ordinal 比較</param>
    /// <returns>各ファイルの照合結果（ファイル名・重複フラグ）</returns>
    public static IReadOnlyList<CheckResult> Check(
        string targetDir,
        string historyDir,
        IReadOnlySet<string>? excludedFileNames = null)
    {
        excludedFileNames ??= new HashSet<string>(StringComparer.Ordinal);

        // history 直下のファイル名を HashSet で取得（O(1) 照合用）
        string[] historyFiles = Directory.GetFiles(historyDir, "*", SearchOption.TopDirectoryOnly);
        HashSet<string> historyNames = new(StringComparer.Ordinal);
        foreach (string path in historyFiles)
        {
            historyNames.Add(Path.GetFileName(path));
        }

        // target 直下のファイルのみ取得
        string[] targetFiles = Directory.GetFiles(targetDir, "*", SearchOption.TopDirectoryOnly);
        List<CheckResult> results = new();

        foreach (string fullPath in targetFiles)
        {
            string fileName = Path.GetFileName(fullPath);

            // 除外対象ならスキップ（照合結果にも含めない）
            if (excludedFileNames.Contains(fileName))
                continue;

            bool isDuplicate = historyNames.Contains(fileName);
            results.Add(new CheckResult(fileName, isDuplicate));
        }

        return results;
    }
}
