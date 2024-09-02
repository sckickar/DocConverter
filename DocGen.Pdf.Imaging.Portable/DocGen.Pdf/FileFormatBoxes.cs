using System.Runtime.InteropServices;

namespace DocGen.Pdf;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct FileFormatBoxes
{
	public const int READER_REQUIREMENTS_BOX = 1920099697;

	public const int JP2_SIGNATURE_BOX = 1783636000;

	public const int FILE_TYPE_BOX = 1718909296;

	public const int JP2_HEADER_BOX = 1785737832;

	public const int CONTIGUOUS_CODESTREAM_BOX = 1785737827;

	public const int INTELLECTUAL_PROPERTY_BOX = 1685074537;

	public const int XML_BOX = 2020437024;

	public const int UUID_BOX = 1970628964;

	public const int UUID_INFO_BOX = 1969843814;

	public const int IMAGE_HEADER_BOX = 1768449138;

	public const int BITS_PER_COMPONENT_BOX = 1651532643;

	public const int COLOUR_SPECIFICATION_BOX = 1668246642;

	public const int PALETTE_BOX = 1885564018;

	public const int COMPONENT_MAPPING_BOX = 1668112752;

	public const int CHANNEL_DEFINITION_BOX = 1667523942;

	public const int RESOLUTION_BOX = 1919251232;

	public const int CAPTURE_RESOLUTION_BOX = 1919251299;

	public const int DEFAULT_DISPLAY_RESOLUTION_BOX = 1919251300;

	public const int UUID_LIST_BOX = 1969451892;

	public const int URL_BOX = 1970433056;

	public const int IMB_VERS = 256;

	public const int IMB_C = 7;

	public const int IMB_UnkC = 1;

	public const int IMB_IPR = 0;

	public const int CSB_METH = 1;

	public const int CSB_PREC = 0;

	public const int CSB_APPROX = 0;

	public const int CSB_ENUM_SRGB = 16;

	public const int CSB_ENUM_GREY = 17;

	public const int FT_BR = 1785737760;
}
