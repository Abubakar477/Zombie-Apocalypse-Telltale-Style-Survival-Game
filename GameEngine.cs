using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Zombie_apocolypse_telltale
{
    public class GameStatus
    {
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Thirst { get; set; }
        public int Stamina { get; set; }
        public bool IsBleeding { get; set; }
        public bool IsInfected { get; set; }
        public List<string> Inventory { get; set; }
        public Dictionary<string, int> Components { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> WeaponDurability { get; set; } = new Dictionary<string, int>();
        public int Chapter { get; set; }
    }

    public class SaveData
    {
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Thirst { get; set; }
        public int Stamina { get; set; }
        public bool IsBleeding { get; set; }
        public bool IsInfected { get; set; }
        public List<string> Inventory { get; set; }
        public Dictionary<string, int> Components { get; set; }
        public Dictionary<string, int> WeaponDurability { get; set; }
        public int Chapter { get; set; }
        public bool ChapterIntroPlayed { get; set; }
        public bool TrustedSarah { get; set; }
        public bool SavedChild { get; set; }
        public bool GameRunning { get; set; }
    }

    class GameEngine
    {
        private int playerHealth = 100;
        private int playerHunger = 100;
        private int playerThirst = 100;
        private int playerStamina = 100;
        private bool isBleeding = false;
        private bool isInfected = false;

        private List<string> inventory = new List<string> { "Food Ration", "Water Bottle" };
        private Dictionary<string, int> components = new Dictionary<string, int>();
        private Dictionary<string, int> weaponDurability = new Dictionary<string, int>();

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
        private readonly Action<GameStatus> updateStatus;

        public GameEngine(Action<string> outputCallback, Action<string, string, string, string> optionsCallback, Action<GameStatus> statusCallback)
        {
            output = outputCallback ?? throw new ArgumentNullException(nameof(outputCallback));
            updateOptions = optionsCallback ?? throw new ArgumentNullException(nameof(optionsCallback));
            updateStatus = statusCallback ?? throw new ArgumentNullException(nameof(statusCallback));
        }

        private GameStatus GetStatus()
        {
            return new GameStatus
            {
                Health = playerHealth,
                Hunger = playerHunger,
                Thirst = playerThirst,
                Stamina = playerStamina,
                IsBleeding = isBleeding,
                IsInfected = isInfected,
                Inventory = new List<string>(inventory),
                Components = new Dictionary<string, int>(components),
                WeaponDurability = new Dictionary<string, int>(weaponDurability),
                Chapter = currentChapter
            };
        }

        private void DrainStats(int hungerDrain, int thirstDrain, int staminaDrain)
        {
            playerHunger = Math.Max(0, playerHunger - hungerDrain);
            playerThirst = Math.Max(0, playerThirst - thirstDrain);
            playerStamina = Math.Max(0, playerStamina - staminaDrain);
        }

        private void ProcessTurnEffects()
        {
            if (playerHunger == 0) playerHealth -= 5;
            if (playerThirst == 0) playerHealth -= 10;
            if (isBleeding)
            {
                playerHealth -= 8;
                output("[ BLEEDING: You lose 8 health! ]");
            }
            if (isInfected)
            {
                playerHealth -= 15;
                output("[ INFECTED: The infection spreads... You lose 15 health! ]");
            }
            // Slowly recover stamina over turns
            playerStamina = Math.Min(100, playerStamina + 10);
        }

        private void PushStatusUpdate()
        {
            updateStatus(GetStatus());
        }

        public void Start()
        {
            output("==================================================");
            output("           ZOMBIE APOCALYPSE: SURVIVAL            ");
            output("==================================================");
            PushStatusUpdate();
            NextState();
        }

        private void NextState()
        {
            if (playerHealth <= 0)
            {
                output("\n*** YOUR WOUNDS WERE TOO SEVERE. YOU HAVE DIED. ***");
                updateOptions(null, null, null, null);
                PushStatusUpdate();
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
                PushStatusUpdate();
                gameRunning = false;
            }
        }

        private void StartReading(string[] paras)
        {
            PushStatusUpdate();
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
            ProcessTurnEffects();
            if (playerHealth <= 0) { NextState(); return; }

            if (currentChapter == 1) updateOptions("Scavenge area", "Flee through emergency exit", "Use Item", null);
            else if (currentChapter == 2) updateOptions("Scavenge supplies", "Escort them to checkpoint", "Use Item", null);
            else if (currentChapter == 3) updateOptions("Dig through the rubble", "Grab the child and run", "Use Item", null);
            else if (currentChapter == 4) 
            {
                string combatOpt = weaponDurability.ContainsKey("Pipe") ? "Use Pipe" : "Fight barehanded";
                string stealthOpt = weaponDurability.ContainsKey("Shiv") ? "Stealth Kill (Shiv)" : "Sneak past";
                updateOptions(combatOpt, stealthOpt, "Use Item", null);
            }

            PushStatusUpdate();
        }

        public int CurrentChapter() => currentChapter;

        private void AddComponent(string name)
        {
            if (!components.ContainsKey(name)) components[name] = 0;
            components[name]++;
            output($"> You found: {name}\n");
        }

        private void AttemptScavenge(string location)
        {
            DrainStats(10, 15, 20); // Scavenging takes energy
            if (playerStamina < 20)
            {
                output("> You are too exhausted to scavenge effectively.\n");
                return;
            }

            int chance = rng.Next(1, 101);
            if (chance <= 40)
            {
                string[] parts = { "Rags", "Alcohol", "Blades", "Scrap", "Empty Bottle" };
                string part1 = parts[rng.Next(parts.Length)];
                string part2 = parts[rng.Next(parts.Length)];
                AddComponent(part1);
                if (rng.Next(100) > 50) AddComponent(part2);
            }
            else if (chance <= 55)
            {
                string[] loot = { "Food Ration", "Water Bottle", "Bandage" };
                string item = loot[rng.Next(loot.Length)];
                output($"> You found a {item}!\n");
                inventory.Add(item);
            }
            else if (chance <= 65 && !weaponDurability.ContainsKey("Pipe"))
            {
                output("> You search and find a sturdy Steel Pipe!\n");
                weaponDurability["Pipe"] = 3;
            }
            else if (chance <= 75)
            {
                output("> You found some Antibiotics!\n");
                inventory.Add("Antibiotics");
            }
            else if (chance <= 85)
            {
                output("> You search carefully but find nothing useful.\n");
            }
            else
            {
                // Ambush
                output("> You were ambushed while searching!\n");
                int dmg = rng.Next(10, 25);
                playerHealth -= dmg;
                output($"[ You took {dmg} damage! ]\n");
                if (rng.Next(1, 100) > 60)
                {
                    isBleeding = true;
                    output("[ You are BLEEDING! Find a bandage quickly. ]\n");
                }
            }
        }

        public void CraftItem(string recipe)
        {
            if (recipe == "Medkit")
            {
                if (components.ContainsKey("Rags") && components["Rags"] > 0 &&
                    components.ContainsKey("Alcohol") && components["Alcohol"] > 0)
                {
                    components["Rags"]--; components["Alcohol"]--;
                    inventory.Add("Medkit");
                    output($"> Crafted: Medkit\n");
                }
                else output("> Missing components for Medkit!\n");
            }
            else if (recipe == "Shiv")
            {
                if (components.ContainsKey("Blades") && components["Blades"] > 0 &&
                    components.ContainsKey("Scrap") && components["Scrap"] > 0)
                {
                    components["Blades"]--; components["Scrap"]--;
                    weaponDurability["Shiv"] = 1;
                    output($"> Crafted: Shiv\n");
                }
                else output("> Missing components for Shiv!\n");
            }
            else if (recipe == "Molotov")
            {
                if (components.ContainsKey("Rags") && components["Rags"] > 0 &&
                    components.ContainsKey("Alcohol") && components["Alcohol"] > 0 &&
                    components.ContainsKey("Empty Bottle") && components["Empty Bottle"] > 0)
                {
                    components["Rags"]--; components["Alcohol"]--; components["Empty Bottle"]--;
                    inventory.Add("Molotov");
                    output($"> Crafted: Molotov\n");
                }
                else output("> Missing components for Molotov!\n");
            }
            PushStatusUpdate();
        }

        public void Choose(int option)
        {
            if (!gameRunning) return;
            if (isReading) { if (option == 1) NextParagraph(); return; }

            if (currentChapter == 1)
            {
                if (option == 1)
                {
                    AttemptScavenge("Mall");
                }
                else if (option == 2)
                {
                    output("> You smash open the exit doors and escape.\n");
                    DrainStats(20, 20, 30);
                    currentChapter = 2; chapterIntroPlayed = false;
                }
                else if (option == 3) UseItem();
            }
            else if (currentChapter == 2)
            {
                if (option == 1)
                {
                    AttemptScavenge("Bunker");
                }
                else if (option == 2)
                {
                    output("> \"Pack your things,\" you tell her. \"We leave soon.\"\n");
                    DrainStats(15, 10, 20);
                    currentChapter = 3; chapterIntroPlayed = false;
                }
                else if (option == 3) UseItem();
            }
            else if (currentChapter == 3)
            {
                if (option == 1)
                {
                    output("> You try to dig her out. You show mercy.\n");
                    DrainStats(10, 10, 40);
                    if (playerStamina < 30) {
                        output("> You lack the stamina to dig efficiently. The roof collapses more!\n");
                        int dmg = rng.Next(25, 45); playerHealth -= dmg;
                        output($"[ You took {dmg} damage! ]\n");
                    } else {
                        output("> You haul her out successfully just in time.\n");
                        int dmg = rng.Next(5, 15); playerHealth -= dmg;
                        output($"[ You took {dmg} damage! ]\n");
                        trustedSarah = true;
                    }
                }
                else if (option == 2)
                {
                    output("> You grab the child and sprint up the ladder!\n");
                    DrainStats(25, 25, 50);
                    currentChapter = 4; chapterIntroPlayed = false; trustedSarah = false;
                }
                else if (option == 3) UseItem();
            }
            else if (currentChapter == 4)
            {
                if (option == 1)
                {
                    if (weaponDurability.ContainsKey("Pipe") && weaponDurability["Pipe"] > 0)
                    {
                        output("> You swing the Steel Pipe and crush skulls!\n");
                        int dmg = playerStamina > 40 ? rng.Next(5, 12) : rng.Next(15, 22);
                        playerHealth -= dmg;
                        output($"[ You took {dmg} damage. ]\n");
                        DrainStats(15, 15, 30);
                        weaponDurability["Pipe"]--;
                        if (weaponDurability["Pipe"] <= 0) { weaponDurability.Remove("Pipe"); output("> Your Pipe shattered!\n"); }
                        savedChild = true; currentChapter = 5; chapterIntroPlayed = false;
                    }
                    else
                    {
                        output("> You try to fight them head on, but you are unarmed!\n");
                        AttemptBarehandedFight();
                    }
                }
                else if (option == 2)
                {
                    if (weaponDurability.ContainsKey("Shiv") && weaponDurability["Shiv"] > 0)
                    {
                        output("> You silently execute the lead clicker with your Shiv!\n");
                        output("> The rest of the horde ignores you as you slip away safely.\n");
                        weaponDurability["Shiv"]--;
                        if (weaponDurability["Shiv"] <= 0) { weaponDurability.Remove("Shiv"); output("> Your Shiv snapped!\n"); }
                        DrainStats(5, 5, 15);
                        savedChild = true; currentChapter = 5; chapterIntroPlayed = false;
                    }
                    else
                    {
                        output("> You try to sneak past... but you step on glass!\n");
                        output("> The horde swarms you!\n");
                        int dmg = rng.Next(25, 45); playerHealth -= dmg;
                        output($"[ You took {dmg} damage! ]\n");
                        DrainStats(25, 20, 60);
                        if (playerHealth > 0) { savedChild = false; currentChapter = 5; chapterIntroPlayed = false; }
                    }
                }
                else if (option == 3) UseItem();
            }

            PushStatusUpdate();
            if (!chapterIntroPlayed && gameRunning && playerHealth > 0) NextState();
            else if (playerHealth <= 0) NextState(); // Die instantly
        }

        private void AttemptBarehandedFight()
        {
            output("> You fight barehanded in a desperate struggle.\n");
            DrainStats(30, 30, 80);
            int dmg = rng.Next(40, 71); 
            if (playerStamina < 20) dmg += 20; // Exhaustion penalty
            
            playerHealth -= dmg;
            output($"[ You took a brutal {dmg} damage! ]\n");
            
            if (rng.Next(1, 100) > 50) 
            {
                isBleeding = true;
                output("[ You are BLEEDING! ]\n");
            }
            if (rng.Next(1, 100) > 75)
            {
                isInfected = true;
                output("[ WARNING: YOU HAVE BEEN BITTEN. YOU ARE INFECTED! ]\n");
            }

            if (playerHealth > 0) { savedChild = false; currentChapter = 5; chapterIntroPlayed = false; }
        }

        private void UseItem()
        {
            if (inventory.Count == 0)
            {
                output("> Your inventory is empty.\n");
                return;
            }

            // A simplified item usage, tries to auto-consume the best item for current status
            if (isBleeding && inventory.Contains("Bandage"))
            {
                inventory.Remove("Bandage");
                isBleeding = false;
                output("> You use a Bandage. The bleeding stops.\n");
                return;
            }
            if (isInfected && inventory.Contains("Antibiotics"))
            {
                inventory.Remove("Antibiotics");
                isInfected = false;
                output("> You swallow Antibiotics. The infection recedes.\n");
                return;
            }
            if (playerHunger <= 50 && inventory.Contains("Food Ration"))
            {
                inventory.Remove("Food Ration");
                playerHunger = Math.Min(100, playerHunger + 40);
                output("> You eat a Food Ration. Hunger satiated.\n");
                return;
            }
            if (playerThirst <= 50 && inventory.Contains("Water Bottle"))
            {
                inventory.Remove("Water Bottle");
                playerThirst = Math.Min(100, playerThirst + 50);
                output("> You drink water. Thirst quenched.\n");
                return;
            }
            if (playerHealth < 100 && inventory.Contains("Medkit"))
            {
                inventory.Remove("Medkit");
                playerHealth = Math.Min(100, playerHealth + 50);
                output("> You use a Medkit. Wounds patched.\n");
                return;
            }

            // Fallbacks: If player just holds item but doesn't "need" it urgently, consume anyway to free space
            if (inventory.Contains("Food Ration"))
            {
                inventory.Remove("Food Ration");
                playerHunger = Math.Min(100, playerHunger + 40);
                output("> You eat a Food Ration.\n");
                return;
            }
            if (inventory.Contains("Water Bottle"))
            {
                inventory.Remove("Water Bottle");
                playerThirst = Math.Min(100, playerThirst + 50);
                output("> You drink water.\n");
                return;
            }
            
            output("> Currently holding no consumable items that you can use right now.\n");
        }

        public void SaveGame() 
        { 
            try
            {
                var data = new SaveData
                {
                    Health = playerHealth,
                    Hunger = playerHunger,
                    Thirst = playerThirst,
                    Stamina = playerStamina,
                    IsBleeding = isBleeding,
                    IsInfected = isInfected,
                    Inventory = new List<string>(inventory),
                    Components = new Dictionary<string, int>(components),
                    WeaponDurability = new Dictionary<string, int>(weaponDurability),
                    Chapter = currentChapter,
                    ChapterIntroPlayed = chapterIntroPlayed,
                    TrustedSarah = trustedSarah,
                    SavedChild = savedChild,
                    GameRunning = gameRunning
                };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("savegame.json", json);
                output("\n[ GAME SAVED SUCCESSFULLY ]\n");
            }
            catch (Exception ex)
            {
                output($"\n[ ERROR SAVING GAME: {ex.Message} ]\n");
            }
        }

        public bool LoadGame() 
        { 
            try
            {
                if (!File.Exists("savegame.json")) return false;

                string json = File.ReadAllText("savegame.json");
                var data = JsonSerializer.Deserialize<SaveData>(json);

                if (data != null)
                {
                    playerHealth = data.Health;
                    playerHunger = data.Hunger;
                    playerThirst = data.Thirst;
                    playerStamina = data.Stamina;
                    isBleeding = data.IsBleeding;
                    isInfected = data.IsInfected;
                    inventory = data.Inventory ?? new List<string>();
                    components = data.Components ?? new Dictionary<string, int>();
                    weaponDurability = data.WeaponDurability ?? new Dictionary<string, int>();
                    currentChapter = data.Chapter;
                    chapterIntroPlayed = data.ChapterIntroPlayed;
                    trustedSarah = data.TrustedSarah;
                    savedChild = data.SavedChild;
                    gameRunning = data.GameRunning;

                    output("\n[ GAME LOADED SUCCESSFULLY ]\n");
                    PushStatusUpdate();
                    NextState();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                output($"\n[ ERROR LOADING GAME: {ex.Message} ]\n");
                return false;
            }
        }
    }
}