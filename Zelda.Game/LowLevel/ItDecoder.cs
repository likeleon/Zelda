using LibModPlugSharp;
using System;

namespace Zelda.Game.LowLevel
{
    class ItDecoder
    {
        IntPtr _modplugFile;

        public ItDecoder()
        {
            var settings = LibModPlugNative.GetModPlugSettings();
            settings.channels = 2;
            settings.bits = 16;
            settings.loopCount = -1;
            LibModPlugNative.SetModPlugSettings(settings);
        }

        public void Load(byte[] soundBuffer)
        {
            Debug.CheckAssertion(_modplugFile == IntPtr.Zero, "IT data is already loaded");

            _modplugFile = LibModPlugNative.ModPlug_Load(soundBuffer, soundBuffer.Length);
        }

        public void Unload()
        {
            Debug.CheckAssertion(_modplugFile != IntPtr.Zero, "IT data is not loaded");

            LibModPlugNative.UnloadMod(_modplugFile);
            _modplugFile = IntPtr.Zero;
        }

        public int Decode(byte[] decodedData, int numSamples)
        {
            return LibModPlugNative.ModPlug_Read(_modplugFile, decodedData, numSamples);
        }

        public int GetNumChannels()
        {
            return LibModPlugNative.NumChannels(_modplugFile);
        }

        public int GetChannelVolume(int channel)
        {
            var numPatterns = LibModPlugNative.NumPatterns(_modplugFile);

            Debug.CheckAssertion(channel >= 0 && channel < GetNumChannels(), "Invalid channel number");

            if (numPatterns == 0)
                return 0;

            unsafe
            {
                uint numRows = 0;
                LibModPlugNote* notes = (LibModPlugNote*)LibModPlugNative.ModPlug_GetPattern(_modplugFile, 0, out numRows);

                if (numRows == 0)
                    return 0;

                return notes[0].Volume;
            }
        }

        public void SetChannelVolume(int channel, int volume)
        {
            int numChannels = GetNumChannels();
            int numPatterns = LibModPlugNative.NumPatterns(_modplugFile);

            for (var pattern = 0; pattern < numPatterns; ++pattern)
            {
                uint numRows = 0;
                unsafe
                {
                    LibModPlugNote* notes = (LibModPlugNote*)LibModPlugNative.ModPlug_GetPattern(_modplugFile, pattern, out numRows);
                    for (var j = channel; j < numRows * numChannels; j += numChannels)
                        notes[j].Volume = (byte)volume;
                }
            }
        }
    }
}
