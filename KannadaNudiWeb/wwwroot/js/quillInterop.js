window.quillInterop = {
    quill: null,
    dotNetRef: null,

    init: function (elementId, dotNetReference) {
        if (!window.Quill) {
            console.error("Quill JS not found");
            return;
        }

        this.dotNetRef = dotNetReference;
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

        this.quill.on('selection-change', function(range, oldRange, source) {
            if (source === 'user' && dotNetReference) {
                dotNetReference.invokeMethodAsync('OnSelectionChanged');
            }
        });
    },

    insertText: function (text) {
        if (this.quill) {
            const range = this.quill.getSelection(true);
            if (range) {
                if (range.length > 0) {
                    this.quill.deleteText(range.index, range.length);
                }
                this.quill.insertText(range.index, text);
                this.quill.setSelection(range.index + text.length);
            } else {
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
