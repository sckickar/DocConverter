namespace DocGen.Pdf.Security;

internal class DerNull : Asn1Null
{
	internal static readonly DerNull Value = new DerNull(0);

	private byte[] m_bytes = new byte[0];

	internal DerNull(int value)
	{
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(5, m_bytes);
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		return asn1 is DerNull;
	}

	public override int GetHashCode()
	{
		return -1;
	}
}
