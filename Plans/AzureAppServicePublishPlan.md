# Shos.MarkDownConverter Azure App Service 発行計画

## 結論

最も現実的な推奨構成は、Windows の Azure App Service を使い、Visual Studio の「発行」前処理で配布専用の Windows 向け Python 仮想環境を生成し、それを Web アプリと一緒に発行する構成です。

理由は単純です。このリポジトリは現在、Windows 開発環境の `.venv` を前提にしており、[src/Shos.MarkDownConverter.Web/appsettings.Development.json](../src/Shos.MarkDownConverter.Web/appsettings.Development.json) でも Windows パスの Python を参照しています。さらに、[src/Shos.MarkDownConverter.Web/Options/MarkItDownOptions.cs](../src/Shos.MarkDownConverter.Web/Options/MarkItDownOptions.cs) は相対パスの `PythonExecutablePath` をアプリのコンテンツルート基準で解決できるため、発行物の中に Windows 用 Python 環境を同梱すれば、そのまま本番でも扱いやすい構成にできます。

Linux App Service は「Visual Studio の発行から一発で、今の `.venv` 相当環境も一緒に出したい」という条件と噛み合いません。Windows で作成した `.venv` は Linux 上でそのまま実行できず、Linux 向けには別途 Linux 環境で仮想環境を作り直すか、コンテナー化に寄せる必要があります。そのため、本件では Windows App Service を第一候補とします。

## 推奨構成

### 推奨案

- Azure App Service は Windows を採用する
- Visual Studio の「発行」で Azure App Service へデプロイする
- 発行対象には .NET アプリ本体に加えて、配布専用の Windows 向け Python 仮想環境を含める
- 本番の `MarkItDown__PythonExecutablePath` は `.python-runtime\\Scripts\\python.exe` のようなアプリ配下の相対パスで指定する
- Python 実行環境は開発用 `.venv` をそのまま流用せず、発行前にスクリプトで生成する

### この構成が最適な理由

- 現在のアプリは外部プロセスで `python -m markitdown` を起動する設計であり、Python 実行ファイルの配置さえ安定すれば、本番移行がしやすい
- [src/Shos.MarkDownConverter.Web/Services/ExternalProcessRunner.cs](../src/Shos.MarkDownConverter.Web/Services/ExternalProcessRunner.cs) は `ProcessStartInfo.ArgumentList` を使っており、実行ファイルパスと引数を安全に分離できている
- [src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs](../src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs) は `PythonExecutablePath` と `ModuleName` の差し替えで起動先を切り替えられる
- Visual Studio 発行運用を維持したまま、一発発行に近づけるには、発行プロセスの中で Python 環境を準備する方法が最も現実的

## 非推奨構成

### 非推奨 1. Linux App Service に Windows の `.venv` を同梱して配る

非推奨です。Windows で作られた `.venv` は Linux の実行形式とディレクトリ構造に一致しないため、そのまま動きません。

### 非推奨 2. App Service 上のグローバル Python へ依存する

非推奨です。App Service 側に期待した Python と MarkItDown が常に入っている保証がなく、再現性が低くなります。環境差分による障害が起きやすく、トラブル時の切り分けも難しくなります。

### 非推奨 3. 開発者ローカルの `.venv` をそのまま恒久運用する

短期的には可能ですが、長期運用には向きません。不要パッケージの混入、サイズ肥大化、ローカル依存の残存が起きやすく、再現可能な発行物になりにくいためです。

## 最短案と保守性重視案

### 最短案

- Windows App Service を使う
- 既存の `.venv` を発行物へ含める
- `MarkItDown__PythonExecutablePath` を `.venv\\Scripts\\python.exe` に設定する

利点は実装が最速なことです。欠点は、発行物が開発者マシンの `.venv` 状態に依存し、再現性が低いことです。

### 保守性重視案

- Windows App Service を使う
- `requirements.txt` かロック済み依存一覧を追加する
- Visual Studio 発行前に PowerShell スクリプトまたは MSBuild Target で `.python-runtime` を毎回作り直す
- 生成した `.python-runtime` を発行対象に含める
- `MarkItDown__PythonExecutablePath` を `.python-runtime\\Scripts\\python.exe` に設定する

この案を推奨します。理由は、Visual Studio の「発行」操作を維持しながら、配布物を再現可能にできるからです。

## 実施ステップ

以下は、調査、実装、Azure 構成、発行、検証の順に並べた実施計画です。

### 1. 調査

1. Azure App Service の候補を Windows で確定する
2. `.venv` の現在サイズと、MarkItDown および依存パッケージの実容量を確認する
3. MarkItDown が依存する追加パッケージを洗い出し、配布用依存一覧を固定化する
4. Azure App Service 上で利用する .NET ランタイムが対象のフレームワークに対応しているか確認する
5. 対応していない場合の回避策として、自己完結発行またはターゲットフレームワーク調整の要否を確認する

### 2. 実装

1. リポジトリに Python 依存一覧を追加する
2. 配布専用 Python 環境を作るスクリプトを追加する
3. Web プロジェクトの publish 処理に、配布専用 Python 環境の生成と同梱を組み込む
4. 本番設定で `MarkItDown__PythonExecutablePath` をアプリ配下の相対パスで参照する方針を確定する
5. 必要なら `MarkItDown__WorkingDirectoryRoot` を App Service の一時領域へ明示設定する
6. README とセットアップ資料に、発行用 Python 環境の生成方針を追記する

### 3. Azure 構成

1. Windows の Azure App Service を作成する
2. 64-bit を有効にする
3. 本番運用を見据えるなら Always On を有効にする
4. アプリケーション ログと詳細エラーログを有効にする
5. 必要な App Settings を登録する
6. 機密情報が増える場合は App Settings 側に寄せ、構成ファイルへ固定値を書き込まない

### 4. 発行

1. Visual Studio で Azure App Service 向け publish profile を作成する
2. 発行前処理で配布専用 Python 環境が生成されることを確認する
3. 発行パッケージに `.python-runtime` が含まれることを確認する
4. Azure App Service へ発行する
5. 発行直後に Kudu または App Service のファイルビューで Python 実行ファイルが配置されていることを確認する

### 5. 検証

1. `/api/options` が正常に応答することを確認する
2. 実際のファイルアップロードで Markdown 変換が通ることを確認する
3. Python 起動失敗時のエラー表示が期待どおりか確認する
4. サイズ超過と非対応拡張子の失敗ケースを確認する
5. App Service ログに終了コードと標準エラーが記録されることを確認する

## リポジトリに追加・変更が必要になりそうな項目

### 追加候補

- 依存一覧ファイル
  - 例: `requirements.txt`
- 発行用 Python 環境生成スクリプト
  - 例: `scripts/Prepare-PythonRuntime.ps1`
- 必要なら本番向け設定ファイル
  - 例: `src/Shos.MarkDownConverter.Web/appsettings.Production.json`

### 変更候補

- [src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj](../src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj)
  - 発行前ターゲット追加
  - 配布用 Python 環境フォルダーの publish 同梱設定追加
- [README.md](../README.md)
  - Azure 発行手順追記
- [Documents/SetupGuide.md](../Documents/SetupGuide.md)
  - 発行用 Python 環境の生成と前提条件追記
- [Documents/Troubleshooting.md](../Documents/Troubleshooting.md)
  - App Service での Python 起動失敗や配置漏れの確認手順追記

## Visual Studio の「発行」設定で確認すべきポイント

1. 発行先が Windows の Azure App Service であること
2. 構成が Release であること
3. 対象プロジェクトが [src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj](../src/Shos.MarkDownConverter.Web/Shos.MarkDownConverter.Web.csproj) であること
4. 発行前イベントまたは MSBuild Target が実行されること
5. 発行物に `.python-runtime` が含まれていること
6. 不要なローカル専用ファイルが混入していないこと
7. 必要なら自己完結発行の採否を明示すること

## Azure 側で必要な設定

### App Settings

- `ASPNETCORE_ENVIRONMENT=Production`
- `MarkItDown__PythonExecutablePath=.python-runtime\\Scripts\\python.exe`
- 必要なら `MarkItDown__WorkingDirectoryRoot=D:\\local\\Temp\\Shos.MarkDownConverter`

### 推奨設定

- 64-bit: 有効
- Always On: 有効
- Web サーバーログ: 有効
- 詳細エラーログ: 有効

### 補足

- `PythonExecutablePath` は絶対パスではなく、アプリ配下の相対パスを優先する
- `.python-runtime` は `wwwroot` ではなくアプリ配下へ配置し、ブラウザーから直接取得できない場所に置く
- 一時変換領域はアプリ本体配置先ではなく、一時領域を使う

## デプロイ後の動作確認チェックリスト

1. Azure App Service の起動に失敗していない
2. トップページが表示できる
3. [src/Shos.MarkDownConverter.Web/appsettings.json](../src/Shos.MarkDownConverter.Web/appsettings.json) の既定設定と Azure App Settings の上書きが意図どおり反映されている
4. `/api/options` が 200 を返す
5. `.txt` または `.md` のような単純な入力で変換が成功する
6. `.docx` や `.pdf` など実運用想定の形式でも変換できる
7. 変換失敗時にユーザー向けエラー表示が壊れていない
8. ログに Python 起動失敗や MarkItDown 標準エラーが残る
9. 一時ファイルが処理後に削除される

## 想定リスクと回避策

### 1. Python 環境のサイズが大きすぎる

リスクです。MarkItDown とオプション依存を含めると仮想環境が大きくなり、発行時間とストレージ使用量が増えます。

回避策は、配布専用環境を別生成にして不要依存を除くこと、必要形式を見直すこと、どうしても大きい場合はコンテナー化へ切り替えることです。

### 2. 開発用 `.venv` の再現性がない

リスクです。開発環境依存のままでは、別の開発者や将来の発行時に同じ成果物を再現しにくくなります。

回避策は、`requirements.txt` またはロックファイルを基準に、発行前に毎回 `.python-runtime` を作ることです。

### 3. Linux へ将来移行しづらい

リスクです。Windows 用仮想環境同梱方式は Linux 移行性が低いです。

回避策は、将来 Linux を狙うなら、この方式を暫定と割り切り、次段階でコンテナー化または CI で Linux 用ランタイムを作る計画を別途持つことです。

### 4. App Service のランタイム差異で起動しない

リスクです。対象 .NET ランタイムやネイティブ依存が App Service 側と噛み合わないと起動失敗します。

回避策は、事前にランタイム対応を確認すること、必要なら自己完結発行を採ること、そして発行後すぐに起動ログを確認することです。

## 一発発行の実現方針

本件でいう「一発で .venv の環境もリリースしたい」を現実的に満たすには、Visual Studio の publish 操作の中で次を自動実行する形が最適です。

1. 発行用 Python 環境を生成する
2. 生成結果を publish 対象に含める
3. App Service 側では相対パスの `PythonExecutablePath` で起動する

手動で `.venv` を作ってから別操作でコピーする運用は避けるべきです。Visual Studio の「発行」だけで完結する導線にまとめることで、運用ミスを減らせます。

## 机上案で終わらせないための最終判断

本件では、まず Windows App Service 前提で保守性重視案を採用し、どうしても初期実装コストを下げたい場合のみ最短案を暫定採用するのが妥当です。

机上の空論ではなく、Visual Studio の「発行」運用で現実に回る方法を優先し、無理がある案は無理だと明記してください。