// DOM 要素参照を 1 か所へ集約し、他モジュールが id 文字列へ直接依存しないようにする。
export function getElements() {
    const errorCauses = document.getElementById('error-causes');
    const errorActions = document.getElementById('error-actions');

    return {
        form: document.getElementById('convert-form'),
        fileInput: document.getElementById('file-input'),
        dropzone: document.getElementById('dropzone'),
        selectedFile: document.getElementById('selected-file'),
        supportedExtensions: document.getElementById('supported-extensions'),
        maxSize: document.getElementById('max-size'),
        convertButton: document.getElementById('convert-button'),
        statusText: document.getElementById('status-text'),
        errorPanel: document.getElementById('error-panel'),
        errorMessage: document.getElementById('error-message'),
        errorCauses,
        errorActions,
        // 見出しとリストをまとめて隠せるよう、リストの親セクションも保持しておく。
        errorCauseSection: errorCauses.closest('.error-section'),
        errorActionSection: errorActions.closest('.error-section'),
        resultPanel: document.getElementById('result-panel'),
        resultOutput: document.getElementById('result-output'),
        copyButton: document.getElementById('copy-button'),
        downloadButton: document.getElementById('download-button')
    };
}