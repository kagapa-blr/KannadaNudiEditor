# Kannada Nudi Android Keyboard

This is a native Android Keyboard (IME) application generated to support:
1.  **Transliteration (Phonetic Typing)**: Type in English (e.g., "namaskara") and get Kannada ("ನಮಸ್ಕಾರ").
2.  **Kannada Script Keyboard (Nudi/Expert)**: Direct key mapping for expert typists.
3.  **Voice Typing**: Native Android speech-to-text for Kannada.

## Project Structure

*   `app/src/main/java/com/kannadanudi/keyboard/`
    *   `KannadaIME.kt`: The main Input Method Service handling key events and voice input.
    *   `TransliterationEngine.kt`: The core logic ported from the C# `TransliterationService`, handling phonetic conversions.
*   `app/src/main/res/xml/`
    *   `qwerty.xml`: Layout for Phonetic/English typing.
    *   `nudi_layout.xml`: Layout for Nudi/Expert typing.
    *   `method.xml`: Definitions for the IME subtypes.

## How to Build and Run

1.  **Open in Android Studio**:
    *   Launch Android Studio.
    *   Select **Open** and choose the `KannadaNudiAndroid` folder.

2.  **Sync Gradle**:
    *   Wait for Android Studio to sync the project and download necessary dependencies.

3.  **Build**:
    *   Go to **Build > Make Project**.

4.  **Run**:
    *   Connect an Android device or start an Emulator.
    *   Click **Run > Run 'app'**.

## How to Enable the Keyboard

Once installed on the device:

1.  Go to **Settings > System > Languages & input > On-screen keyboard > Manage on-screen keyboards**.
2.  Toggle **Kannada Nudi Keyboard** to **ON**.
3.  Open any text field (e.g., in Messages or WhatsApp).
4.  Tap the keyboard switcher icon (usually at the bottom right) and select **Kannada Nudi Keyboard**.

## Usage

*   **Phonetic Mode (Default)**: Type words phonetically in English (e.g., 'k' -> 'ಕ್', 'a' -> 'ಕ').
*   **Switch to Nudi/Expert**: Tap the **'ಕ'** key on the bottom row.
*   **Switch back to Phonetic**: Tap the **'ABC'** key on the bottom row.
*   **Voice Typing**: Tap the **MIC** icon to start speaking in Kannada.
