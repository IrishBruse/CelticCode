namespace CelticCode.Freetype;

using FreeTypeSharp;
using static FreeTypeSharp.Native.FT;

using RaylibSharp;

using FreeTypeSharp.Native;
using System.Runtime.CompilerServices;

public static class FontAtlas
{
    public static unsafe Font GenerateSubpixelTexture(string fontPath, uint fontSize, out int lineHeight, int boldness = 1)
    {
        using FreeTypeLibrary lib = new();

        Assert(FT_New_Face(lib.Native, fontPath, boldness << 16, out nint face));
        FreeTypeFaceFacade ft = new(lib, face);

        Assert(FT_Set_Pixel_Sizes(ft.Face, 0, fontSize));

        int atlasWidth = 0;
        int atlasHeight = 0;

        // Calculate the size of the atlas
        for (uint index = 32; index < 127; index++)
        {
            Assert(FT_Load_Char(ft.Face, index, FT_LOAD_TARGET_LCD));
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
            Recs = new Rectangle[128],
            Glyphs = new GlyphInfo[128],
            GlyphCount = 128,
        };

        int penX = 0;
        uint previous = 0;

        // Render each glyph to the atlas texture
        for (int index = 32; index < 127; index++)
        {
            uint glyphIndex = FT_Get_Char_Index(face, (uint)index);

            Assert(FT_Get_Kerning(ft.Face, previous, glyphIndex, (uint)FT_Kerning_Mode.FT_KERNING_DEFAULT, out FT_Vector kerning));

            penX += (int)kerning.x;

            Assert(FT_Load_Glyph(ft.Face, glyphIndex, FT_LOAD_TARGET_LCD));
            Assert(FT_Render_Glyph((nint)ft.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_LCD));

            int w = (int)(ft.GlyphBitmap.width / 3);
            int h = (int)ft.GlyphBitmap.rows;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte r = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 0);
                    byte g = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 1);
                    byte b = *(byte*)(ft.GlyphBitmap.buffer + (y * ft.GlyphBitmap.pitch) + (x * 3) + 2);

                    Raylib.ImageDrawPixel(ref image, x + penX, y, new Color(r, g, b));
                }
            }

            font.Glyphs[index] = new GlyphInfo
            {
                Value = index,
                OffsetX = ft.GlyphBitmapLeft,
                OffsetY = ft.GlyphMetricVerticalAdvance - ft.GlyphBitmapTop,
                AdvanceX = ft.GlyphMetricHorizontalAdvance,
            };

            // Console.WriteLine($"{(char)index} {w} {ft.GlyphBitmapLeft,2} {ft.GlyphMetricWidth,2} {ft.GlyphMetricHorizontalAdvance,2}");

            font.Recs[index] = new Rectangle(penX, 0, w, h);

            penX += w;
        }

        font.Texture = Raylib.LoadTextureFromImage(image);

        lineHeight = ft.LineSpacing;

        return font;
    }

    static void Assert(FT_Error condition, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
#if DEBUG
        if (condition != FT_Error.FT_Err_Ok)
        {
            throw new Exception("Assertion failed in " + condition + " " + file + ":" + lineNumber);
        }
#else
        Console.WriteLine("Assertion failed in " + file + ":" + lineNumber);
#endif
    }
}
