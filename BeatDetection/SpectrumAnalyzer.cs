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

    private const int SPECTRUM_SIZE = 2048;
    private const FMOD.DSP_FFT_WINDOW FFT_WINDOW_TYPE = FMOD.DSP_FFT_WINDOW.BLACKMAN;
    private float[] constantRegressionValues = { -0.002571428f, 1.5142857f };

    public void Initialize(FMOD.Channel channel)
    {
      float frequency = 0.0f;
      var result = channel.getFrequency(ref frequency);

      if(result != FMOD.RESULT.OK)
      {
        throw new ApplicationException("FMOD error:" + FMOD.Error.String(result));
      }

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

    public Spectrum AnalyzePosition(FMOD.Channel channel)
    {
      var data = new Spectrum(SPECTRUM_SIZE);
      channel.getSpectrum(data.SpectrumL, SPECTRUM_SIZE, 0, FFT_WINDOW_TYPE);
      channel.getSpectrum(data.SpectrumR, SPECTRUM_SIZE, 1, FFT_WINDOW_TYPE);

      // 1) Compute Instant Energy
      data.InstantEnergy = 0.0f;
      for (int i = 0; i < data.Size; ++i)
      {
        data.InstantEnergy += (data.SpectrumL[i] * data.SpectrumL[i]) + (data.SpectrumR[i] * data.SpectrumR[i]);
      }
        
      // 2) Compute Average Energy from Energy Buffer
      data.AverageEnergy = 0.0f;
      for (int i = 0; i < energyHistory.Count; ++i)
      {
        data.AverageEnergy += energyHistory[i];
      }

      if (energyHistory.Count > 0)
      {
        data.AverageEnergy = data.AverageEnergy / energyHistory.Count;
      }

      // 3) Compute variance by comparing values in energyHistory and the average energy
      data.Variance = 0.0f;
      for (int i = 0; i < energyHistory.Count; ++i)
      {
        data.Variance += (energyHistory[i] - data.AverageEnergy) * (energyHistory[i] - data.AverageEnergy);
      }
        
      if (energyHistory.Count > 0)
      {
        data.Variance = data.Variance / energyHistory.Count;
      }

      // 4) Compute the constant value used to determine whether there's a beat or not.
      data.BeatSensibility = 0;

      if (data.Variance > 200)
      {
        data.BeatSensibility = 1.0f;
      }
      else if (data.Variance < 25)
      {
        data.BeatSensibility = 1.45f;
      }
      else
      {
        data.BeatSensibility = (constantRegressionValues[0] * data.Variance) + constantRegressionValues[1];
      }

      // 5) Update Energy History Buffer
      if (energyHistory.Count < historySize)
      {
        energyHistory.Add(data.InstantEnergy);
        historyIndex++;
      }
      else
      {
        if (historyIndex < historySize - 1)
        {
          historyIndex++;
        }
        else
        {
          historyIndex = 0;
        }

        energyHistory[historyIndex] = data.InstantEnergy;
      }

      // 6) Compare instant energy with the averageEnergy times the constant value.`
      if (data.InstantEnergy > (data.BeatSensibility * data.AverageEnergy))
      {
        data.IsBeat = true;
      }
      else
      {
        data.IsBeat = false;
      }

      data.HistorySize = energyHistory.Count;

      return data;
    }
  }
}
