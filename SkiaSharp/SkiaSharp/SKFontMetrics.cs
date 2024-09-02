using System;

namespace SkiaSharp;

public struct SKFontMetrics : IEquatable<SKFontMetrics>
{
	private const uint flagsUnderlineThicknessIsValid = 1u;

	private const uint flagsUnderlinePositionIsValid = 2u;

	private const uint flagsStrikeoutThicknessIsValid = 4u;

	private const uint flagsStrikeoutPositionIsValid = 8u;

	private uint fFlags;

	private float fTop;

	private float fAscent;

	private float fDescent;

	private float fBottom;

	private float fLeading;

	private float fAvgCharWidth;

	private float fMaxCharWidth;

	private float fXMin;

	private float fXMax;

	private float fXHeight;

	private float fCapHeight;

	private float fUnderlineThickness;

	private float fUnderlinePosition;

	private float fStrikeoutThickness;

	private float fStrikeoutPosition;

	public readonly float Top => fTop;

	public readonly float Ascent => fAscent;

	public readonly float Descent => fDescent;

	public readonly float Bottom => fBottom;

	public readonly float Leading => fLeading;

	public readonly float AverageCharacterWidth => fAvgCharWidth;

	public readonly float MaxCharacterWidth => fMaxCharWidth;

	public readonly float XMin => fXMin;

	public readonly float XMax => fXMax;

	public readonly float XHeight => fXHeight;

	public readonly float CapHeight => fCapHeight;

	public readonly float? UnderlineThickness => GetIfValid(fUnderlineThickness, 1u);

	public readonly float? UnderlinePosition => GetIfValid(fUnderlinePosition, 2u);

	public readonly float? StrikeoutThickness => GetIfValid(fStrikeoutThickness, 4u);

	public readonly float? StrikeoutPosition => GetIfValid(fStrikeoutPosition, 8u);

	private readonly float? GetIfValid(float value, uint flag)
	{
		if ((fFlags & flag) != flag)
		{
			return null;
		}
		return value;
	}

	public readonly bool Equals(SKFontMetrics obj)
	{
		if (fFlags == obj.fFlags && fTop == obj.fTop && fAscent == obj.fAscent && fDescent == obj.fDescent && fBottom == obj.fBottom && fLeading == obj.fLeading && fAvgCharWidth == obj.fAvgCharWidth && fMaxCharWidth == obj.fMaxCharWidth && fXMin == obj.fXMin && fXMax == obj.fXMax && fXHeight == obj.fXHeight && fCapHeight == obj.fCapHeight && fUnderlineThickness == obj.fUnderlineThickness && fUnderlinePosition == obj.fUnderlinePosition && fStrikeoutThickness == obj.fStrikeoutThickness)
		{
			return fStrikeoutPosition == obj.fStrikeoutPosition;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKFontMetrics obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKFontMetrics left, SKFontMetrics right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKFontMetrics left, SKFontMetrics right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fFlags);
		hashCode.Add(fTop);
		hashCode.Add(fAscent);
		hashCode.Add(fDescent);
		hashCode.Add(fBottom);
		hashCode.Add(fLeading);
		hashCode.Add(fAvgCharWidth);
		hashCode.Add(fMaxCharWidth);
		hashCode.Add(fXMin);
		hashCode.Add(fXMax);
		hashCode.Add(fXHeight);
		hashCode.Add(fCapHeight);
		hashCode.Add(fUnderlineThickness);
		hashCode.Add(fUnderlinePosition);
		hashCode.Add(fStrikeoutThickness);
		hashCode.Add(fStrikeoutPosition);
		return hashCode.ToHashCode();
	}
}
