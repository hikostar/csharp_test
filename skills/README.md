---
name: claude-code-skill-creator
description: "JsonEditor プロジェクト向けの Claude Code スキル開発基盤。skill-creator メタスキルを活用して、高品質なスキルを効率的に開発・テスト・デプロイするための完全な環境を提供。"
---

# 📚 Claude Code Skill Creator — JsonEditor プロジェクト向けスキル開発基盤

## 🎯 このスキルについて

このプロジェクトは **skill-creator メタスキル** を統合した、Claude Code スキル開発の完全な基盤です。

Anthropic が公式公開した skill-creator を、JsonEditor (C# .NET 8 WPF) プロジェクト向けにカスタマイズしました。

## ✨ できること

1. **スキル設計**: 「〇〇なスキルを作りたい」と伝えるだけで要件ヒアリング
2. **自動生成**: ヒアリング内容から SKILL.md を自動作成
3. **テスト実行**: スキル有り/無しを並列実行して効果を定量化
4. **ビジュアルレビュー**: ブラウザで出力を比較、フィードバック入力
5. **改善サイクル**: フィードバックから自動改善
6. **精度チューニング**: Description のトリガー精度を最適化

## 📁 ファイル構成

```
skills/
├── GETTING_STARTED.md                        # 5分クイックスタート
├── README.md                                 # このファイル
│
├── skill-creator/                            # メタスキル（スキル開発用）
│   ├── SKILL.md                              # メタスキル本体（主要説明）
│   ├── evals/
│   │   ├── evals.json                        # テストケース定義テンプレート
│   │   ├── trigger-eval.json                 # トリガー判定テストテンプレート
│   │   └── results.json                      # テスト実行結果（自動生成）
│   ├── eval-viewer/
│   │   └── generate_review.py                # HTML レビュー画面生成スクリプト
│   ├── references/
│   │   ├── description-tuning-guide.md       # Description の書き方ガイド
│   │   └── assertion-patterns.md             # Assertion パターン集
│   └── scripts/
│       ├── run_evals.py                      # テスト実行スクリプト
│       └── run_loop.py                       # Description チューニングスクリプト
│
└── code-analyzer/                            # C# コード品質分析スキル
    ├── README.md                             # ⭐ セットアップガイド
    ├── SKILL.md                              # スキル定義（6つの分析）
    ├── evals/
    │   ├── evals.json                        # 8 つのテストケース
    │   ├── trigger-eval.json                 # 15 個のトリガー判定テスト
    │   └── results.json                      # テスト実行結果
    ├── references/
    │   └── csharp-analysis-patterns.md       # C# 分析パターン集
    └── scripts/
        └── [Python スクリプト]
```

## 📋 スキル一覧

| スキル | 説明 | ドキュメント |
|-------|------|------------|
| **skill-creator** | メタスキル（新しいスキル開発用） | [SKILL.md](skill-creator/SKILL.md) |
| **code-analyzer** | C# コード品質多角分析 | [README.md](code-analyzer/README.md) |

### code-analyzer スキル（新規）

**目的**: C# コード品質を 6 つの角度から分析 & 改善提案

**6つの分析**:
```
1. カバレッジ分析        → 未テスト関数を優先度付けで提案
2. 複雑度分析           → CC が高いメソッドをリファクタ提案
3. 依存関係分析         → 循環依存・層違反を検出
4. 命名規則チェック     → C# ガイドライン準拠確認
5. Null 安全性分析      → Null 参照エラーのリスク箇所特定
6. パフォーマンス分析   → ボトルネック検出 → 最適化提案
```

**トリガー例**:
```
/code-analyzer

または

"テストカバレッジが低い箇所をテストして"
"複雑度の高いメソッドをリファクタしたい"
"Null 参照エラーのリスク箇所を教えて"
```

**使用開始**: [code-analyzer/README.md](code-analyzer/README.md) を参照

## 🚀 5分クイックスタート

### ステップ1: スキルを認識させる

Claude Code で以下のいずれかを試してください：

```
/skill-creator

または

skill-creator について説明して
```

### ステップ2: スキルのアイデアを伝える

例えば：

```
/skill-creator

次のスキルを開発したいです：

【スキル名】test-analyzer

【目的】
テスト実行結果を自動分析して失敗原因を報告

【トリガー例】
- "テスト失敗の原因を分析して"
- "テストカバレッジが低い箇所を教えて"

【出力】
Markdown レポート + JSON データ
```

Claude が全フローをサポートします。

### ステップ3: ドキュメントを確認

- **最初**: [GETTING_STARTED.md](GETTING_STARTED.md) ← わかりやすい（これを読んでください）
- **詳細**: [skill-creator/SKILL.md](skill-creator/SKILL.md) ← 技術的な詳細
- **参考**: [references/](skill-creator/references/) ← ガイド・パターン集

## 📖 ドキュメント一覧

| ドキュメント | 対象者 | 説明 |
|-----------|-------|------|
| **GETTING_STARTED.md** | すべて | ⭐ ここから開始。5分で概要理解 |
| **SKILL.md** | スキル開発者 | 技術的な詳細。全5機能の解説 |
| **description-tuning-guide.md** | スキル開発者 | Description の書き方。トリガー精度UPのコツ |
| **assertion-patterns.md** | スキル開発者 | テストの合格条件（Assertion）のパターン集 |
| **README.md** | 管理者 | このファイル。統合ガイド |

## 💡 よくある使用例

### 例1: テスト分析スキル

```
テスト結果を自動分析して、失敗原因と改善提案をレポート化
```

**テストデータ**: `tests/JsonEditor.App.Tests/TestResults/`

### 例2: PR 検証スキル

```
C# コードの命名規則、null 安全性、カバレッジをチェック
```

### 例3: ドキュメント生成スキル

```
テスト結果から README/CONTRIBUTING を自動更新
```

## 🔧 セットアップ

### 環境要件

- Python 3.9+
- Claude API キー
- .NET 8 SDK（プロジェクト自体に必須）

### インストール

```bash
# リポジトリのクローン（すでに done）
cd skills/

# Python 依存関係をインストール（anthropic SDK など）
pip install anthropic
```

## 📊 スキル開発サイクル

```
┌─ スキルアイデア
│  │
├─→ ヒアリング（Claude が自動実施）
│  │
├─→ SKILL.md 自動生成
│  │
├─→ テストケース実行
│  │ ├─ スキル有り実行
│  │ └─ スキル無し（ベースライン）実行
│  │
├─→ ビジュアルレビュー（HTML）
│  │ └─ フィードバック入力
│  │
├─→ 改善（Claude が feedback.json を読み込み）
│  │
├─→ 再テスト
│  │
└─→ Description 最適化
   │
   └→ 本番運用
```

## 🎓 理解すべき3つのコンセプト

### 1. **スキル無しベースライン**

同じプロンプトを Claude に「スキルなし」で実行。スキルの有無による効果を定量化できます。

```
スキル有り:    出力品質⬆, トークン数⬇, 実行時間⬇
スキル無し:    一般的な説明, トークン数多, 実行時間長
→ スキルの効果を数値化
```

### 2. **Assertion（合格条件）**

テストケースの「このテストが成功した」という基準を定義します。

```
✅ 定量的（自動判定可）
  - "出力に『失敗』という単語が含まれている"
  - "カバレッジが XX.XX% 形式で記載されている"

❌ 定性的（人間判定）
  - "出力が分かりやすい" ← ビジュアルレビューで判定
```

### 3. **Description（スキルの説明）**

Claude がスキルを「呼ぶべきか」を判断する最重要要素。

```
❌ 弱い: "テスト結果を表示する"

✅ 強い: "複数フレームワーク（NUnit/xUnit）の結果を統合解析。
失敗原因推測、カバレッジ低下箇所指摘、パフォーマンス劣化検出。
CI パイプライン統合、Markdown レポート生成対応。"
```

詳しくは → [description-tuning-guide.md](skill-creator/references/description-tuning-guide.md)

## 📚 参考資料

- **Anthropic 公式**
  - [skill-creator GitHub](https://github.com/anthropics/skills/tree/main/skills/skill-creator)
  - [Claude API ドキュメント](https://docs.anthropic.com/)

- **日本語ガイド**
  - [サーバーワークスブログ](https://blog.serverworks.co.jp/claude-code-skill-creator-guide)

## ❓ よくある質問

### Q: スキル開発の経験がなくても大丈夫？

A: **大丈夫です！** このメタスキルは要件を伝えるだけで、Claude が自動でスキル開発をサポートします。

### Q: 実際に使うにはどうする？

A: [GETTING_STARTED.md](GETTING_STARTED.md) を読んでから `/skill-creator` を実行してください。

### Q: スキルはどこに保存される？

A: プロジェクトの `skills/` ディレクトリに保存されます。Git で管理可能です。

### Q: 複数スキルを同時に開発できる？

A: 可能です。`skills/` 以下に複数のディレクトリを作成できます。

```
skills/
├── test-analyzer/
├── pr-validator/
└── code-reviewer/
```

### Q: テストデータはどこから取得する？

A: JsonEditor プロジェクトの `tests/JsonEditor.App.Tests/TestResults/` を使用できます。

## 🛠️ トラブルシューティング

### スキルが呼ばれない

→ Description を強化してください（[ガイド](skill-creator/references/description-tuning-guide.md)）

### テスト実行に失敗

```bash
# Python 3.9+ を確認
python --version

# anthropic SDK をインストール
pip install anthropic
```

### HTML レビュー画面が表示されない

```bash
# JSON ファイルの妥当性を確認
python -m json.tool skills/skill-creator/evals/evals.json
```

## 📝 次のステップ

1. **[GETTING_STARTED.md](GETTING_STARTED.md) を読む** （5分）
2. **`/skill-creator` を実行** してスキル開発を試す
3. **テストを実行**: `python -m scripts.run_evals evals/evals.json`
4. **ビジュアルレビュー**: `python eval-viewer/generate_review.py evals/`
5. **本番スキルを開発**: プロジェクト固有のスキルを作成

## 📞 サポート

### 問題が発生した場合

1. [GETTING_STARTED.md](GETTING_STARTED.md) の「トラブルシューティング」セクションを確認
2. [references/](skill-creator/references/) のガイドを確認
3. Claude に質問（このスキルは Claude が作成・改善をサポートします）

## 📋 チェックリスト

導入完了確認：

- [ ] skills/ ディレクトリが存在する
- [ ] GETTING_STARTED.md が読める
- [ ] `/skill-creator` コマンドが認識される
- [ ] Python 3.9+ がインストールされている
- [ ] `pip install anthropic` が完了している

すべてチェック完了 → スキル開発開始可能！🎉

---

**導入日**: 2026-06-27  
**バージョン**: 1.0  
**対象プロジェクト**: JsonEditor (C# .NET 8 WPF)  
**ベース**: Anthropic skill-creator v1.0

**作成者**: Claude Code (GitHub Copilot)  
**更新履歴**: 初版作成
