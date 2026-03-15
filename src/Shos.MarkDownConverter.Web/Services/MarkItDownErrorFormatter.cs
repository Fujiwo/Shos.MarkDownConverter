namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// MarkItDown 実行時の失敗を、利用者向けに案内しやすいエラーへ分類します。
/// </summary>
public sealed class MarkItDownErrorFormatter
{
    public ConversionError FormatExecutionFailure(int exitCode, string standardError)
    {
        var stderr = standardError.Trim();
		// 依存不足は再試行では直りにくいため、一般的な変換失敗とは分けて具体的に案内する。
        if (ContainsAny(stderr, "No module named", "ModuleNotFoundError") && stderr.Contains("markitdown", StringComparison.OrdinalIgnoreCase))
        {
            return new ConversionError(
                StatusCodes.Status503ServiceUnavailable,
                "markitdown-missing",
                "MarkItDown が Python 環境に見つかりません。",
                [
                    "選択された Python 環境に markitdown パッケージがインストールされていません。",
                    "アプリが想定と異なる Python 実行環境を参照している可能性があります。"
                ],
                [
                    "対象の Python 環境で markitdown をインストールしてください。",
                    "PythonExecutablePath の設定先が正しいか確認してください。"
                ],
                $"exitCode={exitCode}; stderr={stderr}");
        }

        if (ContainsAny(stderr, "No such file or directory", "can't open file", "is not recognized") && stderr.Contains("python", StringComparison.OrdinalIgnoreCase))
        {
            return new ConversionError(
                StatusCodes.Status503ServiceUnavailable,
                "python-missing",
                "Python 実行環境が見つかりません。",
                [
                    "設定された Python 実行パスが存在しません。",
                    "サーバーに Python がインストールされていない可能性があります。"
                ],
                [
                    "PythonExecutablePath の設定値を確認してください。",
                    "Python をインストール後、アプリを再起動してください。"
                ],
                $"exitCode={exitCode}; stderr={stderr}");
        }

        return new ConversionError(
            StatusCodes.Status422UnprocessableEntity,
            "conversion-failed",
            "ファイルを Markdown に変換できませんでした。",
            [
                "入力ファイルの内容が破損している可能性があります。",
                "この形式に必要な MarkItDown の追加依存関係が不足している可能性があります。",
                "MarkItDown 自体がこのファイル内容を解釈できなかった可能性があります。"
            ],
            [
                "元ファイルを開いて破損していないか確認してください。",
                "必要なら markitdown の追加依存関係を含めて再インストールしてください。",
                "問題が続く場合はアプリケーションログを確認してください。"
            ],
            $"exitCode={exitCode}; stderr={stderr}");
    }

    public ConversionError FormatLaunchFailure(Exception exception)
    {
        return new ConversionError(
            StatusCodes.Status503ServiceUnavailable,
            "python-launch-failed",
            "Python 実行環境を起動できませんでした。",
            [
                "設定した Python 実行パスにアクセスできません。",
                "実行権限やパス設定が正しくない可能性があります。"
            ],
            [
                "PythonExecutablePath の設定値を確認してください。",
                "対象の Python をコマンドラインから直接起動できるか確認してください。",
                "セットアップ手順に沿って環境を再確認してください。"
            ],
            exception.ToString());
    }

    // 複数の候補文字列のいずれかを含むかを、大文字小文字を無視して判定する。
    private static bool ContainsAny(string text, params string[] fragments)
    {
        return fragments.Any(fragment => text.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record ConversionError(
    int StatusCode,
    string Code,
    string Message,
    IReadOnlyList<string> PossibleCauses,
    IReadOnlyList<string> Actions,
    string? DiagnosticDetails);