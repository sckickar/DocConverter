namespace DocGen.Pdf.Security;

internal class EllipticCryptoParam : KeyGenParam
{
	private readonly EllipticCurveParams domainParameters;

	private readonly DerObjectID publicCyptoKey;

	public EllipticCurveParams DomainParameters => domainParameters;

	public DerObjectID PublicKeyParamSet => publicCyptoKey;

	public EllipticCryptoParam(EllipticCurveParams domainParameters, SecureRandomAlgorithm random)
		: base(random, domainParameters.NumberX.BitLength)
	{
		this.domainParameters = domainParameters;
	}

	public EllipticCryptoParam(DerObjectID publicCyptoKey, SecureRandomAlgorithm random)
		: this(EllipticKeyParam.FindParameters(publicCyptoKey), random)
	{
		this.publicCyptoKey = publicCyptoKey;
	}
}
