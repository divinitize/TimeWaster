# Unity Time Waster Tracker

A lightweight and fun Unity Editor overlay that tracks how much time you've spent waiting on **script compilation** and **domain reloads** — because we all know how Unity likes to take its sweet time.

This tool gives you a passive reminder of just how much of your life is lost to the great "compiling scripts..." abyss.

---

## ✨ Features

- 🔻 Minimal overlay in the Scene view (`♥ Life wasted waiting: MM:SS`)
- 📊 Click the overlay to view a breakdown of time spent:
  - Script compilation
  - Domain reload
  - (And a small buffer to approximate the UI delay after reload)
- 🧠 Time is **persisted between sessions**
- 💡 Zero impact on performance or build pipeline
- 🔧 Fully draggable overlay — put it wherever you like

---

## ❗️What It Tracks

This tracker monitors:
- ✅ Script compilation (`CompilationPipeline`)
- ✅ Domain reloads (`AssemblyReloadEvents`)
- ✅ An estimated buffer delay after reload (about 3.5 seconds)

It does **not** track:
- ❌ “Hold on…” popups
- ❌ Asset imports
- ❌ Scene saves or shader compilation
- ❌ Long file move/reimport events

This is by design — the tool is intentionally focused and simple.

---

## 💾 How to Install

### Option 1: UnityPackage
1. Download `TimeWaster.unitypackage`
2. In Unity: `Assets > Import Package > Custom Package...`
3. Done!

### Option 2: Manual Copy
1. Copy the `Editor/TimeWaster/` folder into your Unity project.
2. The overlay will appear in the Scene view.

---

## 📋 Usage

- The overlay appears by default in the **bottom-left** corner of the Scene view.
- Click and drag to move it.
- Click it to open the **Time Waster Stats** window with a breakdown by category.
- Reset the timer from the stats window if you're feeling optimistic.

---

## 📝 License

MIT — use it, modify it, ship it with your own editor tools. Attribution not required but appreciated.

---

## 🙃 Disclaimer

This plugin is purely for entertainment and light diagnostics. It doesn't delay or block anything, nor does it try to log every editor wait moment. If you're looking for full analytics, this ain't it.

Enjoy!
