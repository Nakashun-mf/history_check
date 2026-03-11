# dupcheck

RPA 受注ファイル取り込み処理で、処理前ファイルが `history` フォルダに既に存在しないかをチェックする Windows 向け CLI ツール。

- **言語**: C# 12 / .NET 8
- **配布**: win-x64 単一 exe（.NET インストール不要の自己完結型）

## 使い方

```bash
dupcheck.exe [オプション]
```

| オプション       | 説明 |
|------------------|------|
| `-target <dir>`  | チェック対象ディレクトリ（省略時: exe と同じディレクトリ） |
| `-history <dir>` | history ディレクトリ（省略時: `.\history`） |
| `-log [<file>]`  | ログ出力。ファイル省略時は `dupcheck_YYYYMMDD_HHMMSS.log` |
| `-silent`        | コンソール出力を抑制 |
| `-verbose`       | 全ファイルの判定結果を表示 |
| `-help`          | ヘルプ表示 |

**終了コード**: `0` = 重複なし, `1` = 重複あり, `2` = エラー

## ビルド・発行

```bash
cd dupcheck
dotnet build
dotnet test

# 単体 exe で発行（配布用・.NET 不要）
dotnet publish src/dupcheck/dupcheck.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

- **発行先**: `src/dupcheck/bin/Release/net8.0/win-x64/publish/dupcheck.exe`
- **配布**: 上記 **dupcheck.exe のみ**をコピーして渡せば、対象 PC に .NET がなくても実行可能です。

## 開発者向け

- ソリューション: `dupcheck.sln`
- メインプロジェクト: `src/dupcheck/`
- テスト: `tests/dupcheck.Tests/`（xunit）
- 実装仕様: ルートの `implementation.mdc` を参照
