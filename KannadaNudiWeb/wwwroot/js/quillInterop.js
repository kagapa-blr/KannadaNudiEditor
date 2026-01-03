window.quillInterop = {
    quill: null,

    init: function (elementId) {
        if (!window.Quill) {
            console.error("Quill JS not found");
            return;
        }
        this.quill = new Quill(elementId, {
            theme: 'snow',
            modules: {
                toolbar: [
                    [{ 'header': [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline'],
                    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                    ['clean']
                ]
            }
        });
    },

    insertText: function (text) {
        if (this.quill) {
            const range = this.quill.getSelection(true);
            if (range) {
                // If there is a selection range (highlighted text), delete it first?
                // Quill insertText inserts at index.
                if (range.length > 0) {
                    this.quill.deleteText(range.index, range.length);
                }
                this.quill.insertText(range.index, text);
                this.quill.setSelection(range.index + text.length);
            } else {
                // Fallback to length if no selection?
                // Usually getSelection returns null if not focused.
                // We might need to focus.
                this.quill.focus();
                const len = this.quill.getLength();
                this.quill.insertText(len - 1, text);
            }
        }
    },

    backspace: function (count) {
        if (this.quill) {
            const range = this.quill.getSelection(true);
            if (range && range.index >= count) {
                this.quill.deleteText(range.index - count, count);
            }
        }
    },

    getHtml: function () {
        if (this.quill) {
            return this.quill.root.innerHTML;
        }
        return "";
    },

    setHtml: function (html) {
        if (this.quill) {
            this.quill.clipboard.dangerouslyPasteHTML(html);
        }
    },

    downloadFile: function(filename, content) {
        const element = document.createElement('a');
        const file = new Blob([content], {type: 'text/html'});
        element.href = URL.createObjectURL(file);
        element.download = filename;
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    }
};
