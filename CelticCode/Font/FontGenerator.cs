namespace CelticCode.Font;

using FreeTypeSharp;
using FreeTypeSharp.Native;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static FreeTypeSharp.Native.FT;

public class FontGenerator
{
    public static unsafe void Generate()
    {
        using FreeTypeLibrary lib = new();

        FT_Err(FT_New_Face(lib.Native, "Fonts/CascadiaCode.ttf", 0, out nint face));

        FreeTypeFaceFacade ft = new(lib, face);

        FT_Set_Pixel_Sizes(ft.Face, 0, 12);

        string text = "0a";

        for (int n = 0; n < text.Length; n++)
        {
            uint glyph_index = FT_Get_Char_Index(ft.Face, text[n]);
            FT_Err(FT_Load_Glyph(ft.Face, glyph_index, FT_LOAD_DEFAULT));
            FT_Err(FT_Render_Glyph((nint)ft.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_LCD));

            Image<Rgba32> img = new((int)ft.GlyphSlot->bitmap.width, (int)ft.GlyphSlot->bitmap.rows);
            for (int y = 0; y < ft.GlyphSlot->bitmap.rows; y++)
            {
                for (int x = 0; x < ft.GlyphSlot->bitmap.width; x++)
                {
                    byte* ptr = (byte*)(ft.GlyphSlot->bitmap.buffer + (y * ft.GlyphSlot->bitmap.pitch) + x);
                    img[x, y] = new Rgba32(255, 255, 255, *ptr);
                }
            }
            img.Save($"Test/{text[n]}.png");
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
