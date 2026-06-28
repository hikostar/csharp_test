# 要求仕様書（逆説定義）- JsonEditor

作成日: 2026-06-28  
対象: JsonEditor (.NET 8 / WPF)  
定義方針: 現行実装と既存テストを根拠に、要求を逆算定義する。

## 1. Goal

- 現行実装でユーザーに提供されている価値を、検証可能な要求として固定する。
- 要求ごとに実装証跡とテスト証跡を付与し、追跡可能性を確保する。
- 将来計画を本文要求から分離し、現時点の合意範囲を明確化する。

## 2. Non-goals

- 未実装機能の要求化（JSON Schema 検証、複数タブ編集など）。
- CI 運用改善計画（段階11）の本文要求化。
- 実装方式の再設計（DI 全面導入など）。

## 3. Constraints

- 対象は 1 ウィンドウ 1 ファイル編集を前提とする。
- 想定データサイズは約 10MB とする。
- 自動保存間隔は 5 秒未満を受け付けない。
- 検索/置換の正規表現評価は .NET Regex を用いる。
- 置換プレビューは最大 30 件まで表示する。

## 4. Functional Requirements

### 4.1 ファイル操作

- FR-FILE-01: ユーザーは JSON ファイルを開けること。
- FR-FILE-02: 開く対象に新しい .autosave が存在する場合、復元可否を選択できること。
- FR-FILE-03: ユーザーは現在ファイルへ上書き保存できること。
- FR-FILE-04: ユーザーは保存先を指定して別名保存できること。
- FR-FILE-05: ユーザーは明示操作でバックアップを復元できること。

### 4.2 JSON 検証とツリー

- FR-VAL-01: 編集中テキストはリアルタイムに JSON 妥当性検証されること。
- FR-VAL-02: 不正 JSON 時はエラー内容と位置情報を表示すること。
- FR-TREE-01: 有効 JSON 時はルート $ のツリーを再構築すること。
- FR-TREE-02: Object/Array/Primitive を階層化し、型に応じた表示ラベルを提供すること。

### 4.3 検索/置換

- FR-SR-01: 文字列検索と正規表現検索を切り替えられること。
- FR-SR-02: 大文字小文字の一致条件を切り替えられること。
- FR-SR-03: 次/前検索はラップアラウンド動作を提供すること。
- FR-SR-04: 全置換を実行し、結果テキストを更新できること。
- FR-SR-05: 検索件数を表示できること。

### 4.4 置換プレビュー

- FR-PRE-01: 置換前に置換候補プレビューを生成できること。
- FR-PRE-02: プレビュー項目は開始位置、元文字列、置換文字列を表示すること。
- FR-PRE-03: プレビュー表示件数上限を超える場合、要約に表示中件数を明示すること。

### 4.5 テーマ・設定・自動保存

- FR-THEME-01: ライト/ダークテーマを切り替えられること。
- FR-THEME-02: テーマ変更時にエディタ構文ハイライトも追従すること。
- FR-SET-01: テーマ、検索条件、自動保存間隔を設定として保存/復元できること。
- FR-AUTO-01: 自動保存条件を満たす場合、.autosave バックアップを周期生成すること。
- FR-AUTO-02: 自動保存失敗時は失敗メッセージを通知すること。

## 5. Non-functional Requirements

- NFR-PERF-01: 大規模 JSON 検証は性能テスト予算内で完了すること。
- NFR-PERF-02: 大規模テキストの検索件数計算は性能テスト予算内で完了すること。
- NFR-PERF-03: 大規模テキストの全置換は性能テスト予算内で完了すること。
- NFR-PERF-04: 正規表現プレビュー生成は性能テスト予算内で完了すること。
- NFR-PLAT-01: Windows 上の .NET 8 WPF アプリとして動作すること。

## 6. Acceptance Criteria

- AC-FILE-01 (FR-FILE-01):
  - Given アプリ起動済み
  - When Open で JSON を選択
  - Then エディタに内容が読み込まれ、妥当ならツリーが表示される
- AC-FILE-02 (FR-FILE-02):
  - Given 元ファイルより新しい .autosave が存在
  - When Open 実行時の確認で Yes を選択
  - Then バックアップ内容が読み込まれる
- AC-FILE-03 (FR-FILE-03/04):
  - Given 編集済みテキスト
  - When Save または Save As を実行
  - Then 指定先へ書き込み完了し、状態が保存済みに更新される
- AC-VAL-01 (FR-VAL-01/02):
  - Given 不正 JSON
  - When 検証が実行される
  - Then エラーメッセージと行/列情報が表示される
- AC-TREE-01 (FR-TREE-01/02):
  - Given 有効 JSON
  - When 検証成功
  - Then $ ルート配下に構造が反映される
- AC-SR-01 (FR-SR-01/02/03):
  - Given 検索条件を入力
  - When Next/Previous を実行
  - Then 条件一致箇所へ移動し、端ではラップする
- AC-SR-02 (FR-SR-04/05):
  - Given 置換条件を入力
  - When Replace All を実行
  - Then 一括置換され、件数表示が更新される
- AC-PRE-01 (FR-PRE-01/02/03):
  - Given 置換条件を入力
  - When Preview を実行
  - Then 置換候補一覧と件数要約が表示される
- AC-THEME-01 (FR-THEME-01/02):
  - Given テーマ切替操作
  - When ダーク/ライトを切替
  - Then UI 配色とエディタ強調表示が同期して更新される
- AC-SET-01 (FR-SET-01):
  - Given 設定変更後に終了
  - When 次回起動
  - Then 設定が復元される
- AC-AUTO-01 (FR-AUTO-01/02):
  - Given ファイル編集中で自動保存条件成立
  - When 周期チェック到達
  - Then .autosave が出力され、失敗時は失敗通知される

## 7. Traceability Matrix

| Requirement ID | 実装証跡 | テスト証跡 |
|---|---|---|
| FR-FILE-01 | src/JsonEditor.App/ViewModels/MainViewModel.cs (OpenFileAsync), src/JsonEditor.App/Infrastructure/FileDialogService.cs | tests/JsonEditor.App.Tests/IntegrationTests.cs (OpenValidateAndTreeBuild_WorksEndToEnd), tests/JsonEditor.App.Tests/MainViewModelTests.cs (OpenFileCommand_LoadsSelectedFile) |
| FR-FILE-02 | src/JsonEditor.App/ViewModels/MainViewModel.cs (TryRestoreBackupOnOpenAsync) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (OpenFileCommand_RestoresBackup_WhenUserChoosesYes), tests/JsonEditor.App.Tests/MainViewModelTests.cs (OpenFileCommand_Cancels_WhenUserChoosesCancelForBackupRestore) |
| FR-FILE-03 | src/JsonEditor.App/ViewModels/MainViewModel.cs (SaveFileAsync) | tests/JsonEditor.App.Tests/IntegrationTests.cs (SearchReplaceAndSave_WorksEndToEnd) |
| FR-FILE-04 | src/JsonEditor.App/ViewModels/MainViewModel.cs (SaveAsFileAsync) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (SaveAsFileCommand_SavesJsonToSelectedPath) |
| FR-FILE-05 | src/JsonEditor.App/ViewModels/MainViewModel.cs (RestoreBackupForCurrentFileAsync) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (RestoreBackupCommand_LoadsBackupForCurrentFile), tests/JsonEditor.App.Tests/IntegrationTests.cs (BackupRestoreFlow_WorksEndToEnd) |
| FR-VAL-01 | src/JsonEditor.App/ViewModels/MainViewModel.cs (ValidateAndRebuildTree), src/JsonEditor.Core/Services/JsonValidationService.cs | tests/JsonEditor.App.Tests/JsonValidationServiceTests.cs, tests/JsonEditor.App.Tests/MainViewModelTests.cs (ValidateCommand_SetsStatusValid_AndBuildsTree) |
| FR-VAL-02 | src/JsonEditor.App/ViewModels/MainViewModel.cs (validation error status build) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (ValidateCommand_SetsErrorStatus_WhenJsonIsInvalid), tests/JsonEditor.App.Tests/MainViewModelTests.cs (ValidateCommand_AppendsLineAndColumn_WhenValidationContainsLocation) |
| FR-TREE-01 | src/JsonEditor.Core/Services/JsonTreeBuilder.cs (Build) | tests/JsonEditor.App.Tests/JsonTreeBuilderTests.cs |
| FR-TREE-02 | src/JsonEditor.Core/Services/JsonTreeBuilder.cs (BuildNode, FormatLabel) | tests/JsonEditor.App.Tests/JsonTreeBuilderTests.cs, tests/JsonEditor.App.Tests/IntegrationTests.cs (OpenValidateAndTreeBuild_WorksEndToEnd) |
| FR-SR-01 | src/JsonEditor.Core/Services/SearchReplaceService.cs (BuildRegex, FindNextMatch, FindPreviousMatch) | tests/JsonEditor.App.Tests/SearchReplaceServiceTests.cs |
| FR-SR-02 | src/JsonEditor.Core/Services/SearchReplaceService.cs (StringComparison switch) | tests/JsonEditor.App.Tests/SearchReplaceServiceTests.cs (CountMatches_WorksForCaseInsensitivePlainSearch) |
| FR-SR-03 | src/JsonEditor.Core/Services/SearchReplaceService.cs (wrap logic) | tests/JsonEditor.App.Tests/SearchReplaceServiceTests.cs (FindNextMatch_WrapsToBeginning_WhenNoFurtherMatch), tests/JsonEditor.App.Tests/SearchReplaceServiceTests.cs (FindPreviousMatch_WrapsToEnd_WhenNoPreviousMatch) |
| FR-SR-04 | src/JsonEditor.Core/Services/SearchReplaceService.cs (ReplaceAll), src/JsonEditor.App/ViewModels/MainViewModel.cs (ReplaceAll command) | tests/JsonEditor.App.Tests/SearchReplaceServiceTests.cs (ReplaceAll_WorksForRegex), tests/JsonEditor.App.Tests/MainViewModelTests.cs (ReplaceAllCommand_ReplacesJsonText_AndSetsCompletedStatus) |
| FR-SR-05 | src/JsonEditor.App/ViewModels/MainViewModel.cs (UpdateMatchSummary) | tests/JsonEditor.App.Tests/SearchReplaceServiceTests.cs |
| FR-PRE-01 | src/JsonEditor.Core/Services/SearchReplaceService.cs (BuildReplacePreview), src/JsonEditor.App/ViewModels/MainViewModel.cs (BuildReplacePreview command) | tests/JsonEditor.App.Tests/SearchReplaceServiceTests.cs (BuildReplacePreview_CreatesPreviewItems_ForRegex), tests/JsonEditor.App.Tests/MainViewModelTests.cs (BuildReplacePreviewCommand_GeneratesPreviewItems) |
| FR-PRE-02 | src/JsonEditor.App/ViewModels/MainViewModel.cs (preview item formatting) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (BuildReplacePreviewCommand_GeneratesPreviewItems) |
| FR-PRE-03 | src/JsonEditor.App/ViewModels/MainViewModel.cs (Preview summary with showing 30) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (BuildReplacePreviewCommand_GeneratesPreviewItems) |
| FR-THEME-01 | src/JsonEditor.App/MainWindow.xaml.cs (ApplyTheme), src/JsonEditor.App/ViewModels/MainViewModel.cs (ToggleTheme) | tests/JsonEditor.App.Tests/UIComponentTests.cs (ApplyTheme_UsesDarkThemeDictionary, ApplyTheme_UsesLightThemeDictionary), tests/JsonEditor.App.Tests/MainViewModelTests.cs (ToggleThemeCommand_TogglesThemeFlag) |
| FR-THEME-02 | src/JsonEditor.App/MainWindow.xaml.cs (ApplyEditorHighlighting) | tests/JsonEditor.App.Tests/UIComponentTests.cs (ThemeCheckChanged_AppliesThemeFromViewModelState) |
| FR-SET-01 | src/JsonEditor.Core/Services/AppSettingsStore.cs, src/JsonEditor.App/ViewModels/MainViewModel.cs (InitializeAsync/ShutdownAsync) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (InitializeAsync_LoadsPersistedSettings, ShutdownAsync_PersistsCurrentSettings), tests/JsonEditor.App.Tests/IntegrationTests.cs (ThemeSettings_ArePersistedAcrossInitializeAndShutdown) |
| FR-AUTO-01 | src/JsonEditor.App/ViewModels/MainViewModel.cs (RunAutoSaveLoopAsync, SaveBackupAsync) | tests/JsonEditor.App.Tests/IntegrationTests.cs (BackupRestoreFlow_WorksEndToEnd) |
| FR-AUTO-02 | src/JsonEditor.App/ViewModels/MainViewModel.cs (TryAutoSaveBackupAsync autosave exception status) | tests/JsonEditor.App.Tests/MainViewModelTests.cs (RunAutoSaveOnceAsync_KeepsReplacedJsonText_WhenAutosaveFails, RunAutoSaveOnceAsync_SetsAutosavedBackupStatus_WhenAutosaveSucceeds) |
| NFR-PERF-01 | tests/JsonEditor.App.Tests/PerformanceTests.cs (JsonValidation_LargePayload_CompletesWithinBudget) | tests/JsonEditor.App.Tests/PerformanceTests.cs |
| NFR-PERF-02 | tests/JsonEditor.App.Tests/PerformanceTests.cs (SearchCount_LargePayload_CompletesWithinBudget) | tests/JsonEditor.App.Tests/PerformanceTests.cs |
| NFR-PERF-03 | tests/JsonEditor.App.Tests/PerformanceTests.cs (ReplaceAll_LargePayload_CompletesWithinBudget) | tests/JsonEditor.App.Tests/PerformanceTests.cs |
| NFR-PERF-04 | tests/JsonEditor.App.Tests/PerformanceTests.cs (RegexPreview_LargePayload_CompletesWithinBudget) | tests/JsonEditor.App.Tests/PerformanceTests.cs |
| NFR-PLAT-01 | src/JsonEditor.App/JsonEditor.App.csproj (TargetFramework net8.0-windows, WPF) | tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj (net8.0-windows) |

## 8. Evidence Scope

- 本書の要求は現行リポジトリ実装と既存テストで確認できる事実のみを対象とする。
- 実装されていない将来計画は要求本文に含めない。

## 9. Open Questions

- OQ-01: 置換後に JSON が不正化した場合、即時再検証結果の通知仕様をより明示するか。
- OQ-02: ファイル保存失敗時のユーザー向け詳細ガイダンスを標準化するか。
- OQ-03: 自動保存失敗ケースの専用テストを追加して受け入れ基準を強化するか。