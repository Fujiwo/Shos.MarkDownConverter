# トラブルシューティング

## Python 実行環境が見つからない

```powershell
.\.venv\Scripts\python.exe --version
```

が成功するか確認してください。失敗する場合は `PythonExecutablePath` の設定か Python 自体の導入状態を見直してください。

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

## ファイルを Markdown に変換できませんでした と表示される

- 元ファイルが破損していないか確認してください。
- 追加依存関係が必要な形式では `markitdown[all]` を導入してください。
- サーバーログの標準エラー出力と終了コードを確認してください。
