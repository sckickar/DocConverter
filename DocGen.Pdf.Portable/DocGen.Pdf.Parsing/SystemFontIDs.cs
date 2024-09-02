using System.Text;

namespace DocGen.Pdf.Parsing;

internal static class SystemFontIDs
{
	internal const ushort FONT_FAMILY_ID = 1;

	internal const ushort MANUFACTURER_NAME_ID = 8;

	internal const ushort DESIGNER_ID = 9;

	internal const ushort SAMPLE_TEXT_ID = 19;

	internal const ushort WINDOWS_PLATFORM_ID = 3;

	internal const ushort WINDOWS_SYMBOL_ENCODING_ID = 0;

	internal const ushort WINDOWS_UNICODE_BMP_ENCODING_ID = 1;

	internal const ushort ENGLISH_US_ID = 1033;

	internal static Encoding GetEncodingFromEncodingID(ushort encodingId)
	{
		if ((uint)encodingId <= 1u)
		{
			return Encoding.BigEndianUnicode;
		}
		return null;
	}
}
