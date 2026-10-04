using Assimp;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace Engine
{
    public class Texture : IDisposable
    {
        public int TextureID { get; set; }

        public Texture(string fileName, string baseDir)
        {            
            string path = Path.Combine(baseDir, fileName);            
            if (!File.Exists(path)) path = Path.Combine(baseDir, Path.GetFileName(fileName));
            
            try
            {
                if (File.Exists(path))
                {
                    if (string.Equals(Path.GetExtension(path), ".tga", StringComparison.OrdinalIgnoreCase))
                    {
                        TextureID = GenerateTextureID(path);
                    }
                    else
                    {
                        using (Bitmap bitmap = new Bitmap(path))
                            TextureID = GenerateTextureID(bitmap);
                    }
                    return;
                }
                Console.WriteLine($"Texture -> File not found: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Texture -> Could not load '{path}': {ex.Message}");
            }

            TextureID = GenerateTextureID(5, 5, Settings.Core.Default.MissingTextureColor);
        }

        public Texture(EmbeddedTexture texture)
        {
            if (texture.IsCompressed)
            {
                if (!texture.HasCompressedData) return;
                TextureID = GenerateTextureID(new MemoryStream(texture.CompressedData));
                return;
            }
            //if (!texture.HasNonCompressedData || texture.Width < 1 || texture.Height < 1) return;

            //Texel[] texels = texture.NonCompressedData;
            //Bitmap bitmap = new Bitmap(texture.Width, texture.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
            //BitmapData bmpData;
            //try
            //{
            //    bmpData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            //}
            //catch
            //{
            //    return;
            //}
            //IntPtr ptr = bmpData.Scan0;
            //Debug.Assert(bmpData.Stride > 0);
            //int countBytes = bmpData.Stride * bitmap.Height;
            //byte[] tempBuffer = new byte[countBytes];
            //int dataLineLength = bitmap.Width * 4;
            //int padding = bmpData.Stride - dataLineLength;
            //Debug.Assert(padding >= 0);
            //int n = 0;
            //foreach (var texel in texels)
            //{
            //    tempBuffer[n++] = texel.B;
            //    tempBuffer[n++] = texel.G;
            //    tempBuffer[n++] = texel.R;
            //    tempBuffer[n++] = texel.A;
            //    if (n % dataLineLength == 0) n += padding;
            //}
            //Marshal.Copy(tempBuffer, 0, ptr, countBytes);
            //bitmap.UnlockBits(bmpData);

            //image = bitmap;

        }

        public Texture(StringCollection faces)
        {
            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, TextureID);

            for (int i = 0; i < faces.Count; i++)
            {
                using (Bitmap bitmap = new Bitmap(Util.SkyboxPath + faces[i]))
                {
                    BitmapData data = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    if (data.Scan0 != null)
                    {
                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                            0,
                            PixelInternalFormat.Rgb,
                            bitmap.Width,
                            bitmap.Height,
                            0,
                            PixelFormat.Bgr,
                            PixelType.UnsignedByte,
                            data.Scan0);
                    }
                    else
                    {
                        throw new Exception("Doku y�kleme ba�ar�s�z oldu!");
                    }
                }
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        }

        public static int GenerateTextureID(Bitmap bitmap)
        {
            int ID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ID);

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            if (bmpData.Scan0 != null)
            {
                GL.TexImage2D(
                    TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    bitmap.Width, bitmap.Height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            }
            else
            {
                GL.ActiveTexture(0);
                throw new Exception("Doku y�kleme ba�ar�s�z oldu!");
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, -0.4f);


            GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out float maxAniso);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, maxAniso);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return ID;
        }

        public static int GenerateTextureID(Stream stream)
        {
            Bitmap bitmap = (Bitmap)Image.FromStream(stream);
            using (Graphics gfx = Graphics.FromImage(bitmap))
            {
                gfx.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            return GenerateTextureID(bitmap);
        }

        public static int GenerateTextureID(string TGAFile)
        {
            using (Pfim.IImage image = Pfim.Pfimage.FromFile(TGAFile))
            {
                System.Drawing.Imaging.PixelFormat format;
                switch (image.Format)
                {
                    case Pfim.ImageFormat.Rgba32: format = System.Drawing.Imaging.PixelFormat.Format32bppArgb; break;
                    case Pfim.ImageFormat.Rgb24: format = System.Drawing.Imaging.PixelFormat.Format24bppRgb; break;
                    case Pfim.ImageFormat.R5g6b5: format = System.Drawing.Imaging.PixelFormat.Format16bppRgb565; break;
                    case Pfim.ImageFormat.R5g5b5: format = System.Drawing.Imaging.PixelFormat.Format16bppRgb555; break;
                    case Pfim.ImageFormat.R5g5b5a1: format = System.Drawing.Imaging.PixelFormat.Format16bppArgb1555; break;
                    default:
                        throw new NotSupportedException($"Unsupported TGA pixel format: {image.Format}");
                }

                // Bitmap reads straight from the pinned pixel buffer, so keep it pinned until upload
                GCHandle handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    IntPtr data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    using (Bitmap bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, data))
                        return GenerateTextureID(bitmap);
                }
                finally
                {
                    handle.Free();
                }
            }
        }
        
        public static int GenerateTextureID(int width, int height, Color color)
        {
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gfx = Graphics.FromImage(bitmap))
            {
                gfx.Clear(color);
            }
            return GenerateTextureID(bitmap);
        }

        public void Dispose()
        {
            if (TextureID != 0)
            {
                GL.DeleteTexture(TextureID);
                TextureID = 0;
            }
            GC.SuppressFinalize(this);
        }

    }

}
