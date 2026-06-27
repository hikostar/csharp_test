# AI支援PR 検証サンプル

この文書は、`.github/workflows/pr-review-agent.yml` の動作確認を行うためのサンプルを示す。

## 1. 成功例（AI支援PR）

以下をPR本文に含めると、テンプレート検証を通過する。

```text
## AI支援の有無
- [x] AI支援PR（Copilot/Agent/Skill を利用）
- [ ] 非AI支援PR

## AI変更 検証結果（AI支援PRのみ必須）
[AI変更 検証結果]
- build: pass
- test: pass (59/59, failed 0, skipped 0)
- coverage: line 70.35%, branch 54.23%
- 回帰観点: OK (JSON検証/検索置換/保存復元/テーマ)
- 仕様/検証ドキュメント更新: 有
```

## 2. 失敗例（AI支援PR）

以下の場合、`PR Review Guard` は失敗する。

### 2.1 必須項目欠落

- `- coverage:` 行がない
- `- 回帰観点:` 行がない

### 2.2 プレースホルダ未置換

- `pass/fail` のまま
- `xx.xx%` のまま

## 3. 非AI支援PRの扱い

1. 「AI支援PR」のチェックが未選択で、タイトルにも `[AI]` がない場合、ガードはスキップされる。
2. ただし、通常のCIは実行される。

## 4. 手動検証手順

1. 成功例の本文でPRを作成し、`PR Review Guard` が成功することを確認する。
2. 失敗例の本文でPRを更新し、`PR Review Guard` が失敗することを確認する。
3. 失敗理由ログが「Missing required AI verification field」または「placeholders」であることを確認する。

## 5. ローカル再現手順

GitHub 上でPRを作る前に、ローカルで成功/失敗の両ケースを確認できる。

```powershell
./scripts/validate-pr-guard-local.ps1
```

期待結果:

- `SUCCESS => Passed=True`
- `FAILURE => Passed=False`

## 6. 運用メモ

- AI支援PRでタイトルに `[AI]` を付けると判定が明確になる。
- PRテンプレートは毎回最新を使う（手動コピーの再利用を避ける）。
