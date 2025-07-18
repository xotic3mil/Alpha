﻿

const quillDefaultConfig = {
    theme: 'snow',
    modules: {
        toolbar: [
            ['bold', 'italic', 'underline'],
            [{ 'align': [] }],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            ['link']
        ]
    },
    placeholder: 'Enter description...'
};


function initQuillEditor(textareaId, options = {}) {
    const textarea = document.getElementById(textareaId);
    if (!textarea) {
        console.error(`Textarea with ID '${textareaId}' not found`);
        return null;
    }

    const container = document.createElement('div');
    container.className = 'quill-editor';
    container.style.backgroundColor = 'var(--input-bg)';
    container.style.borderRadius = '0.25rem';
    container.style.marginBottom = '1rem';


    textarea.parentNode.insertBefore(container, textarea);
    textarea.style.display = 'none';

    const initialValue = textarea.value;

    const editorOptions = { ...quillDefaultConfig, ...options };

    const quill = new Quill(container, editorOptions);

    if (initialValue) {
        quill.clipboard.dangerouslyPasteHTML(initialValue);
    }

    quill.on('text-change', function () {
        textarea.value = quill.root.innerHTML;

        const event = new Event('change', { bubbles: true });
        textarea.dispatchEvent(event);
    });

    textarea.quill = quill;

    return quill;
}

function initAllQuillEditors(editorsToInitialize = null) {
    const editors = editorsToInitialize || document.querySelectorAll('[data-quill-editor]:not(.quill-initialized)');
    editors.forEach(textarea => {
        initQuillEditor(textarea.id);
        textarea.classList.add('quill-initialized');
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initAllQuillEditors();
});