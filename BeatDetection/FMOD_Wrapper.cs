namespace BeatDetection
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading;

  public class FMOD_Wrapper : IDisposable
  {
    #region Multithreaded Spectrum Analysing

    private volatile bool runSpectrumAnalyze = false;

    private void SpectrumAnalyze()
    {
      while (runSpectrumAnalyze == true)
      {
        lock (spectrumLock)
        {
          if (musicChannel != null)
          {
            if (IsMusicPlaying())
            {
              if (useFFT == true)
              {
                musicChannel.getSpectrum(data.specL, spectrumSize, 0,
                                         (FMOD.DSP_FFT_WINDOW)FFT_WINDOW_TYPE);
                musicChannel.getSpectrum(data.specR, spectrumSize, 1,
                                         (FMOD.DSP_FFT_WINDOW)FFT_WINDOW_TYPE);
              }
              else
              {
                musicChannel.getWaveData(data.specL, spectrumSize, 0);
                musicChannel.getWaveData(data.specR, spectrumSize, 1);
              }

              switch (runBeatDetection)
              {
                case BeatDetectionType.None:
                  data.isBeat = false;
                  break;
                case BeatDetectionType.Simple:
                  SimpleSoundEnergy_BeatDetection(ref data);
                  break;
                case BeatDetectionType.FrequencyBands:
                  FrequencySelectedSoundEnergy_BeatDetection(ref data);
                  break;
              }
            }
            else
              data.Clear();
          }
          else
            data.Clear();

          // TODO: Calculate averages to be used in object coloring and such. Amplitude averages, Volume averages, etc.

          spectrumBuffer.UpdateData(data);
          SpectrumAnalysisPerSecond++;
          Update();
        }
      }
    }

    #endregion

    #region Beat Detection Algorithms

    #region Beat Detection Fields

    float songFrequency = 44100; // Default value. This get set in the Play() function.

    // SimpleSoundEnergy_BeatDetection Fields
    float instantEnergy = 0;
    float averageEnergy = 0;
    List<float> energyHistory;
    int historySize = 0;
    int historyIndex = 0;
    float variance = 0;

    float[] constantRegressionValues = { -0.002571428f, -100.5142857f }; // This value can be changed to get more accurate beat detection

    // FrequencySelectedSoundEnergy_BeatDetection Fields

    #endregion Beat Detection Fields

    private void SimpleSoundEnergy_BeatDetection(ref SpectrumData data)
    {
      // 1) Compute Instant Energy
      instantEnergy = 0;
      for (int i = 0; i < data.spectrumSize; ++i)
        instantEnergy += (data.specL[i] * data.specL[i]) + (data.specR[i] * data.specR[i]);

      // 2) Compute Average Energy from Energy Buffer
      averageEnergy = 0;
      for (int i = 0; i < energyHistory.Count; ++i)
        averageEnergy += energyHistory[i];
      if (energyHistory.Count > 0)
        averageEnergy = averageEnergy / energyHistory.Count;

      // 3) Compute variance by comparing values in energyHistory and the average energy
      variance = 0;
      for (int i = 0; i < energyHistory.Count; ++i)
        variance += (energyHistory[i] - averageEnergy) * (energyHistory[i] - averageEnergy);
      if (energyHistory.Count > 0)
        variance = variance / energyHistory.Count;

      // 4) Compute the constant value used to determine whether there's a beat or not.
      float Constant = 0;

      if (variance > 200)
        Constant = 1.0f;
      else if (variance < 25)
        Constant = 1.45f;
      else
        Constant = (constantRegressionValues[0] * variance) + constantRegressionValues[1];

      // 5) Update Energy History Buffer
      if (energyHistory.Count < historySize)
      {
        energyHistory.Add(instantEnergy);
        historyIndex++;
      }
      else
      {
        if (historyIndex < historySize - 1)
          historyIndex++;
        else
          historyIndex = 0;

        energyHistory[historyIndex] = instantEnergy;
      }

      // 6) Compare instant energy with the averageEnergy times the constant value.`
      if (instantEnergy > (Constant * averageEnergy))
        data.isBeat = true;
      else
        data.isBeat = false;

      data.beatData.instantEnergy = instantEnergy;
      data.beatData.averageEnergy = averageEnergy;
      data.beatData.variance = variance;
      data.beatData.beatSensibility = Constant;
      data.beatData.historySize = energyHistory.Count;
    }

    /// <summary>
    /// Note: This function will only run correctly if FFT analysis is performed on the data.
    /// </summary>
    /// <param name="data"></param>
    private void FrequencySelectedSoundEnergy_BeatDetection(ref SpectrumData data)
    {
      // TODO: Implement Beat Detection Algorithm
    }

    private void BeatDetectionReset()
    {
      instantEnergy = 0;
      averageEnergy = 0;
      energyHistory.Clear();
      historyIndex = 0;
      variance = 0;

      historySize = (int)(songFrequency / spectrumSize);
    }

    #endregion

    #region Initialize, Shutdown, Update and Reset methods

    public FMOD_Wrapper()
    {
      // load FMOD
      Verify(FMOD.Factory.System_Create(ref fmodSystem));
      // check it's right version
      uint version = 0;
      Verify(fmodSystem.getVersion(ref version));
      if (version < FMOD.VERSION.number)
      {
        throw new ApplicationException("Invalid FMOD version");
      }
      Verify(fmodSystem.init(numSoundChannels, FMOD.INITFLAGS.NORMAL, (IntPtr)null));
      sounds = new Dictionary<string, FMOD.Sound>();

      spectrumBuffer = SpectrumBuffer.Instance;
      energyHistory = new List<float>();

      spectrumSize = spectrumBuffer.SectrumSize;
      data.Reset(spectrumSize);

      FFT_WINDOW_TYPE = (int)FMOD.DSP_FFT_WINDOW.BLACKMAN;

      //spectrumAnalysisThread = new Thread(new ThreadStart(SpectrumAnalyze));
      //runSpectrumAnalyze = true;
      //spectrumAnalysisThread.Start();
    }

    public void Dispose()
    {
      lock (spectrumLock)
      {
        runSpectrumAnalyze = false;

        // Close the thread. If you don't do this here the prgram won't close.
        //spectrumAnalysisThread.Abort();
        foreach (FMOD.Sound sound in sounds.Values)
        {
          Verify(sound.release());
        }
        sounds.Clear();
        StopMusic();
        if (null != fmodSystem)
        {
          fmodSystem.release();
          fmodSystem = null;
        }
      }
    }

    private void Update()
    {
      try
      {
        fmodSystem.update();
      }
      catch (System.Exception)
      {
      }
    }

    /// <summary>
    /// Resets the fields according to the options set in OptionsMenuScreen. Call once you exit out of OptionsMenuScreen.
    /// </summary>
    public void Reset(int _spectrumSize, int _FFT_WINDOW_TYPE)
    {
      lock (spectrumLock)
      {
        spectrumBuffer.SectrumSize = _spectrumSize;
        spectrumBuffer.Reset();

        spectrumSize = _spectrumSize;
        FFT_WINDOW_TYPE = _FFT_WINDOW_TYPE;

        data.Reset(spectrumSize);

        // Beat Detection Resets
        BeatDetectionReset();
      }
    }

    #endregion

    #region Play, Stop, IsPlaying , Volume control Methods

    /// <summary>
    /// Load a sound off the disk, to play later.
    /// (to reduce load time)
    /// </summary>
    /// <param name="filename">Name of the file holding the sound</param>
    public void LoadSound(string filename)
    {
      FMOD.Sound sound = null;
      if (VerifyFileExists(filename))
      {
        Verify(fmodSystem.createSound(filename, FMOD.MODE.DEFAULT, ref sound));
        sounds.Add(filename, sound);
      }
    }

    /// <summary>
    /// Play a sound
    /// </summary>
    /// <param name="filename">Name of the previously loaded file holding the music</param>
    public void PlaySound(string filename)
    {
      FMOD.Sound sound;
      if (sounds.TryGetValue(filename, out sound))
      {
        lock (spectrumLock)
        {
          FMOD.Channel dummyChannel = new FMOD.Channel();
          Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, sound, true, ref dummyChannel));
          Verify(dummyChannel.setVolume(soundVolume));
          Verify(dummyChannel.setPaused(false));
        }
      }
      else if (!ignoreMissingAudioFiles)
      {
        throw new ApplicationException("Sound " + filename + " not loaded");
      }
    }

    /// <summary>
    /// Set a piece of music to be played
    /// </summary>
    /// <param name="filename">Name of the file holding the music</param>
    public void PlayMusic(string filename)
    {
      // if we're already playing this, nothing to do
      if (musicFilename != filename)
      {
        // if we're already playing music, stop it
        StopMusic();
        // play the new music (if the file exists)
        String path = filename;
        if (VerifyFileExists(path))
        {
          lock (spectrumLock)
          {
            FMOD.MODE mode = FMOD.MODE.SOFTWARE | FMOD.MODE.LOOP_OFF | FMOD.MODE.ACCURATETIME;
            Verify(fmodSystem.createStream(path, mode, ref music));
            Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, music, true, ref musicChannel));
            Verify(musicChannel.setVolume(musicVolume));
            Verify(musicChannel.setPaused(false));
            musicPaused = false;
            musicFilename = filename;

            musicChannel.getFrequency(ref songFrequency);
            BeatDetectionReset();
          }
        }
      }
    }


    /// <summary>
    /// stop playing music
    /// </summary>
    /// <remarks>This function MUST NOT THROW, because it's called by Dispose</remarks>
    public void StopMusic()
    {
      musicFilename = String.Empty;
      if (null != musicChannel)
      {
        lock (spectrumLock)
        {
          try
          {
            musicChannel.stop();
          }
          catch (System.Exception)
          { }
          musicChannel = null;
        }
        if (null != music)
        {
          music.release();
          music = null;
        }
      }
    }

    public void PauseMusic()
    {
      if (musicChannel != null)
      {
        lock (spectrumLock)
        {
          if (musicPaused != true)
            musicChannel.setPaused(true);
          musicPaused = true;
        }
      }
    }

    public void ResumeMusic()
    {
      if (musicChannel != null)
      {
        lock (spectrumLock)
        {
          if (musicPaused == true)
            musicChannel.setPaused(false);
          musicPaused = false;
        }
      }
    }

    /// <summary>
    /// Is background music currently playing?
    /// </summary>
    /// <returns>true if it's playing</returns>
    public bool IsMusicPlaying()
    {
      bool isPlaying = false;
      if (null != musicChannel)
      {
        if (FMOD.RESULT.OK != musicChannel.isPlaying(ref isPlaying))
        {
          isPlaying = false;
        }
      }
      return isPlaying;
    }

    public uint GetMusicLength()
    {
      uint pos = 0;
      music.getLength(ref pos, FMOD.TIMEUNIT.MS);
      return pos;
    }

    public uint GetMusicPos()
    {
      uint pos = 0;
      musicChannel.getPosition(ref pos, FMOD.TIMEUNIT.MS);
      return pos;
    }

    public void SetMusicPos(uint pos)
    {
      if (musicChannel != null)
        musicChannel.setPosition(pos, FMOD.TIMEUNIT.MS);
    }

    /// <summary>
    /// The volume sound will be played at (0 = off, 1 = max)
    /// </summary>
    public float SoundVolume
    {
      get { return soundVolume; }
      set
      {
        if ((value >= 0.0f) && (value <= 1.0f))
          soundVolume = value;
      }
    }

    /// <summary>
    /// The volume sound will be played at (0 = off, 1 = max)
    /// </summary>
    public float MusicVolume
    {
      get { return musicVolume; }
      set
      {
        if ((value >= 0.0f) && (value <= 1.0f))
          musicVolume = value;
        if (null != musicChannel)
        {
          musicChannel.setVolume(musicVolume);
        }
      }
    }

    #endregion

    #region Verification Methods

    /// <summary>
    /// Check the result from an FMOD function call, and throw if it's in error
    /// </summary>
    /// <param name="result">return value from an FMOD function to test</param>
    private static void Verify(FMOD.RESULT result)
    {
      if (FMOD.RESULT.OK != result)
      {
        throw new ApplicationException("FMOD error:" + FMOD.Error.String(result));
      }
    }

    /// <summary>
    /// Throw error if file doesn't exist (and we've got checking turned on
    /// </summary>
    /// <param name="pathname">full pathname of file to check</param>
    private bool VerifyFileExists(string pathname)
    {
      if (File.Exists(pathname))
      {
        return true;
      }

      if (!ignoreMissingAudioFiles)
      {
        throw new System.IO.FileNotFoundException("Audio file not found:" + pathname);
      }

      return false;
    }

    #endregion

    #region Fields

    /// <summary>
    /// The thread handle for the spectrum analysis thread. This is started in Initialize.
    /// </summary>
    private Thread spectrumAnalysisThread = null;
    private readonly object spectrumLock = new object();

    private SpectrumData data;

    private volatile int spectrumSize;
    private volatile int FFT_WINDOW_TYPE;
    public volatile bool useFFT = true;

    public enum BeatDetectionType { None, Simple, FrequencyBands };
    public volatile BeatDetectionType runBeatDetection = BeatDetectionType.Simple;

    /// <summary>
    /// Do we ignore missing sound and music files?
    /// </summary>
    public bool IgnoreMissingAudioFiles
    {
      get { return ignoreMissingAudioFiles; }
      set { ignoreMissingAudioFiles = value; }
    }

    /// <summary>
    /// The wrapped FMOD engine
    /// </summary>
    private FMOD.System fmodSystem;

    /// <summary>
    /// The buffer where we will store the audioAnalysis data.
    /// </summary>
    private SpectrumBuffer spectrumBuffer;

    /// <summary>
    /// Internal cache of sounds (gunshot, etc.)
    /// </summary>
    private Dictionary<string, FMOD.Sound> sounds;

    /// <summary>
    /// Name of file holding the current background music
    /// </summary>
    private String musicFilename = String.Empty;

    /// <summary>
    /// The actual music
    /// </summary>
    private FMOD.Sound music;

    /// <summary>
    /// Channel used to play background music
    /// </summary>
    private FMOD.Channel musicChannel;

    public bool musicPaused;

    /// <summary>
    /// Number of sound channels we want FMOD to have
    /// </summary>
    const int numSoundChannels = 8;

    /// <summary>
    /// Do we ignore missing sound and music files?
    /// </summary>
    private bool ignoreMissingAudioFiles = true;

    /// <summary>
    /// The volume sound will be played at (0 = off, 1 = max)
    /// </summary>
    public float soundVolume = 1.0f;

    /// <summary>
    /// The volume sound will be played at (0 = off, 1 = max)
    /// </summary>
    public float musicVolume = 1.0f;


    private volatile uint spectrumAnalysisPerSecond = 0;
    public uint SpectrumAnalysisPerSecond
    {
      get { return spectrumAnalysisPerSecond; }
      set { spectrumAnalysisPerSecond = value; }
    }

    #endregion Fields
  }
}