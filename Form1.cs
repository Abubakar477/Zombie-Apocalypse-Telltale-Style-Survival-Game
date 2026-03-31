using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Media;
using System.Threading.Tasks;
using System.Media;
using System.Reflection;

namespace Zombie_apocolypse_telltale
{
    public partial class Form1 : Form
    {
        private GameEngine engine;
        private readonly Dictionary<int, Image> chapterImages = new Dictionary<int, Image>();
        private int lastChapter = 0;
        private SoundPlayer bgMusic;
        private Panel autoStartPanel;
        private Panel bottomBox;

        private Label lblHealthHeader;
        private Label lblInventoryHeader;
        private Label lblHungerHeader;
        private ProgressBar pbHunger;
        private Label lblThirstHeader;
        private ProgressBar pbThirst;
        private Label lblStaminaHeader;
        private ProgressBar pbStamina;
        private Label lblStatusEffects;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Load -= Form1_Load;
            this.Load += Form1_Load;
            engine = new GameEngine(AppendOutput, UpdateOptions, UpdateStatus);
        }

        private string GetImagePath(string imageNameWithoutExt)
        {
            string[] extensions = { ".jpg", ".png", ".jpeg" };
            foreach (var ext in extensions)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", imageNameWithoutExt + ext);
                if (File.Exists(path)) return path;
            }
            return null;
        }

        private void ShakeScreen()
        {
            Point originalLocation = this.Location;
            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
                this.Location = new Point(originalLocation.X + rnd.Next(-15, 15), originalLocation.Y + rnd.Next(-15, 15));
                System.Threading.Thread.Sleep(30);
                Application.DoEvents();
            }
            this.Location = originalLocation;
        }

        private void SetupModernUI()
        {
            if (pbScene != null) pbScene.Visible = false;

            this.BackColor = Color.Black;

            bottomBox = new Panel();
            bottomBox.BackColor = Color.FromArgb(15, 15, 15);
            this.Controls.Add(bottomBox);

            foreach (Control c in this.Controls)
            {
                if (c is Panel && c.Name != "autoStartPanel" && c.Name != bottomBox.Name)
                    c.BackColor = Color.Transparent;

                if (c is Label && c.Name != "lblTitle" && c.Name != "lblChapter")
                {
                    c.Visible = false; 
                }
            }

            lblHealthHeader = new Label() { Text = "HEALTH", ForeColor = Color.LightGreen, Font = new Font("Consolas", 10F, FontStyle.Bold), BackColor = Color.FromArgb(150, 0, 0, 0), AutoSize = true };
            this.Controls.Add(lblHealthHeader);
            lblHealthHeader.BringToFront();

            lblHungerHeader = new Label() { Text = "HUNGER", ForeColor = Color.LightYellow, Font = new Font("Consolas", 10F, FontStyle.Bold), BackColor = Color.FromArgb(150, 0, 0, 0), AutoSize = true };
            this.Controls.Add(lblHungerHeader); lblHungerHeader.BringToFront();
            pbHunger = new ProgressBar(); this.Controls.Add(pbHunger); pbHunger.BringToFront();

            lblThirstHeader = new Label() { Text = "THIRST", ForeColor = Color.LightSkyBlue, Font = new Font("Consolas", 10F, FontStyle.Bold), BackColor = Color.FromArgb(150, 0, 0, 0), AutoSize = true };
            this.Controls.Add(lblThirstHeader); lblThirstHeader.BringToFront();
            pbThirst = new ProgressBar(); this.Controls.Add(pbThirst); pbThirst.BringToFront();

            lblStaminaHeader = new Label() { Text = "STAMINA", ForeColor = Color.Orange, Font = new Font("Consolas", 10F, FontStyle.Bold), BackColor = Color.FromArgb(150, 0, 0, 0), AutoSize = true };
            this.Controls.Add(lblStaminaHeader); lblStaminaHeader.BringToFront();
            pbStamina = new ProgressBar(); this.Controls.Add(pbStamina); pbStamina.BringToFront();

            lblStatusEffects = new Label() { Text = "", ForeColor = Color.Red, Font = new Font("Consolas", 12F, FontStyle.Bold), BackColor = Color.FromArgb(150, 0, 0, 0), AutoSize = true };
            this.Controls.Add(lblStatusEffects); lblStatusEffects.BringToFront();

            lblInventoryHeader = new Label() { Text = "INVENTORY", ForeColor = Color.LightGreen, Font = new Font("Consolas", 10F, FontStyle.Bold), BackColor = Color.FromArgb(150, 0, 0, 0), AutoSize = true };
            this.Controls.Add(lblInventoryHeader);
            lblInventoryHeader.BringToFront();

            Button[] options = { btnOption1, btnOption2, btnOption3, btnOption4, btnContinue };
            foreach (var btn in options)
            {
                if (btn == null) continue;
                btn.Parent = bottomBox;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.BackColor = Color.FromArgb(25, 25, 25);
                btn.ForeColor = Color.LightGreen;
                btn.Cursor = Cursors.Hand;
                btn.Font = new Font("Consolas", 10F, FontStyle.Bold);
            }

            // =========================================================
            // UPDATE: System Buttons ko Bottom Box mein shift kiya
            // =========================================================
            Button[] topBtns = { btnSave, btnLoad, btnRestart };
            foreach (var btn in topBtns)
            {
                if (btn == null) continue;
                btn.Parent = bottomBox; // Inko oopar se neechay bottom box mein daal diya
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = Color.FromArgb(30, 30, 30);
                btn.ForeColor = Color.LightSkyBlue; // Inka color thora alag rakha taake options se mix na hon
                btn.Font = new Font("Consolas", 9F, FontStyle.Bold);
                btn.Cursor = Cursors.Hand;
            }
            
            // Add custom Craft button
            Button btnCraft = new Button();
            btnCraft.Name = "btnCraft";
            btnCraft.Text = "CRAFTING";
            btnCraft.Parent = bottomBox;
            btnCraft.FlatStyle = FlatStyle.Flat;
            btnCraft.BackColor = Color.DarkGoldenrod;
            btnCraft.ForeColor = Color.White;
            btnCraft.Font = new Font("Consolas", 9F, FontStyle.Bold);
            btnCraft.Cursor = Cursors.Hand;
            btnCraft.Click += BtnCraft_Click;
            this.Controls.Add(btnCraft);
            btnCraft.BringToFront();

            if (rtbOutput != null)
            {
                rtbOutput.Parent = bottomBox;
                rtbOutput.BorderStyle = BorderStyle.None;
                rtbOutput.BackColor = Color.FromArgb(15, 15, 15);
                
                // =========================================================
                // UPDATE: Paragraph ka Font aur Color change kiya
                // =========================================================
                rtbOutput.ForeColor = Color.WhiteSmoke; // Pyara sa off-white color
                rtbOutput.Font = new Font("Verdana", 11F, FontStyle.Regular); // Naya khula-khula font
            }

            if (lstInventory != null)
            {
                lstInventory.BorderStyle = BorderStyle.FixedSingle;
                lstInventory.BackColor = Color.FromArgb(20, 20, 20);
                lstInventory.ForeColor = Color.White;
            }

            // Start Screen ke liye initially sab hide karna
            if (bottomBox != null) bottomBox.Visible = false;
            if (lblHealthHeader != null) lblHealthHeader.Visible = false;
            if (lblInventoryHeader != null) lblInventoryHeader.Visible = false;
            if (pbHealth != null) pbHealth.Visible = false;
            if (lstInventory != null) lstInventory.Visible = false;
            if (lblChapter != null) lblChapter.Visible = false;
            if (lblHungerHeader != null) lblHungerHeader.Visible = false;
            if (pbHunger != null) pbHunger.Visible = false;
            if (lblThirstHeader != null) lblThirstHeader.Visible = false;
            if (pbThirst != null) pbThirst.Visible = false;
            if (lblStaminaHeader != null) lblStaminaHeader.Visible = false;
            if (pbStamina != null) pbStamina.Visible = false;
            if (lblStatusEffects != null) lblStatusEffects.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupModernUI();
            LoadImagesFromFolder();

            this.Resize += (s, ev) => {

                int w = this.ClientSize.Width;
                int h = this.ClientSize.Height;

                if (bottomBox != null)
                {
                    bottomBox.Width = w;
                    bottomBox.Height = 220;
                    bottomBox.Left = 0;
                    bottomBox.Top = h - 220;
                }

                if (rtbOutput != null)
                {
                    rtbOutput.Dock = DockStyle.None;
                    rtbOutput.Left = 10;
                    rtbOutput.Top = 15;
                    rtbOutput.Width = w - 20;
                    rtbOutput.Height = 100;
                }

                int btnW = 350;
                int btnH = 35;
                int gap = 20;

                int startX = (w - (btnW * 2) - gap) / 2;
                int startY = 125;

                if (btnOption1 != null) btnOption1.Bounds = new Rectangle(startX, startY, btnW, btnH);
                if (btnOption2 != null) btnOption2.Bounds = new Rectangle(startX + btnW + gap, startY, btnW, btnH);
                if (btnOption3 != null) btnOption3.Bounds = new Rectangle(startX, startY + btnH + 10, btnW, btnH);
                if (btnOption4 != null) btnOption4.Bounds = new Rectangle(startX + btnW + gap, startY + btnH + 10, btnW, btnH);

                if (btnContinue != null) btnContinue.Bounds = new Rectangle((w - 300) / 2, startY + 10, 300, 45);

                // =========================================================
                // UPDATE: System Buttons ko Bottom Box ke neechay right side par set kiya
                // =========================================================
                int sysBtnY = 175; // Bottom box ke andar neechay wali jagah
                if (btnSave != null) btnSave.Bounds = new Rectangle(w - 270, sysBtnY, 80, 30);
                if (btnLoad != null) btnLoad.Bounds = new Rectangle(w - 180, sysBtnY, 80, 30);
                if (btnRestart != null) btnRestart.Bounds = new Rectangle(w - 90, sysBtnY, 80, 30);
                
                Button btnCraft = this.Controls.Find("btnCraft", true).FirstOrDefault() as Button;
                if (btnCraft != null) btnCraft.Bounds = new Rectangle(w - 360, sysBtnY, 80, 30);

                int currentY = 20;
                int leftPad = 20; 

                if (lblChapter != null)
                {
                    lblChapter.Left = leftPad;
                    lblChapter.Top = currentY;
                    lblChapter.BackColor = Color.FromArgb(150, 0, 0, 0); 
                    currentY += 40; 
                }

                if (lblStatusEffects != null)
                {
                    lblStatusEffects.Left = leftPad;
                    lblStatusEffects.Top = currentY;
                    currentY += 25; 
                }

                if (lblHealthHeader != null)
                {
                    lblHealthHeader.Left = leftPad;
                    lblHealthHeader.Top = currentY;
                    currentY += 20; 
                }
                if (pbHealth != null)
                {
                    pbHealth.Left = leftPad;
                    pbHealth.Top = currentY;
                    pbHealth.Width = 150;
                    pbHealth.Height = 15;
                    currentY += 25; 
                }

                if (lblHungerHeader != null) { lblHungerHeader.Left = leftPad; lblHungerHeader.Top = currentY; currentY += 20; }
                if (pbHunger != null) { pbHunger.Left = leftPad; pbHunger.Top = currentY; pbHunger.Width = 150; pbHunger.Height = 15; currentY += 25; }

                if (lblThirstHeader != null) { lblThirstHeader.Left = leftPad; lblThirstHeader.Top = currentY; currentY += 20; }
                if (pbThirst != null) { pbThirst.Left = leftPad; pbThirst.Top = currentY; pbThirst.Width = 150; pbThirst.Height = 15; currentY += 25; }

                if (lblStaminaHeader != null) { lblStaminaHeader.Left = leftPad; lblStaminaHeader.Top = currentY; currentY += 20; }
                if (pbStamina != null) { pbStamina.Left = leftPad; pbStamina.Top = currentY; pbStamina.Width = 150; pbStamina.Height = 15; currentY += 25; }

                if (lblInventoryHeader != null)
                {
                    lblInventoryHeader.Left = leftPad;
                    lblInventoryHeader.Top = currentY;
                    currentY += 20; 
                }

                if (lstInventory != null)
                {
                    lstInventory.Left = leftPad;
                    lstInventory.Top = currentY;
                    lstInventory.Width = 150;
                    lstInventory.Height = 120; 
                }

                if (bottomBox != null) bottomBox.BringToFront();
                if (autoStartPanel != null) autoStartPanel.BringToFront();
            };
            this.OnResize(EventArgs.Empty);

            autoStartPanel = new Panel();
            autoStartPanel.Dock = DockStyle.Fill;

            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, autoStartPanel, new object[] { true });

            try
            {
                string mainBgPath = GetImagePath("za");
                if (mainBgPath != null)
                {
                    autoStartPanel.BackgroundImage = Image.FromFile(mainBgPath);
                    autoStartPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    autoStartPanel.BackColor = Color.Black;
                }
            }
            catch { autoStartPanel.BackColor = Color.Black; }

            Label lblTitle = new Label();
            lblTitle.Text = "ZOMBIE APOCALYPSE\nSURVIVAL";
            lblTitle.ForeColor = Color.DarkRed;
            lblTitle.Font = new Font("Consolas", 32F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Text = "";

            Button btnPlay = new Button();
            btnPlay.Text = "START GAME";
            btnPlay.Font = new Font("Consolas", 16F, FontStyle.Bold);
            btnPlay.ForeColor = Color.White;
            btnPlay.BackColor = Color.FromArgb(40, 40, 40);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.Size = new Size(250, 60);
            btnPlay.Cursor = Cursors.Hand;
            btnPlay.Visible = false;

            // Cool Typewriter Intro
            _ = PlayIntroAsync(lblTitle, "ZOMBIE APOCALYPSE\nSURVIVAL", btnPlay);

            btnPlay.Click += (s, ev) => {
                autoStartPanel.Visible = false;
                
                if (bottomBox != null) bottomBox.Visible = true;
                if (lblHealthHeader != null) lblHealthHeader.Visible = true;
                if (lblInventoryHeader != null) lblInventoryHeader.Visible = true;
                if (pbHealth != null) pbHealth.Visible = true;
                if (lstInventory != null) lstInventory.Visible = true;
                if (lblChapter != null) lblChapter.Visible = true;
                if (lblHungerHeader != null) lblHungerHeader.Visible = true;
                if (pbHunger != null) pbHunger.Visible = true;
                if (lblThirstHeader != null) lblThirstHeader.Visible = true;
                if (pbThirst != null) pbThirst.Visible = true;
                if (lblStaminaHeader != null) lblStaminaHeader.Visible = true;
                if (pbStamina != null) pbStamina.Visible = true;
                if (lblStatusEffects != null) lblStatusEffects.Visible = true;

                engine.Start();
            };

            autoStartPanel.Controls.Add(lblTitle);
            autoStartPanel.Controls.Add(btnPlay);
            this.Controls.Add(autoStartPanel);
            autoStartPanel.BringToFront();

            autoStartPanel.Resize += (s, ev) => {
                lblTitle.Left = (autoStartPanel.Width - lblTitle.Width) / 2;
                lblTitle.Top = (autoStartPanel.Height / 2) - lblTitle.Height - 20;
                btnPlay.Left = (autoStartPanel.Width - btnPlay.Width) / 2;
                btnPlay.Top = (autoStartPanel.Height / 2) + 40;
            };
        }

        private async Task PlayIntroAsync(Label lblTitle, string text, Button btnPlay)
        {
            foreach (char c in text)
            {
                lblTitle.Text += c;
                await Task.Delay(50); // Speed of typewriter
            }
            await Task.Delay(500);
            btnPlay.Visible = true;
        }

        private void AppendOutput(string text)
        {
            if (InvokeRequired) { Invoke(new Action(() => AppendOutput(text))); return; }

            // Default color WhiteSmoke rakhi hai taake naye font ke mutabiq chalay
            Color textColor = Color.WhiteSmoke; 
            if (text.Contains("damage") || text.Contains("DIED") || text.Contains("hopeless") || text.Contains("blood"))
            {
                textColor = Color.LightCoral;
                ShakeScreen();
            }
            else if (text.Contains("restored") || text.Contains("SURVIVED") || text.Contains("Medkit"))
            {
                textColor = Color.LightGreen;
            }

            rtbOutput.SelectionStart = rtbOutput.TextLength;
            rtbOutput.SelectionLength = 0;
            rtbOutput.SelectionColor = textColor;

            rtbOutput.AppendText(text + Environment.NewLine + Environment.NewLine);
            rtbOutput.ScrollToCaret();
        }

        private void UpdateStatus(GameStatus status)
        {
            if (InvokeRequired) { Invoke(new Action(() => UpdateStatus(status))); return; }

            pbHealth.Value = Math.Max(0, Math.Min(100, status.Health));
            if (pbHunger != null) pbHunger.Value = Math.Max(0, Math.Min(100, status.Hunger));
            if (pbThirst != null) pbThirst.Value = Math.Max(0, Math.Min(100, status.Thirst));
            if (pbStamina != null) pbStamina.Value = Math.Max(0, Math.Min(100, status.Stamina));

            lstInventory.Items.Clear();
            foreach (var kvp in status.Components)
            {
                lstInventory.Items.Add($"{kvp.Key} x{kvp.Value}");
            }
            foreach (var kvp in status.WeaponDurability)
            {
                lstInventory.Items.Add($"{kvp.Key} [{kvp.Value}]");
            }
            // Add normal inventory items (Medkit, etc)
            foreach (var item in status.Inventory)
            {
                lstInventory.Items.Add(item);
            }

            if (lblChapter != null) lblChapter.Text = $"Chapter: {status.Chapter}";

            string fx = "";
            if (status.IsBleeding) fx += "[BLEEDING] ";
            if (status.IsInfected) fx += "[INFECTED]";
            lblStatusEffects.Text = fx;

            if (status.Chapter != lastChapter)
            {
                rtbOutput.Clear();
                PlayBackgroundMusic($"chapter{status.Chapter}.wav");

                try
                {
                    string chapImgPath = GetImagePath($"chapter{status.Chapter}");
                    if (chapImgPath != null)
                    {
                        this.BackgroundImage = Image.FromFile(chapImgPath);
                        this.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                    else
                    {
                        this.BackgroundImage = null;
                        this.BackColor = Color.Black;
                    }
                }
                catch { }

                lastChapter = status.Chapter;
            }
        }

        private void PlayBackgroundMusic(string fileName)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", fileName);
                if (File.Exists(path))
                {
                    if (bgMusic != null) { bgMusic.Stop(); bgMusic.Dispose(); }
                    bgMusic = new SoundPlayer(path);
                    bgMusic.PlayLooping();
                }
            }
            catch { }
        }

        private void LoadImagesFromFolder() { /* Logic unchanged */ }

        private void rtbOutput_TextChanged(object sender, EventArgs e) { }
        private void btnStart_Click(object sender, EventArgs e) { }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            engine = new GameEngine(AppendOutput, UpdateOptions, UpdateStatus);
            rtbOutput.Clear();
            if (bgMusic != null) bgMusic.Stop();
            
            if (bottomBox != null) bottomBox.Visible = false;
            if (pbHealth != null) pbHealth.Visible = false;
            if (lstInventory != null) lstInventory.Visible = false;
            if (lblChapter != null) lblChapter.Visible = false;
            if (lblHealthHeader != null) lblHealthHeader.Visible = false;
            if (lblInventoryHeader != null) lblInventoryHeader.Visible = false;
            if (lblHungerHeader != null) lblHungerHeader.Visible = false;
            if (pbHunger != null) pbHunger.Visible = false;
            if (lblThirstHeader != null) lblThirstHeader.Visible = false;
            if (pbThirst != null) pbThirst.Visible = false;
            if (lblStaminaHeader != null) lblStaminaHeader.Visible = false;
            if (pbStamina != null) pbStamina.Visible = false;
            if (lblStatusEffects != null) lblStatusEffects.Visible = false;

            if (autoStartPanel != null) autoStartPanel.Visible = true;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
            if (!engine.LoadGame())
            {
                MessageBox.Show("No saved game found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateOptions(string o1, string o2, string o3, string o4)
        {
            if (InvokeRequired) { Invoke(new Action(() => UpdateOptions(o1, o2, o3, o4))); return; }
            btnOption1.Text = o1 ?? ""; btnOption2.Text = o2 ?? "";
            btnOption3.Text = o3 ?? ""; btnOption4.Text = o4 ?? "";
            btnOption1.Enabled = !string.IsNullOrEmpty(o1);
            btnOption2.Enabled = !string.IsNullOrEmpty(o2);
            btnOption3.Enabled = !string.IsNullOrEmpty(o3);
            btnOption4.Enabled = !string.IsNullOrEmpty(o4);
        }

        private void btnSave_Click(object sender, EventArgs e) { engine.SaveGame(); }
        
        private void BtnCraft_Click(object sender, EventArgs e)
        {
            if (engine.CurrentChapter() == 5 || pbHealth.Value <= 0) return;
            // Provide a quick craft shortcut
            DialogResult res = MessageBox.Show(
                "Select what to craft:\n\n" +
                "YES: [Medkit] (Alcohol + Rags)\n\n" +
                "NO: [Shiv] (Blades + Scrap)\n\n" +
                "CANCEL: [Molotov] (Alcohol + Rags + Empty Bottle)", 
                "Crafting Menu", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                
            if (res == DialogResult.Yes) engine.CraftItem("Medkit");
            else if (res == DialogResult.No) engine.CraftItem("Shiv");
            else if (res == DialogResult.Cancel) engine.CraftItem("Molotov");
        }

        private void btnOption1_Click(object sender, EventArgs e) { engine.Choose(1); }
        private void btnOption2_Click(object sender, EventArgs e) { engine.Choose(2); }
        private void btnOption3_Click(object sender, EventArgs e) { engine.Choose(3); }
        private void btnOption4_Click(object sender, EventArgs e) { engine.Choose(4); }
        private void btnContinue_Click(object sender, EventArgs e) { btnContinue.Visible = false; engine.NextParagraph(); }
    }
}