namespace dupcheck;

/// <summary>
/// コマンドライン引数のパース結果を保持するレコード。
/// </summary>
/// <param name="TargetDir">チェック対象（処理前ファイル）のディレクトリパス</param>
/// <param name="HistoryDir">処理済みファイルが格納されたディレクトリパス</param>
/// <param name="LogFile">ログファイルパス。null の場合はログ出力しない</param>
/// <param name="Silent">コンソール出力を完全に抑制する</param>
/// <param name="Verbose">全ファイルの判定結果を出力する</param>
public record Options(
    string TargetDir,
    string HistoryDir,
    string? LogFile,
    bool Silent,
    bool Verbose
);
