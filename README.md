# Shos.MarkDownConverter

Shos.MarkDownConverter は、ブラウザーから単一ファイルをアップロードし、サーバー側で Python 版 MarkItDown を実行して Markdown を返す ASP.NET Core Web アプリケーションです。

## 必要環境

- .NET SDK 10.0 以上
- Python 3.10 以上
- Python 仮想環境 `.venv`
- Python パッケージ MarkItDown

## セットアップ

1. .NET 10 SDK をインストールします。
2. Python 3.10 以上をインストールします。
3. リポジトリ直下に仮想環境を作成します。
4. 仮想環境の Python に MarkItDown をインストールします。

```powershell
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install "markitdown[all]"
```

開発環境では [src/Shos.MarkDownConverter.Web/appsettings.Development.json](src/Shos.MarkDownConverter.Web/appsettings.Development.json) から `.venv\Scripts\python.exe` を使う設定です。MarkItDown の一部形式は追加依存関係に依存します。`[all]` を利用すると、README に記載した既定の対応拡張子を扱いやすくなります。

## 実行手順

```powershell
dotnet restore Shos.MarkDownConverter.slnx
dotnet run --project src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj
```

起動後に表示される URL をブラウザーで開いて利用します。

## 詳細ドキュメント

- セットアップ: [Document/SetupGuide.md](Document/SetupGuide.md)
- ユーザーマニュアル: [Document/UserManual.md](Document/UserManual.md)
- トラブルシューティング: [Document/Troubleshooting.md](Document/Troubleshooting.md)
- テストガイド: [Document/TestGuide.md](Document/TestGuide.md)

## 設定

MarkItDown 呼び出しに関する設定は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) と [src/Shos.MarkDownConverter.Web/appsettings.Development.json](src/Shos.MarkDownConverter.Web/appsettings.Development.json) の `MarkItDown` セクションで変更できます。

- `PythonExecutablePath`: Python 実行ファイルのパス。開発環境では `.venv\Scripts\python.exe` を使う想定です。
- `ModuleName`: 既定では `markitdown`
- `MaxUploadSizeBytes`: アップロード上限
- `AllowedExtensions`: 受け付ける拡張子一覧

`PythonExecutablePath` は相対パスでも指定でき、Web プロジェクトのルートを基準に絶対パスへ解決されます。共通設定は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) に置き、開発環境固有の Python パスは [src/Shos.MarkDownConverter.Web/appsettings.Development.json](src/Shos.MarkDownConverter.Web/appsettings.Development.json) で上書きする運用です。環境ごとに変更する場合は、ユーザーシークレットまたは環境変数を優先してください。

## ユーザーマニュアル

1. トップページで変換対象ファイルを 1 つ選択します。
2. `変換する` を押します。
3. 変換結果がテキストエリアに表示されます。
4. `コピー` でクリップボードにコピーできます。
5. `ダウンロード` で `.md` ファイルとして保存できます。

エラー時は、画面に次の 3 点を表示します。

- 何が起きたかの要約
- 考えられる原因
- 次に試す対処方法

## 対応ファイル形式の考え方

アプリケーションは MarkItDown の CLI をそのまま利用し、変換ルールを C# 側で再実装しません。既定の拡張子一覧は、日常的に扱うことが多い文書、表計算、テキスト、基本画像形式に絞っています。

- 文書: `.pdf`, `.docx`, `.pptx`, `.xlsx`
- テキスト系: `.txt`, `.md`, `.csv`, `.json`, `.xml`, `.html`, `.htm`
- 画像: `.jpg`, `.jpeg`, `.png`

より広い形式を扱いたい場合は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) の `AllowedExtensions` に追加してください。実際に変換できるかどうかは、インストールした MarkItDown のバージョンと追加依存関係に依存します。

## テスト

```powershell
dotnet test Shos.MarkDownConverter.slnx
```

単体テストでは入力検証、対応形式判定、MarkItDown 呼び出しラッパー、エラー整形を検証します。結合テストではアップロード API の正常系、変換失敗、Python 不在相当の応答を検証します。UI を含む E2E テストを実行する場合は、Playwright ブラウザーの導入が必要です。詳細は [Document/TestGuide.md](Document/TestGuide.md) を参照してください。

## トラブルシューティング

### Python が見つからない

- `MarkItDown:PythonExecutablePath` が正しいか確認してください。
- `.\.venv\Scripts\python.exe --version` が通るか確認してください。
- 画面上の「考えられる原因」と「対処方法」に従って設定値を見直してください。

### MarkItDown が見つからない

- `.\.venv\Scripts\python.exe -m pip show markitdown` でインストール状態を確認してください。
- アプリが `.venv\Scripts\python.exe` を参照しているか確認してください。
- 必要なら `\.\.venv\Scripts\python.exe -m pip install "markitdown[all]"` を再実行してください。

### 一部形式だけ失敗する

- `markitdown[all]` または必要なオプション依存関係が入っているか確認してください。
- 元ファイルが破損していないか確認してください。

### 変換結果が返らない

- アプリケーションログで標準エラー出力と終了コードを確認してください。
- ファイルサイズ上限と許可拡張子を確認してください。
- UI に表示される原因候補と対処方法を優先して確認してください。

## 既知の制約

- 変換品質と対応範囲は MarkItDown の実装とインストール済み依存関係に依存します。
- 本アプリは単一ファイル変換専用です。
- 変換結果はブラウザー上に保持し、サーバー側へ永続保存しません。
- YouTube URL などファイルアップロード以外の入力は対象外です。
