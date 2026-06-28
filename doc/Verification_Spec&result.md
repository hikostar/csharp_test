# 検証仕様兼結果報告書 - JsonEditor

作成日: 2026-06-27
対象プロジェクト: JsonEditor WPF Application
対象バージョン: .NET 8.0
ドキュメント種別: 検証仕様書 + 実行結果報告書

---

## 1. 目的

本書は JsonEditor のテスト方針、実行手順、実測結果、進捗を一体管理する。

1. どの段階で何を検証するかを明確化する
2. 実行した結果を同一ドキュメントで追跡する
3. テスト数とカバレッジ推移を継続的に記録する

---

## 2. テスト環境

| 項目 | 値 |
|:---|:---|
| テストフレームワーク | xUnit 2.5.3 |
| テスト SDK | Microsoft.NET.Test.Sdk 17.8.0 |
| モック | Moq 4.20.70 |
| カバレッジ | coverlet.collector 6.0.0 (XPlat Code Coverage) |
| ターゲット | net8.0-windows |
| 実行 OS | Windows |

カバレッジ設定ファイル:
- .runsettings

---

## 3. 段階別仕様と実施状況

### 段階1-2: Core サービス層テスト

対象:
- JsonTreeBuilder
- JsonValidationService
- SearchReplaceService

状態:
- 完了

### 段階3-5: WPF テスト基盤 + ViewModel 基礎テスト

対象:
- RelayCommand
- JsonTreeNodeViewModel

状態:
- 完了

### 段階6: MainViewModel の DI 対応 + 単体テスト

実装内容:
1. MainViewModel を依存注入対応へ変更
2. ファイルダイアログ/メッセージボックス依存を抽象化経由へ統一
3. MainViewModelTests を追加

状態:
- 完了

### 段階7: UI コンポーネントテスト

実装内容:
1. UIComponentTests を追加
2. MainWindow のテーマ適用、エディタ同期、選択反映、テーマイベントを検証
3. テーマ辞書参照を pack URI 化し、実行ホスト差異での解決失敗を解消

状態:
- 完了

### 段階8: 統合テスト

実装内容:
1. ファイル読込 -> 検証 -> ツリー表示
2. 検索/置換 -> 保存
3. バックアップ復元フロー
4. 設定の保存/復元フロー
5. 不正 JSON のエラー挙動

状態:
- 完了

### 段階9: パフォーマンステスト

実装内容:
1. 大規模 JSON 検証
2. 大規模検索カウント
3. 大規模一括置換
4. 正規表現プレビュー

状態:
- 完了

### 段階10: CI/CD 統合

実装内容:
1. GitHub Actions ワークフロー追加
2. Build/Test/Coverage 実行
3. テスト結果アーティファクトアップロード

状態:
- 完了

### 段階11: 品質運用フェーズ

実装方針:
1. カバレッジ下限閾値の導入
2. 回帰失敗時の自動通知
3. パフォーマンス閾値の継続監視

状態:
- 計画中

### 段階12: WinUI3 スパイク検証

実装内容:
1. `feature/winui3-spike` ブランチで WinUI3 プロジェクトを追加
2. Copilot WinUI plugin / WinApp CLI / WinUI templates を導入
3. `MainPage` に Open / Save / Validate / StatusMessage の最小機能を実装
4. 依存方向 `JsonEditor.WinUI3 -> JsonEditor.Core` を維持

要件ID:
- WINUI-SPIKE-FR-01: WinUI3 プロジェクトがソリューションに追加されビルド可能
- WINUI-SPIKE-FR-02: WinUI3 から Core 参照を維持し逆依存なし
- WINUI-SPIKE-FR-03: Open でファイル内容を読み込み可能
- WINUI-SPIKE-FR-04: Save でテキスト保存可能
- WINUI-SPIKE-FR-05: Validate で Core 検証結果を通知可能
- WINUI-SPIKE-NFR-01: 例外時にクラッシュせず失敗メッセージ通知

状態:
- 実施中（自動検証は一部完了、手動UI検証は継続）

---

## 4. 現在のテスト資産

テストプロジェクト:
- tests/JsonEditor.App.Tests

テストクラス:
1. JsonTreeBuilderTests
2. JsonValidationServiceTests
3. SearchReplaceServiceTests
4. RelayCommandTests
5. JsonTreeNodeViewModelTests
6. MainViewModelTests
7. UIComponentTests
8. IntegrationTests
9. PerformanceTests

総テスト数:
- 62

---

## 5. 実行コマンド

### 通常実行

```powershell
dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj
```

### カバレッジ付き実行

```powershell
dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --settings .runsettings --collect:"XPlat Code Coverage"
```

### フィルタ実行例

```powershell
dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --filter "ClassName=UIComponentTests"
dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --filter "Category=Performance"
```

---

## 6. 実行結果 (2026-06-28 最新)

### 6.0 WinUI3 スパイク実行結果（2026-06-28）

実行コマンド:
- `copilot plugin list`
- `winapp --help`
- `dotnet new list winui`
- `dotnet build src/JsonEditor.WinUI3/JsonEditor.WinUI3.csproj -v minimal`
- `dotnet build JsonEditor.sln -v minimal`
- `dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --filter "FullyQualifiedName~JsonValidationServiceTests" -v minimal`

結果:
- Plugin: PASS (`winui@awesome-copilot (v0.4.0)`)
- WinApp CLI: PASS (`Windows App Development CLI 0.4.0`)
- WinUI Templates: PASS（`winui, winui3, wasdk-single` を含むテンプレート一覧表示）
- WinUI3 Build: PASS（0 warning / 0 error）
- Solution Build: PASS（0 warning / 0 error）
- Core JSON Validation Tests: PASS（2 passed, 0 failed）

未完了項目（手動）:
- M-01 Open 正常系
- M-02 Open キャンセル
- M-03 Save 正常系
- M-04 Save キャンセル
- M-05 Validate 正常 JSON
- M-06 Validate 不正 JSON
- M-07 例外系（非クラッシュ）

### 6.1 単体・統合・UI・性能テスト実行

結果:
- 合格: 62
- 失敗: 0
- スキップ: 0

実行ログ要約:
- Test Run Successful
- JsonEditor.App.Tests.dll (net8.0-windows)

### 6.2 カバレッジ結果

実行日時:
- 2026-06-28 17:28:57 +09:00

結果ファイル:
- tests/JsonEditor.App.Tests/TestResults/c0b8d2be-1c53-4d65-8306-7817df261d12/coverage.cobertura.xml

集計:
- ラインカバレッジ: 70.35% (515/732)
- ブランチカバレッジ: 54.23% (128/236)

### 6.3 各テスト結果詳細

実行条件:
- 実行コマンド: dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj
- 判定: 全62件 PASS

JsonTreeBuilderTests (1/1 PASS):
1. Build_ReturnsRootNode_ForValidJson: PASS

JsonValidationServiceTests (2/2 PASS):
1. Validate_ReturnsValid_ForCorrectJson: PASS
2. Validate_ReturnsInvalid_ForBrokenJson: PASS

SearchReplaceServiceTests (5/5 PASS):
1. CountMatches_WorksForCaseInsensitivePlainSearch: PASS
2. ReplaceAll_WorksForRegex: PASS
3. FindNextMatch_WrapsToBeginning_WhenNoFurtherMatch: PASS
4. FindPreviousMatch_WrapsToEnd_WhenNoPreviousMatch: PASS
5. BuildReplacePreview_CreatesPreviewItems_ForRegex: PASS

RelayCommandTests (8/8 PASS):
1. Execute_CallsAction: PASS
2. CanExecute_DefaultReturnsTrue: PASS
3. CanExecute_WithCondition_ReturnsTrueWhenConditionMet: PASS
4. CanExecute_WithCondition_ReturnsFalseWhenConditionNotMet: PASS
5. RaiseCanExecuteChanged_FiresCanExecuteChangedEvent: PASS
6. CanExecuteChanged_FiredWhenConditionChanges: PASS
7. Execute_WithParameter_IgnoresParameter: PASS
8. MultipleExecute_CallsActionEachTime: PASS

JsonTreeNodeViewModelTests (11/11 PASS):
1. Constructor_SetsLabel: PASS
2. Children_InitializesEmptyCollection: PASS
3. Children_CanAddItems: PASS
4. Children_CanAddMultipleItems: PASS
5. Children_IsObservableCollection: PASS
6. Children_CanRemoveItems: PASS
7. Label_IsInitOnly: PASS
8. NestedHierarchy_BuildsCorrectly: PASS
9. Children_CanClear: PASS
10. EmptyLabel_IsAccepted: PASS
11. MultipleInstances_AreIndependent: PASS

MainViewModelTests (20/20 PASS):
1. Constructor_Throws_WhenValidationServiceIsNull: PASS
2. ValidateCommand_SetsStatusValid_AndBuildsTree: PASS
3. ValidateCommand_SetsErrorStatus_WhenJsonIsInvalid: PASS
4. ValidateCommand_AppendsLineAndColumn_WhenValidationContainsLocation: PASS
5. ReplaceAllCommand_ReplacesJsonText_AndSetsCompletedStatus: PASS
6. ReplaceAllCommand_SetsErrorStatus_WhenServiceThrows: PASS
7. NextMatchCommand_UpdatesSelection_WhenMatchExists: PASS
8. PreviousMatchCommand_UpdatesSelection_WhenMatchExists: PASS
9. BuildReplacePreviewCommand_GeneratesPreviewItems: PASS
10. BuildReplacePreviewCommand_SetsErrorStatus_WhenRegexIsInvalid: PASS
11. ToggleThemeCommand_TogglesThemeFlag: PASS
12. OpenFileCommand_LoadsSelectedFile: PASS
13. OpenFileCommand_RestoresBackup_WhenUserChoosesYes: PASS
14. OpenFileCommand_Cancels_WhenUserChoosesCancelForBackupRestore: PASS
15. SaveAsFileCommand_SavesJsonToSelectedPath: PASS
16. RestoreBackupCommand_LoadsBackupForCurrentFile: PASS
17. InitializeAsync_LoadsPersistedSettings: PASS
18. ShutdownAsync_PersistsCurrentSettings: PASS
19. RunAutoSaveOnceAsync_KeepsReplacedJsonText_WhenAutosaveFails: PASS
20. RunAutoSaveOnceAsync_SetsAutosavedBackupStatus_WhenAutosaveSucceeds: PASS

### 6.4 最小品質ゲート実行結果 (2026-06-28)

実行コマンド:
- dotnet --info
- dotnet build JsonEditor.sln
- dotnet test JsonEditor.sln
- dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
- powershell -ExecutionPolicy Bypass -File scripts/validate-pr-guard-local.ps1

結果:
- 環境確認: .NET SDK 8.0.422
- Build: PASS (0 warning / 0 error)
- Test: PASS (62 passed, 0 failed)
- Coverage: PASS (70.35% line, 54.23% branch)
- PR Guard: PASS (ExitCode=0)
	- 補足: スクリプト標準出力にサンプルの `FAILURE` 行が表示されるが、実行終了コードは 0

IntegrationTests (5/5 PASS):
1. OpenValidateAndTreeBuild_WorksEndToEnd: PASS
2. SearchReplaceAndSave_WorksEndToEnd: PASS
3. BackupRestoreFlow_WorksEndToEnd: PASS
4. ThemeSettings_ArePersistedAcrossInitializeAndShutdown: PASS
5. InvalidJson_ShowsValidationError: PASS

PerformanceTests (4/4 PASS):
1. JsonValidation_LargePayload_CompletesWithinBudget: PASS
2. SearchCount_LargePayload_CompletesWithinBudget: PASS
3. ReplaceAll_LargePayload_CompletesWithinBudget: PASS
4. RegexPreview_LargePayload_CompletesWithinBudget: PASS

UIComponentTests (6/6 PASS):
1. ApplyTheme_UsesDarkThemeDictionary: PASS
2. ApplyTheme_UsesLightThemeDictionary: PASS
3. EditorTextChanged_UpdatesViewModelJsonText: PASS
4. RefreshEditorFromViewModel_ReflectsViewModelText: PASS
5. ApplySelectionFromViewModel_SelectsMatchedRange: PASS
6. ThemeCheckChanged_AppliesThemeFromViewModelState: PASS

---

## 7. 進捗サマリ

| 指標 | 初期 | 現在 | 変化 |
|:---|:---:|:---:|:---:|
| テスト数 | 8 | 62 | +54 |
| テストクラス数 | 3 | 9 | +6 |
| ラインカバレッジ | 53.75% (Core中心) | 70.35% | 改善 |
| ブランチカバレッジ | 39.09% (Core中心) | 54.23% | 改善 |

段階進捗:
- 段階1-10: 完了
- 段階11: 計画中

---

## 8. 既知の制約

1. UI レイヤー全体のE2E自動化は未導入
2. カバレッジ閾値でCI失敗させる品質ゲートは未設定
3. パフォーマンス閾値違反の自動検知は未導入

---

## 9. 次アクション (段階11)

1. CIにカバレッジ下限チェックを追加
2. 主要パフォーマンステストにしきい値監視と履歴比較を追加
3. テスト失敗/品質低下のレポートを運用手順化

---

## 10. AI支援変更時の最小検証セット

本章は Copilot/Agent/Skill を用いた変更に対する最低限の受入検証を定義する。

### 10.1 実行対象

対象変更:
- `src/JsonEditor.Core` のロジック変更
- `src/JsonEditor.App` の ViewModel/画面連携変更
- 検索置換、検証、保存復元、自動保存、テーマ関連の挙動変更

除外条件:
- 文言のみの修正
- コメントのみの修正

### 10.2 必須コマンド

1. ビルド

```powershell
dotnet build JsonEditor.sln
```

2. 通常テスト

```powershell
dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj
```

3. カバレッジ付きテスト

```powershell
dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --settings .runsettings --collect:"XPlat Code Coverage"
```

### 10.4 開発支援時の確認

1. 変更案が `JsonEditor.Core` と `JsonEditor.App` の責務境界を壊していないこと
2. 必要なテスト観点が先に列挙されていること
3. 仕様/検証/運用ドキュメントの更新要否が明示されていること

### 10.3 回帰観点チェック

1. 妥当 JSON 入力時に `Valid JSON` 表示とツリー再構築が行われる
2. 不正 JSON 入力時にエラー表示し、クラッシュしない
3. 検索/置換（通常・Regex、大小文字）で件数と選択位置が整合する
4. 保存/別名保存/バックアップ復元の主要フローが成立する
5. テーマ切替と設定復元（起動/終了）が成立する

### 10.4 判定基準

1. テスト失敗 0 件
2. 既存テスト総数を減らさない
3. カバレッジが直近基準値（ライン 70.35%、ブランチ 54.23%）を大きく下回らない

注記:
- 一時的に下回る場合は、理由とフォローアップ計画を PR 説明に明記する

### 10.5 PR 記載テンプレート（検証欄）

参照テンプレート:
- `.github/pull_request_template.md`
- `doc/AI_PR_Validation_Examples.md`

運用ルール:
- AI支援PRでは PR テンプレートの「AI変更 検証結果（AI支援PRのみ必須）」欄と同一項目を記載する
- 非AI支援PRでは任意とする

```text
[AI変更 検証結果]
- 対象要件ID: FR-xxx, NFR-xxx
- 追加/更新テスト: TestClass.TestName, ...
- build: pass/fail
- test: pass/fail (passed/failed/skipped)
- 実行日時: YYYY-MM-DD HH:mm:ss +09:00
- coverage: line xx.xx%, branch xx.xx%
- coverage artifact: tests/JsonEditor.App.Tests/TestResults/<id>/coverage.cobertura.xml
- 回帰観点: OK/NG (要点)
- 仕様/検証ドキュメント更新: 有/無
```

---

## 11. 付録: 主要成果物

- テスト設定: .runsettings
- CI: .github/workflows/ci.yml
- 段階6追加: tests/JsonEditor.App.Tests/MainViewModelTests.cs
- 段階7追加: tests/JsonEditor.App.Tests/UIComponentTests.cs
- 段階8追加: tests/JsonEditor.App.Tests/IntegrationTests.cs
- 段階9追加: tests/JsonEditor.App.Tests/PerformanceTests.cs

---

## 12. 再検証結果 (2026-06-28)

### 12.1 実行コマンド

1. `dotnet --info`
2. `dotnet build JsonEditor.sln`
3. `dotnet test JsonEditor.sln`
4. `dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura`
5. `powershell -ExecutionPolicy Bypass -File scripts/validate-pr-guard-local.ps1`

### 12.2 実行結果

実行日時:
- 2026-06-28 17:28:57 +09:00

- 環境確認: .NET SDK 8.0.422
- Build: PASS (0 warning / 0 error)
- Test: PASS (62 passed, 0 failed, 0 skipped)
- Coverage: PASS (Line 70.35%, Branch 54.23%)
- Coverage Artifact: tests/JsonEditor.App.Tests/TestResults/c0b8d2be-1c53-4d65-8306-7817df261d12/coverage.cobertura.xml
- PR Guard: PASS (ExitCode=0)
	- 補足: スクリプト標準出力にサンプルの `FAILURE` 行が表示されるが、終了コードは 0

### 12.3 判定メモ

- コードとテストの品質ゲートは通過
- 5つの品質ゲートコマンドは全て通過
- スクリプト出力の補助メッセージは終了コードで判定する

---

最終更新日: 2026-06-28
ドキュメント版: 3.3

---

## 13. Requirement ID トレーサビリティテンプレート

本章は `Verification & Test Design Agent` が検証仕様更新時に使用する標準テンプレートを定義する。

### 13.1 記載ルール

1. すべての追加/更新テストは Requirement ID に紐づける。
2. Requirement ID ごとに「対象テスト」「実行結果」「証跡リンク」を記載する。
3. 未実装または未実行の項目は `Pending` とし、理由と次アクションを明記する。

### 13.2 テンプレート

```markdown
| Requirement ID | テストクラス/テスト名 | 実行コマンド | 結果 | 証跡 |
|---|---|---|---|---|
| FR-AUTO-02 | MainViewModelTests.RunAutoSaveOnceAsync_KeepsReplacedJsonText_WhenAutosaveFails | dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --filter "RunAutoSaveOnceAsync_KeepsReplacedJsonText_WhenAutosaveFails" | PASS | tests/JsonEditor.App.Tests/MainViewModelTests.cs |
| FR-XXX | ExampleTests.ExampleBehavior_Works | dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --filter "ExampleBehavior_Works" | PASS | tests/JsonEditor.App.Tests/ExampleTests.cs |
| NFR-XXX | PerformanceTests.ExampleBudget_CompletesWithinBudget | dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --filter "ExampleBudget_CompletesWithinBudget" | PASS | tests/JsonEditor.App.Tests/PerformanceTests.cs |
```

### 13.3 更新チェックリスト

1. PR の AI変更 検証結果欄に対象要件IDを記載したか。
2. 追加/更新テスト名が PR 記載と本章テンプレートで一致しているか。
3. Quality Gate 実行時に本章の対象カテゴリ（単体/統合/UI/性能）がカバーされたか。

### 13.4 FR-AUTO-02 実績トレーサビリティ (2026-06-28)

| Requirement ID | テストクラス/テスト名 | 実行コマンド | 結果 | 証跡 |
|---|---|---|---|---|
| FR-AUTO-02 | MainViewModelTests.RunAutoSaveOnceAsync_KeepsReplacedJsonText_WhenAutosaveFails | dotnet test JsonEditor.sln | PASS | tests/JsonEditor.App.Tests/MainViewModelTests.cs |
| FR-AUTO-02 | MainViewModelTests.RunAutoSaveOnceAsync_SetsAutosavedBackupStatus_WhenAutosaveSucceeds | dotnet test JsonEditor.sln | PASS | tests/JsonEditor.App.Tests/MainViewModelTests.cs |
