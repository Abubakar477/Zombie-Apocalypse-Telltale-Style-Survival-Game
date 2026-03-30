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
                output("\n*** YOUR WOUNDS WERE TOO SEVERE. YOU HAVE DIED. ***");
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
                        "In the crushing chaos, her hand slipped from yours. You barely escaped the mall alive."
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
                        "Months later. You are ambushed, but saved by your friend Usama, a researcher.",
                        "In his bunker, there is an immune child. Usama begs you to escort them to a Military Checkpoint."
                    };
                    StartReading(paras);
                    return;
                }
                ShowChapterOptions();
            }
            else if (currentChapter == 3)
            {
                if (!chapterIntroPlayed)
                {
                    var paras = new[] {
                        "--- CHAPTER 3: BETRAYAL ---",
                        "Sarah sabotages the bunker doors just as explosions rock the city.",
                        "The ceiling is caving in. You must run!"
                    };
                    StartReading(paras);
                    return;
                }
                ShowChapterOptions();
            }
            else if (currentChapter == 4)
            {
                if (!chapterIntroPlayed)
                {
                    var paras = new[] {
                        "--- CHAPTER 4: THE TRAIN YARD ---",
                        "You burst into the train yard. A massive horde follows you.",
                        "Hold them back while Usama starts the train engine!"
                    };
                    StartReading(paras);
                    return;
                }
                ShowChapterOptions();
            }
            else if (currentChapter == 5)
            {
                if (!chapterIntroPlayed)
                {
                    var paras = new List<string> {
                        "==================================================",
                        "The massive diesel engine roars to life!",
                        "You pull yourself aboard, leaving the horde behind."
                    };

                    if (savedChild && trustedSarah)
                    {
                        paras.Add("You collapse, victorious. The child is perfectly safe.");
                        paras.Add($"*** TRUE SURVIVOR ENDING! FINAL HEALTH: {playerHealth} ***");
                    }
                    else if (savedChild && !trustedSarah)
                    {
                        paras.Add("You left the traitor, but secured the cure for humanity.");
                        paras.Add($"*** THE CURE ENDING! FINAL HEALTH: {playerHealth} ***");
                    }
                    else
                    {
                        paras.Add("The child was bitten. Humanity's last hope might be lost.");
                        paras.Add($"*** TRAGIC ESCAPE ENDING! FINAL HEALTH: {playerHealth} ***");
                    }
                    StartReading(paras.ToArray());
                    return;
                }
                updateOptions(null, null, null, null);
                updateStatus(playerHealth, inventory, currentChapter);
                gameRunning = false;
            }
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
            // --- NAYA FIX: Faltu status wala button nikal diya ---
            if (currentChapter == 1) updateOptions("Search the guard's body", "Flee through emergency exit", null, null);
            else if (currentChapter == 2) updateOptions("Scavenge medical supplies", "Escort them to checkpoint", null, null);
            else if (currentChapter == 3) updateOptions("Dig through the rubble", "Grab the child and run", "Use Medkit", null);
            else if (currentChapter == 4) updateOptions("Use inventory weapon", "Fight barehanded", "Use Medkit", null);

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
                        output("> You quickly search the body and find a Fire Axe!\n");
                        inventory.Add("Fire Axe");
                    }
                    else output("> Nothing left here.\n");
                }
                else if (option == 2)
                {
                    output("> You smash open the exit doors and escape.\n");
                    currentChapter = 2; chapterIntroPlayed = false;
                }
            }
            else if (currentChapter == 2)
            {
                if (option == 1)
                {
                    if (!inventory.Contains("Medkit"))
                    {
                        output("> You find a Medkit!\n");
                        inventory.Add("Medkit");
                    }
                    else output("> Nothing left here.\n");
                }
                else if (option == 2)
                {
                    output("> \"Pack your things,\" you tell her. \"We leave soon.\"\n");
                    currentChapter = 3; chapterIntroPlayed = false;
                }
            }
            else if (currentChapter == 3)
            {
                if (option == 1)
                {
                    output("> You try to dig her out. You show mercy.\n");
                    int dmg = rng.Next(20, 41); playerHealth -= dmg;
                    output($"[ You took {dmg} damage! ]\n");
                    trustedSarah = true;
                }
                else if (option == 2)
                {
                    output("> You grab the child and sprint up the ladder!\n");
                    currentChapter = 4; chapterIntroPlayed = false; trustedSarah = false;
                }
                else if (option == 3) UseMedkit();
            }
            else if (currentChapter == 4)
            {
                if (option == 1)
                {
                    if (inventory.Contains("Fire Axe"))
                    {
                        output("> You swing the Fire Axe and clear a path!\n");
                        int dmg = rng.Next(5, 16); playerHealth -= dmg;
                        output($"[ You took {dmg} damage. ]\n");
                        savedChild = true; currentChapter = 5; chapterIntroPlayed = false;
                    }
                    else
                    {
                        output("> You have no weapon! The horde swarms you!\n");
                        int dmg = rng.Next(40, 61); playerHealth -= dmg;
                        output($"[ You took a massive {dmg} damage! ]\n");
                        if (playerHealth > 0) { savedChild = false; currentChapter = 5; chapterIntroPlayed = false; }
                    }
                }
                else if (option == 2)
                {
                    output("> You fight barehanded in a desperate struggle.\n");
                    int dmg = rng.Next(50, 81); playerHealth -= dmg;
                    output($"[ You took a brutal {dmg} damage! ]\n");
                    if (playerHealth > 0) { savedChild = false; currentChapter = 5; chapterIntroPlayed = false; }
                }
                else if (option == 3) UseMedkit();
            }

            updateStatus(playerHealth, inventory, currentChapter);
            if (!chapterIntroPlayed && gameRunning) NextState();
        }

        private void UseMedkit()
        {
            if (inventory.Contains("Medkit"))
            {
                if (playerHealth == 100) { output("> Health is full!\n"); return; }
                output("> You use the Medkit and patch your wounds.\n");
                inventory.Remove("Medkit");
                playerHealth = Math.Min(100, playerHealth + 40);
                output($"[ Health restored to {playerHealth} ]\n");
            }
            else output("> You don't have a Medkit!\n");
        }

        public void SaveGame() { /* Save Logic same as before */ }
        public bool LoadGame() { return false; /* Load Logic same as before */ }
    }
}