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
      using (var box = new FMOD_Wrapper())
      {
        box.PlayMusic(@"D:\Music\test.mp3");

        Thread.Sleep(5000);
      }
    }
  }
}
