namespace Shos.MarkDownConverter.Web.Services;

public sealed class MarkItDownErrorFormatter
{
    public ConversionError FormatExecutionFailure(int exitCode, string standardError)
    {
        var stderr = standardError.Trim();
        if (ContainsAny(stderr, "No module named", "ModuleNotFoundError") && stderr.Contains("markitdown", StringComparison.OrdinalIgnoreCase))
        {
            return DependencyMissing("MarkItDown が見つかりません。Python 環境に markitdown をインストールしてください。");
        }

        if (ContainsAny(stderr, "No such file or directory", "can't open file", "is not recognized") && stderr.Contains("python", StringComparison.OrdinalIgnoreCase))
        {
            return DependencyMissing("Python 実行環境が見つかりません。設定した Python 実行パスを確認してください。");
        }

        return new ConversionError(
            StatusCodes.Status422UnprocessableEntity,
            "ファイルを Markdown に変換できませんでした。ファイル形式と MarkItDown の追加依存関係を確認してください。",
            [
                "対応形式か確認してください。",
                "必要な MarkItDown の追加依存関係がインストールされているか確認してください。",
                "詳細はアプリケーションログを確認してください。"
            ],
            $"exitCode={exitCode}; stderr={stderr}");
    }

    public ConversionError FormatLaunchFailure(Exception exception)
    {
        return DependencyMissing(
            "Python 実行環境を起動できませんでした。設定した Python 実行パスを確認してください。",
            exception.ToString());
    }

    private static ConversionError DependencyMissing(string message, string? diagnosticDetails = null)
    {
        return new ConversionError(
            StatusCodes.Status503ServiceUnavailable,
            message,
            [
                "Python のインストール状態を確認してください。",
                "markitdown パッケージが利用可能か確認してください。",
                "README のセットアップ手順に沿って環境を再確認してください。"
            ],
            diagnosticDetails);
    }

    private static bool ContainsAny(string text, params string[] fragments)
    {
        return fragments.Any(fragment => text.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record ConversionError(int StatusCode, string Message, IReadOnlyList<string> Tips, string? DiagnosticDetails);