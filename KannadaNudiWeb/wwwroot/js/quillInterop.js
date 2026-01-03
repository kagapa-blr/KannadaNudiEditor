window.quillInterop = {
    quill: null,
    dotNetRef: null,

    init: function (elementId, dotNetReference) {
        if (!window.Quill) {
            console.error("Quill JS not found");
            return;
        }

        console.log("Initializing Quill on", elementId);
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
                // console.log("Selection changed (user), clearing buffer");
                dotNetReference.invokeMethodAsync('OnSelectionChanged');
            }
        });
    },

    insertText: function (text) {
        if (this.quill) {
            const range = this.quill.getSelection(true); // true forces focus check? No, it returns null if not focused unless true passed?
            // "If true is passed as an argument, getSelection will check for selection even if the editor does not have focus."

            console.log("Inserting text:", text);

            if (range) {
                if (range.length > 0) {
                    this.quill.deleteText(range.index, range.length);
                }
                this.quill.insertText(range.index, text, 'api');
                // Ensure we move selection to end of inserted text
                this.quill.setSelection(range.index + text.length, 0, 'api');
            } else {
                // Focus and append
                this.quill.focus();
                const len = this.quill.getLength();
                this.quill.insertText(len - 1, text, 'api');
                this.quill.setSelection(len - 1 + text.length, 0, 'api');
            }
        } else {
            console.error("Quill instance null during insertText");
        }
    },

    backspace: function (count) {
        if (this.quill) {
            const range = this.quill.getSelection(true);
            if (range && range.index >= count) {
                console.log("Backspacing count:", count);
                this.quill.deleteText(range.index - count, count, 'api');
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
