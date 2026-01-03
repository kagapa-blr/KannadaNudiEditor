window.speechInterop = {
    recognition: null,
    dotNetRef: null,

    start: function (dotNetReference, lang) {
        if (!('webkitSpeechRecognition' in window)) {
            console.error("Web Speech API not supported.");
            return false;
        }

        this.dotNetRef = dotNetReference;
        this.recognition = new webkitSpeechRecognition();
        this.recognition.continuous = true;
        this.recognition.interimResults = false; // We only want final results
        this.recognition.lang = lang || 'kn-IN';

        this.recognition.onresult = function (event) {
            let finalTranscript = '';
            for (let i = event.resultIndex; i < event.results.length; ++i) {
                if (event.results[i].isFinal) {
                    finalTranscript += event.results[i][0].transcript;
                }
            }
            if (finalTranscript.length > 0) {
                dotNetReference.invokeMethodAsync('OnSpeechResult', finalTranscript);
            }
        };

        this.recognition.onerror = function (event) {
            console.error("Speech recognition error", event.error);
            dotNetReference.invokeMethodAsync('OnSpeechError', event.error);
        };

        this.recognition.onend = function () {
             // specific logic if needed
        };

        this.recognition.start();
        return true;
    },

    stop: function () {
        if (this.recognition) {
            this.recognition.stop();
            this.recognition = null;
        }
    }
};
