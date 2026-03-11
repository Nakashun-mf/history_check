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
  <div class="cover-subtitle">受注ファイル二重取込み防止チェックツール</div>
  <div class="cover-divider"></div>
  <div class="cover-description">
    RPAによる受注ファイル自動取り込み処理において、<br>
    処理前ファイルが既に <code style="background:rgba(255,255,255,0.1);color:#4facfe">history</code> フォルダに存在しないかを<br>
    高速・確実にチェックする Windows 向け CLI ツール。
  </div>
</div>
<div class="cover-meta">
  <div class="cover-meta-item">
    <strong>バージョン</strong>
    1.0.0
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

dupcheck は、RPA による受注ファイル自動取り込みフローにおいて、**同一ファイルの二重処理を防止**するための Windows 向け軽量 CLI ツールです。

## 解決する課題

RPA が受注ファイルを取り込む際、処理済みファイルが再び取り込まれると業務上の重大なエラーにつながります。dupcheck は取り込み前に `history` フォルダとの照合を実施し、重複を確実に検出します。

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
<span class="tree-dir">C:\rpa\orders\</span><br>
├── <span class="tree-dir">history\</span>　　　　　　　<span class="tree-note">← 処理済みファイルの格納先</span><br>
│　　├── <span class="tree-file">受注_20240101.txt</span><br>
│　　├── <span class="tree-file">受注_20240102.txt</span><br>
│　　└── <span class="tree-file">受注_20240103</span>　　<span class="tree-note">← 拡張子なしも対応</span><br>
├── <span class="tree-file">受注_20240104.txt</span>　　<span class="tree-note">← チェック対象（処理前）</span><br>
├── <span class="tree-file">受注_20240105.txt</span>　　<span class="tree-note">← チェック対象（処理前）</span><br>
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
- `-log` 指定時のログファイル（同一ディレクトリの場合）

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
dupcheck.exe -target C:\orders -history C:\orders\history

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
| `-history <dir>` | 可 | `.\history` | 処理済みファイルのディレクトリパス |
| `-log [<file>]` | 可 | 出力しない | ログファイルパス（省略時は自動生成） |
| `-silent` | 可 | 無効 | コンソール出力を完全に抑制 |
| `-verbose` | 可 | 無効 | 全ファイルの判定結果を出力 |
| `-help` | — | — | ヘルプを表示して終了（終了コード `0`） |

## `-log` オプションの挙動

`-log` はファイルパスを省略できます：

```cmd
REM ファイル名を自動生成: dupcheck_20240105_103000.log
dupcheck.exe -log

REM ファイル名を明示指定
dupcheck.exe -log C:\logs\dupcheck.log
```

## 排他オプション

> **重要:** `-silent` と `-verbose` は同時に指定できません。両方指定した場合はエラーメッセージを表示して終了コード `2` で終了します。

---

<!-- ===================== 6. 出力フォーマット ===================== -->

# 6. 出力フォーマット

## 標準出力（デフォルト）

重複ファイルのみ `[重複]` として表示されます。

```
[dupcheck] チェック開始: 2024-01-05 10:30:00
[dupcheck] 対象ディレクトリ : C:\rpa\orders
[dupcheck] historyディレクトリ: C:\rpa\orders\history

[重複] 受注_20240103.txt

---
チェック対象: 5件 / 重複あり: 1件 / 正常: 4件
```

## `-verbose` 時の出力

正常ファイルも `[正常]` として全件表示されます。

```
[dupcheck] チェック開始: 2024-01-05 10:30:00
[dupcheck] 対象ディレクトリ : C:\rpa\orders
[dupcheck] historyディレクトリ: C:\rpa\orders\history

[重複] 受注_20240103.txt
[正常] 受注_20240104.txt
[正常] 受注_20240105.txt

---
チェック対象: 3件 / 重複あり: 1件 / 正常: 2件
```

## ログファイルの仕様

| 項目 | 仕様 |
|------|------|
| エンコーディング | UTF-8 BOM 付き |
| 改行コード | CRLF（Windows 標準） |
| ファイルモード | 新規作成（既存ファイルは上書き） |
| 内容 | コンソール出力と同じ |

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
| `受注_20240105.txt` | 存在しない | <span class="badge badge-green">正常</span> |
| `受注_20240103.txt` | 存在する | <span class="badge badge-red">重複</span> |
| `ABC.txt` | `abc.txt` が存在 | <span class="badge badge-green">正常</span>（別ファイル） |
| `受注_20240104` | `受注_20240104` が存在 | <span class="badge badge-red">重複</span> |

---

<!-- ===================== 9. 使用例 ===================== -->

# 9. 使用例

## 例 1 — 最小実行

exe と同じフォルダを対象に実行します。

```cmd
C:\rpa\orders> dupcheck.exe
[dupcheck] チェック開始: 2024-01-05 10:30:00
[dupcheck] 対象ディレクトリ : C:\rpa\orders
[dupcheck] historyディレクトリ: C:\rpa\orders\history

チェック対象: 3件 / 重複あり: 0件 / 正常: 3件
```

## 例 2 — パスを明示してログ出力あり

```cmd
dupcheck.exe -target C:\orders -history C:\orders\history -log
```

自動生成されるログファイル名: `dupcheck_20240105_103000.log`

## 例 3 — ログファイル名を指定

```cmd
dupcheck.exe -target C:\orders -log C:\logs\result.log
```

## 例 4 — 全ファイルの判定結果を表示

```cmd
dupcheck.exe -verbose
[重複] 受注_20240103.txt
[正常] 受注_20240104.txt
[正常] 受注_20240105.txt

---
チェック対象: 3件 / 重複あり: 1件 / 正常: 2件
```

---

<!-- ===================== 10. RPA 連携 ===================== -->

# 10. RPA 連携ガイド

## 推奨構成

RPA から dupcheck を呼び出す際は、`-silent` オプションと終了コードの組み合わせを推奨します。

```cmd
dupcheck.exe -target {対象フォルダ} -history {historyフォルダ} -silent -log
```

| 設定 | 理由 |
|------|------|
| `-silent` | RPA がコンソール出力を誤処理しないよう抑制 |
| `-log` | 終了コードだけでは追跡困難な場合の証跡確保 |
| 終了コードで分岐 | 信頼性の高い結果判定 |

## UiPath / WinActor での実装イメージ

```
[コマンド実行]
  コマンド: C:\rpa\tools\dupcheck.exe
  引数: -target C:\orders -silent -log C:\logs\dup.log
  終了コード: → 変数「exitCode」に格納

[条件分岐]
  exitCode = 0 → [受注処理フロー]
  exitCode = 1 → [重複通知・処理中断]
  exitCode = 2 → [エラー通知・管理者連絡]
```

> **ヒント:** ログファイルを共有フォルダに出力するよう `-log` パスを設定すると、担当者がリアルタイムで確認できます。

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
    受注ファイル二重取込み防止チェックツール<br>
    Version 1.0.0
  </div>
  <div style="margin-top:48px;font-size:12px;color:#4a6a8a">
    本ドキュメントの内容は予告なく変更される場合があります。
  </div>
</div>
<div class="cover-meta">
  <div class="cover-meta-item">
    <strong>ライセンス</strong>
    社内利用限定
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
