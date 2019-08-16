using System.IO;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Results
{
    public struct FileAttachment
    {
        private FileAttachment(Stream stream, string filename)
        {
            Stream = stream;
            Filename = filename;
        }

        public Stream Stream { get; }
        public string Filename { get; }

        public static FileAttachment FromStream(Stream stream, string filename)
        {
            return new FileAttachment(stream, filename);
        }
    }
}