# MimeTypes

This project uses a copy of /etc/media.types file from the media-types linux package.

# Usage

## Init
The static constructor tries to read the embedded ressource "mime.types.gz" of the executing assembly.
You can also use ReadMimeTypesFrom(Stream,..) to replace all mime types.

        ReadMimeTypesFrom(Stream stream, bool disposeStream = false)
        
Stream can be either GZip compressed or raw. <br/>One mime type per line followed by file suffixes seperated by any combination of ' ' or ',' or ';' or '\t' <br/>
'#' Marks a comment

Examples:

        image/jpg jpg jpeg
        video/jpg # Will be skipped since no suffix specified
        
## GetMimeTypeExtensions
        IEnumerable<string> GetMimeTypeExtensions(string mimetype)
Returns an IEnumerable<string> of all suffices for the specified mime type.
        
## GetMimeTypes
        1. IEnumerable<string> GetMimeTypes(string fileName)
        2. IEnumerable<string> GetMimeTypes()

1.  Returns an IEnumerable<string> containing all found mime types, or FallbackMimeType if non was found.<br/>
  fileName must contain '.' for example "test.png" or ".png" or "/test/test.png" for "image/png".<br/>
  Throws ArgumentNullException if fileName is null.
2.  Returns a distinct enumeration of all mime types

  
## TryGetMimeType
        bool TryGetMimeType(string? fileName, out IEnumerable<string> mimeTypes)
True if the suffix is found. mimeTypes is a list of all found mime types.<br/>
fileName must contain '.' for example "test.png" or ".png" or "/test/test.png" for "image/png".
  
## Example
        MimeTypes.MimeTypes.GetMimeType("test.png"); // returns { "image/png" }
