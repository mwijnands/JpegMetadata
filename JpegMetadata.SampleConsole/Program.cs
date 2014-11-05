using System;
using XperiCode.JpegMetadata;

namespace XperiCode.JpegMetadata.SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var adapter = new JpegMetadataAdapter(@"d:\test.jpg");

            adapter.Metadata.Title = "Profile";
            adapter.Metadata.Rating = 3;
            adapter.Metadata.Comments += string.Format("{0}[{1:dd-MM-yyyy HH:mm:ss}] Added new comment.", Environment.NewLine, DateTime.Now);

            bool saved = adapter.Save();

            Console.Write("Saved: {0}", saved);
            Console.ReadKey();
        }
    }
}
