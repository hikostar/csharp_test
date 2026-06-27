# PR Authoring Prompt (JsonEditor)

以下の入力を使って、`.github/pull_request_template.md` に準拠したPR本文を作成してください。

## 入力フォーマット

```text
変更概要:

変更ファイル:

AI支援の有無:

検証結果:
- build:
- test:
- coverage:

回帰確認:

ドキュメント更新:
```

## 出力ルール

1. セクション名は PR テンプレートと同一にする。
2. AI支援PRの場合、「AI変更 検証結果」を必ず埋める。
3. 検証結果が不明な項目は `TBD` と書き、推測で埋めない。
4. 設計制約違反の可能性がある場合は「レビューメモ」に明記する。
