using System;

namespace DocGen.Pdf.Security;

internal abstract class EllipticPoint
{
	internal readonly EllipticCurves curve;

	internal readonly EllipticCurveElements pointX;

	internal readonly EllipticCurveElements pointY;

	internal readonly bool isCompress;

	internal EllipticMultiplier multiplier;

	internal EllipticComp preInfo;

	internal EllipticCurves Curve => curve;

	internal EllipticCurveElements PointX => pointX;

	internal EllipticCurveElements PointY => pointY;

	internal bool IsInfinity
	{
		get
		{
			if (pointX == null)
			{
				return pointY == null;
			}
			return false;
		}
	}

	internal bool IsCompressed => isCompress;

	protected internal EllipticPoint(EllipticCurves curve, EllipticCurveElements pointX, EllipticCurveElements pointY, bool isCompress)
	{
		if (curve == null)
		{
			throw new ArgumentNullException("curve");
		}
		this.curve = curve;
		this.pointX = pointX;
		this.pointY = pointY;
		this.isCompress = isCompress;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is EllipticPoint ellipticPoint))
		{
			return false;
		}
		if (IsInfinity)
		{
			return ellipticPoint.IsInfinity;
		}
		if (pointX.Equals(ellipticPoint.pointX))
		{
			return pointY.Equals(ellipticPoint.pointY);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (IsInfinity)
		{
			return 0;
		}
		return pointX.GetHashCode() ^ pointY.GetHashCode();
	}

	internal void SetInfo(EllipticComp preInfo)
	{
		this.preInfo = preInfo;
	}

	internal virtual byte[] Encoded()
	{
		return Encoded(isCompress);
	}

	internal abstract byte[] Encoded(bool compressed);

	internal abstract EllipticPoint SumValue(EllipticPoint value);

	internal abstract EllipticPoint Subtract(EllipticPoint value);

	internal abstract EllipticPoint Negate();

	internal abstract EllipticPoint Twice();

	internal abstract EllipticPoint Multiply(Number value);

	internal virtual void CheckMultiplier()
	{
		if (multiplier != null)
		{
			return;
		}
		lock (this)
		{
			if (multiplier == null)
			{
				multiplier = new FiniteFieldMulipler();
			}
		}
	}
}
