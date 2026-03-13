# テストガイド

## 通常テスト

```powershell
dotnet test Shos.MarkDownConverter.slnx
```

## UI を含む E2E テスト

UI テストは Playwright for .NET を使います。初回のみブラウザーの導入が必要です。

```powershell
dotnet build tests/Shos.MarkDownConverter.Web.E2ETests/Shos.MarkDownConverter.Web.E2ETests.csproj
powershell -ExecutionPolicy Bypass -File .\tests\Shos.MarkDownConverter.Web.E2ETests\bin\Debug\net10.0\playwright.ps1 install chromium
dotnet test tests/Shos.MarkDownConverter.Web.E2ETests/Shos.MarkDownConverter.Web.E2ETests.csproj
```

E2E テストでは次を確認します。

- ファイル選択から結果表示までの正常系
- コピーとダウンロード操作
- 非対応拡張子のエラー表示
- サイズ超過のエラー表示
- Python 実行環境不在相当のエラー表示

## 注意点

- E2E テストはローカルで Web アプリを実プロセスとして起動します。
- 並列実行でアプリのビルド出力が競合しないよう、E2E テストは逐次実行します。
- 実行後に `Shos.MarkDownConverter.Web.dll` がロックされる場合は、残留した `dotnet` プロセスがないか確認してください。
