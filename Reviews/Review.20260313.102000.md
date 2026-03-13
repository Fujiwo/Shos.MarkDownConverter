**指摘事項**
1. 既定の PythonExecutablePath が非開発環境で実質壊れています。概要: MarkItDownOptions.cs は既定値を python にしている一方で、非絶対パスを無条件に content root 基準の絶対パスへ変換しており、結果として PATH 上の python ではなく projectRoot/python を起動しようとします。なぜ問題か: 既定設定のままでは Development の上書きが効かない環境で Python 起動に失敗し、MarkItDown 連携が成立しません。影響範囲: 本番相当環境、appsettings.Development.json を使わないローカル実行、README の設定説明。修正の方向性: bare command 名はそのまま残し、ディレクトリ区切りを含む値だけ相対パス解決する形に分岐してください。該当箇所: MarkItDownOptions.cs, appsettings.json, README.md

2. サイズ超過時のエラー契約が実装と一致していません。概要: Program.cs で MultipartBodyLengthLimit を先に適用しているため、巨大ファイルは Program.cs の IFormFile バインド前にフレームワーク側で拒否されます。それにもかかわらず、UploadValidationService.cs と UploadValidationServiceTests.cs は file-too-large の構造化エラーが返る前提になっています。なぜ問題か: 実際の UI は app.js と app.js の汎用フォールバックに落ち、ユーザーにはサイズ超過だと分からない可能性が高いです。影響範囲: 主要要件である入力検証、ユーザー向けエラー表示、テストの信頼性。修正の方向性: oversized multipart を 413 か明示的な JSON エラーへマップし、統合テストと E2E で実パスを固定してください。該当箇所: Program.cs, UploadValidationService.cs, app.js

3. キャンセル時に Python 子プロセスを取りこぼします。概要: ExternalProcessRunner.cs は WaitForExitAsync に CancellationToken を渡すだけで、キャンセル発生時にプロセスツリーを終了していません。なぜ問題か: クライアント切断やタイムアウト時に Python が残り続け、MarkItDownConversionService.cs の一時ディレクトリ削除とも競合します。影響範囲: 長時間変換、複数同時実行、サーバー資源の回収、ログノイズ。修正の方向性: OperationCanceledException を捕捉して Kill(entireProcessTree: true) 後に終了待ちし、そのうえで再送出するべきです。該当箇所: ExternalProcessRunner.cs, MarkItDownConversionService.cs

4. 500 系の UI 文言がサーバーの実際の失敗理由を捨て、原因を誤誘導します。概要: サーバーは Program.cs で ProblemDetails を返していますが、クライアントは app.js でそれを読まず、MarkItDown 実行中の障害を示唆する固定文言に置き換えています。なぜ問題か: 実際には権限、テンポラリ書き込み、シリアライズなど別原因でも、ユーザーと保守担当の両方を MarkItDown 側へ誤誘導します。影響範囲: 例外時の調査、運用時の一次切り分け、ドキュメントの信頼性。修正の方向性: 構造化 ErrorResponse がない場合でも ProblemDetails の title/detail を優先し、原因候補はサーバーが明示したときだけ具体化してください。該当箇所: Program.cs, app.js

5. テストは重要な実経路を十分に固定できていません。概要: 実サービスは起動失敗時に MarkItDownConversionService.cs から FormatLaunchFailure へ流れ、MarkItDownErrorFormatter.cs の python-launch-failed が現実的な契約ですが、統合テストは ConvertEndpointTests.cs で人工的な python-missing を固定しています。加えて、サイズ超過は単体テスト止まりで、コピー機能は README では要件化されているのに UiWorkflowTests.cs では未検証です。なぜ問題か: 失敗系の API 契約と UI 主要機能の回帰を、今のテスト群では守れません。影響範囲: Python 不在時のエラー契約、サイズ超過時の UI、コピー機能。修正の方向性: 現実の実装に合わせて統合テストを合わせるか契約を一本化し、サイズ超過とコピー操作を統合または E2E で追加してください。該当箇所: ConvertEndpointTests.cs, UiWorkflowTests.cs, README.md

**未確認事項・前提**
ソリューション全体の dotnet test は、この環境では既に動作中の dotnet host による Web DLL ロックのため最後までクリーンには確認できませんでした。静的レビューと個別テスト結果から上記 5 点は十分に根拠がありますが、サイズ超過の実 HTTP 再現だけはその環境要因のためコード読解中心で判断しています。

**総評**
実装の骨格自体は小さく整理されていますが、Python 実行パスの既定値、サイズ超過時の失敗経路、キャンセル時のプロセス回収という運用上の根幹に欠陥があります。優先して直すべき上位 3 件は、1. PythonExecutablePath の正規化バグ、2. サイズ超過時の API/UI 契約崩れ、3. キャンセル時の子プロセス回収です。
