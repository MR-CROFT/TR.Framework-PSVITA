📦 How to Export to PSVita – TR AOD Framework

This guide will help you set up everything needed to **export the Tomb Raider AOD Framework to PlayStation Vita** using Unity.

---

## ✅ Step-by-Step Installation

### 1. 🔧 Install PSVita SDK

You must install the **Vita SDK** first.

🔗 Official website:  
👉 https://vitasdk.org/

📄 Follow the installation instructions on the website.  
This step sets up the development environment required to build and run code on the PS Vita.

---

### 2. 🧱 Install Unity 2018.2.19f1

This project uses **Unity 2018.2.19f1**, the last version officially compatible with the PSVita SDK.

If you haven’t installed it yet:

🔗 Download Unity 2018.2.19f1:  
👉 https://unity3d.com/get-unity/download/archive

✅ If you’ve already installed it, you can skip this step.

Make sure to include:
- ✅ Windows Build Support (for PC testing)
- ✅ *No need for Android, iOS, etc.*

---

### 3. 🎮 Install Unity PSVita Editor Support

You’ll now need to add **PSVita platform support** for Unity.

🔗 Download it from Internet Archive:  
👉 https://archive.org/details/unitypsvitasupport

📦 Look for:  
`Unity 2018.2.19f1 PSVita Editor Support`

Unpack the files and follow any included instructions to install the Vita support into your Unity Editor installation.

---

### 4. 🔓 Unlock the PSVita Build Option in Unity

You must now unlock PSVita platform support inside Unity using **Unihacker**.

Unihacker unlocks the PS Vita module in Unity 2018 so it appears in **File > Build Settings**.

🔍 Search online for "Unihacker Unity PSVita" or use trusted homebrew/dev communities to find the tool.

> ⚠️ Use responsibly. This is intended for homebrew and fan development only.

---

### ✅ Done!

After completing these steps:

- Open the project in Unity 2018.2.19f1
- Go to **File → Build Settings**
- Select **PS Vita** and click **Switch Platform**

📦 Build & Convert to VPK

Once the platform is switched to **PS Vita**:

1. Go to **File → Build Settings → Build**
2. Choose a folder and let Unity build the game files
3. Then, **drag the output folder onto `UnityTools.exe`** located in:/PSVitaBuilds/UnityTools.exe

Tranfer the VPK into your PSVita and install.



---

## 📌 Notes

- PC is now the **default platform** in the project for easier development and testing.
- You can switch to PSVita at any time after completing the above steps.