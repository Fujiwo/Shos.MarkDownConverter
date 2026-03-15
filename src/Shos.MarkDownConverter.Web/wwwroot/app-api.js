// API 呼び出しとレスポンス本文の読込だけを担当し、UI ロジックから通信の詳細を切り離す。
export async function fetchOptions() {
    const response = await fetch('/api/options');
    return response.json();
}

export async function postConvert(formData) {
    // fetch の戻り値だけでなく本文もまとめて返し、呼び出し側で分岐しやすくする。
    const response = await fetch('/api/convert', {
        method: 'POST',
        body: formData
    });

    return {
        response,
        payload: await readResponsePayload(response)
    };
}

async function readResponsePayload(response) {
    const responseText = await response.text();
    if (!responseText) {
        return null;
    }

    try {
        return JSON.parse(responseText);
    } catch {
        // 失敗時も本文があれば UI 側で扱えるように、生テキストとして保持して返す。
        return {
            rawText: responseText
        };
    }
}