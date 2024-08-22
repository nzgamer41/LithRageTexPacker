using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Bmp;
using System;
using System.IO;
using SixLabors.ImageSharp.Formats;

public class TextureSaver
{
    public static void SaveTextureFromBmp(string bmpFilePath, string paletteFilePath, string textureFilePath)
    {
        //CreatePaletteFromBmp(bmpFilePath);
        //string paletteFilePath = Path.ChangeExtension(bmpFilePath, ".PAL");
        using (Image<Rgb24> image = Image.Load<Rgb24>(bmpFilePath))
        using (FileStream paletteStream = new FileStream(paletteFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        using (BinaryReader paletteReader = new BinaryReader(paletteStream))
        using (BinaryWriter paletteWriter = new BinaryWriter(paletteStream))
        using (FileStream textureStream = new FileStream(textureFilePath, FileMode.Create, FileAccess.Write))
        using (BinaryWriter textureWriter = new BinaryWriter(textureStream))
        {
            // Ensure the image is 256x128
            if (image.Width != 256 || image.Height != 128)
            {
                throw new InvalidOperationException("The image must be 256x128 pixels.");
            }

            // Read the palette
            Rgb24[] palette = new Rgb24[256];
            for (int i = 0; i < 256; i++)
            {
                byte r = paletteReader.ReadByte();
                byte g = paletteReader.ReadByte();
                byte b = paletteReader.ReadByte();
                palette[i] = new Rgb24(r, g, b); // 24-bit color
            }


            // Write the height and width to the texture file (note the swapped order)
            textureWriter.Write((short)image.Width);
            textureWriter.Write((short)image.Height);

            // Map each pixel in the BMP to an index in the external palette and save the texture
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Rgb24 bmpColor = image[x, y];
                    //byte paletteIndex = FindOrAddPaletteIndex(externalPalette, ref paletteSize, bmpColor, paletteWriter);
                    byte paletteIndex = FindNearestPaletteIndex(palette, palette.Length, bmpColor);

                    // Write the palette index to the texture file
                    int flippedY = image.Height - 1 - y;
                    textureWriter.Write(paletteIndex);
                }
            }
        }
    }

    private static byte FindNearestPaletteIndex(Rgb24[] palette, int paletteSize, Rgb24 color)
    {
        int nearestIndex = 0;
        double nearestDistance = double.MaxValue;

        for (int i = 0; i < paletteSize; i++)
        {
            double distance = GetColorDistance(color, palette[i]);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return (byte)nearestIndex;
    }

    private static double GetColorDistance(Rgb24 color1, Rgb24 color2)
    {
        int distance = Math.Abs(color1.R - color2.R) +
                   Math.Abs(color1.G - color2.G) +
                   Math.Abs(color1.B - color2.B);
        return distance;
    }

    public static void CreatePaletteFromBmp(string bmpFilePath)
    {
        using (FileStream bmpStream = new FileStream(bmpFilePath, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(bmpStream))
        {
            // BMP file header is 14 bytes
            reader.BaseStream.Seek(14, SeekOrigin.Begin);

            // Read the DIB header to find the color palette
            int dibHeaderSize = reader.ReadInt32();

            // Seek to the color table, which is right after the DIB header
            reader.BaseStream.Seek(14 + dibHeaderSize, SeekOrigin.Begin);

            // BMP 8-bit color palette consists of 256 colors, each of 4 bytes (B, G, R, 0)
            byte[] palette = new byte[256 * 3]; // 3 bytes per color for R, G, B

            for (int i = 0; i < 256; i++)
            {
                byte blue = reader.ReadByte();
                byte green = reader.ReadByte();
                byte red = reader.ReadByte();
                reader.ReadByte(); // Skip the reserved byte (usually 0)

                palette[i * 3] = red;
                palette[i * 3 + 1] = green;
                palette[i * 3 + 2] = blue;
            }

            // Write the palette to a .PAL file
            string paletteFilePath = Path.ChangeExtension(bmpFilePath, ".PAL");

            using (FileStream paletteStream = new FileStream(paletteFilePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter paletteWriter = new BinaryWriter(paletteStream))
            {
                paletteWriter.Write(palette);
            }

            Console.WriteLine($"Palette saved to {paletteFilePath}");
        }
    }
}
