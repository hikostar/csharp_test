---
name: pull-request-review
description: 'PR差分を設計仕様と検証仕様に照らしてレビューする。Use for pull request review, design-constraint checks, missing tests, and documentation gap checks.'
---

# Skill: JsonEditor PR Review

## 目的

JsonEditor の PR 差分を、設計仕様と検証仕様に照らしてレビューする。

## 入力

1. PR差分
2. `doc/Design_Specification.md`
3. `doc/Verification_Spec&result.md`
4. `README.md` の Copilot運用章

## 手順

1. 差分を層ごとに分類する（App/Core/Tests/Docs）。
2. 設計制約（依存方向、責務境界、非機能制約）違反を確認する。
3. 変更に対応するテスト有無を確認する。
4. AI支援PRなら PRテンプレートの検証欄の記入有無を確認する。
5. ドキュメント更新の必要性と実際の更新有無を照合する。

## レビュー観点

### 1. 設計整合

1. `JsonEditor.Core` に WPF 参照が混入していない。
2. 依存方向（App -> Core）が維持されている。
3. 検索/置換/検証ロジックが Core に残っている。

### 2. 品質

1. 変更に対応するテストが存在する。
2. 既存テストを削減していない。
3. 例外方針（`StatusMessage`）を壊していない。

### 3. PRテンプレート

1. AI支援PRなら「AI変更 検証結果」が記入されている。
2. build/test/coverage が明記されている。
3. ドキュメント更新有無が明記されている。

## 出力

1. 重大度順の指摘（High/Medium/Low）
2. 欠落テスト
3. 不足ドキュメント
4. マージ可否判定（Ready/Needs Changes）

以下の形式でまとめる。

```text
Findings (High to Low)
- [Severity] file: reason

Missing Tests
- ...

Doc Gaps
- ...

Decision
- Ready / Needs Changes
```

## 注意

- 実装提案よりも、リスク指摘を優先する。
- 指摘はファイルパスを添えて具体化する。
- 推測で補完せず、未確認事項は未確認と明示する。