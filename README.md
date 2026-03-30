🧟 Zombie Apocalypse: Telltale Survival

An immersive choice-driven zombie survival game built with C# WinForms, featuring dynamic UI, sound effects, and branching storylines inspired by Telltale-style gameplay.

🎮 Overview

Zombie Apocalypse: Telltale Survival is an interactive story game where every decision matters. Players navigate through dangerous scenarios, manage health and inventory, and experience different outcomes based on their choices.

The game features a modern dark-themed UI, chapter-based progression, and immersive elements like screen shake effects and background music.

✨ Features
🧠 Choice-Based Storytelling
Make decisions that directly impact the storyline and ending.
❤️ Health System
Track player health with a visual progress bar.
🎒 Inventory System
Collect and manage items throughout the game.
🎨 Modern UI Design
Dark theme
Styled buttons and panels
Dynamic layout resizing
🖼️ Dynamic Backgrounds
Chapter-based background images that change as the story progresses.
🔊 Sound Effects & Music
Background music changes per chapter for immersion.
📳 Screen Shake Effect
Visual feedback during intense moments (e.g., damage events).
💾 Save & Load System
Save your progress and continue anytime.
🔁 Restart Functionality
Restart the game instantly from the main menu.
🛠️ Tech Stack
Language: C#
Framework: .NET (Windows Forms)
Libraries Used:
System.Windows.Forms
System.Media (for sound)
System.Drawing (for UI)
🧩 How It Works

The game is powered by a custom GameEngine that:

Controls story progression
Handles player choices
Updates UI through callbacks:
AppendOutput() → Displays story text
UpdateOptions() → Updates choice buttons
UpdateStatus() → Updates health, inventory, and chapter
📂 Project Structure
Zombie apocolypse telltale/
│── Form1.cs                 # Main UI logic and rendering
│── GameEngine.cs            # Core game logic (story + choices)
│── Program.cs               # Entry point
│── Properties/              # Resources and settings
│── images/                  # Background and chapter images
│── Sounds/                  # Background music (chapter-based)
▶️ Getting Started
Prerequisites
Visual Studio (recommended)
.NET Framework / .NET SDK
Run the Project

Clone the repository:

git clone https://github.com/your-username/zombie-apocalypse-telltale.git

Open:

Zombie apocolypse telltale.sln
Press F5 to build and run
🎯 Gameplay
Launch the game
Click START GAME
Read the story carefully
Choose from available options
Manage your health and inventory
Survive… or die 👀
🖼️ Assets

Make sure these folders exist in your project directory:

images/ → Backgrounds (chapter1.jpg, etc.)
Sounds/ → Music files (chapter1.wav, etc.)
💡 Notable Implementation Details
Dynamic UI Scaling: Adjusts layout on window resize
Thread-safe UI updates: Uses Invoke() for safe cross-thread updates
Custom Start Screen: Overlay panel with background image and start button
Text Styling: Color-coded output (damage, healing, survival events)
🚀 Future Improvements
🎵 Add sound effects for choices
💾 Multiple save slots
🌐 Export story system to JSON (mod support)
🧠 Smarter branching logic
🎨 Animations and transitions
🤝 Contributing

Feel free to fork this project and improve it!

Fork the repo
Create a new branch
Commit your changes
Submit a pull request
📄 License

Add your preferred license (MIT recommended).

👨‍💻 Author

M.ABU BAKAR
