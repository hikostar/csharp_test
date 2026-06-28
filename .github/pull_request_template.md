## 概要
<!-- 関連 Issue があれば記載: fixes #123 -->

## 変更種別
- [ ] バグ修正
- [ ] 機能追加
- [ ] リファクタリング
- [ ] ドキュメント更新
- [ ] CI/運用改善

## AI支援の有無
- [ ] AI支援PR（Copilot/Agent/Skill を利用）
- [ ] 非AI支援PR

## 設計制約チェック
- [ ] [設計仕様書](doc/Design_Specification.md) 第13章の制約に違反していない
- [ ] 依存方向（App -> Core）を維持している
- [ ] Core に UI/WPF 依存を追加していない

## テスト・検証
- [ ] `dotnet build JsonEditor.sln` が成功
- [ ] `dotnet test tests/JsonEditor.App.Tests/JsonEditor.App.Tests.csproj` が成功
- [ ] カバレッジ実行を確認（`--settings .runsettings --collect:"XPlat Code Coverage"`）
- [ ] [検証仕様兼結果報告書](doc/Verification_Spec&result.md) 第10章の回帰観点を確認

## AI変更 検証結果（AI支援PRのみ必須）
<!-- AI支援PRの場合は必ず記載。非AI支援PRでは削除可。 -->
```text
[AI変更 検証結果]
- 対象要件ID: FR-xxx, NFR-xxx
- 追加/更新テスト: TestClass.TestName, ...
- build: pass/fail
- test: pass/fail (passed/failed/skipped)
- coverage: line xx.xx%, branch xx.xx%
- 回帰観点: OK/NG (要点)
- 仕様/検証ドキュメント更新: 有/無
```

## ドキュメント更新
- [ ] ユーザー影響がある場合は [操作マニュアル](doc/User_Manual.md) を更新
- [ ] 設計変更がある場合は [設計仕様書](doc/Design_Specification.md) を更新
- [ ] 検証観点変更がある場合は [検証仕様兼結果報告書](doc/Verification_Spec&result.md) を更新

## レビューメモ
<!-- レビュアーに見てほしいポイントや未解決論点を記載 -->
