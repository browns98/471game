using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public partial class Game : Form
    {
        /// <summary>
        /// The DirectX device we will draw on
        /// </summary>
        private Device device = null;

        /// <summary> 
        /// Height of our playing area (meters)
        /// </summary>
        private float playingH = 4;

        /// <summary>
        /// Width of our playing area (meters)
        /// </summary>
        private float playingW = 7.2f;

        /// <summary>
        /// Vertex buffer for our drawing
        /// </summary>
        private VertexBuffer vertices = null;

        /// <summary>
        /// The background image class
        /// </summary>
        private Background background = null;

        /// <summary>
        /// All of the enemies in the game
        /// </summary>
        List<Polygon> enemies = new List<Polygon>();

        /// <summary>
        /// All of the lasers in play
        /// </summary>
        List<Polygon> lasers = new List<Polygon>();

        /// <summary>
        /// Our player sprite
        /// </summary>
        GameSprite player = new GameSprite();

        /// <summary>
        /// The collision testing subsystem
        /// </summary>
        Collision collision = new Collision();

        /// <summary>
        /// The sounds
        /// </summary>
        private GameSounds sounds;
        
        /// <summary>
        /// What the last time reading was
        /// </summary>
        private long lastTime;

        private long lastShot;

        /// <summary>
        /// A stopwatch to use to keep track of time
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        
        /// <summary>
        /// Initialize the Direct3D device for rendering
        /// </summary>
        /// <returns>true if successful</returns>
        private bool InitializeDirect3D()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        //TODO: Add up and down buttons for movement
        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close(); // Esc was pressed
            else if (e.KeyCode == Keys.Right)
            {
                Vector2 v = player.V;
                v.X = 2.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Left)
            {
                Vector2 v = player.V;
                v.X = -2.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Up)
            {
                Vector2 v = player.V;
                v.Y = 2.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Down)
            {
                Vector2 v = player.V;
                v.Y = -2.5f;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Space && stopwatch.ElapsedMilliseconds > lastShot + 400)
            {
                //TODO: Make the player shoot
                AddLaser(player.P);
                sounds.Shoot();
                lastShot = stopwatch.ElapsedMilliseconds;
            }

        }

        //TODO: Make this work for up and down for the player as well
        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                Vector2 v = player.V;
                v.X = 0;
                player.V = v;
            }
            else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                Vector2 v = player.V;
                v.Y = 0;
                player.V = v;
            }
        }

        public Game()
        {
            InitializeComponent();
            if (!InitializeDirect3D())
                return;

            vertices = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
                                        4,      // How many
                                        device, // What device
                                        0,      // No special usage
                                        CustomVertex.PositionColored.Format,
                                        Pool.Managed);

            background = new Background(device, playingW+2, playingH);
            sounds = new GameSounds(this);

            // Determine the last time
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;

            Texture spritetexture = TextureLoader.FromFile(device, "../../ship.bmp");
            player.Tex = spritetexture;

            float L = 0.225f;
            float Y = 0.05f;

            player.AddVertex(new Vector2(-L, L));
            player.AddTex(new Vector2(Y, 0));

            player.AddVertex(new Vector2(0, L));
            player.AddTex(new Vector2(0.5f, 0));

            player.AddVertex(new Vector2(L, 0.005f));
            player.AddTex(new Vector2(1.0f-Y, 0.5f-0.005f));

            player.AddVertex(new Vector2(L, -0.005f));
            player.AddTex(new Vector2(1.0f-Y, 0.5f+0.005f));

            player.AddVertex(new Vector2(0, -L));
            player.AddTex(new Vector2(0.5f, 1));

            player.AddVertex(new Vector2(-L, -L));
            player.AddTex(new Vector2(Y, 1));

            player.Color = Color.Transparent;
            player.Transparent = true;
            player.P = new Vector2(2f, 2);

            AddEnemy(new Vector2(6, 2), 1);
            AddEnemy(new Vector2(8, 2), 2);
            AddEnemy(new Vector2(10, 2), 3);
            AddEnemy(new Vector2(12, 2), 4);
        }


        //TODO: Need to render enemies at certain times. Don't know when, but probably need to put that here or in Advance
        public void Render()
        {
            //This is the soundtrack. This can be changed if necessary
            //sounds.Soundtrack();
            if (device == null)
                return;

            device.Clear(ClearFlags.Target, System.Drawing.Color.Blue, 1.0f, 0);

            int wid = Width;                            // Width of our display window
            int hit = Height;                           // Height of our display window.
            float aspect = (float)wid / (float)hit;     // What is the aspect ratio?

            device.RenderState.ZBufferEnable = false;   // We'll not use this feature
            device.RenderState.Lighting = false;        // Or this one...
            device.RenderState.CullMode = Cull.None;    // Or this one...

            float widP = playingH * aspect;         // Total width of window
            
            float winCenter = player.P.X;
            //if (winCenter - widP / 2 < 0)
                winCenter = widP / 2;
            //else if (winCenter + widP / 2 > playingW)
                //winCenter = playingW - widP / 2;

            device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP / 2,
                                                                  winCenter + widP / 2,
                                                                  0, playingH, 0, 1);

            //Begin the scene
            device.BeginScene();

            // Render the background
            background.Render();

            foreach (Polygon p in lasers)
            {
                p.Render(device);
            }

            foreach (Polygon p in enemies)
                p.Render(device);

            player.Render(device);

            //End the scene
            device.EndScene();
            device.Present();
        }

        /// <summary>
        /// Advance the game in time
        /// </summary>
        public void Advance()
        {
            // How much time change has there been?
            long time = stopwatch.ElapsedMilliseconds;
            float delta = (time - lastTime) * 0.001f;       // Delta time in milliseconds
            lastTime = time;

            Vector2 q = player.V;
            Vector2 r = player.P;

            //These added borders, is there a better way to do this?
            if (player.P.X < 0.1f)
            {
                q.X = 0;
                r.X = 0.1f;
            }
            else if (player.P.X > playingW - 0.25f)
            {
                q.X = 0;
                r.X = playingW-0.25f;
            }
            if (player.P.Y < 0.1f)
            {
                q.Y = 0;
                r.Y = 0.1f;
            }
            else if (player.P.Y > playingH - 0.1f)
            {
                q.Y = 0;
                r.Y = playingH - 0.1f;
            }

            player.P = r;
            player.V = q;
            player.Advance(0);

            while (delta > 0)
            {
                float step = delta;
                if (step > 0.05f)
                    step = 0.05f;

                float maxspeed = Math.Max(Math.Abs(player.V.X), Math.Abs(player.V.Y));
                if (maxspeed > 0)
                {
                    step = (float)Math.Min(step, 0.05 / maxspeed);
                }

                player.Advance(step);

                foreach (Polygon p in lasers)
                    p.Advance(step);

                foreach (Polygon p in enemies)
                    p.Advance(step);

                foreach (Polygon p in enemies)
                {
                    if (collision.Test(player, p))
                    {
                        float depth = collision.P1inP2 ?
                                  collision.Depth : -collision.Depth;
                        player.P = player.P + collision.N * depth;
                        Vector2 v = player.V;
                        if (collision.N.X != 0)
                            v.X = 0;
                        if (collision.N.Y != 0)
                            v.Y = 0;
                        player.V = v;
                        player.Advance(0);
                    }
                }

                List<Polygon> newlasers = new List<Polygon>();
                List<Polygon> tempenemies = enemies;
                bool hit = false;

                foreach (Polygon f in lasers)
                {
                    List<Polygon> newenemies = new List<Polygon>();
                    hit = false;
                    if (tempenemies.Count() > 0)
                    {
                        foreach (Polygon p in tempenemies)
                        {
                            if (collision.Test(f, p))
                            {
                                // Score a collision with p
                                // and we won't need this laser anymore.
                                hit = true;
                                sounds.Explosion();
                            }
                            else
                            {
                                // Otherwise, we still need the enemy
                                newenemies.Add(p);
                            }
                        }
                    }
                    if (!hit)
                        newlasers.Add(f);
                    tempenemies = newenemies;
                }

                lasers = newlasers;
                enemies = tempenemies;

                
                delta -= step;
            }
        }

        public void AddLaser(Vector2 p)
        {
            float left = 0;
            float right = 0.1f;
            float top = 0.1f;
            float bottom = 0;
            GameSprite obs = new GameSprite();

            Texture lasertexture = TextureLoader.FromFile(device, "../../redLaser.bmp");
            obs.Tex = lasertexture;

            obs.AddVertex(new Vector2(left, top));
            obs.AddTex(new Vector2(0, 0));
            obs.AddVertex(new Vector2(right, top));
            obs.AddTex(new Vector2(1, 0));
            obs.AddVertex(new Vector2(right, bottom));
            obs.AddTex(new Vector2(1, 1));
            obs.AddVertex(new Vector2(left, bottom));
            obs.AddTex(new Vector2(0, 1));
            obs.Color = Color.Transparent;
            obs.Transparent = true;
            Vector2 q = p;
            q.Y -= 0.05f;
            q.X += 0.25f;
            obs.P = q;
            obs.V = new Vector2(3, 0);
            lasers.Add(obs);
        }

        public void AddEnemy(Vector2 p, int type)
        {
            GameSprite enemy = new GameSprite();

            Texture enemytexture;
            switch (type)
            {
                case 2:
                    enemy.AddVertex(new Vector2(-0.2f, 0.09f));
                    enemy.AddVertex(new Vector2(-0.065f, 0.15f));
                    enemy.AddVertex(new Vector2(0.065f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0.09f));
                    enemy.AddVertex(new Vector2(0.2f, -0.15f));
                    enemy.AddVertex(new Vector2(-0.2f, -0.15f));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy2.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0.2f));
                    enemy.AddTex(new Vector2(0.32f, 0.1f));
                    enemy.AddTex(new Vector2(0.64f, 0.1f));
                    enemy.AddTex(new Vector2(1, 0.2f));
                    enemy.AddTex(new Vector2(1, 1));
                    enemy.AddTex(new Vector2(0, 1));
                    break;
                case 3:
                    enemy.AddVertex(new Vector2(-0.2f, 0.07f));
                    enemy.AddVertex(new Vector2(-0.07f, 0.2f));
                    enemy.AddVertex(new Vector2(0.07f, 0.2f));
                    enemy.AddVertex(new Vector2(0.2f, 0.07f));
                    enemy.AddVertex(new Vector2(0.2f, -0.2f));
                    enemy.AddVertex(new Vector2(-0.2f, -0.2f));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy3.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0.36f));
                    enemy.AddTex(new Vector2(0.36f, 0.06f));
                    enemy.AddTex(new Vector2(0.64f, 0.06f));
                    enemy.AddTex(new Vector2(1, 0.36f));
                    enemy.AddTex(new Vector2(1, 1));
                    enemy.AddTex(new Vector2(0, 1));
                    break;
                case 4:
                    enemy.AddVertex(new Vector2(-0.2f, 0));
                    enemy.AddVertex(new Vector2(-0.05f, 0.15f));
                    enemy.AddVertex(new Vector2(0.05f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0));
                    enemy.AddVertex(new Vector2(0.15f, -0.05f));
                    enemy.AddVertex(new Vector2(-0.15f, -0.05f));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy4.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0.55f));
                    enemy.AddTex(new Vector2(0.32f, 0.14f));
                    enemy.AddTex(new Vector2(0.68f, 0.14f));
                    enemy.AddTex(new Vector2(1, 0.55f));
                    enemy.AddTex(new Vector2(1, 0.8f));
                    enemy.AddTex(new Vector2(0, 0.8f));
                    break;
                default:
                    enemy.AddVertex(new Vector2(-0.2f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0.15f));
                    enemy.AddVertex(new Vector2(0.2f, 0));
                    enemy.AddVertex(new Vector2(0.15f, -0.15f));
                    enemy.AddVertex(new Vector2(-0.15f, -0.15f));
                    enemy.AddVertex(new Vector2(-0.2f, 0));
                    enemytexture = TextureLoader.FromFile(device, "../../enemy1.bmp");
                    enemy.Tex = enemytexture;
                    enemy.AddTex(new Vector2(0, 0));
                    enemy.AddTex(new Vector2(1, 0));
                    enemy.AddTex(new Vector2(1, 0.5f));
                    enemy.AddTex(new Vector2(0.86f, 0.93f));
                    enemy.AddTex(new Vector2(0.14f, 0.93f));
                    enemy.AddTex(new Vector2(0, 0.5f));
                    break;
            }

            enemy.Color = Color.Transparent;
            enemy.Transparent = true;
            enemy.P = p;
            //This sets the original velocity. 
            //TODO: Figure out how to change Y velocity depending on enemy
            enemy.V = new Vector2(-1.5f, 0);
            enemies.Add(enemy);
        }
    }
}
