using System.Collections.Generic;

namespace XperiCode.JpegMetadata
{
    public class JpegMetadata
    {
        public string Title { get; set; }
        public string Subject { get; set; }
        public int Rating { get; set; }
        public IList<string> Keywords { get; set; }
        public string Comments { get; set; }

        internal JpegMetadata()
        {
            this.Keywords = new List<string>();
        }
    }
}
