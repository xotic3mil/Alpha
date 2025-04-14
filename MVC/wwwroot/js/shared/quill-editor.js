/**
 * Quill editor initialization for rich text fields
 */

const quillDefaultConfig = {
    theme: 'snow',
    modules: {
        toolbar: [
            ['bold', 'italic', 'underline', 'strike'],
            ['blockquote', 'code-block'],
            [{ 'header': 1 }, { 'header': 2 }],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            [{ 'script': 'sub' }, { 'script': 'super' }],
            [{ 'indent': '-1' }, { 'indent': '+1' }],
            ['link'],
            ['clean']
        ]
    },
    placeholder: 'Enter description...'
};

/**
 * @param {string} textareaId 
 * @param {object} options 
 * @returns {Quill} 
 */
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

function initAllQuillEditors() {
    document.querySelectorAll('[data-quill-editor]').forEach(textarea => {
        initQuillEditor(textarea.id);
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initAllQuillEditors();
});