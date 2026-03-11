using System.Text;

namespace dupcheck;

/// <summary>
/// コンソール出力およびログファイル出力を担当する。
/// -silent 時はコンソールへ一切出力しない。
/// -log 指定時は同じ内容を UTF-8 BOM のファイルに新規作成（上書き）する。
/// </summary>
public sealed class Logger : IDisposable
{
    private readonly bool _silent;
    private readonly bool _verbose;
    private readonly string? _logFilePath;
    private readonly StreamWriter? _logWriter;

    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public Logger(bool silent, bool verbose, string? logFilePath)
    {
        _silent = silent;
        _verbose = verbose;
        _logFilePath = logFilePath;

        if (!string.IsNullOrEmpty(logFilePath))
        {
            _logWriter = new StreamWriter(logFilePath, append: false, Utf8WithBom);
        }
        else
        {
            _logWriter = null;
        }
    }

    /// <summary>1行をコンソールおよびログファイルに出力する（-silent の場合は何もしない）</summary>
    public void WriteLine(string line)
    {
        if (!_silent)
        {
            Console.WriteLine(line);
        }
        _logWriter?.WriteLine(line);
    }

    /// <summary>エラーメッセージを stderr およびログファイルに出力する（-silent でも stderr は出力する仕様とするが、仕様では「コンソール出力を完全に抑制」なので silent 時はコンソールは出さない）</summary>
    public void WriteError(string message)
    {
        if (!_silent)
        {
            Console.Error.WriteLine(message);
        }
        _logWriter?.WriteLine(message);
    }

    /// <summary>ログファイルをフラッシュしてクローズする</summary>
    public void Dispose()
    {
        _logWriter?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>照合結果を仕様フォーマットで出力する</summary>
    public void WriteResults(
        string startTime,
        string targetDir,
        string historyDir,
        IReadOnlyList<CheckResult> results)
    {
        WriteLine("[dupcheck] チェック開始: " + startTime);
        WriteLine("[dupcheck] 対象ディレクトリ : " + targetDir);
        WriteLine("[dupcheck] historyディレクトリ: " + historyDir);
        WriteLine("");

        int duplicateCount = 0;
        foreach (CheckResult r in results)
        {
            if (r.IsDuplicate)
            {
                WriteLine("[重複] " + r.FileName);
                duplicateCount++;
            }
            else if (_verbose)
            {
                WriteLine("[正常] " + r.FileName);
            }
        }

        int normalCount = results.Count - duplicateCount;
        WriteLine("");
        WriteLine($"---");
        WriteLine($"チェック対象: {results.Count}件 / 重複あり: {duplicateCount}件 / 正常: {normalCount}件");
    }
}
