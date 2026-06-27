# code-analyzer スキル — セットアップ & 使用ガイド

## 📚 概要

**code-analyzer** は、C# JsonEditor プロジェクトの **コード品質を 6 つの角度から多面的に分析** し、改善提案を自動生成するスキルです。

### 6つの分析柱

```
1️⃣ カバレッジ分析      → 未テスト関数を特定・優先度付け
2️⃣ 複雑度分析        → CC が高いメソッド → リファクタ提案
3️⃣ 依存関係分析      → 循環依存・層違反の検出
4️⃣ 命名規則チェック   → C# ガイドライン準拠確認
5️⃣ Null 安全性分析   → Null 参照エラーのリスク箇所特定
6️⃣ パフォーマンス分析 → ボトルネック検出 → 最適化提案
```

## 🚀 クイックスタート（3分）

### ステップ1: スキルを起動

Claude Code で以下のいずれかを実行：

```
/code-analyzer

または

"コード品質を分析して"
```

### ステップ2: 分析対象を指定

```
Claude> カバレッジが低い箇所をテストして

Claude Code が自動で以下を実行:
  1. tests/JsonEditor.App.Tests/TestResults/*.xml を解析
  2. カバレッジが低い関数を特定
  3. テスト追加方法を提案
  4. Markdown レポート生成
```

### ステップ3: 結果を確認

```markdown
# カバレッジ分析レポート

## 全体統計
- ラインカバレッジ: 69.67%
- ブランチカバレッジ: 51.27%

## 未テスト関数（優先度順）
1. **MainViewModel.LoadJsonFile()** - 0%
   理由: UI 依存、IFileDialogService のモック化が必要
   テスト方法: [具体的な手順]
   
2. **JsonTreeBuilder.NormalizeKey()** - 15%
   理由: エッジケースのテスト不足
   提案: Unicode, emoji のテストケース追加
```

## 📊 各分析の詳細

### 1. カバレッジ分析

**トリガー例**:
```
"テストカバレッジが69%で、前月比5%低下。原因を教えて"
"カバレッジレポートから未テスト関数を優先順位付きで教えて"
```

**出力例**:
```markdown
### 優先度トップ 3
1. MainViewModel（0%）
   → テスト困難な理由: UI/ダイアログ依存
   → 解決策: 依存注入化 + IFileDialogService モック

2. SearchReplaceService.ReplaceAll()（25%）
   → 未テストケース: 正規表現の複雑パターン
   → 追加提案: Unicode、制御文字、改行パターン

3. JsonTreeNode.Clone()（40%）
   → 簡単にテスト化可能
   → 見積もり: 1-2 時間
```

**活用シーン**:
- ✅ PR マージ前のカバレッジ確認
- ✅ スプリント終了時の品質レビュー
- ✅ デバッグ時の原因特定

---

### 2. 複雑度分析

**トリガー例**:
```
"複雑度が高いメソッドをリファクタしたい"
"SearchReplaceService のコードを簡素化する方法を教えて"
```

**出力例**:
```markdown
### 高複雑度メソッド（リファクタ推奨）

#### 1. SearchReplaceService.ExecuteReplace() - CC: 24 🔴

**原因**:
- if ネストが 5 層
- switch ケース × 複数条件組み合わせ

**改善提案**:
```csharp
// Before (CC: 24)
public string ExecuteReplace(...)
{
  if (input == null) return "";
  if (string.IsNullOrEmpty(pattern)) return input;
  if (useRegex) {
    if (regexCache.Contains(pattern)) { ... }
    else { ... }
  } else {
    switch (mode) { ... }
  }
}

// After (CC: 8)
private string GetReplacementStrategy(...)
{
  if (useRegex) return RegexReplace(...);
  else return StringReplace(...);
}
```

**効果**: CC 24 → 8、テストケース削減 60%
```

**活用シーン**:
- ✅ デバッグ時の問題箇所特定
- ✅ コードレビュー時の改善提案
- ✅ リファクタ計画立案

---

### 3. 依存関係分析

**トリガー例**:
```
"依存関係が複雑になっていないか確認して"
"MainViewModel の依存が多すぎないか分析して"
```

**出力例**:
```markdown
### 依存関係分析結果

#### 層構造 ✓ OK
UI (App) → Core (Services) → Models

#### MainViewModel の依存度
- 被依存数: 0 （ViewModel は参照されない）
- 依存数: 11 （多い）
  * IJsonTreeBuilder
  * IJsonValidationService
  * ISearchReplaceService
  * ... (計 11 個)

**改善提案**: 
ファサード パターン で統合

public interface IJsonEditorService
{
  Task<JsonTree> LoadAndBuild(string path);
  ValidationResult Validate(string json);
  // ... 他の操作をラップ
}

効果: 依存数 11 → 1, テスト容易性大幅向上
```

**活用シーン**:
- ✅ 新機能開発時のアーキテクチャ検討
- ✅ リファクタの影響範囲確認
- ✅ モジュール分割の判断

---

### 4. 命名規則チェック

**トリガー例**:
```
"命名規則の違反をチェックして"
"C# コーディング規約に準拠しているか確認して"
```

**出力例**:
```markdown
### 命名規則チェック結果

#### 違反（3 件）

1. **RelayCommand.cs**: Line 45
   ```csharp
   private bool canExecute;  ❌
   // 修正: private bool _canExecute;
   ```

2. **MainViewModel.cs**: Line 78
   ```csharp
   public string jsonPath { get; set; }  ❌
   // 修正: public string JsonPath { get; set; }
   ```

3. **SearchReplaceService.cs**: Line 112
   ```csharp
   int replace_count = 0;  ❌
   // 修正: int replaceCount = 0;
   ```

#### 準拠率: 97.3% (143/147)
```

**活用シーン**:
- ✅ PR レビュー時の自動チェック
- ✅ 新規開発者のオンボーディング
- ✅ コーディング規約の学習

---

### 5. Null 安全性分析

**トリガー例**:
```
"Null 参照エラーのリスク箇所を教えて"
"Null 安全性を分析して改善提案をして"
```

**出力例**:
```markdown
### Null 安全性分析

#### Nullability Context: ✓ 有効

#### 脆弱性（5 件）

1. **JsonTreeNode.cs**: Line 15
   ```csharp
   public string Key { get; set; }  // ❌ null 非許容なのに...
   public string Key { get; set; } = null;  // ← null で初期化
   ```
   修正: `public string Key { get; set; } = "";`

2. **MainViewModel.cs**: Line 234
   ```csharp
   SelectedNode.Delete();  // ❌ null チェック漏れ
   ```
   修正: `SelectedNode?.Delete();`

#### Null 安全性スコア: 78/100
```

**活用シーン**:
- ✅ 本番バグ予防
- ✅ コードレビュー時の指摘
- ✅ CI パイプラインの自動チェック

---

### 6. パフォーマンス分析

**トリガー例**:
```
"PerformanceTests から遅いメソッドを分析して"
"パフォーマンス最適化できる箇所は？"
```

**出力例**:
```markdown
### パフォーマンス分析

#### ボトルネックメソッド（実行時テスト結果）

1. **JsonTreeBuilder.BuildTree()** - 45ms 🔴
   入力: large.json (10MB, 100K items)
   
   原因: O(n²) 検索
   ```csharp
   // ❌ 現在: O(n²)
   foreach (var item in items)
   {
     if (items.FirstOrDefault(x => x.Id == item.ParentId) != null)
       // ...
   }
   
   // ✅ 改善: O(n)
   var parentMap = items.ToDictionary(x => x.Id);
   foreach (var item in items)
   {
     if (parentMap.ContainsKey(item.ParentId))
       // ...
   }
   ```
   期待改善率: **65% 短縮** (45ms → 16ms)

2. **SearchReplaceService.ReplaceAll()** - 32ms
   原因: 正規表現をコンパイルし直している
   改善: Compiled Regex をキャッシュ
   期待改善率: **45% 短縮** (32ms → 18ms)
```

**活用シーン**:
- ✅ 大規模データ処理の最適化
- ✅ パフォーマンス劣化の原因分析
- ✅ スケーラビリティの改善

---

## 💡 複合分析：全角度からのコード品質評価

**トリガー例**:
```
"コード品質を総合的に分析して、改善優先順位と3ヶ月計画を提案して"
```

**出力例**:
```markdown
# 総合コード品質分析レポート

## スコアカード

| 項目 | 現状 | 目標 | 優先度 |
|------|------|------|--------|
| カバレッジ | 69% | 80% | 高 |
| 複雑度 | CC avg 8.2 | 6.0 | 中 |
| 命名規則準拠 | 97% | 100% | 低 |
| Null 安全性 | 78/100 | 95/100 | 高 |
| 依存関係 | 良好 | 良好 | - |

## 実装計画（3ヶ月）

### Q1 (即座) - テストカバレッジ向上
- [ ] MainViewModel テスト化（10 時間）
- [ ] 依存注入化（5 時間）
- [ ] カバレッジ: 69% → 75%

### Q2 (2-4 週) - 複雑度削減
- [ ] SearchReplaceService リファクタ（6 時間）
- [ ] JsonTreeBuilder 最適化（4 時間）
- [ ] CC avg: 8.2 → 6.5

### Q3 (5-12 週) - Null 安全性強化
- [ ] Null チェック漏れ修正（3 時間）
- [ ] Nullable Context チューニング（2 時間）
- [ ] スコア: 78 → 95

## 投資効果
- コード品質: 現状 74/100 → 目標 92/100
- バグ削減見込み: 35-40%
- テスト実行時間: 2.3秒 → 1.8秒（21% 短縮）
- 開発効率: メンテナンスコスト 30% 削減
```

**活用シーン**:
- ✅ プロジェクト管理者への品質報告
- ✅ 長期改善計画の立案
- ✅ ロードマップ作成

---

## 🛠️ セットアップ

### ファイル構成

```
skills/code-analyzer/
├── SKILL.md                              ← 本スキルの定義
├── evals/
│   ├── evals.json                        ← テストケース定義
│   ├── trigger-eval.json                 ← トリガー精度テスト
│   └── results.json                      ← テスト実行結果
├── references/
│   └── csharp-analysis-patterns.md       ← 分析パターン集
└── scripts/
    └── run_evals.py                      ← テスト実行スクリプト
```

### 環境要件

- Python 3.9+
- anthropic SDK
- C# コード & テスト結果ファイル

### インストール

```bash
cd skills/code-analyzer

# テストを実行
python -m scripts.run_evals evals/evals.json --verbose

# HTML レビュー画面を生成
python ../../skill-creator/eval-viewer/generate_review.py ./evals/
```

---

## 📝 実装チェックリスト

スキルを本番運用するまでの確認項目：

- [ ] SKILL.md の description がトリガーしやすいか（[ガイド](../../skill-creator/references/description-tuning-guide.md) 参照）
- [ ] evals.json に 8 個のテストケースが定義されているか
- [ ] 各テストケースの assertions が定量的か
- [ ] trigger-eval.json で精度テストを実施したか
- [ ] HTML レビュー画面を生成 & 確認したか

---

## 🎯 次のステップ

1. **スキルを試す**: `/code-analyzer` で起動
2. **テストを実行**: `python -m scripts.run_evals evals/evals.json`
3. **フィードバック収集**: ビジュアルレビューで改善提案
4. **Description チューニング**: 精度最適化
5. **本番デプロイ**: チーム内で利用開始

---

**作成日**: 2026-06-27  
**対象**: JsonEditor プロジェクト (C# .NET 8 WPF)  
**ベース**: skill-creator メタスキル v1.0

**関連リンク**:
- [SKILL.md](SKILL.md) - 技術的詳細
- [csharp-analysis-patterns.md](references/csharp-analysis-patterns.md) - 分析パターン集
- [skill-creator ガイド](../../skill-creator/SKILL.md) - メタスキル
