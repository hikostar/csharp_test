---
name: code-analyzer
description: "C# JsonEditor プロジェクトのコード品質を多角的に分析・改善するスキル。
カバレッジが低い関数の特定、サイクロマティック複雑度の高いメソッドの簡素化提案、
過度な依存関係の検出、C# 命名規則の準拠状況確認、null 安全性の脆弱性指摘、
パフォーマンス最適化候補を自動抽出。テストプロジェクトのテスト結果ファイル、
ソースコード、カバレッジレポートを多次元分析してレポート・改善提案を生成。"
---

# code-analyzer — C# コード品質分析スキル

## 概要

このスキルは、JsonEditor C# プロジェクトのコード品質を **複数の角度から自動分析** し、改善提案を生成します。

### 解決する課題

```
❌ 手動でテストカバレッジを確認するのは時間がかかる
❌ 複雑度の高いメソッドを見落としてしまう
❌ 依存関係が複雑になっていることに気づかない
❌ 命名規則の準拠状況が不明確
❌ null 参照エラーのリスクを定量化できない
```

### 実現する機能

```
✅ 低カバレッジ関数を自動特定
✅ 複雑度分析と簡素化提案
✅ 依存関係の過度性を指摘
✅ 命名規則の準拠状況を判定
✅ Null 安全性脆弱性を指摘
✅ パフォーマンス最適化候補を抽出
✅ Markdown / JSON 複数形式でレポート生成
```

## コア分析機能（6つの柱）

### 1. カバレッジ分析（Coverage Analysis）

**入力**: Cobertura カバレッジレポート + テスト結果

**分析内容**:
- 全体カバレッジ率（Line/Branch）
- 関数別カバレッジ（どの関数が未テスト？）
- カバレッジ低下箇所の抽出
- テスト必要度の優先順位付け

**出力例**:
```markdown
## カバレッジ分析結果

### 全体統計
- ラインカバレッジ: 69.67% (510/732)
- ブランチカバレッジ: 51.27% (121/236)

### 未テスト関数（優先順位順）
1. **MainViewModel.LoadJsonFile()** - 0% (5 lines)
   理由: UI 依存、ダイアログ操作が必要
   提案: IFileDialogService をモック化して テスト化
   
2. **JsonTreeBuilder.NormalizeKey()** - 15% (3/20 branches)
   理由: エッジケース（special characters）のテスト不足
   提案: Unicode, emoji, null 文字列のテストケース追加
```

### 2. 複雑度分析（Complexity Analysis）

**入力**: C# ソースコード

**分析内容**:
- サイクロマティック複雑度（Cyclomatic Complexity）
- ネストの深さ（Max Nesting Level）
- 関数サイズ（LOC）
- 認知複雑度（Cognitive Complexity）

**複雑度レベル判定**:
```
🟢 低 (CC < 10)     - 問題なし、テスト容易
🟡 中 (10 ≤ CC < 20) - 注意必要、テスト拡充推奨
🔴 高 (CC ≥ 20)     - リスク大、リファクタ推奨
```

**出力例**:
```markdown
## 複雑度分析結果

### 高複雑度メソッド（リファクタ推奨）
1. **SearchReplaceService.ExecuteReplace()** - CC: 24 🔴
   - ネストの深さ: 5 段階
   - LOC: 87 行
   - 提案:
     * 検索ロジックを Extract Method
     * 置換ロジックを別メソッドに分離
     * 結果判定ロジックを Predicate に変更
   - 簡素化後の予想 CC: 12

2. **JsonValidationService.ValidateJson()** - CC: 18 🟡
   - ネストの深さ: 4 段階
   - LOC: 52 行
   - 提案: null チェック部分をガード句に
```

### 3. 依存関係分析（Dependency Analysis）

**入力**: ソースコードの using/new ステートメント

**分析内容**:
- クラス間の依存関係グラフ
- 循環依存の検出
- 依存度の高いクラス特定
- 層間違い（e.g., UI → Model に直結）の検出

**出力例**:
```markdown
## 依存関係分析結果

### 循環依存の検出
⚠️ 循環依存なし（OK）

### 依存度の高いクラス
1. **JsonTreeNode** - 被依存数: 12
   → Core モデルとして適切

2. **MainViewModel** - 依存数: 11
   → サービス依存が多い（改善余地あり）
   
3. **IJsonValidationService** - 被依存数: 8
   → 重要な抽象化（安定性重視）

### 層構造の違反
✓ UI (App) → Core (Services) のみ（OK）
✗ 逆方向の依存なし（OK）
```

### 4. 命名規則チェック（Naming Convention)

**入力**: ソースコード（クラス、メソッド、プロパティ）

**チェック項目**:
```
[C# Standard Naming Guidelines]
✓ クラス/インターフェース: PascalCase (JsonTreeBuilder, IFileDialogService)
✓ メソッド: PascalCase (GetJsonTree, ExecuteReplace)
✓ プロパティ: PascalCase (IsValid, SelectedNode)
✓ 定数: UPPER_SNAKE_CASE または PascalCase (MAX_DEPTH)
✓ ローカル変数/パラメータ: camelCase (jsonPath, treeNode)
✓ インターフェース: I + PascalCase (IJsonTreeBuilder)
✓ 非公開メンバー: _camelCase (_cache, _logger)
```

**出力例**:
```markdown
## 命名規則チェック結果

### 違反箇所（3件）
1. **RelayCommand.canExecute** (private field)
   問題: underscore 欠落 (_canExecute に変更)
   ファイル: src/JsonEditor.App/Infrastructure/RelayCommand.cs:45

2. **MainViewModel.jsonPath** (property)
   問題: camelCase (JsonPath に変更)
   ファイル: src/JsonEditor.App/ViewModels/MainViewModel.cs:78

3. **SearchReplaceService.replace_count** (variable)
   問題: snake_case (replaceCount に変更)
   ファイル: src/JsonEditor.Core/Services/SearchReplaceService.cs:112

### 準拠率
✅ 97.3% (143/147 メンバー)
```

### 5. Null 安全性分析（Null Safety Analysis）

**入力**: ソースコード + C# nullability context

**分析内容**:
- null 許容型の宣言状況（`string?` vs `string`）
- null チェック漏れの可能性
- Null Coalescing の使用状況
- Nullable Context の設定確認

**出力例**:
```markdown
## Null 安全性分析結果

### Nullability Context
✓ 有効 (Project ファイルに <Nullable>enable</Nullable>)

### 脆弱性 (5件)
1. **JsonTreeNode.Key** - null 非許容だが値 null で初期化
   ファイル: src/JsonEditor.Core/Models/JsonTreeNode.cs:15
   修正: `public string Key { get; set; } = "";` に変更

2. **MainViewModel.SelectedNode** - null チェック漏れ
   ファイル: src/JsonEditor.App/ViewModels/MainViewModel.cs:234
   修正: `var node = SelectedNode ?? throw new InvalidOperationException(...);`

### Null 安全性スコア
⚠️ 78/100 (改善余地あり)
```

### 6. パフォーマンス分析（Performance Analysis）

**入力**: ソースコード + 実行時テスト結果

**分析内容**:
- 遅いメソッド検出
- メモリ効率が悪いアルゴリズム特定
- LINQ の非効率な使用
- 不要な文字列連結
- コレクションのN+1 問題

**出力例**:
```markdown
## パフォーマンス分析結果

### ボトルネックメソッド（実行時テストから）
1. **JsonTreeBuilder.BuildTree()** - 45ms (平均)
   入力: large.json (10MB)
   提案: Dictionary → HashSet に変更で 20% 高速化見込み

2. **SearchReplaceService.ReplaceAll()** - 32ms
   提案: 正規表現コンパイルをキャッシュ

### メモリ効率改善
- ObservableCollection の度重なる Add → AddRange に変更
- 文字列連結ループ → StringBuilder に変更
```

## 使用方法

### トリガーシーン

```
✅ "コード品質を分析して"
✅ "テストカバレッジが低い箇所を教えて"
✅ "複雑度の高いメソッドをリファクタしたい"
✅ "依存関係が複雑になっていないか確認して"
✅ "命名規則の準拠状況は？"
✅ "Null 参照エラーのリスク箇所は？"
✅ "パフォーマンス最適化できる箇所は？"
```

### 入力データ

```
1. ソースコード
   - src/JsonEditor.App/**/*.cs
   - src/JsonEditor.Core/**/*.cs

2. テスト結果
   - tests/JsonEditor.App.Tests/TestResults/*.xml

3. カバレッジレポート
   - tests/JsonEditor.App.Tests/TestResults/coverage.cobertura.xml

4. パフォーマンステスト結果（オプション）
   - tests/JsonEditor.App.Tests/TestResults/perf-results.json
```

### 出力フォーマット

**Markdown**（デフォルト）
```bash
claude> "コード品質を分析して"
# コード品質分析レポート
## 1. カバレッジ分析...
## 2. 複雑度分析...
...
```

**JSON**（プログラマティック利用）
```bash
claude> "JSON 形式でコード品質を分析して"
{
  "coverage": {...},
  "complexity": {...},
  "dependencies": {...},
  ...
}
```

## 実装参考資料

### ファイル解析パターン

[references/csharp-analysis-patterns.md](references/csharp-analysis-patterns.md) 参照

- Roslyn AST を使用したコード解析
- Cobertura XML パース
- 複雑度メトリクス計算アルゴリズム
- 依存関係グラフ構築方法

### テストケース

[evals/evals.json](evals/evals.json) 参照

```json
{
  "id": 1,
  "prompt": "tests/JsonEditor.App.Tests のテスト結果から、カバレッジが低い関数を特定して改善提案をして",
  "expected_output": "カバレッジ率、未テスト関数リスト、優先度付け、テスト方法提案を含むレポート"
}
```

### トリガー判定テスト

[evals/trigger-eval.json](evals/trigger-eval.json) 参照

```json
{
  "id": 1,
  "prompt": "カバレッジが69%で前月比5%低下。原因を分析して",
  "should_trigger": true
}
```

## ベストプラクティス

### DO ✅

```
✅ 複数の分析を組み合わせてホリスティックなレポートを生成
✅ 改善提案は「なぜ？」と「どうするか？」を明確に
✅ サンプルコードを示す（修正前→修正後）
✅ 優先順位を付ける（インパクト大→小）
✅ 定量的な指標を含める（現在値 → 改善後予想値）
```

### DON'T ❌

```
❌ 「複雑」という定性的な指摘だけ（CC:24 など数値を）
❌ リファクタを強制 （「すべきだ」→「検討してください」）
❌ 1 つの分析に偏る（カバレッジだけ、複雑度だけ）
❌ 大規模な変更を一気に提案（段階的に）
```

## プロジェクト固有の設定

### テストデータの場所

```
tests/JsonEditor.App.Tests/
├── TestResults/
│   ├── JsonEditor.App.Tests_2026-06-27.xml  (テスト結果)
│   └── coverage.cobertura.xml               (カバレッジ)
└── [テストコード]
```

### 対象コード

- **Core**: `src/JsonEditor.Core/` (命名規則, 複雑度, 依存関係)
- **App**: `src/JsonEditor.App/` (WPF UI, ViewModel)
- **Infrastructure**: `src/JsonEditor.App/Infrastructure/` (共通ユーティリティ)

### 環境

- **言語**: C# (.NET 8)
- **フレームワーク**: xUnit (テスト)
- **カバレッジ**: Cobertura (coverlet.collector)
- **解析**: Roslyn (オプション)

## 参考資料

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Microsoft .NET Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Cyclomatic Complexity](https://en.wikipedia.org/wiki/Cyclomatic_complexity)

---

**作成日**: 2026-06-27  
**版**: 1.0  
**対象プロジェクト**: JsonEditor (C# .NET 8 WPF)  
**ベース**: skill-creator メタスキル
