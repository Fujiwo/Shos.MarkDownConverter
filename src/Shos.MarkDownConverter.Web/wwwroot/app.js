const form = document.getElementById('convert-form');
const fileInput = document.getElementById('file-input');
const dropzone = document.getElementById('dropzone');
const selectedFile = document.getElementById('selected-file');
const supportedExtensions = document.getElementById('supported-extensions');
const maxSize = document.getElementById('max-size');
const convertButton = document.getElementById('convert-button');
const statusText = document.getElementById('status-text');
const errorPanel = document.getElementById('error-panel');
const errorMessage = document.getElementById('error-message');
const errorCauses = document.getElementById('error-causes');
const errorActions = document.getElementById('error-actions');
const errorCauseSection = errorCauses.closest('.error-section');
const errorActionSection = errorActions.closest('.error-section');
const resultPanel = document.getElementById('result-panel');
const resultOutput = document.getElementById('result-output');
const copyButton = document.getElementById('copy-button');
const downloadButton = document.getElementById('download-button');

let downloadFileName = 'converted.md';

initialize();

async function initialize() {
    await loadOptions();
    wireEvents();
}

async function loadOptions() {
    try {
        const response = await fetch('/api/options');
        const options = await response.json();
        supportedExtensions.textContent = options.allowedExtensions.join(', ');
        maxSize.textContent = formatBytes(options.maxUploadSizeBytes);
        fileInput.setAttribute('accept', options.allowedExtensions.join(','));
    } catch {
        supportedExtensions.textContent = '取得に失敗しました';
        maxSize.textContent = '-';
    }
}

function wireEvents() {
    fileInput.addEventListener('change', () => {
        selectedFile.textContent = fileInput.files.length > 0 ? fileInput.files[0].name : '未選択';
    });

    form.addEventListener('submit', submitForm);
    copyButton.addEventListener('click', copyResult);
    downloadButton.addEventListener('click', downloadResult);

    ['dragenter', 'dragover'].forEach((eventName) => {
        dropzone.addEventListener(eventName, (event) => {
            event.preventDefault();
            dropzone.classList.add('dragover');
        });
    });

    ['dragleave', 'drop'].forEach((eventName) => {
        dropzone.addEventListener(eventName, (event) => {
            event.preventDefault();
            dropzone.classList.remove('dragover');
        });
    });

    dropzone.addEventListener('drop', (event) => {
        if (event.dataTransfer.files.length === 0) {
            return;
        }

        fileInput.files = event.dataTransfer.files;
        selectedFile.textContent = event.dataTransfer.files[0].name;
    });
}

async function submitForm(event) {
    event.preventDefault();
    clearError();
    resultPanel.hidden = true;

    try {
        const formData = new FormData(form);
        setBusy(true, '変換中です...');
        const response = await fetch('/api/convert', {
            method: 'POST',
            body: formData
        });
        const payload = await readResponsePayload(response);

        if (!response.ok) {
            showError(buildServerError(response, payload));
            return;
        }

        if (!payload || typeof payload.markdown !== 'string' || typeof payload.downloadFileName !== 'string') {
            showError({
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
            });
            return;
        }

        resultOutput.value = payload.markdown;
        downloadFileName = payload.downloadFileName;
        resultPanel.hidden = false;
        statusText.textContent = '変換が完了しました。';
    } catch {
        showError({
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
        });
    } finally {
        setBusy(false, statusText.textContent || '');
    }
}

async function copyResult() {
    if (!resultOutput.value) {
        return;
    }

    try {
        if (!navigator.clipboard?.writeText) {
            throw new Error('Clipboard API unavailable');
        }

        await navigator.clipboard.writeText(resultOutput.value);
        statusText.textContent = '変換結果をコピーしました。';
    } catch {
        showError(buildClipboardError());
    }
}

function downloadResult() {
    if (!resultOutput.value) {
        return;
    }

    const blob = new Blob([resultOutput.value], { type: 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = downloadFileName;
    anchor.click();
    URL.revokeObjectURL(url);
    statusText.textContent = 'Markdown をダウンロードしました。';
}

function showError(payload) {
    errorPanel.hidden = false;
    errorMessage.textContent = payload.message || 'エラーが発生しました。';
    errorCauses.innerHTML = '';
    errorActions.innerHTML = '';
    const causes = Array.isArray(payload.possibleCauses) ? payload.possibleCauses : [];
    const actions = Array.isArray(payload.actions) ? payload.actions : [];

    causes.forEach((cause) => {
        const item = document.createElement('li');
        item.textContent = cause;
        errorCauses.appendChild(item);
    });

    actions.forEach((action) => {
        const item = document.createElement('li');
        item.textContent = action;
        errorActions.appendChild(item);
    });

    errorCauseSection.hidden = causes.length === 0;
    errorActionSection.hidden = actions.length === 0;

    statusText.textContent = '変換に失敗しました。';
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

function buildServerError(response, payload) {
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

function clearError() {
    errorPanel.hidden = true;
    errorMessage.textContent = '';
    errorCauses.innerHTML = '';
    errorActions.innerHTML = '';
    errorCauseSection.hidden = false;
    errorActionSection.hidden = false;
}

function setBusy(isBusy, message) {
    convertButton.disabled = isBusy;
    fileInput.disabled = isBusy;
    statusText.textContent = message;
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

function buildClipboardError() {
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