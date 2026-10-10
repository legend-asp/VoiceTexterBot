using FFMpegCore;
using VoiceTexterBot.Extensions;

namespace VoiceTexterBot.Utilities
{
    public static class AudioConverter
    {
        public static void TryConvert(string inputFile, string outputFile)
        {
            var ffmpegBin = Path.Combine(DirectoryExtension.GetSolutionRoot(), "FFmpeg-win64", "bin");
            GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegBin);

            FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                .WithFastStart())
                .ProcessSynchronously();
        }
    }
}