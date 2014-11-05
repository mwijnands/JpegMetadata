using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace XperiCode.JpegMetadata
{
    public class JpegMetadataAdapter
    {
        private readonly string _filePath;
        private readonly JpegMetadata _metadata;

        public JpegMetadata Metadata
        {
            get
            {
                return this._metadata;
            }
        }

        public JpegMetadataAdapter(string filePath)
        {
            this._filePath = filePath;
            this._metadata = this.ReadMetadata(filePath);
        }

        public bool Save()
        {
            try
            {
                if (TrySave())
                {
                    return true;
                }

                return TryPadAndSave();
            }
            catch
            {
                return false;
            }
        }

        private JpegMetadata ReadMetadata(string filePath)
        {
            using (var jpegStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var decoder = BitmapDecoder.Create(jpegStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                if (!decoder.CodecInfo.FileExtensions.Contains("jpg"))
                {
                    throw new ArgumentException("File is not a JPEG.");
                }

                var jpegFrame = decoder.Frames[0];
                var metaData = (BitmapMetadata)jpegFrame.Metadata;

                return new JpegMetadata
                {
                    Title = metaData.Title ?? string.Empty,
                    Subject = metaData.Subject ?? string.Empty,
                    Rating = metaData.Rating,
                    Keywords = metaData.Keywords == null ? new List<string>() : new List<string>(metaData.Keywords),
                    Comments = metaData.Comment ?? string.Empty
                };
            }
        }

        private bool TrySave()
        {
            using (var jpegStream = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                var decoder = new JpegBitmapDecoder(jpegStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                var jpegFrame = decoder.Frames[0];
                var metadataWriter = jpegFrame.CreateInPlaceBitmapMetadataWriter();

                SetMetadata(metadataWriter);

                if (metadataWriter.TrySave())
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryPadAndSave()
        {
            var start = new ParameterizedThreadStart(PadAndSave);
            var myThread = new Thread(start);
            var result = new JpegMetadataSaveResult();

            myThread.SetApartmentState(ApartmentState.STA);
            myThread.Start(result);

            while (myThread.IsAlive)
            {
            }

            return result.IsSuccess;
        }

        private void PadAndSave(object result)
        {
            try
            {
                string tempFileName = Path.GetTempFileName();

                using (var jpegStream = new FileStream(this._filePath, FileMode.Open, FileAccess.Read))
                {
                    var decoder = new JpegBitmapDecoder(jpegStream, BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);

                    var jpegFrame = decoder.Frames[0];
                    if (jpegFrame == null || jpegFrame.Metadata == null)
                    {
                        return;
                    }

                    var encoder = CreateJpegBitmapEncoderWithMetadata(jpegFrame);

                    using (var tempFileStream = File.Open(tempFileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        encoder.Save(tempFileStream);
                    }
                }

                File.Copy(tempFileName, _filePath, true);

                try
                {
                    File.Delete(tempFileName);
                }
                catch (IOException)
                {
                    // Not a problem if temporary file can't be deleted.
                }

                ((JpegMetadataSaveResult)result).IsSuccess = true;
            }
            catch
            {
                // Ignore exception on this thread and don't set IsSuccess property.
            }
        }

        private JpegBitmapEncoder CreateJpegBitmapEncoderWithMetadata(BitmapFrame jpegFrame)
        {

            var metadataCopy = (BitmapMetadata)jpegFrame.Metadata.Clone();
            uint padding = (uint)4096;

            metadataCopy.SetQuery("/app1/ifd/PaddingSchema:Padding", padding);
            metadataCopy.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", padding);
            metadataCopy.SetQuery("/xmp/PaddingSchema:Padding", padding);

            SetMetadata(metadataCopy);

            var newJpegFrame = BitmapFrame.Create(jpegFrame, jpegFrame.Thumbnail, metadataCopy, jpegFrame.ColorContexts);

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(newJpegFrame);

            return encoder;
        }

        private void SetMetadata(BitmapMetadata metadata)
        {
            metadata.Title = this.Metadata.Title;
            metadata.Subject = this.Metadata.Subject;
            metadata.Rating = this.Metadata.Rating;
            metadata.Keywords = new ReadOnlyCollection<string>(this.Metadata.Keywords);
            metadata.Comment = this.Metadata.Comments;
        }
    }
}
