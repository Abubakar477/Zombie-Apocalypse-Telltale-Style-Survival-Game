using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Zombie_apocolypse_telltale
{
    public partial class Form1 : Form
    {
        private GameEngine engine;
        private readonly Dictionary<int, Image> chapterImages = new Dictionary<int, Image>();
        private int lastChapter = 1;

        public Form1()
        {
            InitializeComponent();
            engine = new GameEngine(AppendOutput, UpdateOptions, UpdateStatus);
        }
        

        private int ExtractChapterNumber()
        {
            var text = lblChapter.Text ?? "";
            if (text.StartsWith("Chapter:"))
            {
                var parts = text.Split(':');
                if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int n)) return n;
            }
            return 1;
        }

        // Simple input dialog to get text from user
        private string ShowInputDialog(string text, string caption, string defaultValue = "")
        {
            string result = defaultValue;
            using (Form prompt = new Form())
            {
                prompt.Width = 400;
                prompt.Height = 150;
                prompt.Text = caption;
                Label textLabel = new Label() { Left = 10, Top = 10, Text = text, Width = 360 };
                TextBox inputBox = new TextBox() { Left = 10, Top = 40, Width = 360, Text = defaultValue };
                Button confirmation = new Button() { Text = "OK", Left = 220, Width = 70, Top = 70, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 300, Width = 70, Top = 70, DialogResult = DialogResult.Cancel };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    result = inputBox.Text;
                }
            }
            return result;
        }

        // Returns a list of candidate directories to search for story images.
        private IEnumerable<string> GetImageDirectories(string baseDir)
        {
            var results = new List<string>();
            try
            {
                // common default
                var imagesDefault = Path.Combine(baseDir, "Images");
                if (Directory.Exists(imagesDefault)) results.Add(imagesDefault);

                // also search for directories containing 'image' or 'story' in their name
                var subdirs = Directory.GetDirectories(baseDir);
                foreach (var d in subdirs)
                {
                    var name = Path.GetFileName(d).ToLowerInvariant();
                    if (name.Contains("image") || name.Contains("story"))
                    {
                        results.Add(d);
                    }
                }
            }
            catch { }

            return results;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadImagesFromFolder();
            // start the game immediately
            engine.Start();
        }

        private void AppendOutput(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendOutput(text)));
                return;
            }

            rtbOutput.AppendText(text + Environment.NewLine + Environment.NewLine);
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
            foreach (var item in inventory)
            {
                lstInventory.Items.Add(item);
            }
            lblChapter.Text = $"Chapter: {chapter}";

            // Only change background color when chapter actually changes
            if (chapter != lastChapter)
            {
                try
                {
                    switch (chapter)
                    {
                        case 1:
                            rtbOutput.BackColor = Color.Black;
                            break;
                        case 2:
                            rtbOutput.BackColor = Color.FromArgb(30, 30, 60);
                            break;
                        case 3:
                            rtbOutput.BackColor = Color.FromArgb(50, 30, 30);
                            break;
                        case 4:
                            rtbOutput.BackColor = Color.FromArgb(20, 20, 20);
                            break;
                        default:
                            rtbOutput.BackColor = Color.Black;
                            break;
                    }
                }
                catch { }
                lastChapter = chapter;
            }

            // update scene image if available for this chapter
            if (chapterImages.TryGetValue(chapter, out Image img))
            {
                // replace image (dispose previous)
                var previous = pbScene.Image;
                pbScene.Image = img;
                if (previous != null && !ReferenceEquals(previous, img))
                {
                    try { previous.Dispose(); } catch { }
                }
            }
            else
            {
                // try to find image file named chapter{n}.* in any images folder
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dirs = GetImageDirectories(baseDir);
                if (dirs != null)
                {
                    string[] exts = new[] { "png", "jpg", "jpeg", "bmp" };
                    foreach (var imagesDir in dirs)
                    {
                        foreach (var ext in exts)
                        {
                            string path = Path.Combine(imagesDir, $"chapter{chapter}.{ext}");
                            if (File.Exists(path))
                            {
                                AddChapterImageFromFile(chapter, path);
                                if (chapterImages.TryGetValue(chapter, out Image loaded))
                                {
                                    var previous = pbScene.Image;
                                    pbScene.Image = loaded;
                                    if (previous != null && !ReferenceEquals(previous, loaded))
                                    {
                                        try { previous.Dispose(); } catch { }
                                    }
                                }
                                break;
                            }
                        }
                        if (chapterImages.ContainsKey(chapter)) break;
                    }
                }
            }
        }

        // Load any images present in the Images folder matching chapter{n}.*
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

                    // First pass: files explicitly named chapterN -> use that mapping
                    var remaining = new List<string>();
                    foreach (var f in files.OrderBy(p => p))
                    {
                        var name = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                        if (name.StartsWith("chapter"))
                        {
                            string numPart = name.Substring("chapter".Length);
                            if (int.TryParse(numPart, out int chap) && chap > 0)
                            {
                                AddChapterImageFromFile(chap, f);
                                continue;
                            }
                        }

                        // collect for auto-mapping
                        remaining.Add(f);
                    }

                    // Auto-map remaining files sequentially to unused chapter numbers starting at 1
                    int assign = 1;
                    // find next free chapter number
                    if (chapterImages.Count > 0)
                    {
                        // find smallest positive integer not used
                        var used = new HashSet<int>(chapterImages.Keys);
                        while (used.Contains(assign)) assign++;
                    }

                    foreach (var f in remaining)
                    {
                        // ensure not overriding existing
                        while (chapterImages.ContainsKey(assign)) assign++;
                        AddChapterImageFromFile(assign, f);
                        assign++;
                    }
                }
            }
            catch { /* ignore load errors */ }
        }

        // Safely load an image from file and store a cloned copy to avoid file locks
        public void AddChapterImageFromFile(int chapter, string filePath)
        {
            if (!File.Exists(filePath)) return;
            try
            {
                using (var temp = Image.FromFile(filePath))
                {
                    var bmp = new Bitmap(temp);
                    if (chapterImages.ContainsKey(chapter))
                    {
                        try { chapterImages[chapter].Dispose(); } catch { }
                        chapterImages[chapter] = bmp;
                    }
                    else
                    {
                        chapterImages.Add(chapter, bmp);
                    }
                }
            }
            catch { }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            // recreate engine and restart
            engine = new GameEngine(AppendOutput, UpdateOptions, UpdateStatus);
            rtbOutput.Clear();
            lstInventory.Items.Clear();
            pbHealth.Value = pbHealth.Maximum;
            engine.Start();
        }

        private void btnOption1_Click(object sender, EventArgs e)
        {
            engine.Choose(1);
        }

        private void btnOption2_Click(object sender, EventArgs e)
        {
            engine.Choose(2);
        }

        private void btnOption3_Click(object sender, EventArgs e)
        {
            engine.Choose(3);
        }

        private void btnOption4_Click(object sender, EventArgs e)
        {
            engine.Choose(4);
        }

        // Handler added to satisfy designer wiring
        private void btnContinue_Click(object sender, EventArgs e)
        {
            try { btnContinue.Visible = false; } catch { }
            try { engine.NextParagraph(); } catch { }
        }

        // Handler for start button (if present)
        private void btnStart_Click(object sender, EventArgs e)
        {
            try { panelStart.Visible = false; } catch { }
            try { panelContent.Visible = true; } catch { }
            try { engine.Start(); } catch { }
        }
    }
}
