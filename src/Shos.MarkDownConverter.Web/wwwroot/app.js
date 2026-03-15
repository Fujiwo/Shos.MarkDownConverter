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

// 画面全体の初期化、イベント配線、変換要求の送信を調停するモジュール。
const elements = getElements();

let downloadFileName = 'converted.md';

// 初期表示時に設定を読み込んでからイベントを結び、UI の初期状態をそろえる。
initialize();

async function initialize() {
    await loadOptions();
    wireEvents();
}

async function loadOptions() {
    try {
        renderOptions(elements, await fetchOptions());
    } catch {
        // options の取得に失敗しても画面は開けるようにし、利用者には取得失敗だけを伝える。
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
    // 送信のたびに前回の結果とエラーを消して、今回の応答だけを画面に残す。
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

        // 成功ステータスでも想定外の JSON が返る可能性があるため、表示前に形を確認する。
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
        // 権限や対応状況は実行環境差が大きいため、利用前に API の存在を確認する。
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

    // 一時 URL を使って保存だけを行い、サーバー側へ生成結果を保持させない。
    const blob = new Blob([elements.resultOutput.value], { type: 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = downloadFileName;
    anchor.click();
    URL.revokeObjectURL(url);
    showStatus(elements, 'Markdown をダウンロードしました。');
}
