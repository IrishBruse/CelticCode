namespace CelticCode.Freetype;

using System;
using System.Diagnostics;
using System.Drawing;

using FreeTypeSharp;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;

using RaylibSharp;

public static class FontAtlas
{
    public static unsafe Font GenerateSubpixelTexture(string fontPath, uint fontSize)
    {
        using FreeTypeLibrary lib = new();

        Debug.Assert(FT_New_Face(lib.Native, fontPath, 0, out nint face) == FT_Error.FT_Err_Ok);

        FT_Library_SetLcdFilter(lib.Native, FT_LcdFilter.FT_LCD_FILTER_DEFAULT);

        FreeTypeFaceFacade ft = new(lib, face);

        FT_Set_Pixel_Sizes(ft.Face, 0, fontSize);

        int atlasWidth = 0;
        int atlasHeight = 0;

        for (uint index = 32; index < 128; index++)
        {
            Debug.Assert(FT_Load_Char(ft.Face, index, FT_LOAD_TARGET_LCD) == FT_Error.FT_Err_Ok);
            atlasWidth += (int)ft.GlyphBitmap.width;
            atlasHeight = (int)Math.Max(atlasHeight, ft.GlyphBitmap.rows);
        }

        // For subpixel rendering, we need to divide the width by 3
        atlasWidth /= 3;

        Image image = Raylib.GenImageColor(atlasWidth, atlasHeight, Color.Black);

        Font font = new()
        {
            GlyphPadding = 0,
            BaseSize = (int)fontSize,
            Recs = new RectangleF[128],
            Glyphs = new GlyphInfo[128],
            GlyphCount = 128,
        };

        int xoffset = 0;

        // Render each glyph to the atlas texture
        for (int index = 32; index < 128; index++)
        {
            char letter = (char)index;

            Debug.Assert(FT_Load_Char(ft.Face, letter, FT_LOAD_TARGET_LCD) == FT_Error.FT_Err_Ok);

            FT_Bitmap_Embolden(lib.Native, (nint)ft.GlyphBitmapPtr, 0, (nint)(double)(2.0 / 64.0));
            FT_Render_Glyph((nint)ft.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_LCD);

            int w = (int)(ft.GlyphBitmap.width / 3);
            int h = (int)ft.GlyphBitmap.rows;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte r = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 0);
                    byte g = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 1);
                    byte b = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 2);

                    Raylib.ImageDrawPixel(ref image, x + xoffset, y, Color.FromArgb(r, g, b));
                }
            }

            font.Glyphs[index] = new GlyphInfo
            {
                Value = index,
                OffsetX = ft.GlyphMetricHorizontalAdvance,
                OffsetY = ft.GlyphMetricVerticalAdvance - ft.GlyphBitmapTop,
                AdvanceX = 0,
            };

            font.Recs[index] = new RectangleF(xoffset, 0, w, h);

            xoffset += w;
        }

        font.Texture = Raylib.LoadTextureFromImage(image);

        return font;
    }
}
