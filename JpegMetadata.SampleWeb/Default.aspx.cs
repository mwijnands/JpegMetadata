using System;
using XperiCode.JpegMetadata;

namespace XperiCode.JpegMetadata.SampleWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var adapter = new JpegMetadataAdapter(@"d:\test.jpg");

	        adapter.Metadata.Title = "Beach";
	        adapter.Metadata.Subject = "Summer holiday 2014";
	        adapter.Metadata.Rating = 4;
	        adapter.Metadata.Keywords.Add("beach");
	        adapter.Metadata.Comments = "This is a comment.";

            bool saved = adapter.Save();

            Response.Write(string.Format("Saved: {0}", saved));
        }
    }
}