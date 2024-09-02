using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart.Drawing;

internal sealed class ColorConvert
{
	public static Color ColorFromString(string parseStr)
	{
		object obj = TypeDescriptor.GetConverter(typeof(Color)).ConvertFrom(parseStr);
		if (obj != null && obj is Color)
		{
			return (Color)obj;
		}
		return Color.Empty;
	}

	public static string ColorToString(Color color, bool writeName)
	{
		object obj = TypeDescriptor.GetConverter(typeof(Color)).ConvertTo(color, typeof(string));
		if (obj != null && obj is string)
		{
			return (string)obj;
		}
		return "";
	}
}
