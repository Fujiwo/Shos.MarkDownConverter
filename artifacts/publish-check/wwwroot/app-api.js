export async function fetchOptions() {
    const response = await fetch('/api/options');
    return response.json();
}

export async function postConvert(formData) {
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
        return {
            rawText: responseText
        };
    }
}