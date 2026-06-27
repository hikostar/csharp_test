# JsonEditor

.NET 8 + WPF で作成した JSON エディタの実装開始版です。

## 現在の実装範囲

- JSONテキスト編集
- 本格構文ハイライト（AvalonEdit）
- リアルタイム JSON バリデーション
- JSON ツリービュー表示
- 検索/置換（通常文字列、大小文字切替、正規表現）
- 検索ナビゲーション（次へ/前へ）と選択ジャンプ
- 正規表現置換プレビュー（先頭30件）
- ファイル読込/保存
- ライト/ダークテーマ切替
- Undo/Redo（AvalonEdit 標準機能）
- 一定間隔の自動保存バックアップ（.autosave）
- 自動保存バックアップ復元（ファイルオープン時の確認 + 手動復元）

## 実行方法

1. .NET 8 SDK をインストール
2. ビルド

```powershell
dotnet build JsonEditor.sln
```

3. 起動

```powershell
dotnet run --project src/JsonEditor.App/JsonEditor.App.csproj
```

## エクスプローラーから実行する方法

1. 配布用 exe を生成

```powershell
powershell -ExecutionPolicy Bypass -File scripts/publish-exe.ps1
```

2. 生成された exe をダブルクリック

- dist/JsonEditor-win-x64/JsonEditor.App.exe
- JsonEditor.exe

補足:

- 配布フォルダを丸ごと移動しても実行できます。
- scripts/publish-exe.ps1 実行後はルートの JsonEditor.exe も更新されるので、今後はこの1ファイルをダブルクリックすれば起動できます。
- コード修正後は上記 publish コマンドを再実行して exe を更新してください。

## テスト

```powershell
dotnet test JsonEditor.sln
```

## ドキュメント

- [設計仕様書](doc/Design_Specification.md)
- [操作マニュアル](doc/User_Manual.md)
- [検証仕様兼結果報告書](doc/Verification_Spec&result.md)
- [AI支援PR 検証サンプル](doc/AI_PR_Validation_Examples.md)
- [コントリビューションガイド](CONTRIBUTING.md)

## Copilot運用ガイド（本リポジトリ）

### 目的

- 仕様逸脱を減らし、レビュー手戻りを抑える
- AI 生成コードでも設計・検証責任を明確にする

### 適用範囲

- `src/JsonEditor.App`
- `src/JsonEditor.Core`
- `tests/JsonEditor.App.Tests`
- `doc/*.md`（仕様/検証ドキュメント更新）

### 責任分界

- Copilot: 変更案生成、テスト雛形、ドキュメント下書き
- 開発者: 要件確定、最終判断、実行結果確認、マージ判断
- レビューア: 設計整合、例外系、回帰リスク、検証妥当性の確認

### 最低ルール

1. 変更時は設計制約を優先する（詳細は [設計仕様書](doc/Design_Specification.md) の「Copilot適用時の設計制約」参照）
2. 変更時は最小検証セットを実行する（詳細は [検証仕様兼結果報告書](doc/Verification_Spec&result.md) の「AI支援変更時の最小検証セット」参照）
3. 仕様影響がある場合はドキュメント更新を同一 PR に含める
4. 不正確な推測実装を避け、未確定要件は TODO ではなく論点として明示する

### 推奨プロンプト雛形

以下を埋めて依頼すると、要件漏れと再修正が減る。

```text
目的:
- 何を達成したいか

制約:
- 変更してはいけない挙動/公開 API
- 対象ファイル/対象外ファイル

受入条件:
- 期待するユーザー挙動
- 必須テスト（単体/統合/性能）
- ドキュメント更新有無
```

### PR運用（AI支援PR）

1. PR本文は `.github/pull_request_template.md` を使用する
2. AI支援PRでは「AI変更 検証結果」欄の記入を必須とする
3. 作成者は build/test/coverage の実行結果を記入する
4. レビューアは `.agent.md` と `.github/skills/pr-review/SKILL.md` の観点で確認する

### Copilotカスタマイズ資産

- リポジトリ制約: `.instructions.md`
- 標準プロンプト集: `.prompt.md`
- PR本文生成プロンプト: `.github/prompts/pr-authoring.prompt.md`
- PRレビューAgent: `.agent.md`
- PRレビューSkill: `.github/skills/pr-review/SKILL.md`
- 開発支援Skill: `.github/skills/dev-support/SKILL.md`
- 運用手順: `CONTRIBUTING.md`
- ワークフロー検証例: `doc/AI_PR_Validation_Examples.md`
- ローカル検証スクリプト: `scripts/validate-pr-guard-local.ps1`

## 既知の制約

- 対象サイズは 10MB 程度を想定
- JSON Schema 検証は未実装
- 置換プレビューは簡易テキスト表示（差分ビューではない）
