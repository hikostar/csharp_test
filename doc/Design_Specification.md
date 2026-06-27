# 設計仕様書 - JsonEditor

**作成日**: 2026-06-25  
**対象プロジェクト**: JsonEditor  
**対象バージョン**: .NET 8 / WPF

---

## 1. 目的

本書は JsonEditor の現行実装に基づき、アプリケーション設計を保守・改修可能な粒度で定義する。

- レイヤー構成と責務を明確化する
- 主要コンポーネント間の依存関係を明文化する
- 設定・保存・復元・検索置換などの仕様を固定する
- 将来拡張の方向性を現行仕様と分離して提示する

---

## 2. 適用範囲

### 2.1 対象

- `src/JsonEditor.App`
- `src/JsonEditor.Core`
- `tests/JsonEditor.App.Tests`（設計整合の裏付けとして参照）

### 2.2 非対象

- インストーラー作成手順の詳細
- CI/CD パイプライン設計
- 外部連携（クラウド保存、プラグイン機構など）

---

## 3. システム概要

JsonEditor は JSON テキスト編集と JSON 構造確認を目的とした WPF デスクトップアプリケーションである。主機能は以下。

- JSON テキスト編集（AvalonEdit）
- リアルタイム JSON バリデーション
- JSON ツリービュー表示
- 検索/置換（文字列、正規表現、大小文字条件）
- 次/前マッチ移動（ラップアラウンド）
- 置換プレビュー（最大 30 件）
- ファイル開く/保存/別名保存
- 自動保存バックアップ（`.autosave`）と復元
- ライト/ダークテーマ切替

---

## 4. アーキテクチャ

### 4.1 レイヤー構成

1. Presentation 層: `JsonEditor.App`
- 画面表示、ユーザー入力処理、表示状態管理
- `MainWindow` と `MainViewModel` が中心

2. Domain/Application Logic 層: `JsonEditor.Core`
- JSON 検証、ツリー生成、検索置換、設定永続化

3. Test 層: `JsonEditor.App.Tests`
- Core サービスおよび一部 App クラスの単体テスト

### 4.2 依存方向

- `JsonEditor.App` -> `JsonEditor.Core`
- `JsonEditor.App.Tests` -> `JsonEditor.Core`（および App の一部）
- `JsonEditor.Core` は WPF 非依存

### 4.3 設計上の現状

- DI コンテナは未導入
- `MainViewModel` 内でサービスを直接 `new` している
- UI イベントと ViewModel 状態同期は `MainWindow.xaml.cs` で実装

---

## 5. 起動・終了シーケンス

### 5.1 起動

1. `App.OnStartup` が `MainWindow` を生成・表示
2. `MainWindow.DataContext` の `MainViewModel` を取得
3. `MainViewModel.InitializeAsync` 実行
4. 設定ファイルをロードし、テーマ/検索設定/自動保存間隔を適用
5. `MainWindow.ApplyTheme` でテーマリソース反映

### 5.2 終了

1. `App.OnExit` で `MainViewModel.ShutdownAsync` を実行
2. 現在設定を `settings.json` へ保存
3. 自動保存タイマーを破棄

---

## 6. コンポーネント仕様

### 6.1 Presentation 層

#### MainWindow

- 役割
  - メニュー、検索バー、エディタ、ツリー、プレビュー、ステータスバーを表示
  - エディタテキスト変更を ViewModel に反映
  - ViewModel の選択位置をエディタ選択へ同期
  - テーマリソース差し替えを実行

- UI 構成
  - Menu: File/Edit/View
  - 検索バー: Find/Replace、Prev/Next/Preview、Regex/Match Case/Dark Theme
  - メイン左: AvalonEdit
  - メイン右上: TreeView
  - メイン右下: 置換プレビュー ListBox
  - 下部: ステータス + Autosave 秒数

#### MainViewModel

- 役割
  - 画面状態管理
  - コマンド処理
  - Core サービス呼び出し
  - 自動保存ループ管理

- 公開コマンド
  - `OpenFileCommand`
  - `SaveFileCommand`
  - `SaveAsFileCommand`
  - `RestoreBackupCommand`
  - `ValidateCommand`
  - `ReplaceAllCommand`
  - `NextMatchCommand`
  - `PreviousMatchCommand`
  - `BuildReplacePreviewCommand`
  - `ToggleThemeCommand`

- 主要状態
  - テキスト: `JsonText`, `SearchText`, `ReplaceText`
  - 検索条件: `IsRegexSearch`, `MatchCaseSearch`
  - テーマ: `IsDarkTheme`
  - 選択位置: `SelectedMatchStart`, `SelectedMatchLength`
  - 保存関連: `CurrentFilePath`, `AutoSaveIntervalSeconds`
  - 表示関連: `StatusMessage`, `ReplacePreviewSummary`

### 6.2 Core 層

#### JsonValidationService

- `Validate(string text)`
- 仕様
  - 空/空白は有効 JSON として扱う
  - `JsonDocument.Parse` 成功時は `IsValid = true`
  - 失敗時は `JsonException.Message` と `LineNumber`/`BytePositionInLine` を返す

#### JsonTreeBuilder

- `Build(string jsonText)`
- 仕様
  - 空/空白は `null` を返す
  - ルートラベルは `$`
  - Object はプロパティごと、Array は `[index]` ごとに子ノード生成
  - 値表示は型ごとに `FormatLabel` で整形

#### SearchReplaceService

- 機能
  - `FindNextMatch`
  - `FindPreviousMatch`
  - `BuildReplacePreview`
  - `CountMatches`
  - `ReplaceAll`

- 仕様
  - 文字列検索時は `Ordinal` / `OrdinalIgnoreCase`
  - 正規表現検索時は `RegexOptions.Compiled | Multiline`（必要時 IgnoreCase）
  - 正規表現タイムアウトは 250ms
  - 次/前検索はラップアラウンドあり
  - プレビューは呼び出し側で最大件数指定（現行 30）

#### AppSettingsStore

- `LoadAsync(filePath)`
  - ファイル未存在時はデフォルト設定を返す
- `SaveAsync(filePath, settings)`
  - 親ディレクトリを作成し JSON 保存（整形あり）

### 6.3 データモデル

- `AppSettings`
  - `Theme`（初期値 `Light`）
  - `AutoSaveIntervalSeconds`（初期値 `30`）
  - `UseRegexSearch`
  - `MatchCaseSearch`

- `SearchOptions`
  - `SearchText`, `ReplaceText`, `MatchCase`, `UseRegex`

- `JsonValidationResult`
  - `IsValid`, `ErrorMessage`, `LineNumber`, `BytePositionInLine`

- `JsonTreeNode`
  - `Label`, `Children`

- `ReplacePreviewItem`
  - `Start`, `Length`, `OriginalText`, `ReplacementText`

---

## 7. 機能仕様詳細

### 7.1 ファイル操作

- Open
  - JSON/全ファイルフィルターで選択
  - 既存 `.autosave` が元ファイルより新しい場合、復元確認ダイアログを表示
- Save
  - `CurrentFilePath` に上書き
- Save As
  - 保存先を選択後に Save 実行

### 7.2 検索・置換

- `SearchText` 変更時に一致件数を再計算
- `Next/Previous` 実行で選択範囲を更新
- `ReplaceAll` 実行後は検索件数を再計算
- 例外（不正 Regex など）は `StatusMessage` に表示

### 7.3 置換プレビュー

- `BuildReplacePreview` 実行時に一覧再生成
- 表示形式: `開始位置: '元文字列' => '置換文字列'`
- 要約表示: `Preview: N match(es)`
- 件数上限超過時: `Preview: N match(es) (showing 30)`

### 7.4 JSON 検証とツリー更新

- `JsonText` 更新時に検証実行
- 有効時: `Valid JSON` + ツリー再構築
- 無効時: エラーメッセージ表示（行・列情報がある場合は `Line`/`Column` を付与、ツリーは再生成しない）

### 7.5 自動保存

- `PeriodicTimer` ベースで周期監視
- 条件
  - `CurrentFilePath` が空でない
  - 最終編集から `AutoSaveIntervalSeconds` 未満
- 出力先: `{CurrentFilePath}.autosave`
- 間隔最小値: 5 秒（未満入力は無視）

### 7.6 テーマ

- Light/Dark の ResourceDictionary を切り替え
- 起動時に設定値を復元
- 終了時に設定値を保存

---

## 8. 永続化仕様

### 8.1 設定ファイル

- 保存先: `%LocalAppData%/JsonEditor/settings.json`
- 保存タイミング: アプリ終了時
- 読み込みタイミング: アプリ起動時

### 8.2 バックアップファイル

- 保存先: 開いている JSON と同じ場所
- ファイル名: 元ファイル名 + `.autosave`
- 復元方式
  - ファイルオープン時の自動提案
  - メニューからの手動復元

---

## 9. エラーハンドリング方針

- 例外は極力 UI クラッシュにせず `StatusMessage` で通知
- 主な通知メッセージ
  - `Search error: ...`
  - `Replace error: ...`
  - `Preview error: ...`
  - `Autosave failed: ...`
  - `No backup found`

---

## 10. 非機能・制約

- 想定対象サイズ: 約 10MB
- JSON Schema 検証: 未実装
- 置換プレビュー: テキスト一覧のみ（差分ビュー未実装）
- ショートカットキー: 専用定義なし
- 複数ファイル同時編集: 1 ウィンドウ 1 ファイル前提

---

## 11. テスト方針と現状

### 11.1 テスト実装済み

- `JsonTreeBuilderTests`
- `JsonValidationServiceTests`
- `SearchReplaceServiceTests`
- `RelayCommandTests`
- `JsonTreeNodeViewModelTests`

### 11.2 テスト上の課題

- `MainViewModel` は依存直生成のため単体テストしにくい
- WPF UI 連携（`MainWindow.xaml.cs`）は自動テスト対象外が多い

---

## 12. 将来拡張案（提案）

本章は現行仕様ではなく、改善提案である。

### 12.1 優先度 High

1. DI 導入
- `MainViewModel` の依存をコンストラクタ注入へ移行
- テスト容易性と責務分離を改善

2. ViewModel テスト拡張
- 検索ナビゲーション、自動保存条件、復元分岐のユニットテスト追加

### 12.2 優先度 Medium

1. JSON Schema 検証
- 形式妥当性に加えてスキーマ検証をサポート

2. キーボードショートカット
- Open/Save/Find/Replace などの操作効率を向上

3. 置換プレビュー改善
- 差分表示、対象行へのジャンプ機能

### 12.3 優先度 Low

1. 大容量ファイル最適化
- 部分解析や仮想化による応答性改善

2. 複数ドキュメント対応
- タブ管理による複数ファイル編集

## 13. Copilot適用時の設計制約

本章は AI 支援（Copilot/Agent/Skill）での実装時に必ず守る制約を定義する。

### 13.1 依存方向

1. `JsonEditor.Core` は WPF 参照を持たない
2. `JsonEditor.App` から `JsonEditor.Core` への一方向依存を維持する
3. テストは本番コードの内部実装ではなく公開挙動を検証する

### 13.2 変更粒度

1. UI 表示都合の修正で Core の責務を増やさない
2. 検索/置換/検証ロジックは Core 側へ集約する
3. 例外処理方針（クラッシュ回避 + `StatusMessage` 通知）を崩さない

### 13.3 非機能制約

1. 想定サイズ 10MB 前提の操作応答性を悪化させない
2. 正規表現検索のタイムアウト設定（250ms）を維持する
3. 自動保存の最小間隔 5 秒制約を維持する

### 13.4 変更時の必須確認

1. 本書 6/7/8/10 章に仕様差分がないか確認する
2. 仕様差分がある場合は同一変更で本書を更新する
3. 検証手順は [検証仕様兼結果報告書](doc/Verification_Spec&result.md) に追記する

### 13.5 Copilot運用リファレンス

1. リポジトリ制約は `.instructions.md` を参照する
2. PR本文作成は `.github/pull_request_template.md` と `.github/prompts/pr-authoring.prompt.md` を使用する
3. PRレビュー観点は `.agent.md` と `.github/skills/pr-review/SKILL.md` を使用する
4. 開発支援は `.prompt.md` と `.github/skills/dev-support/SKILL.md` を使用する

## 14. 変更管理ルール（ドキュメント運用）

- 新機能追加時
  - 本書の 6, 7, 8, 10 章を更新
- 不具合修正時
  - 9 章（エラー方針）と 11 章（テスト）を更新
- 仕様変更時
  - 「現行仕様」と「将来拡張案」の混同を避ける
