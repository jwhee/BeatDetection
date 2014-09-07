namespace BeatDetection
{
  using System;

  public class SpectrumData
  {
    // specL and specR are the 2 sound channels that FMOD returns, one for left speaker and one for right speaker.
    public float[] specL, specR;
    public float averageL, averageR;
    public int spectrumSize;
    public bool isBeat;
    public BeatData beatData;

    public SpectrumData(int _spectrumSize)
    {
      spectrumSize = _spectrumSize;
      specL = new float[spectrumSize];
      specR = new float[spectrumSize];
      averageL = 0.0f;
      averageR = 0.0f;
      isBeat = false;

      beatData = new BeatData();
    }

    public void Set(SpectrumData data)
    {
      // Block copy is (probably) the fastest way to copy data in C#
      Buffer.BlockCopy(data.specL, 0, specL, 0, spectrumSize * sizeof(float));
      Buffer.BlockCopy(data.specR, 0, specR, 0, spectrumSize * sizeof(float));

      isBeat = data.isBeat;
      beatData = data.beatData;

      averageL = data.averageL;
      averageR = data.averageR;
    }

    public void Reset(int _spectrumSize)
    {
      // By assigning these values to null we ask the memory manager to delete their previous data.
      specL = null;
      specR = null;

      spectrumSize = _spectrumSize;

      specL = new float[spectrumSize];
      specR = new float[spectrumSize];
      isBeat = false;
    }

    public void Clear()
    {
      Array.Clear(specL, 0, spectrumSize);
      Array.Clear(specR, 0, spectrumSize);
      isBeat = false;
      averageL = 0.0f;
      averageR = 0.0f;
    }
  }
}
