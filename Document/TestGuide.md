# テストガイド

## テスト実行

```powershell
dotnet test Shos.MarkDownConverter.slnx
```

## 確認している内容

- 単体テスト: 入力検証、設定正規化、エラー整形、変換サービス、キャンセル時の外部プロセス回収
- API 結合テスト: 正常系、Python 起動失敗、サイズ超過、未処理例外を含む JSON 契約
- E2E テスト: 結果表示、コピー、ダウンロード、非対応拡張子、サイズ超過、Python 起動失敗の表示

## Playwright ブラウザーが未導入の場合

```powershell
dotnet build tests/Shos.MarkDownConverter.Web.E2ETests/Shos.MarkDownConverter.Web.E2ETests.csproj
powershell -ExecutionPolicy Bypass -File .\tests\Shos.MarkDownConverter.Web.E2ETests\bin\Debug\net10.0\playwright.ps1 install chromium
```

E2E テスト実行中に `Shos.MarkDownConverter.Web.dll` のロックで再ビルドに失敗した場合は、残留している `dotnet` プロセスがないか確認してから再実行してください。
