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

## 既知の制約

- 対象サイズは 10MB 程度を想定
- JSON Schema 検証は未実装
- 置換プレビューは簡易テキスト表示（差分ビューではない）
