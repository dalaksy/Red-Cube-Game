using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Test_game
{
    public partial class Form1 : Form
    {
        // --- CONSTANTS ---
        private const int WinW = 800;
        private const int WinH = 600;
        private const int EnemySize = 42;
        private const int PlayerSize = 28;
        private const int ItemSize = 18;
        private const int PlayerSpeed = 6;
        private const float EnemySpeed = 3.0f;

        // --- COLORS ---
        private Color pColor = Color.Crimson;
        private Color eColor = Color.RoyalBlue;
        private Color iColor = Color.ForestGreen;

        // --- STATE ---
        private enum GameState { Playing, Won, Lost }
        private GameState currentState;
        private int worldX = 1, worldY = 1;
        private Room[,] world = new Room[3, 3];
        private PointF playerPos;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();
        private int collectedCount = 0;
        private const int totalItems = 8;
        private bool showInfoWindow = false; // Состояние окна инфо

        private Button btnRetry = new Button();
        private Button btnExit = new Button();

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(WinW, WinH);
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            InitUI();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;

            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.E) Application.Exit();
                if (e.KeyCode == Keys.R) StartNewGame();
                if (e.KeyCode == Keys.F) showInfoWindow = !showInfoWindow; // Переключение инфо
                if (e.KeyCode == Keys.C) PickColor("player");
                if (e.KeyCode == Keys.V) PickColor("enemy");
                if (e.KeyCode == Keys.B) PickColor("item");
                if (!pressedKeys.Contains(e.KeyCode)) pressedKeys.Add(e.KeyCode);
            };
            this.KeyUp += (s, e) => { pressedKeys.Remove(e.KeyCode); };

            StartNewGame();
        }

        private void PickColor(string target)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    if (target == "player") pColor = cd.Color;
                    if (target == "enemy") eColor = cd.Color;
                    if (target == "item") iColor = cd.Color;
                }
            }
            this.Focus();
        }

        private void InitUI()
        {
            btnRetry.Text = "Restart";
            btnRetry.Font = new Font("Arial", 12);
            btnRetry.Size = new Size(140, 45);
            btnRetry.Location = new Point(WinW / 2 - 150, WinH / 2 + 60);
            btnRetry.Visible = false;
            btnRetry.Click += (s, e) => StartNewGame();

            btnExit.Text = "Exit";
            btnExit.Font = new Font("Arial", 12);
            btnExit.Size = new Size(140, 45);
            btnExit.Location = new Point(WinW / 2 + 10, WinH / 2 + 60);
            btnExit.Visible = false;
            btnExit.Click += (s, e) => Application.Exit();

            this.Controls.Add(btnRetry);
            this.Controls.Add(btnExit);
        }

        private void StartNewGame()
        {
            gameTimer.Stop();
            pressedKeys.Clear();
            collectedCount = 0;
            worldX = 1; worldY = 1;
            playerPos = new PointF(WinW / 2 - PlayerSize / 2, WinH / 2 - PlayerSize / 2);
            btnRetry.Visible = btnExit.Visible = false;
            currentState = GameState.Playing;
            SetupWorldLayout();
            this.Focus();
            gameTimer.Start();
        }

        private void SetupWorldLayout()
        {
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    world[x, y] = new Room();

            int cT = 120, cB = 300, vL = 250, vR = 550, pW = 300, gT = 200, gB = 400;

            world[0, 0].Walls.Add(new Rectangle(0, 0, WinW, cT));
            world[0, 0].Walls.Add(new Rectangle(0, 0, 60, WinH));
            world[0, 0].Walls.Add(new Rectangle(pW, cB, WinW - pW, WinH - cB));
            world[0, 0].Items.Add(new Rectangle(120, 180, ItemSize, ItemSize));
            world[0, 0].Enemies.Add(new PointF(200, 200));

            world[1, 0].Walls.Add(new Rectangle(0, 0, WinW, cT));
            world[1, 0].Walls.Add(new Rectangle(0, cB, vL, WinH - cB));
            world[1, 0].Walls.Add(new Rectangle(vR, cB, WinW - vR, WinH - cB));
            world[1, 0].Items.Add(new Rectangle(WinW / 2 - ItemSize / 2, cT + 20, ItemSize, ItemSize));
            world[1, 0].Enemies.Add(new PointF(WinW / 2 + 100, cT + 80));

            world[2, 0].Walls.Add(new Rectangle(0, 0, WinW, cT));
            world[2, 0].Walls.Add(new Rectangle(WinW - 60, 0, 60, WinH));
            world[2, 0].Walls.Add(new Rectangle(0, cB, 500, WinH - cB));
            world[2, 0].Items.Add(new Rectangle(WinW - 150, 180, ItemSize, ItemSize));
            world[2, 0].Enemies.Add(new PointF(WinW - 240, 200));

            world[0, 1].Walls.Add(new Rectangle(0, 0, 60, WinH));
            world[0, 1].Walls.Add(new Rectangle(pW, 0, WinW - pW, gT));
            world[0, 1].Walls.Add(new Rectangle(pW, gB, WinW - pW, WinH - gB));
            world[0, 1].Items.Add(new Rectangle(120, WinH / 2, ItemSize, ItemSize));
            world[0, 1].Enemies.Add(new PointF(400, WinH / 2));

            world[1, 1].Walls.Add(new Rectangle(0, 0, vL, gT));
            world[1, 1].Walls.Add(new Rectangle(vR, 0, WinW - vR, gT));
            world[1, 1].Walls.Add(new Rectangle(0, gB, vL, WinH - gB));
            world[1, 1].Walls.Add(new Rectangle(vR, gB, WinW - vR, WinH - gB));

            world[2, 1].Walls.Add(new Rectangle(WinW - 60, 0, 60, WinH));
            world[2, 1].Walls.Add(new Rectangle(0, 0, 500, gT));
            world[2, 1].Walls.Add(new Rectangle(0, gB, 500, WinH - gB));
            world[2, 1].Items.Add(new Rectangle(WinW - 200, WinH / 2, ItemSize, ItemSize));
            world[2, 1].Enemies.Add(new PointF(WinW - 450, WinH / 2));

            world[0, 2].Walls.Add(new Rectangle(0, WinH - cT, WinW, cT));
            world[0, 2].Walls.Add(new Rectangle(0, 0, 60, WinH));
            world[0, 2].Walls.Add(new Rectangle(pW, 0, WinW - pW, cB));
            world[0, 2].Items.Add(new Rectangle(120, WinH - 200, ItemSize, ItemSize));
            world[0, 2].Enemies.Add(new PointF(200, WinH - 250));

            world[1, 2].Walls.Add(new Rectangle(0, WinH - cT, WinW, cT));
            world[1, 2].Walls.Add(new Rectangle(0, 0, vL, cB));
            world[1, 2].Walls.Add(new Rectangle(vR, 0, WinW - vR, cB));
            world[1, 2].Items.Add(new Rectangle(WinW / 2 - ItemSize / 2, WinH - 200, ItemSize, ItemSize));
            world[1, 2].Enemies.Add(new PointF(WinW / 2 - 150, WinH - 250));

            world[2, 2].Walls.Add(new Rectangle(0, WinH - cT, WinW, cT));
            world[2, 2].Walls.Add(new Rectangle(WinW - 60, 0, 60, WinH));
            world[2, 2].Walls.Add(new Rectangle(0, 0, 500, cB));
            world[2, 2].Items.Add(new Rectangle(WinW - 150, WinH - 200, ItemSize, ItemSize));
            world[2, 2].Enemies.Add(new PointF(WinW - 240, WinH - 250));
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (currentState != GameState.Playing) return;
            float nX = playerPos.X, nY = playerPos.Y;
            if (pressedKeys.Contains(Keys.W)) nY -= PlayerSpeed;
            if (pressedKeys.Contains(Keys.S)) nY += PlayerSpeed;
            if (pressedKeys.Contains(Keys.A)) nX -= PlayerSpeed;
            if (pressedKeys.Contains(Keys.D)) nX += PlayerSpeed;

            if (!IsBoxColliding(nX, playerPos.Y, PlayerSize, PlayerSize)) playerPos.X = nX;
            if (!IsBoxColliding(playerPos.X, nY, PlayerSize, PlayerSize)) playerPos.Y = nY;

            var room = world[worldX, worldY];
            for (int i = 0; i < room.Enemies.Count; i++)
            {
                PointF eP = room.Enemies[i];
                float dx = playerPos.X - eP.X, dy = playerPos.Y - eP.Y;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d > 1)
                {
                    float sX = (dx / d) * EnemySpeed, sY = (dy / d) * EnemySpeed;
                    if (!IsBoxColliding(eP.X + sX, eP.Y, EnemySize, EnemySize)) eP.X += sX;
                    if (!IsBoxColliding(eP.X, eP.Y + sY, EnemySize, EnemySize)) eP.Y += sY;
                    room.Enemies[i] = eP;
                }
                if (new RectangleF(playerPos.X, playerPos.Y, PlayerSize, PlayerSize).IntersectsWith(new RectangleF(eP.X, eP.Y, EnemySize, EnemySize)))
                    GameOver(GameState.Lost);
            }

            float cx = playerPos.X + PlayerSize / 2, cy = playerPos.Y + PlayerSize / 2;
            if (cx > WinW && worldX < 2) { worldX++; playerPos.X = 25; }
            else if (cx < 0 && worldX > 0) { worldX--; playerPos.X = WinW - PlayerSize - 25; }
            else if (cy > WinH && worldY < 2) { worldY++; playerPos.Y = 25; }
            else if (cy < 0 && worldY > 0) { worldY--; playerPos.Y = WinH - PlayerSize - 25; }

            for (int i = room.Items.Count - 1; i >= 0; i--)
                if (new RectangleF(playerPos.X, playerPos.Y, PlayerSize, PlayerSize).IntersectsWith(new RectangleF(room.Items[i].X, room.Items[i].Y, ItemSize, ItemSize)))
                {
                    room.Items.RemoveAt(i); collectedCount++;
                    if (collectedCount >= totalItems) GameOver(GameState.Won);
                }
            this.Invalidate();
        }

        private bool IsBoxColliding(float x, float y, float w, float h)
        {
            RectangleF r = new RectangleF(x, y, w, h);
            foreach (var wall in world[worldX, worldY].Walls) if (r.IntersectsWith(wall)) return true;
            return false;
        }

        private void GameOver(GameState state)
        {
            gameTimer.Stop(); currentState = state;
            btnRetry.Visible = btnExit.Visible = true;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.Clear(Color.FromArgb(30, 30, 30));
            if (currentState == GameState.Playing)
            {
                var r = world[worldX, worldY];
                foreach (var w in r.Walls) g.FillRectangle(Brushes.Black, w);
                using (SolidBrush ib = new SolidBrush(iColor)) foreach (var it in r.Items) g.FillRectangle(ib, it.X, it.Y, ItemSize, ItemSize);
                using (SolidBrush eb = new SolidBrush(eColor)) foreach (var en in r.Enemies) g.FillRectangle(eb, en.X, en.Y, EnemySize, EnemySize);
                using (SolidBrush pb = new SolidBrush(pColor)) g.FillRectangle(pb, playerPos.X, playerPos.Y, PlayerSize, PlayerSize);

                // Основной HUD
                g.DrawString($"Collected: {collectedCount}/{totalItems} | Sector: {(worldY * 3 + worldX + 1)}", new Font("Arial", 10, FontStyle.Bold), Brushes.White, 10, 10);
                g.DrawString("Press [F] for Info", new Font("Arial", 9), Brushes.LightGray, 10, 25);

                // Окно информации
                if (showInfoWindow)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), 200, 150, 400, 300);
                    g.DrawRectangle(Pens.White, 200, 150, 400, 300);
                    string infoText = "GAME CONTROLS\n\nWASD - Move Cube\nE - Exit Game\nR - Restart Level\nF - Close Info\n\nCOLORS:\nC - Change Player Color\nV - Change Enemy Color\nB - Change Item Color";
                    g.DrawString(infoText, new Font("Consolas", 12, FontStyle.Bold), Brushes.White, 220, 170);
                }
            }
            else
            {
                string m = currentState == GameState.Won ? "VICTORY!" : "DEFEAT";
                g.DrawString(m, new Font("Arial", 45, FontStyle.Bold), currentState == GameState.Won ? Brushes.Gold : Brushes.Red, WinW / 2 - 180, WinH / 2 - 60);
            }
        }
    }

    public class Room { public List<Rectangle> Walls = new List<Rectangle>(), Items = new List<Rectangle>(); public List<PointF> Enemies = new List<PointF>(); }
}