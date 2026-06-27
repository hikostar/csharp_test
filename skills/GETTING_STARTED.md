# skill-creator 導入ガイド（JsonEditor プロジェクト）

## 概要

このプロジェクトに **skill-creator** が導入されました。このメタスキルを使用することで、Claude と協力して高品質なスキルを効率的に開発できます。

## ファイル構成

```
skills/
└── skill-creator/
    ├── SKILL.md                              # メタスキル本体（主要説明）
    ├── evals/
    │   ├── evals.json                        # テストケース定義テンプレート
    │   ├── trigger-eval.json                 # トリガー判定テストテンプレート
    │   └── results.json                      # テスト実行結果（自動生成）
    ├── eval-viewer/
    │   └── generate_review.py                # HTML レビュー画面生成スクリプト
    ├── references/
    │   ├── description-tuning-guide.md       # Description の書き方ガイド
    │   ├── assertion-patterns.md             # Assertion パターン集
    │   └── test-analysis-patterns.md         # テスト解析パターン集
    └── scripts/
        ├── run_evals.py                      # テスト実行スクリプト
        └── run_loop.py                       # Description チューニングスクリプト
```

## クイックスタート（5分）

### ステップ1: Skill を認識させる

Claude Code で以下のスラッシュコマンドを試してみてください：

```
/skill-creator

またはこのスキルについて質問:
"skill-creator ってどう使うの？"
```

Claude がこのスキルを認識して説明してくれます。

### ステップ2: スキルのアイデアを伝える

例えば：

```
/skill-creator

以下のスキルを作成したいです：

【スキル名】test-analyzer（テスト分析スキル）

【目的】
JsonEditor.App.Tests のテスト結果を自動分析して：
- 失敗テストの根本原因を推測
- カバレッジ低下箇所を指摘
- パフォーマンス劣化を検出

【トリガー例】
- "最新のテスト結果をレポートして"
- "テストカバレッジが低い箇所を教えて"
- "テスト失敗の原因は？"

【出力形式】
- Markdown（GitHub で表示可能）
- JSON（プログラマティック利用可能）
```

### ステップ3: SKILL.md を自動生成

Claude がヒアリングから `SKILL.md` のドラフトを生成します：

```
skills/test-analyzer/
├── SKILL.md          ← 自動生成
├── evals/
│   └── evals.json    ← テストケース
└── references/       ← 詳細資料
```

### ステップ4: テストと評価

```bash
cd skills/test-analyzer
python -m scripts.run_evals evals/evals.json
```

出力例：
```
✓ Test 1: PASS (スキル有り: 2340 tokens, 2.3s)
✓ Test 2: PASS (スキル無し比較で 3x token削減)
✗ Test 3: FAIL (Markdown 見出し形式エラー)
```

### ステップ5: ビジュアルレビュー

```bash
python eval-viewer/generate_review.py ./evals/ --output review.html
```

ブラウザで `review.html` を開いて、スキル有り/無しの出力を比較。フィードバックを入力して「Submit」。

### ステップ6: 改善と最適化

Claude が feedback.json を読み込んで SKILL.md を改善。再度テスト実行。

## よく使う操作

### 新しいスキルを作る

```
/skill-creator

[具体的なスキルアイデアを説明]
```

Claude が全フローをサポートします。

### 既存スキルのテストを実行

```bash
cd skills/[skill-name]
python -m scripts.run_evals evals/evals.json --verbose
```

### Description を最適化

```bash
cd skills/[skill-name]
python -m scripts.run_loop \
  --eval-set trigger-eval.json \
  --skill-path . \
  --model claude-sonnet-4-6 \
  --max-iterations 5 \
  --verbose
```

### HTML レビュー画面を生成

```bash
cd skills/[skill-name]
python eval-viewer/generate_review.py ./evals/ --output review.html
# ブラウザで review.html を開く
```

## JsonEditor プロジェクト固有のTips

### テストデータの場所

スキル開発時に参照するテスト結果：

```
tests/JsonEditor.App.Tests/TestResults/
├── JsonEditor.App.Tests_2026-06-27.xml    # テスト結果（NUnit形式）
└── coverage.cobertura.xml                  # カバレッジレポート
```

### テストフレームワーク

- **主フレームワーク**: xUnit
- **テストプロジェクト**: tests/JsonEditor.App.Tests/
- **現在のカバレッジ**: 約 69.67% (2026-06-27 時点)

### よく作られるスキル

#### 1. テスト分析スキル
```
"tests/JsonEditor.App.Tests/TestResults/ を分析して失敗原因をレポート"
```

#### 2. PR検証スキル
```
"C# コードの品質チェック：命名規則、null 安全性、カバレッジ確保"
```

#### 3. ドキュメント生成スキル
```
"CONTRIBUTING.md や README を自動更新"
```

#### 4. パフォーマンス分析スキル
```
"PerformanceTests の結果から最適化候補を特定"
```

## よくある質問

### Q: スキルはどこに保存される？

A: プロジェクトの `skills/` ディレクトリに保存されます。Git で管理可能です。

```
skills/
├── skill-creator/           ← このメタスキル
├── test-analyzer/           ← テスト分析スキル（例）
└── code-reviewer/           ← コードレビュースキル（例）
```

### Q: スキルなしベースラインって何？

A: 同じプロンプトをスキル **なし** で Claude に実行させた結果です。スキルの有無による効果を定量化できます。

| 項目 | スキル有り | スキル無し |
|------|-----------|---------|
| 出力品質 | 構造化レポート | 一般的な説明 |
| トークン数 | 2,340 | 3,200 |
| 実行時間 | 2.3s | 2.5s |

### Q: Assertion って何？

A: テストケースの合格条件です。

```json
✅ 「出力に失敗テスト数が含まれている」（定量的）
❌ 「レポートが分かりやすい」（主観的→NG）
```

定性的な評価は **ビジュアルレビューで人間が判定** します。

### Q: トリガー精度ってなぜ重要？

A: 不正確な description だと：
- スキルが呼ばれない（トリガー漏れ）
- 場違いで呼ばれる（誤トリガー）

trigger-eval.json でチューニングすることで精度を上げられます。

### Q: 複数のスキルを同時に管理できる？

A: 可能です。各スキルは独立した SKILL.md を持つため、並行開発できます。

```bash
skills/
├── test-analyzer/       # 開発中
├── pr-validator/        # テスト中
└── code-reviewer/       # 本番運用中
```

## トラブルシューティング

### スキルが呼ばれない

**原因**: description が弱い、または短すぎる

**対策**: 
1. [description-tuning-guide.md](references/description-tuning-guide.md) を確認
2. 複雑性と具体的な用途を明示
3. trigger-eval.json でテスト

### テスト実行に失敗

**原因**: Python 環境、テストデータのパスが不正

**対策**:
```bash
# Python 3.9+ か確認
python --version

# anthropic SDK がインストールされているか確認
pip list | grep anthropic

# テストデータのパスを確認
ls tests/JsonEditor.App.Tests/TestResults/
```

### HTML レビュー画面が表示されない

**原因**: evals.json または results.json が不正な形式

**対策**:
```bash
# JSON の妥当性を確認
python -m json.tool evals/evals.json

# ファイルが存在するか確認
ls -la evals/
```

## 参考資料

| 資料 | 説明 |
|------|------|
| [SKILL.md](SKILL.md) | メタスキル本体（全体像） |
| [description-tuning-guide.md](references/description-tuning-guide.md) | Description の書き方 |
| [evals.json](evals/evals.json) | テストケースのテンプレート |
| [trigger-eval.json](evals/trigger-eval.json) | トリガー判定テストのテンプレート |

## 外部リンク

- 📚 [Anthropic skill-creator GitHub](https://github.com/anthropics/skills/tree/main/skills/skill-creator)
- 📖 [サーバーワークスブログ: skill-creator ガイド](https://blog.serverworks.co.jp/claude-code-skill-creator-guide)

## 次のステップ

1. **初めてのスキル作成**: `/skill-creator` で簡単なスキルを試作
2. **テスト実行**: evals.json を編集してテストを実行
3. **ビジュアルレビュー**: HTML レビュー画面でフィードバック
4. **本番スキル**: 実プロジェクトに合わせた複雑なスキルを開発

---

**導入日**: 2026-06-27  
**バージョン**: 1.0  
**対象プロジェクト**: JsonEditor (C# .NET 8 WPF)
