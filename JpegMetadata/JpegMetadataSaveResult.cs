namespace XperiCode.JpegMetadata
{
    internal class JpegMetadataSaveResult
    {
        public bool IsSuccess { get; set; }
        public string FilePath { get; private set; }
        public JpegMetadata Metadata { get; private set; }

        public JpegMetadataSaveResult(string filePath, JpegMetadata metadata)
        {
            this.FilePath = filePath;
            this.Metadata = metadata;
        }
    }
}
