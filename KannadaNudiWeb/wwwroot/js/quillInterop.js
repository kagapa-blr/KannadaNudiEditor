window.quillInterop = {
    quill: null,
    dotNetRef: null,
    lastKeyHandledTime: 0,

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
                    [{ 'font': [] }, { 'size': [] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ 'color': [] }, { 'background': [] }],
                    [{ 'script': 'sub' }, { 'script': 'super' }],
                    [{ 'header': 1 }, { 'header': 2 }, 'blockquote', 'code-block'],
                    [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'indent': '-1' }, { 'indent': '+1' }],
                    [{ 'direction': 'rtl' }, { 'align': [] }],
                    ['link', 'image', 'video', 'formula'],
                    ['clean']
                ]
            }
        });

        // Disable predictive text features to prevent mobile keyboard interference
        if (this.quill.root) {
            this.quill.root.setAttribute('autocomplete', 'off');
            this.quill.root.setAttribute('autocorrect', 'off');
            this.quill.root.setAttribute('autocapitalize', 'off');
            this.quill.root.setAttribute('spellcheck', 'false');
        }

        this.quill.on('selection-change', function(range, oldRange, source) {
            if (source === 'user' && dotNetReference) {
                dotNetReference.invokeMethodAsync('OnSelectionChanged');
            }
        });

        // Mobile Input Support via text-change
        this.quill.on('text-change', (delta, oldDelta, source) => {
            if (source === 'user' && window.isKannadaMode) {
                if (!this.dotNetRef) return;

                let index = 0;
                let handled = false;

                // Iterate ops to find the operation and its position
                for (let i = 0; i < delta.ops.length; i++) {
                    const op = delta.ops[i];
                    if (op.retain) {
                        index += op.retain;
                    } else if (op.insert && typeof op.insert === 'string') {
                        // Handle single character insertions (excluding newlines)
                        if (op.insert.length === 1 && op.insert !== '\n') {
                            // Revert the user's insertion immediately by deleting it
                            this.quill.deleteText(index, op.insert.length, 'silent');

                            // Send the character to C# for transliteration
                            this.dotNetRef.invokeMethodAsync('ProcessKannadaKey', op.insert);
                            handled = true;
                            break; // Handle one char at a time for safety
                        }
                        index += op.insert.length;
                    } else if (op.delete) {
                         // Check if this deletion was already handled by keydown
                         const now = Date.now();
                         // We only handle it if keydown didn't catch it recently
                         if (now - this.lastKeyHandledTime > 100) {
                             this.dotNetRef.invokeMethodAsync('ProcessBackspace');
                             handled = true;
                         }
                         // For delete, we don't need to 'revert' anything, just notify C#.
                         break;
                    }
                }
            }
        });
    },

    insertText: function (text) {
        if (this.quill) {
            const range = this.quill.getSelection(true);
            console.log("Inserting text:", text);

            if (range) {
                if (range.length > 0) {
                    this.quill.deleteText(range.index, range.length);
                }
                this.quill.insertText(range.index, text, 'api');
                this.quill.setSelection(range.index + text.length, 0, 'api');
            } else {
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

    getText: function() {
        if (this.quill) {
            return this.quill.getText();
        }
        return "";
    },

    setHtml: function (html) {
        if (this.quill) {
            this.quill.clipboard.dangerouslyPasteHTML(html);
        }
    },

    saveAsDocx: async function(filename, htmlContent) {
        if (!window.docshift) {
            console.error("DocShift library not loaded");
            return;
        }
        try {
            const blob = await window.docshift.toDocx(htmlContent);
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        } catch (error) {
            console.error("Error generating DocX:", error);
        }
    },

    downloadFile: function(filename, content, contentType) {
        contentType = contentType || 'text/html';
        const element = document.createElement('a');
        const file = new Blob([content], {type: contentType});
        element.href = URL.createObjectURL(file);
        element.download = filename;
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    },

    // Input Interception Logic moved here
    registerKeyInterceptor: function (dotNetReference) {
        // We use the container or editor div.
        // Quill usually creates .ql-editor inside the container.
        // We can listen on window or the container.
        // Listening on container is safer.
        const editorDiv = document.getElementById('quill-editor'); // This is the container passed to init

        if(!editorDiv) {
            console.error("Quill editor div not found during registration");
            return;
        }

        console.log("Registering key interceptor on #quill-editor");

        editorDiv.addEventListener('keydown', (e) => {
            if (window.isKannadaMode) {
                console.log("Keydown intercepted (Kannada Mode):", e.key);

                // Handle Backspace
                if (e.key === 'Backspace') {
                    console.log("Processing Backspace");
                    this.lastKeyHandledTime = Date.now(); // Mark as handled
                    // We notify C# to update buffer, but we let Quill perform the deletion normally.
                    dotNetReference.invokeMethodAsync('ProcessBackspace');
                    return;
                }

                // Handle single chars
                if (e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey) {
                    console.log("Preventing default and processing key:", e.key);
                    e.preventDefault();
                    e.stopPropagation();
                    this.lastKeyHandledTime = Date.now(); // Mark as handled
                    dotNetReference.invokeMethodAsync('ProcessKannadaKey', e.key);
                }
            }
        }, true); // Capture phase
    },

    setKannadaMode: function (val) {
        console.log("Setting Kannada Mode:", val);
        window.isKannadaMode = val;
    }
};
