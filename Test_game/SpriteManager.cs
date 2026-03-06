using System;
using System.Collections.Generic;
using System.Drawing;

namespace Test_game
{
    public class SpriteManager
    {
        private Dictionary<string, Image[]> walkSprites = new Dictionary<string, Image[]>();
        private Dictionary<string, Image> idleSprites = new Dictionary<string, Image>();
        private int animFrame = 0;
        private int frameTick = 0;
        private const int AnimSpeed = 8;

        public void LoadSprites(string path)
        {
            try
            {
                Image[] all = new Image[12];
                for (int i = 0; i < 12; i++)
                    all[i] = Image.FromFile(System.IO.Path.Combine(path, $"p{i}.png"));

                idleSprites["down"] = all[0];
                walkSprites["down"] = new Image[] { all[1], all[2] };
                idleSprites["left"] = all[3];
                walkSprites["left"] = new Image[] { all[4], all[5] };
                idleSprites["up"] = all[6];
                walkSprites["up"] = new Image[] { all[7], all[8] };
                idleSprites["right"] = all[9];
                walkSprites["right"] = new Image[] { all[10], all[11] };
            }
            catch { }
        }

        public void DrawPlayer(Graphics g, float x, float y, int w, int h, string dir, bool isMoving, Color fallback)
        {
            if (!idleSprites.ContainsKey("down"))
            {
                using (SolidBrush b = new SolidBrush(fallback)) g.FillRectangle(b, x, y, w, h);
                return;
            }

            Image img = isMoving ? walkSprites[dir][animFrame] : idleSprites[dir];
            if (isMoving)
            {
                if (++frameTick >= AnimSpeed) { animFrame = (animFrame == 0) ? 1 : 0; frameTick = 0; }
            }
            g.DrawImage(img, x, y, w, h);
        }
    }
}