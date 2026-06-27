---
name: skill-creator
description: "JSONエディタプロジェクト向けのスキル作成メタスキル。新しいスキルの要件ヒアリング、SKILL.mdの自動生成、テストケース設計、定量評価、改善サイクルを支援する。スキル開発の品質を確保しながら開発効率を向上させるメタスキル。"
---

# skill-creator — JSONエディタプロジェクト向けスキル生成ツール

## 概要

このスキルは Claude に「スキル作成」のプロセスを自動化させる**メタスキル**です。

通常のスキル開発フローは以下の課題を抱えています：

- 要件の曖昧性によるスキル品質低下
- 試行錯誤による開発時間の増加
- テストと評価プロセスの欠落
- description のトリガー精度が不確定

このスキルを使用することで、以下のサイクルを **Claude と協力して** 実行できます：

```
[スキル設計] → [SKILL.md生成] → [テスト実行] → [品質評価] → [改善] → [最適化]
```

## このスキルが提供する機能

### 1. スキル設計ヒアリング

「〇〇なスキルを作りたい」と伝えるだけで、以下を自動確認します：

- スキルで実現する具体的なゴール
- トリガーする発話パターン（should-trigger / should-not-trigger）
- 期待する出力フォーマット
- テストケースの必要性

**例**
```
ユーザー: "C# プロジェクトのテスト結果を集計するスキルを作りたい"

Claude が自動確認:
  1. 何を実現するか？（リアルタイム集計？レポート生成？）
  2. どう発話するか？（/test-report? 手動呼び出し？）
  3. 出力は？（JSON？Markdown表？）
  4. テスト対象は？（実プロジェクトの TestResults/?）
```

### 2. SKILL.md の自動生成

ヒアリング内容をもとに **実行可能な SKILL.md** を生成します。

**特に重要: description の書き方**

```yaml
# ❌ トリガーしない description（弱い）
description: "テスト結果を表示するスキル"

# ✅ トリガーしやすい description（強い）
description: "C# プロジェクトのテスト実行結果を分析・集計・レポート化するスキル。
テスト失敗の原因分析、カバレッジ低下箇所の指摘、パフォーマンス低下の検出などに用いる。
TestResults/ ディレクトリを走査して、JSON/XML/Markdown レポートを生成する。"
```

Claude は「十分自分でできる」と判断するとスキルを呼ばないため、**複雑性と用途を明示する** ことがコツです。

### 3. テストケース実行と定量評価

スキルの品質を **定量的に測定** します。

#### テストケースの定義 (`evals/evals.json`)

```json
{
  "skill_name": "test-analyzer",
  "evals": [
    {
      "id": 1,
      "prompt": "tests/JsonEditor.App.Tests/TestResults/ 以下のテスト結果を分析して問題点をレポートして",
      "expected_output": "失敗したテストケース、カバレッジ、パフォーマンス劣化を指摘"
    },
    {
      "id": 2,
      "prompt": "このプロジェクトのテスト成功率を教えて",
      "expected_output": "スキルを呼ばずに自分で計算する（スキルなしベースライン）"
    }
  ]
}
```

#### Assertion（合格条件）の設定

```json
{
  "assertions": [
    {
      "text": "出力に失敗テスト数が含まれている",
      "type": "contains"
    },
    {
      "text": "Markdown の見出し構造が正しい",
      "type": "qualitative"
    },
    {
      "text": "カバレッジ数値が前回比較可能な形式",
      "type": "regex",
      "pattern": "\\d+\\.\\d+%"
    }
  ]
}
```

**重要ポイント：スキル有り vs スキル無しの並列実行**

```
スキルあり実行:  [SKILL活用] → 出力A（品質・速度・トークン数を測定）
スキルなし実行:  [素のClaude] → 出力B（ベースライン）

比較: 出力A が明らかに出力B より優れているか？を定量化
```

### 4. ビジュアルレビュー（eval-viewer）

テスト実行後、`eval-viewer/generate_review.py` でブラウザレビュー画面を生成します。

**Outputs タブ**
- テストケースごとに「スキルあり」と「スキルなし」の出力を並列表示
- テキストエリアでフィードバックを入力可能

**Benchmark タブ**
- パス率 / 実行時間 / トークン数の統計
- 前回イテレーション（feedback.json）との比較グラフ

### 5. Description トリガー精度チューニング

スキル完成に近づいたら、description を自動最適化します：

```powershell
python -m scripts.run_loop `
  --eval-set trigger-eval.json `
  --skill-path ./skills/skill-creator `
  --model claude-sonnet-4-6 `
  --max-iterations 5 `
  --verbose
```

20件のトリガー判定テスト（should-trigger / should-not-trigger）を用いて：
- train セット 60% でチューニング
- test セット 40% で最終検証
- オーバーフィット防止のため test スコアが最高の description を選択

## 実装ガイド

### ステップ1: スキル要件定義

```bash
# スキルを呼び出す
/skill-creator
```

Claude に以下を伝えます：

```
作りたいスキル: "C# テストレポート生成スキル"

目的: 
- JsonEditor.App.Tests のテスト結果を自動分析
- 失敗原因の推測と改善提案を含むレポート生成
- CI/CD パイプラインへの統合想定

トリガー例:
- "最新のテスト結果をレポートして"
- "テストカバレッジが低い箇所を教えて"
- "テスト失敗の根本原因を分析して"

出力形式:
- Markdown（GitHub で表示可能）
- JSON（プログラマティック利用）
```

### ステップ2: SKILL.md 自動生成

Claude がヒアリング結果から SKILL.md のドラフトを生成：

```
skills/
└── test-analyzer/
    ├── SKILL.md           ← 自動生成
    ├── references/
    │   ├── test-analysis-patterns.md
    │   └── assertion-patterns.md
    ├── scripts/
    │   └── parse_testresults.py
    └── evals/
        ├── evals.json     ← 初期テストケース
        └── trigger-eval.json
```

### ステップ3: テスト実行

```bash
cd skills/test-analyzer
python -m scripts.run_evals evals/evals.json
```

出力：
```
✓ Test 1: PASS (トークン数: 2340, 実行時間: 2.3s)
✓ Test 2: PASS (スキルなし比較: トークン数 3x削減)
  Assertion "Markdown 見出し正確性": PASS
  Assertion "カバレッジ数値": PASS
✗ Test 3: FAIL
  → フィードバック: "カバレッジ下がった項目も明示的に報告すること"
```

### ステップ4: ビジュアルレビュー

```bash
cd skills/test-analyzer
python eval-viewer/generate_review.py evals/ --output review.html
# ブラウザで review.html を開く
```

レビュー画面で：
1. 各テストケースを確認
2. フィードバックを入力
3. 「Submit All Reviews」 → `feedback.json` に保存

### ステップ5: SKILL.md 改善

Claude が `feedback.json` を読み込み：
- description のトリガー精度を調整
- ロジックを修正
- 出力フォーマットを改善

### ステップ6: トリガー精度チューニング

```bash
python -m scripts.run_loop \
  --eval-set trigger-eval.json \
  --skill-path ./skills/test-analyzer \
  --model claude-sonnet-4-6 \
  --max-iterations 5
```

description の複数候補から最適なものが自動選択されます。

## つまずきやすいポイント

### ❌ SKILL.md が長くなりすぎる

**症状**: 500行を超えている

**対策**: 詳細ロジックを `references/` に分離して SKILL.md からリンク

```markdown
# SKILL.md（簡潔）
詳細は [テスト解析パターン](references/test-analysis-patterns.md) を参照

# references/test-analysis-patterns.md（詳細）
- パターン1: NUnit 形式の XML 解析
- パターン2: xUnit JSON 形式
- ...（詳細な実装）
```

### ❌ Assertion をすべてのケースに書く

**症状**: 「Markdown 文体が正しい」という主観的な評価を assertion で判定しようとする

**対策**: 定性的評価は **ビジュアルレビューで人間が判断** する

```json
✅ 定量的な assertion（OK）
{
  "text": "出力に失敗テスト数が含まれている",
  "type": "contains"
}

❌ 定性的な assertion（NG）
{
  "text": "レポートが分かりやすい",
  "type": "qualitative"  // これは人間が判定
}
```

### ❌ Description を短くしすぎる

**症状**: スキルが呼ばれない（Claude が「自分でできる」と判断）

**対策**: トリガー例を複数書き、複雑性を明示

```yaml
❌ 弱い
description: "テスト結果を分析する"

✅ 強い
description: "C# .NET プロジェクトのテスト実行結果を解析・集計・レポート化するスキル。
複数のテストフレームワーク（NUnit/xUnit）、複数の出力形式（XML/JSON）に対応。
失敗の根本原因分析、カバレッジ低下箇所の指摘、パフォーマンス劣化検出。
CI 環境での自動実行、Markdown レポート生成、スラッシュコマンド統合に活用。"
```

## プロジェクト固有の設定

このプロジェクトで skill-creator を使用する際の注意：

### テストデータの場所

```
tests/JsonEditor.App.Tests/TestResults/
  ├── テストプロセッサ出力（XML形式）
  └── Cobertura カバレッジレポート
```

### サポートするテストフレームワーク

- **NUnit** （xUnit も一部対応）
- **coverlet.collector** による Cobertura カバレッジ

### 環境要件

- Python 3.9+
- .NET 8 SDK
- anthropic Python SDK

## よくある質問

### Q: スキルなしベースラインは何ですか？

Claude にスキルなしで同じプロンプトを実行します。スキルの有無で結果を比較することで、「スキル導入による効果」を定量化できます。

### Q: Feedback を反映させるには？

1. eval-viewer で feedback を入力
2. Claude にフィードバック内容を見せる
3. Claude が SKILL.md を改善
4. 再テスト実行

### Q: 複数のスキルを管理できますか？

可能です。`skills/` 直下に複数のスキルディレクトリを作成：

```
skills/
├── skill-creator/      ← このスキル（メタスキル）
├── test-analyzer/      ← テスト分析スキル
├── pr-validator/       ← PR 検証スキル
└── code-reviewer/      ← コードレビュースキル
```

## 参考資料

- [Anthropic skill-creator GitHub](https://github.com/anthropics/skills/tree/main/skills/skill-creator)
- サーバーワークスブログ: [skill-creator とは？](https://blog.serverworks.co.jp/claude-code-skill-creator-guide)

---

**更新日**: 2026-06-27  
**バージョン**: 1.0
