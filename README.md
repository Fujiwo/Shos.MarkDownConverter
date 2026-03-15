# Shos.MarkDownConverter

Shos.MarkDownConverter は、ブラウザーから単一ファイルをアップロードし、サーバー側で Python 版 MarkItDown を実行して Markdown を返す ASP.NET Core Web アプリケーションです。

## このアプリの全体像

Shos.MarkDownConverter は、ブラウザーから 1 つのファイルを受け取り、サーバー側で MarkItDown を実行し、その結果を Markdown として画面表示、コピー、ダウンロードできるようにするアプリケーションです。C# 側は文書変換のルールを再実装せず、入力検証、設定解決、外部プロセス実行、エラー整形、Web API 応答の組み立てを担当します。

アプリケーションの起動入口は [src/Shos.MarkDownConverter.Web/Program.cs](src/Shos.MarkDownConverter.Web/Program.cs) です。サービス登録と HTTP パイプライン構成は [src/Shos.MarkDownConverter.Web/Extensions](src/Shos.MarkDownConverter.Web/Extensions) に切り出し、設定値の補完と正規化は [src/Shos.MarkDownConverter.Web/Options](src/Shos.MarkDownConverter.Web/Options) にまとめています。入力検証、MarkItDown の呼び出し、エラー生成、一時ディレクトリ管理などの業務ロジックは [src/Shos.MarkDownConverter.Web/Services](src/Shos.MarkDownConverter.Web/Services) が担います。

フロントエンドは静的な HTML、CSS、JavaScript で構成されており、[src/Shos.MarkDownConverter.Web/wwwroot](src/Shos.MarkDownConverter.Web/wwwroot) 配下で API 通信、DOM 参照、表示更新を役割ごとに分けています。サーバー側とフロントエンド側の境界を保つことで、変換処理の責務と UI の責務を分離しやすくしています。

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

## Azure App Service への発行

Windows の Azure App Service へ Visual Studio の「発行」からデプロイする想定で、publish 時に配布専用の `.python-runtime` を自動生成して発行物へ同梱する設定を入れています。

- Python 依存は [requirements.publish.txt](requirements.publish.txt) を正本にします
- publish 前処理は [scripts/Prepare-PythonRuntime.ps1](scripts/Prepare-PythonRuntime.ps1) で行います
- 配布用ランタイムの staging 先は Web プロジェクト配下の `obj/.../python-runtime` です
- 本番既定値は [src/Shos.MarkDownConverter.Web/appsettings.Production.json](src/Shos.MarkDownConverter.Web/appsettings.Production.json) で `.python-runtime\Scripts\python.exe` を参照します

Visual Studio からの発行前に、発行元の Windows 環境で `python` コマンドが利用できる状態にしてください。別の Python 実行ファイルを使いたい場合は、publish 時に `PythonBuildCommand` MSBuild プロパティで上書きできます。

### Python ランタイム運用方針

- 開発時の Python はリポジトリ直下の `.venv` を使います
- publish 時の staging は Web プロジェクト配下の `obj/python-runtime` にだけ作ります
- publish 出力へコピーされる最終配置名は `.python-runtime` です
- `src/Shos.MarkDownConverter.Web/python-runtime` と `src/Shos.MarkDownConverter.Web/.python-runtime` は、通常運用では常駐させません

過去の試行で [src/Shos.MarkDownConverter.Web](src/Shos.MarkDownConverter.Web) 配下に `python-runtime` または `.python-runtime` が残っている場合は、不要なローカル生成物です。再発行の前に削除してください。まとめて掃除する場合は [scripts/Clean-PythonRuntime.ps1](scripts/Clean-PythonRuntime.ps1) を使います。

```powershell
.\scripts\Clean-PythonRuntime.ps1
```

## src 配下の構成と責務

このリポジトリで実装を追うときは、まず [src/Shos.MarkDownConverter.Web](src/Shos.MarkDownConverter.Web) の責務分割を把握すると全体が見えやすくなります。

- [src/Shos.MarkDownConverter.Web/Program.cs](src/Shos.MarkDownConverter.Web/Program.cs)
	アプリケーションの起動入口です。サービス登録、例外処理、リクエストサイズ制御、静的ファイル配信、API エンドポイント登録、フォールバック設定を順番に組み立てます。
- [src/Shos.MarkDownConverter.Web/Extensions](src/Shos.MarkDownConverter.Web/Extensions)
	起動時の配線を拡張メソッドへ切り出した層です。[src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterServiceCollectionExtensions.cs](src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterServiceCollectionExtensions.cs) では設定読込とサービス登録を行い、[src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterWebApplicationExtensions.cs](src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterWebApplicationExtensions.cs) では例外処理、サイズ制限、API エンドポイント定義を行います。
- [src/Shos.MarkDownConverter.Web/Options](src/Shos.MarkDownConverter.Web/Options)
	設定値の保持だけでなく、既定値補完、相対パス解決、アップロード制限値の計算を担当します。[src/Shos.MarkDownConverter.Web/Options/MarkItDownOptions.cs](src/Shos.MarkDownConverter.Web/Options/MarkItDownOptions.cs) が中核で、設定値を実行時に使いやすい形へ正規化します。
- [src/Shos.MarkDownConverter.Web/Services](src/Shos.MarkDownConverter.Web/Services)
	業務ロジックの中心です。[src/Shos.MarkDownConverter.Web/Services/UploadValidationService.cs](src/Shos.MarkDownConverter.Web/Services/UploadValidationService.cs) が入力検証を行い、[src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs](src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs) が変換処理全体を統括し、[src/Shos.MarkDownConverter.Web/Services/ExternalProcessRunner.cs](src/Shos.MarkDownConverter.Web/Services/ExternalProcessRunner.cs) が Python プロセスを起動します。[src/Shos.MarkDownConverter.Web/Services/MarkItDownErrorFormatter.cs](src/Shos.MarkDownConverter.Web/Services/MarkItDownErrorFormatter.cs) は外部プロセスの失敗をユーザー向けエラーへ変換し、[src/Shos.MarkDownConverter.Web/Services/ConversionWorkspace.cs](src/Shos.MarkDownConverter.Web/Services/ConversionWorkspace.cs) は一時ディレクトリを管理します。
- [src/Shos.MarkDownConverter.Web/Models](src/Shos.MarkDownConverter.Web/Models)
	API の応答モデルをまとめた層です。成功時は [src/Shos.MarkDownConverter.Web/Models/ConvertResponse.cs](src/Shos.MarkDownConverter.Web/Models/ConvertResponse.cs)、失敗時は [src/Shos.MarkDownConverter.Web/Models/ErrorResponse.cs](src/Shos.MarkDownConverter.Web/Models/ErrorResponse.cs)、画面初期表示用の設定取得には [src/Shos.MarkDownConverter.Web/Models/UiOptionsResponse.cs](src/Shos.MarkDownConverter.Web/Models/UiOptionsResponse.cs) を使います。
- [src/Shos.MarkDownConverter.Web/wwwroot](src/Shos.MarkDownConverter.Web/wwwroot)
	静的フロントエンドの実装です。[src/Shos.MarkDownConverter.Web/wwwroot/app.js](src/Shos.MarkDownConverter.Web/wwwroot/app.js) が画面全体の入口で、[src/Shos.MarkDownConverter.Web/wwwroot/app-api.js](src/Shos.MarkDownConverter.Web/wwwroot/app-api.js) が API 通信、[src/Shos.MarkDownConverter.Web/wwwroot/app-dom.js](src/Shos.MarkDownConverter.Web/wwwroot/app-dom.js) が DOM 参照、[src/Shos.MarkDownConverter.Web/wwwroot/app-ui.js](src/Shos.MarkDownConverter.Web/wwwroot/app-ui.js) が表示更新を担当します。

実装を読み始める順番としては、[src/Shos.MarkDownConverter.Web/Program.cs](src/Shos.MarkDownConverter.Web/Program.cs) から [src/Shos.MarkDownConverter.Web/Extensions](src/Shos.MarkDownConverter.Web/Extensions) を確認し、その後に [src/Shos.MarkDownConverter.Web/Options](src/Shos.MarkDownConverter.Web/Options)、[src/Shos.MarkDownConverter.Web/Services](src/Shos.MarkDownConverter.Web/Services)、[src/Shos.MarkDownConverter.Web/wwwroot](src/Shos.MarkDownConverter.Web/wwwroot)、最後に tests 配下を見ると全体像を追いやすくなります。

## リクエスト処理フロー

このアプリの変換処理は、画面初期化から結果表示までを次の流れで進めます。

1. 画面表示時に [src/Shos.MarkDownConverter.Web/wwwroot/app.js](src/Shos.MarkDownConverter.Web/wwwroot/app.js) が初期化処理を行い、[src/Shos.MarkDownConverter.Web/wwwroot/app-api.js](src/Shos.MarkDownConverter.Web/wwwroot/app-api.js) 経由で /api/options を呼び出します。
2. サーバー側では [src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterWebApplicationExtensions.cs](src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterWebApplicationExtensions.cs) が /api/options を提供し、対応拡張子とアップロード上限を返します。
3. ユーザーがファイルを選択して変換を実行すると、フロントエンドは multipart/form-data で /api/convert へ送信します。
4. サーバー側では、まずリクエストサイズの上限を確認し、その後 [src/Shos.MarkDownConverter.Web/Services/UploadValidationService.cs](src/Shos.MarkDownConverter.Web/Services/UploadValidationService.cs) がファイルの有無、空ファイル、サイズ超過、許可拡張子を検証します。
5. 検証に成功すると、[src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs](src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs) が一時ディレクトリを作成し、アップロードファイルを保存し、Python で MarkItDown を実行します。
6. 外部プロセスの終了後、変換結果の Markdown ファイルが生成されていればその内容を読み込み、ダウンロード用ファイル名と一緒に返します。
7. 失敗した場合は、入力検証エラー、サイズ超過、Python 起動失敗、MarkItDown 実行失敗、想定外例外のいずれかとして整形し、画面で扱いやすい JSON を返します。
8. フロントエンドは応答内容を受け取り、成功時は結果表示欄へ Markdown を出し、失敗時はエラーパネルへ要約、原因候補、対処方法を表示します。

## 設定の読み方と解決ルール

MarkItDown 呼び出しに関する主要設定は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) と環境別の appsettings で管理します。設定の読み込みは単純な値取得ではなく、起動時に正規化と補完を行ってから各サービスで利用します。

サービス登録時には [src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterServiceCollectionExtensions.cs](src/Shos.MarkDownConverter.Web/Extensions/MarkDownConverterServiceCollectionExtensions.cs) が MarkItDown セクションを取得し、最大アップロードサイズを読み取った上で multipart リクエスト用の制限値を計算します。その後、[src/Shos.MarkDownConverter.Web/Options/MarkItDownOptions.cs](src/Shos.MarkDownConverter.Web/Options/MarkItDownOptions.cs) の Normalize により、PythonExecutablePath、ModuleName、AllowedExtensions、WorkingDirectoryRoot などを実行時向けの値へ補正します。

- `PythonExecutablePath`: Python 実行ファイルのパス。`python` のようなコマンド名も指定できます。相対パスを指定した場合は、必要に応じて Web プロジェクトのルート基準で絶対パスへ解決します。開発環境では `.venv\Scripts\python.exe` を使う想定です。
- `ModuleName`: 実行する Python モジュール名。既定値は `markitdown` です。
- `MaxUploadSizeBytes`: ファイルサイズ上限。既定値の正本は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) にあり、この値をもとに multipart リクエスト全体の上限も計算します。
- `AllowedExtensions`: 受け付ける拡張子一覧。未指定時は既定値が補われ、先頭のドット、大小文字、重複を吸収した上で整列されます。
- `WorkingDirectoryRoot`: 一時作業ディレクトリの親を明示したい場合に使います。未指定時はシステムの一時ディレクトリを利用します。

`PythonExecutablePath` に `python` のようなコマンド名を指定した場合は、そのまま PATH 解決に委ねます。`.\tools\python.exe` や `..\..\.venv\Scripts\python.exe` のような相対パスを指定した場合だけ、Web プロジェクトのルートを基準に絶対パスへ解決します。共通設定は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) に置き、開発環境固有の Python パスは [src/Shos.MarkDownConverter.Web/appsettings.Development.json](src/Shos.MarkDownConverter.Web/appsettings.Development.json) で上書きする運用です。環境ごとに変更する場合は、ユーザーシークレットまたは環境変数を優先してください。

Production では [src/Shos.MarkDownConverter.Web/appsettings.Production.json](src/Shos.MarkDownConverter.Web/appsettings.Production.json) により、発行物に同梱された `.python-runtime\Scripts\python.exe` を既定で参照します。Azure App Service では必要に応じて `MarkItDown__PythonExecutablePath` を App Settings から上書きしてください。

## MarkItDown 連携の仕組み

このアプリは文書変換ロジックを C# 側で持たず、MarkItDown の CLI を安全に実行するラッパーとして振る舞います。変換処理の入口は [src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs](src/Shos.MarkDownConverter.Web/Services/MarkItDownConversionService.cs) です。

変換時にはまず [src/Shos.MarkDownConverter.Web/Services/ConversionWorkspace.cs](src/Shos.MarkDownConverter.Web/Services/ConversionWorkspace.cs) が変換ごとに一意な一時ディレクトリを作成します。これにより、同時に複数の変換要求が来ても入力ファイルや出力ファイルが衝突しにくくなります。次に、アップロードされたファイルをそのディレクトリへ保存し、出力先として output.md を用意します。

外部プロセスの起動は [src/Shos.MarkDownConverter.Web/Services/ExternalProcessRunner.cs](src/Shos.MarkDownConverter.Web/Services/ExternalProcessRunner.cs) が担当します。引数は文字列連結ではなく ProcessStartInfo.ArgumentList で渡しているため、ファイルパスや引数に空白が含まれても安全に扱いやすくなっています。標準出力と標準エラーは別々に回収し、失敗時の診断材料として使います。

Python プロセスが正常終了し、出力ファイルが存在すれば、Markdown 本文を読み込んで API 応答へ載せます。プロセスが失敗した場合や出力ファイルが生成されなかった場合は、ユーザーに分かりやすい構造化エラーへ変換します。変換途中で要求がキャンセルされた場合は、プロセスツリーごと停止し、孤立プロセスが残らないようにしています。

## エラー処理とユーザー向け応答

このアプリでは、単に失敗を返すのではなく、できるだけ画面でそのまま表示できる形に整えたエラー応答を返すようにしています。成功時は [src/Shos.MarkDownConverter.Web/Models/ConvertResponse.cs](src/Shos.MarkDownConverter.Web/Models/ConvertResponse.cs)、失敗時は [src/Shos.MarkDownConverter.Web/Models/ErrorResponse.cs](src/Shos.MarkDownConverter.Web/Models/ErrorResponse.cs) を返します。

入力検証の失敗は [src/Shos.MarkDownConverter.Web/Services/UploadValidationService.cs](src/Shos.MarkDownConverter.Web/Services/UploadValidationService.cs) が担当します。ファイル未選択、空ファイル、サイズ超過、未対応拡張子を順に判定し、それぞれに応じたメッセージ、原因候補、対処方法を返します。

サイズ超過については、複数の段階で制御しています。リクエスト受信前の Content-Length チェック、ASP.NET Core の multipart 制限、変換前の file.Length 確認のいずれかで検出される可能性があります。[src/Shos.MarkDownConverter.Web/Services/PayloadTooLargeDetector.cs](src/Shos.MarkDownConverter.Web/Services/PayloadTooLargeDetector.cs) は、実行環境や例外の出方が異なる場合でも 413 相当として扱えるようにしています。

MarkItDown や Python の起動失敗は [src/Shos.MarkDownConverter.Web/Services/MarkItDownErrorFormatter.cs](src/Shos.MarkDownConverter.Web/Services/MarkItDownErrorFormatter.cs) が担当します。標準エラー出力の内容から、Python が見つからないのか、markitdown パッケージが見つからないのか、あるいはファイル変換そのものに失敗したのかを分類し、利用者向けの文面へ変換します。

想定外の例外については、内部例外の詳細をそのまま画面へ出さず、ProblemDetails 形式の 500 応答を返します。フロントエンド側では [src/Shos.MarkDownConverter.Web/wwwroot/app-ui.js](src/Shos.MarkDownConverter.Web/wwwroot/app-ui.js) が message、possibleCauses、actions を分けて描画し、空のセクションは表示しません。

## フロントエンド構成

フロントエンドの JavaScript は、単一ファイルにすべてを詰め込まず、役割ごとにモジュールを分けています。入口になるのは [src/Shos.MarkDownConverter.Web/wwwroot/app.js](src/Shos.MarkDownConverter.Web/wwwroot/app.js) です。

[src/Shos.MarkDownConverter.Web/wwwroot/app.js](src/Shos.MarkDownConverter.Web/wwwroot/app.js) は画面初期化、イベント登録、変換要求の送信、コピーやダウンロードなど、画面全体の流れを調停します。画面表示時には options を取得し、フォーム送信時には convert API を呼び出して結果またはエラーを反映します。

[src/Shos.MarkDownConverter.Web/wwwroot/app-api.js](src/Shos.MarkDownConverter.Web/wwwroot/app-api.js) は通信専用の層です。/api/options と /api/convert の呼び出しをまとめ、成功時も失敗時も応答本文を読み取って上位へ返します。これにより、UI 層は fetch の細部を意識せずに扱えます。

[src/Shos.MarkDownConverter.Web/wwwroot/app-dom.js](src/Shos.MarkDownConverter.Web/wwwroot/app-dom.js) は DOM 要素取得を 1 か所へ集約しています。フォーム、ファイル入力、ステータス表示、エラーパネル、結果表示欄などをまとめて取得し、各モジュールから同じ要素参照を使えるようにしています。

[src/Shos.MarkDownConverter.Web/wwwroot/app-ui.js](src/Shos.MarkDownConverter.Web/wwwroot/app-ui.js) は表示更新専用の層です。結果表示、状態表示、エラー表示、対応拡張子一覧の表示、サイズ表記、コピー失敗時のメッセージ構築などを担当します。API 通信と表示更新を分けることで、どの処理が画面ロジックで、どの処理が通信ロジックかを追いやすくしています。

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

サーバーが ProblemDetails を返した場合は、その内容を優先して表示します。原因候補や対処方法が付かないエラーでは、空のセクションは表示しません。

## 対応ファイル形式の考え方

アプリケーションは MarkItDown の CLI をそのまま利用し、変換ルールを C# 側で再実装しません。既定の拡張子一覧は、日常的に扱うことが多い文書、表計算、テキスト、基本画像形式に絞っています。

- 文書: `.pdf`, `.docx`, `.pptx`, `.xlsx`
- テキスト系: `.txt`, `.md`, `.csv`, `.json`, `.xml`, `.html`, `.htm`
- 画像: `.jpg`, `.jpeg`, `.png`

より広い形式を扱いたい場合は [src/Shos.MarkDownConverter.Web/appsettings.json](src/Shos.MarkDownConverter.Web/appsettings.json) の `AllowedExtensions` に追加してください。実際に変換できるかどうかは、インストールした MarkItDown のバージョンと追加依存関係に依存します。

## テストの見取り図

```powershell
dotnet test Shos.MarkDownConverter.slnx
```

このリポジトリでは、単体テスト、結合テスト、E2E テストを分けて、入力検証、設定解決、外部プロセス実行、API 応答、UI 表示までを段階的に確認しています。

単体テストでは Services や Options のロジックを個別に検証します。たとえば [tests/Shos.MarkDownConverter.Web.Tests/MarkItDownConversionServiceTests.cs](tests/Shos.MarkDownConverter.Web.Tests/MarkItDownConversionServiceTests.cs) では変換処理の正常系と異常系を、[tests/Shos.MarkDownConverter.Web.Tests/MarkItDownErrorFormatterTests.cs](tests/Shos.MarkDownConverter.Web.Tests/MarkItDownErrorFormatterTests.cs) では標準エラー出力からの分類を、[tests/Shos.MarkDownConverter.Web.Tests/UploadValidationServiceTests.cs](tests/Shos.MarkDownConverter.Web.Tests/UploadValidationServiceTests.cs) では入力検証ルールを確認しています。

結合テストでは、API エンドポイント全体の応答を確認します。[tests/Shos.MarkDownConverter.Web.IntegrationTests/ConvertEndpointTests.cs](tests/Shos.MarkDownConverter.Web.IntegrationTests/ConvertEndpointTests.cs) では、変換成功、変換失敗、Python 起動失敗、サイズ超過、未処理例外時の応答形式とステータスコードを検証しています。

E2E テストでは、ブラウザー操作を含めた一連の利用フローを確認します。[tests/Shos.MarkDownConverter.Web.E2ETests/UiWorkflowTests.cs](tests/Shos.MarkDownConverter.Web.E2ETests/UiWorkflowTests.cs) では、変換成功、ダウンロード、コピー、未対応拡張子、サイズ超過、Python 起動失敗時の画面表示を検証しています。詳細は [Documents/TestGuide.md](Documents/TestGuide.md) を参照してください。

## 詳細ドキュメント

- セットアップ: [Documents/SetupGuide.md](Documents/SetupGuide.md)
- ユーザーマニュアル: [Documents/UserManual.md](Documents/UserManual.md)
- トラブルシューティング: [Documents/Troubleshooting.md](Documents/Troubleshooting.md)
- テストガイド: [Documents/TestGuide.md](Documents/TestGuide.md)

## トラブルシューティング

### Python が見つからない

- `MarkItDown:PythonExecutablePath` が正しいか確認してください。
- `PythonExecutablePath` がコマンド名なら PATH から解決できるか、相対パスなら Web プロジェクト基準で正しい場所を指すか確認してください。
- `.\.venv\Scripts\python.exe --version` が通るか確認してください。
- 画面上の「考えられる原因」と「対処方法」に従って設定値を見直してください。

### ファイルサイズが上限を超える

- 画面に表示されるサイズ上限を確認してください。
- サイズ超過時は API が 413 と構造化エラーを返し、UI に上限超過メッセージを表示します。
- 必要なら `MaxUploadSizeBytes` を見直してください。

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
- 実際に受け付ける形式は `AllowedExtensions` の設定と Python 環境側の依存関係の両方に影響されます。
- 本アプリは単一ファイル変換専用です。
- 変換結果はブラウザー上に保持し、サーバー側へ永続保存しません。
- YouTube URL などファイルアップロード以外の入力は対象外です。
