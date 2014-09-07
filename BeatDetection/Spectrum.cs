namespace BeatDetection
{
  public class Spectrum
  {
    public float[] SpectrumL { get; set; }
    public float[] SpectrumR { get; set; }
    public float AverageL { get; set; }
    public float AverageR { get; set; }
    public int Size { get; set; }
    public bool IsBeat { get; set; }

    public float InstantEnergy { get; set; }
    public float AverageEnergy { get; set; }
    public float Variance { get; set; }
    public float BeatSensibility { get; set; }
    public int HistorySize { get; set; }

    public Spectrum(int spectrumSize)
    {
      this.Size = spectrumSize;
      this.SpectrumL = new float[spectrumSize];
      this.SpectrumR = new float[spectrumSize];
    }
  }
}
