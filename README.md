# dupcheck

受注ファイルの二重取込み防止チェックツール

RPAによる受注ファイル自動取り込み処理において、処理前ファイルが既に `history` フォルダに存在しないかをチェックする Windows 向け CLI ツール。

---

## 動作環境

- Windows 10 / 11 x64
- **単体 exe で配布する場合**: .NET のインストールは不要（exe にランタイムを含む自己完結型）
- 開発・ビルド時のみ: .NET 8 SDK

---

## ディレクトリ構成（前提）

```
root/
├── history/               ← 処理済みファイル格納先
│   ├── 受注_20240101.txt
│   └── 受注_20240102
├── 受注_20240103.txt      ← チェック対象（処理前ファイル）
├── 受注_20240104
└── dupcheck.exe
```

---

## 使い方

```
dupcheck.exe [オプション]
```

### オプション一覧

| オプション       | 省略 | デフォルト                        | 説明                                                                 |
|------------------|------|-----------------------------------|----------------------------------------------------------------------|
| `-target <dir>`  | 可   | exe と同じディレクトリ            | チェック対象（処理前ファイル）のディレクトリパス                     |
| `-history <dir>` | 可   | `.\history`                       | 処理済みファイルが格納されたディレクトリパス                         |
| `-log [<file>]`  | 可   | 出力しない                        | ログファイルパス。省略時は `dupcheck_YYYYMMDD_HHMMSS.log` を自動生成 |
| `-silent`        | 可   | 無効                              | コンソール出力を抑制。終了コードのみで結果を受け取る場合に使用       |
| `-verbose`       | 可   | 無効                              | 全ファイルの判定結果を出力（デフォルトは重複ファイルのみ）           |
| `-help`          | —    | —                                 | ヘルプを表示して終了                                                 |

> `-silent` と `-verbose` の併用はエラー（終了コード 2）

### 実行例

```bash
# 最小実行（同一ディレクトリ・./history を参照）
dupcheck.exe

# 対象・history を明示指定してログ出力あり
dupcheck.exe -target C:\orders -history C:\orders\history -log

# ログファイル名を指定
dupcheck.exe -log C:\logs\dupcheck.log

# RPA 連携用（コンソール出力なし・終了コードのみ）
dupcheck.exe -silent

# 全ファイルの結果を表示
dupcheck.exe -verbose

# ヘルプ
dupcheck.exe -help
```

---

## 終了コード

| コード | 条件                                   |
|--------|----------------------------------------|
| `0`    | 正常終了（重複なし）                   |
| `1`    | 重複ファイルあり                       |
| `2`    | 引数エラー・ディレクトリ不正など異常終了 |

---

## 出力例

```
[dupcheck] チェック開始: 2024-01-05 10:30:00
[dupcheck] 対象ディレクトリ : C:\orders
[dupcheck] historyディレクトリ: C:\orders\history

[重複] 受注_20240103.txt
[正常] 受注_20240105.txt    ← -verbose 時のみ表示

---
チェック対象: 3件 / 重複あり: 1件 / 正常: 2件
```

---

## 照合ルール

- ファイル名の**完全一致**（拡張子含む）で比較
- **大文字・小文字は区別する**（`ABC.txt` ≠ `abc.txt`）
- 拡張子なしファイルも正常に処理
- 日本語・全角文字を含むファイル名に対応（UTF-8）

## 除外対象

チェック対象から以下は自動的に除外される：

- `dupcheck.exe` 本体
- `history` フォルダ
- ログ出力先ファイル（同一ディレクトリの場合）

---

## プロジェクト構成

```
dupcheck/
├── src/
│   └── dupcheck/
│       ├── dupcheck.csproj
│       ├── Program.cs          # エントリポイント・引数解析
│       ├── Checker.cs          # ファイル照合ロジック
│       ├── CheckResult.cs      # 結果データクラス
│       ├── Options.cs          # オプション定義
│       └── Logger.cs           # コンソール・ファイル出力
└── tests/
    └── dupcheck.Tests/
        ├── dupcheck.Tests.csproj
        └── CheckerTests.cs     # ユニットテスト
```

---

## 単体 exe での配布（推奨）

**dupcheck.exe 1つだけを配布すれば、対象 PC でそのまま実行できます。.NET のインストールは不要です。**

### 発行コマンド

```bash
cd dupcheck
dotnet publish src/dupcheck/dupcheck.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### 出力先

- `dupcheck/src/dupcheck/bin/Release/net8.0/win-x64/publish/dupcheck.exe`

この **dupcheck.exe のみ**をコピーして配布・配置してください。同じフォルダに `history` を用意するか、`-target` / `-history` でパスを指定して実行します。

### 通常ビルド（開発用）

```bash
cd dupcheck
dotnet build
```

---

## テスト

```bash
dotnet test
```

---

## スコープ外

- ファイル内容（ハッシュ・バイナリ）による重複チェック
- GUI インターフェース
- history フォルダへのファイル移動・削除操作
- history サブフォルダの再帰検索（将来拡張として検討中）
