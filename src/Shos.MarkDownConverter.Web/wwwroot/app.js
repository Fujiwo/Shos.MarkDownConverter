import { postConvert, fetchOptions } from './app-api.js';
import { getElements } from './app-dom.js';
import {
    buildClipboardError,
    buildNetworkFailureError,
    buildServerError,
    buildUnexpectedSuccessPayloadError,
    clearError,
    hideResult,
    renderOptions,
    renderOptionsLoadFailure,
    setBusy,
    setSelectedFileName,
    showError,
    showResult,
    showStatus
} from './app-ui.js';

const elements = getElements();

let downloadFileName = 'converted.md';

initialize();

async function initialize() {
    await loadOptions();
    wireEvents();
}

async function loadOptions() {
    try {
        renderOptions(elements, await fetchOptions());
    } catch {
        renderOptionsLoadFailure(elements);
    }
}

function wireEvents() {
    elements.fileInput.addEventListener('change', () => {
        setSelectedFileName(elements, elements.fileInput.files.length > 0 ? elements.fileInput.files[0].name : '未選択');
    });

    elements.form.addEventListener('submit', submitForm);
    elements.copyButton.addEventListener('click', copyResult);
    elements.downloadButton.addEventListener('click', downloadResult);

    ['dragenter', 'dragover'].forEach((eventName) => {
        elements.dropzone.addEventListener(eventName, (event) => {
            event.preventDefault();
            elements.dropzone.classList.add('dragover');
        });
    });

    ['dragleave', 'drop'].forEach((eventName) => {
        elements.dropzone.addEventListener(eventName, (event) => {
            event.preventDefault();
            elements.dropzone.classList.remove('dragover');
        });
    });

    elements.dropzone.addEventListener('drop', (event) => {
        if (event.dataTransfer.files.length === 0) {
            return;
        }

        elements.fileInput.files = event.dataTransfer.files;
        setSelectedFileName(elements, event.dataTransfer.files[0].name);
    });
}

async function submitForm(event) {
    event.preventDefault();
    clearError(elements);
    hideResult(elements);

    try {
        const formData = new FormData(elements.form);
        setBusy(elements, true, '変換中です...');
        const { response, payload } = await postConvert(formData);

        if (!response.ok) {
            showError(elements, buildServerError(response, payload));
            return;
        }

        if (!payload || typeof payload.markdown !== 'string' || typeof payload.downloadFileName !== 'string') {
            showError(elements, buildUnexpectedSuccessPayloadError());
            return;
        }

        downloadFileName = payload.downloadFileName;
        showResult(elements, payload.markdown);
    } catch {
        showError(elements, buildNetworkFailureError());
    } finally {
        setBusy(elements, false, elements.statusText.textContent || '');
    }
}

async function copyResult() {
    if (!elements.resultOutput.value) {
        return;
    }

    try {
        if (!navigator.clipboard?.writeText) {
            throw new Error('Clipboard API unavailable');
        }

        await navigator.clipboard.writeText(elements.resultOutput.value);
        showStatus(elements, '変換結果をコピーしました。');
    } catch {
        showError(elements, buildClipboardError());
    }
}

function downloadResult() {
    if (!elements.resultOutput.value) {
        return;
    }

    const blob = new Blob([elements.resultOutput.value], { type: 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = downloadFileName;
    anchor.click();
    URL.revokeObjectURL(url);
    showStatus(elements, 'Markdown をダウンロードしました。');
}
