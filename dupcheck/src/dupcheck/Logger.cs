using System.Text;

namespace dupcheck;

/// <summary>
/// 全出力を担当する。
/// stdout    : 重複ファイル名のみ・1行1ファイル名。-silent 時は出力しない。
/// -dupfile  : 人間が読みやすいブロック形式・追記モード。
/// -log      : CSV 形式・追記モード。ヘッダーは新規作成時のみ書き出す。
/// </summary>
public sealed class Logger : IDisposable
{
    private readonly bool _silent;
    private readonly bool _verbose;
    private readonly StreamWriter? _csvWriter;
    private readonly StreamWriter? _dupWriter;

    internal static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private const string CsvHeader = "実行日時,対象ディレクトリ,historyディレクトリ,ファイル名,判定";

    public Logger(bool silent, bool verbose, string? logFilePath, string? dupFilePath)
    {
        _silent = silent;
        _verbose = verbose;

        if (!string.IsNullOrEmpty(logFilePath))
        {
            bool isNew = !File.Exists(logFilePath) || new FileInfo(logFilePath).Length == 0;
            _csvWriter = new StreamWriter(logFilePath, append: true, isNew ? Utf8WithBom : Utf8NoBom);
            if (isNew)
            {
                _csvWriter.WriteLine(CsvHeader);
            }
        }

        if (!string.IsNullOrEmpty(dupFilePath))
        {
            bool isNew = !File.Exists(dupFilePath) || new FileInfo(dupFilePath).Length == 0;
            _dupWriter = new StreamWriter(dupFilePath, append: true, isNew ? Utf8WithBom : Utf8NoBom);
        }
    }

    public void Dispose()
    {
        _csvWriter?.Dispose();
        _dupWriter?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>照合結果を stdout・-dupfile・-log の各出力先に書き出す</summary>
    public void WriteResults(
        string startTime,
        string targetDir,
        string historyDir,
        IReadOnlyList<CheckResult> results)
    {
        WriteStdout(results);
        WriteDupFile(startTime, targetDir, historyDir, results);
        WriteCsv(startTime, targetDir, historyDir, results);
    }

    /// <summary>stdout: 重複ファイル名のみ・1行1ファイル名</summary>
    private void WriteStdout(IReadOnlyList<CheckResult> results)
    {
        if (_silent) return;

        foreach (CheckResult r in results)
        {
            if (r.IsDuplicate)
                Console.WriteLine(r.FileName);
        }
    }

    /// <summary>-dupfile: 人間が読みやすいブロック形式・追記</summary>
    private void WriteDupFile(string startTime, string targetDir, string historyDir, IReadOnlyList<CheckResult> results)
    {
        if (_dupWriter == null) return;

        int duplicateCount = results.Count(r => r.IsDuplicate);
        int normalCount = results.Count - duplicateCount;

        _dupWriter.WriteLine("=====================================");
        _dupWriter.WriteLine("実行: " + startTime);
        _dupWriter.WriteLine("対象: " + targetDir);
        _dupWriter.WriteLine("history: " + historyDir);
        _dupWriter.WriteLine("-------------------------------------");

        foreach (CheckResult r in results)
        {
            if (r.IsDuplicate)
                _dupWriter.WriteLine("[重複] " + r.FileName);
            else if (_verbose)
                _dupWriter.WriteLine("[正常] " + r.FileName);
        }

        _dupWriter.WriteLine("-------------------------------------");
        _dupWriter.WriteLine($"合計: {results.Count}件 / 重複: {duplicateCount}件 / 正常: {normalCount}件");
        _dupWriter.WriteLine("=====================================");
        _dupWriter.WriteLine("");
    }

    /// <summary>-log: CSV 形式・追記</summary>
    private void WriteCsv(string startTime, string targetDir, string historyDir, IReadOnlyList<CheckResult> results)
    {
        if (_csvWriter == null) return;

        foreach (CheckResult r in results)
        {
            if (!r.IsDuplicate && !_verbose) continue;

            _csvWriter.WriteLine(string.Join(",",
                CsvField(startTime),
                CsvField(targetDir),
                CsvField(historyDir),
                CsvField(r.FileName),
                CsvField(r.IsDuplicate ? "重複" : "正常")));
        }
    }

    /// <summary>CSV フィールドをエスケープする。カンマ・ダブルクォート・改行を含む場合はダブルクォートで囲む</summary>
    private static string CsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
