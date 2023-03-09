namespace CelticCode.Freetype;

using System;
using System.Collections.Generic;
using System.IO;

using FreeTypeSharp;
using FreeTypeSharp.Native;

using Veldrid;

using static FreeTypeSharp.Native.FT;

public class FontGenerator
{
    public static Dictionary<char, Glyph> Glyphs { get; set; } = new();

    public static unsafe void Generate(GraphicsDevice graphicsDevice, ref Texture texture)
    {
        using FreeTypeLibrary lib = new();

        FT_Err(FT_New_Face(lib.Native, "Fonts/CascadiaCode.ttf", 0, out nint face));

        FreeTypeFaceFacade ft = new(lib, face);

        FT_Set_Pixel_Sizes(ft.Face, 0, 12);

        FT_Library_SetLcdFilterWeights(lib.Native, new byte[] { 8, 77, 86, 77, 8 });

        uint atlasWidth = 0;
        uint atlasHeight = 0;

        for (uint index = 32; index < 128; index++)
        {
            FT_Err(FT_Load_Char(ft.Face, index, FT_LOAD_TARGET_LCD));
            atlasWidth += ft.GlyphBitmap.width;
            atlasHeight = Math.Max(atlasHeight, ft.GlyphBitmap.rows);
        }

        Console.WriteLine(atlasWidth);

        // For subpixel rendering, we need to divide the width by 3
        atlasWidth /= 3;

        texture = graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            atlasWidth,
            atlasHeight,
            1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled
        ));

        uint xoffset = 0;

        // Render each glyph to the atlas texture
        for (int index = 32; index < 128; index++)
        {
            char letter = (char)index;
            FT_Err(FT_Load_Char(ft.Face, letter, FT_LOAD_TARGET_LCD));
            FT_Err(FT_Render_Glyph((nint)ft.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_LCD));
            Console.WriteLine(ft.GlyphBitmap.width);
            uint w = ft.GlyphBitmap.width / 3;
            uint h = ft.GlyphBitmap.rows;

            byte[] img = new byte[w * h * 4];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte* r = (byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 0);
                    byte* g = (byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 1);
                    byte* b = (byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 2);

                    img[((x + (y * w)) * 4) + 0] = *r;
                    img[((x + (y * w)) * 4) + 1] = *g;
                    img[((x + (y * w)) * 4) + 2] = *b;
                    img[((x + (y * w)) * 4) + 3] = 255;

                    // if (x == 0)
                    // {
                    //     img[((x + (y * w)) * 4) + 0] = 255;
                    //     img[((x + (y * w)) * 4) + 1] = 0;
                    //     img[((x + (y * w)) * 4) + 2] = 0;
                    //     img[((x + (y * w)) * 4) + 3] = 255;
                    // }

                    // if (x == w - 1)
                    // {
                    //     img[((x + (y * w)) * 4) + 0] = 0;
                    //     img[((x + (y * w)) * 4) + 1] = 255;
                    //     img[((x + (y * w)) * 4) + 2] = 0;
                    //     img[((x + (y * w)) * 4) + 3] = 255;
                    // }
                }
            }

            Glyph value = new(
                new(ft.GlyphMetricHorizontalAdvance, ft.GlyphMetricVerticalAdvance),
                new(ft.GlyphBitmapLeft, ft.GlyphBitmapTop),
                new(w, h),
                xoffset / (float)atlasWidth
            );

            if (letter == ']' || letter == '[')
            {
                Console.WriteLine(letter + ":" + value + "-" + (xoffset));
            }

            Glyphs.Add((char)index, value);

            graphicsDevice.UpdateTexture(texture, img, xoffset, 0, 0, w, h, 1, 0, 0);

            xoffset += ft.GlyphBitmap.width / 3;
        }
    }

    private static void FT_Err(FT_Error err)
    {
        if (err != FT_Error.FT_Err_Ok)
        {
            throw new FreeTypeException(err);
        }
    }
}
