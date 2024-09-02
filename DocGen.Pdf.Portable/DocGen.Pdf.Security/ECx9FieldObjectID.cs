namespace DocGen.Pdf.Security;

internal class ECx9FieldObjectID : Asn1Encode
{
	private EllipticCurveElements m_field;

	public EllipticCurveElements Value => m_field;

	public ECx9FieldObjectID(EllipticCurveElements field)
	{
		m_field = field;
	}

	public ECx9FieldObjectID(Number point, Asn1Octet sequence)
		: this(new FinitePFieldObject(point, new Number(1, sequence.GetOctets())))
	{
	}

	public ECx9FieldObjectID(int num, int num1, int num2, int num3, Asn1Octet sequence)
		: this(new Finite2MFieldObject(num, num1, num2, num3, new Number(1, sequence.GetOctets())))
	{
	}

	public override Asn1 GetAsn1()
	{
		int byteLength = ECConvertPoint.GetByteLength(m_field);
		return new DerOctet(ECConvertPoint.ConvetByte(m_field.ToIntValue(), byteLength));
	}
}
