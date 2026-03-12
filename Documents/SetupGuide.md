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

## 設定ファイル

- 共通設定: [src/Shos.MarkDownConverter.Web/appsettings.json](../src/Shos.MarkDownConverter.Web/appsettings.json)
- 開発環境設定: [src/Shos.MarkDownConverter.Web/appsettings.Development.json](../src/Shos.MarkDownConverter.Web/appsettings.Development.json)

開発環境では、`MarkItDown:PythonExecutablePath` に `.venv\Scripts\python.exe` の相対パスを設定しています。

## 起動確認

```powershell
dotnet run --project src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj
```

起動後に表示された URL をブラウザーで開き、トップ画面が表示されることを確認します。

## MarkItDown の確認

```powershell
.\.venv\Scripts\python.exe -m pip show markitdown
```

パッケージ情報が表示されれば導入済みです。
