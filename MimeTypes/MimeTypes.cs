using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MimeTypes
{
    public static class MimeTypes
    {
        private const string FALLBACK_MIMETYPE = "application/octet-stream";
        private static string? _fallbackMimeType = null;

        /// <summary>
        /// Sets the default mimetype. If null "application/octet-stream" will be used.
        /// </summary>
        [AllowNull]
        public static string FallbackMimeType { get => _fallbackMimeType ?? FALLBACK_MIMETYPE; set => _fallbackMimeType = value; }

        private static ILookup<string,string> mimeTypes;
        private static ILookup<string,string> suffixes;

        static MimeTypes()
        {
            mimeTypes = Enumerable.Empty<string>().ToLookup(s => s, StringComparer.OrdinalIgnoreCase);
            suffixes = Enumerable.Empty<string>().ToLookup(s => s,StringComparer.OrdinalIgnoreCase);
            var execAss = Assembly.GetExecutingAssembly();
            string ressName = execAss.GetManifestResourceNames().Where(name => name.EndsWith("mime.types.gz",StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (!string.IsNullOrEmpty(ressName))
            {
                var stream = execAss.GetManifestResourceStream(ressName);
                ReadMimeTypesFrom(stream, true);
            }
        }

        /// <summary>
        /// Replaces all the mime types with the content of the specified stream
        /// </summary>
        /// <param name="stream">
        /// Can be a gzip stream, can contain '#'-comments, mimetype followed by sequence of suffixes without '.'.
        /// <br/>
        /// Allowed seperation chars are ',' ' ' '\t' ';'
        /// <br/>
        /// Example:        
        /// <br/>
        /// image/jpg jpg jpeg
        /// video/jpg #will be skipped since no suffix specified
        /// </param>
        /// <param name="disposeStream">
        /// True, if the stream should be disposed
        /// </param>
        public static void ReadMimeTypesFrom(Stream stream, bool disposeStream = false)
        {
            if (!stream.CanSeek)
            {
                MemoryStream ms = new();
                stream.CopyTo(ms);
                if (disposeStream) stream.Dispose();
                stream = ms;
                disposeStream = true;
            }
            try
            {
                using System.IO.Compression.GZipStream gzip = new(stream,System.IO.Compression.CompressionMode.Decompress,true);
                mimeTypes = ReadStream(gzip);
            }
            catch (System.IO.InvalidDataException)
            {
                _ = stream.Seek(0, SeekOrigin.Begin);
                mimeTypes = ReadStream(stream);
            }
            finally
            {
                if (disposeStream) stream.Dispose();
                if (mimeTypes != null)
                    suffixes = mimeTypes.SelectMany(group => group.Select(ext => (group.Key, ext))).ToLookup(pair => pair.ext, pair => pair.Key, StringComparer.OrdinalIgnoreCase);
            }
        }

        private static ILookup<string, string> ReadStream(Stream stream)
        {
            List<(string key,string extensions)> data = new();
            using var sr = new StreamReader(stream);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains('#'))
                    line = line[..line.IndexOf('#')];
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts =  line.Split(new[] {' ', '\t' ,',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                    for (int i = 1; i < parts.Length; i++)                    
                        data.Add((parts[0],parts[i]));             
            }
            return data.ToLookup(data => data.key, data=> data.extensions,StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Attempts to fetch all available file extensions for a MIME-type.
        /// </summary>
        /// <param name="mimeType">The name of the MIME-type</param>
        /// <returns>All available extensions for the given MIME-type</returns>
        public static IEnumerable<string> GetMimeTypeExtensions(string mimeType)
        {
            if (mimeType is null)
            {
                throw new ArgumentNullException(nameof(mimeType));
            }
            return mimeTypes[mimeType];
        }

        /// <summary>
        /// Fetches all available MIME-types.
        /// </summary>
        /// <returns>All available MIME-types</returns>
        public static IEnumerable<string> GetMimeTypes() => mimeTypes.Select(group => group.Key);

        /// <summary>
        /// Tries to get the MIME-type for the given file name.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="mimeType">The MIME-type for the given file name.</param>
        /// <returns><c>true</c> if a MIME-type was found, <c>false</c> otherwise.</returns>
        public static bool TryGetMimeType(string? fileName, out IEnumerable<string> mimeTypes)
        {
            if (fileName is null)
            {
                mimeTypes = Enumerable.Empty<string>();
                return false;
            }

            var dotIndex = fileName.LastIndexOf('.');

            if (dotIndex != -1 && fileName.Length > dotIndex + 1)
            {
                mimeTypes = MimeTypes.suffixes[fileName[(dotIndex + 1)..]];
                return mimeTypes.Any();
            }
            mimeTypes = Enumerable.Empty<string>();
            return false;
        }

        /// <summary>
        /// Gets the MIME-type for the given file name,
        /// or <see cref="FallbackMimeType"/> if a mapping doesn't exist.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The MIME-type for the given file name.</returns>
        public static IEnumerable<string> GetMimeTypes(string fileName)
        {
            return fileName is null
                ? throw new ArgumentNullException(nameof(fileName))
                : TryGetMimeType(fileName, out var result) ? result : Enumerable.Repeat(FallbackMimeType, 1);
        }

        #region Common Mime Types
        public static string SVG => "image/svg+xml";
        public static string JPEG => "image/jpeg";
        public static string JPG => JPEG;
        public static string PNG => "image/png";
        public static string TIFF => "image/tiff";
        public static string AVIF => "image/avif";
        public static string AAC => "audio/aac";
        public static string AVI => "video/x-msvideo";
        public static string BIN => "application/octet-stream";
        public static string BMP => "image/bmp";

        public static string CSS => "text/css";
        public static string CSV => "text/csv";
        public static string DOC => "application/msword";
        public static string DOCX => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public static string GZ => "application/gzip";
        public static string GIF => "image/gif";
        public static string HTML => "text/html";
        public static string ICO => "image/vnd.microsoft.icon";
        public static string JAR => "application/java-archive";
        public static string JS => "text/javascript";
        public static string JSON => "application/json";

        public static string MIDI => "audio/x-midi";

        public static string MP3 => "audio/mp3";

        public static string MP4 => "video/mp4";
        public static string MPEG => "video/mpeg";
        public static string OGA => "audio/ogg";
        public static string OGV => "video/ogg";
        public static string OPUS => "audio/opus";
        public static string PDF => "application/pdf";

        public static string PHP => "text/x-php";

        public static string TXT => "text/plain";

        #endregion

        /// <summary>
        /// Returns true if one possible mimetype starts with "video/"
        /// </summary>
        /// <param name="filename">The name or full path of the file.</param>
        /// <returns>True if any possible mime type starts with "video/"</returns>
        public static bool IsVideo(string filename)
        {
            return GetMimeTypes(filename).Any(mimetype => mimetype.StartsWith("video/", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns true if one possible mimetype starts with "audio/"
        /// </summary>
        /// <param name="filename">The name or full path of the file.</param>
        /// <returns>True if any possible mime type starts with "audio/"</returns>
        public static bool IsAudio(string filename)
        {
            return GetMimeTypes(filename).Any(mimetype => mimetype.StartsWith("audio/", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns true if one possible mimetype starts with "image/"
        /// </summary>
        /// <param name="filename">The name or full path of the file.</param>
        /// <returns>True if any possible mime type starts with "image/"</returns>
        public static bool IsImage(string filename)
        {
            return GetMimeTypes(filename).Any(mimetype => mimetype.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns true if one possible mimetype starts with "text/"
        /// </summary>
        /// <param name="filename">The name or full path of the file.</param>
        /// <returns>True if any possible mime type starts with "text/"</returns>
        public static bool IsText(string filename)
        {
            return GetMimeTypes(filename).Any(mimetype => mimetype.StartsWith("text/", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns true if one possible mimetype is either audio, video or image
        /// </summary>
        /// <param name="filename">The name or full path of the file.</param>
        /// <returns>True if any possible mime type starts with "video/", "audio/" or "image/"</returns>
        public static bool IsMedia(string filename)
        {
            return GetMimeTypes(filename).Any(mimetype => mimetype.StartsWith("video/", StringComparison.InvariantCultureIgnoreCase)
            || mimetype.StartsWith("audio/", StringComparison.InvariantCultureIgnoreCase)
            || mimetype.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
