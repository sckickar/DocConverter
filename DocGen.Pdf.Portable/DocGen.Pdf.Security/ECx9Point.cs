namespace DocGen.Pdf.Security;

internal class ECx9Point : Asn1Encode
{
	private readonly EllipticPoint m_point;

	public EllipticPoint Point => m_point;

	public ECx9Point(EllipticPoint point)
	{
		m_point = point;
	}

	public ECx9Point(EllipticCurves curve, Asn1Octet sequence)
	{
		m_point = curve.GetDecodedECPoint(sequence.GetOctets());
	}

	public override Asn1 GetAsn1()
	{
		return new DerOctet(m_point.Encoded());
	}
}
