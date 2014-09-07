using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace BeatDetection.Test
{
  [TestClass]
  public class UnitTest1
  {
    //[TestMethod]
    public void AnalyzeTest()
    {
      using (var soundEngine = new SoundEngine())
      {
        soundEngine.Load(@"D:\Music\test.mp3");

        var analyzer = new SpectrumAnalyzer();
        analyzer.Analyze(soundEngine.Channel);
      }
    }

    [TestMethod]
    public void PlayTest()
    {
      using (var soundEngine = new SoundEngine())
      {
        soundEngine.Load(@"D:\Music\test.mp3")
          .AddLowpass(200.0f)
          .AddHighpass(100.0f)
          .Play();

        while (soundEngine.IsPlaying) { }
      }
    }
  }
}
