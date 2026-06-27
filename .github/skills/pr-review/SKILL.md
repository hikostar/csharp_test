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

## 出力

1. 重大度順の指摘（High/Medium/Low）
2. 欠落テスト
3. 不足ドキュメント
4. マージ可否判定（Ready/Needs Changes）

## 注意

- 実装提案よりも、リスク指摘を優先する。
- 指摘はファイルパスを添えて具体化する。
- 推測で補完せず、未確認事項は未確認と明示する。
