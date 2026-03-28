using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Media;

namespace Zombie_apocolypse_telltale
{
    public partial class Form1 : Form
    {
        private GameEngine engine;
        private readonly Dictionary<int, Image> chapterImages = new Dictionary<int, Image>();
        private int lastChapter = 0;
        private SoundPlayer bgMusic;

        // --- NAYA: Custom Start Menu Panel ---
        private Panel autoStartPanel;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Screen flickering rokne ke liye
            engine = new GameEngine(AppendOutput, UpdateOptions, UpdateStatus);
        }

        // --- Screen Shake Animation ---
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
            Button[] options = { btnOption1, btnOption2, btnOption3, btnOption4, btnContinue, btnSave, btnLoad, btnRestart };

            foreach (var btn in options)
            {
                if (btn == null) continue;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
                btn.BackColor = Color.FromArgb(20, 20, 20);
                btn.ForeColor = Color.White;
                btn.Cursor = Cursors.Hand;
                btn.Font = new Font("Consolas", 10F, FontStyle.Bold);

                btn.MouseEnter += (s, e) => { if (btn.Enabled) btn.BackColor = Color.FromArgb(70, 70, 70); };
                btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(20, 20, 20); };
            }

            if (rtbOutput != null) rtbOutput.BorderStyle = BorderStyle.None;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupModernUI();
            LoadImagesFromFolder();

            // ==========================================
            // 100% CODE SE BANA HUA START MENU
            // ==========================================
            autoStartPanel = new Panel();
            autoStartPanel.Dock = DockStyle.Fill;
            autoStartPanel.BackColor = Color.Black;

            Label lblTitle = new Label();
            lblTitle.Text = "ZOMBIE APOCALYPSE\nSURVIVAL";
            lblTitle.ForeColor = Color.DarkRed;
            lblTitle.Font = new Font("Consolas", 32F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Top = 80;
            lblTitle.Left = this.Width / 2 - 250;

            Button btnPlay = new Button();
            btnPlay.Text = "START GAME";
            btnPlay.Font = new Font("Consolas", 16F, FontStyle.Bold);
            btnPlay.ForeColor = Color.White;
            btnPlay.BackColor = Color.FromArgb(40, 40, 40);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.Size = new Size(250, 60);
            btnPlay.Top = 250;
            btnPlay.Left = this.Width / 2 - 130;
            btnPlay.Cursor = Cursors.Hand;

            // Start Button Click Event
            btnPlay.Click += (s, ev) =>
            {
                autoStartPanel.Visible = false; // Menu hide
                engine.Start(); // Story Start
            };

            autoStartPanel.Controls.Add(lblTitle);
            autoStartPanel.Controls.Add(btnPlay);
            this.Controls.Add(autoStartPanel);
            autoStartPanel.BringToFront(); // Sab se aage le aayen
        }

        private void AppendOutput(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendOutput(text)));
                return;
            }

            Color textColor = Color.White;

            if (text.Contains("damage") || text.Contains("DIED") || text.Contains("hopeless") || text.Contains("blood"))
            {
                textColor = Color.LightCoral;
                ShakeScreen(); // Shake Effect!
            }
            else if (text.Contains("restored") || text.Contains("SURVIVED") || text.Contains("find a") || text.Contains("Medkit"))
                textColor = Color.LightGreen;
            else if (text.StartsWith("---") || text.StartsWith("==="))
                textColor = Color.Cyan;
            else if (text.StartsWith(">") || text.StartsWith("["))
                textColor = Color.LightGray;

            rtbOutput.SelectionStart = rtbOutput.TextLength;
            rtbOutput.SelectionLength = 0;
            rtbOutput.SelectionColor = textColor;

            foreach (char c in text)
            {
                rtbOutput.AppendText(c.ToString());
                rtbOutput.ScrollToCaret();
                System.Threading.Thread.Sleep(15);
                Application.DoEvents();
            }

            rtbOutput.AppendText(Environment.NewLine + Environment.NewLine);
            rtbOutput.ScrollToCaret();
        }

        private void UpdateOptions(string opt1, string opt2, string opt3, string opt4)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateOptions(opt1, opt2, opt3, opt4)));
                return;
            }

            btnOption1.Text = string.IsNullOrEmpty(opt1) ? "" : opt1;
            btnOption2.Text = string.IsNullOrEmpty(opt2) ? "" : opt2;
            btnOption3.Text = string.IsNullOrEmpty(opt3) ? "" : opt3;
            btnOption4.Text = string.IsNullOrEmpty(opt4) ? "" : opt4;

            btnOption1.Enabled = !string.IsNullOrEmpty(opt1);
            btnOption2.Enabled = !string.IsNullOrEmpty(opt2);
            btnOption3.Enabled = !string.IsNullOrEmpty(opt3);
            btnOption4.Enabled = !string.IsNullOrEmpty(opt4);
        }

        private void UpdateStatus(int health, List<string> inventory, int chapter)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(health, inventory, chapter)));
                return;
            }

            pbHealth.Value = Math.Max(0, Math.Min(pbHealth.Maximum, health));
            lstInventory.Items.Clear();
            foreach (var item in inventory) lstInventory.Items.Add(item);
            lblChapter.Text = $"Chapter: {chapter}";

            if (chapter != lastChapter)
            {
                try
                {
                    rtbOutput.Clear();
                    PlayBackgroundMusic($"chapter{chapter}.wav");

                    switch (chapter)
                    {
                        case 1: rtbOutput.BackColor = Color.Black; break;
                        case 2: rtbOutput.BackColor = Color.FromArgb(30, 30, 60); break;
                        case 3: rtbOutput.BackColor = Color.FromArgb(50, 30, 30); break;
                        case 4: rtbOutput.BackColor = Color.FromArgb(20, 20, 20); break;
                        default: rtbOutput.BackColor = Color.Black; break;
                    }
                }
                catch { }
                lastChapter = chapter;
            }

            if (chapterImages.TryGetValue(chapter, out Image img))
            {
                var previous = pbScene.Image;
                pbScene.Image = img;
                if (previous != null && !ReferenceEquals(previous, img))
                {
                    try { previous.Dispose(); } catch { }
                }
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

        private IEnumerable<string> GetImageDirectories(string baseDir)
        {
            var results = new List<string>();
            try
            {
                var imagesDefault = Path.Combine(baseDir, "Images");
                if (Directory.Exists(imagesDefault)) results.Add(imagesDefault);
                var subdirs = Directory.GetDirectories(baseDir);
                foreach (var d in subdirs)
                {
                    var name = Path.GetFileName(d).ToLowerInvariant();
                    if (name.Contains("image") || name.Contains("story")) results.Add(d);
                }
            }
            catch { }
            return results;
        }

        private void LoadImagesFromFolder()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dirs = GetImageDirectories(baseDir);
                if (dirs == null) return;
                foreach (var imagesDir in dirs)
                {
                    if (!Directory.Exists(imagesDir)) continue;
                    var files = Directory.GetFiles(imagesDir);
                    foreach (var f in files)
                    {
                        var name = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                        if (name.StartsWith("chapter") && int.TryParse(name.Substring(7), out int chap))
                        {
                            AddChapterImageFromFile(chap, f);
                        }
                    }
                }
            }
            catch { }
        }

        public void AddChapterImageFromFile(int chapter, string filePath)
        {
            if (!File.Exists(filePath)) return;
            try
            {
                using (var temp = Image.FromFile(filePath))
                {
                    var bmp = new Bitmap(temp);
                    if (chapterImages.ContainsKey(chapter)) chapterImages[chapter] = bmp;
                    else chapterImages.Add(chapter, bmp);
                }
            }
            catch { }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            engine = new GameEngine(AppendOutput, UpdateOptions, UpdateStatus);
            rtbOutput.Clear();
            lstInventory.Items.Clear();
            pbHealth.Value = pbHealth.Maximum;
            if (bgMusic != null) { bgMusic.Stop(); }
            if (autoStartPanel != null) { autoStartPanel.Visible = false; }
            engine.Start();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
            if (!engine.LoadGame())
            {
                MessageBox.Show("No saved game found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (autoStartPanel != null) autoStartPanel.Visible = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e) { engine.SaveGame(); }
        private void btnOption1_Click(object sender, EventArgs e) { engine.Choose(1); }
        private void btnOption2_Click(object sender, EventArgs e) { engine.Choose(2); }
        private void btnOption3_Click(object sender, EventArgs e) { engine.Choose(3); }
        private void btnOption4_Click(object sender, EventArgs e) { engine.Choose(4); }
        private void btnContinue_Click(object sender, EventArgs e)
        {
            try { btnContinue.Visible = false; } catch { }
            try { engine.NextParagraph(); } catch { }
        }
    }
}