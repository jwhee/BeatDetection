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
        soundEngine.LoadMusic(@"D:\Music\test.mp3");

        var analyzer = new SpectrumAnalyzer();
        analyzer.Analyze(soundEngine.AnalyzeChannel);
      }
    }

    [TestMethod]
    public void PlayTest()
    {
      using (var soundEngine = new SoundEngine())
      {
        soundEngine.LoadMusic(@"D:\Music\test.mp3")
          .PlayMusic();

        while (soundEngine.IsMusicPlaying) { }
      }
    }
  }
}
