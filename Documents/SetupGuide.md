# セットアップガイド

## 前提環境

- Windows 上の .NET SDK 10.0 以上
- Python 3.10 以上
- PowerShell

## 初回セットアップ

1. リポジトリのルートへ移動します。
2. Python 仮想環境を作成します。
3. MarkItDown を仮想環境へインストールします。
4. .NET 依存関係を復元します。

```powershell
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install "markitdown[all]"
dotnet restore Shos.MarkDownConverter.slnx
```

## Azure App Service 発行前提

Windows の Azure App Service へ Visual Studio の「発行」で配置する場合は、publish 時に配布専用の `.python-runtime` を自動生成します。

- Python 依存の正本は [requirements.publish.txt](../requirements.publish.txt) です
- 発行前処理スクリプトは [scripts/Prepare-PythonRuntime.ps1](../scripts/Prepare-PythonRuntime.ps1) です
- Web プロジェクトの publish では `.python-runtime` が発行物へ同梱されます

発行元マシンでは、`python` コマンドで Python 3.10 以上を起動できるようにしてください。PATH 上の別名を使う場合は、Visual Studio の publish で MSBuild プロパティ `PythonBuildCommand` を上書きしてください。

## 設定ファイル

- 共通設定: [src/Shos.MarkDownConverter.Web/appsettings.json](../src/Shos.MarkDownConverter.Web/appsettings.json)
- 開発環境設定: [src/Shos.MarkDownConverter.Web/appsettings.Development.json](../src/Shos.MarkDownConverter.Web/appsettings.Development.json)

開発環境では、`MarkItDown:PythonExecutablePath` に `.venv\Scripts\python.exe` の相対パスを設定しています。

`MarkItDown:MaxUploadSizeBytes` の既定値も、共通設定ファイルの [src/Shos.MarkDownConverter.Web/appsettings.json](../src/Shos.MarkDownConverter.Web/appsettings.json) を正本として読み取ります。

`PythonExecutablePath` を変更した場合は、実際にそのパスで Python を起動できるか確認してください。コマンド名を設定している場合は PATH から解決される必要があり、相対パスを設定している場合は Web プロジェクトのルート基準で解決されます。

## 起動まわりの構成

- 起動エントリーポイントは [src/Shos.MarkDownConverter.Web/Program.cs](../src/Shos.MarkDownConverter.Web/Program.cs) です。
- サービス登録と設定補完は [src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterServiceCollectionExtensions.cs](../src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterServiceCollectionExtensions.cs) で行います。
- API ルーティングと例外処理は [src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterWebApplicationExtensions.cs](../src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterWebApplicationExtensions.cs) にまとめています。

## 起動確認

```powershell
dotnet run --project src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj
```

起動後に表示された URL をブラウザーで開き、トップ画面が表示されることを確認します。

## MarkItDown の確認

```powershell
.\.venv\Scripts\python.exe --version
.\.venv\Scripts\python.exe -m pip show markitdown
```

パッケージ情報が表示されれば導入済みです。

## 発行確認

```powershell
dotnet publish src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj -c Release
```

publish 後は、出力ディレクトリ配下に `.python-runtime\Scripts\python.exe` が含まれていることを確認してください。
