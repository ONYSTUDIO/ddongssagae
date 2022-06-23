using System;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class ZipHelper
{
    public static string Zip(string text)
    {
        return Convert.ToBase64String(Zip(Encoding.UTF8.GetBytes(text)));
    }

    public static byte[] Zip(byte[] rowData)
    {
        byte[] compressed = null;
        using (var outStream = new MemoryStream())
        {
            using (var zipStream = new GZipStream(outStream, CompressionMode.Compress))
                zipStream.Write(rowData, 0, rowData.Length);
            compressed = outStream.ToArray();
        }
        return compressed;
    }

    public static string Unzip(string text)
    {
        return Encoding.UTF8.GetString(Unzip(Convert.FromBase64String(text)));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("스타일", "IDE0063:간단한 'using' 문 사용", Justification = "<보류 중>")]
    public static byte[] Unzip(byte[] buffer)
    {
        byte[] uncompressed = null;
        using (var inStream = new MemoryStream(buffer))
        {
            using (var zipStream = new GZipStream(inStream, CompressionMode.Decompress))
            {
                using (var outStream = new MemoryStream())
                {
                    zipStream.CopyTo(outStream);

                    uncompressed = outStream.ToArray();
                }
            }
        }
        return uncompressed;
    }
}