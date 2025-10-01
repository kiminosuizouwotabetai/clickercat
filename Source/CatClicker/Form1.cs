using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatClicker
{
    public partial class Form1 : Form
    {
        private int score = 0;
        private int clickValue = 1;
        private bool boostActive = false;
        private int boostTimeLeft = 0;
        private Timer boostTimer;
        private Timer redPulseTimer;
        private bool pulseBright = true;

        public Form1()
        {
            InitializeComponent();
            InitializeTimers();
        }

        private void InitializeTimers()
        {
            boostTimer = new Timer();
            boostTimer.Interval = 1000;
            boostTimer.Tick += BoostTimer_Tick;
            redPulseTimer = new Timer();
            redPulseTimer.Interval = 150;
            redPulseTimer.Tick += RedPulseTimer_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateScoreDisplay();
        }

        private void catPictureBox_Click(object sender, EventArgs e)
        {
            score += clickValue;
            UpdateScoreDisplay();
        }

        private void boostButton_Click(object sender, EventArgs e)
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
            boostButton.Enabled = false;
            boostButton.Text = $"БУСТ: {boostTimeLeft}с";
        }

        private void RedPulseTimer_Tick(object sender, EventArgs e)
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

        private void BoostTimer_Tick(object sender, EventArgs e)
        {
            boostTimeLeft--;
            boostButton.Text = $"БУСТ: {boostTimeLeft}с";

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
            boostButton.Enabled = true;
            boostButton.Text = "ТУРБОБУСТ";
            this.BackColor = Color.FromArgb(192, 192, 255);
        }

        private void UpdateScoreDisplay()
        {
            scoreLabel.Text = $"Счёт: {score}";
            clickValueLabel.Text = $"Количество кликов: {clickValue}";
        }
    }
}
