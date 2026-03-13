# トラブルシューティング

## Python が見つからない

```powershell
.\.venv\Scripts\python.exe --version
```

が成功するか確認してください。失敗する場合は `.venv` の作り直しを検討してください。

`PythonExecutablePath` にコマンド名を設定している場合は PATH 解決できる必要があります。相対パスを設定している場合は、Web プロジェクトのルート基準で正しいパスか確認してください。

## MarkItDown が見つからない

```powershell
.\.venv\Scripts\python.exe -m pip show markitdown
```

でパッケージ情報が表示されるか確認してください。

見つからない場合は、次を実行してください。

```powershell
.\.venv\Scripts\python.exe -m pip install "markitdown[all]"
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
