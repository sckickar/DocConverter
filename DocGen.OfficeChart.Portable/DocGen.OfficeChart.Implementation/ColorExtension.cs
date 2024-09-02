using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal static class ColorExtension
{
	private static readonly object m_threadLocker = new object();

	private static Dictionary<string, Color> m_dictSystemColors = new Dictionary<string, Color>();

	public static Color Black = Color.FromArgb(255, 0, 0, 0);

	public static Color White = Color.FromArgb(255, 255, 255, 255);

	public static Color Empty = Color.FromArgb(0, 0, 0, 0);

	public static Color Red = Color.FromArgb(255, 255, 0, 0);

	public static Color Blue = Color.FromArgb(255, 0, 0, 255);

	public static Color DarkGray = Color.FromArgb(255, 128, 128, 128);

	public static Color Yellow = Color.FromArgb(255, 255, 255, 0);

	public static Color Cyan = Color.FromArgb(255, 0, 255, 255);

	public static Color Magenta = Color.FromArgb(255, 255, 0, 255);

	public static Color Gray = Color.FromArgb(255, 192, 192, 192);

	public static Color ChartForeground = White;

	public static Color ChartBackground = Black;

	public static Color ChartNeutral = Black;

	internal static void AddColors()
	{
		m_dictSystemColors.Add("activeborder", Color.FromArgb(255, 180, 180, 180));
		m_dictSystemColors.Add("activecaption", Color.FromArgb(255, 153, 180, 209));
		m_dictSystemColors.Add("activecaptiontext", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("appworkspace", Color.FromArgb(255, 171, 171, 171));
		m_dictSystemColors.Add("control", Color.FromArgb(255, 240, 240, 240));
		m_dictSystemColors.Add("controldark", Color.FromArgb(255, 160, 160, 160));
		m_dictSystemColors.Add("controldarkdark", Color.FromArgb(255, 105, 105, 105));
		m_dictSystemColors.Add("controllight", Color.FromArgb(255, 227, 227, 227));
		m_dictSystemColors.Add("controllightlight", Color.FromArgb(255, 255, 255, 255));
		m_dictSystemColors.Add("controltext", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("desktop", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("graytext", Color.FromArgb(255, 109, 109, 109));
		m_dictSystemColors.Add("highlight", Color.FromArgb(255, 51, 153, 255));
		m_dictSystemColors.Add("highlighttext", Color.FromArgb(255, 255, 255, 255));
		m_dictSystemColors.Add("inactiveborder", Color.FromArgb(255, 244, 247, 252));
		m_dictSystemColors.Add("inactivecaption", Color.FromArgb(255, 191, 205, 219));
		m_dictSystemColors.Add("inactivecaptiontext", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("info", Color.FromArgb(255, 255, 255, 225));
		m_dictSystemColors.Add("infotext", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("menu", Color.FromArgb(255, 240, 240, 240));
		m_dictSystemColors.Add("menutext", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("scrollbar", Color.FromArgb(255, 200, 200, 200));
		m_dictSystemColors.Add("window", Color.FromArgb(255, 255, 255, 255));
		m_dictSystemColors.Add("windowframe", Color.FromArgb(255, 100, 100, 100));
		m_dictSystemColors.Add("windowtext", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("black", Color.FromArgb(255, 0, 0, 0));
		m_dictSystemColors.Add("white", Color.FromArgb(255, 255, 255, 255));
	}

	public static int ToArgb(this Color color)
	{
		return (color.A << 24) + (color.R << 16) + (color.G << 8) + color.B;
	}

	public static Color FromArgb(int value)
	{
		byte blue = (byte)value;
		value >>= 8;
		byte green = (byte)value;
		value >>= 8;
		byte red = (byte)value;
		value >>= 8;
		return Color.FromArgb((byte)value, red, green, blue);
	}

	public static Color FromName(string name)
	{
		lock (m_threadLocker)
		{
			if (m_dictSystemColors.Count == 0)
			{
				AddColors();
			}
		}
		return m_dictSystemColors[name.ToLower()];
	}
}
