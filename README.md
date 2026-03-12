# Shos.MarkDownConverter

Shos.MarkDownConverter は、ブラウザーから単一ファイルをアップロードし、サーバー側で Python 版 MarkItDown を実行して Markdown を返す ASP.NET Core Web アプリケーションです。

## 必要環境

- .NET SDK 10.0 以上
- Python 3.10 以上
- Python パッケージ MarkItDown

## セットアップ

1. .NET 10 SDK をインストールします。
2. Python 3.10 以上をインストールします。
3. 任意の仮想環境を有効化します。
4. MarkItDown をインストールします。

```powershell
python -m pip install "markitdown[all]"
```

MarkItDown の一部形式は追加依存関係に依存します。`[all]` を利用すると、README に記載した既定の対応拡張子を扱いやすくなります。

## 実行手順

```powershell
dotnet restore Shos.MarkDownConverter.slnx
dotnet run --project src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj
```

起動後に表示される URL をブラウザーで開いて利用します。

## 設定

MarkItDown 呼び出しに関する設定は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) の `MarkItDown` セクションで変更できます。

- `PythonExecutablePath`: Python 実行ファイルのパス
- `ModuleName`: 既定では `markitdown`
- `MaxUploadSizeBytes`: アップロード上限
- `AllowedExtensions`: 受け付ける拡張子一覧

環境ごとに変更する場合は、ユーザーシークレットまたは環境変数を優先してください。値のハードコードは避けてください。

## ユーザーマニュアル

1. トップページで変換対象ファイルを 1 つ選択します。
2. `変換する` を押します。
3. 変換結果がテキストエリアに表示されます。
4. `コピー` でクリップボードにコピーできます。
5. `ダウンロード` で `.md` ファイルとして保存できます。

## 対応ファイル形式の考え方

アプリケーションは MarkItDown の CLI をそのまま利用し、変換ルールを C# 側で再実装しません。既定の拡張子一覧には、MarkItDown README で明示されている代表的な形式を含めています。

- 文書: `.pdf`, `.docx`, `.pptx`, `.xlsx`, `.xls`, `.epub`
- テキスト系: `.txt`, `.md`, `.csv`, `.json`, `.xml`, `.html`, `.htm`
- メディア: `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.tif`, `.tiff`, `.webp`, `.wav`, `.mp3`
- その他: `.zip`, `.msg`, `.eml`

実際に変換できるかどうかは、インストールした MarkItDown のバージョンと追加依存関係に依存します。

## テスト

```powershell
dotnet test Shos.MarkDownConverter.slnx
```

単体テストでは入力検証、対応形式判定、MarkItDown 呼び出しラッパー、エラー整形を検証します。結合テストではアップロード API の正常系、変換失敗、Python 不在相当の応答を検証します。

## トラブルシューティング

### Python が見つからない

- `MarkItDown:PythonExecutablePath` が正しいか確認してください。
- `python --version` が通るか確認してください。

### MarkItDown が見つからない

- `python -m pip show markitdown` でインストール状態を確認してください。
- 仮想環境を使っている場合は、アプリがその Python を参照しているか確認してください。

### 一部形式だけ失敗する

- `markitdown[all]` または必要なオプション依存関係が入っているか確認してください。
- 元ファイルが破損していないか確認してください。

### 変換結果が返らない

- アプリケーションログで標準エラー出力と終了コードを確認してください。
- ファイルサイズ上限と許可拡張子を確認してください。

## 既知の制約

- 変換品質と対応範囲は MarkItDown の実装とインストール済み依存関係に依存します。
- 本アプリは単一ファイル変換専用です。
- 変換結果はブラウザー上に保持し、サーバー側へ永続保存しません。
- YouTube URL などファイルアップロード以外の入力は対象外です。
