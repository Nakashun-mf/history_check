---
marp: true
theme: A4-Manual
paginate: true
---


<!-- ===================== COVER ===================== -->
<!-- _class: cover -->

<div class="cover-accent"></div>
<div class="cover-content">
  <div class="cover-category">Software Manual</div>
  <div class="cover-title">dupcheck</div>
  <div class="cover-subtitle">ファイル重複チェックツール</div>
  <div class="cover-divider"></div>
  <div class="cover-description">
    RPA ワークフローなどにおいて、<br>
    処理対象ファイルが既に <code style="background:rgba(255,255,255,0.1);color:#4facfe">history</code> フォルダに存在しないかを<br>
    高速・確実にチェックする Windows 向け CLI ツール。
  </div>
</div>
<div class="cover-meta">
  <div class="cover-meta-item">
    <strong>バージョン</strong>
    2.0.0
  </div>
  <div class="cover-meta-item">
    <strong>対象 OS</strong>
    Windows 10 / 11 x64
  </div>
  <div class="cover-meta-item">
    <strong>ランタイム</strong>
    .NET 8 内包（不要）
  </div>
  <div class="cover-meta-item">
    <strong>発行日</strong>
    2026年3月
  </div>
</div>

---

<!-- ===================== TOC ===================== -->
<!-- _class: toc -->

# 目次

<ul class="toc-list">
  <li class="toc-item"><span class="toc-num">1</span><span>概要と特長</span></li>
  <li class="toc-item"><span class="toc-num">2</span><span>動作環境とインストール</span></li>
  <li class="toc-item"><span class="toc-num">3</span><span>ディレクトリ構成</span></li>
  <li class="toc-item"><span class="toc-num">4</span><span>基本的な使い方</span></li>
  <li class="toc-item"><span class="toc-num">5</span><span>オプション詳細</span></li>
  <li class="toc-item"><span class="toc-num">6</span><span>出力フォーマット</span></li>
  <li class="toc-item"><span class="toc-num">7</span><span>終了コード</span></li>
  <li class="toc-item"><span class="toc-num">8</span><span>照合ルール</span></li>
  <li class="toc-item"><span class="toc-num">9</span><span>使用例</span></li>
  <li class="toc-item"><span class="toc-num">10</span><span>RPA 連携ガイド</span></li>
  <li class="toc-item"><span class="toc-num">11</span><span>トラブルシューティング</span></li>
</ul>

---

<!-- ===================== 1. 概要 ===================== -->

# 1. 概要と特長

dupcheck は、RPA ワークフローなどにおいて、**同一ファイルの二重処理を防止**するための Windows 向け軽量 CLI ツールです。

## 解決する課題

ファイルを処理するフローにおいて、処理済みファイルが再び取り込まれると重大なエラーにつながります。dupcheck は処理前に `history` フォルダとの照合を実施し、重複を確実に検出します。

## 主な特長

| 特長 | 説明 |
|------|------|
| **単体 exe 配布** | .NET ランタイムのインストール不要。exe 1 ファイルで動作 |
| **高速照合** | HashSet による O(1) 照合。大量ファイルでも遅延なし |
| **厳密比較** | 大文字・小文字を区別した完全一致。誤検出なし |
| **RPA 対応** | 終了コードで結果通知。`-silent` で出力を完全抑制 |
| **ログ出力** | UTF-8 BOM 付きで日本語ファイル名も確実に記録 |
| **シンプル設計** | 外部ライブラリ不使用。標準ライブラリのみで動作 |

---

<!-- ===================== 2. 動作環境 ===================== -->

# 2. 動作環境とインストール

## 動作環境

| 項目 | 要件 |
|------|------|
| OS | Windows 10 x64 / Windows 11 x64 |
| .NET ランタイム | **不要**（単体 exe 版は自己完結型） |
| ディスク容量 | 約 65 MB（ランタイム内包のため） |
| 権限 | チェック対象ディレクトリへの読み取り権限 |

## インストール手順

dupcheck は配置するだけで使用可能です。インストーラーは不要です。

**手順 1** — `dupcheck.exe` を任意のフォルダにコピーします。

```
C:\rpa\orders\
└── dupcheck.exe   ← ここに配置
```

**手順 2** — `history` フォルダを同じ場所に作成します（または `-history` オプションで指定）。

```
C:\rpa\orders\
├── history\       ← 処理済みファイルの格納先
└── dupcheck.exe
```

> **Note:** デフォルトでは exe と同じディレクトリを対象フォルダ、`.\history` を履歴フォルダとして使用します。

---

<!-- ===================== 3. ディレクトリ構成 ===================== -->

# 3. ディレクトリ構成

## 標準的なフォルダ構成

<div class="tree">
<span class="tree-dir">C:\data\files\</span><br>
├── <span class="tree-dir">history\</span>　　　　　　　<span class="tree-note">← 処理済みファイルの格納先</span><br>
│　　├── <span class="tree-file">data_20240101.txt</span><br>
│　　├── <span class="tree-file">data_20240102.txt</span><br>
│　　└── <span class="tree-file">data_20240103</span>　　<span class="tree-note">← 拡張子なしも対応</span><br>
├── <span class="tree-file">data_20240104.txt</span>　　<span class="tree-note">← チェック対象（処理前）</span><br>
├── <span class="tree-file">data_20240105.txt</span>　　<span class="tree-note">← チェック対象（処理前）</span><br>
└── <span class="tree-dir">dupcheck.exe</span>　　　　　<span class="tree-note">← 自動除外される</span>
</div>

## 検索スコープ

dupcheck はルート直下のファイルのみを対象とします。

| フォルダ | 検索範囲 | サブフォルダ |
|---------|---------|------------|
| 対象フォルダ（`-target`） | ルート直下のみ | 含まない |
| 履歴フォルダ（`-history`） | ルート直下のみ | 含まない |

## 自動除外されるファイル

以下はチェック対象から自動的に除外されます：

- `dupcheck.exe` — ツール本体
- `-log` 指定時の CSV ファイル（同一ディレクトリの場合）
- `-dupfile` 指定時のログファイル（同一ディレクトリの場合）

---

<!-- ===================== 4. 基本的な使い方 ===================== -->

# 4. 基本的な使い方

## コマンド構文

```
dupcheck.exe [オプション]
```

## 最もシンプルな実行

```cmd
dupcheck.exe
```

引数なしで実行すると、exe と同じディレクトリを対象フォルダ、`.\history` を履歴フォルダとして使用します。

## 実行の流れ

<div class="flow">
  <div class="flow-step">起動・<br>引数解析</div>
  <div class="flow-arrow">→</div>
  <div class="flow-step">ディレクトリ<br>検証</div>
  <div class="flow-arrow">→</div>
  <div class="flow-step">ファイル<br>一覧取得</div>
  <div class="flow-arrow">→</div>
  <div class="flow-step">HashSet<br>照合</div>
  <div class="flow-arrow">→</div>
  <div class="flow-step">結果出力・<br>終了コード</div>
</div>

## クイックスタート例

```cmd
REM 最小実行
dupcheck.exe

REM ディレクトリを明示指定
dupcheck.exe -target C:\data\files -history C:\data\files\history

REM ログを出力（ファイル名は自動生成）
dupcheck.exe -log

REM 全ファイルの判定結果を表示
dupcheck.exe -verbose
```

---

<!-- ===================== 5. オプション ===================== -->

# 5. オプション詳細

## オプション一覧

| オプション | 省略 | デフォルト | 説明 |
|-----------|------|-----------|------|
| `-target <dir>` | 可 | exe と同じディレクトリ | チェック対象ディレクトリのパス |
| `-history <dir>` | 可 | `.\history` | 処理済みファイルの格納ディレクトリパス |
| `-log [<file>]` | 可 | 出力しない | CSV 形式で追記。ファイル省略時は `dupcheck.csv` |
| `-dupfile [<file>]` | 可 | 出力しない | 人間が読みやすいブロック形式で追記。ファイル省略時は `dupcheck.log` |
| `-silent` | 可 | 無効 | stdout を含む全出力を抑制（終了コードのみ） |
| `-verbose` | 可 | 無効 | 全ファイルの判定結果を `-dupfile` / `-log` に出力 |
| `-help` | — | — | ヘルプを表示して終了（終了コード `0`） |

## `-log` / `-dupfile` オプションの挙動

どちらもファイルパスを省略できます：

```cmd
REM デフォルトファイル名で追記
dupcheck.exe -log          REM → dupcheck.csv に追記
dupcheck.exe -dupfile      REM → dupcheck.log に追記

REM ファイル名を明示指定
dupcheck.exe -log C:\logs\result.csv
dupcheck.exe -dupfile C:\logs\detail.log
```

ファイルが存在しない場合は新規作成、存在する場合は追記します。CSV ヘッダーは新規作成時のみ書き出します。

## 排他オプション

> **重要:** `-silent` と `-verbose` は同時に指定できません。両方指定した場合はエラーメッセージを表示して終了コード `2` で終了します。

---

<!-- ===================== 6. 出力フォーマット ===================== -->

# 6. 出力フォーマット

dupcheck には用途別に 3 種類の出力があります。

## stdout（常時）

**重複ファイル名のみ**を 1 行 1 ファイル名で出力します。RPA がそのまま読み取れます。`-silent` 時は出力しません。

```
data_20240103.txt
data_20240104
```

重複ゼロの場合は何も出力しません（終了コード `0` で判断できます）。

## `-dupfile`（人間向けログ・追記）

実行ごとのブロック形式で追記します。PC 上でテキストエディタを使って過去の実行履歴を確認できます。

```
=====================================
実行: 2024-01-05 10:30:00
対象: C:\data\files
history: C:\data\files\history
-------------------------------------
[重複] data_20240103.txt
-------------------------------------
合計: 5件 / 重複: 1件 / 正常: 4件
=====================================
```

`-verbose` を付けると正常ファイルも `[正常]` として記録されます。

## `-log`（CSV・追記）

Excel での分析用に CSV 形式で追記します。ヘッダーは新規作成時のみ書き出します。

```csv
実行日時,対象ディレクトリ,historyディレクトリ,ファイル名,判定
2024-01-05 10:30:00,C:\data\files,C:\data\files\history,data_20240103.txt,重複
2024-01-06 09:15:00,C:\data\files,C:\data\files\history,data_20240106.txt,重複
```

`-verbose` を付けると正常ファイルも記録されます。

## ファイル出力の共通仕様

| 項目 | 仕様 |
|------|------|
| エンコーディング | UTF-8 BOM 付き（新規作成時）/ UTF-8 BOM なし（追記時） |
| 改行コード | CRLF（Windows 標準） |
| ファイルモード | 追記（ファイルが存在しない場合は新規作成） |

---

<!-- ===================== 7. 終了コード ===================== -->

# 7. 終了コード

dupcheck は処理結果を終了コード（ERRORLEVEL）で通知します。RPA やバッチから終了コードを参照することで、後続処理の分岐が可能です。

<div class="exit-grid">
  <div class="exit-card ok">
    <div class="exit-code">0</div>
    <div class="exit-label">正常終了</div>
    <div class="exit-desc">重複ファイルなし。<br>処理を続行できます。</div>
  </div>
  <div class="exit-card warn">
    <div class="exit-code">1</div>
    <div class="exit-label">重複あり</div>
    <div class="exit-desc">1件以上の重複を<br>検出しました。</div>
  </div>
  <div class="exit-card error">
    <div class="exit-code">2</div>
    <div class="exit-label">エラー</div>
    <div class="exit-desc">引数エラー・ディレクトリ<br>不正・例外が発生しました。</div>
  </div>
</div>

## バッチファイルでの参照例

```bat
dupcheck.exe -silent
IF %ERRORLEVEL% EQU 0 (
    echo 重複なし。処理を続行します。
    GOTO :PROCESS
)
IF %ERRORLEVEL% EQU 1 (
    echo 重複ファイルを検出しました。処理を中断します。
    GOTO :ABORT
)
echo エラーが発生しました。
GOTO :ERROR
```

> `-silent` と組み合わせることで、コンソール出力なしに終了コードのみで結果を取得できます。

---

<!-- ===================== 8. 照合ルール ===================== -->

# 8. 照合ルール

## 基本ルール

dupcheck はファイル名の**完全一致**でのみ照合します。ファイルの内容・サイズ・更新日時は比較しません。

| ルール | 内容 |
|--------|------|
| 比較対象 | ファイル名のみ（フルパス比較は行わない） |
| 大文字・小文字 | **区別する**（`ABC.txt` ≠ `abc.txt`） |
| 比較アルゴリズム | `StringComparison.Ordinal`（高速・バイト単位） |
| 照合データ構造 | `HashSet<string>` — O(1) 検索 |
| 対象範囲 | ルート直下のみ（サブフォルダは検索しない） |
| 拡張子なし | 正常に処理（拡張子ありと別ファイルとして扱う） |
| 日本語ファイル名 | 対応（UTF-8 環境） |

## 照合例

| ファイル名（target） | history に存在 | 判定 |
|---------------------|----------------|------|
| `data_20240105.txt` | 存在しない | <span class="badge badge-green">正常</span> |
| `data_20240103.txt` | 存在する | <span class="badge badge-red">重複</span> |
| `ABC.txt` | `abc.txt` が存在 | <span class="badge badge-green">正常</span>（別ファイル） |
| `data_20240104` | `data_20240104` が存在 | <span class="badge badge-red">重複</span> |

---

<!-- ===================== 9. 使用例 ===================== -->

# 9. 使用例

## 例 1 — 最小実行

exe と同じフォルダを対象に実行します。重複があれば stdout にファイル名が出力されます。

```cmd
C:\data\files> dupcheck.exe
data_20240103.txt
```

重複ゼロの場合は何も出力されません（終了コード `0`）。

## 例 2 — 全出力を使う

```cmd
dupcheck.exe -target C:\data\files -history C:\data\files\history -dupfile -log
```

- stdout: 重複ファイル名のみ（RPA が読む）
- `dupcheck.log`: 人間向けブロック形式（追記）
- `dupcheck.csv`: Excel 分析用 CSV（追記）

## 例 3 — ファイル名を明示指定

```cmd
dupcheck.exe -log C:\logs\result.csv -dupfile C:\logs\detail.log
```

## 例 4 — 終了コードのみで判定（RPA 向け）

```cmd
dupcheck.exe -silent
```

stdout を含む全出力を抑制し、終了コードのみで結果を通知します。

---

<!-- ===================== 10. RPA 連携 ===================== -->

# 10. RPA 連携ガイド

## 出力の使い分け

| 出力 | 取得方法 | 用途 |
|------|---------|------|
| stdout | コマンド実行ノードの出力取得 | 重複ファイル名の一覧をフロー内で処理 |
| `-dupfile` | テキストファイル読み取り | PC 上での目視確認・証跡保管 |
| `-log` | CSV ファイル読み取り | Excel での定期集計・分析 |
| 終了コード | ERRORLEVEL / 戻り値 | フローの分岐判定 |

## 推奨構成

```cmd
dupcheck.exe -target {対象フォルダ} -history {historyフォルダ} -dupfile -log
```

RPA は stdout から重複ファイル名を取得し、フロー内で後続処理（ファイル退避・通知など）を行います。

## RPA での実装イメージ

```
[コマンド実行]
  コマンド: C:\tools\dupcheck.exe
  引数: -target C:\data\files -dupfile C:\logs\detail.log -log C:\logs\result.csv
  stdout 出力: → 変数「dupNames」に格納
  終了コード: → 変数「exitCode」に格納

[条件分岐]
  exitCode = 0 → [ファイル処理フロー]
  exitCode = 1 → dupNames の各ファイルを退避 → [重複通知]
  exitCode = 2 → [エラー通知・管理者連絡]
```

> **ヒント:** `-dupfile` と `-log` のパスを共有フォルダに設定すると、担当者がリアルタイムで確認・集計できます。

---

<!-- ===================== 11. トラブルシューティング ===================== -->

# 11. トラブルシューティング

## よくある問題と対処法

| 症状 | 原因 | 対処法 |
|------|------|--------|
| 終了コード `2` が返る | ディレクトリが存在しない | `-target`・`-history` のパスを確認 |
| 終了コード `2` が返る | `-silent` と `-verbose` を同時指定 | いずれか一方のみ指定する |
| 重複が検出されない | 大文字・小文字の違い | ファイル名を正確に確認（区別する） |
| 重複が検出されない | サブフォルダにファイルがある | ルート直下にファイルを移動する |
| 文字化けが発生 | ログビューアのエンコーディング | UTF-8 BOM 付きで開く設定にする |
| exe が起動しない | セキュリティソフトによるブロック | 信頼済みアプリとして登録する |

## エラーメッセージ一覧

| メッセージ | 意味 |
|-----------|------|
| `ディレクトリが存在しません: <path>` | 指定パスが見つからない |
| `-silent と -verbose は同時に指定できません` | 排他オプションの競合 |
| `不明なオプション: <option>` | タイポまたは未対応オプション |

## ヘルプの表示

```cmd
dupcheck.exe -help
```

---

<!-- ===================== BACK COVER ===================== -->
<!-- _class: cover -->

<div class="cover-accent"></div>
<div class="cover-content" style="justify-content:center;align-items:center;text-align:center">
  <div style="font-size:64px;font-weight:800;color:rgba(79,172,254,0.15);line-height:1;margin-bottom:24px">dupcheck</div>
  <div style="width:48px;height:3px;background:linear-gradient(90deg,#e94560,#f5a623);margin:0 auto 28px;border-radius:2px"></div>
  <div style="font-size:15px;color:#a8c8f0;line-height:2">
    ファイル重複チェックツール<br>
    Version 2.0.0
  </div>
  <div style="margin-top:48px;font-size:12px;color:#4a6a8a">
    本ドキュメントの内容は予告なく変更される場合があります。
  </div>
</div>
<div class="cover-meta">
  <div class="cover-meta-item">
    <strong>ライセンス</strong>
    MIT
  </div>
  <div class="cover-meta-item">
    <strong>対応プラットフォーム</strong>
    Windows 10 / 11 x64
  </div>
  <div class="cover-meta-item">
    <strong>技術スタック</strong>
    C# 12 / .NET 8
  </div>
</div>
