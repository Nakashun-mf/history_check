using Xunit;

namespace dupcheck.Tests;

/// <summary>
/// Checker のユニットテスト。
/// </summary>
public sealed class CheckerTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _targetDir;
    private readonly string _historyDir;

    public CheckerTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "dupcheck_tests_" + Guid.NewGuid().ToString("N")[..8]);
        _targetDir = Path.Combine(_tempRoot, "target");
        _historyDir = Path.Combine(_tempRoot, "history");
        Directory.CreateDirectory(_targetDir);
        Directory.CreateDirectory(_historyDir);
    }

    public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

    private void CreateFile(string dir, string fileName, string content = "")
    {
        string path = Path.Combine(dir, fileName);
        File.WriteAllText(path, content);
    }

    [Fact]
    public void 重複あり_完全一致()
    {
        CreateFile(_targetDir, "order.txt");
        CreateFile(_historyDir, "order.txt");

        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir);

        CheckResult r = Assert.Single(results);
        Assert.Equal("order.txt", r.FileName);
        Assert.True(r.IsDuplicate);
    }

    [Fact]
    public void 重複なし()
    {
        CreateFile(_targetDir, "order_new.txt");
        CreateFile(_historyDir, "order_old.txt");

        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir);

        CheckResult r = Assert.Single(results);
        Assert.Equal("order_new.txt", r.FileName);
        Assert.False(r.IsDuplicate);
    }

    [Fact]
    public void 拡張子なしファイルの照合()
    {
        CreateFile(_targetDir, "noext");
        CreateFile(_historyDir, "noext");

        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir);

        CheckResult r = Assert.Single(results);
        Assert.Equal("noext", r.FileName);
        Assert.True(r.IsDuplicate);
    }

    [Fact]
    public void 日本語ファイル名の照合()
    {
        CreateFile(_targetDir, "受注_20240105.txt");
        CreateFile(_historyDir, "受注_20240105.txt");

        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir);

        CheckResult r = Assert.Single(results);
        Assert.Equal("受注_20240105.txt", r.FileName);
        Assert.True(r.IsDuplicate);
    }

    [Fact]
    public void 大文字小文字が異なる場合は別物()
    {
        CreateFile(_targetDir, "ABC.txt");
        CreateFile(_historyDir, "abc.txt");

        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir);

        CheckResult r = Assert.Single(results);
        Assert.Equal("ABC.txt", r.FileName);
        // Ordinal 比較なので重複ではない
        Assert.False(r.IsDuplicate);
    }

    [Fact]
    public void targetが空ディレクトリ()
    {
        CreateFile(_historyDir, "only_in_history.txt");

        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir);

        Assert.Empty(results);
    }

    [Fact]
    public void historyが空ディレクトリ()
    {
        CreateFile(_targetDir, "new_file.txt");

        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir);

        CheckResult r = Assert.Single(results);
        Assert.Equal("new_file.txt", r.FileName);
        Assert.False(r.IsDuplicate);
    }

    [Fact]
    public void dupcheck_exeが除外される()
    {
        CreateFile(_targetDir, "dupcheck.exe");
        CreateFile(_targetDir, "other.txt");
        CreateFile(_historyDir, "other.txt");

        var excluded = new HashSet<string>(StringComparer.Ordinal) { Checker.ExeName };
        IReadOnlyList<CheckResult> results = Checker.Check(_targetDir, _historyDir, excluded);

        // dupcheck.exe は結果に含まれず、other.txt のみ
        CheckResult r = Assert.Single(results);
        Assert.Equal("other.txt", r.FileName);
        Assert.True(r.IsDuplicate);
    }
}
