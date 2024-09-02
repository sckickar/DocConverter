using System;
using System.ComponentModel;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartColorModel
{
	public const int NumColorsInPalette = 17;

	public const int NumColorsInMetroPalette = 10;

	private const int c_alpha = 150;

	private static Color[] c_defaultColorTable;

	private static Color[] c_skyblueColorTable;

	private static Color[] c_redyellowColorTable;

	private static Color[] c_greenyellowColorTable;

	private static Color[] c_pinkvioletColorTable;

	private static Color[] c_defaultAlphaColorTable;

	private static Color[] c_defaultOldColorTable;

	private static Color[] c_defaultOldAlphaColorTable;

	private static Color[] c_earthTonesColorTable;

	private static Color[] c_analogColorTable;

	private static Color[] c_colorfulColorTable;

	private static Color[] c_natureColorTable;

	private static Color[] c_pastelColorTable;

	private static Color[] c_triadColorTable;

	private static Color[] c_warmColdColorTable;

	private static Color[] c_grayScaleColorTable;

	private static Color[] c_metroColorTable;

	private Color[] m_activePalette;

	private ChartColorPalette m_colorPalette;

	private Color[] m_customColorTable;

	private bool m_allowGradient;

	public Color[] CustomColors
	{
		get
		{
			return m_customColorTable;
		}
		set
		{
			if (m_customColorTable == value)
			{
				return;
			}
			m_customColorTable = value;
			if (m_colorPalette == ChartColorPalette.Custom && m_customColorTable != null)
			{
				if (m_customColorTable.Length != 0)
				{
					m_activePalette = m_customColorTable;
				}
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public ChartColorPalette Palette
	{
		get
		{
			return m_colorPalette;
		}
		set
		{
			if (m_colorPalette != value)
			{
				Color[] palette = GetPalette(value);
				m_colorPalette = value;
				if (palette != null)
				{
					m_activePalette = palette;
					RaiseChanged(this, EventArgs.Empty);
				}
			}
		}
	}

	[DefaultValue(false)]
	public bool AllowGradient
	{
		get
		{
			return m_allowGradient;
		}
		set
		{
			if (m_allowGradient != value)
			{
				m_allowGradient = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public event EventHandler Changed;

	static ChartColorModel()
	{
		InitializePalettes();
	}

	internal ChartColorModel()
	{
		m_colorPalette = ChartColorPalette.Default;
		m_activePalette = GetPalette(m_colorPalette);
	}

	public void Dispose()
	{
		c_analogColorTable = null;
		c_colorfulColorTable = null;
		c_defaultAlphaColorTable = null;
		c_defaultColorTable = null;
		c_defaultOldAlphaColorTable = null;
		c_defaultOldColorTable = null;
		c_earthTonesColorTable = null;
		c_grayScaleColorTable = null;
		c_greenyellowColorTable = null;
		c_metroColorTable = null;
		c_natureColorTable = null;
		c_pastelColorTable = null;
		c_pinkvioletColorTable = null;
		c_redyellowColorTable = null;
		c_skyblueColorTable = null;
		c_triadColorTable = null;
		c_warmColdColorTable = null;
	}

	public Color GetColor(int index)
	{
		if (index >= 0 && m_activePalette != null && m_activePalette.Length != 0)
		{
			return m_activePalette[index % m_activePalette.Length];
		}
		return Color.Empty;
	}

	public DocGen.Drawing.SkiaSharpHelper.Image CreatePaletteIcon(Size sz, ChartColorPalette palette, int colorCount)
	{
		Bitmap bitmap = new Bitmap(sz.Width, sz.Height);
		float num = (float)sz.Width / (float)colorCount;
		Color[] palette2 = GetPalette(palette);
		using Graphics graphics = Graphics.FromImage(bitmap);
		if (palette2 != null && palette2.Length != 0)
		{
			for (int i = 0; i < colorCount; i++)
			{
				using SolidBrush brush = new SolidBrush(palette2[i % palette2.Length]);
				graphics.FillRectangle(brush, (float)i * num, 0f, num, sz.Height);
			}
		}
		graphics.DrawRectangle(Pens.Black, 0f, 0f, sz.Width - 1, sz.Height - 1);
		return bitmap;
	}

	private static void InitializeSkyBlueStyle()
	{
		c_skyblueColorTable = new Color[16]
		{
			ColorTranslator.FromHtml("#2E7472"),
			ColorTranslator.FromHtml("#48C1BD"),
			ColorTranslator.FromHtml("#53BD8C"),
			ColorTranslator.FromHtml("#37855D"),
			ColorTranslator.FromHtml("#003A3A"),
			ColorTranslator.FromHtml("#3E9F95"),
			ColorTranslator.FromHtml("#3CB769"),
			ColorTranslator.FromHtml("#5B7D41"),
			ColorTranslator.FromHtml("#5CC3CA"),
			ColorTranslator.FromHtml("#6891CB"),
			ColorTranslator.FromHtml("#5B5FAB"),
			ColorTranslator.FromHtml("#613A5B"),
			ColorTranslator.FromHtml("#A470AC"),
			ColorTranslator.FromHtml("#180B42"),
			ColorTranslator.FromHtml("#5150A2"),
			ColorTranslator.FromHtml("#965DA2")
		};
	}

	private static void InitializeRedYellowStyle()
	{
		c_redyellowColorTable = new Color[16]
		{
			ColorTranslator.FromHtml("#6E3F98"),
			ColorTranslator.FromHtml("#DF1D3B"),
			ColorTranslator.FromHtml("#205F2F"),
			ColorTranslator.FromHtml("#FCAF17"),
			ColorTranslator.FromHtml("#86328C"),
			ColorTranslator.FromHtml("#FFF200"),
			ColorTranslator.FromHtml("#F4783B"),
			ColorTranslator.FromHtml("#2E3192"),
			ColorTranslator.FromHtml("#00A9A3"),
			ColorTranslator.FromHtml("#A51D35"),
			ColorTranslator.FromHtml("#EC008C"),
			ColorTranslator.FromHtml("#702C8D"),
			ColorTranslator.FromHtml("#00A651"),
			ColorTranslator.FromHtml("#E41E26"),
			ColorTranslator.FromHtml("#0071B5"),
			ColorTranslator.FromHtml("#B2D235")
		};
	}

	private static void InitializeGreenYellowStyle()
	{
		c_greenyellowColorTable = new Color[16]
		{
			ColorTranslator.FromHtml("#231F20"),
			ColorTranslator.FromHtml("#8C1846"),
			ColorTranslator.FromHtml("#EB2E92"),
			ColorTranslator.FromHtml("#592C8A"),
			ColorTranslator.FromHtml("#57C9E8"),
			ColorTranslator.FromHtml("#5894CE"),
			ColorTranslator.FromHtml("#F47820"),
			ColorTranslator.FromHtml("#FFCB05"),
			ColorTranslator.FromHtml("#57803A"),
			ColorTranslator.FromHtml("#CDDC29"),
			ColorTranslator.FromHtml("#76C043"),
			ColorTranslator.FromHtml("#52260F"),
			ColorTranslator.FromHtml("#EE7123"),
			ColorTranslator.FromHtml("#FBF281"),
			ColorTranslator.FromHtml("#B76E11"),
			ColorTranslator.FromHtml("#ED1C24")
		};
	}

	private static void InitializePinkVioletStyle()
	{
		c_pinkvioletColorTable = new Color[16]
		{
			ColorTranslator.FromHtml("#E16131"),
			ColorTranslator.FromHtml("#E8A92A"),
			ColorTranslator.FromHtml("#DEE340"),
			ColorTranslator.FromHtml("#585E2D"),
			ColorTranslator.FromHtml("#ADB03C"),
			ColorTranslator.FromHtml("#DFE791"),
			ColorTranslator.FromHtml("#EBBD22"),
			ColorTranslator.FromHtml("#F5D28D"),
			ColorTranslator.FromHtml("#C06231"),
			ColorTranslator.FromHtml("#C18C3F"),
			ColorTranslator.FromHtml("#5F411D"),
			ColorTranslator.FromHtml("#D4CB82"),
			ColorTranslator.FromHtml("#7D6F2F"),
			ColorTranslator.FromHtml("#AE9735"),
			ColorTranslator.FromHtml("#E1D055"),
			ColorTranslator.FromHtml("#B86A2F")
		};
	}

	private static void InitializeDefaultPalette()
	{
		c_defaultColorTable = new Color[16]
		{
			Color.FromArgb(237, 139, 76),
			Color.FromArgb(43, 63, 122),
			Color.FromArgb(183, 69, 77),
			Color.FromArgb(245, 76, 78),
			Color.FromArgb(173, 209, 63),
			Color.FromArgb(106, 106, 160),
			Color.FromArgb(167, 185, 205),
			Color.FromArgb(136, 149, 52),
			Color.FromArgb(117, 187, 238),
			Color.FromArgb(204, 135, 119),
			Color.FromArgb(161, 100, 55),
			Color.FromArgb(152, 147, 143),
			Color.FromArgb(50, 50, 50),
			Color.FromArgb(128, 0, 0),
			Color.FromArgb(0, 64, 255),
			Color.FromArgb(184, 2, 184)
		};
	}

	private static void InitializeDefaultAlphaPalette()
	{
		c_defaultAlphaColorTable = new Color[17];
		int num = 0;
		Color[] array = c_defaultColorTable;
		foreach (Color baseColor in array)
		{
			c_defaultAlphaColorTable[num++] = Color.FromArgb(150, baseColor);
		}
	}

	private static void InitializeDefaultOldAlphaPalette()
	{
		c_defaultOldAlphaColorTable = new Color[17];
		int num = 0;
		Color[] array = c_defaultOldColorTable;
		foreach (Color baseColor in array)
		{
			c_defaultOldAlphaColorTable[num++] = Color.FromArgb(150, baseColor);
		}
	}

	private static void InitializeDefaultOldPalette()
	{
		c_defaultOldColorTable = new Color[17];
		c_defaultOldColorTable[0] = Color.FromArgb(153, 153, 255);
		c_defaultOldColorTable[1] = Color.FromArgb(153, 51, 102);
		c_defaultOldColorTable[2] = Color.FromArgb(255, 255, 204);
		c_defaultOldColorTable[3] = Color.FromArgb(102, 0, 102);
		c_defaultOldColorTable[4] = Color.FromArgb(204, 255, 255);
		c_defaultOldColorTable[5] = Color.FromArgb(255, 128, 128);
		c_defaultOldColorTable[6] = Color.FromArgb(0, 102, 204);
		c_defaultOldColorTable[7] = Color.FromArgb(204, 204, 255);
		c_defaultOldColorTable[8] = Color.FromArgb(0, 128, 128);
		c_defaultOldColorTable[9] = Color.FromArgb(255, 117, 186);
		c_defaultOldColorTable[10] = Color.FromArgb(255, 255, 153);
		c_defaultOldColorTable[11] = Color.FromArgb(3, 198, 198);
		c_defaultOldColorTable[12] = Color.FromArgb(128, 0, 128);
		c_defaultOldColorTable[13] = Color.FromArgb(128, 0, 0);
		c_defaultOldColorTable[14] = Color.FromArgb(0, 128, 255);
		c_defaultOldColorTable[15] = Color.FromArgb(184, 2, 184);
	}

	private static void InitializeEarthTonePalette()
	{
		c_earthTonesColorTable = new Color[17];
		c_earthTonesColorTable[0] = Color.FromArgb(142, 255, 0, 0);
		c_earthTonesColorTable[1] = Color.FromArgb(142, 0, 255, 0);
		c_earthTonesColorTable[2] = Color.FromArgb(142, 0, 0, 255);
		c_earthTonesColorTable[3] = Color.FromArgb(142, 255, 255, 0);
		c_earthTonesColorTable[4] = Color.FromArgb(142, 0, 255, 255);
		c_earthTonesColorTable[5] = Color.FromArgb(142, 255, 0, 255);
		c_earthTonesColorTable[6] = Color.FromArgb(142, 170, 120, 20);
		c_earthTonesColorTable[7] = Color.FromArgb(70, 255, 0, 0);
		c_earthTonesColorTable[8] = Color.FromArgb(70, 0, 255, 0);
		c_earthTonesColorTable[9] = Color.FromArgb(70, 0, 0, 255);
		c_earthTonesColorTable[10] = Color.FromArgb(70, 255, 255, 0);
		c_earthTonesColorTable[11] = Color.FromArgb(70, 0, 255, 255);
		c_earthTonesColorTable[12] = Color.FromArgb(70, 255, 0, 255);
		c_earthTonesColorTable[13] = Color.FromArgb(70, 170, 120, 20);
		c_earthTonesColorTable[14] = Color.FromArgb(132, 100, 120, 50);
		c_earthTonesColorTable[15] = Color.FromArgb(132, 40, 80, 150);
	}

	private static void InitializeAnalogPalette()
	{
		c_analogColorTable = new Color[17];
		c_analogColorTable[0] = Color.FromArgb(0, 134, 137);
		c_analogColorTable[1] = Color.FromArgb(32, 55, 189);
		c_analogColorTable[2] = Color.FromArgb(47, 166, 208);
		c_analogColorTable[3] = Color.FromArgb(96, 126, 218);
		c_analogColorTable[4] = Color.FromArgb(87, 161, 255);
		c_analogColorTable[5] = Color.FromArgb(82, 255, 254);
		c_analogColorTable[6] = Color.FromArgb(47, 122, 208);
		c_analogColorTable[7] = Color.FromArgb(47, 206, 208);
		c_analogColorTable[8] = Color.FromArgb(96, 157, 218);
		c_analogColorTable[9] = Color.FromArgb(0, 200, 202);
		c_analogColorTable[10] = Color.FromArgb(45, 15, 156);
		c_analogColorTable[11] = Color.FromArgb(56, 227, 228);
		c_analogColorTable[12] = Color.FromArgb(47, 65, 208);
		c_analogColorTable[13] = Color.FromArgb(47, 158, 208);
		c_analogColorTable[14] = Color.FromArgb(145, 187, 230);
		c_analogColorTable[15] = Color.FromArgb(36, 10, 132);
	}

	private static void InitializeColorfulPalette()
	{
		c_colorfulColorTable = new Color[17];
		c_colorfulColorTable[0] = Color.FromArgb(0, 0, 255);
		c_colorfulColorTable[1] = Color.FromArgb(251, 59, 153);
		c_colorfulColorTable[2] = Color.FromArgb(0, 255, 255);
		c_colorfulColorTable[3] = Color.FromArgb(0, 128, 255);
		c_colorfulColorTable[4] = Color.FromArgb(255, 0, 128);
		c_colorfulColorTable[5] = Color.FromArgb(255, 255, 122);
		c_colorfulColorTable[6] = Color.FromArgb(128, 0, 255);
		c_colorfulColorTable[7] = Color.FromArgb(0, 255, 128);
		c_colorfulColorTable[8] = Color.FromArgb(218, 2, 2);
		c_colorfulColorTable[9] = Color.FromArgb(255, 255, 61);
		c_colorfulColorTable[10] = Color.FromArgb(122, 122, 255);
		c_colorfulColorTable[11] = Color.FromArgb(0, 255, 0);
		c_colorfulColorTable[12] = Color.FromArgb(255, 255, 61);
		c_colorfulColorTable[13] = Color.FromArgb(255, 0, 0);
		c_colorfulColorTable[14] = Color.FromArgb(0, 224, 224);
		c_colorfulColorTable[15] = Color.FromArgb(1, 70, 175);
	}

	private static void InitializeNaturePalette()
	{
		c_natureColorTable = new Color[17];
		c_natureColorTable[0] = Color.FromArgb(119, 149, 17);
		c_natureColorTable[1] = Color.FromArgb(119, 17, 119);
		c_natureColorTable[2] = Color.FromArgb(17, 99, 180);
		c_natureColorTable[3] = Color.FromArgb(241, 129, 17);
		c_natureColorTable[4] = Color.FromArgb(241, 223, 17);
		c_natureColorTable[5] = Color.FromArgb(66, 153, 42);
		c_natureColorTable[6] = Color.FromArgb(17, 68, 119);
		c_natureColorTable[7] = Color.FromArgb(119, 17, 17);
		c_natureColorTable[8] = Color.FromArgb(68, 119, 17);
		c_natureColorTable[9] = Color.FromArgb(17, 17, 119);
		c_natureColorTable[10] = Color.FromArgb(119, 17, 68);
		c_natureColorTable[11] = Color.FromArgb(224, 86, 19);
		c_natureColorTable[12] = Color.FromArgb(236, 191, 12);
		c_natureColorTable[13] = Color.FromArgb(95, 172, 18);
		c_natureColorTable[14] = Color.FromArgb(55, 130, 205);
		c_natureColorTable[15] = Color.FromArgb(1, 1, 105);
	}

	private static void InitializePastelPalette()
	{
		c_pastelColorTable = new Color[17];
		c_pastelColorTable[0] = Color.FromArgb(163, 163, 245);
		c_pastelColorTable[1] = Color.FromArgb(163, 245, 163);
		c_pastelColorTable[2] = Color.FromArgb(239, 173, 108);
		c_pastelColorTable[3] = Color.FromArgb(53, 142, 232);
		c_pastelColorTable[4] = Color.FromArgb(53, 67, 232);
		c_pastelColorTable[5] = Color.FromArgb(158, 142, 198);
		c_pastelColorTable[6] = Color.FromArgb(245, 204, 163);
		c_pastelColorTable[7] = Color.FromArgb(163, 245, 245);
		c_pastelColorTable[8] = Color.FromArgb(204, 163, 245);
		c_pastelColorTable[9] = Color.FromArgb(245, 245, 163);
		c_pastelColorTable[10] = Color.FromArgb(163, 245, 204);
		c_pastelColorTable[11] = Color.FromArgb(68, 178, 232);
		c_pastelColorTable[12] = Color.FromArgb(53, 97, 232);
		c_pastelColorTable[13] = Color.FromArgb(245, 245, 163);
		c_pastelColorTable[14] = Color.FromArgb(201, 151, 118);
		c_pastelColorTable[15] = Color.FromArgb(176, 115, 238);
	}

	private static void InitializeTriadPalette()
	{
		c_triadColorTable = new Color[17];
		c_triadColorTable[0] = Color.FromArgb(190, 0, 255);
		c_triadColorTable[1] = Color.FromArgb(0, 255, 190);
		c_triadColorTable[2] = Color.FromArgb(255, 190, 0);
		c_triadColorTable[3] = Color.FromArgb(206, 255, 61);
		c_triadColorTable[4] = Color.FromArgb(155, 122, 255);
		c_triadColorTable[5] = Color.FromArgb(88, 4, 116);
		c_triadColorTable[6] = Color.FromArgb(0, 255, 127);
		c_triadColorTable[7] = Color.FromArgb(255, 127, 0);
		c_triadColorTable[8] = Color.FromArgb(254, 255, 61);
		c_triadColorTable[9] = Color.FromArgb(122, 122, 255);
		c_triadColorTable[10] = Color.FromArgb(0, 0, 102);
		c_triadColorTable[11] = Color.FromArgb(168, 168, 252);
		c_triadColorTable[12] = Color.FromArgb(0, 255, 204);
		c_triadColorTable[13] = Color.FromArgb(255, 204, 0);
		c_triadColorTable[14] = Color.FromArgb(254, 255, 120);
		c_triadColorTable[15] = Color.FromArgb(199, 199, 255);
	}

	private static void InitializeWarmColdPalette()
	{
		c_warmColdColorTable = new Color[17];
		c_warmColdColorTable[0] = Color.FromArgb(7, 89, 166);
		c_warmColdColorTable[1] = Color.FromArgb(255, 225, 0);
		c_warmColdColorTable[2] = Color.FromArgb(255, 72, 0);
		c_warmColdColorTable[3] = Color.FromArgb(255, 213, 61);
		c_warmColdColorTable[4] = Color.FromArgb(122, 151, 255);
		c_warmColdColorTable[5] = Color.FromArgb(35, 4, 116);
		c_warmColdColorTable[6] = Color.FromArgb(255, 141, 0);
		c_warmColdColorTable[7] = Color.FromArgb(255, 9, 0);
		c_warmColdColorTable[8] = Color.FromArgb(255, 163, 61);
		c_warmColdColorTable[9] = Color.FromArgb(122, 184, 255);
		c_warmColdColorTable[10] = Color.FromArgb(0, 47, 102);
		c_warmColdColorTable[11] = Color.FromArgb(168, 207, 252);
		c_warmColdColorTable[12] = Color.FromArgb(255, 234, 0);
		c_warmColdColorTable[13] = Color.FromArgb(255, 86, 0);
		c_warmColdColorTable[14] = Color.FromArgb(255, 192, 120);
		c_warmColdColorTable[15] = Color.FromArgb(199, 225, 255);
	}

	private static void InitializeGrayScalePalette()
	{
		c_grayScaleColorTable = new Color[17];
		c_grayScaleColorTable[0] = Color.FromArgb(204, 204, 204);
		c_grayScaleColorTable[1] = Color.FromArgb(221, 221, 221);
		c_grayScaleColorTable[2] = Color.FromArgb(238, 238, 238);
		c_grayScaleColorTable[3] = Color.FromArgb(255, 255, 255);
		c_grayScaleColorTable[4] = Color.FromArgb(68, 68, 68);
		c_grayScaleColorTable[5] = Color.FromArgb(85, 85, 85);
		c_grayScaleColorTable[6] = Color.FromArgb(102, 102, 102);
		c_grayScaleColorTable[7] = Color.FromArgb(119, 119, 119);
		c_grayScaleColorTable[8] = Color.FromArgb(136, 136, 136);
		c_grayScaleColorTable[9] = Color.FromArgb(153, 153, 153);
		c_grayScaleColorTable[10] = Color.FromArgb(170, 170, 170);
		c_grayScaleColorTable[11] = Color.FromArgb(187, 187, 187);
		c_grayScaleColorTable[12] = Color.FromArgb(0, 0, 0);
		c_grayScaleColorTable[13] = Color.FromArgb(17, 17, 17);
		c_grayScaleColorTable[14] = Color.FromArgb(34, 34, 34);
		c_grayScaleColorTable[15] = Color.FromArgb(51, 51, 51);
	}

	private static void InitializeMetroPalette()
	{
		c_metroColorTable = new Color[10];
		c_metroColorTable[0] = Color.FromArgb(255, 27, 161, 226);
		c_metroColorTable[1] = Color.FromArgb(255, 160, 80, 0);
		c_metroColorTable[2] = Color.FromArgb(255, 51, 153, 51);
		c_metroColorTable[3] = Color.FromArgb(255, 162, 193, 57);
		c_metroColorTable[4] = Color.FromArgb(255, 216, 0, 115);
		c_metroColorTable[5] = Color.FromArgb(255, 240, 150, 9);
		c_metroColorTable[6] = Color.FromArgb(255, 230, 113, 184);
		c_metroColorTable[7] = Color.FromArgb(255, 162, 0, 255);
		c_metroColorTable[8] = Color.FromArgb(255, 229, 20, 0);
		c_metroColorTable[9] = Color.FromArgb(255, 0, 171, 169);
	}

	private static void InitializePalettes()
	{
		InitializeSkyBlueStyle();
		InitializeRedYellowStyle();
		InitializeGreenYellowStyle();
		InitializePinkVioletStyle();
		InitializeDefaultPalette();
		InitializeDefaultAlphaPalette();
		InitializeDefaultOldPalette();
		InitializeDefaultOldAlphaPalette();
		InitializeEarthTonePalette();
		InitializeAnalogPalette();
		InitializeColorfulPalette();
		InitializeNaturePalette();
		InitializePastelPalette();
		InitializeTriadPalette();
		InitializeWarmColdPalette();
		InitializeGrayScalePalette();
		InitializeMetroPalette();
	}

	private Color[] GetPalette(ChartColorPalette palette)
	{
		Color[] result = null;
		switch (palette)
		{
		case ChartColorPalette.Default:
			result = c_defaultColorTable;
			break;
		case ChartColorPalette.DefaultAlpha:
			result = c_defaultAlphaColorTable;
			break;
		case ChartColorPalette.DefaultOld:
			result = c_defaultOldColorTable;
			break;
		case ChartColorPalette.DefaultOldAlpha:
			result = c_defaultOldAlphaColorTable;
			break;
		case ChartColorPalette.EarthTone:
			result = c_earthTonesColorTable;
			break;
		case ChartColorPalette.Analog:
			result = c_analogColorTable;
			break;
		case ChartColorPalette.Colorful:
			result = c_colorfulColorTable;
			break;
		case ChartColorPalette.Nature:
			result = c_natureColorTable;
			break;
		case ChartColorPalette.Pastel:
			result = c_pastelColorTable;
			break;
		case ChartColorPalette.Triad:
			result = c_triadColorTable;
			break;
		case ChartColorPalette.WarmCold:
			result = c_warmColdColorTable;
			break;
		case ChartColorPalette.GrayScale:
			result = c_grayScaleColorTable;
			break;
		case ChartColorPalette.SkyBlueStyle:
			result = c_skyblueColorTable;
			break;
		case ChartColorPalette.RedYellowStyle:
			result = c_redyellowColorTable;
			break;
		case ChartColorPalette.GreenYellowStyle:
			result = c_greenyellowColorTable;
			break;
		case ChartColorPalette.PinkVioletStyle:
			result = c_pinkvioletColorTable;
			break;
		case ChartColorPalette.Custom:
			if (m_customColorTable != null && m_customColorTable.Length != 0)
			{
				result = m_customColorTable;
			}
			break;
		case ChartColorPalette.Metro:
			result = c_metroColorTable;
			break;
		default:
			result = c_defaultOldAlphaColorTable;
			break;
		}
		return result;
	}

	private void RaiseChanged(object sender, EventArgs args)
	{
		if (this.Changed != null)
		{
			this.Changed(sender, args);
		}
	}
}
