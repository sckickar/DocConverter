using System;

namespace SkiaSharp;

public readonly struct SKColorF : IEquatable<SKColorF>
{
	private const float EPSILON = 0.001f;

	public static readonly SKColorF Empty;

	private readonly float fR;

	private readonly float fG;

	private readonly float fB;

	private readonly float fA;

	public float Hue
	{
		get
		{
			ToHsv(out var h, out var _, out var _);
			return h;
		}
	}

	public float Red => fR;

	public float Green => fG;

	public float Blue => fB;

	public float Alpha => fA;

	public SKColorF(float red, float green, float blue)
	{
		fR = red;
		fG = green;
		fB = blue;
		fA = 1f;
	}

	public SKColorF(float red, float green, float blue, float alpha)
	{
		fR = red;
		fG = green;
		fB = blue;
		fA = alpha;
	}

	public SKColorF WithRed(float red)
	{
		return new SKColorF(red, fG, fB, fA);
	}

	public SKColorF WithGreen(float green)
	{
		return new SKColorF(fR, green, fB, fA);
	}

	public SKColorF WithBlue(float blue)
	{
		return new SKColorF(fR, fG, blue, fA);
	}

	public SKColorF WithAlpha(float alpha)
	{
		return new SKColorF(fR, fG, fB, alpha);
	}

	public SKColorF Clamp()
	{
		return new SKColorF(Clamp(fR), Clamp(fG), Clamp(fB), Clamp(fA));
		static float Clamp(float v)
		{
			if (v > 1f)
			{
				return 1f;
			}
			if (v < 0f)
			{
				return 0f;
			}
			return v;
		}
	}

	public static SKColorF FromHsl(float h, float s, float l, float a = 1f)
	{
		h /= 360f;
		s /= 100f;
		l /= 100f;
		float red = l;
		float green = l;
		float blue = l;
		if (Math.Abs(s) > 0.001f)
		{
			float num = ((!(l < 0.5f)) ? (l + s - s * l) : (l * (1f + s)));
			float v = 2f * l - num;
			red = HueToRgb(v, num, h + 1f / 3f);
			green = HueToRgb(v, num, h);
			blue = HueToRgb(v, num, h - 1f / 3f);
		}
		return new SKColorF(red, green, blue, a);
	}

	private static float HueToRgb(float v1, float v2, float vH)
	{
		if (vH < 0f)
		{
			vH += 1f;
		}
		if (vH > 1f)
		{
			vH -= 1f;
		}
		if (6f * vH < 1f)
		{
			return v1 + (v2 - v1) * 6f * vH;
		}
		if (2f * vH < 1f)
		{
			return v2;
		}
		if (3f * vH < 2f)
		{
			return v1 + (v2 - v1) * (2f / 3f - vH) * 6f;
		}
		return v1;
	}

	public static SKColorF FromHsv(float h, float s, float v, float a = 1f)
	{
		h /= 360f;
		s /= 100f;
		v /= 100f;
		float red = v;
		float green = v;
		float blue = v;
		if (Math.Abs(s) > 0.001f)
		{
			h *= 6f;
			if (Math.Abs(h - 6f) < 0.001f)
			{
				h = 0f;
			}
			int num = (int)h;
			float num2 = v * (1f - s);
			float num3 = v * (1f - s * (h - (float)num));
			float num4 = v * (1f - s * (1f - (h - (float)num)));
			switch (num)
			{
			case 0:
				red = v;
				green = num4;
				blue = num2;
				break;
			case 1:
				red = num3;
				green = v;
				blue = num2;
				break;
			case 2:
				red = num2;
				green = v;
				blue = num4;
				break;
			case 3:
				red = num2;
				green = num3;
				blue = v;
				break;
			case 4:
				red = num4;
				green = num2;
				blue = v;
				break;
			default:
				red = v;
				green = num2;
				blue = num3;
				break;
			}
		}
		return new SKColorF(red, green, blue, a);
	}

	public void ToHsl(out float h, out float s, out float l)
	{
		float num = fR;
		float num2 = fG;
		float num3 = fB;
		float num4 = Math.Min(Math.Min(num, num2), num3);
		float num5 = Math.Max(Math.Max(num, num2), num3);
		float num6 = num5 - num4;
		h = 0f;
		s = 0f;
		l = (num5 + num4) / 2f;
		if (Math.Abs(num6) > 0.001f)
		{
			if (l < 0.5f)
			{
				s = num6 / (num5 + num4);
			}
			else
			{
				s = num6 / (2f - num5 - num4);
			}
			float num7 = ((num5 - num) / 6f + num6 / 2f) / num6;
			float num8 = ((num5 - num2) / 6f + num6 / 2f) / num6;
			float num9 = ((num5 - num3) / 6f + num6 / 2f) / num6;
			if (Math.Abs(num - num5) < 0.001f)
			{
				h = num9 - num8;
			}
			else if (Math.Abs(num2 - num5) < 0.001f)
			{
				h = 1f / 3f + num7 - num9;
			}
			else
			{
				h = 2f / 3f + num8 - num7;
			}
			if (h < 0f)
			{
				h += 1f;
			}
			if (h > 1f)
			{
				h -= 1f;
			}
		}
		h *= 360f;
		s *= 100f;
		l *= 100f;
	}

	public void ToHsv(out float h, out float s, out float v)
	{
		float num = fR;
		float num2 = fG;
		float num3 = fB;
		float num4 = Math.Min(Math.Min(num, num2), num3);
		float num5 = Math.Max(Math.Max(num, num2), num3);
		float num6 = num5 - num4;
		h = 0f;
		s = 0f;
		v = num5;
		if (Math.Abs(num6) > 0.001f)
		{
			s = num6 / num5;
			float num7 = ((num5 - num) / 6f + num6 / 2f) / num6;
			float num8 = ((num5 - num2) / 6f + num6 / 2f) / num6;
			float num9 = ((num5 - num3) / 6f + num6 / 2f) / num6;
			if (Math.Abs(num - num5) < 0.001f)
			{
				h = num9 - num8;
			}
			else if (Math.Abs(num2 - num5) < 0.001f)
			{
				h = 1f / 3f + num7 - num9;
			}
			else
			{
				h = 2f / 3f + num8 - num7;
			}
			if (h < 0f)
			{
				h += 1f;
			}
			if (h > 1f)
			{
				h -= 1f;
			}
		}
		h *= 360f;
		s *= 100f;
		v *= 100f;
	}

	public override string ToString()
	{
		return ((SKColor)this).ToString();
	}

	public unsafe static implicit operator SKColorF(SKColor color)
	{
		SKColorF result = default(SKColorF);
		SkiaApi.sk_color4f_from_color((uint)color, &result);
		return result;
	}

	public unsafe static explicit operator SKColor(SKColorF color)
	{
		return SkiaApi.sk_color4f_to_color(&color);
	}

	public bool Equals(SKColorF obj)
	{
		if (fR == obj.fR && fG == obj.fG && fB == obj.fB)
		{
			return fA == obj.fA;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SKColorF obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKColorF left, SKColorF right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKColorF left, SKColorF right)
	{
		return !left.Equals(right);
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fR);
		hashCode.Add(fG);
		hashCode.Add(fB);
		hashCode.Add(fA);
		return hashCode.ToHashCode();
	}
}
