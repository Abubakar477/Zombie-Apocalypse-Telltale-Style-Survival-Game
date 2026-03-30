using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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

                int currentY = 20;
                int leftPad = 20; 

                if (lblChapter != null)
                {
                    lblChapter.Left = leftPad;
                    lblChapter.Top = currentY;
                    lblChapter.BackColor = Color.FromArgb(150, 0, 0, 0); 
                    currentY += 40; 
                }

                if (lblHealthHeader != null)
                {
                    lblHealthHeader.Left = leftPad;
                    lblHealthHeader.Top = currentY;
                    currentY += 25; 
                }

                if (pbHealth != null)
                {
                    pbHealth.Left = leftPad;
                    pbHealth.Top = currentY;
                    pbHealth.Width = 200;
                    pbHealth.Height = 20;
                    currentY += 40; 
                }

                if (lblInventoryHeader != null)
                {
                    lblInventoryHeader.Left = leftPad;
                    lblInventoryHeader.Top = currentY;
                    currentY += 25; 
                }

                if (lstInventory != null)
                {
                    lstInventory.Left = leftPad;
                    lstInventory.Top = currentY;
                    lstInventory.Width = 200;
                    lstInventory.Height = 150; 
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

            Button btnPlay = new Button();
            btnPlay.Text = "START GAME";
            btnPlay.Font = new Font("Consolas", 16F, FontStyle.Bold);
            btnPlay.ForeColor = Color.White;
            btnPlay.BackColor = Color.FromArgb(40, 40, 40);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.Size = new Size(250, 60);
            btnPlay.Cursor = Cursors.Hand;

            btnPlay.Click += (s, ev) => {
                autoStartPanel.Visible = false;
                
                if (bottomBox != null) bottomBox.Visible = true;
                if (lblHealthHeader != null) lblHealthHeader.Visible = true;
                if (lblInventoryHeader != null) lblInventoryHeader.Visible = true;
                if (pbHealth != null) pbHealth.Visible = true;
                if (lstInventory != null) lstInventory.Visible = true;
                if (lblChapter != null) lblChapter.Visible = true;

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

        private void UpdateStatus(int health, List<string> inventory, int chapter)
        {
            if (InvokeRequired) { Invoke(new Action(() => UpdateStatus(health, inventory, chapter))); return; }

            pbHealth.Value = Math.Max(0, Math.Min(100, health));
            lstInventory.Items.Clear();
            foreach (var item in inventory) lstInventory.Items.Add(item);

            if (lblChapter != null) lblChapter.Text = $"Chapter: {chapter}";

            if (chapter != lastChapter)
            {
                rtbOutput.Clear();
                PlayBackgroundMusic($"chapter{chapter}.wav");

                try
                {
                    string chapImgPath = GetImagePath($"chapter{chapter}");
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

                lastChapter = chapter;
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
        private void btnOption1_Click(object sender, EventArgs e) { engine.Choose(1); }
        private void btnOption2_Click(object sender, EventArgs e) { engine.Choose(2); }
        private void btnOption3_Click(object sender, EventArgs e) { engine.Choose(3); }
        private void btnOption4_Click(object sender, EventArgs e) { engine.Choose(4); }
        private void btnContinue_Click(object sender, EventArgs e) { btnContinue.Visible = false; engine.NextParagraph(); }
    }
}