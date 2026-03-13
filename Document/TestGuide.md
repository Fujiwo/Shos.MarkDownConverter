# テストガイド

## テスト実行

```powershell
dotnet test Shos.MarkDownConverter.slnx
```

## 確認している内容

- 単体テスト: 入力検証、エラー整形、変換サービス
- API 結合テスト: エラー応答の JSON 契約
- E2E テスト: 画面に要約、原因候補、対処方法が表示されること

## Playwright ブラウザーが未導入の場合

```powershell
dotnet build tests/Shos.MarkDownConverter.Web.E2ETests/Shos.MarkDownConverter.Web.E2ETests.csproj
powershell -ExecutionPolicy Bypass -File .\tests\Shos.MarkDownConverter.Web.E2ETests\bin\Debug\net10.0\playwright.ps1 install chromium
```
