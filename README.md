# Kannada Nudi Editor

**Kannada Nudi Editor** is a powerful, user-friendly text editor designed to make writing in Kannada effortless.  
Whether you are drafting documents, notes, or any form of text in Kannada, this editor provides a seamless experience with intuitive features and tools.

Maintained and developed by [**KAGAPA**](https://kagapa.com/).

---

## 🖥️ OS Compatibility

- **Currently Supported:** Windows
- **Future Plans:** Expanding support to other operating systems.

---

## 🌟 Key Features

1. **System-Wide Kannada Input**
    - Write in Kannada anywhere on your system.
    - Seamlessly switch to Kannada input across all applications.

2. **Integrated Spell Check**
    - Built-in spell checker underlines incorrect Kannada words in real-time.
    - Right-click for suggestions to make corrections faster.

3. **Speech-to-Text Integration (Kannada & English)**
    - Convert speech to text in both Kannada and English.
    - Ideal for dictation, improving productivity and accessibility.

4. **Unicode ↔ ASCII Conversion**
    - Smoothly convert between Unicode and ASCII text formats.
    - Ensures legacy compatibility and document portability.

5. **Comprehensive Editing Tools**
    - Standard functionalities: copy, paste, undo, redo, and advanced formatting.
    - Intuitive UI for a smooth editing experience.

---

## 🌏 Open-Source & Developer Call

**Kannada Nudi Editor** is open-source and aims to make Kannada typing simple and accessible.  
We invite developers, language enthusiasts, and contributors to help improve and expand the project.  

**Why contribute?**

- Preserve and promote the Kannada language.
- Collaborate with a community of passionate developers.
- Gain experience on a feature-rich text editor with advanced language tools.

Let's build something amazing together!

---

## 📦 NuGet Dependencies

Ensure all required packages are installed:

```bash
dotnet add package Newtonsoft.Json
dotnet add package Syncfusion.SfRichTextBoxAdv.WPF
dotnet add package Syncfusion.SfRichTextRibbon.WPF
dotnet add package Syncfusion.SfSkinManager.WPF
dotnet add package Syncfusion.Themes.Windows11Light.WPF
dotnet add package Syncfusion.DocToPDFConverter.Wpf
```

---

## 🐍 Python Setup for Speech-to-Text

The Speech-to-Text feature uses a Python script. Follow these steps:

### 1. Create a virtual environment

```powershell
python -m venv venv
```

### 2. Activate the virtual environment

```powershell
# Windows PowerShell
.\\venv\\Scripts\\Activate.ps1
```

### 3. Install required packages

```bash
pip install SpeechRecognition PyAudio pyinstaller
```

### 4. Build executable

```bash
pyinstaller --onefile recognize_mic.py
```

The resulting `.exe` will be in the `dist` folder. Ensure `recognize_mic.py` is in the working directory before building.

---

## 🚀 How to Use Kannada Nudi Editor

1. **Download & Install**
    - Get the latest release from [GitHub Releases](https://github.com/kagapa-blr/KannadaNudiEditor/releases).
    - Run the installer and follow the setup wizard.

2. **Start Writing**
    - Launch the editor.
    - Type in Kannada effortlessly across your system.

---

## 🛠 Contributing

We welcome contributions to improve Kannada Nudi Editor.

### Steps to Contribute

1. **Fork & Clone**
    ```bash
    git clone https://github.com/kagapa-blr/KannadaNudiEditor.git
    cd KannadaNudiEditor
    ```

2. **Switch to Development Branch**
    ```bash
    git checkout dev
    ```

3. **Add Features / Fix Issues**
    ```bash
    git checkout -b feature-name
    # Make changes
    git commit -m "Add: Description of your feature"
    ```

4. **Push & Create Pull Request**
    ```bash
    git push origin feature-name
    ```
    - Open a PR to the `dev` branch.
    - Provide detailed description of changes.

---

For questions or suggestions, open an issue or reach out via GitHub.  

**Maintained and developed by [KAGAPA](https://kagapa.com/).**

