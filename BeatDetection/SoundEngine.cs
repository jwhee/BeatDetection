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

    public void Play(string path)
    {
      if (File.Exists(path))
      {
        // Load music into sound
        FMOD.MODE mode = FMOD.MODE.SOFTWARE | FMOD.MODE.LOOP_OFF | FMOD.MODE.ACCURATETIME;
        this.Verify(fmodSystem.createStream(path, mode, ref fmodSound));

        // Load sound into channel
        this.Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, fmodSound, true, ref fmodChannel));
        this.Verify(fmodChannel.setVolume(1.0f));

        // Play channel
        this.Verify(fmodChannel.setPaused(false));
      }
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

    private void Verify(FMOD.RESULT result)
    {
      if (result != FMOD.RESULT.OK)
      {
        throw new ApplicationException("FMOD error:" + FMOD.Error.String(result));
      }
    }
  }
}
