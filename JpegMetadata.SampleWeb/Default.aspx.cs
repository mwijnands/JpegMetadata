using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XperiCode.JpegMetadata;

namespace JpegMetadata.SampleWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var adapter = new JpegMetadataAdapter(@"d:\test.jpg");

            adapter.Metadata.Title = "Profile";
            adapter.Metadata.Rating = 3;
            adapter.Metadata.Comments += string.Format("{0}[{1:dd-MM-yyyy HH:mm:ss}] Added new comment.", Environment.NewLine, DateTime.Now);

            bool saved = adapter.Save();

            Response.Write(string.Format("Saved: {0}", saved));
        }
    }
}