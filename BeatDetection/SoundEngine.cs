namespace BeatDetection
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;

  public class SoundEngine : IDisposable
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

    public uint Position
    {
      get
      {
        uint pos = 0;
        playChannel.getPosition(ref pos, FMOD.TIMEUNIT.MS);
        return pos;
      }
    }

    public bool IsPlaying
    {
      get
      {
        bool isPlaying = false;

        if (playChannel != null)
        {
          if (playChannel.isPlaying(ref isPlaying) != FMOD.RESULT.OK)
          {
            isPlaying = false;
          }
        }

        return isPlaying;
      }
    }

    public bool IsAnalyzeChannelPlaying
    {
      get
      {
        bool isPlaying = false;

        if (analyzeChannel != null)
        {
          if (analyzeChannel.isPlaying(ref isPlaying) != FMOD.RESULT.OK)
          {
            isPlaying = false;
          }
        }

        return isPlaying;
      }
    }

    public FMOD.Channel AnalyzeChannel
    {
      get
      {
        return analyzeChannel;
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
      this.Stop();

      if(fmodSystem != null)
      {
        fmodSystem.release();
        fmodSystem = null;
      }
    }

    public SoundEngine Load(string path)
    {
      if (File.Exists(path))
      {
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

        // Initialize highpass filter
        this.Verify(fmodSystem.createDSPByType(FMOD.DSP_TYPE.HIGHPASS, ref highpassFilter));
        this.Verify(fmodSystem.createDSPByType(FMOD.DSP_TYPE.LOWPASS, ref lowpassFilter));

        FMOD.DSPConnection dummy = null;
        this.Verify(analyzeChannel.addDSP(highpassFilter, ref dummy));
        this.Verify(analyzeChannel.addDSP(lowpassFilter, ref dummy));

        this.Verify(highpassFilter.setBypass(true));
        this.Verify(lowpassFilter.setBypass(true));

        analyzer.Initialize(this.analyzeChannel);
      }

      return this;
    }

    public SoundEngine SetAnalyzeFrequency(float low, float high)
    {
      this.AddLowpass(high);
      this.AddHighpass(low);
      return this;
    }

    public SoundEngine AddHighpass(float cutoff = 5000.0f)
    {
      this.Verify(highpassFilter.setParameter((int)FMOD.DSP_HIGHPASS.CUTOFF, cutoff));
      this.Verify(highpassFilter.setBypass(false));

      return this;
    }

    public void RemoveHighpass()
    {
      this.Verify(highpassFilter.setBypass(true));
    }

    public SoundEngine AddLowpass(float cutoff = 5000.0f)
    {
      this.Verify(lowpassFilter.setParameter((int)FMOD.DSP_LOWPASS.CUTOFF, cutoff));
      this.Verify(lowpassFilter.setBypass(false));

      return this;
    }

    public void RemoveLowpass()
    {
      this.Verify(lowpassFilter.setBypass(true));
    }

    public async Task Play(int secondsDelay = 1)
    {
      // Play channel
      this.Verify(analyzeChannel.setPaused(false));

      await Task.Delay(secondsDelay * 1000);

      this.Verify(playChannel.setPaused(false));
    }

    public List<uint> BeatPositions { get { return beatList; } }
    private List<uint> beatList = new List<uint>();
    uint lastBeatPos = 0;
    int nextBeatIndex = 0;
    public void Update()
    {
      fmodSystem.update();

      if (this.IsAnalyzeChannelPlaying)
      {
        uint pos = 0;
        this.analyzeChannel.getPosition(ref pos, FMOD.TIMEUNIT.MS);

        var data = analyzer.AnalyzePosition(this.analyzeChannel);
        if (data.IsBeat && pos - lastBeatPos > 300)
        {
          beatList.Add(pos);
          lastBeatPos = pos;
        }
      }

      if (this.IsPlaying)
      {
        var isBeat = false;
        var pos = this.Position;

        if (pos != 0 && nextBeatIndex < beatList.Count)
        {
          uint nextBeat = beatList[nextBeatIndex];

          while (nextBeat < pos)
          {
            isBeat = true;
            nextBeatIndex++;

            if (nextBeatIndex < beatList.Count)
            {
              nextBeat = beatList[nextBeatIndex];
            }
            else
            {
              break;
            }
          }
        }

        if (isBeat && subscribers != null)
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
    public SoundEngine Subscribe(Action<uint> action)
    {
      lock (this.subscribers)
      {
        if (!this.subscribers.Contains(action))
        {
          this.subscribers.Add(action);
        }
      }

      return this;
    }

    public SoundEngine Unsubscribe(Action<uint> action)
    {
      lock (this.subscribers)
      {
        this.subscribers.RemoveAll(x => x == action);
      }

      return this;
    }

    public void Stop()
    {
      if(analyzeChannel != null)
      {
        try
        {
          analyzeChannel.stop();
        }
        catch (Exception)
        {
          // Do nothing
        }
        analyzeChannel = null;
      }

      if (playChannel != null)
      {
        try
        {
          playChannel.stop();
        }
        catch (Exception)
        {
          // Do nothing
        }
        playChannel = null;
      }

      if(analyzeSound != null)
      {
        analyzeSound.release();
        analyzeSound = null;
      }

      if (playSound != null)
      {
        playSound.release();
        playSound = null;
      }
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
