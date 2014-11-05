using System;
using XperiCode.JpegMetadata;

namespace XperiCode.JpegMetadata.SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var adapter = new JpegMetadataAdapter(@"d:\test.jpg");

	        adapter.Metadata.Title = "Beach";
	        adapter.Metadata.Subject = "Summer holiday 2014";
	        adapter.Metadata.Rating = 4;
	        adapter.Metadata.Keywords.Add("beach");
	        adapter.Metadata.Comments = "This is a comment.";

            bool saved = adapter.Save();

            Console.Write("Saved: {0}", saved);
            Console.ReadKey();
        }
    }
}
