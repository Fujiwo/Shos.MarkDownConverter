export function renderOptions(elements, options) {
    elements.supportedExtensions.textContent = options.allowedExtensions.join(', ');
    elements.maxSize.textContent = formatBytes(options.maxUploadSizeBytes);
    elements.fileInput.setAttribute('accept', options.allowedExtensions.join(','));
}

export function renderOptionsLoadFailure(elements) {
    elements.supportedExtensions.textContent = '取得に失敗しました';
    elements.maxSize.textContent = '-';
}

export function setSelectedFileName(elements, fileName) {
    elements.selectedFile.textContent = fileName;
}

export function clearError(elements) {
    elements.errorPanel.hidden = true;
    elements.errorMessage.textContent = '';
    elements.errorCauses.innerHTML = '';
    elements.errorActions.innerHTML = '';
    elements.errorCauseSection.hidden = false;
    elements.errorActionSection.hidden = false;
}

export function hideResult(elements) {
    elements.resultPanel.hidden = true;
}

export function showResult(elements, markdown) {
    elements.resultOutput.value = markdown;
    elements.resultPanel.hidden = false;
    elements.statusText.textContent = '変換が完了しました。';
}

export function showStatus(elements, message) {
    elements.statusText.textContent = message;
}

export function showError(elements, payload) {
    elements.errorPanel.hidden = false;
    elements.errorMessage.textContent = payload.message || 'エラーが発生しました。';
    elements.errorCauses.innerHTML = '';
    elements.errorActions.innerHTML = '';

    const causes = Array.isArray(payload.possibleCauses) ? payload.possibleCauses : [];
    const actions = Array.isArray(payload.actions) ? payload.actions : [];

    causes.forEach((cause) => {
        const item = document.createElement('li');
        item.textContent = cause;
        elements.errorCauses.appendChild(item);
    });

    actions.forEach((action) => {
        const item = document.createElement('li');
        item.textContent = action;
        elements.errorActions.appendChild(item);
    });

    elements.errorCauseSection.hidden = causes.length === 0;
    elements.errorActionSection.hidden = actions.length === 0;
    elements.statusText.textContent = '変換に失敗しました。';
}

export function setBusy(elements, isBusy, message) {
    elements.convertButton.disabled = isBusy;
    elements.fileInput.disabled = isBusy;
    elements.statusText.textContent = message;
}

export function buildServerError(response, payload) {
    if (isStructuredErrorPayload(payload)) {
        return payload;
    }

    if (isProblemDetailsPayload(payload)) {
        return {
            code: 'problem-details',
            message: payload.detail || payload.title || 'サーバー内部でエラーが発生しました。',
            possibleCauses: [],
            actions: []
        };
    }

    if (response.status >= 500) {
        return {
            code: 'server-error',
            message: 'サーバー内部でエラーが発生しました。',
            possibleCauses: [],
            actions: [
                '時間をおいて再試行してください。',
                '問題が続く場合はアプリケーションログを確認してください。'
            ]
        };
    }

    return {
        code: 'unexpected-error-payload',
        message: '変換に失敗しましたが、詳細なエラー情報を取得できませんでした。',
        possibleCauses: [],
        actions: [
            '時間をおいて再試行してください。',
            '問題が続く場合はアプリケーションログを確認してください。'
        ]
    };
}

export function buildUnexpectedSuccessPayloadError() {
    return {
        code: 'unexpected-success-payload',
        message: '変換結果の受信に失敗しました。',
        possibleCauses: [
            'サーバーが想定外の応答を返しました。',
            'アプリケーションの内部エラーで応答形式が崩れた可能性があります。'
        ],
        actions: [
            '時間をおいて再試行してください。',
            '問題が続く場合はアプリケーションログを確認してください。'
        ]
    };
}

export function buildNetworkFailureError() {
    return {
        code: 'network-failure',
        message: 'サーバーに接続できませんでした。',
        possibleCauses: [
            'Web アプリが起動していない可能性があります。',
            'ネットワーク接続またはローカル通信に問題がある可能性があります。'
        ],
        actions: [
            'サーバーが起動しているか確認してください。',
            '時間をおいて再試行してください。'
        ]
    };
}

export function buildClipboardError() {
    return {
        code: 'clipboard-write-failed',
        message: '変換結果をクリップボードにコピーできませんでした。',
        possibleCauses: [
            'ブラウザーがクリップボード操作を許可していない可能性があります。',
            '現在の実行環境が Clipboard API に対応していない可能性があります。'
        ],
        actions: [
            'ブラウザーの権限設定を確認して再試行してください。',
            '必要ならダウンロード機能を利用してください。'
        ]
    };
}

function isStructuredErrorPayload(payload) {
    return Boolean(
        payload &&
        typeof payload.message === 'string' &&
        Array.isArray(payload.possibleCauses) &&
        Array.isArray(payload.actions));
}

function isProblemDetailsPayload(payload) {
    return Boolean(
        payload &&
        (typeof payload.title === 'string' || typeof payload.detail === 'string'));
}

function formatBytes(bytes) {
    if (bytes < 1024) {
        return `${bytes} B`;
    }

    const kiloBytes = bytes / 1024;
    if (kiloBytes < 1024) {
        return `${kiloBytes.toFixed(1)} KB`;
    }

    return `${(kiloBytes / 1024).toFixed(1)} MB`;
}