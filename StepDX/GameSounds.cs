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

        private SecondaryBuffer explosion = null;
        private SecondaryBuffer shoot = null;
        private SecondaryBuffer soundtrack = null;

        public GameSounds(Form form)
        {
            SoundDevice = new Device();
            SoundDevice.SetCooperativeLevel(form, CooperativeLevel.Priority);

            Load(ref shoot, "../../shoot.wav");
            Load(ref soundtrack, "../../shoot.wav");

            Load(ref explosion, "../../explosion.wav");
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

        public void Explosion()
        {
            if (explosion == null)
                return;

            explosion.SetCurrentPosition(0);
            explosion.Play(0, BufferPlayFlags.Default);
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