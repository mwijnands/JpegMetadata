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
                return _metadata;
            }
        }

        public JpegMetadataAdapter(string filePath)
        {
            _filePath = filePath;
            _metadata = ReadMetadata(filePath);
        }

        public bool Save()
        {
            try
            {
                if (TrySave(_filePath, _metadata))
                {
                    return true;
                }

                return TryPadAndSave(_filePath, _metadata);
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
                var decoder = BitmapDecoder.Create(jpegStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                if (!decoder.CodecInfo.FileExtensions.Contains("jpg"))
                {
                    throw new ArgumentException("File is not a JPEG.");
                }

                var jpegFrame = decoder.Frames[0];
                var metadata = (BitmapMetadata)jpegFrame.Metadata;

                return CreateMetadata(metadata);
            }
        }

        private bool TrySave(string filePath, JpegMetadata metadata)
        {
            using (var jpegStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                var decoder = new JpegBitmapDecoder(jpegStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                var jpegFrame = decoder.Frames[0];
                var metadataWriter = jpegFrame.CreateInPlaceBitmapMetadataWriter();

                SetMetadata(metadataWriter, metadata);

                if (metadataWriter.TrySave())
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryPadAndSave(string filePath, JpegMetadata metadata)
        {
            var result = new JpegMetadataSaveResult(filePath, metadata);
            var thread = new Thread(() => PadAndSave(result));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return result.IsSuccess;
        }

        private void PadAndSave(JpegMetadataSaveResult result)
        {
            try
            {
                string tempFileName = Path.GetTempFileName();

                using (var jpegStream = new FileStream(result.FilePath, FileMode.Open, FileAccess.Read))
                {
                    var decoder = new JpegBitmapDecoder(jpegStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

                    var jpegFrame = decoder.Frames[0];
                    if (jpegFrame == null)
                    {
                        return;
                    }

                    var encoder = CreateJpegBitmapEncoderWithMetadata(jpegFrame, result.Metadata);

                    using (var tempFileStream = File.Open(tempFileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        encoder.Save(tempFileStream);
                    }
                }

                File.Copy(tempFileName, result.FilePath, true);

                try
                {
                    File.Delete(tempFileName);
                }
                catch (IOException)
                {
                    // Not a problem if temporary file can't be deleted.
                }

                result.IsSuccess = true;
            }
            catch (Exception)
            {
                // Ignore exception on this thread and don't set IsSuccess property.
            }
        }

        private JpegBitmapEncoder CreateJpegBitmapEncoderWithMetadata(BitmapFrame jpegFrame, JpegMetadata metadata)
        {
            var frameMetadata = (BitmapMetadata)jpegFrame.Metadata;
            if (frameMetadata == null)
            {
                frameMetadata = new BitmapMetadata("jpeg");
            }

            var metadataCopy = frameMetadata.Clone();
            uint padding = (uint)4096;

            metadataCopy.SetQuery("/app1/ifd/PaddingSchema:Padding", padding);
            metadataCopy.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", padding);
            metadataCopy.SetQuery("/xmp/PaddingSchema:Padding", padding);

            SetMetadata(metadataCopy, metadata);

            var newJpegFrame = BitmapFrame.Create(jpegFrame, jpegFrame.Thumbnail, metadataCopy, jpegFrame.ColorContexts);

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(newJpegFrame);

            return encoder;
        }

        private JpegMetadata CreateMetadata(BitmapMetadata metadata)
        {
            return new JpegMetadata
            {
                Title = metadata.Title ?? string.Empty,
                Subject = metadata.Subject ?? string.Empty,
                Rating = metadata.Rating,
                Keywords = metadata.Keywords == null ? new List<string>() : new List<string>(metadata.Keywords),
                Comments = metadata.Comment ?? string.Empty
            };
        }

        private void SetMetadata(BitmapMetadata destination, JpegMetadata source)
        {
            destination.Title = source.Title;
            destination.Subject = source.Subject;
            destination.Rating = source.Rating;
            destination.Keywords = new ReadOnlyCollection<string>(source.Keywords);
            destination.Comment = source.Comments;
        }
    }
}
