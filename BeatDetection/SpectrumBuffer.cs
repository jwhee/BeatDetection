namespace BeatDetection
{
  public class SpectrumBuffer
  {
    #region Fields

    private static SpectrumBuffer instance;
    public static SpectrumBuffer Instance
    {
      get
      {
        if (instance == null)
        {
          instance = new SpectrumBuffer();
        }
        return instance;
      }
    }

    private SpectrumData data1;
    private SpectrumData data2;

    private volatile int updateBuff;
    private bool amReading;

    private int spectrumSize;
    public int SectrumSize
    {
      get { return spectrumSize; }
      set
      {
        if (value == 256 || value == 512 || value == 1024 || value == 2048)
          spectrumSize = value;
        else
          spectrumSize = 512;
      }
    }

    #endregion

    #region Initialize

    private SpectrumBuffer()
    {
      spectrumSize = 512;
      data1 = new SpectrumData(spectrumSize);
      data2 = new SpectrumData(spectrumSize);
      updateBuff = 1;
      amReading = false;
    }

    /// <summary>
    /// Resets the fields according to the options set in OptionsMenuScreen. This is called by the Sound Manager class.
    /// </summary>
    public void Reset()
    {
      data1.Reset(spectrumSize);
      data2.Reset(spectrumSize);
    }

    #endregion

    #region DataFetching and Updating

    public void UpdateData(SpectrumData data)
    {
      if (amReading)
        return;

      if (updateBuff == 1)
      {
        data2.Set(data);
        updateBuff = 2;
      }
      else
      {
        data1.Set(data);
        updateBuff = 1;
      }
    }

    public SpectrumData GetLatestData()
    {
      amReading = true;

      amReading = false;
      if (updateBuff == 1)
        return data1;
      else
        return data2;
    }

    #endregion
  }
}
