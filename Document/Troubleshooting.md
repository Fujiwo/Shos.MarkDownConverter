# トラブルシューティング

## Python が見つからない

```powershell
.\.venv\Scripts\python.exe --version
```

が成功するか確認してください。失敗する場合は `.venv` の作り直しを検討してください。

## MarkItDown が見つからない

```powershell
.\.venv\Scripts\python.exe -m pip show markitdown
```

でパッケージ情報が表示されるか確認してください。

## 対応形式なのに変換できない

- `markitdown[all]` を再インストールしてください。
- 元ファイルが破損していないか確認してください。
- アプリケーションログの標準エラー出力を確認してください。

## UI テストが動かない

Playwright ブラウザーが未導入の可能性があります。手順は [Document/TestGuide.md](TestGuide.md) を参照してください。
