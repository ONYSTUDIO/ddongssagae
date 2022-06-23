using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.IO;

public class HttpTextureStream
{
    public struct Header
    {
        public const char sign1 = 'U';
        public const char sign2 = 'N';
        public int width;
        public int height;
        public int texture_format;

        public Header(int in_width, int in_height, TextureFormat format)
        {
            width = in_width;
            height = in_height;
            texture_format = (int)format;
        }

        public Header(byte[] buffer)
        {
            BitConverter.ToChar(buffer, 0);
            BitConverter.ToChar(buffer, 1);
            width = BitConverter.ToInt32(buffer, 2);
            height = BitConverter.ToInt32(buffer, 6);
            texture_format = BitConverter.ToInt32(buffer, 10);
        }

        public byte[] GetData()
        {
            List<byte> header_bytes = new List<byte>();
            header_bytes.Add((byte)sign1);
            header_bytes.Add((byte)sign2);
            header_bytes.AddRange(BitConverter.GetBytes(width));
            header_bytes.AddRange(BitConverter.GetBytes(height));
            header_bytes.AddRange(BitConverter.GetBytes(texture_format));
            return header_bytes.ToArray();
        }
    }

    // 정적변수나 상수는 사이즈가 측정 안되므로 시그니쳐 2바이트만 임의로 더해준다
    public static int header_size { get { return Marshal.SizeOf(typeof(Header)) + 2; } }
    public const string ios_ext = ".pvrtc";
    public const string aos_ext = ".etc";
    public const string pc_ext = ".dxt";

#if UNITY_EDITOR
    public static void Write(byte[] image_buffer, int width, int height, TextureFormat format, string origin_path)
    {
        if (null == image_buffer || 0 == image_buffer.Length)
        {
            Log.Error("HttpTextureStream - error write");
            return;
        }

        Header header = new Header(width, height, format);
        byte[] header_buffer = header.GetData();
        byte[] final_buffer = new byte[header_buffer.Length + image_buffer.Length];

        Buffer.BlockCopy(header_buffer, 0, final_buffer, 0, header_buffer.Length);
        Buffer.BlockCopy(image_buffer, 0, final_buffer, header_buffer.Length, image_buffer.Length);

        string final_path = Path.GetDirectoryName(origin_path) + "/" + Path.GetFileNameWithoutExtension(origin_path) + GetExtension(format);
        File.WriteAllBytes(final_path, final_buffer);
    }

    public static string GetExtension(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.PVRTC_RGB2:
            case TextureFormat.PVRTC_RGB4:
            case TextureFormat.PVRTC_RGBA2:
            case TextureFormat.PVRTC_RGBA4:
                return ios_ext;

            case TextureFormat.ETC2_RGB:
            case TextureFormat.ETC2_RGBA1:
            case TextureFormat.ETC2_RGBA8:
            case TextureFormat.ETC_RGB4:
                return aos_ext;

            case TextureFormat.DXT1:
            case TextureFormat.DXT5:
                return pc_ext;

            default:
                return string.Empty;
        }
    }
#endif

    public static bool IsHttpCompressedTexture(byte[] data)
    {
        return data != null && data.Length > header_size && data[0] == Header.sign1 && data[1] == Header.sign2;
    }

    public static Texture2D Read(byte[] image_buffer)
    {
        if (null == image_buffer || 0 == image_buffer.Length)
        {
            Log.Error("HttpTextureStream - error read");
            return Texture2D.blackTexture;
        }
        if (false == IsHttpCompressedTexture(image_buffer))
        {
            Texture2D png_texture = new Texture2D(2, 2);
            png_texture.LoadImage(image_buffer);
            return png_texture;
        }

        Header header = new Header(image_buffer);

        byte[] pixel_buffer = new byte[image_buffer.Length - header_size];
        Buffer.BlockCopy(image_buffer, header_size, pixel_buffer, 0, pixel_buffer.Length);

        Texture2D texture = new Texture2D(header.width, header.height, (TextureFormat)header.texture_format, false);
        texture.LoadRawTextureData(pixel_buffer);
        texture.Apply();

        return texture;
    }
}
