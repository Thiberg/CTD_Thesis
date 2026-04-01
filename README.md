# CTD_Thesis

Master's thesis in games — Unity game, written report, and presentation.

---

## Repository Structure

```
CTD_Thesis/
├── Assets/           ← Unity project assets (scripts, scenes, prefabs, art, audio…)
├── Packages/         ← Unity Package Manager manifest
├── ProjectSettings/  ← Unity project settings
└── README.md
```

The `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, and `UserSettings/` folders are excluded from version control via `.gitignore` — Unity regenerates them automatically.

---

## Prerequisites

| Tool | Purpose |
|------|---------|
| [Unity Hub](https://unity.com/download) | Install & manage Unity Editor versions |
| [Git](https://git-scm.com/) | Version control |
| [Git LFS](https://git-lfs.com/) | Store large binary assets (textures, audio, models…) |

> **Important:** Install Git LFS **before** cloning. Large binary files (`.png`, `.fbx`, `.wav`, etc.) are tracked with Git LFS to keep the repository size manageable.

---

## Getting Started on a New Device

```bash
# 1. Install Git LFS (one-time per machine)
git lfs install

# 2. Clone the repository
git clone https://github.com/Thiberg/CTD_Thesis.git
cd CTD_Thesis

# 3. Open the project in Unity Hub
#    File → Open Project → select the CTD_Thesis folder
```

Unity will reimport assets and regenerate the `Library/` folder on first open — this is normal and may take a few minutes.

---

## Day-to-Day Workflow

```bash
# Pull the latest changes before starting work
git pull

# Stage, commit, and push your work
git add .
git commit -m "Short description of what changed"
git push
```

Git LFS handles large binary files transparently — no extra commands needed after the initial `git lfs install`.
