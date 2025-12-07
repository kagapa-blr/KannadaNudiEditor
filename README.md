
# Kannada Nudi Editor

**Kannada Nudi Editor** is a powerful and user-friendly text editor designed to make writing in Kannada effortless.
Whether you are drafting documents, notes, or any form of text in Kannada, this editor provides a seamless experience
with intuitive features and tools.

Maintained and developed by [**KAGAPA**](https://kagapa.com/).

---

## 🖥️ OS Compatibility

- **Currently Supported:** Windows

We are working to expand support to other operating systems in the future. Stay tuned!

---

## 🌟 Key Features

1. **Kannada Writing System-Wide**
    - Write in Kannada anywhere on your system effortlessly. With system-wide support, you can switch to Kannada input
      seamlessly across all applications, making it convenient for consistent Kannada usage.

2. **Integrated Spell Check**
    - The editor comes with a built-in spell checker that underlines incorrect Kannada words in real-time.
    - Right-click on underlined words to get accurate suggestions, making corrections faster and easier.

3. **Speech-to-Text Integration (Kannada & English)**
    - Effortlessly convert speech to text in both Kannada and English.
    - Ideal for users who prefer dictation over typing, enhancing productivity and accessibility.

4. **UNICODE to ASCII and Vice Versa Conversion**
    - Supports smooth conversion between UNICODE and ASCII text formats.
    - Useful for legacy compatibility and ensuring your documents are accessible across different platforms.

5. **Comprehensive Editing Tools**
    - Equipped with all essential editor functionalities like copy, paste, undo, redo, and advanced formatting options.
    - Intuitive UI makes it easy to navigate and use these features effectively.

---

## 🌏 A Call to Developers: Let’s Make Kannada Easy for Everyone!

**Kannada Nudi Editor** is an open-source project with a vision to make Kannada typing and editing simple and accessible
for everyone. We believe that **"anybody can write easily in Kannada and correct their writings"**.

We invite developers, language enthusiasts, and open-source contributors to join us in expanding and refining this
project. Your contributions can help us create a globally recognized platform for Kannada text editing, making it easier
for millions of users to communicate effortlessly.

**Why Contribute?**

- Be a part of an impactful project aimed at preserving and promoting the Kannada language.
- Collaborate with a community of passionate developers.
- Gain experience by working on a feature-rich editor with advanced text-processing capabilities.

Let’s build something amazing together! 

---

## 📦 NuGet Dependencies

To make sure all required packages are available, run the following commands in the root of your project:

```bash
dotnet add package Newtonsoft.Json
dotnet add package Syncfusion.SfRichTextBoxAdv.WPF
dotnet add package Syncfusion.SfRichTextRibbon.WPF
dotnet add package Syncfusion.SfSkinManager.WPF
dotnet add package Syncfusion.Themes.Windows11Light.WPF
dotnet add package Syncfusion.DocToPDFConverter.Wpf
```

These commands will install the latest stable versions of the required NuGet packages for the project.

---


---

## Python Environment Setup for Speech-to-Text

The Speech-to-Text feature uses a Python script. Follow the steps below to set up the environment and build the executable.

### Setup Instructions

```powershell
# Create a Python virtual environment
python -m venv venv

# Temporarily allow script execution (PowerShell users only)
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# Activate the virtual environment
.\\venv\\Scripts\\Activate.ps1

# Install required Python packages
pip install SpeechRecognition PyAudio

# Install pyinstaller to build a standalone executable from the Python script
pip install pyinstaller

# Build the executable from recognize_mic.py
pyinstaller --onefile recognize_mic.py
pyinstaller --onefile recognize_mic.py
```

### Output

After the build completes, the `.exe` file will be located in the `dist` folder:

```
dist/
└── recognize_mic.exe
Ensure that `recognize_mic.py` is located in the same directory where you run the `pyinstaller` command (typically your project root or the directory you are working in) before running the commands above.

Ensure that `recognize_mic.py` is located in the same directory before running the commands above.

---



## 🚀 How to Use Kannada Nudi Editor

1. **Download and Install**
    - Download the latest release from [here](https://github.com/kagapa-blr/KannadaNudiEditor/releases).
    - Run the installer and follow the installation wizard to complete the setup.
    - Launch the editor and start writing effortlessly in Kannada!

---

## 🛠 How to Contribute to This Project

We welcome contributions from the community to help improve and expand the **Kannada Nudi Editor**. Follow these steps
to get started:

1. **Fork and Clone the Repository**
    - Fork the repository to your GitHub account.
    - Clone the forked repository to your local machine:
      ```bash
      git clone https://github.com/kagapa-blr/KannadaNudiEditor.git
      ```
2. **Switch to the Development Branch**
    - Navigate to the project directory:
      ```bash
      cd KannadaNudiEditor
      ```
    - Checkout the development branch:
      ```bash
      git checkout dev
      ```
3. **Add or Modify Features**
    - Create a new branch for your feature:
      ```bash
      git checkout -b feature-name
      ```
    - Implement your changes and commit them with clear messages:
      ```bash
      git commit -m "Add: Description of the feature"
      ```
4. **Submit a Pull Request**
    - Push your branch to GitHub:
      ```bash
      git push origin feature-name
      ```
    - Open a pull request to the `dev` branch of the original repository.
    - Provide a detailed description of the changes made.

Our team will review your pull request and get back to you as soon as possible.

---

For any questions or suggestions, feel free to open an issue or reach out.  
Thank you for contributing to **Kannada Nudi Editor**! 🙏

---

**Maintained and developed by [KAGAPA](https://kagapa.com/).**
