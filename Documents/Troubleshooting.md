# トラブルシューティング

## Python が見つからない

```powershell
.\.venv\Scripts\python.exe --version
```

が成功するか確認してください。失敗する場合は `.venv` の作り直しを検討してください。

`PythonExecutablePath` にコマンド名を設定している場合は PATH 解決できる必要があります。相対パスを設定している場合は、Web プロジェクトのルート基準で正しいパスか確認してください。

Azure App Service へ発行した環境では、`.python-runtime\Scripts\python.exe` が配置されているか、`MarkItDown__PythonExecutablePath` がその場所を指しているか確認してください。

## MarkItDown が見つからない

```powershell
.\.venv\Scripts\python.exe -m pip show markitdown
```

でパッケージ情報が表示されるか確認してください。

見つからない場合は、次を実行してください。

```powershell
.\.venv\Scripts\python.exe -m pip install "markitdown[all]"
```

publish で同梱する配布専用ランタイムが壊れている場合は、発行元の Windows 環境で次を実行して再生成できるか確認してください。

```powershell
dotnet publish src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj -c Release
```

そのうえで、publish 出力に `.python-runtime\Scripts\python.exe` と `markitdown` の依存が含まれているか確認してください。

## publish で Python ランタイム生成に失敗する

- 発行元マシンで `python --version` が成功するか確認してください
- [requirements.publish.txt](../requirements.publish.txt) が存在するか確認してください
- [scripts/Prepare-PythonRuntime.ps1](../scripts/Prepare-PythonRuntime.ps1) が実行可能か確認してください
- 別の Python 実行ファイルを使う場合は、publish 時に `PythonBuildCommand` MSBuild プロパティを指定してください

ランタイムの前提は次のとおりです。

- 開発時の Python はリポジトリ直下の `.venv` を使います
- publish 時の staging は Web プロジェクト配下の `obj/python-runtime` にだけ作ります
- publish 出力へコピーされる最終配置名は `.python-runtime` です
- `src/Shos.MarkDownConverter.Web/python-runtime` と `src/Shos.MarkDownConverter.Web/.python-runtime` は通常運用では残しません

`error NETSDK1152` で publish が止まる場合は、過去の試行で生成された [src/Shos.MarkDownConverter.Web](../src/Shos.MarkDownConverter.Web) 配下の `python-runtime` または `.python-runtime` が publish 項目に混ざっている可能性があります。不要なローカル生成物なので削除してから再試行してください。まとめて掃除する場合は次を実行します。

```powershell
.\scripts\Clean-PythonRuntime.ps1
```

## 対応形式なのに変換できない

- `markitdown[all]` を再インストールしてください。
- 元ファイルが破損していないか確認してください。
- アプリケーションログの標準エラー出力を確認してください。

## ファイルサイズが上限を超えている

- 画面のサイズ上限表示と、[src/Shos.MarkDownConverter.Web/appsettings.json](../src/Shos.MarkDownConverter.Web/appsettings.json) の `MaxUploadSizeBytes` を確認してください。
- サイズ超過時はファイルサイズ基準の上限超過メッセージが表示されます。
- 必要ならファイルを小さくするか、上限を見直してください。

## コピーできない

- ブラウザーがクリップボード操作を許可しているか確認してください。
- 失敗時は権限確認を促すメッセージが表示されます。
- すぐに必要な場合はダウンロード機能を使ってください。

## UI テストが動かない

Playwright ブラウザーが未導入の可能性があります。手順は [Documents/TestGuide.md](TestGuide.md) を参照してください。

## UI 変更が反映されない

- ブラウザーを再読み込みして、古い JavaScript キャッシュが残っていないか確認してください。
- [src/Shos.MarkDownConverter.Web/wwwroot/app.js](../src/Shos.MarkDownConverter.Web/wwwroot/app.js) から読み込むモジュール群に構文エラーがないか確認してください。
- 開発中はブラウザーの開発者ツールで Console と Network を確認してください。
