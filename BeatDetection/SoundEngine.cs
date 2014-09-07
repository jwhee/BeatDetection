namespace BeatDetection
{
  using System;
  using System.IO;

  public class SoundEngine : IDisposable
  {
    private const int NUM_MAX_CHANNELS = 8;

    private FMOD.System fmodSystem;
    private FMOD.Channel fmodChannel;
    private FMOD.Sound fmodSound;
    private FMOD.DSP highpassFilter;
    private FMOD.DSP lowpassFilter;

    public SoundEngine()
    {
      // Initialize fmod system
      this.Verify(FMOD.Factory.System_Create(ref fmodSystem));
      this.Verify(fmodSystem.init(NUM_MAX_CHANNELS, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));
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
        this.Verify(fmodSystem.createStream(path, mode, ref fmodSound));

        // Load sound into channel
        this.Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, fmodSound, true, ref fmodChannel));
        this.Verify(fmodChannel.setVolume(1.0f));

        // Initialize highpass filter
        fmodSystem.createDSPByType(FMOD.DSP_TYPE.HIGHPASS, ref highpassFilter);
        fmodSystem.createDSPByType(FMOD.DSP_TYPE.LOWPASS, ref lowpassFilter);

        FMOD.DSPConnection con = null;
        fmodChannel.addDSP(highpassFilter, ref con);
        fmodChannel.addDSP(lowpassFilter, ref con);

        highpassFilter.setBypass(true);
        lowpassFilter.setBypass(true);
      }

      return this;
    }

    public SoundEngine AddHighpass(float cutoff = 5000.0f)
    {
      highpassFilter.setParameter((int)FMOD.DSP_HIGHPASS.CUTOFF, cutoff);
      highpassFilter.setBypass(false);

      return this;
    }

    public void RemoveHighpass()
    {
      highpassFilter.setBypass(true);
    }

    public SoundEngine AddLowpass(float cutoff = 5000.0f)
    {
      lowpassFilter.setParameter((int)FMOD.DSP_LOWPASS.CUTOFF, cutoff);
      lowpassFilter.setBypass(false);

      return this;
    }

    public void RemoveLowpass()
    {
      lowpassFilter.setBypass(true);
    }

    public void Play()
    {
      // Play channel
      this.Verify(fmodChannel.setPaused(false));
    }

    public void Stop()
    {
      if(fmodChannel != null)
      {
        try
        {
          fmodChannel.stop();
        }
        catch (Exception)
        {
          // Do nothing
        }
        fmodChannel = null;
      }

      if(fmodSound != null)
      {
        fmodSound.release();
        fmodSound = null;
      }
    }

    public bool IsPlaying
    {
      get
      {
        bool isPlaying = false;

        if (fmodChannel != null)
        {
          if (fmodChannel.isPlaying(ref isPlaying) != FMOD.RESULT.OK)
          {
            isPlaying = false;
          }
        }

        return isPlaying;
      }
    }

    public FMOD.Channel Channel
    {
      get
      {
        return fmodChannel;
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
