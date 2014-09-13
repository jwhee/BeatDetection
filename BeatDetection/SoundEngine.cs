namespace BeatDetection
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading;
  using System.Threading.Tasks;

  public class SoundEngine : ISoundEngine
  {
    private const int NUM_MAX_CHANNELS = 8;

    private FMOD.System fmodSystem;
    private FMOD.Channel analyzeChannel;
    private FMOD.Sound analyzeSound;
    private FMOD.DSP highpassFilter;
    private FMOD.DSP lowpassFilter;

    private FMOD.Channel playChannel;
    private FMOD.Sound playSound;

    private SpectrumAnalyzer analyzer;

    public uint MusicPosition
    {
      get
      {
        if (this.playChannel == null)
          return 0;

        uint pos = 0;
        this.playChannel.getPosition(ref pos, FMOD.TIMEUNIT.MS);
        return pos;
      }
    }

    public bool IsMusicPlaying
    {
      get
      {
        bool isPlaying = false;

        if (this.playChannel != null)
        {
          if (this.playChannel.isPlaying(ref isPlaying) != FMOD.RESULT.OK)
          {
            isPlaying = false;
          }
        }

        return isPlaying;
      }
    }

    public List<uint> BeatPositions 
    { 
      get 
      { 
        return new List<uint>(this.beatList);
      }
    }

    private bool IsAnalyzeChannelPlaying
    {
      get
      {
        bool isPlaying = false;

        if (this.analyzeChannel != null)
        {
          if (this.analyzeChannel.isPlaying(ref isPlaying) != FMOD.RESULT.OK)
          {
            isPlaying = false;
          }
        }

        return isPlaying;
      }
    }

    public SoundEngine()
    {
      // Initialize fmod system
      this.Verify(FMOD.Factory.System_Create(ref fmodSystem));
      this.Verify(fmodSystem.init(NUM_MAX_CHANNELS, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));

      this.analyzer = new SpectrumAnalyzer();

      this.subscribers = new List<Action<uint>>();
    }

    public void Dispose()
    {
      this.tokenSource.Cancel();

      this.StopMusic();

      if(this.fmodSystem != null)
      {
        this.fmodSystem.release();
        this.fmodSystem = null;
      }
    }

    public ISoundEngine LoadMusic(string path)
    {
      if (File.Exists(path))
      {
        beatList.Clear();

        // Load music into sound
        FMOD.MODE mode = FMOD.MODE.SOFTWARE | FMOD.MODE.LOOP_OFF | FMOD.MODE.ACCURATETIME;
        this.Verify(fmodSystem.createStream(path, mode, ref analyzeSound));
        this.Verify(fmodSystem.createStream(path, mode, ref playSound));

        // Load sound into analyze channel
        this.Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, analyzeSound, true, ref analyzeChannel));
        this.Verify(analyzeChannel.setMute(true));

        // Load sound into play channel
        this.Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, playSound, true, ref playChannel));
        //this.Verify(playChannel.setMute(true));

        this.analyzer.Initialize(this.analyzeChannel);
      }

      return this;
    }

    private int millisecondsDelay = 0;
    public ISoundEngine PlayMusic(uint secondsDelay = 1)
    {
      this.millisecondsDelay = (int)secondsDelay * 1000;
      return this;
    }

    public ISoundEngine StopMusic()
    {
      if (this.analyzeChannel != null)
      {
        try
        {
          this.analyzeChannel.stop();
        }
        catch (Exception)
        {
          // Do nothing
        }

        this.analyzeChannel = null;
      }

      if (this.playChannel != null)
      {
        try
        {
          this.playChannel.stop();
        }
        catch (Exception)
        {
          // Do nothing
        }

        this.playChannel = null;
      }

      if (this.analyzeSound != null)
      {
        this.analyzeSound.release();
        this.analyzeSound = null;
      }

      if (this.playSound != null)
      {
        this.playSound.release();
        this.playSound = null;
      }

      return this;
    }

    public ISoundEngine SetBeatDetectionFrequency(float low, float high)
    {
      FMOD.DSPConnection dummy = null;

      this.Verify(fmodSystem.createDSPByType(FMOD.DSP_TYPE.LOWPASS, ref lowpassFilter));
      this.Verify(analyzeChannel.addDSP(lowpassFilter, ref dummy));
      this.Verify(lowpassFilter.setParameter((int)FMOD.DSP_LOWPASS.CUTOFF, high));

      this.Verify(fmodSystem.createDSPByType(FMOD.DSP_TYPE.HIGHPASS, ref highpassFilter));
      this.Verify(analyzeChannel.addDSP(highpassFilter, ref dummy));
      this.Verify(highpassFilter.setParameter((int)FMOD.DSP_HIGHPASS.CUTOFF, low));

      return this;
    }

    private CancellationTokenSource tokenSource;
    public ISoundEngine StartBeatDetection()
    {
      // Cancel existing beat detection task
      if (this.tokenSource != null)
      {
        this.tokenSource.Cancel();
      }

      // Start a new beat detection task
      this.tokenSource = new CancellationTokenSource();
      var token = this.tokenSource.Token;
      Task.Run(() => this.runBeatDetection(token), token);

      return this;
    }

    private List<uint> beatList = new List<uint>();
    private uint lastBeatPos = 0;
    private void runBeatDetection(CancellationToken ct)
    {
      this.analyzeChannel.setPaused(false);
      while(this.IsAnalyzeChannelPlaying)
      {
        if (ct.IsCancellationRequested)
        {
          break;
        }

        uint pos = 0;
        this.analyzeChannel.getPosition(ref pos, FMOD.TIMEUNIT.MS);

        var data = this.analyzer.Analyze(this.analyzeChannel);
        if (data.IsBeat && pos - this.lastBeatPos > 300)
        {
          this.beatList.Add(pos);
          this.lastBeatPos = pos;
        }
      }
    }

    private int nextBeatIndex = 0;
    public void Update(int elaspedMilliseconds)
    {
      this.fmodSystem.update();

      millisecondsDelay -= elaspedMilliseconds;
      if (millisecondsDelay <= 0 && this.playChannel != null)
      {
        this.playChannel.setPaused(false);
      }

      if (this.IsMusicPlaying)
      {
        var isBeat = false;
        var pos = this.MusicPosition;

        // Check if current music position is a beat
        if (pos != 0 && this.nextBeatIndex < this.beatList.Count)
        {
          uint nextBeat = this.beatList[this.nextBeatIndex];

          while (nextBeat < pos && pos - nextBeat < 100)
          {
            isBeat = true;
            this.nextBeatIndex++;

            if (this.nextBeatIndex < this.beatList.Count)
            {
              nextBeat = this.beatList[this.nextBeatIndex];
            }
            else
            {
              break;
            }
          }
        }

        // If a beat is found, notify all subsribers
        if (isBeat && this.subscribers != null)
        {
          lock (this.subscribers)
          {
            foreach (var notify in subscribers)
            {
              notify(pos);
            }
          }
        }
      }
    }

    private List<Action<uint>> subscribers;
    public ISoundEngine RegisterOnBeatCallback(Action<uint> onBeatCallback)
    {
      lock (this.subscribers)
      {
        if (!this.subscribers.Contains(onBeatCallback))
        {
          this.subscribers.Add(onBeatCallback);
        }
      }

      return this;
    }

    public ISoundEngine UnregisterOnBeatCallback(Action<uint> onBeatCallback)
    {
      lock (this.subscribers)
      {
        this.subscribers.RemoveAll(x => x == onBeatCallback);
      }

      return this;
    }

    private void Verify(FMOD.RESULT result)
    {
      if (result != FMOD.RESULT.OK)
      {
        throw new ApplicationException("FMOD error:" + FMOD.Error.String(result));
      }
    }
  }
}
