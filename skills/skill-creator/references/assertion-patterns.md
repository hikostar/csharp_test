# Assertion パターン集

Skill テストケースの Assertion（合格条件）で使用できるパターン集です。

## 使用方法

```json
{
  "id": 1,
  "prompt": "テストプロンプト",
  "expected_output": "期待する出力の説明",
  "assertions": [
    {
      "text": "説明",
      "type": "contains|regex|qualitative",
      "value": "検索対象（contains の場合）",
      "pattern": "正規表現（regex の場合）"
    }
  ]
}
```

## Assertion タイプ別パターン

### 1. Contains（文字列包含判定）

出力に特定のキーワードが含まれているかを判定します。

#### JSONエディタプロジェクト向けパターン

```json
[
  {
    "text": "出力にテスト成功数が含まれている",
    "type": "contains",
    "value": "成功"
  },
  {
    "text": "出力にカバレッジ率が含まれている",
    "type": "contains",
    "value": "%"
  },
  {
    "text": "出力に失敗テスト一覧が含まれている",
    "type": "contains",
    "value": "失敗"
  },
  {
    "text": "Markdown 形式の見出しが存在する",
    "type": "contains",
    "value": "#"
  }
]
```

### 2. Regex（正規表現パターン）

複雑なパターンマッチングが必要な場合に使用します。

#### JSONエディタプロジェクト向けパターン

```json
[
  {
    "text": "パーセンテージ形式のカバレッジが記載されている",
    "type": "regex",
    "pattern": "\\d+\\.\\d+%"
  },
  {
    "text": "ISO 形式の日付が含まれている",
    "type": "regex",
    "pattern": "\\d{4}-\\d{2}-\\d{2}"
  },
  {
    "text": "JSON形式のデータが含まれている",
    "type": "regex",
    "pattern": "\\{[^}]+\\}"
  },
  {
    "text": "テスト結果（成功/失敗数）の統計が含まれている",
    "type": "regex",
    "pattern": "成功:\\s*\\d+|失敗:\\s*\\d+"
  },
  {
    "text": "Markdown の表が存在する",
    "type": "regex",
    "pattern": "\\|.*\\|.*\\|"
  }
]
```

### 3. Qualitative（定性的評価）

主観的な評価が必要な項目。**ビジュアルレビューで人間が判定** します。

⚠️ **重要**: これらは Assertion として自動判定できません。ビジュアルレビュー画面で人間が確認してください。

```json
[
  {
    "text": "レポートの文章が分かりやすい",
    "type": "qualitative"
  },
  {
    "text": "提案されている改善案が実用的である",
    "type": "qualitative"
  },
  {
    "text": "出力フォーマットが一貫している",
    "type": "qualitative"
  }
]
```

## 定量的 vs 定性的 Assertion の選別

| 項目 | 判定方法 | 例 |
|------|--------|-----|
| **定量的** | 自動判定（contains, regex） | 数値、キーワード、フォーマット |
| **定性的** | 人間判定（qualitative） | 分かりやすさ、妥当性、一貫性 |

### ✅ 定量的な Assertion（OK）

```json
[
  {"text": "出力に『失敗』という単語が含まれている", "type": "contains", "value": "失敗"},
  {"text": "カバレッジが XX.XX% 形式で記載されている", "type": "regex", "pattern": "\\d+\\.\\d+%"},
  {"text": "JSON 出力が有効な JSON 形式である", "type": "regex", "pattern": "^\\{.*\\}$"}
]
```

### ❌ 定性的な Assertion（NG - 自動判定不可）

```json
[
  {"text": "出力が分かりやすい", "type": "qualitative"},  // 何が「分かりやすい」？主観的
  {"text": "提案が有用である", "type": "qualitative"},   // 「有用」の定義が不明確
  {"text": "レポートの品質が高い", "type": "qualitative"} // 「品質」は人間判定
]
```

## プロジェクト固有の Assertion テンプレート

### テスト分析スキル向け

```json
{
  "id": 1,
  "prompt": "tests/JsonEditor.App.Tests のテスト結果を分析してください",
  "assertions": [
    {
      "text": "総テスト数が記載されている",
      "type": "regex",
      "pattern": "テスト数:\\s*\\d+"
    },
    {
      "text": "成功・失敗の数値が記載されている",
      "type": "regex",
      "pattern": "成功:\\s*\\d+.*失敗:\\s*\\d+"
    },
    {
      "text": "カバレッジ率（69.67% 等）が記載されている",
      "type": "regex",
      "pattern": "\\d+\\.\\d+%"
    },
    {
      "text": "失敗テストの詳細が含まれている",
      "type": "contains",
      "value": "失敗"
    },
    {
      "text": "改善提案が含まれている",
      "type": "contains",
      "value": "提案"
    }
  ]
}
```

### PR 検証スキル向け

```json
{
  "id": 2,
  "prompt": "この C# コードを品質チェックしてください",
  "assertions": [
    {
      "text": "命名規則のフィードバックが含まれている",
      "type": "contains",
      "value": "命名"
    },
    {
      "text": "null 安全性の指摘が含まれている",
      "type": "contains",
      "value": "null"
    },
    {
      "text": "改善コード例が JSON または Markdown で提示されている",
      "type": "regex",
      "pattern": "\\{|```"
    }
  ]
}
```

## Assertion チェックリスト

新しい Assertion を追加する前に：

- [ ] 自動判定可能な項目か？（Yes → contains/regex, No → qualitative）
- [ ] パターンが曖昧でないか？（例: 「含まれている」なら何を？）
- [ ] 定量化できるか？（数値、キーワード、フォーマット）
- [ ] テストが再現可能か？（別の実行でも同じ結果になるか）
- [ ] 偽陽性・偽陰性がないか？（意図しない False PASS/FAIL）

## よくある間違い

### ❌ 間違い 1: 定性的評価を Assertion に含める

```json
// NG
{
  "text": "レポートが素晴らしい",
  "type": "qualitative"
}
```

**対策**: ビジュアルレビュー画面で人間が判定

### ❌ 間違い 2: 曖昧な Contains

```json
// NG
{
  "text": "結果が正しい",
  "type": "contains",
  "value": "結果"
}
```

**対策**: 具体的に何が含まれるべきかを明記

```json
// OK
{
  "text": "出力にテスト成功数が含まれている",
  "type": "contains",
  "value": "成功"
}
```

### ❌ 間違い 3: 正規表現が複雑すぎる

```json
// NG
{
  "text": "複雑なパターン",
  "type": "regex",
  "pattern": "(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)"
}
```

**対策**: シンプルで読みやすいパターンに

```json
// OK
{
  "text": "IP アドレス形式が含まれている",
  "type": "regex",
  "pattern": "\\d+\\.\\d+\\.\\d+\\.\\d+"
}
```

## 参考リンク

- [evals.json テンプレート](../evals/evals.json)
- [SKILL.md](../SKILL.md)
- [description-tuning-guide.md](description-tuning-guide.md)

---

**作成日**: 2026-06-27  
**対象プロジェクト**: JsonEditor (C# .NET 8 WPF)
