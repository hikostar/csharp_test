# Contributing to JsonEditor

この文書は、JsonEditor への変更提案とPRレビュー運用を定義する。

## 1. ブランチ運用

1. 機能追加は `feature/<topic>` で作業する。
2. バグ修正は `bugfix/<topic>` で作業する。
3. ドキュメント専用変更は `docs/<topic>` を推奨する。

## 2. PRタイトル規約

`[Area] Summary` 形式を使う。

- Area の例: `Core`, `App`, `Tests`, `Docs`, `CI`
- 例: `[Core] Fix regex timeout handling`

AI支援PRの場合はタイトルに `[AI]` か `[AI-assisted]` を含める。

## 3. 必須チェック

1. `dotnet build JsonEditor.sln`
2. `dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj`
3. `dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj --settings .runsettings --collect:"XPlat Code Coverage"`

## 4. AI支援PRルール

1. PR本文は `.github/pull_request_template.md` を使用する。
2. AI支援PRでは「AI変更 検証結果」を必ず記入する。
3. 未確定要件は推測実装せず、PR本文のレビューメモで論点化する。
4. 設計制約は `doc/Design_Specification.md` 第13章を準拠基準とする。

## 5. レビュー観点

1. 設計整合: 依存方向と責務境界を維持しているか。
2. 品質: 変更に対するテストが存在するか。
3. 検証: `doc/Verification_Spec&result.md` 第10章の回帰観点を満たすか。
4. ドキュメント: ユーザー挙動や設計変更に応じた更新が同一PRに含まれるか。

レビュー補助資産:

- Skill: `.github/skills/pull-request-review/SKILL.md`
- Prompt: `.github/prompts/pr-authoring.prompt.md`

Copilotカスタマイズの使い分け:

- `/delivery-orchestration` は要件整理から実装・検証・判定まで一気通貫で進めたいときに使う。
- `/development-support` は局所的な実装相談、変更影響確認、必要テスト整理に使う。
- `/pull-request-review` は PR差分の設計制約確認、不足テスト確認、ドキュメント不足確認に使う。

## 6. マージ前確認

1. CI（`CI` と `PR Review Guard`）が成功している。
2. PRテンプレートの必須欄が空欄でない。
3. 変更理由と検証結果がPR本文で追跡可能である。
