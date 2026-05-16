# JCMU Addon: Git Initializer

A proof-of-concept plugin for the **Jinn Context Menu Utility (JCMU)** platform. This addon dynamically registers a context menu option in Windows, allowing users to instantly initialize a Git repository in any directory via a right-click.

This repository serves as a live demonstration of the `JCMU.SDK` and the platform's ability to download, compile in isolation, and safely execute third-party C# code directly from the Windows Shell.

## 🚀 Features

* **Seamless Integration:** Cascades neatly under the `JinnCM -> Git Tools` Windows context menu.
* **Instant Execution:** Right-click any folder and select "Initialize Git Repository" to run `git init`.
* **Visual Feedback:** Briefly displays the console output (Standard Out/Error) so you know exactly what happened before closing.

## 📋 Prerequisites

* [Git for Windows](https://git-scm.com/download/win) must be installed and accessible via your system's PATH.
* The JCMU Core Engine must be installed and initialized on your machine.

## 📦 Installation

Because JCMU uses GitHub as its decentralized App Store, you can install this addon directly via the JCMU CLI. 

Open your terminal and run:

```bash
jcmu install JinnFletch/JCMU.Addon.GitInit
```

The JCMU Core will automatically:
1. Clone this repository.
2. Compile the binaries using `dotnet publish`.
3. Validate the `manifest.json`.
4. Register the context menu keys in the Windows Registry.

## 💻 Usage

1. Open Windows File Explorer.
2. Right-click on any folder (or in the empty space of an open folder).
3. Navigate to **JinnCM** > **Git Tools** > **Initialize Git Repository**.
4. A console window will briefly appear, execute the git command, display the result, and close.

## 🛠️ For Developers: Under the Hood

This project demonstrates several key features of the JCMU Addon architecture:

* **`IJcmuAddon` Contract:** The `GitInitAddon.cs` class implements the standard SDK interface, mapping the registry keys and handling the execution boundary.
* **Manifest Publishing:** The `manifest.json` is configured in the `.csproj` to copy to the output directory upon build, ensuring the Core can read the addon's identity.
* **Dependency Isolation:** This addon utilizes the external `JinnDev.Utilities.CommandLine` library to execute OS processes. Thanks to JCMU's `AssemblyLoadContext` (ALC) architecture, this dependency is loaded exclusively for this addon, proving that plugins can bring their own dependencies without causing DLL Hell with the Core engine.

## 📝 License

MIT License. See [LICENSE](LICENSE) for more information.