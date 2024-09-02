namespace DocGen.Pdf.Security;

internal class SHA512MessageDigest : BigDigest
{
	private const int m_digestLength = 64;

	public override string AlgorithmName => "SHA-512";

	public override int MessageDigestSize => 64;

	public override int DoFinal(byte[] bytes, int offset)
	{
		Finish();
		Asn1Constants.UInt64ToBe(base.Header1, bytes, offset);
		Asn1Constants.UInt64ToBe(base.Header2, bytes, offset + 8);
		Asn1Constants.UInt64ToBe(base.Header3, bytes, offset + 16);
		Asn1Constants.UInt64ToBe(base.Header4, bytes, offset + 24);
		Asn1Constants.UInt64ToBe(base.Header5, bytes, offset + 32);
		Asn1Constants.UInt64ToBe(base.Header6, bytes, offset + 40);
		Asn1Constants.UInt64ToBe(base.Header7, bytes, offset + 48);
		Asn1Constants.UInt64ToBe(base.Header8, bytes, offset + 56);
		Reset();
		return 64;
	}

	public override void Reset()
	{
		base.Reset();
		base.Header1 = 7640891576956012808uL;
		base.Header2 = 13503953896175478587uL;
		base.Header3 = 4354685564936845355uL;
		base.Header4 = 11912009170470909681uL;
		base.Header5 = 5840696475078001361uL;
		base.Header6 = 11170449401992604703uL;
		base.Header7 = 2270897969802886507uL;
		base.Header8 = 6620516959819538809uL;
	}
}
