namespace CelticCode.Freetype;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using FreeTypeSharp;
using FreeTypeSharp.Native;

using Veldrid;

using static FreeTypeSharp.Native.FT;

public class FontGenerator
{
    public static Dictionary<char, Glyph> Glyphs { get; set; } = new();

    public static unsafe void GenerateSubpixelTexture(GraphicsDevice graphicsDevice, ref Texture texture)
    {
        using FreeTypeLibrary lib = new();

        Debug.Assert(FT_New_Face(lib.Native, "Assets/Fonts/CascadiaCode.ttf", 0, out nint face) != FT_Error.FT_Err_Ok);

        FreeTypeFaceFacade ft = new(lib, face);

        FT_Set_Pixel_Sizes(ft.Face, 0, 12);

        uint atlasWidth = 0;
        uint atlasHeight = 0;

        for (uint index = 32; index < 128; index++)
        {
            Debug.Assert(FT_Load_Char(ft.Face, index, FT_LOAD_TARGET_LCD) != FT_Error.FT_Err_Ok);
            atlasWidth += ft.GlyphBitmap.width;
            atlasHeight = Math.Max(atlasHeight, ft.GlyphBitmap.rows);
        }

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

            Debug.Assert(FT_Load_Char(ft.Face, letter, FT_LOAD_TARGET_LCD) != FT_Error.FT_Err_Ok);
            Debug.Assert(FT_Render_Glyph((nint)ft.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_LCD) != FT_Error.FT_Err_Ok);

            uint w = ft.GlyphBitmap.width / 3;
            uint h = ft.GlyphBitmap.rows;

            byte[] img = new byte[w * h * 4];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte r = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 0);
                    byte g = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 1);
                    byte b = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 2);

                    img[((x + (y * w)) * 4) + 0] = r;
                    img[((x + (y * w)) * 4) + 1] = g;
                    img[((x + (y * w)) * 4) + 2] = b;
                    img[((x + (y * w)) * 4) + 3] = 255;
                }
            }

            Glyph value = new(
                new(ft.GlyphMetricHorizontalAdvance, ft.GlyphMetricVerticalAdvance),
                new(ft.GlyphBitmapLeft, ft.GlyphBitmapTop),
                new(w, h),
                xoffset / (float)atlasWidth
            );

            Glyphs.Add((char)index, value);

            graphicsDevice.UpdateTexture(texture, img, xoffset, 0, 0, w, h, 1, 0, 0);

            xoffset += ft.GlyphBitmap.width / 3;
        }
    }

    public static unsafe void GenerateGrayscaleTexture(GraphicsDevice graphicsDevice, ref Texture texture)
    {
        using FreeTypeLibrary lib = new();

        Debug.Assert(FT_New_Face(lib.Native, "Assets/Fonts/CascadiaCode.ttf", 0, out nint face) != FT_Error.FT_Err_Ok);

        FreeTypeFaceFacade ft = new(lib, face);

        FT_Set_Pixel_Sizes(ft.Face, 0, 12);

        uint atlasWidth = 0;
        uint atlasHeight = 0;

        for (uint index = 32; index < 128; index++)
        {
            Debug.Assert(FT_Load_Char(ft.Face, index, FT_LOAD_TARGET_LCD) != FT_Error.FT_Err_Ok);
            atlasWidth += ft.GlyphBitmap.width;
            atlasHeight = Math.Max(atlasHeight, ft.GlyphBitmap.rows);
        }

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

            Debug.Assert(FT_Load_Char(ft.Face, letter, FT_LOAD_TARGET_NORMAL) != FT_Error.FT_Err_Ok);
            Debug.Assert(FT_Render_Glyph((nint)ft.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_NORMAL) != FT_Error.FT_Err_Ok);

            uint w = ft.GlyphBitmap.width;
            uint h = ft.GlyphBitmap.rows;

            byte[] img = new byte[w * h * 4];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte gray = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + x + 0);

                    img[((x + (y * w)) * 4) + 0] = gray;
                    img[((x + (y * w)) * 4) + 1] = gray;
                    img[((x + (y * w)) * 4) + 2] = gray;
                    img[((x + (y * w)) * 4) + 3] = 255;
                }
            }

            Glyph value = new(
                new(ft.GlyphMetricHorizontalAdvance, ft.GlyphMetricVerticalAdvance),
                new(ft.GlyphBitmapLeft, ft.GlyphBitmapTop),
                new(w, h),
                xoffset / (float)atlasWidth
            );

            Glyphs.Add((char)index, value);

            graphicsDevice.UpdateTexture(texture, img, xoffset, 0, 0, w, h, 1, 0, 0);

            xoffset += ft.GlyphBitmap.width;
        }
    }
}
