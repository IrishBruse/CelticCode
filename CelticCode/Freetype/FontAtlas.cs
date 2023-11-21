namespace CelticCode.Freetype;

using System.Diagnostics;

using FreeTypeSharp;
using static FreeTypeSharp.Native.FT;

using RaylibSharp;

using FreeTypeSharp.Native;
using System.Drawing;

public static class FontAtlas
{
    public static unsafe Font GenerateSubpixelTexture(string fontPath, uint fontSize)
    {
        using FreeTypeLibrary lib = new();

        Debug.Assert(FT_New_Face(lib.Native, fontPath, 0x00000000, out nint face) == FT_Error.FT_Err_Ok);

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
            Debug.Assert(FT_Render_Glyph((nint)ft.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_LCD) == FT_Error.FT_Err_Ok);

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

            // int w = (int)ft.GlyphBitmap.width;
            // int h = (int)ft.GlyphBitmap.rows;

            // for (int y = 0; y < h; y++)
            // {
            //     for (int x = 0; x < w; x++)
            //     {
            //         byte v = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + x);
            //         Raylib.ImageDrawPixel(ref image, x + xoffset, y, Color.FromArgb(v, v, v));
            //     }
            // }

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

    private static unsafe bool EmboldenGlyphBitmap(FreeTypeLibrary lib, FreeTypeFaceFacade ft, int xStrength, int yStrength)
    {
        FT_Error err = FT_Bitmap_Embolden(lib.Native, (IntPtr)ft.GlyphBitmapPtr, xStrength, yStrength);
        if (err != FT_Error.FT_Err_Ok)
        {
            return false;
        }

        FT_FaceRec* faceRec = ft.FaceRec;

        if ((int)faceRec->glyph->advance.x > 0)
        {
            faceRec->glyph->advance.x += xStrength;
        }

        if ((int)faceRec->glyph->advance.y > 0)
        {
            faceRec->glyph->advance.x += yStrength;
        }

        faceRec->glyph->metrics.width += xStrength;
        faceRec->glyph->metrics.height += yStrength;
        faceRec->glyph->metrics.horiBearingY += yStrength;
        faceRec->glyph->metrics.horiAdvance += xStrength;
        faceRec->glyph->metrics.vertBearingX -= xStrength / 2;
        faceRec->glyph->metrics.vertBearingY += yStrength;
        faceRec->glyph->metrics.vertAdvance += yStrength;

        faceRec->glyph->bitmap_top += yStrength >> 6;

        return true;
    }
}
