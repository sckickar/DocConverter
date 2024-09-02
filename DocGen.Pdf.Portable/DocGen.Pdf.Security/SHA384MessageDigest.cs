namespace DocGen.Pdf.Security;

internal class SHA384MessageDigest : BigDigest
{
	private const int DigestLength = 48;

	public override string AlgorithmName => "SHA-384";

	public override int MessageDigestSize => 48;

	public override int DoFinal(byte[] bytes, int offset)
	{
		Finish();
		Asn1Constants.UInt64ToBe(base.Header1, bytes, offset);
		Asn1Constants.UInt64ToBe(base.Header2, bytes, offset + 8);
		Asn1Constants.UInt64ToBe(base.Header3, bytes, offset + 16);
		Asn1Constants.UInt64ToBe(base.Header4, bytes, offset + 24);
		Asn1Constants.UInt64ToBe(base.Header5, bytes, offset + 32);
		Asn1Constants.UInt64ToBe(base.Header6, bytes, offset + 40);
		Reset();
		return 48;
	}

	public override void Reset()
	{
		base.Reset();
		base.Header1 = 14680500436340154072uL;
		base.Header2 = 7105036623409894663uL;
		base.Header3 = 10473403895298186519uL;
		base.Header4 = 1526699215303891257uL;
		base.Header5 = 7436329637833083697uL;
		base.Header6 = 10282925794625328401uL;
		base.Header7 = 15784041429090275239uL;
		base.Header8 = 5167115440072839076uL;
	}
}
