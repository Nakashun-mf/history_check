namespace dupcheck;

/// <summary>
/// エントリポイント・引数解析・終了コード制御。
/// 終了コード: 0=正常(重複なし), 1=重複あり, 2=引数エラー・例外。
/// </summary>
internal static class Program
{
    private const string DefaultHistoryDir = @".\history";

    public static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[dupcheck] エラー: " + ex.Message);
            return 2;
        }
    }

    private static int Run(string[] args)
    {
        if (args.Length > 0 && (args[0] == "-help" || args[0] == "--help" || args[0] == "/?"))
        {
            PrintHelp();
            return 0;
        }

        if (!TryParseOptions(args, out Options? options, out string? errorMessage))
        {
            Console.Error.WriteLine("[dupcheck] " + errorMessage);
            return 2;
        }

        if (!ValidateOptions(options!, out errorMessage))
        {
            Console.Error.WriteLine("[dupcheck] " + errorMessage);
            return 2;
        }

        Options opts = options!;

        // 除外ファイル名: dupcheck.exe とログファイル名（指定時のみ）
        HashSet<string> excluded = new(StringComparer.Ordinal) { Checker.ExeName };
        if (!string.IsNullOrEmpty(opts.LogFile))
        {
            excluded.Add(Path.GetFileName(opts.LogFile));
        }

        IReadOnlyList<CheckResult> results = Checker.Check(opts.TargetDir, opts.HistoryDir, excluded);

        string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        using (var logger = new Logger(opts.Silent, opts.Verbose, opts.LogFile))
        {
            logger.WriteResults(startTime, opts.TargetDir, opts.HistoryDir, results);
        }

        int duplicateCount = results.Count(r => r.IsDuplicate);
        return duplicateCount > 0 ? 1 : 0;
    }

    private static void PrintHelp()
    {
        Console.WriteLine(@"dupcheck — 処理前ファイルが history に既に存在しないかチェックします。

使い方:
  dupcheck.exe [オプション]

オプション:
  -target <dir>    チェック対象（処理前ファイル）のディレクトリ（省略時: exe と同じディレクトリ）
  -history <dir>   処理済みファイルが格納されたディレクトリ（省略時: .\history）
  -log [<file>]    ログファイルパス（省略時: 出力しない。ファイル省略時: dupcheck_YYYYMMDD_HHMMSS.log）
  -silent          コンソール出力を抑制
  -verbose         全ファイルの判定結果を出力（省略時: 重複ファイルのみ）
  -help            このヘルプを表示

終了コード:
  0  正常終了（重複なし）
  1  重複ファイルが1件以上あり
  2  引数エラー・ディレクトリ不正・その他例外");
    }

    /// <summary>
    /// コマンドラインをパースして Options を組み立てる。
    /// </summary>
    private static bool TryParseOptions(string[] args, out Options? options, out string? errorMessage)
    {
        options = null;
        errorMessage = null;

        string targetDir = GetDefaultTargetDir();
        string historyDir = DefaultHistoryDir;
        string? logFile = null;
        bool silent = false;
        bool verbose = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg == "-target")
            {
                if (i + 1 >= args.Length) { errorMessage = "-target にディレクトリを指定してください。"; return false; }
                targetDir = args[++i];
            }
            else if (arg == "-history")
            {
                if (i + 1 >= args.Length) { errorMessage = "-history にディレクトリを指定してください。"; return false; }
                historyDir = args[++i];
            }
            else if (arg == "-log")
            {
                if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                {
                    logFile = args[++i];
                }
                else
                {
                    logFile = "dupcheck_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
                }
            }
            else if (arg == "-silent")
                silent = true;
            else if (arg == "-verbose")
                verbose = true;
            else if (arg == "-help")
            {
                // 先頭で -help は既に処理済みのためここには来ない
            }
            else
            {
                errorMessage = "不明なオプション: " + arg;
                return false;
            }
        }

        // 相対パスをカレントディレクトリ基準で絶対パスに正規化
        targetDir = Path.GetFullPath(targetDir);
        historyDir = Path.GetFullPath(historyDir);
        if (logFile != null && !Path.IsPathRooted(logFile))
        {
            logFile = Path.Combine(Environment.CurrentDirectory, logFile);
        }

        options = new Options(targetDir, historyDir, logFile, silent, verbose);
        return true;
    }

    private static string GetDefaultTargetDir()
    {
        string? exeDir = null;
        try
        {
            exeDir = Path.GetDirectoryName(Environment.ProcessPath);
        }
        catch { }

        return string.IsNullOrEmpty(exeDir) ? Environment.CurrentDirectory : exeDir;
    }

    private static bool ValidateOptions(Options options, out string? errorMessage)
    {
        errorMessage = null;

        if (options.Silent && options.Verbose)
        {
            errorMessage = "-silent と -verbose は同時に指定できません。";
            return false;
        }

        if (!Directory.Exists(options.TargetDir))
        {
            errorMessage = "対象ディレクトリが存在しません: " + options.TargetDir;
            return false;
        }

        if (!Directory.Exists(options.HistoryDir))
        {
            errorMessage = "history ディレクトリが存在しません: " + options.HistoryDir;
            return false;
        }

        return true;
    }
}
