using DocGen.Drawing;

namespace DocGen.Chart.Drawing;

internal sealed class ControlPaintExtension
{
	public static Color DarkDark(Color baseColor)
	{
		return Darker(baseColor, 1f);
	}

	public static Color Light(Color baseColor)
	{
		return Lighter(baseColor, 0.5f);
	}

	public static Color LightDarkColor(Color color, float percentage)
	{
		float num = (int)color.R;
		float num2 = (int)color.G;
		float num3 = (int)color.B;
		if (percentage < 0f)
		{
			percentage = 1f + percentage;
			num *= percentage;
			num2 *= percentage;
			num3 *= percentage;
		}
		else
		{
			num = (255f - num) * percentage + num;
			num2 = (255f - num2) * percentage + num2;
			num3 = (255f - num3) * percentage + num3;
		}
		return Color.FromArgb(color.A, (int)num, (int)num2, (int)num3);
	}

	public static Color Darker(Color color, float darkerPercentage)
	{
		return LightDarkColor(color, 0f - darkerPercentage);
	}

	public static Color Lighter(Color color, float lighterPercentage)
	{
		return LightDarkColor(color, lighterPercentage);
	}
}
