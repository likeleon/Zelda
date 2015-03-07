using System;
using System.Collections.Generic;
using System.IO;

namespace Zelda.Game
{
    class ChannelInfo
    {
        public string FileName { get; set;}
        public StreamWriter Writer { get; set;}
    }

    static class Log
    {
        static readonly Dictionary<string, ChannelInfo> _channels = new Dictionary<string, ChannelInfo>();
        static string _supportDir;

        public static void Initialize(string supportDir)
        {
            _supportDir = supportDir;
        }

        public static void AddChannel(string channelName, string baseFilenName)
        {
            if (_channels.ContainsKey(channelName))
                return;

            if (string.IsNullOrEmpty(baseFilenName))
            {
                _channels.Add(channelName, new ChannelInfo());
                return;
            }

            foreach (string fileName in FileNamesForChannel(channelName, baseFilenName))
            {
                try
                {
                    StreamWriter writer = File.CreateText(fileName);
                    writer.AutoFlush = true;

                    _channels.Add(channelName, new ChannelInfo()
                    {
                        FileName = fileName,
                        Writer = writer
                    });
                    return;
                }
                catch (IOException)
                {
                }
            }
        }

        static IEnumerable<string> FileNamesForChannel(string channelName, string baseFileName)
        {
            string path = _supportDir + "Logs";
            Directory.CreateDirectory(path);

            for (int i = 0; ; ++i)
                yield return Path.Combine(path, i > 0 ? "{0}.{1}".F(baseFileName, i) : baseFileName);
        }

        public static void Write(string channel, string format, params object[] args)
        {
            ChannelInfo info;
            if (!_channels.TryGetValue(channel, out info))
                throw new Exception("Tried logging to non-existant channel " + channel);

            if (info.Writer != null)
                info.Writer.WriteLine(format, args);

            Console.WriteLine("{0}: {1}".F(channel, String.Format(format, args)));
        }
    }
}
