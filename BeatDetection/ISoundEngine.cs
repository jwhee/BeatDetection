namespace BeatDetection
{
  using System;
  using System.Collections.Generic;

  public interface ISoundEngine : IDisposable
  {
    /// <summary>
    /// Gets the current music position in milliseconds
    /// </summary>
    uint MusicPosition { get; }

    /// <summary>
    /// Gets a value indicating whether or not the music is playing
    /// </summary>
    bool IsMusicPlaying { get; }

    /// <summary>
    /// Gets the current snap shot of list of beat positions
    /// </summary>
    List<uint> BeatPositions { get; }

    /// <summary>
    /// The update function. This should be called every game update frame
    /// </summary>
    void Update();

    /// <summary>
    /// Load music into sound engine
    /// </summary>
    /// <param name="path">path to music file</param>
    /// <returns>self</returns>
    ISoundEngine LoadMusic(string path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="low">low cutoff frequency</param>
    /// <param name="high">high cutoff frequency</param>
    /// <returns>self</returns>
    ISoundEngine SetBeatDetectionFrequency(float low, float high);

    /// <summary>
    /// Queue music to be played given a delay
    /// </summary>
    /// <param name="secondsDelay">delay in seconds before playing</param>
    /// <returns>self</returns>
    ISoundEngine PlayMusic(int secondsDelay = 1);

    /// <summary>
    /// Stop music
    /// </summary>
    /// <returns>self</returns>
    ISoundEngine StopMusic();

    /// <summary>
    /// Register a callback whenever a beat is detected
    /// </summary>
    /// <param name="onBeatCallback">the callback</param>
    /// <returns>self</returns>
    ISoundEngine RegisterOnBeatCallback(Action<uint> onBeatCallback);

    /// <summary>
    /// Unregister a callback
    /// </summary>
    /// <param name="onBeatCallback">the callback</param>
    /// <returns>self</returns>
    ISoundEngine UnregisterOnBeatCallback(Action<uint> onBeatCallback);
  }
}
