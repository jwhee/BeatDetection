using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace BeatDetection.Test
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod1()
    {
      using (var soundEngine = new SoundEngine())
      {
        soundEngine.Load(@"D:\Music\test.mp3");

        var analyzer = new SpectrumAnalyzer();
        analyzer.Analyze(soundEngine.Channel);
      }
    }
  }
}
