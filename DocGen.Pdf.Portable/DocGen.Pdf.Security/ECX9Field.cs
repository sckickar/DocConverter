using System;

namespace DocGen.Pdf.Security;

internal class ECX9Field : Asn1Encode
{
	private ECx9FieldObject m_fieldID;

	private EllipticCurves m_curve;

	private EllipticPoint m_point;

	private Number m_num1;

	private Number m_num2;

	private byte[] m_seed;

	public EllipticCurves Curve => m_curve;

	public EllipticPoint PointG => m_point;

	public Number NumberX => m_num1;

	public Number NumberY
	{
		get
		{
			if (m_num2 == null)
			{
				return Number.One;
			}
			return m_num2;
		}
	}

	public ECX9Field(Asn1Sequence sequence)
	{
		if (!(sequence[0] is DerInteger) || !((DerInteger)sequence[0]).Value.Equals(Number.One))
		{
			throw new ArgumentException("bad version in ECX9Field");
		}
		ECx9Curve eCx9Curve = null;
		eCx9Curve = ((!(sequence[2] is ECx9Curve)) ? new ECx9Curve(new ECx9FieldObject((Asn1Sequence)sequence[1]), (Asn1Sequence)sequence[2]) : ((ECx9Curve)sequence[2]));
		m_curve = eCx9Curve.Curve;
		if (sequence[3] is ECx9Point)
		{
			m_point = ((ECx9Point)sequence[3]).Point;
		}
		else
		{
			m_point = new ECx9Point(m_curve, (Asn1Octet)sequence[3]).Point;
		}
		m_num1 = ((DerInteger)sequence[4]).Value;
		m_seed = eCx9Curve.GetSeed();
		if (sequence.Count == 6)
		{
			m_num2 = ((DerInteger)sequence[5]).Value;
		}
	}

	public ECX9Field(EllipticCurves curve, EllipticPoint point, Number num, Number num1)
		: this(curve, point, num, num1, null)
	{
	}

	public ECX9Field(EllipticCurves curve, EllipticPoint point, Number num, Number num1, byte[] seed)
	{
		m_curve = curve;
		m_point = point;
		m_num1 = num;
		m_num2 = num1;
		m_seed = seed;
		if (curve is FiniteCurves)
		{
			m_fieldID = new ECx9FieldObject(((FiniteCurves)curve).PointQ);
		}
		else if (curve is Field2MCurves)
		{
			Field2MCurves field2MCurves = (Field2MCurves)curve;
			m_fieldID = new ECx9FieldObject(field2MCurves.PointM, field2MCurves.ElementX, field2MCurves.ElementY, field2MCurves.ElementZ);
		}
	}

	public byte[] Seed()
	{
		return m_seed;
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(new DerInteger(1), m_fieldID, new ECx9Curve(m_curve, m_seed), new ECx9Point(m_point), new DerInteger(m_num1));
		if (m_num2 != null)
		{
			asn1EncodeCollection.Add(new DerInteger(m_num2));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
