using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatDetection
{
  public class SpectrumAnalyzer
  {
    private List<float> energyHistory = new List<float>();
    private int historySize = 0;
    private int historyIndex = 0;

    private const int SPECTRUM_SIZE = 512;
    private const FMOD.DSP_FFT_WINDOW FFT_WINDOW_TYPE = FMOD.DSP_FFT_WINDOW.BLACKMAN;
    private float[] constantRegressionValues = { -0.002571428f, -100.5142857f }; // This value can be changed to get more accurate beat detection

    public void Initialize(FMOD.Channel channel)
    {
      float frequency = 0.0f;
      channel.getFrequency(ref frequency);
      historySize = (int)(frequency / SPECTRUM_SIZE);
      historyIndex = 0;

      energyHistory.Clear();
    }

    public void Analyze(FMOD.Channel channel)
    {
      FMOD.Sound sound = null;
      channel.getCurrentSound(ref sound);
      uint soundLength = 0;
      sound.getLength(ref soundLength, FMOD.TIMEUNIT.MS);

      for (uint pos = 0; pos < soundLength; pos = pos + 250)
      {
        channel.setPosition(pos, FMOD.TIMEUNIT.MS);
        var data = this.AnalyzePosition(channel);
      }

      channel.setPosition(0, FMOD.TIMEUNIT.MS);
    }

    public SpectrumData AnalyzePosition(FMOD.Channel channel)
    {
      var data = new SpectrumData(SPECTRUM_SIZE);
      channel.getSpectrum(data.specL, SPECTRUM_SIZE, 0, FFT_WINDOW_TYPE);
      channel.getSpectrum(data.specR, SPECTRUM_SIZE, 1, FFT_WINDOW_TYPE);

      // 1) Compute Instant Energy
      var instantEnergy = 0.0f;
      for (int i = 0; i < data.spectrumSize; ++i)
        instantEnergy += (data.specL[i] * data.specL[i]) + (data.specR[i] * data.specR[i]);

      // 2) Compute Average Energy from Energy Buffer
      var averageEnergy = 0.0f;
      for (int i = 0; i < energyHistory.Count; ++i)
        averageEnergy += energyHistory[i];
      if (energyHistory.Count > 0)
        averageEnergy = averageEnergy / energyHistory.Count;

      // 3) Compute variance by comparing values in energyHistory and the average energy
      var variance = 0.0f;
      for (int i = 0; i < energyHistory.Count; ++i)
        variance += (energyHistory[i] - averageEnergy) * (energyHistory[i] - averageEnergy);
      if (energyHistory.Count > 0)
        variance = variance / energyHistory.Count;

      // 4) Compute the constant value used to determine whether there's a beat or not.
      float Constant = 0;

      if (variance > 200)
        Constant = 1.0f;
      else if (variance < 25)
        Constant = 1.45f;
      else
        Constant = (constantRegressionValues[0] * variance) + constantRegressionValues[1];

      // 5) Update Energy History Buffer
      if (energyHistory.Count < historySize)
      {
        energyHistory.Add(instantEnergy);
        historyIndex++;
      }
      else
      {
        if (historyIndex < historySize - 1)
          historyIndex++;
        else
          historyIndex = 0;

        energyHistory[historyIndex] = instantEnergy;
      }

      // 6) Compare instant energy with the averageEnergy times the constant value.`
      if (instantEnergy > (Constant * averageEnergy))
        data.isBeat = true;
      else
        data.isBeat = false;

      data.beatData.instantEnergy = instantEnergy;
      data.beatData.averageEnergy = averageEnergy;
      data.beatData.variance = variance;
      data.beatData.beatSensibility = Constant;
      data.beatData.historySize = energyHistory.Count;

      return data;
    }
  }
}
