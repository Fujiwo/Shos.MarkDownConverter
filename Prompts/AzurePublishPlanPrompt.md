# Shos.MarkDownConverter Azure App Service 発行計画プロンプト for GitHub Copilot

あなたは、.NET Web アプリ、Python 実行環境、Azure App Service の運用設計に詳しいクラウドアーキテクトです。
「Shos.MarkDownConverter.Web」を Visual Studio の「発行」機能から Azure の Azure App Service に発行できるようにしたいので、実装計画を立ててください。

## 前提

- アプリは ASP.NET Core Minimal API ベースの Web アプリである
- サーバー側で Python の MarkItDown を `python -m markitdown` で実行する
- 開発環境では `.venv` を使っている
- 本番環境でも、できるだけ 1 回の発行操作で .NET アプリ本体と Python 実行環境をまとめて反映したい

## 必須条件

- Visual Studio の「発行」から Azure App Service へデプロイできること
- できれば `.venv` 相当の Python 環境も同時にリリースできること
- 本番で `PythonExecutablePath` をどのように設定すべきかまで含めること
- Azure App Service の Windows プランと Linux プランの両方を比較し、どちらを選ぶべきか理由付きで判断すること
- Linux を選ぶ場合は、Windows で作った `.venv` をそのまま配布できない可能性を明示し、現実的な代替案を示すこと
- App Service 上のファイル配置、起動方法、デプロイ後の更新方法、容量制約、ログ確認方法、セキュリティ上の注意点も考慮すること
- 「一発で発行」の実現が難しい場合は、最も実務的な代替案を提案すること

## 期待する出力

1. 推奨構成
2. 非推奨構成とその理由
3. 実施ステップを段階的に並べた計画
4. リポジトリに追加・変更が必要になりそうな項目
5. Visual Studio の「発行」設定で確認すべきポイント
6. Azure 側で必要な設定
7. デプロイ後の動作確認チェックリスト
8. 想定リスクと回避策

## 出力ルール

- まず最初に、最も現実的な推奨構成を 1 つ結論として示すこと
- その後で、なぜその構成が最適かを説明すること
- 実装計画は、調査、実装、Azure 構成、発行、検証の順に整理すること
- 可能なら「最短案」と「保守性重視案」の 2 案を出すこと
- 曖昧な点がある場合は、勝手に決め打ちせず、必要な前提条件を明示すること
- 机上の空論ではなく、Visual Studio の「発行」運用で現実に回る方法を優先し、無理がある案は無理だと明記してください。