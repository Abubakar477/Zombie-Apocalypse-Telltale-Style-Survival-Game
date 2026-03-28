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

        // Karma aur Consequences variables
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
                    var paras = new[]
                    {
                        "--- CHAPTER 1: THE OUTBREAK ---\nThe world didn't end with a war; it ended on a Tuesday afternoon.",
                        "You and your wife, Zara, were at the Grand Plaza Mall, picking out a gift. The crowd was dense. Suddenly, a piercing scream shattered the air near the food court.",
                        "Within seconds, the panic turned into a stampede. People weren't just running; they were being hunted. A highly aggressive strain of the undead virus had hit the city center.",
                        "You grabbed Zara's hand, fighting through the terrified mob towards the exit. But the infected were incredibly fast. A horde broke through the glass doors ahead of you.",
                        "In the crushing chaos of the panicked crowd, her hand slipped from yours. You fought like a madman to reach her, but it was too late. The infected overwhelmed the area. Heartbroken, traumatized, and forced by pure survival instinct, you barely escaped the mall alive."
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
                    var paras = new[]
                    {
                        "--- CHAPTER 2: THE IMMUNE CHILD & THE BUNKER ---\nMonths later. The city is a ruin, heavily infested. You are a lone, hardened survivor.",
                        "While scavenging a ruined pharmacy, you are ambushed by three infected. Just as you run out of ammo, a suppressed gunshot drops the closest zombie.",
                        "You turn to see your old friend, a brilliant medical researcher. She leads you safely to a hidden, heavily reinforced underground bunker. Inside the dimly lit bunker, you meet her small group of survivors.",
                        "In the corner sits a young child, looking perfectly healthy. He is immune. She explains that his blood is the key to synthesizing a vaccine, but the bunker lacks the equipment. She begs you to escort them to a rumored Military Scientific Checkpoint in the East."
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
                    var paras = new[]
                    {
                        "--- CHAPTER 3: BETRAYAL AND FIRE FROM THE SKY ---\nTensions in the bunker reach a boiling point. Food is desperately low.",
                        "A technical engineer in your group, Sarah, has grown incredibly paranoid. She secretly sabotages the bunker's heavy steel blast doors.",
                        "Just as you discover her sabotage, the ground shakes violently. Military sirens begin to wail across the ruined city above. The government has initiated a Firebombing protocol. Deafening explosions rock the bunker. The ceiling begins to cave in.",
                        "In the chaos, the technical woman tries to flee, but the collapsing rubble traps her. You, the medical friend, and the immune child must run for your lives as the bunker is destroyed!"
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
                    var paras = new[]
                    {
                        "--- CHAPTER 4: THE TRAIN YARD ESCAPE ---\nYou burst out of the bunker's emergency hatch into a nightmare. The city is burning, and the explosions have drawn a massive horde of the undead directly towards you.",
                        "You stumble into the old, abandoned central train yard. Sitting on the main track is a massive, heavy-duty industrial train engine. The horde breaches the train yard gates. Your medical friend yells she can hotwire it but needs time.",
                        "You must hold the horde back and protect the child. This is it. You have to protect the child and the engineer as she starts the engine."
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
                    var paras = new List<string>
                    {
                        "==================================================",
                        "Just as a zombie lunges for the child, the massive diesel engine roars to life!",
                        "You pull yourself aboard. The train smashes through the barricades, leaving the horde behind."
                    };

                    if (savedChild && trustedSarah)
                    {
                        paras.Add("You collapse on the floor, bleeding but victorious. The child is perfectly safe.");
                        paras.Add("Suddenly, the radio crackles. It's Sarah. She survived the rubble because you tried to dig her out.");
                        paras.Add("She guides your train to a hidden military outpost. Your mercy paid off.");
                        paras.Add($"*** TRUE SURVIVOR ENDING! FINAL HEALTH: {playerHealth} ***");
                    }
                    else if (savedChild && !trustedSarah)
                    {
                        paras.Add("You collapse on the floor, bleeding but victorious. The child is safe.");
                        paras.Add("You look back at the burning city. You left the traitor to die, but you secured the cure for humanity.");
                        paras.Add($"*** THE CURE ENDING! FINAL HEALTH: {playerHealth} ***");
                    }
                    else
                    {
                        paras.Add("You sit in silence as the train rolls on. The child lies quietly in the corner, a fresh bite mark on his arm.");
                        paras.Add("He was immune to the airborne strain, but a direct bite... you don't know if he will survive.");
                        paras.Add("Humanity's last hope might be lost, despite your escape.");
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
                {
                    updateOptions("Continue...", null, null, null);
                }
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
            if (currentChapter == 1) updateOptions("Search the guard's body", "Flee through the emergency exit", "Check Status/Inventory", null);
            else if (currentChapter == 2) updateOptions("Scavenge medical supplies", "Escort them to the checkpoint", "Check Status/Inventory", null);
            else if (currentChapter == 3) updateOptions("Dig through the rubble", "Grab the child and run", "Use Medkit", "Check Status/Inventory");
            else if (currentChapter == 4) updateOptions("Use inventory weapon", "Fight barehanded", "Use Medkit", "Check Status/Inventory");

            updateStatus(playerHealth, inventory, currentChapter);
        }

        public void Choose(int option)
        {
            if (!gameRunning) return;

            if (isReading)
            {
                if (option == 1) NextParagraph();
                return;
            }

            if (currentChapter == 1)
            {
                if (option == 1)
                {
                    if (!inventory.Contains("Fire Axe"))
                    {
                        output("> You quickly search the body and find a heavy Fire Axe!\n");
                        inventory.Add("Fire Axe");
                    }
                    else output("> You already searched here. There's nothing left.\n");
                }
                else if (option == 2)
                {
                    output("> Heartbroken but driven by survival, you smash open the exit doors and escape into the ruined streets.\n");
                    currentChapter = 2;
                    chapterIntroPlayed = false;
                }
                else if (option == 3) ShowStatus();
            }
            else if (currentChapter == 2)
            {
                if (option == 1)
                {
                    if (!inventory.Contains("Medkit"))
                    {
                        output("> You find a First-Aid Medkit hidden in a cabinet.\n");
                        inventory.Add("Medkit");
                    }
                    else output("> You have already gathered all useful supplies here.\n");
                }
                else if (option == 2)
                {
                    output("> You nod. \"Pack your things,\" you tell her. \"We leave soon.\"\n");
                    currentChapter = 3;
                    chapterIntroPlayed = false;
                }
                else if (option == 3) ShowStatus();
            }
            else if (currentChapter == 3)
            {
                if (option == 1)
                {
                    output("> You waste precious time trying to dig her out. You show mercy to the traitor.\n");
                    int damage = rng.Next(20, 41);
                    playerHealth -= damage;
                    output($"[ You took {damage} damage! Current Health: {playerHealth} ]\n");
                    output("> She manages to slip away, leaving you behind. Realizing it's hopeless, you turn back.\n");
                    trustedSarah = true;
                }
                else if (option == 2)
                {
                    output("> You ignore the traitor, grab the child, and sprint up the emergency ladder just as the bunker caves in!\n");
                    output("> You hear her screams fade as the concrete seals her fate. She will remember that... if she survives.\n");
                    currentChapter = 4;
                    chapterIntroPlayed = false;
                    trustedSarah = false;
                }
                else if (option == 3) UseMedkit();
                else if (option == 4) ShowStatus();
            }
            else if (currentChapter == 4)
            {
                if (option == 1)
                {
                    if (inventory.Contains("Fire Axe"))
                    {
                        output("> You swing the Fire Axe with brutal force! You cleave a path through the undead, ensuring the child gets on board safely.\n");
                        int damage = rng.Next(5, 16);
                        playerHealth -= damage;
                        output($"[ You took {damage} damage. Current Health: {playerHealth} ]\n");
                        savedChild = true;
                        currentChapter = 5;
                        chapterIntroPlayed = false;
                    }
                    else
                    {
                        output("> You reach for a weapon, but your hands are empty! The horde swarms you!\n");
                        int damage = rng.Next(40, 61);
                        playerHealth -= damage;
                        output($"[ You took a massive {damage} damage! Current Health: {playerHealth} ]\n");
                        if (playerHealth > 0)
                        {
                            output("> Barely alive, you manage to push them back just in time, but the child gets bitten in the chaos.\n");
                            savedChild = false;
                            currentChapter = 5;
                            chapterIntroPlayed = false;
                        }
                    }
                }
                else if (option == 2)
                {
                    output("> You scream and charge the undead with your bare fists. It's a desperate struggle.\n");
                    int damage = rng.Next(50, 81);
                    playerHealth -= damage;
                    output($"[ You took a brutal {damage} damage! Current Health: {playerHealth} ]\n");
                    if (playerHealth > 0)
                    {
                        output("> By a miracle, you survive the onslaught, but in the chaos, the child is bitten before you can pull him aboard.\n");
                        savedChild = false;
                        currentChapter = 5;
                        chapterIntroPlayed = false;
                    }
                }
                else if (option == 3) UseMedkit();
                else if (option == 4) ShowStatus();
            }

            if (playerHealth <= 0)
            {
                output("*** YOUR WOUNDS WERE TOO SEVERE. YOU HAVE DIED. ***\n");
                updateOptions(null, null, null, null);
                gameRunning = false;
                return;
            }

            if (chapterIntroPlayed == false && gameRunning) NextState();
        }

        private void ShowStatus()
        {
            output("=== PLAYER STATUS ===");
            output($"Health: {playerHealth} / 100");
            output("Inventory: " + (inventory.Count == 0 ? "Empty" : string.Join(", ", inventory)));
            output("=====================\n");
        }

        private void UseMedkit()
        {
            if (inventory.Contains("Medkit"))
            {
                if (playerHealth == 100)
                {
                    output("> Your health is already full. Don't waste supplies!\n");
                    return;
                }

                output("> You use the Medkit and patch your wounds.\n");
                inventory.Remove("Medkit");
                playerHealth += 40;
                if (playerHealth > 100) playerHealth = 100;
                output($"[ Health restored! Current Health: {playerHealth} ]\n");
            }
            else
            {
                output("> You don't have a Medkit in your inventory!\n");
            }
        }

        public void SaveGame()
        {
            try
            {
                var saveData = new Dictionary<string, object>
                {
                    { "Health", playerHealth },
                    { "Chapter", currentChapter },
                    { "Inventory", inventory },
                    { "TrustedSarah", trustedSarah },
                    { "SavedChild", savedChild }
                };

                string jsonString = JsonSerializer.Serialize(saveData);
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");
                File.WriteAllText(filePath, jsonString);

                output("\n[ System: Game Progress Saved Successfully! ]\n");
            }
            catch (Exception ex)
            {
                output($"\n[ System Error: Could not save game - {ex.Message} ]\n");
            }
        }

        public bool LoadGame()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var loadData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                    playerHealth = int.Parse(loadData["Health"].ToString());
                    currentChapter = int.Parse(loadData["Chapter"].ToString());

                    var invElement = (JsonElement)loadData["Inventory"];
                    inventory.Clear();
                    foreach (var item in invElement.EnumerateArray()) inventory.Add(item.GetString());

                    if (loadData.ContainsKey("TrustedSarah")) trustedSarah = bool.Parse(loadData["TrustedSarah"].ToString());
                    if (loadData.ContainsKey("SavedChild")) savedChild = bool.Parse(loadData["SavedChild"].ToString());

                    chapterIntroPlayed = false;
                    isReading = false;
                    gameRunning = true;

                    output("\n[ System: Game Loaded Successfully! Welcome back. ]\n");
                    updateStatus(playerHealth, inventory, currentChapter);
                    NextState();

                    return true;
                }
                return false;
            }
            catch
            {
                output("\n[ System Error: Corrupt save file or loading failed. ]\n");
                return false;
            }
        }
    }
}