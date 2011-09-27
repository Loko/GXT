using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GXT.Audio
{
    /// <summary>
    /// This is the main audio manager.  Encapsulates core audio 
    /// functionality and needs.
    /// 
    /// Author: Jeff Lansing
    /// Base reference by Lawrence Jung
    /// 
    /// TODO: MAJOR REHAUL FOR GENERAL PURPOSE
    /// </summary>
    public class gxtAudioManager : gxtSingleton<gxtAudioManager>
    {
        private bool enabled;
        private AudioEngine audioEngine;
        private float masterVolume;
        private List<gxtAudioScene> audioScenes;

        /// <summary>
        /// Enabled flag
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Master Volume Multiplier
        /// </summary>
        public float MasterVolume { get { return masterVolume; } set { masterVolume = value; } }

        // get microphone method?

        public void Initialize(string audioEngineFile, bool initEnabled = true, float volume = 1.0f)
        {
            audioEngine = new AudioEngine(audioEngineFile);
            enabled = initEnabled;
            masterVolume = volume;
            audioScenes = new List<gxtAudioScene>();
        }

        public void Update()
        {
            audioEngine.Update();
        }

        public void AddAudioScene(gxtAudioScene audioScene, string soundBankFile, string waveBankFile)
        {
            audioScene.SoundBank = new SoundBank(audioEngine, soundBankFile);
            audioScene.WaveBank = new WaveBank(audioEngine, waveBankFile);
            audioScenes.Add(audioScene);
        }

        public bool RemoveAudioScene(gxtAudioScene audioScene)
        {
            return audioScenes.Remove(audioScene);
        }

        public AudioCategory GetCategory(string name)
        {
            return audioEngine.GetCategory(name);
        }
        /*
        #region Variables
        //Audio objects
        AudioEngine engine;//create an audio engine
        SoundBank sb; // create a soundbank
        WaveBank wb; //create a wavebank
        AudioCategory audioCat; //create audio category

        //private Cue cue;// create a cue for the music
        private Cue[] cueMusic; //array of cues for the music
        private Cue bgMusicCue; //cue for the background music
        private Cue seCue;//cue for sound effects
        private string musicName; //name of the music
        private float musicVolume = 0.0f; //music Volume
        private float musicPitch = 12.0f; //music pitch
        private float musicTempo = 12.0f; //music tempo
        public int musicValue, prevMusicValue, numMusic, currSongNumPlay;// music value
        private float MAX_VOLUME = 100.0f; //maximum volume value
        private float MIN_VOLUME = 0.0f;//minimum volume value
        private bool played = false; //check to see if all the music have been played already
        private bool paused = false; //check to see if the music have been paused

        #endregion

        /// <summary>
        /// The Audio default constructor
        /// </summary>
        /// 
        #region Audio Methods
        //Audio default constructor, set the inital values
        public gxtAudioManager()
        {
            musicValue = 12;
            musicPitch = 12;
            musicTempo = 12;
            numMusic = 6;
            currSongNumPlay = 0;
        }
        //Initialize takes in the audioengine file path, sound bank file path and the wave bank file path
        //set the engine, soundbank and wavebank, create a new Cue array, set the music value and get the audio
        //Category, set the music Volume
        public void Initialize(string aePath, string sbPath, string wbPath)
        {
            engine = new AudioEngine(aePath);
            sb = new SoundBank(engine, sbPath);
            wb = new WaveBank(engine, wbPath);

            cueMusic = new Cue[numMusic];
            prevMusicValue = musicValue;
            audioCat = engine.GetCategory("World");
        }
        //The Update method takes in gameTime, updates the audio engine
        public void Update(GameTime gameTime)
        {
            // Console.WriteLine(currSongNumPlay);
            //SetVolume();
            //SetPitch();
            //SetTempo();
            engine.Update();
        }

        //PlaySoundEffects takes in a string that represents the soundeffect file name.
        public void PlaySoundEffects(string soundEffect)
        {
            seCue = sb.GetCue(soundEffect);
            seCue.Play();
        }

        //PlayBackGround music takes in a string that represents the music name. If is the song
        //is not playing, it gets the song from the library and plays it
        public void PlayBackGround(string song)
        {
            Console.WriteLine(song);
            if (song != musicName)
            {
                if (bgMusicCue != null && bgMusicCue.IsPlaying == true)
                {
                    bgMusicCue.Pause(); //pause the cue before playing
                }
                bgMusicCue = sb.GetCue(song); //gets the song from the library
                bgMusicCue.Play(); // play the song
                musicName = song; //set the music name
            }
        }

        //PauseBackGround music takes in a string that represent the music name. If the song is playing
        //pause it
        public void PauseBackGround(string song)
        {
            if (bgMusicCue.IsPlaying)
            {
                bgMusicCue.Pause();
            }
        }
        //ResumeBackGround music checks to see if the background music is paused. If it is 
        //resume the game
        public void ResumeBackGround()
        {
            if (bgMusicCue.IsPaused)
            {
                bgMusicCue.Resume();
            }
        }
        //PlaySoundMelody checks to see if is playing, If not it gets the music requested, play them and set
        //the volume to the minimum value
        public void PlaySoundMelody(string worldName) //will allow to pass in a string like world1 to get the correct music
        {
            Console.WriteLine("PlaySongMelody");
            if (!played)
            {
                for (int i = 0; i < numMusic; i++)
                {
                    cueMusic[i] = sb.GetCue(worldName + "_" + (i + 1) + ""); //(i + 1) //gets the music from the library here
                    cueMusic[i].Play();//plays that music
                    cueMusic[i].SetVariable("Volume", MIN_VOLUME); //set the minimum volume
                }
                played = true; //set the boolean to true
            }
        }

        //PauseSoundMelody checks to see if the world music is playing. If it is then
        //it loops through the cue music and pause all of them
        public void PauseSoundMelody(string worldName)
        {
            if (played && !cueMusic[0].IsPaused)
            {
                for (int i = 0; i < numMusic; i++)
                {
                    cueMusic[i].Pause();
                }
                paused = true;
            }
        }

        //ResumeSoundMelody checks to see if the world music is paused. It is paused
        //it loops through all the world music an resumes them.
        public void ResumeSoundMelody(string worldName)
        {
            if (played && cueMusic[0].IsPaused)
            {
                for (int i = 0; i < numMusic; i++)
                {
                    cueMusic[i].Resume();
                }
                paused = false;
            }
        }

        public void StopSoundMelody(string worldName)
        {
            if (played && cueMusic[0].IsPlaying)
            {
                for (int i = 0; i < numMusic; i++)
                {
                    cueMusic[i].Stop(AudioStopOptions.Immediate);
                }
                paused = false;
            }
        }
        #endregion
        //This region contains methods that changes the music
        #region ChangeMusic
        //setVolume, sets the volume of the audio Category
        public void SetVolume()
        {
            //musicVolume = MathHelper.Clamp(musicVolume, 0.0f, 100.0f);
            //musicTempo = MathHelper.Clamp(musicTempo, 0.0f, 24.0f);
            //musicPitch = MathHelper.Clamp(musicPitch, 0.0f, 24.0f);
            //currSongNumPlay = (int)MathHelper.Clamp(currSongNumPlay, 0.0f, 5.0f);
            audioCat.SetVolume(1.0f); //This method sets the volume value of all the songs contained in this category
        }
        //SetPitch Method loops through all the songs and changes the pitch
        public void SetPitch()
        {
            //Console.WriteLine("Set Pitch " + musicPitch);
            if (played)
            {
                for (int i = 0; i < numMusic; i++)
                {
                    cueMusic[i].SetVariable("Pitch", musicPitch);
                }
            }
        }
        //SetTempo Method loops through all the songs and change their Tempo
        public void SetTempo()
        {
            //Console.WriteLine("Set Tempo " + musicTempo);
            if (played)
            {
                for (int i = 0; i < numMusic; i++)
                {
                    cueMusic[i].SetVariable("Tempo", musicTempo);
                }
            }
        }
        //The PlayTrack method increase the volume the track that the player has activated
        public void PlayTrack()
        {
            if (!paused)
            {
                Console.WriteLine("PlayTrack" + currSongNumPlay);
                cueMusic[currSongNumPlay - 1].SetVariable("Volume", MAX_VOLUME);
            }
        }

        #endregion

        #region Properties
        //Music Volume Propterty, return the musicVolume
        public float MusicVolume
        {
            get { return musicVolume; }
            set { musicVolume = value; }
        }
        //Music Pitch Property, Checks to see if the value added to the musicPitch is valid
        //Also returns the current musicPitch value
        public float MusicPitch
        {
            get { return musicPitch; }
            set
            {
                if (value > 24)
                {
                    musicPitch = 24;
                }
                else if (value < 0)
                {
                    musicPitch = 0;
                }
                else
                {
                    musicPitch = value;
                }
            }
        }
        //Music Tempo Property, Check to see if the value added to the musicTempo is valid
        //Can also return the current MusicTempo
        public float MusicTempo
        {
            get { return musicTempo; }
            set
            {
                if (value > 24)
                {
                    musicTempo = 24;
                }
                else if (value < 0)
                {
                    musicTempo = 0;
                }
                else
                {
                    musicTempo = value;
                }


            }
        }
        //SongNumPlay Property,checks to make sure the increment does not exceed the number of music in the libaray
        public int SongNumPlay
        {
            get { return currSongNumPlay; }
            set
            {
                int temp = value;
                if (temp > numMusic)
                {
                }
                else
                {
                    currSongNumPlay = value;
                }
            }
        }
        //NumMusic Property, sets the number of music the Game World Will have
        public int NumMusic
        {
            get { return numMusic; }
            set { numMusic = value; }
        }
        //Paused property, returns a boolean if the audio is paused
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }
        #endregion
    }
    */
    }
}
