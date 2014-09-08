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

      analyzer = new SpectrumAnalyzer();
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
        this.Verify(fmodSystem.createDSPByType(FMOD.DSP_TYPE.ITLOWPASS, ref lowpassFilter));

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
      this.Verify(lowpassFilter.setParameter((int)FMOD.DSP_ITLOWPASS.CUTOFF, cutoff));
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

    List<uint> beatList = new List<uint>();
    int beatIndex = 0;

    public void Update()
    {
      fmodSystem.update();

      if (this.IsAnalyzeChannelPlaying)
      {
        uint pos = 0;
        this.analyzeChannel.getPosition(ref pos, FMOD.TIMEUNIT.MS);

        var data = analyzer.AnalyzePosition(this.analyzeChannel);
        if (data.IsBeat)
        {
          beatList.Add(pos);
        }
      }
    }

    public bool IsBeat()
    {
      var isBeat = false;

      uint pos = 0;
      this.playChannel.getPosition(ref pos, FMOD.TIMEUNIT.MS);

      if (pos != 0 && beatIndex < beatList.Count)
      {
        uint nextBeat = beatList[beatIndex];
        uint previousBeat = 0;
        if (beatIndex > 0)
        {
          previousBeat = beatList[beatIndex - 1];
        }

        if(nextBeat - previousBeat < 120)
        {
          isBeat = true;
        }

        while (nextBeat < pos)
        {
          isBeat = true;
          beatIndex++;

          if(beatIndex < beatList.Count)
          {
            nextBeat = beatList[beatIndex];
          }
          else
          {
            break;
          }
        }
      }

      return isBeat;
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
