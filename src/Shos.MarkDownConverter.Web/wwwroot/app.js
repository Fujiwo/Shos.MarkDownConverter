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
const errorTips = document.getElementById('error-tips');
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
    setBusy(true, '変換中です...');

    try {
        const formData = new FormData(form);
        const response = await fetch('/api/convert', {
            method: 'POST',
            body: formData
        });
        const payload = await response.json();

        if (!response.ok) {
            showError(payload);
            return;
        }

        resultOutput.value = payload.markdown;
        downloadFileName = payload.downloadFileName;
        resultPanel.hidden = false;
        statusText.textContent = '変換が完了しました。';
    } catch {
        showError({
            message: '通信に失敗しました。アプリケーションが起動しているか確認してください。',
            tips: ['サーバーが起動しているか確認してください。', '時間をおいて再試行してください。']
        });
    } finally {
        setBusy(false, statusText.textContent || '');
    }
}

async function copyResult() {
    if (!resultOutput.value) {
        return;
    }

    await navigator.clipboard.writeText(resultOutput.value);
    statusText.textContent = '変換結果をコピーしました。';
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
    errorTips.innerHTML = '';
    const tips = Array.isArray(payload.tips) ? payload.tips : [];

    tips.forEach((tip) => {
        const item = document.createElement('li');
        item.textContent = tip;
        errorTips.appendChild(item);
    });

    statusText.textContent = '変換に失敗しました。';
}

function clearError() {
    errorPanel.hidden = true;
    errorMessage.textContent = '';
    errorTips.innerHTML = '';
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