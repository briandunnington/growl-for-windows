using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class PrefSound : DefaultablePreference
    {
        public static PrefSound Default = new PrefSound(null, DEFAULT_DISPLAY_LABEL, null, true);
        public static PrefSound None = new PrefSound(false, Properties.Resources.PrefSound_None, null);

        private bool? playSound = null;
        private string soundFile = null;


        private PrefSound(bool? playSound, string name, string soundFile)
            : this(playSound, name, soundFile, false)
        {
        }

        private PrefSound(bool? playSound, string name, string soundFile, bool isDefault)
            : base(name, isDefault)
        {
            this.playSound = playSound;
            this.soundFile = soundFile;
        }

        public bool? PlaySound
        {
            get
            {
                return this.playSound;
            }
        }

        public string SoundFile
        {
            get
            {
                return this.soundFile;
            }
        }

        public static PrefSound[] GetList(bool allowDefault)
        {
            List<PrefSound> list = new List<PrefSound>();
            if (allowDefault) list.Add(Default);
            list.Add(None);

            // read available sounds from C:\WINDOWS\Media
#if !MONO
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string windowsPath = System.IO.Path.GetDirectoryName(systemPath);
            string mediaPath = System.IO.Path.Combine(windowsPath, "Media");
            if (System.IO.Directory.Exists(mediaPath))
            {
                System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(mediaPath);
                System.IO.FileInfo[] files = d.GetFiles("*.wav");
                foreach (System.IO.FileInfo file in files)
                {
                    PrefSound ps = new PrefSound(true, file.Name, file.FullName);
                    list.Add(ps);
                }
            }
#endif
            PrefSound[] arr = list.ToArray();
            return arr;
        }

        public override int GetHashCode()
        {
            if (this.playSound.HasValue && this.playSound.Value)
                return this.soundFile.GetHashCode();
            else
                return this.playSound.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PrefSound s = obj as PrefSound;
            if (s != null)
            {
                return this.Name == s.ActualName;
            }
            else
                return base.Equals(obj);
        }

        internal static PrefSound FromFilePath(string filePath)
        {
            try
            {
                if (!String.IsNullOrEmpty(filePath))
                {
                    System.IO.FileInfo file = new System.IO.FileInfo(filePath);
                    PrefSound ps = new PrefSound(true, file.Name, file.FullName);
                    return ps;
                }
            }
            catch
            {
            }
            return PrefSound.None;
        }
    }
}
