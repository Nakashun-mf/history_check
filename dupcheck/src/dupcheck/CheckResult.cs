namespace dupcheck;

/// <summary>
/// 照合結果を表すレコード。
/// </summary>
/// <param name="FileName">ファイル名</param>
/// <param name="IsDuplicate">history に同名ファイルが存在する場合 true</param>
public record CheckResult(
    string FileName,
    bool IsDuplicate
);
