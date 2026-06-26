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
- 59

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

## 6. 実行結果 (2026-06-27 最新)

### 6.1 単体・統合・UI・性能テスト実行

結果:
- 合格: 59
- 失敗: 0
- スキップ: 0

実行ログ要約:
- Test Run Successful
- JsonEditor.App.Tests.dll (net8.0-windows)

### 6.2 カバレッジ結果

結果ファイル:
- tests/JsonEditor.App.Tests/TestResults/c0b8d2be-1c53-4d65-8306-7817df261d12/coverage.cobertura.xml

集計:
- ラインカバレッジ: 70.35% (515/732)
- ブランチカバレッジ: 54.23% (128/236)

### 6.3 各テスト結果詳細

実行条件:
- 実行コマンド: dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj
- 判定: 全59件 PASS

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

MainViewModelTests (17/17 PASS):
1. Constructor_Throws_WhenValidationServiceIsNull: PASS
2. ValidateCommand_SetsStatusValid_AndBuildsTree: PASS
3. ValidateCommand_SetsErrorStatus_WhenJsonIsInvalid: PASS
4. ReplaceAllCommand_ReplacesJsonText_AndSetsCompletedStatus: PASS
5. ReplaceAllCommand_SetsErrorStatus_WhenServiceThrows: PASS
6. NextMatchCommand_UpdatesSelection_WhenMatchExists: PASS
7. PreviousMatchCommand_UpdatesSelection_WhenMatchExists: PASS
8. BuildReplacePreviewCommand_GeneratesPreviewItems: PASS
9. BuildReplacePreviewCommand_SetsErrorStatus_WhenRegexIsInvalid: PASS
10. ToggleThemeCommand_TogglesThemeFlag: PASS
11. OpenFileCommand_LoadsSelectedFile: PASS
12. OpenFileCommand_RestoresBackup_WhenUserChoosesYes: PASS
13. OpenFileCommand_Cancels_WhenUserChoosesCancelForBackupRestore: PASS
14. SaveAsFileCommand_SavesJsonToSelectedPath: PASS
15. RestoreBackupCommand_LoadsBackupForCurrentFile: PASS
16. InitializeAsync_LoadsPersistedSettings: PASS
17. ShutdownAsync_PersistsCurrentSettings: PASS

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
| テスト数 | 8 | 59 | +51 |
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

## 10. 付録: 主要成果物

- テスト設定: .runsettings
- CI: .github/workflows/ci.yml
- 段階6追加: tests/JsonEditor.App.Tests/MainViewModelTests.cs
- 段階7追加: tests/JsonEditor.App.Tests/UIComponentTests.cs
- 段階8追加: tests/JsonEditor.App.Tests/IntegrationTests.cs
- 段階9追加: tests/JsonEditor.App.Tests/PerformanceTests.cs

---

最終更新日: 2026-06-27
ドキュメント版: 3.1
