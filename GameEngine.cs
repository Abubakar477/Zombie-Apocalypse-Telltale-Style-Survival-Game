using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Zombie_apocolypse_telltale
{
    class GameEngine
    {
        private int playerHealth = 100;
        private List<string> inventory = new List<string>();
        private int currentChapter = 1;
        private bool gameRunning = true;
        private bool chapterIntroPlayed = false;
        private Random rng = new Random();

        private bool trustedSarah = false;
        private bool savedChild = false;

        private List<string> chapterParagraphs = new List<string>();
        private int currentParagraphIndex = 0;
        private bool isReading = false;

        private readonly Action<string> output;
        private readonly Action<string, string, string, string> updateOptions;
        private readonly Action<int, List<string>, int> updateStatus;

        public GameEngine(Action<string> outputCallback, Action<string, string, string, string> optionsCallback, Action<int, List<string>, int> statusCallback)
        {
            output = outputCallback ?? throw new ArgumentNullException(nameof(outputCallback));
            updateOptions = optionsCallback ?? throw new ArgumentNullException(nameof(optionsCallback));
            updateStatus = statusCallback ?? throw new ArgumentNullException(nameof(statusCallback));
        }

        public void Start()
        {
            output("==================================================");
            output("           ZOMBIE APOCALYPSE: SURVIVAL            ");
            output("==================================================");
            updateStatus(playerHealth, inventory, currentChapter);
            NextState();
        }

        private void NextState()
        {
            if (playerHealth <= 0)
            {
                output("*** YOUR WOUNDS WERE TOO SEVERE. YOU HAVE DIED. ***");
                updateOptions(null, null, null, null);
                updateStatus(playerHealth, inventory, currentChapter);
                gameRunning = false;
                return;
            }

            if (currentChapter == 1)
            {
                if (!chapterIntroPlayed)
                {
                    var paras = new[] {
                        "--- CHAPTER 1: THE OUTBREAK ---",
                        "The world didn't end with a war; it ended on a Tuesday afternoon.",
                        "You and your wife, Zara, were at the Grand Plaza Mall. Panic turned into a stampede.",
                        "In the chaos, her hand slipped from yours. You barely escaped the mall alive."
                    };
                    StartReading(paras);
                    return;
                }
                ShowChapterOptions();
            }
            else if (currentChapter == 2)
            {
                if (!chapterIntroPlayed)
                {
                    var paras = new[] {
                        "--- CHAPTER 2: THE IMMUNE CHILD ---",
                        "Months later. You meet your friend Usama, a researcher.",
                        "In the bunker, there is an immune child. Usama begs you to escort them to a Military Checkpoint."
                    };
                    StartReading(paras);
                    return;
                }
                ShowChapterOptions();
            }
            // Add more chapters as needed...
        }

        private void StartReading(string[] paras)
        {
            updateStatus(playerHealth, inventory, currentChapter);
            chapterParagraphs = new List<string>(paras);
            currentParagraphIndex = 0;
            isReading = true;
            NextParagraph();
        }

        public void NextParagraph()
        {
            if (currentParagraphIndex < chapterParagraphs.Count)
            {
                output(chapterParagraphs[currentParagraphIndex] + "\n");
                currentParagraphIndex++;
                if (currentParagraphIndex < chapterParagraphs.Count)
                    updateOptions("Continue...", null, null, null);
                else
                {
                    isReading = false;
                    chapterIntroPlayed = true;
                    if (currentChapter == 5) NextState();
                    else ShowChapterOptions();
                }
            }
        }

        private void ShowChapterOptions()
        {
            if (currentChapter == 1) updateOptions("Search the guard's body", "Flee through the emergency exit", "Check Status", null);
            else if (currentChapter == 2) updateOptions("Scavenge supplies", "Escort them", "Check Status", null);
            updateStatus(playerHealth, inventory, currentChapter);
        }

        public void Choose(int option)
        {
            if (!gameRunning) return;
            if (isReading) { if (option == 1) NextParagraph(); return; }

            if (currentChapter == 1)
            {
                if (option == 1)
                {
                    if (!inventory.Contains("Fire Axe"))
                    {
                        output("> Found a Fire Axe!\n");
                        inventory.Add("Fire Axe");
                    }
                }
                else if (option == 2)
                {
                    currentChapter = 2; chapterIntroPlayed = false;
                }
            }
            // Har action ke baad status update karna zaroori hai
            updateStatus(playerHealth, inventory, currentChapter);
            if (!chapterIntroPlayed) NextState();
        }

        public void SaveGame() { /* Implementation same as before */ }
        public bool LoadGame() { return false; /* Implementation same as before */ }
    }
}