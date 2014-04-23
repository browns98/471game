using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace StepDX
{
    class GameSounds
    {
        private Device SoundDevice = null;

        private SecondaryBuffer[] clank = new SecondaryBuffer[10];
        int clankToUse = 0;

        private SecondaryBuffer shoot = null;
        private SecondaryBuffer soundtrack = null;

        public GameSounds(Form form)
        {
            SoundDevice = new Device();
            SoundDevice.SetCooperativeLevel(form, CooperativeLevel.Priority);

            Load(ref shoot, "../../shoot.wav");
            Load(ref soundtrack, "../../shoot.wav");

            for (int i = 0; i < clank.Length; i++)
                Load(ref clank[i], "../../explosion.wav");
        }

        private void Load(ref SecondaryBuffer buffer, string filename)
        {
            try
            {
                buffer = new SecondaryBuffer(filename, SoundDevice);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to load " + filename,
                                "Danger, Will Robinson", MessageBoxButtons.OK);
                buffer = null;
            }
        }

        public void Clank()
        {
            clankToUse = (clankToUse + 1) % clank.Length;

            if (clank[clankToUse] == null)
                return;

            if (!clank[clankToUse].Status.Playing)
                clank[clankToUse].Play(0, BufferPlayFlags.Default);
        }

        public void Shoot()
        {
            if (shoot == null)
                return;

            if (!shoot.Status.Playing)
                shoot.Play(0, BufferPlayFlags.Default);
        }

        public void Soundtrack()
        {
            if (soundtrack == null)
                return;

            if (!soundtrack.Status.Playing)
            {
                soundtrack.SetCurrentPosition(0);
                soundtrack.Play(0, BufferPlayFlags.Default);
            }
        }

        public void SoundtrackEnd()
        {
            if (soundtrack == null)
                return;

            if (soundtrack.Status.Playing)
                soundtrack.Stop();
        }

    }
}