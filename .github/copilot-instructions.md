# Shos.MarkDownConverter Copilot Instructions

## プロジェクトの目的

- ブラウザーから単一ファイルをアップロードし、Python 版 MarkItDown で Markdown に変換する。
- 変換結果の画面表示、コピー、ダウンロードを提供する。

## 採用技術

- .NET 10
- ASP.NET Core Minimal API + 静的 HTML/CSS/JavaScript
- Python 3.10 以上
- MarkItDown CLI を `python -m markitdown` で実行

## 実装方針

- 変換ロジックは MarkItDown に委譲し、C# 側で再実装しない。
- UI、入力検証、外部プロセス実行を小さく分離する。
- 依存関係は必要最小限に保つ。
- Agent Skills は必要なものだけを限定導入する。
- 現在は MarkItDown 連携の確認と実装補助に限って Skill を使う。
- Web アプリ本体の通常実装では、Skill 導入自体を目的にしない。

## テスト方針

- 単体テストで入力検証、対応形式判定、エラー整形、MarkItDown 呼び出しラッパーの正常系と異常系を確認する。
- 結合テストでアップロード API の正常系、変換失敗、Python 不在相当の応答を確認する。
- 外部依存はスタブ化し、テストを安定させる。

## MarkItDown 連携時の注意点

- Python 実行パスは設定値から取得する。
- コマンドは文字列連結せず `ProcessStartInfo.ArgumentList` を使う。
- 標準出力、標準エラー、終了コードを必ず記録する。
- 一時ディレクトリは変換ごとに分離し、finally で削除する。

## セキュリティ上の注意点

- アップロードファイル名を信用せず、一時保存名はランダム化する。
- 許可拡張子とサイズ上限を必ず検証する。
- 画面には例外詳細をそのまま出さず、ユーザー向けメッセージに整形する。
- パス結合と外部プロセス引数は安全な API を使う。