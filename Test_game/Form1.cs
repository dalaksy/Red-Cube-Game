using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Test_game
{
    public partial class Form1 : Form
    {
        private const int WinW = 800;
        private const int WinH = 600;
        private const int ViewW = 1920;
        private const int ViewH = 1080;
        private const int EnemySize = 42;

        private const int PlayerW = 24;
        private const int PlayerH = 34;

        private const int ItemSize = 18;
        private const int PlayerSpeed = 6;
        private const float EnemySpeed = 3.0f;

        private readonly Color pColor = Color.Crimson;
        private readonly Color eColor = Color.RoyalBlue;
        private readonly Color iColor = Color.ForestGreen;
        private readonly Color nColor = Color.Orange;

        private enum GameState { Playing, Won, Lost }
        private GameState currentState;
        private int worldX = 1, worldY = 1;
        private Room[,] world = new Room[3, 3];
        private PointF playerPos;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();
        private int collectedCount = 0;
        private const int totalItems = 8;
        private bool showInfoWindow = false;
        private bool showMapView = false;
        private bool isQuestAccepted = false;
        private bool showDialogue = false;

        private SpriteManager sprites = new SpriteManager();
        private string playerFacing = "down";
        private bool isMoving = false;

        private Button btnRetry = new Button();
        private Button btnExit = new Button();

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(WinW, WinH);
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            sprites.LoadSprites(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets"));

            InitUI();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;

            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Q) Application.Exit();
                if (e.KeyCode == Keys.R) StartNewGame();
                if (e.KeyCode == Keys.I) showInfoWindow = !showInfoWindow;
                if (e.KeyCode == Keys.F && currentState == GameState.Playing) ToggleMapView();
                if (e.KeyCode == Keys.E) TryInteract();
                if (!pressedKeys.Contains(e.KeyCode)) pressedKeys.Add(e.KeyCode);
            };
            this.KeyUp += (s, e) => { pressedKeys.Remove(e.KeyCode); };

            StartNewGame();
        }

        private void TryInteract()
        {
            if (worldX == 1 && worldY == 1)
            {
                var npcPos = world[1, 1].NPCs[0];
                float dist = (float)Math.Sqrt(Math.Pow(playerPos.X - npcPos.X, 2) + Math.Pow(playerPos.Y - npcPos.Y, 2));
                if (dist < 80)
                {
                    if (collectedCount == totalItems) GameOver(GameState.Won);
                    else { isQuestAccepted = true; showDialogue = !showDialogue; }
                }
            }
        }

        private void ToggleMapView()
        {
            showMapView = !showMapView;
            if (showMapView) { this.ClientSize = new Size(ViewW, ViewH); this.Location = new Point(0, 0); }
            else { this.ClientSize = new Size(WinW, WinH); this.CenterToScreen(); }
            if (currentState != GameState.Playing) UpdateButtonPositions();
        }

        private void UpdateButtonPositions()
        {
            btnRetry.Location = new Point(this.ClientSize.Width / 2 - 150, this.ClientSize.Height / 2 + 60);
            btnExit.Location = new Point(this.ClientSize.Width / 2 + 10, this.ClientSize.Height / 2 + 60);
        }

        private void InitUI()
        {
            btnRetry.Text = "Restart"; btnRetry.Font = new Font("Arial", 12); btnRetry.Size = new Size(140, 45);
            btnRetry.Visible = false; btnRetry.Click += (s, e) => StartNewGame();
            btnExit.Text = "Exit"; btnExit.Font = new Font("Arial", 12); btnExit.Size = new Size(140, 45);
            btnExit.Visible = false; btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnRetry); this.Controls.Add(btnExit);
        }

        private void StartNewGame()
        {
            gameTimer.Stop(); pressedKeys.Clear(); collectedCount = 0; isQuestAccepted = false; showDialogue = false;
            worldX = 1; worldY = 1;
            playerPos = new PointF(WinW / 2 - PlayerW / 2, WinH / 2 - PlayerH / 2);
            btnRetry.Visible = btnExit.Visible = false; currentState = GameState.Playing;
            SetupWorldLayout(); this.Focus(); gameTimer.Start();
        }

        private void SetupWorldLayout()
        {
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    world[x, y] = new Room();

            int cT = 120, cB = 300, vL = 250, vR = 550, pW = 300, gT = 200, gB = 400;

            world[0, 0].Walls.Add(new Rectangle(0, 0, WinW, cT)); world[0, 0].Walls.Add(new Rectangle(0, 0, 60, WinH)); world[0, 0].Walls.Add(new Rectangle(pW, cB, WinW - pW, WinH - cB));
            world[0, 0].Items.Add(new Rectangle(120, 180, ItemSize, ItemSize)); world[0, 0].Enemies.Add(new PointF(200, 200));

            world[1, 0].Walls.Add(new Rectangle(0, 0, WinW, cT)); world[1, 0].Walls.Add(new Rectangle(0, cB, vL, WinH - cB)); world[1, 0].Walls.Add(new Rectangle(vR, cB, WinW - vR, WinH - cB));
            world[1, 0].Items.Add(new Rectangle(WinW / 2 - ItemSize / 2, cT + 20, ItemSize, ItemSize)); world[1, 0].Enemies.Add(new PointF(WinW / 2 + 100, cT + 80));

            world[2, 0].Walls.Add(new Rectangle(0, 0, WinW, cT)); world[2, 0].Walls.Add(new Rectangle(WinW - 60, 0, 60, WinH)); world[2, 0].Walls.Add(new Rectangle(0, cB, 500, WinH - cB));
            world[2, 0].Items.Add(new Rectangle(WinW - 150, 180, ItemSize, ItemSize)); world[2, 0].Enemies.Add(new PointF(WinW - 240, 200));

            world[0, 1].Walls.Add(new Rectangle(0, 0, 60, WinH)); world[0, 1].Walls.Add(new Rectangle(pW, 0, WinW - pW, gT)); world[0, 1].Walls.Add(new Rectangle(pW, gB, WinW - pW, WinH - gB));
            world[0, 1].Items.Add(new Rectangle(120, WinH / 2, ItemSize, ItemSize)); world[0, 1].Enemies.Add(new PointF(400, WinH / 2));

            world[1, 1].Walls.Add(new Rectangle(0, 0, vL, gT));
            world[1, 1].Walls.Add(new Rectangle(vR, 0, WinW - vR, gT));
            world[1, 1].Walls.Add(new Rectangle(vR, gB, WinW - vR, WinH - gB));
            world[1, 1].Walls.Add(new Rectangle(0, 550, vL, 50));
            world[1, 1].Walls.Add(new Rectangle(0, gB, 60, 150));
            world[1, 1].NPCs.Add(new PointF(150, 460));

            world[2, 1].Walls.Add(new Rectangle(WinW - 60, 0, 60, WinH)); world[2, 1].Walls.Add(new Rectangle(0, 0, 500, gT)); world[2, 1].Walls.Add(new Rectangle(0, gB, 500, WinH - gB));
            world[2, 1].Items.Add(new Rectangle(WinW - 200, WinH / 2, ItemSize, ItemSize)); world[2, 1].Enemies.Add(new PointF(WinW - 450, WinH / 2));

            world[0, 2].Walls.Add(new Rectangle(0, WinH - cT, WinW, cT)); world[0, 2].Walls.Add(new Rectangle(0, 0, 60, WinH)); world[0, 2].Walls.Add(new Rectangle(pW, 0, WinW - pW, cB));
            world[0, 2].Items.Add(new Rectangle(120, WinH - 200, ItemSize, ItemSize)); world[0, 2].Enemies.Add(new PointF(200, WinH - 250));

            world[1, 2].Walls.Add(new Rectangle(0, WinH - cT, WinW, cT)); world[1, 2].Walls.Add(new Rectangle(0, 0, vL, cB)); world[1, 2].Walls.Add(new Rectangle(vR, 0, WinW - vR, cB));
            world[1, 2].Items.Add(new Rectangle(WinW / 2 - ItemSize / 2, WinH - 200, ItemSize, ItemSize)); world[1, 2].Enemies.Add(new PointF(WinW / 2 - 150, WinH - 250));

            world[2, 2].Walls.Add(new Rectangle(0, WinH - cT, WinW, cT)); world[2, 2].Walls.Add(new Rectangle(WinW - 60, 0, 60, WinH)); world[2, 2].Walls.Add(new Rectangle(0, 0, 500, cB));
            world[2, 2].Items.Add(new Rectangle(WinW - 150, WinH - 200, ItemSize, ItemSize)); world[2, 2].Enemies.Add(new PointF(WinW - 240, WinH - 250));
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (currentState != GameState.Playing) return;
            float nX = playerPos.X, nY = playerPos.Y;
            isMoving = false;
            if (pressedKeys.Contains(Keys.W)) { nY -= PlayerSpeed; playerFacing = "up"; isMoving = true; }
            if (pressedKeys.Contains(Keys.S)) { nY += PlayerSpeed; playerFacing = "down"; isMoving = true; }
            if (pressedKeys.Contains(Keys.A)) { nX -= PlayerSpeed; playerFacing = "left"; isMoving = true; }
            if (pressedKeys.Contains(Keys.D)) { nX += PlayerSpeed; playerFacing = "right"; isMoving = true; }

            if (!IsBoxColliding(nX, playerPos.Y, PlayerW, PlayerH)) playerPos.X = nX;
            if (!IsBoxColliding(playerPos.X, nY, PlayerW, PlayerH)) playerPos.Y = nY;

            if (!isQuestAccepted && worldX == 1 && worldY == 1)
            {
                playerPos.X = Math.Clamp(playerPos.X, 1, WinW - PlayerW - 1);
                playerPos.Y = Math.Clamp(playerPos.Y, 1, WinH - PlayerH - 1);
            }
            else
            {
                if (playerPos.X > WinW && worldX < 2) { worldX++; playerPos.X -= WinW; showDialogue = false; }
                else if (playerPos.X < -PlayerW && worldX > 0) { worldX--; playerPos.X += WinW; showDialogue = false; }
                if (playerPos.Y > WinH && worldY < 2) { worldY++; playerPos.Y -= WinH; showDialogue = false; }
                else if (playerPos.Y < -PlayerH && worldY > 0) { worldY--; playerPos.Y += WinH; showDialogue = false; }
            }
            playerPos.X = Math.Clamp(playerPos.X, worldX == 0 ? 0 : -PlayerW, worldX == 2 ? WinW - PlayerW : WinW);
            playerPos.Y = Math.Clamp(playerPos.Y, worldY == 0 ? 0 : -PlayerH, worldY == 2 ? WinH - PlayerH : WinH);

            UpdateEnemies(); CheckItemCollisions(); this.Invalidate();
        }

        private void UpdateEnemies()
        {
            var room = world[worldX, worldY];
            for (int i = 0; i < room.Enemies.Count; i++)
            {
                PointF eP = room.Enemies[i]; float dx = playerPos.X - eP.X, dy = playerPos.Y - eP.Y, d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d > 1)
                {
                    float sX = (dx / d) * EnemySpeed, sY = (dy / d) * EnemySpeed;
                    if (!IsBoxColliding(eP.X + sX, eP.Y, EnemySize, EnemySize)) eP.X += sX;
                    if (!IsBoxColliding(eP.X, eP.Y + sY, EnemySize, EnemySize)) eP.Y += sY;
                    room.Enemies[i] = eP;
                }
                if (new RectangleF(playerPos.X, playerPos.Y, PlayerW, PlayerH).IntersectsWith(new RectangleF(eP.X, eP.Y, EnemySize, EnemySize))) GameOver(GameState.Lost);
            }
        }

        private void CheckItemCollisions()
        {
            if (!isQuestAccepted) return;
            var room = world[worldX, worldY];
            for (int i = room.Items.Count - 1; i >= 0; i--)
                if (new RectangleF(playerPos.X, playerPos.Y, PlayerW, PlayerH).IntersectsWith(room.Items[i])) { room.Items.RemoveAt(i); collectedCount++; }
        }

        private bool IsBoxColliding(float x, float y, float w, float h)
        {
            RectangleF r = new RectangleF(x, y, w, h); var room = world[worldX, worldY];
            foreach (var wall in room.Walls) if (r.IntersectsWith(wall)) return true;
            foreach (var npc in room.NPCs) if (r.IntersectsWith(new RectangleF(npc.X, npc.Y, PlayerW, PlayerH))) return true;
            return false;
        }

        private void GameOver(GameState state) { gameTimer.Stop(); currentState = state; UpdateButtonPositions(); btnRetry.Visible = btnExit.Visible = true; this.Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.Clear(Color.FromArgb(30, 30, 30));
            float gPX = worldX * WinW + playerPos.X, gPY = worldY * WinH + playerPos.Y;

            if (showMapView)
            {
                float camX = Math.Clamp(gPX - this.ClientSize.Width / 2, 0, WinW * 3 - this.ClientSize.Width);
                float camY = Math.Clamp(gPY - this.ClientSize.Height / 2, 0, WinH * 3 - this.ClientSize.Height);
                g.TranslateTransform(-camX, -camY);
                for (int y = 0; y < 3; y++) for (int x = 0; x < 3; x++) DrawRoom(g, world[x, y], x * WinW, y * WinH);
                sprites.DrawPlayer(g, gPX, gPY, PlayerW, PlayerH, playerFacing, isMoving, pColor);
                g.ResetTransform();
            }
            else
            {
                DrawRoom(g, world[worldX, worldY], 0, 0);
                sprites.DrawPlayer(g, playerPos.X, playerPos.Y, PlayerW, PlayerH, playerFacing, isMoving, pColor);
            }

            if (currentState == GameState.Playing)
            {
                string goal = isQuestAccepted ? (collectedCount < totalItems ? $"Collected: {collectedCount}/{totalItems}" : "GOAL: RETURN TO NPC!") : "FIND THE NPC TO START";
                g.DrawString(goal, new Font("Arial", 12, FontStyle.Bold), isQuestAccepted ? Brushes.White : Brushes.Gold, 10, 10);
                g.DrawString("Press [I] for Info | [F] for Map", new Font("Arial", 10), Brushes.LightGray, 10, 30);
                if (showInfoWindow)
                {
                    int cX = this.ClientSize.Width / 2, cY = this.ClientSize.Height / 2; g.FillRectangle(new SolidBrush(Color.FromArgb(220, 0, 0, 0)), cX - 200, cY - 150, 400, 300);
                    g.DrawString("CONTROLS\n\nWASD - Move\nE - Interact\nQ - Exit\nR - Restart\nI - Info\nF - Map", new Font("Consolas", 12, FontStyle.Bold), Brushes.White, cX - 180, cY - 130);
                }
            }
            else
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                string m = currentState == GameState.Won ? "VICTORY!" : "DEFEAT"; g.DrawString(m, new Font("Arial", 45, FontStyle.Bold), currentState == GameState.Won ? Brushes.Gold : Brushes.Red, this.ClientSize.Width / 2 - 130, this.ClientSize.Height / 2 - 60);
            }
        }

        private void DrawRoom(Graphics g, Room r, int ox, int oy)
        {
            foreach (var w in r.Walls) g.FillRectangle(Brushes.Black, ox + w.X, oy + w.Y, w.Width, w.Height);
            using (SolidBrush ib = new SolidBrush(iColor)) foreach (var it in r.Items) g.FillRectangle(ib, ox + it.X, oy + it.Y, ItemSize, ItemSize);
            using (SolidBrush eb = new SolidBrush(eColor)) foreach (var en in r.Enemies) g.FillRectangle(eb, ox + en.X, oy + en.Y, EnemySize, EnemySize);
            using (SolidBrush nb = new SolidBrush(nColor)) foreach (var npc in r.NPCs)
                {
                    g.FillRectangle(nb, ox + npc.X, oy + npc.Y, PlayerW, PlayerH);
                    if (!isQuestAccepted) g.DrawString("!", new Font("Arial", 14, FontStyle.Bold), Brushes.Yellow, ox + npc.X + (PlayerW / 2 - 5), oy + npc.Y - 25);
                    if (showDialogue && isQuestAccepted && collectedCount < totalItems) g.DrawString("Collect 8 particles!", new Font("Arial", 9, FontStyle.Bold), Brushes.White, ox + npc.X - 30, oy + npc.Y - 20);
                    else if (collectedCount == totalItems) g.DrawString("Return items! [E]", new Font("Arial", 9, FontStyle.Bold), Brushes.Gold, ox + npc.X - 30, oy + npc.Y - 20);
                }
            using (Pen p = new Pen(Color.FromArgb(60, Color.White), 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }) g.DrawRectangle(p, ox, oy, WinW, WinH);
        }
    }

    public class Room
    {
        public List<Rectangle> Walls = new List<Rectangle>(), Items = new List<Rectangle>();
        public List<PointF> Enemies = new List<PointF>(), NPCs = new List<PointF>();
    }
}