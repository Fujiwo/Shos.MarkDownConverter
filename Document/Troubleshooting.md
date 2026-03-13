# トラブルシューティング

## Python 実行環境が見つからない

```powershell
.\.venv\Scripts\python.exe --version
```

が成功するか確認してください。失敗する場合は `PythonExecutablePath` の設定か Python 自体の導入状態を見直してください。

`PythonExecutablePath` が `python` のようなコマンド名なら PATH から解決できる必要があります。相対パスを指定している場合は、Web プロジェクトのルート基準で正しい場所を指しているか確認してください。

## MarkItDown が見つからない

```powershell
.\.venv\Scripts\python.exe -m pip show markitdown
```

で見つからない場合は、次を実行してください。

```powershell
.\.venv\Scripts\python.exe -m pip install "markitdown[all]"
```

## このファイル形式は受け付けていません と表示される

- 対応拡張子一覧に含まれているか確認してください。
- 必要なら [src/Shos.MarkDownConverter.Web/appsettings.json](../src/Shos.MarkDownConverter.Web/appsettings.json) の `AllowedExtensions` を見直してください。

## ファイルサイズが上限を超えています と表示される

- 画面のサイズ上限表示と `MaxUploadSizeBytes` の設定値を確認してください。
- サイズ超過時は API が 413 を返し、画面には上限超過の構造化エラーが表示されます。
- 必要なら元ファイルを小さくするか、設定上限を見直してください。

## ファイルを Markdown に変換できませんでした と表示される

- 元ファイルが破損していないか確認してください。
- 追加依存関係が必要な形式では `markitdown[all]` を導入してください。
- サーバーログの標準エラー出力と終了コードを確認してください。
