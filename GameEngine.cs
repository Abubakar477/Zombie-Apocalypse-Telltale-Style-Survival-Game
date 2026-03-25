using System;
using System;
using System.Collections.Generic;

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

        // paragraph sequencing for chapter text
        private List<string> chapterParagraphs = new List<string>();
        private int currentParagraphIndex = 0;

        private readonly Action<string> output;
        private readonly Action<string, string, string, string> updateOptions;
        private readonly Action<int, List<string>, int> updateStatus; // No change needed

        public GameEngine(Action<string> outputCallback, Action<string, string, string, string> optionsCallback, Action<int, List<string>, int> statusCallback)
        {
            output = outputCallback ?? throw new ArgumentNullException(nameof(outputCallback));
            updateOptions = optionsCallback ?? throw new ArgumentNullException(nameof(optionsCallback));
            updateStatus = statusCallback ?? throw new ArgumentNullException(nameof(statusCallback)); // No change needed
        }

        public void Start()
        {
            output("==================================================");
            output("          ZOMBIE APOCALYPSE: SURVIVAL             ");
            output("==================================================");
            // initial status update
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

                    // output all paragraphs for the intro
                    foreach (var p in paras) output(p);

                    chapterIntroPlayed = true;
                }

                updateOptions("Search the guard's body", "Flee through the emergency exit", "Check Status/Inventory", null);
                updateStatus(playerHealth, inventory, currentChapter);
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

                    foreach (var p in paras) output(p);

                    chapterIntroPlayed = true;
                }

                updateOptions("Scavenge medical supplies", "Escort them to the checkpoint", "Check Status/Inventory", null);
                updateStatus(playerHealth, inventory, currentChapter);
            }
            else if (currentChapter == 3)
            {
                if (!chapterIntroPlayed)
                {
                    var paras = new[]
                    {
                        "--- CHAPTER 3: BETRAYAL AND FIRE FROM THE SKY ---\nTensions in the bunker reach a boiling point. Food is desperately low.",
                        "A technical engineer in your group, Sarah, has grown incredibly paranoid. She secretly sabotages the bunker's heavy steel blast doors.",
                        "Just as you discover her sabotage, the ground shakes violently. Military sirens begin to wail across the ruined city above. The government has initiated a Firebombing protocol. Deafen-ing explosions rock the bunker. The ceiling begins to cave in.",
                        "In the chaos, the technical woman tries to flee, but the collapsing rubble traps her. You, the medical friend, and the immune child must run for your lives as the bunker is destroyed!"
                    };

                    foreach (var p in paras) output(p);

                    chapterIntroPlayed = true;
                }

                updateOptions("Dig through the rubble", "Grab the child and run", "Use Medkit", "Check Status/Inventory");
                updateStatus(playerHealth, inventory, currentChapter);
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

                    foreach (var p in paras) output(p);

                    chapterIntroPlayed = true;
                }

                updateOptions("Use inventory weapon", "Fight barehanded", "Use Medkit", "Check Status/Inventory");
                updateStatus(playerHealth, inventory, currentChapter);
            }
            else if (currentChapter == 5)
            {
                output("==================================================");
                output("Just as a zombie lunges for the child, the massive diesel engine roars to life!");
                output("You pull yourself aboard. The train smashes through the barricades, leaving the horde behind.");
                output("You collapse on the floor, bleeding but victorious.");
                output($"*** YOU SURVIVED! FINAL HEALTH: {playerHealth} ***");
                updateOptions(null, null, null, null);
                updateStatus(playerHealth, inventory, currentChapter);
                gameRunning = false;
            }
        }

        // Show the next paragraph of the current chapter. When all paragraphs have
        // been shown, restore the chapter options so the player can choose actions.
        public void NextParagraph()
        {
            if (chapterParagraphs == null || chapterParagraphs.Count == 0) return;

            if (currentParagraphIndex < chapterParagraphs.Count)
            {
                output(chapterParagraphs[currentParagraphIndex]);
                currentParagraphIndex++;
                // if we've reached the end, show options for the chapter
                if (currentParagraphIndex >= chapterParagraphs.Count)
                {
                    // mark intro as played and enable options for this chapter
                    chapterIntroPlayed = true;
                    // call NextState to allow options to be displayed
                    NextState();
                }
            }
        }

        public void Choose(int option)
        {
            if (!gameRunning) return;

            if (currentChapter == 1)
            {
                if (option == 1)
                {
                    if (!inventory.Contains("Fire Axe"))
                    {
                        output("> You quickly search the body and find a heavy Fire Axe!");
                        inventory.Add("Fire Axe");
                    }
                    else
                    {
                        output("> You already searched here. There's nothing left.");
                    }
                }
                else if (option == 2)
                {
                    output("> Heartbroken but driven by survival, you smash open the exit doors and escape into the ruined streets.");
                    currentChapter = 2;
                    chapterIntroPlayed = false;
                }
                else if (option == 3)
                {
                    ShowStatus();
                }
            }
            else if (currentChapter == 2)
            {
                if (option == 1)
                {
                    if (!inventory.Contains("Medkit"))
                    {
                        output("> You find a First-Aid Medkit hidden in a cabinet.");
                        inventory.Add("Medkit");
                    }
                    else
                    {
                        output("> You have already gathered all useful supplies here.");
                    }
                }
                else if (option == 2)
                {
                    output("> You nod. \"Pack your things,\" you tell her. \"We leave soon.\"");
                    currentChapter = 3;
                    chapterIntroPlayed = false;
                }
                else if (option == 3)
                {
                    ShowStatus();
                }
            }
            else if (currentChapter == 3)
            {
                if (option == 1)
                {
                    output("> You waste precious time trying to dig her out. A falling beam strikes you!");
                    int damage = rng.Next(20, 41);
                    playerHealth -= damage;
                    output($"[ You took {damage} damage! Current Health: {playerHealth} ]");
                    output("> Realizing it's hopeless, you turn back.");
                }
                else if (option == 2)
                {
                    output("> You ignore the traitor, grab the child, and sprint up the emergency ladder just as the bunker caves in!");
                    currentChapter = 4;
                    chapterIntroPlayed = false;
                }
                else if (option == 3)
                {
                    UseMedkit();
                }
                else if (option == 4)
                {
                    ShowStatus();
                }
            }
            else if (currentChapter == 4)
            {
                if (option == 1)
                {
                    if (inventory.Contains("Fire Axe"))
                    {
                        output("> You swing the Fire Axe with brutal force! You cleave through the undead.");
                        int damage = rng.Next(5, 16);
                        playerHealth -= damage;
                        output($"[ You took {damage} damage. Current Health: {playerHealth} ]");
                        currentChapter = 5;
                    }
                    else
                    {
                        output("> You reach for a weapon, but your hands are empty! The horde swarms you!");
                        int damage = rng.Next(40, 61);
                        playerHealth -= damage;
                        output($"[ You took a massive {damage} damage! Current Health: {playerHealth} ]");
                        if (playerHealth > 0)
                        {
                            output("> Barely alive, you manage to push them back just in time.");
                            currentChapter = 5;
                        }
                    }
                }
                else if (option == 2)
                {
                    output("> You scream and charge the undead with your bare fists.");
                    int damage = rng.Next(50, 81);
                    playerHealth -= damage;
                    output($"[ You took a brutal {damage} damage! Current Health: {playerHealth} ]");
                    if (playerHealth > 0)
                    {
                        output("> By a miracle, you survive the onslaught long enough.");
                        currentChapter = 5;
                    }
                }
                else if (option == 3)
                {
                    UseMedkit();
                }
                else if (option == 4)
                {
                    ShowStatus();
                }
                else
                {
                    int damage = rng.Next(15, 31);
                    playerHealth -= damage;
                    output($"[ You took {damage} damage! Current Health: {playerHealth} ]");
                }
            }

            if (playerHealth <= 0)
            {
                output("*** YOUR WOUNDS WERE TOO SEVERE. YOU HAVE DIED. ***");
                updateOptions(null, null, null, null);
                gameRunning = false;
                return;
            }

            NextState();
        }

        private void ShowStatus()
        {
            output("=== PLAYER STATUS ===");
            output($"Health: {playerHealth} / 100");
            output("Inventory: " + (inventory.Count == 0 ? "Empty" : string.Join(", ", inventory)));
            output("=====================");
        }

        private void UseMedkit()
        {
            if (inventory.Contains("Medkit"))
            {
                if (playerHealth == 100)
                {
                    output("> Your health is already full. Don't waste supplies!");
                    return;
                }

                output("> You use the Medkit and patch your wounds.");
                inventory.Remove("Medkit");
                playerHealth += 40;
                if (playerHealth > 100) playerHealth = 100;
                output($"[ Health restored! Current Health: {playerHealth} ]");
            }
            else
            {
                output("> You don't have a Medkit in your inventory!");
            }
        }
    }
}
