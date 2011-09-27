using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GXT.Audio
{
    /// <summary>
    /// 
    /// 
    /// Author: Jeff Lansing
    /// </summary>
    public class gxtAudioScene
    {
        private bool enabled;
        private SoundBank soundBank;
        private WaveBank waveBank;
        private List<gxtAudioCollection> audioCollections;

        public bool Enabled { get { return enabled; } set { enabled = value; } }

        public SoundBank SoundBank { get { return soundBank; } set { soundBank = value; } }

        public WaveBank WaveBank { get { return waveBank; } set { waveBank = value; } }

        #region AudioCollection
        private class gxtAudioCollection
        {
            public AudioCategory category;
            public List<Cue> cues;
            private float volume;

            public float Volume
            {
                get { return volume; }
                set
                {
                    if (value != volume)
                    {
                        volume = value;
                        category.SetVolume(volume * gxtAudioManager.Singleton.MasterVolume);
                    }
                }
            }

            public gxtAudioCollection(AudioCategory category)
            {
                this.category = category;
                cues = new List<Cue>();
                Volume = 1.0f;
            }

            private bool FindCue(string cueName, out Cue cue)
            {
                cue = cues.Find(item => item.Name == cueName);
                if (cue == null)
                {
                    gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Cue: {0} Does Not Exist in the Audio Category: {1}", cueName, category.Name);
                    return false;
                }
                return true;
            }

            public void AddCue(Cue cue)
            {
                cues.Add(cue);
            }

            public bool RemoveCue(Cue cue)
            {
                return cues.Remove(cue);
            }

            public void Pause(string cueName)
            {
                Cue cue;
                if (FindCue(cueName, out cue))
                    cue.Pause();
            }

            public void Resume(string cueName)
            {
                Cue cue;
                if (FindCue(cueName, out cue))
                    cue.Resume();
            }

            public void Stop(string cueName)
            {
                Cue cue;
                if (FindCue(cueName, out cue))
                    cue.Stop(AudioStopOptions.Immediate);
            }

            public void Update()
            {
                for (int i = cues.Count - 1; i > 0; i--)
                {
                    if (cues[i].IsDisposed)
                        cues.RemoveAt(i);
                }
            }
        }
        #endregion AudioCollection

        public void Initialize(string soundBankFile, string waveBankFile, bool initEnabled = true)
        {
            enabled = initEnabled;
            audioCollections = new List<gxtAudioCollection>();
            gxtAudioManager.Singleton.AddAudioScene(this, soundBankFile, waveBankFile);
        }

        public void AddCategory(string name)
        {
            AudioCategory category = gxtAudioManager.Singleton.GetCategory(name);
            if (!audioCollections.Exists(item => item.category == category))
            {
                gxtAudioCollection collection = new gxtAudioCollection(category);
                audioCollections.Add(collection);
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Audio Category: {0} already exists in the audio scene", name);
            }
        }

        public bool ContainsCategory(string name)
        {
            bool exists = audioCollections.Exists(item => item.category.Name == name);
            if (!exists)
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Category: {0} Does Not Exist in the Audio Scene");
            return exists;
        }

        public bool RemoveCategory(string name)
        {
            int removed = audioCollections.RemoveAll(item => item.category.Name == name);
            gxtDebug.Assert(removed > 1, "Multple audio categories in one scene");
            return removed > 0;
        }

        public void Clear()
        {
            audioCollections.Clear();
        }

        private bool FindCollection(string name, out gxtAudioCollection collection)
        {
            collection = audioCollections.Find(item => item.category.Name == name);
            return collection != null;
        }

        public void Update()
        {
            if (!enabled)
                return;
            for (int i = 0; i < audioCollections.Count; i++)
            {
                audioCollections[i].Update();
            }
        }

        public void PauseAll()
        {
            for (int i = 0; i < audioCollections.Count; i++)
            {
                audioCollections[i].category.Pause();
            }
        }

        public void PauseCategory(string categoryName)
        {
            gxtAudioCollection collection;
            if (FindCollection(categoryName, out collection))
            {
                collection.category.Pause();
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Audio Category: {0} Not Found in the Scene", categoryName);
            }
        }

        public void PauseCue(string categoryName, string cueName)
        {
            gxtAudioCollection collection;
            if (FindCollection(categoryName, out collection))
            {
                collection.Pause(cueName);
            }
        }

        public void ResumeAll()
        {
            for (int i = 0; i < audioCollections.Count; i++)
            {
                audioCollections[i].category.Resume();
            }
        }

        public void ResumeCategory(string categoryName)
        {
            gxtAudioCollection collection;
            if (FindCollection(categoryName, out collection))
            {
                collection.category.Resume();
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Audio Category: {0} Not Found in the Scene", categoryName);
            }
        }

        public void ResumeCue(string categoryName, string cueName)
        {
            gxtAudioCollection collection;
            if (FindCollection(categoryName, out collection))
            {
                collection.Resume(cueName);
            }
        }

        public void StopAll()
        {
            for (int i = 0; i < audioCollections.Count; i++)
            {
                audioCollections[i].category.Stop(AudioStopOptions.Immediate);  // make variable??
            }
        }

        public void StopCategory(string categoryName)
        {
            gxtAudioCollection collection;
            if (FindCollection(categoryName, out collection))
            {
                collection.category.Stop(AudioStopOptions.Immediate);
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Audio Category: {0} Not Found in the Scene", categoryName);
            }
        }

        public void StopCue(string categoryName, string cueName)
        {
            gxtAudioCollection collection;
            if (FindCollection(categoryName, out collection))
            {
                collection.Resume(cueName);
            }
        }

        public float GetVolume(string category)
        {
            gxtAudioCollection collection;
            if (FindCollection(category, out collection))
            {
                return collection.Volume;
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Audio Category: {0} Not Found in the Scene", category);
                return 1.0f;
            }
        }

        public void SetVolume(string category, float volume)
        {
            gxtDebug.Assert(volume >= 0.0f);
            gxtAudioCollection collection;
            if (FindCollection(category, out collection))
            {
                collection.Volume = volume;
            }
            else
            {
                gxtLog.WriteLineV(gxtVerbosityLevel.WARNING, "Audio Category: {0} Not Found in the Scene", category);
            }
        }

        public void PlaySound(string categoryName, string cueName)
        {
            gxtAudioCollection collection;
            if (FindCollection(categoryName, out collection))
            {
                Cue cue = SoundBank.GetCue(cueName);
                cue.Play();
                collection.AddCue(cue);
            }
        }

        // playsound (cat, name, position, listenradius)
        // updatelistener(position, listenradius)
        // updatesoundposition(cat, name, new position, new listenradius)
    }
}
