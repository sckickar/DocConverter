using System;
using System.Drawing;
using System.Globalization;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal abstract class Colorspace
{
	private PdfBrush m_defaultBrush = new PdfSolidBrush(DocGen.Drawing.Color.Black);

	internal abstract int Components { get; }

	internal virtual PdfBrush DefaultBrush => m_defaultBrush;

	internal abstract DocGen.Drawing.Color GetColor(string[] pars);

	internal abstract DocGen.Drawing.Color GetColor(byte[] bytes, int offset);

	internal virtual void SetOperatorValues(bool IsRectangle, bool IsCircle, string RectangleWidth)
	{
	}

	internal abstract PdfBrush GetBrush(string[] pars, PdfPageResources resource);

	internal static bool IsColorSpace(string name)
	{
		name = GetColorSpaceName(name);
		switch (name)
		{
		case "DeviceCMYK":
		case "DeviceGray":
		case "Separation":
		case "Indexed":
		case "DeviceN":
		case "Pattern":
		case "CalCMYK":
		case "CalGray":
		case "DeviceRGB":
		case "CalRGB":
		case "ICCBased":
			return true;
		default:
			return false;
		}
	}

	private static string GetColorSpaceName(string name)
	{
		return name switch
		{
			"G" => "DeviceGray", 
			"RGB" => "DeviceRGB", 
			"CMYK" => "DeviceCMYK", 
			"I" => "Indexed", 
			_ => name, 
		};
	}

	internal static Colorspace CreateColorSpace(string name)
	{
		name = GetColorSpaceName(name);
		return name switch
		{
			"DeviceGray" => new DeviceGray(), 
			"DeviceRGB" => new DeviceRGB(), 
			"DeviceCMYK" => new DeviceCMYK(), 
			"ICCBased" => new ICCBased(), 
			"CalRGB" => new DeviceRGB(), 
			"CalGray" => new CalGray(), 
			"Lab" => new LabColor(), 
			"Indexed" => new Indexed(), 
			"Separation" => new Separation(), 
			"DeviceN" => new DeviceN(), 
			"Pattern" => new Pattern(), 
			"Shading" => new ShadingPattern(), 
			_ => throw new NotSupportedException("Color space is not supported."), 
		};
	}

	internal static Colorspace CreateColorSpace(string value, IPdfPrimitive array)
	{
		Colorspace colorspace = CreateColorSpace(value);
		if (colorspace is CalRGB calRGB)
		{
			calRGB.SetValue(array as PdfArray);
			return calRGB;
		}
		if (colorspace is CalGray calGray)
		{
			calGray.SetValue(array as PdfArray);
			return calGray;
		}
		if (colorspace is LabColor labColor)
		{
			labColor.SetValue(array as PdfArray);
			return labColor;
		}
		if (colorspace is ICCBased iCCBased)
		{
			iCCBased.Profile = new ICCProfile(array as PdfArray);
			return iCCBased;
		}
		if (colorspace is Indexed indexed)
		{
			indexed.SetValue(array as PdfArray);
			return indexed;
		}
		if (colorspace is Separation separation)
		{
			separation.SetValue(array as PdfArray);
			return separation;
		}
		if (colorspace is DeviceN deviceN)
		{
			deviceN.SetValue(array as PdfArray);
			return deviceN;
		}
		return colorspace;
	}

	protected static DocGen.Drawing.Color GetRgbColor(string[] pars)
	{
		float.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
		float.TryParse(pars[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2);
		float.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var result3);
		return DocGen.Drawing.Color.FromArgb(255, (byte)(result * 255f), (byte)(result2 * 255f), (byte)(result3 * 255f));
	}

	protected static DocGen.Drawing.Color GetRgbColor(float[] values)
	{
		return DocGen.Drawing.Color.FromArgb(255, (byte)(values[0] * 255f), (byte)(values[1] * 255f), (byte)(values[2] * 255f));
	}

	protected static DocGen.Drawing.Color GetRgbColor(double[] values)
	{
		return DocGen.Drawing.Color.FromArgb(255, (byte)(values[0] * 255.0), (byte)(values[1] * 255.0), (byte)(values[2] * 255.0));
	}

	protected static DocGen.Drawing.Color GetCmykColor(string[] pars)
	{
		float.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
		float.TryParse(pars[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2);
		float.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var result3);
		float.TryParse(pars[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var result4);
		return ConvertCMYKtoRGB(result, result2, result3, result4);
	}

	protected static DocGen.Drawing.Color GetGrayColor(string[] pars)
	{
		float.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
		return DocGen.Drawing.Color.FromArgb(255, (byte)(result * 255f), (byte)(result * 255f), (byte)(result * 255f));
	}

	private static DocGen.Drawing.Color ConvertCMYKtoRGB(float c, float m, float y, float k)
	{
		float num = (float)((double)c * (-4.387332384609988 * (double)c + 54.48615194189176 * (double)m + 18.82290502165302 * (double)y + 212.25662451639585 * (double)k + -285.2331026137004) + (double)m * (1.7149763477362134 * (double)m - 5.6096736904047315 * (double)y + -17.873870861415444 * (double)k - 5.497006427196366) + (double)y * (-2.5217340131683033 * (double)y - 21.248923337353073 * (double)k + 17.5119270841813) + (double)k * (-21.86122147463605 * (double)k - 189.48180835922747) + 255.0);
		float num2 = (float)((double)c * (8.841041422036149 * (double)c + 60.118027045597366 * (double)m + 6.871425592049007 * (double)y + 31.159100130055922 * (double)k + -79.2970844816548) + (double)m * (-15.310361306967817 * (double)m + 17.575251261109482 * (double)y + 131.35250912493976 * (double)k - 190.9453302588951) + (double)y * (4.444339102852739 * (double)y + 9.8632861493405 * (double)k - 24.86741582555878) + (double)k * (-20.737325471181034 * (double)k - 187.80453709719578) + 255.0);
		float num3 = (float)((double)c * (0.8842522430003296 * (double)c + 8.078677503112928 * (double)m + 30.89978309703729 * (double)y - 0.23883238689178934 * (double)k + -14.183576799673286) + (double)m * (10.49593273432072 * (double)m + 63.02378494754052 * (double)y + 50.606957656360734 * (double)k - 112.23884253719248) + (double)y * (0.03296041114873217 * (double)y + 115.60384449646641 * (double)k + -193.58209356861505) + (double)k * (-22.33816807309886 * (double)k - 180.12613974708367) + 255.0);
		return DocGen.Drawing.Color.FromArgb(255, (int)((num > 255f) ? 255f : ((num < 0f) ? 0f : num)), (int)((num2 > 255f) ? 255f : ((num2 < 0f) ? 0f : num2)), (int)((num3 > 255f) ? 255f : ((num3 < 0f) ? 0f : num3)));
	}

	protected static DocGen.Drawing.Color GetRgbColor(byte[] bytes, int offset)
	{
		return DocGen.Drawing.Color.FromArgb(255, bytes[offset], bytes[offset + 1], bytes[offset + 2]);
	}

	protected static DocGen.Drawing.Color GetGrayColor(byte[] bytes, int offset)
	{
		return DocGen.Drawing.Color.FromArgb(255, bytes[offset] * 255, bytes[offset] * 255, bytes[offset] * 255);
	}

	protected static DocGen.Drawing.Color GetCmykColor(byte[] bytes, int offset)
	{
		return DocGen.Drawing.Color.Empty;
	}

	protected static System.Drawing.Color GetCmykColorHelper(string[] pars)
	{
		float.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
		float.TryParse(pars[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2);
		float.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var result3);
		float.TryParse(pars[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var result4);
		return ConvertCMYKtoRGBHelper(result, result2, result3, result4);
	}

	private static System.Drawing.Color ConvertCMYKtoRGBHelper(float c, float m, float y, float k)
	{
		float num = (float)((double)c * (-4.387332384609988 * (double)c + 54.48615194189176 * (double)m + 18.82290502165302 * (double)y + 212.25662451639585 * (double)k + -285.2331026137004) + (double)m * (1.7149763477362134 * (double)m - 5.6096736904047315 * (double)y + -17.873870861415444 * (double)k - 5.497006427196366) + (double)y * (-2.5217340131683033 * (double)y - 21.248923337353073 * (double)k + 17.5119270841813) + (double)k * (-21.86122147463605 * (double)k - 189.48180835922747) + 255.0);
		float num2 = (float)((double)c * (8.841041422036149 * (double)c + 60.118027045597366 * (double)m + 6.871425592049007 * (double)y + 31.159100130055922 * (double)k + -79.2970844816548) + (double)m * (-15.310361306967817 * (double)m + 17.575251261109482 * (double)y + 131.35250912493976 * (double)k - 190.9453302588951) + (double)y * (4.444339102852739 * (double)y + 9.8632861493405 * (double)k - 24.86741582555878) + (double)k * (-20.737325471181034 * (double)k - 187.80453709719578) + 255.0);
		float num3 = (float)((double)c * (0.8842522430003296 * (double)c + 8.078677503112928 * (double)m + 30.89978309703729 * (double)y - 0.23883238689178934 * (double)k + -14.183576799673286) + (double)m * (10.49593273432072 * (double)m + 63.02378494754052 * (double)y + 50.606957656360734 * (double)k - 112.23884253719248) + (double)y * (0.03296041114873217 * (double)y + 115.60384449646641 * (double)k + -193.58209356861505) + (double)k * (-22.33816807309886 * (double)k - 180.12613974708367) + 255.0);
		return System.Drawing.Color.FromArgb(255, (int)((num > 255f) ? 255f : ((num < 0f) ? 0f : num)), (int)((num2 > 255f) ? 255f : ((num2 < 0f) ? 0f : num2)), (int)((num3 > 255f) ? 255f : ((num3 < 0f) ? 0f : num3)));
	}

	protected static System.Drawing.Color GetCmykColorHelper(byte[] bytes, int offset)
	{
		return System.Drawing.Color.Empty;
	}

	protected static System.Drawing.Color GetRgbColorHelper(double[] values)
	{
		return System.Drawing.Color.FromArgb(255, (byte)(values[0] * 255.0), (byte)(values[1] * 255.0), (byte)(values[2] * 255.0));
	}

	protected static System.Drawing.Color GetGrayColorHelper(byte[] bytes, int offset)
	{
		return System.Drawing.Color.FromArgb(255, bytes[offset] * 255, bytes[offset] * 255, bytes[offset] * 255);
	}

	protected static System.Drawing.Color GetRgbColorHelper(string[] pars)
	{
		float.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
		float.TryParse(pars[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2);
		float.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var result3);
		return System.Drawing.Color.FromArgb(255, (byte)(result * 255f), (byte)(result2 * 255f), (byte)(result3 * 255f));
	}

	protected static System.Drawing.Color GetRgbColorHelper(byte[] bytes, int offset)
	{
		return System.Drawing.Color.FromArgb(255, bytes[offset], bytes[offset + 1], bytes[offset + 2]);
	}

	protected static System.Drawing.Color GetGrayColorHelper(string[] pars)
	{
		float.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
		return System.Drawing.Color.FromArgb(255, (byte)(result * 255f), (byte)(result * 255f), (byte)(result * 255f));
	}

	internal string[] ToParams(double[] values)
	{
		string[] array = new string[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			array[i] = values[i].ToString();
		}
		return array;
	}
}
