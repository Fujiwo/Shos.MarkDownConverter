# セットアップガイド

## 前提環境

- .NET SDK 10.0 以上
- Python 3.10 以上
- PowerShell

## 初回セットアップ

```powershell
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install "markitdown[all]"
dotnet restore Shos.MarkDownConverter.slnx
```

## 開発環境の Python 設定

開発環境では [src/Shos.MarkDownConverter.Web/appsettings.Development.json](../src/Shos.MarkDownConverter.Web/appsettings.Development.json) で `.venv\Scripts\python.exe` を参照します。

`PythonExecutablePath` を変更した場合は、実際にそのパスで Python を起動できるか確認してください。

```powershell
.\.venv\Scripts\python.exe --version
.\.venv\Scripts\python.exe -m pip show markitdown
```

## 起動確認

```powershell
dotnet run --project src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj
```

ブラウザーでトップページを開き、対応拡張子一覧が表示されることを確認してください。
