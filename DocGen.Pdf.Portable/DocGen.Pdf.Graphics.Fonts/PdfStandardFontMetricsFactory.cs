namespace DocGen.Pdf.Graphics.Fonts;

internal class PdfStandardFontMetricsFactory
{
	private const float c_subSuperScriptFactor = 1.52f;

	private const float c_HelveticaAscent = 931f;

	private const float c_HelveticaDescent = -225f;

	private const string c_HelveticaName = "Helvetica";

	private const float c_HelveticaBoldAscent = 962f;

	private const float c_HelveticaBoldDescent = -228f;

	private const string c_HelveticaBoldName = "Helvetica-Bold";

	private const float c_HelveticaItalicAscent = 931f;

	private const float c_HelveticaItalicDescent = -225f;

	private const string c_HelveticaItalicName = "Helvetica-Oblique";

	private const float c_HelveticaBoldItalicAscent = 962f;

	private const float c_HelveticaBoldItalicDescent = -228f;

	private const string c_HelveticaBoldItalicName = "Helvetica-BoldOblique";

	private const float c_CourierAscent = 805f;

	private const float c_CourierDescent = -250f;

	private const string c_CourierName = "Courier";

	private const float c_CourierBoldAscent = 801f;

	private const float c_CourierBoldDescent = -250f;

	private const string c_CourierBoldName = "Courier-Bold";

	private const float c_CourierItalicAscent = 805f;

	private const float c_CourierItalicDescent = -250f;

	private const string c_CourierItalicName = "Courier-Oblique";

	private const float c_CourierBoldItalicAscent = 801f;

	private const float c_CourierBoldItalicDescent = -250f;

	private const string c_CourierBoldItalicName = "Courier-BoldOblique";

	private const float c_TimesAscent = 898f;

	private const float c_TimesDescent = -218f;

	private const string c_TimesName = "Times-Roman";

	private const float c_TimesBoldAscent = 935f;

	private const float c_TimesBoldDescent = -218f;

	private const string c_TimesBoldName = "Times-Bold";

	private const float c_TimesItalicAscent = 883f;

	private const float c_TimesItalicDescent = -217f;

	private const string c_TimesItalicName = "Times-Italic";

	private const float c_TimesBoldItalicAscent = 921f;

	private const float c_TimesBoldItalicDescent = -218f;

	private const string c_TimesBoldItalicName = "Times-BoldItalic";

	private const float c_symbolAscent = 1010f;

	private const float c_symbolDescent = -293f;

	private const string c_symbolName = "Symbol";

	private const float c_zapfDingbatsAscent = 820f;

	private const float c_zapfDingbatsDescent = -143f;

	private const string c_zapfDingbatsName = "ZapfDingbats";

	private static float[] c_arialWidth = new float[224]
	{
		278f, 278f, 355f, 556f, 556f, 889f, 667f, 191f, 333f, 333f,
		389f, 584f, 278f, 333f, 278f, 278f, 556f, 556f, 556f, 556f,
		556f, 556f, 556f, 556f, 556f, 556f, 278f, 278f, 584f, 584f,
		584f, 556f, 1015f, 667f, 667f, 722f, 722f, 667f, 611f, 778f,
		722f, 278f, 500f, 667f, 556f, 833f, 722f, 778f, 667f, 778f,
		722f, 667f, 611f, 722f, 667f, 944f, 667f, 667f, 611f, 278f,
		278f, 278f, 469f, 556f, 333f, 556f, 556f, 500f, 556f, 556f,
		278f, 556f, 556f, 222f, 222f, 500f, 222f, 833f, 556f, 556f,
		556f, 556f, 333f, 500f, 278f, 556f, 500f, 722f, 500f, 500f,
		500f, 334f, 260f, 334f, 584f, 0f, 556f, 0f, 222f, 556f,
		333f, 1000f, 556f, 556f, 333f, 1000f, 667f, 333f, 1000f, 0f,
		611f, 0f, 0f, 222f, 222f, 333f, 333f, 350f, 556f, 1000f,
		333f, 1000f, 500f, 333f, 944f, 0f, 500f, 667f, 0f, 333f,
		556f, 556f, 556f, 556f, 260f, 556f, 333f, 737f, 370f, 556f,
		584f, 0f, 737f, 333f, 400f, 584f, 333f, 333f, 333f, 556f,
		537f, 278f, 333f, 333f, 365f, 556f, 834f, 834f, 834f, 611f,
		667f, 667f, 667f, 667f, 667f, 667f, 1000f, 722f, 667f, 667f,
		667f, 667f, 278f, 278f, 278f, 278f, 722f, 722f, 778f, 778f,
		778f, 778f, 778f, 584f, 778f, 722f, 722f, 722f, 722f, 667f,
		667f, 611f, 556f, 556f, 556f, 556f, 556f, 556f, 889f, 500f,
		556f, 556f, 556f, 556f, 278f, 278f, 278f, 278f, 556f, 556f,
		556f, 556f, 556f, 556f, 556f, 584f, 611f, 556f, 556f, 556f,
		556f, 500f, 556f, 500f
	};

	private static float[] c_arialBoldWidth = new float[224]
	{
		278f, 333f, 474f, 556f, 556f, 889f, 722f, 238f, 333f, 333f,
		389f, 584f, 278f, 333f, 278f, 278f, 556f, 556f, 556f, 556f,
		556f, 556f, 556f, 556f, 556f, 556f, 333f, 333f, 584f, 584f,
		584f, 611f, 975f, 722f, 722f, 722f, 722f, 667f, 611f, 778f,
		722f, 278f, 556f, 722f, 611f, 833f, 722f, 778f, 667f, 778f,
		722f, 667f, 611f, 722f, 667f, 944f, 667f, 667f, 611f, 333f,
		278f, 333f, 584f, 556f, 333f, 556f, 611f, 556f, 611f, 556f,
		333f, 611f, 611f, 278f, 278f, 556f, 278f, 889f, 611f, 611f,
		611f, 611f, 389f, 556f, 333f, 611f, 556f, 778f, 556f, 556f,
		500f, 389f, 280f, 389f, 584f, 0f, 556f, 0f, 278f, 556f,
		500f, 1000f, 556f, 556f, 333f, 1000f, 667f, 333f, 1000f, 0f,
		611f, 0f, 0f, 278f, 278f, 500f, 500f, 350f, 556f, 1000f,
		333f, 1000f, 556f, 333f, 944f, 0f, 500f, 667f, 0f, 333f,
		556f, 556f, 556f, 556f, 280f, 556f, 333f, 737f, 370f, 556f,
		584f, 0f, 737f, 333f, 400f, 584f, 333f, 333f, 333f, 611f,
		556f, 278f, 333f, 333f, 365f, 556f, 834f, 834f, 834f, 611f,
		722f, 722f, 722f, 722f, 722f, 722f, 1000f, 722f, 667f, 667f,
		667f, 667f, 278f, 278f, 278f, 278f, 722f, 722f, 778f, 778f,
		778f, 778f, 778f, 584f, 778f, 722f, 722f, 722f, 722f, 667f,
		667f, 611f, 556f, 556f, 556f, 556f, 556f, 556f, 889f, 556f,
		556f, 556f, 556f, 556f, 278f, 278f, 278f, 278f, 611f, 611f,
		611f, 611f, 611f, 611f, 611f, 584f, 611f, 611f, 611f, 611f,
		611f, 556f, 611f, 556f
	};

	private static float[] c_fixedWidth = new float[224]
	{
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f, 600f,
		600f, 600f, 600f, 600f
	};

	private static float[] c_timesRomanWidth = new float[224]
	{
		250f, 333f, 408f, 500f, 500f, 833f, 778f, 180f, 333f, 333f,
		500f, 564f, 250f, 333f, 250f, 278f, 500f, 500f, 500f, 500f,
		500f, 500f, 500f, 500f, 500f, 500f, 278f, 278f, 564f, 564f,
		564f, 444f, 921f, 722f, 667f, 667f, 722f, 611f, 556f, 722f,
		722f, 333f, 389f, 722f, 611f, 889f, 722f, 722f, 556f, 722f,
		667f, 556f, 611f, 722f, 722f, 944f, 722f, 722f, 611f, 333f,
		278f, 333f, 469f, 500f, 333f, 444f, 500f, 444f, 500f, 444f,
		333f, 500f, 500f, 278f, 278f, 500f, 278f, 778f, 500f, 500f,
		500f, 500f, 333f, 389f, 278f, 500f, 500f, 722f, 500f, 500f,
		444f, 480f, 200f, 480f, 541f, 0f, 500f, 0f, 333f, 500f,
		444f, 1000f, 500f, 500f, 333f, 1000f, 556f, 333f, 889f, 0f,
		611f, 0f, 0f, 333f, 333f, 444f, 444f, 350f, 500f, 1000f,
		333f, 980f, 389f, 333f, 722f, 0f, 444f, 722f, 0f, 333f,
		500f, 500f, 500f, 500f, 200f, 500f, 333f, 760f, 276f, 500f,
		564f, 0f, 760f, 333f, 400f, 564f, 300f, 300f, 333f, 500f,
		453f, 250f, 333f, 300f, 310f, 500f, 750f, 750f, 750f, 444f,
		722f, 722f, 722f, 722f, 722f, 722f, 889f, 667f, 611f, 611f,
		611f, 611f, 333f, 333f, 333f, 333f, 722f, 722f, 722f, 722f,
		722f, 722f, 722f, 564f, 722f, 722f, 722f, 722f, 722f, 722f,
		556f, 500f, 444f, 444f, 444f, 444f, 444f, 444f, 667f, 444f,
		444f, 444f, 444f, 444f, 278f, 278f, 278f, 278f, 500f, 500f,
		500f, 500f, 500f, 500f, 500f, 564f, 500f, 500f, 500f, 500f,
		500f, 500f, 500f, 500f
	};

	private static float[] c_timesRomanBoldWidth = new float[224]
	{
		250f, 333f, 555f, 500f, 500f, 1000f, 833f, 278f, 333f, 333f,
		500f, 570f, 250f, 333f, 250f, 278f, 500f, 500f, 500f, 500f,
		500f, 500f, 500f, 500f, 500f, 500f, 333f, 333f, 570f, 570f,
		570f, 500f, 930f, 722f, 667f, 722f, 722f, 667f, 611f, 778f,
		778f, 389f, 500f, 778f, 667f, 944f, 722f, 778f, 611f, 778f,
		722f, 556f, 667f, 722f, 722f, 1000f, 722f, 722f, 667f, 333f,
		278f, 333f, 581f, 500f, 333f, 500f, 556f, 444f, 556f, 444f,
		333f, 500f, 556f, 278f, 333f, 556f, 278f, 833f, 556f, 500f,
		556f, 556f, 444f, 389f, 333f, 556f, 500f, 722f, 500f, 500f,
		444f, 394f, 220f, 394f, 520f, 0f, 500f, 0f, 333f, 500f,
		500f, 1000f, 500f, 500f, 333f, 1000f, 556f, 333f, 1000f, 0f,
		667f, 0f, 0f, 333f, 333f, 500f, 500f, 350f, 500f, 1000f,
		333f, 1000f, 389f, 333f, 722f, 0f, 444f, 722f, 0f, 333f,
		500f, 500f, 500f, 500f, 220f, 500f, 333f, 747f, 300f, 500f,
		570f, 0f, 747f, 333f, 400f, 570f, 300f, 300f, 333f, 556f,
		540f, 250f, 333f, 300f, 330f, 500f, 750f, 750f, 750f, 500f,
		722f, 722f, 722f, 722f, 722f, 722f, 1000f, 722f, 667f, 667f,
		667f, 667f, 389f, 389f, 389f, 389f, 722f, 722f, 778f, 778f,
		778f, 778f, 778f, 570f, 778f, 722f, 722f, 722f, 722f, 722f,
		611f, 556f, 500f, 500f, 500f, 500f, 500f, 500f, 722f, 444f,
		444f, 444f, 444f, 444f, 278f, 278f, 278f, 278f, 500f, 556f,
		500f, 500f, 500f, 500f, 500f, 570f, 500f, 556f, 556f, 556f,
		556f, 500f, 556f, 500f
	};

	private static float[] c_timesRomanItalicWidth = new float[224]
	{
		250f, 333f, 420f, 500f, 500f, 833f, 778f, 214f, 333f, 333f,
		500f, 675f, 250f, 333f, 250f, 278f, 500f, 500f, 500f, 500f,
		500f, 500f, 500f, 500f, 500f, 500f, 333f, 333f, 675f, 675f,
		675f, 500f, 920f, 611f, 611f, 667f, 722f, 611f, 611f, 722f,
		722f, 333f, 444f, 667f, 556f, 833f, 667f, 722f, 611f, 722f,
		611f, 500f, 556f, 722f, 611f, 833f, 611f, 556f, 556f, 389f,
		278f, 389f, 422f, 500f, 333f, 500f, 500f, 444f, 500f, 444f,
		278f, 500f, 500f, 278f, 278f, 444f, 278f, 722f, 500f, 500f,
		500f, 500f, 389f, 389f, 278f, 500f, 444f, 667f, 444f, 444f,
		389f, 400f, 275f, 400f, 541f, 0f, 500f, 0f, 333f, 500f,
		556f, 889f, 500f, 500f, 333f, 1000f, 500f, 333f, 944f, 0f,
		556f, 0f, 0f, 333f, 333f, 556f, 556f, 350f, 500f, 889f,
		333f, 980f, 389f, 333f, 667f, 0f, 389f, 556f, 0f, 389f,
		500f, 500f, 500f, 500f, 275f, 500f, 333f, 760f, 276f, 500f,
		675f, 0f, 760f, 333f, 400f, 675f, 300f, 300f, 333f, 500f,
		523f, 250f, 333f, 300f, 310f, 500f, 750f, 750f, 750f, 500f,
		611f, 611f, 611f, 611f, 611f, 611f, 889f, 667f, 611f, 611f,
		611f, 611f, 333f, 333f, 333f, 333f, 722f, 667f, 722f, 722f,
		722f, 722f, 722f, 675f, 722f, 722f, 722f, 722f, 722f, 556f,
		611f, 500f, 500f, 500f, 500f, 500f, 500f, 500f, 667f, 444f,
		444f, 444f, 444f, 444f, 278f, 278f, 278f, 278f, 500f, 500f,
		500f, 500f, 500f, 500f, 500f, 675f, 500f, 500f, 500f, 500f,
		500f, 444f, 500f, 444f
	};

	public static float[] c_timesRomanBoldItalicWidth = new float[224]
	{
		250f, 389f, 555f, 500f, 500f, 833f, 778f, 278f, 333f, 333f,
		500f, 570f, 250f, 333f, 250f, 278f, 500f, 500f, 500f, 500f,
		500f, 500f, 500f, 500f, 500f, 500f, 333f, 333f, 570f, 570f,
		570f, 500f, 832f, 667f, 667f, 667f, 722f, 667f, 667f, 722f,
		778f, 389f, 500f, 667f, 611f, 889f, 722f, 722f, 611f, 722f,
		667f, 556f, 611f, 722f, 667f, 889f, 667f, 611f, 611f, 333f,
		278f, 333f, 570f, 500f, 333f, 500f, 500f, 444f, 500f, 444f,
		333f, 500f, 556f, 278f, 278f, 500f, 278f, 778f, 556f, 500f,
		500f, 500f, 389f, 389f, 278f, 556f, 444f, 667f, 500f, 444f,
		389f, 348f, 220f, 348f, 570f, 0f, 500f, 0f, 333f, 500f,
		500f, 1000f, 500f, 500f, 333f, 1000f, 556f, 333f, 944f, 0f,
		611f, 0f, 0f, 333f, 333f, 500f, 500f, 350f, 500f, 1000f,
		333f, 1000f, 389f, 333f, 722f, 0f, 389f, 611f, 0f, 389f,
		500f, 500f, 500f, 500f, 220f, 500f, 333f, 747f, 266f, 500f,
		606f, 0f, 747f, 333f, 400f, 570f, 300f, 300f, 333f, 576f,
		500f, 250f, 333f, 300f, 300f, 500f, 750f, 750f, 750f, 500f,
		667f, 667f, 667f, 667f, 667f, 667f, 944f, 667f, 667f, 667f,
		667f, 667f, 389f, 389f, 389f, 389f, 722f, 722f, 722f, 722f,
		722f, 722f, 722f, 570f, 722f, 722f, 722f, 722f, 722f, 611f,
		611f, 500f, 500f, 500f, 500f, 500f, 500f, 500f, 722f, 444f,
		444f, 444f, 444f, 444f, 278f, 278f, 278f, 278f, 500f, 556f,
		500f, 500f, 500f, 500f, 500f, 570f, 500f, 556f, 556f, 556f,
		556f, 444f, 500f, 444f
	};

	private static float[] c_symbolWidth = new float[190]
	{
		250f, 333f, 713f, 500f, 549f, 833f, 778f, 439f, 333f, 333f,
		500f, 549f, 250f, 549f, 250f, 278f, 500f, 500f, 500f, 500f,
		500f, 500f, 500f, 500f, 500f, 500f, 278f, 278f, 549f, 549f,
		549f, 444f, 549f, 722f, 667f, 722f, 612f, 611f, 763f, 603f,
		722f, 333f, 631f, 722f, 686f, 889f, 722f, 722f, 768f, 741f,
		556f, 592f, 611f, 690f, 439f, 768f, 645f, 795f, 611f, 333f,
		863f, 333f, 658f, 500f, 500f, 631f, 549f, 549f, 494f, 439f,
		521f, 411f, 603f, 329f, 603f, 549f, 549f, 576f, 521f, 549f,
		549f, 521f, 549f, 603f, 439f, 576f, 713f, 686f, 493f, 686f,
		494f, 480f, 200f, 480f, 549f, 750f, 620f, 247f, 549f, 167f,
		713f, 500f, 753f, 753f, 753f, 753f, 1042f, 987f, 603f, 987f,
		603f, 400f, 549f, 411f, 549f, 549f, 713f, 494f, 460f, 549f,
		549f, 549f, 549f, 1000f, 603f, 1000f, 658f, 823f, 686f, 795f,
		987f, 768f, 768f, 823f, 768f, 768f, 713f, 713f, 713f, 713f,
		713f, 713f, 713f, 768f, 713f, 790f, 790f, 890f, 823f, 549f,
		250f, 713f, 603f, 603f, 1042f, 987f, 603f, 987f, 603f, 494f,
		329f, 790f, 790f, 786f, 713f, 384f, 384f, 384f, 384f, 384f,
		384f, 494f, 494f, 494f, 494f, 329f, 274f, 686f, 686f, 686f,
		384f, 384f, 384f, 384f, 384f, 384f, 494f, 494f, 494f, -1f
	};

	private static float[] c_zapfDingbatsWidth = new float[202]
	{
		278f, 974f, 961f, 974f, 980f, 719f, 789f, 790f, 791f, 690f,
		960f, 939f, 549f, 855f, 911f, 933f, 911f, 945f, 974f, 755f,
		846f, 762f, 761f, 571f, 677f, 763f, 760f, 759f, 754f, 494f,
		552f, 537f, 577f, 692f, 786f, 788f, 788f, 790f, 793f, 794f,
		816f, 823f, 789f, 841f, 823f, 833f, 816f, 831f, 923f, 744f,
		723f, 749f, 790f, 792f, 695f, 776f, 768f, 792f, 759f, 707f,
		708f, 682f, 701f, 826f, 815f, 789f, 789f, 707f, 687f, 696f,
		689f, 786f, 787f, 713f, 791f, 785f, 791f, 873f, 761f, 762f,
		762f, 759f, 759f, 892f, 892f, 788f, 784f, 438f, 138f, 277f,
		415f, 392f, 392f, 668f, 668f, 390f, 390f, 317f, 317f, 276f,
		276f, 509f, 509f, 410f, 410f, 234f, 234f, 334f, 334f, 732f,
		544f, 544f, 910f, 667f, 760f, 760f, 776f, 595f, 694f, 626f,
		788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f,
		788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f,
		788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f,
		788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f, 788f,
		894f, 838f, 1016f, 458f, 748f, 924f, 748f, 918f, 927f, 928f,
		928f, 834f, 873f, 828f, 924f, 924f, 917f, 930f, 931f, 463f,
		883f, 836f, 836f, 867f, 867f, 696f, 696f, 874f, 874f, 760f,
		946f, 771f, 865f, 771f, 888f, 967f, 888f, 831f, 873f, 927f,
		970f, 918f
	};

	private PdfStandardFontMetricsFactory()
	{
	}

	public static PdfFontMetrics GetMetrics(PdfFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = null;
		pdfFontMetrics = fontFamily switch
		{
			PdfFontFamily.Helvetica => GetHelveticaMetrics(fontFamily, fontStyle, size), 
			PdfFontFamily.Courier => GetCourierMetrics(fontFamily, fontStyle, size), 
			PdfFontFamily.TimesRoman => GetTimesMetrics(fontFamily, fontStyle, size), 
			PdfFontFamily.Symbol => GetSymbolMetrics(fontFamily, fontStyle, size), 
			PdfFontFamily.ZapfDingbats => GetZapfDingbatsMetrics(fontFamily, fontStyle, size), 
			_ => GetHelveticaMetrics(PdfFontFamily.Helvetica, fontStyle, size), 
		};
		pdfFontMetrics.Name = fontFamily.ToString();
		pdfFontMetrics.SubScriptSizeFactor = 1.52f;
		pdfFontMetrics.SuperscriptSizeFactor = 1.52f;
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetHelveticaMetrics(PdfFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		if ((fontStyle & PdfFontStyle.Bold) > PdfFontStyle.Regular && (fontStyle & PdfFontStyle.Italic) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 962f;
			pdfFontMetrics.Descent = -228f;
			pdfFontMetrics.PostScriptName = "Helvetica-BoldOblique";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_arialBoldWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else if ((fontStyle & PdfFontStyle.Bold) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 962f;
			pdfFontMetrics.Descent = -228f;
			pdfFontMetrics.PostScriptName = "Helvetica-Bold";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_arialBoldWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else if ((fontStyle & PdfFontStyle.Italic) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 931f;
			pdfFontMetrics.Descent = -225f;
			pdfFontMetrics.PostScriptName = "Helvetica-Oblique";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_arialWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else
		{
			pdfFontMetrics.Ascent = 931f;
			pdfFontMetrics.Descent = -225f;
			pdfFontMetrics.PostScriptName = "Helvetica";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_arialWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetCourierMetrics(PdfFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		if ((fontStyle & PdfFontStyle.Bold) > PdfFontStyle.Regular && (fontStyle & PdfFontStyle.Italic) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 801f;
			pdfFontMetrics.Descent = -250f;
			pdfFontMetrics.PostScriptName = "Courier-BoldOblique";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_fixedWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else if ((fontStyle & PdfFontStyle.Bold) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 801f;
			pdfFontMetrics.Descent = -250f;
			pdfFontMetrics.PostScriptName = "Courier-Bold";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_fixedWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else if ((fontStyle & PdfFontStyle.Italic) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 805f;
			pdfFontMetrics.Descent = -250f;
			pdfFontMetrics.PostScriptName = "Courier-Oblique";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_fixedWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else
		{
			pdfFontMetrics.Ascent = 805f;
			pdfFontMetrics.Descent = -250f;
			pdfFontMetrics.PostScriptName = "Courier";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_fixedWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetTimesMetrics(PdfFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		if ((fontStyle & PdfFontStyle.Bold) > PdfFontStyle.Regular && (fontStyle & PdfFontStyle.Italic) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 921f;
			pdfFontMetrics.Descent = -218f;
			pdfFontMetrics.PostScriptName = "Times-BoldItalic";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_timesRomanBoldItalicWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else if ((fontStyle & PdfFontStyle.Bold) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 935f;
			pdfFontMetrics.Descent = -218f;
			pdfFontMetrics.PostScriptName = "Times-Bold";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_timesRomanBoldWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else if ((fontStyle & PdfFontStyle.Italic) > PdfFontStyle.Regular)
		{
			pdfFontMetrics.Ascent = 883f;
			pdfFontMetrics.Descent = -217f;
			pdfFontMetrics.PostScriptName = "Times-Italic";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_timesRomanItalicWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		else
		{
			pdfFontMetrics.Ascent = 898f;
			pdfFontMetrics.Descent = -218f;
			pdfFontMetrics.PostScriptName = "Times-Roman";
			pdfFontMetrics.Size = size;
			pdfFontMetrics.WidthTable = new StandardWidthTable(c_timesRomanWidth);
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetSymbolMetrics(PdfFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		pdfFontMetrics.Ascent = 1010f;
		pdfFontMetrics.Descent = -293f;
		pdfFontMetrics.PostScriptName = "Symbol";
		pdfFontMetrics.Size = size;
		pdfFontMetrics.WidthTable = new StandardWidthTable(c_symbolWidth);
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetZapfDingbatsMetrics(PdfFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		pdfFontMetrics.Ascent = 820f;
		pdfFontMetrics.Descent = -143f;
		pdfFontMetrics.PostScriptName = "ZapfDingbats";
		pdfFontMetrics.Size = size;
		pdfFontMetrics.WidthTable = new StandardWidthTable(c_zapfDingbatsWidth);
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		return pdfFontMetrics;
	}
}
