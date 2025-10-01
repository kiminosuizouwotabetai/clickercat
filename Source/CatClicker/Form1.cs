using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace CatClicker
{
    public partial class Form1 : Form
    {
        private int score = 0;
        private int clickValue = 1;
        private bool boostActive = false;
        private int boostTimeLeft = 0;
        private Timer boostTimer;
        private Timer boostAnimationTimer;
        private Timer redPulseTimer;
        private bool pulseBright = true;

        private Guna2Button gunaBoostButton;
        private Timer buttonAnimationTimer;
        private float buttonPulseScale = 1.0f;
        private bool buttonPulseGrowing = true;
        private Timer backgroundTimer;
        private float gradientAngle = 0;
        private Color[] gradientColors = {
            Color.FromArgb(200, 100, 200),
            Color.FromArgb(100, 200, 200),
            Color.FromArgb(200, 200, 100)
        };
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private Timer particleTimer;
        private List<string> clickSoundPaths = new List<string>();
        private Random soundRandom = new Random();
        private SoundPlayer clickSoundPlayer;
        private float boostScale = 1.0f;
        private List<BoostParticle> boostParticles = new List<BoostParticle>();

        class Particle
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float SpeedX { get; set; }
            public float SpeedY { get; set; }
            public float Size { get; set; }
            public Color Color { get; set; }
        }

        class BoostParticle
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float SpeedX { get; set; }
            public float SpeedY { get; set; }
            public float Size { get; set; }
            public Color Color { get; set; }
            public int Life { get; set; } = 100;
        }

        public Form1()
        {
            InitializeComponent();

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.Resize += Form1_Resize;

            InitializeTimers();
            InitializeSound();
            InitializeGunaButton();
            InitializeBackgroundAnimation();
            InitializeParticles();
            ImproveUIStyle();
            CenterControls();
        }

        private void StartButtonBoostAnimation()
        {
            buttonAnimationTimer = new Timer();
            buttonAnimationTimer.Interval = 50;
            buttonAnimationTimer.Tick += ButtonAnimationTimer_Tick;
            buttonAnimationTimer.Start();
        }

        private void StopButtonBoostAnimation()
        {
            if (buttonAnimationTimer != null)
            {
                buttonAnimationTimer.Stop();
                buttonAnimationTimer.Dispose();
                buttonAnimationTimer = null;
            }

            if (gunaBoostButton != null)
            {
                gunaBoostButton.Size = new Size(200, 60);
                gunaBoostButton.FillColor = Color.FromArgb(255, 105, 180);
                CenterControls();
            }
        }

        private void ButtonAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (gunaBoostButton == null || !boostActive) return;

            if (buttonPulseGrowing)
            {
                buttonPulseScale += 0.08f;
                if (buttonPulseScale >= 1.15f) buttonPulseGrowing = false;
            }
            else
            {
                buttonPulseScale -= 0.08f;
                if (buttonPulseScale <= 0.95f) buttonPulseGrowing = true;
            }

            gunaBoostButton.Size = new Size(
                (int)(200 * buttonPulseScale),
                (int)(60 * buttonPulseScale)
            );

            CenterControls();
            int red = 255;
            int green = (int)(105 * (2 - buttonPulseScale));
            int blue = (int)(180 * (2 - buttonPulseScale));
            gunaBoostButton.FillColor = Color.FromArgb(255, red, green, blue);
        }

        private void InitializeSound()
        {
            try
            {
                clickSoundPaths.Add(@"B:\YA\repos\clicker\Source\CatClicker\Resources\1.wav");
                clickSoundPaths.Add(@"B:\YA\repos\clicker\Source\CatClicker\Resources\2.wav");
                clickSoundPaths.Add(@"B:\YA\repos\clicker\Source\CatClicker\Resources\3.wav");
                clickSoundPaths.Add(@"B:\YA\repos\clicker\Source\CatClicker\Resources\4.wav");
                foreach (string path in clickSoundPaths.ToList())
                {
                    if (!File.Exists(path))
                    {
                        clickSoundPaths.Remove(path);
                    }
                }
            }
            catch { }
        }

        private void InitializeGunaButton()
        {
            gunaBoostButton = new Guna2Button();
            gunaBoostButton.Size = new Size(200, 60);
            gunaBoostButton.Text = "ТУРБОБУСТ";
            gunaBoostButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            gunaBoostButton.ForeColor = Color.White;
            gunaBoostButton.FillColor = Color.FromArgb(255, 105, 180);
            gunaBoostButton.Animated = true;
            gunaBoostButton.BorderRadius = 20;
            gunaBoostButton.BorderThickness = 0;
            gunaBoostButton.BorderColor = Color.Transparent;
            gunaBoostButton.BackColor = Color.Transparent;
            gunaBoostButton.Click += BoostPictureBox_Click;

            this.Controls.Add(gunaBoostButton);
        }

        private void InitializeTimers()
        {
            boostTimer = new Timer();
            boostTimer.Interval = 1000;
            boostTimer.Tick += BoostTimer_Tick;

            redPulseTimer = new Timer();
            redPulseTimer.Interval = 150;
            redPulseTimer.Tick += RedPulseTimer_Tick;

            boostAnimationTimer = new Timer();
            boostAnimationTimer.Interval = 30;
            boostAnimationTimer.Tick += BoostAnimationTimer_Tick;
        }

        private void InitializeBackgroundAnimation()
        {
            backgroundTimer = new Timer();
            backgroundTimer.Interval = 50;
            backgroundTimer.Tick += BackgroundTimer_Tick;
            backgroundTimer.Start();

            this.Paint += Form1_Paint;
        }

        private void InitializeParticles()
        {
            particles.Clear();

            for (int i = 0; i < 160; i++) 
            {
                particles.Add(new Particle
                {
                    X = random.Next(0, Math.Max(1, this.ClientSize.Width)),
                    Y = random.Next(0, Math.Max(1, this.ClientSize.Height)),
                    SpeedX = (float)(random.NextDouble() - 0.5) * 4,
                    SpeedY = (float)(random.NextDouble() - 0.5) * 4,
                    Size = random.Next(8, 25),
                    Color = Color.FromArgb(90, random.Next(150, 255), random.Next(150, 255), random.Next(150, 255))
                });
            }

            particleTimer = new Timer();
            particleTimer.Interval = 30;
            particleTimer.Tick += ParticleTimer_Tick;
            particleTimer.Start();

            this.Paint += Form1_ParticlePaint;
        }

        private void ImproveUIStyle()
        {
            scoreLabel.Font = new Font("Comic Sans MS", 24, FontStyle.Bold);
            scoreLabel.ForeColor = Color.White;
            scoreLabel.BackColor = Color.Transparent;

            clickValueLabel.Font = new Font("Comic Sans MS", 16, FontStyle.Bold);
            clickValueLabel.ForeColor = Color.White;
            clickValueLabel.BackColor = Color.Transparent;
        }

        private void CenterControls()
        {
            if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0)
                return;

            int catWidth = (int)(this.ClientSize.Width * 0.4);
            int catHeight = (int)(this.ClientSize.Height * 0.5);
            catPictureBox.Size = new Size(catWidth, catHeight);
            catPictureBox.Location = new Point(
                (this.ClientSize.Width - catWidth) / 2,
                (this.ClientSize.Height - catHeight) / 3
            );

            scoreLabel.Location = new Point(
                (this.ClientSize.Width - scoreLabel.Width) / 2,
                Math.Max(20, catPictureBox.Top - 80)
            );

            clickValueLabel.Location = new Point(
                (this.ClientSize.Width - clickValueLabel.Width) / 2,
                scoreLabel.Bottom + 10
            );

            if (gunaBoostButton != null)
            {
                gunaBoostButton.Location = new Point(
                    (this.ClientSize.Width - gunaBoostButton.Width) / 2,
                    catPictureBox.Bottom + 20
                );
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.Empty,
                Color.Empty,
                gradientAngle,
                true))
            {
                ColorBlend colorBlend = new ColorBlend();
                colorBlend.Colors = gradientColors;
                colorBlend.Positions = new float[] { 0f, 0.5f, 1f };
                brush.InterpolationColors = colorBlend;

                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            if (boostActive)
            {
                DrawBoostAnimation(e.Graphics);
            }
        }

        private void DrawBoostAnimation(Graphics g)
        {
            foreach (var particle in boostParticles)
            {
                using (SolidBrush brush = new SolidBrush(particle.Color))
                {
                    g.FillEllipse(brush, particle.X, particle.Y, particle.Size, particle.Size);
                }
            }
        }

        private void BoostAnimationTimer_Tick(object sender, EventArgs e)
        {
            boostScale = 1.0f + (float)Math.Sin(DateTime.Now.Millisecond * 0.01f) * 0.2f;
            UpdateBoostParticles();
            this.Invalidate();
        }

        private void UpdateBoostParticles()
        {
            if (boostParticles.Count < 160 && random.Next(0, 100) < 70)
            {
                float centerX = catPictureBox.Left + catPictureBox.Width / 2;
                float centerY = catPictureBox.Top + catPictureBox.Height / 2;

                boostParticles.Add(new BoostParticle
                {
                    X = centerX + (float)(random.NextDouble() - 0.5) * 100,
                    Y = centerY + (float)(random.NextDouble() - 0.5) * 100,
                    SpeedX = (float)(random.NextDouble() - 0.5) * 3,
                    SpeedY = (float)(random.NextDouble() - 0.5) * 3,
                    Size = random.Next(6, 18),
                    Color = Color.FromArgb(200,
                        random.Next(200, 255),
                        random.Next(200, 255),
                        random.Next(50, 150))
                });
            }

            for (int i = boostParticles.Count - 1; i >= 0; i--)
            {
                var particle = boostParticles[i];
                particle.X += particle.SpeedX;
                particle.Y += particle.SpeedY;
                particle.Life--;

                if (particle.Life <= 0)
                {
                    boostParticles.RemoveAt(i);
                }
            }
        }

        private void BackgroundTimer_Tick(object sender, EventArgs e)
        {
            gradientAngle += 1f;
            if (gradientAngle >= 360f) gradientAngle = 0f;
            this.Invalidate();
        }

        private void Form1_ParticlePaint(object sender, PaintEventArgs e)
        {
            foreach (var particle in particles)
            {
                using (SolidBrush brush = new SolidBrush(particle.Color))
                {
                    e.Graphics.FillEllipse(brush, particle.X, particle.Y, particle.Size, particle.Size);
                }
            }
        }

        private void ParticleTimer_Tick(object sender, EventArgs e)
        {
            bool needsRefresh = false;

            for (int i = 0; i < particles.Count; i++)
            {
                var particle = particles[i];
                float oldX = particle.X;
                float oldY = particle.Y;

                particle.X += particle.SpeedX;
                particle.Y += particle.SpeedY;

                if (particle.X < 0) particle.X = 0;
                else if (particle.X > this.ClientSize.Width - particle.Size)
                    particle.X = this.ClientSize.Width - particle.Size;

                if (particle.Y < 0) particle.Y = 0;
                else if (particle.Y > this.ClientSize.Height - particle.Size)
                    particle.Y = this.ClientSize.Height - particle.Size;

                if (oldX != particle.X || oldY != particle.Y)
                    needsRefresh = true;
            }

            if (needsRefresh)
                this.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateScoreDisplay();
        }

        private void catPictureBox_Click(object sender, EventArgs e)
        {
            score += clickValue;
            UpdateScoreDisplay();
            AnimateCatClick();
            PlayClickSound();
        }

        private void AnimateCatClick()
        {
            var originalLocation = catPictureBox.Location;
            catPictureBox.Location = new Point(originalLocation.X, originalLocation.Y - 10);

            var timer = new Timer();
            timer.Interval = 100;
            timer.Tick += (s, e) =>
            {
                catPictureBox.Location = originalLocation;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void PlayClickSound()
        {
            try
            {
                if (clickSoundPaths.Count > 0)
                {
                    int randomIndex = soundRandom.Next(0, clickSoundPaths.Count);
                    string selectedSound = clickSoundPaths[randomIndex];

                    // Воспроизводим WAV файл
                    using (var player = new SoundPlayer(selectedSound))
                    {
                        player.Play();
                    }
                }
                else
                {
                    Console.Beep(800, 50);
                }
            }
            catch
            {
                Console.Beep(800, 50);
            }
        }

        private void BoostPictureBox_Click(object sender, EventArgs e)
        {
            if (!boostActive)
            {
                ActivateBoost();
            }
        }

        private void ActivateBoost()
        {
            boostActive = true;
            clickValue = 2;
            boostTimeLeft = 10;

            boostTimer.Start();
            redPulseTimer.Start();
            boostAnimationTimer.Start();
            gunaBoostButton.Enabled = false;
            gunaBoostButton.Text = $"БУСТ: {boostTimeLeft}с";
            StartButtonBoostAnimation();

            boostParticles.Clear();
        }

        private void RedPulseTimer_Tick(object sender, EventArgs e)
        {
            if (boostActive) // ТОЛЬКО когда буст активен
            {
                if (pulseBright)
                {
                    this.BackColor = Color.FromArgb(255, 80, 80);
                    pulseBright = false;
                }
                else
                {
                    this.BackColor = Color.FromArgb(200, 50, 50);
                    pulseBright = true;
                }
            }
        }

        private void BoostTimer_Tick(object sender, EventArgs e)
        {
            boostTimeLeft--;
            gunaBoostButton.Text = $"БУСТ: {boostTimeLeft}с";

            if (boostTimeLeft <= 0)
            {
                DeactivateBoost();
            }
        }

        private void DeactivateBoost()
        {
            boostActive = false;
            clickValue = 1;
            boostTimer.Stop();
            redPulseTimer.Stop();
            boostAnimationTimer.Stop();
            StopButtonBoostAnimation();

            gunaBoostButton.Enabled = true;
            gunaBoostButton.Text = "ТУРБОБУСТ";
            this.BackColor = Color.FromArgb(192, 192, 255);

            boostParticles.Clear();
            this.Invalidate();
        }

        private void UpdateScoreDisplay()
        {
            scoreLabel.Text = $"Счёт: {score}";
            clickValueLabel.Text = $"Количество кликов: {clickValue}";
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            InitializeParticles();
            CenterControls();
            this.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
