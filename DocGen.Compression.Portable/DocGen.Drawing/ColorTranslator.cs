using System;

namespace DocGen.Drawing;

public sealed class ColorTranslator
{
	private const int Win32RedShift = 0;

	private const int Win32GreenShift = 8;

	private const int Win32BlueShift = 16;

	private ColorTranslator()
	{
	}

	public static int ToWin32(Color c)
	{
		return c.R | (c.G << 8) | (c.B << 16);
	}

	public static int ToOle(Color c)
	{
		if (c.IsKnownColor)
		{
			switch (c.ToKnownColor())
			{
			case KnownColor.ActiveBorder:
				return -2147483638;
			case KnownColor.ActiveCaption:
				return -2147483646;
			case KnownColor.ActiveCaptionText:
				return -2147483639;
			case KnownColor.AppWorkspace:
				return -2147483636;
			case KnownColor.ButtonFace:
				return -2147483633;
			case KnownColor.ButtonHighlight:
				return -2147483628;
			case KnownColor.ButtonShadow:
				return -2147483632;
			case KnownColor.Control:
				return -2147483633;
			case KnownColor.ControlDark:
				return -2147483632;
			case KnownColor.ControlDarkDark:
				return -2147483627;
			case KnownColor.ControlLight:
				return -2147483626;
			case KnownColor.ControlLightLight:
				return -2147483628;
			case KnownColor.ControlText:
				return -2147483630;
			case KnownColor.Desktop:
				return -2147483647;
			case KnownColor.GradientActiveCaption:
				return -2147483621;
			case KnownColor.GradientInactiveCaption:
				return -2147483620;
			case KnownColor.GrayText:
				return -2147483631;
			case KnownColor.Highlight:
				return -2147483635;
			case KnownColor.HighlightText:
				return -2147483634;
			case KnownColor.HotTrack:
				return -2147483635;
			case KnownColor.InactiveBorder:
				return -2147483637;
			case KnownColor.InactiveCaption:
				return -2147483645;
			case KnownColor.InactiveCaptionText:
				return -2147483629;
			case KnownColor.Info:
				return -2147483624;
			case KnownColor.InfoText:
				return -2147483625;
			case KnownColor.Menu:
				return -2147483644;
			case KnownColor.MenuBar:
				return -2147483618;
			case KnownColor.MenuHighlight:
				return -2147483619;
			case KnownColor.MenuText:
				return -2147483641;
			case KnownColor.ScrollBar:
				return int.MinValue;
			case KnownColor.Window:
				return -2147483643;
			case KnownColor.WindowFrame:
				return -2147483642;
			case KnownColor.WindowText:
				return -2147483640;
			}
		}
		return ToWin32(c);
	}

	public static Color FromOle(int oleColor)
	{
		if ((int)(oleColor & 0xFF000000u) == int.MinValue && (oleColor & 0xFFFFFF) <= 24)
		{
			switch (oleColor)
			{
			case -2147483638:
				return ColorConverter.FromKnownColor(KnownColor.ActiveBorder);
			case -2147483646:
				return ColorConverter.FromKnownColor(KnownColor.ActiveCaption);
			case -2147483639:
				return ColorConverter.FromKnownColor(KnownColor.ActiveCaptionText);
			case -2147483636:
				return ColorConverter.FromKnownColor(KnownColor.AppWorkspace);
			case -2147483633:
				return ColorConverter.FromKnownColor(KnownColor.Control);
			case -2147483632:
				return ColorConverter.FromKnownColor(KnownColor.ControlDark);
			case -2147483627:
				return ColorConverter.FromKnownColor(KnownColor.ControlDarkDark);
			case -2147483626:
				return ColorConverter.FromKnownColor(KnownColor.ControlLight);
			case -2147483628:
				return ColorConverter.FromKnownColor(KnownColor.ControlLightLight);
			case -2147483630:
				return ColorConverter.FromKnownColor(KnownColor.ControlText);
			case -2147483647:
				return ColorConverter.FromKnownColor(KnownColor.Desktop);
			case -2147483621:
				return ColorConverter.FromKnownColor(KnownColor.GradientActiveCaption);
			case -2147483620:
				return ColorConverter.FromKnownColor(KnownColor.GradientInactiveCaption);
			case -2147483631:
				return ColorConverter.FromKnownColor(KnownColor.GrayText);
			case -2147483635:
				return ColorConverter.FromKnownColor(KnownColor.Highlight);
			case -2147483634:
				return ColorConverter.FromKnownColor(KnownColor.HighlightText);
			case -2147483637:
				return ColorConverter.FromKnownColor(KnownColor.InactiveBorder);
			case -2147483645:
				return ColorConverter.FromKnownColor(KnownColor.InactiveCaption);
			case -2147483629:
				return ColorConverter.FromKnownColor(KnownColor.InactiveCaptionText);
			case -2147483624:
				return ColorConverter.FromKnownColor(KnownColor.Info);
			case -2147483625:
				return ColorConverter.FromKnownColor(KnownColor.InfoText);
			case -2147483644:
				return ColorConverter.FromKnownColor(KnownColor.Menu);
			case -2147483618:
				return ColorConverter.FromKnownColor(KnownColor.MenuBar);
			case -2147483619:
				return ColorConverter.FromKnownColor(KnownColor.MenuHighlight);
			case -2147483641:
				return ColorConverter.FromKnownColor(KnownColor.MenuText);
			case int.MinValue:
				return ColorConverter.FromKnownColor(KnownColor.ScrollBar);
			case -2147483643:
				return ColorConverter.FromKnownColor(KnownColor.Window);
			case -2147483642:
				return ColorConverter.FromKnownColor(KnownColor.WindowFrame);
			case -2147483640:
				return ColorConverter.FromKnownColor(KnownColor.WindowText);
			}
		}
		return KnownColorTable.ArgbToKnownColor(Color.FromArgb((byte)(oleColor & 0xFF), (byte)((oleColor >> 8) & 0xFF), (byte)((oleColor >> 16) & 0xFF)).ToArgb());
	}

	public static Color FromWin32(int win32Color)
	{
		return FromOle(win32Color);
	}

	public static Color FromHtml(string htmlColor)
	{
		Color result = Color.Empty;
		if (htmlColor == null || htmlColor.Length == 0)
		{
			return result;
		}
		if (htmlColor[0] == '#' && (htmlColor.Length == 7 || htmlColor.Length == 4))
		{
			if (htmlColor.Length == 7)
			{
				result = Color.FromArgb(Convert.ToInt32(htmlColor.Substring(1, 2), 16), Convert.ToInt32(htmlColor.Substring(3, 2), 16), Convert.ToInt32(htmlColor.Substring(5, 2), 16));
			}
			else
			{
				string text = char.ToString(htmlColor[1]);
				string text2 = char.ToString(htmlColor[2]);
				string text3 = char.ToString(htmlColor[3]);
				result = Color.FromArgb(Convert.ToInt32(text + text, 16), Convert.ToInt32(text2 + text2, 16), Convert.ToInt32(text3 + text3, 16));
			}
		}
		if (htmlColor[0] == '#' && (htmlColor.Length == 9 || htmlColor.Length == 6))
		{
			if (htmlColor.Length == 9)
			{
				result = Color.FromArgb(Convert.ToInt32(htmlColor.Substring(1, 2), 16), Convert.ToInt32(htmlColor.Substring(3, 2), 16), Convert.ToInt32(htmlColor.Substring(5, 2), 16), Convert.ToInt32(htmlColor.Substring(7, 2), 16));
			}
			else
			{
				string text4 = char.ToString(htmlColor[1]);
				string text5 = char.ToString(htmlColor[2]);
				string text6 = char.ToString(htmlColor[3]);
				string text7 = char.ToString(htmlColor[4]);
				result = Color.FromArgb(Convert.ToInt32(text4 + text4, 16), Convert.ToInt32(text5 + text5, 16), Convert.ToInt32(text6 + text6, 16), Convert.ToInt32(text7 + text7, 16));
			}
		}
		if (result.IsEmpty && string.Equals(htmlColor, "LightGrey", StringComparison.OrdinalIgnoreCase))
		{
			result = Color.LightGray;
		}
		if (result.IsEmpty)
		{
			if (ColorConverter.SystemColors.ContainsKey(htmlColor.ToLower()))
			{
				object obj = ColorConverter.SystemColors[htmlColor.ToLower()];
				if (obj != null)
				{
					result = (Color)obj;
				}
			}
			else if (ColorConverter.Colors.ContainsKey(htmlColor.ToLower()))
			{
				object obj2 = ColorConverter.Colors[htmlColor.ToLower()];
				if (obj2 != null)
				{
					result = (Color)obj2;
				}
			}
		}
		return result;
	}

	public static string ToHtml(Color c)
	{
		string result = string.Empty;
		if (c.IsEmpty)
		{
			return result;
		}
		if (!c.IsSystemColor)
		{
			result = ((!c.IsNamedColor) ? ("#" + c.R.ToString("X2", null) + c.G.ToString("X2", null) + c.B.ToString("X2", null)) : ((!(c == Color.LightGray)) ? c.Name : "LightGrey"));
		}
		else
		{
			switch (c.ToKnownColor())
			{
			case KnownColor.ActiveBorder:
				result = "activeborder";
				break;
			case KnownColor.ActiveCaption:
			case KnownColor.GradientActiveCaption:
				result = "activecaption";
				break;
			case KnownColor.AppWorkspace:
				result = "appworkspace";
				break;
			case KnownColor.Desktop:
				result = "background";
				break;
			case KnownColor.Control:
				result = "buttonface";
				break;
			case KnownColor.ControlLight:
				result = "buttonface";
				break;
			case KnownColor.ControlDark:
				result = "buttonshadow";
				break;
			case KnownColor.ControlText:
				result = "buttontext";
				break;
			case KnownColor.ActiveCaptionText:
				result = "captiontext";
				break;
			case KnownColor.GrayText:
				result = "graytext";
				break;
			case KnownColor.Highlight:
			case KnownColor.HotTrack:
				result = "highlight";
				break;
			case KnownColor.HighlightText:
			case KnownColor.MenuHighlight:
				result = "highlighttext";
				break;
			case KnownColor.InactiveBorder:
				result = "inactiveborder";
				break;
			case KnownColor.InactiveCaption:
			case KnownColor.GradientInactiveCaption:
				result = "inactivecaption";
				break;
			case KnownColor.InactiveCaptionText:
				result = "inactivecaptiontext";
				break;
			case KnownColor.Info:
				result = "infobackground";
				break;
			case KnownColor.InfoText:
				result = "infotext";
				break;
			case KnownColor.Menu:
			case KnownColor.MenuBar:
				result = "menu";
				break;
			case KnownColor.MenuText:
				result = "menutext";
				break;
			case KnownColor.ScrollBar:
				result = "scrollbar";
				break;
			case KnownColor.ControlDarkDark:
				result = "threeddarkshadow";
				break;
			case KnownColor.ControlLightLight:
				result = "buttonhighlight";
				break;
			case KnownColor.Window:
				result = "window";
				break;
			case KnownColor.WindowFrame:
				result = "windowframe";
				break;
			case KnownColor.WindowText:
				result = "windowtext";
				break;
			}
		}
		return result;
	}
}
