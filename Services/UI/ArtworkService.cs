using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace MusicBeeWrapped.Services.UI
{
    /// <summary>
    /// Extracts album artwork from audio files (.m4a, .mp3, .flac) or album folders,
    /// downscales to a crisp thumbnail, and returns a self-contained base64 data URI.
    /// Silently returns null if no artwork is found or if any error occurs.
    /// </summary>
    public class ArtworkService
    {
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string GetTrackArtworkDataUri(TrackPlay play, int maxDimension = 380)
        {
            if (play == null) return null;

            string cacheKey = !string.IsNullOrWhiteSpace(play.Album)
                ? $"{play.Artist ?? ""}|||{play.Album}"
                : (play.FileUrl ?? $"{play.Artist}|||{play.Title}");

            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            string dataUri = null;
            try
            {
                string resolvedPath = ResolveAudioFilePath(play.FileUrl);
                if (!string.IsNullOrEmpty(resolvedPath))
                {
                    byte[] rawBytes = ExtractEmbeddedArtworkBytes(resolvedPath)
                                   ?? FindFolderArtworkBytes(resolvedPath);

                    if (rawBytes != null && rawBytes.Length > 64)
                    {
                        dataUri = ConvertToThumbnailDataUri(rawBytes, maxDimension);
                    }
                }
            }
            catch
            {
                // Silently ignore any file/decoding errors
                dataUri = null;
            }

            _cache[cacheKey] = dataUri;
            return dataUri;
        }

        public string GetAlbumArtworkDataUri(IEnumerable<TrackPlay> plays, string artist, string album, int maxDimension = 380)
        {
            if (plays == null) return null;

            string cacheKey = $"{artist ?? ""}|||{album ?? ""}";
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var candidates = plays
                .Where(p => !string.IsNullOrWhiteSpace(p.FileUrl) &&
                            (string.IsNullOrEmpty(album) || string.Equals((p.Album ?? "").Trim(), album.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                            (string.IsNullOrEmpty(artist) || string.Equals((p.Artist ?? "").Trim(), artist.Trim(), StringComparison.OrdinalIgnoreCase)))
                .Select(p => p.FileUrl)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5);

            foreach (var fileUrl in candidates)
            {
                try
                {
                    string resolved = ResolveAudioFilePath(fileUrl);
                    if (!string.IsNullOrEmpty(resolved))
                    {
                        byte[] rawBytes = ExtractEmbeddedArtworkBytes(resolved)
                                       ?? FindFolderArtworkBytes(resolved);
                        if (rawBytes != null && rawBytes.Length > 64)
                        {
                            var uri = ConvertToThumbnailDataUri(rawBytes, maxDimension);
                            if (!string.IsNullOrEmpty(uri))
                            {
                                _cache[cacheKey] = uri;
                                return uri;
                            }
                        }
                    }
                }
                catch
                {
                    // Try next candidate silently
                }
            }

            _cache[cacheKey] = null;
            return null;
        }

        public static string GetMonogram(string primary, string secondary = null)
        {
            string source = !string.IsNullOrWhiteSpace(primary) ? primary.Trim() : (secondary ?? "♪").Trim();
            if (string.IsNullOrEmpty(source)) return "♪";

            var words = source.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                              .Where(w => !string.Equals(w, "The", StringComparison.OrdinalIgnoreCase) &&
                                          !string.Equals(w, "A", StringComparison.OrdinalIgnoreCase))
                              .ToList();

            if (words.Count >= 2 && char.IsLetterOrDigit(words[0][0]) && char.IsLetterOrDigit(words[1][0]))
            {
                return (char.ToUpperInvariant(words[0][0]).ToString() + char.ToUpperInvariant(words[1][0])).ToString();
            }
            if (words.Count == 1 && words[0].Length >= 2)
            {
                return words[0].Substring(0, 2).ToUpperInvariant();
            }
            return source.Substring(0, 1).ToUpperInvariant();
        }

        private string ResolveAudioFilePath(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return null;
            if (fileUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                fileUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                if (File.Exists(fileUrl)) return fileUrl;

                // Fallback for relocated/unmounted drive letters (e.g. P:\Music\... -> User's Music folder)
                if (fileUrl.Length > 3 && fileUrl[1] == ':' && fileUrl[2] == '\\')
                {
                    string relativeFromDrive = fileUrl.Substring(3); // e.g. "Music\iPhone\..."
                    string myMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

                    if (!string.IsNullOrEmpty(myMusic))
                    {
                        string candidate1 = Path.Combine(myMusic, relativeFromDrive);
                        if (File.Exists(candidate1)) return candidate1;

                        if (relativeFromDrive.StartsWith("Music\\", StringComparison.OrdinalIgnoreCase))
                        {
                            string withoutMusicPrefix = relativeFromDrive.Substring(6);
                            string candidate2 = Path.Combine(myMusic, withoutMusicPrefix);
                            if (File.Exists(candidate2)) return candidate2;
                        }
                    }
                }
            }
            catch
            {
                // Ignore path resolution errors
            }

            return null;
        }

        private byte[] ExtractEmbeddedArtworkBytes(string filePath)
        {
            string ext = Path.GetExtension(filePath)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) return null;

            // Read up to the first 6 MB of the file (headers & embedded artwork live at the front)
            byte[] headerBytes;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int maxRead = (int)Math.Min(fs.Length, 6 * 1024 * 1024);
                headerBytes = new byte[maxRead];
                int totalRead = 0;
                while (totalRead < maxRead)
                {
                    int r = fs.Read(headerBytes, totalRead, maxRead - totalRead);
                    if (r <= 0) break;
                    totalRead += r;
                }
            }

            if (ext == ".m4a" || ext == ".mp4" || ext == ".aac")
            {
                return ExtractM4aCoverArt(headerBytes);
            }
            if (ext == ".mp3")
            {
                return ExtractId3v2CoverArt(headerBytes);
            }
            if (ext == ".flac")
            {
                return ExtractFlacCoverArt(headerBytes);
            }

            return null;
        }

        private byte[] ExtractM4aCoverArt(byte[] bytes)
        {
            // Look for 'covr' atom followed by 'data' atom
            int limit = bytes.Length - 24;
            for (int i = 0; i < limit; i++)
            {
                if (bytes[i] == 0x63 && bytes[i + 1] == 0x6F && bytes[i + 2] == 0x76 && bytes[i + 3] == 0x72) // 'covr'
                {
                    if (bytes[i + 8] == 0x64 && bytes[i + 9] == 0x61 && bytes[i + 10] == 0x74 && bytes[i + 11] == 0x61) // 'data'
                    {
                        int dataAtomSize = (bytes[i + 4] << 24) | (bytes[i + 5] << 16) | (bytes[i + 6] << 8) | bytes[i + 7];
                        int imgLen = dataAtomSize - 16;
                        int imgStart = i + 20;
                        if (imgLen > 128 && imgStart + imgLen <= bytes.Length)
                        {
                            var img = new byte[imgLen];
                            Buffer.BlockCopy(bytes, imgStart, img, 0, imgLen);
                            return img;
                        }
                    }
                }
            }
            return null;
        }

        private byte[] ExtractId3v2CoverArt(byte[] bytes)
        {
            if (bytes.Length < 14 || bytes[0] != 'I' || bytes[1] != 'D' || bytes[2] != '3')
                return null;

            int version = bytes[3];
            int tagSize = ((bytes[6] & 0x7F) << 21) | ((bytes[7] & 0x7F) << 14) | ((bytes[8] & 0x7F) << 7) | (bytes[9] & 0x7F);
            int limit = Math.Min(bytes.Length - 16, tagSize + 10);

            for (int i = 10; i < limit; i++)
            {
                // Look for APIC frame
                if (bytes[i] == 'A' && bytes[i + 1] == 'P' && bytes[i + 2] == 'I' && bytes[i + 3] == 'C')
                {
                    int frameSize = version == 4
                        ? (((bytes[i + 4] & 0x7F) << 21) | ((bytes[i + 5] & 0x7F) << 14) | ((bytes[i + 6] & 0x7F) << 7) | (bytes[i + 7] & 0x7F))
                        : ((bytes[i + 4] << 24) | (bytes[i + 5] << 16) | (bytes[i + 6] << 8) | bytes[i + 7]);

                    if (frameSize <= 16 || i + 10 + frameSize > bytes.Length)
                        continue;

                    int frameStart = i + 10;
                    int frameEnd = frameStart + frameSize;

                    // Scan inside APIC frame for JPEG (FF D8 FF) or PNG (89 50 4E 47) magic bytes
                    for (int p = frameStart; p < frameStart + 128 && p < frameEnd - 8; p++)
                    {
                        bool isJpeg = bytes[p] == 0xFF && bytes[p + 1] == 0xD8 && bytes[p + 2] == 0xFF;
                        bool isPng = bytes[p] == 0x89 && bytes[p + 1] == 0x50 && bytes[p + 2] == 0x4E && bytes[p + 3] == 0x47;
                        if (isJpeg || isPng)
                        {
                            int imgLen = frameEnd - p;
                            var img = new byte[imgLen];
                            Buffer.BlockCopy(bytes, p, img, 0, imgLen);
                            return img;
                        }
                    }
                }
            }
            return null;
        }

        private byte[] ExtractFlacCoverArt(byte[] bytes)
        {
            if (bytes.Length < 42 || bytes[0] != 'f' || bytes[1] != 'L' || bytes[2] != 'a' || bytes[3] != 'C')
                return null;

            int pos = 4;
            while (pos + 4 < bytes.Length)
            {
                byte header = bytes[pos];
                bool isLast = (header & 0x80) != 0;
                int blockType = header & 0x7F;
                int blockLen = (bytes[pos + 1] << 16) | (bytes[pos + 2] << 8) | bytes[pos + 3];
                pos += 4;

                if (blockLen <= 0 || pos + blockLen > bytes.Length) break;

                if (blockType == 6) // METADATA_BLOCK_PICTURE
                {
                    for (int p = pos; p < pos + 256 && p < pos + blockLen - 8; p++)
                    {
                        bool isJpeg = bytes[p] == 0xFF && bytes[p + 1] == 0xD8 && bytes[p + 2] == 0xFF;
                        bool isPng = bytes[p] == 0x89 && bytes[p + 1] == 0x50 && bytes[p + 2] == 0x4E && bytes[p + 3] == 0x47;
                        if (isJpeg || isPng)
                        {
                            int imgLen = (pos + blockLen) - p;
                            var img = new byte[imgLen];
                            Buffer.BlockCopy(bytes, p, img, 0, imgLen);
                            return img;
                        }
                    }
                }

                if (isLast) break;
                pos += blockLen;
            }
            return null;
        }

        private byte[] FindFolderArtworkBytes(string audioFilePath)
        {
            try
            {
                string dir = Path.GetDirectoryName(audioFilePath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return null;

                string[] commonNames = { "cover.jpg", "folder.jpg", "front.jpg", "album.jpg", "cover.png", "folder.png" };
                foreach (var name in commonNames)
                {
                    string candidate = Path.Combine(dir, name);
                    if (File.Exists(candidate))
                    {
                        return File.ReadAllBytes(candidate);
                    }
                }
            }
            catch
            {
                // Ignore
            }
            return null;
        }

        private string ConvertToThumbnailDataUri(byte[] rawBytes, int maxDimension)
        {
            using (var msIn = new MemoryStream(rawBytes))
            using (var srcImage = Image.FromStream(msIn, false, false))
            {
                int srcW = srcImage.Width;
                int srcH = srcImage.Height;
                if (srcW <= 0 || srcH <= 0) return null;

                double scale = Math.Min(1.0, (double)maxDimension / Math.Max(srcW, srcH));
                int dstW = Math.Max(1, (int)Math.Round(srcW * scale));
                int dstH = Math.Max(1, (int)Math.Round(srcH * scale));

                using (var bmp = new Bitmap(dstW, dstH))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(srcImage, 0, 0, dstW, dstH);

                    using (var msOut = new MemoryStream())
                    {
                        var jpegEncoder = ImageCodecInfo.GetImageDecoders()
                            .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        if (jpegEncoder != null)
                        {
                            var encParams = new EncoderParameters(1);
                            encParams.Param[0] = new EncoderParameter(Encoder.Quality, 82L);
                            bmp.Save(msOut, jpegEncoder, encParams);
                        }
                        else
                        {
                            bmp.Save(msOut, ImageFormat.Jpeg);
                        }
                        return "data:image/jpeg;base64," + Convert.ToBase64String(msOut.ToArray());
                    }
                }
            }
        }
    }
}
