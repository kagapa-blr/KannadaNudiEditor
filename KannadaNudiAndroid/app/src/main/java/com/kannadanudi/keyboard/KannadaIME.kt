package com.kannadanudi.keyboard

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.inputmethodservice.InputMethodService
import android.inputmethodservice.Keyboard
import android.inputmethodservice.KeyboardView
import android.net.Uri
import android.os.Bundle
import android.provider.Settings
import android.speech.RecognitionListener
import android.speech.RecognizerIntent
import android.speech.SpeechRecognizer
import android.view.KeyEvent
import android.view.View
import android.view.inputmethod.EditorInfo
import android.view.inputmethod.InputConnection
import android.widget.Toast
import androidx.core.content.ContextCompat
import java.util.ArrayList

class KannadaIME : InputMethodService(), KeyboardView.OnKeyboardActionListener {

    private lateinit var keyboardView: KeyboardView
    private lateinit var qwertyKeyboard: Keyboard
    private lateinit var nudiKeyboard: Keyboard
    private var isCaps = false
    private val transliterationEngine = TransliterationEngine()
    private var speechRecognizer: SpeechRecognizer? = null
    private var isListening = false

    override fun onCreate() {
        super.onCreate()
        try {
            speechRecognizer = SpeechRecognizer.createSpeechRecognizer(this)
            speechRecognizer?.setRecognitionListener(object : RecognitionListener {
                override fun onReadyForSpeech(params: Bundle?) {}
                override fun onBeginningOfSpeech() {}
                override fun onRmsChanged(rmsdB: Float) {}
                override fun onBufferReceived(buffer: ByteArray?) {}
                override fun onEndOfSpeech() {
                    isListening = false
                }

                override fun onError(error: Int) {
                    isListening = false
                    val message = when (error) {
                        SpeechRecognizer.ERROR_NO_MATCH -> "No match"
                        SpeechRecognizer.ERROR_NETWORK -> "Network error"
                        SpeechRecognizer.ERROR_INSUFFICIENT_PERMISSIONS -> "Permission denied"
                        else -> "Error: $error"
                    }
                    Toast.makeText(this@KannadaIME, message, Toast.LENGTH_SHORT).show()
                }

                override fun onResults(results: Bundle?) {
                    val matches = results?.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION)
                    if (!matches.isNullOrEmpty()) {
                        val text = matches[0]
                        currentInputConnection.commitText(text, 1)
                    }
                }

                override fun onPartialResults(partialResults: Bundle?) {}
                override fun onEvent(eventType: Int, params: Bundle?) {}
            })
        } catch (e: Exception) {
            // Speech recognition not supported or initialization failed
            speechRecognizer = null
        }
    }

    override fun onCreateInputView(): View {
        val rootView = layoutInflater.inflate(R.layout.keyboard_view, null)
        keyboardView = rootView.findViewById(R.id.keyboard)
        qwertyKeyboard = Keyboard(this, R.xml.qwerty)
        nudiKeyboard = Keyboard(this, R.xml.nudi_layout)

        keyboardView.keyboard = qwertyKeyboard
        keyboardView.setOnKeyboardActionListener(this)

        return rootView
    }

    override fun onStartInput(attribute: EditorInfo?, restarting: Boolean) {
        super.onStartInput(attribute, restarting)
        transliterationEngine.clearBuffer()

        // Default to Phonetic (Baraha)
        transliterationEngine.setLayout(KeyboardLayout.Baraha)
    }

    override fun onStartInputView(info: EditorInfo?, restarting: Boolean) {
        super.onStartInputView(info, restarting)
        // Ensure keyboard is set when view is started
        keyboardView.keyboard = qwertyKeyboard
        keyboardView.invalidateAllKeys()
    }

    override fun onKey(primaryCode: Int, keyCodes: IntArray?) {
        val inputConnection = currentInputConnection ?: return

        when (primaryCode) {
            Keyboard.KEYCODE_DELETE -> {
                handleBackspace(inputConnection)
            }
            Keyboard.KEYCODE_SHIFT -> {
                isCaps = !isCaps
                keyboardView.isShifted = isCaps
                keyboardView.invalidateAllKeys()
            }
            Keyboard.KEYCODE_DONE -> {
                inputConnection.sendKeyEvent(KeyEvent(KeyEvent.ACTION_DOWN, KeyEvent.KEYCODE_ENTER))
                inputConnection.sendKeyEvent(KeyEvent(KeyEvent.ACTION_UP, KeyEvent.KEYCODE_ENTER))
            }
            -102 -> { // MIC Code
                checkAudioPermissionAndListen()
            }
            -200 -> { // Switch to Nudi
                transliterationEngine.setLayout(KeyboardLayout.Nudi)
                keyboardView.keyboard = nudiKeyboard
                keyboardView.invalidateAllKeys()
            }
            -201 -> { // Switch to Qwerty
                transliterationEngine.setLayout(KeyboardLayout.Baraha)
                keyboardView.keyboard = qwertyKeyboard
                keyboardView.invalidateAllKeys()
            }
            -2 -> {
                // Mode Change (123/Symbols) - Not implemented yet
                // Fallthrough prevention
            }
            else -> {
                var code = primaryCode.toChar()
                if (Character.isLetter(code) && isCaps) {
                    code = Character.toUpperCase(code)
                }

                // Process through Transliteration Engine
                val keyString = code.toString()
                val result = transliterationEngine.getTransliteration(keyString)

                if (result.backspaceCount > 0) {
                    inputConnection.deleteSurroundingText(result.backspaceCount, 0)
                }

                inputConnection.commitText(result.text, 1)
            }
        }
    }

    private fun handleBackspace(inputConnection: InputConnection) {
        // Clear buffer on backspace to prevent state desynchronization
        // This treats backspace as "commit current and delete previous char"
        transliterationEngine.clearBuffer()
        inputConnection.deleteSurroundingText(1, 0)
    }

    private fun checkAudioPermissionAndListen() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.RECORD_AUDIO) == PackageManager.PERMISSION_GRANTED) {
            startVoiceRecognition()
        } else {
            Toast.makeText(this, "Microphone permission required for Voice Typing", Toast.LENGTH_LONG).show()
            // Direct user to Settings to grant permission
            val intent = Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS).apply {
                data = Uri.fromParts("package", packageName, null)
                addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            }
            startActivity(intent)
        }
    }

    private fun startVoiceRecognition() {
        if (isListening) {
            speechRecognizer?.stopListening()
            isListening = false
        } else {
            val intent = Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH).apply {
                putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM)
                putExtra(RecognizerIntent.EXTRA_LANGUAGE, "kn-IN")
                putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 1)
            }
            try {
                speechRecognizer?.startListening(intent)
                isListening = true
                Toast.makeText(this, "Listening...", Toast.LENGTH_SHORT).show()
            } catch (e: Exception) {
                Toast.makeText(this, "Voice typing unavailable", Toast.LENGTH_SHORT).show()
            }
        }
    }

    override fun onPress(primaryCode: Int) {}
    override fun onRelease(primaryCode: Int) {}
    override fun onText(text: CharSequence?) {}
    override fun swipeLeft() {}
    override fun swipeRight() {}
    override fun swipeDown() {}
    override fun swipeUp() {}

    override fun onDestroy() {
        super.onDestroy()
        speechRecognizer?.destroy()
    }
}
