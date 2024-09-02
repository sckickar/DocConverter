using System;

namespace DocGen.Pdf.Security;

internal class EllipticKeyParam : CipherParameter
{
	private static readonly string[] algorithms = new string[6] { "EC", "ECDSA", "ECDH", "ECDHC", "ECGOST3410", "ECMQV" };

	private readonly string algorithm;

	private readonly EllipticCurveParams parameters;

	private readonly DerObjectID publicCyptoKey;

	public string AlgorithmName => algorithm;

	public EllipticCurveParams Parameters => parameters;

	public DerObjectID PublicKeyParamSet => publicCyptoKey;

	protected EllipticKeyParam(string algorithm, bool isPrivate, EllipticCurveParams parameters)
		: base(isPrivate)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		this.algorithm = VerifyAlgorithmName(algorithm);
		this.parameters = parameters;
	}

	protected EllipticKeyParam(string algorithm, bool isPrivate, DerObjectID publicCyptoKey)
		: base(isPrivate)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		if (publicCyptoKey == null)
		{
			throw new ArgumentNullException("publicCyptoKey");
		}
		this.algorithm = VerifyAlgorithmName(algorithm);
		parameters = FindParameters(publicCyptoKey);
		this.publicCyptoKey = publicCyptoKey;
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is EllipticCurveParams obj))
		{
			return false;
		}
		return Equals(obj);
	}

	protected bool Equals(EllipticKeyParam element)
	{
		if (parameters.Equals(element.parameters))
		{
			return Equals((CipherParameter)element);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return parameters.GetHashCode() ^ base.GetHashCode();
	}

	internal static string VerifyAlgorithmName(string algorithm)
	{
		string result = algorithm.ToUpper();
		if (Array.IndexOf(algorithms, algorithm, 0, algorithms.Length) < 0)
		{
			throw new ArgumentException("unknown algorithm: " + algorithm);
		}
		return result;
	}

	internal static EllipticCurveParams FindParameters(DerObjectID publicCyptoKey)
	{
		if (publicCyptoKey == null)
		{
			throw new ArgumentNullException("publicCyptoKey");
		}
		EllipticCurveParams ellipticCurveParams = EllipticGOSTCurves.GetByOid(publicCyptoKey);
		if (ellipticCurveParams == null)
		{
			ECX9Field eCCurveByObjectID = EllipicCryptoKeyGen.GetECCurveByObjectID(publicCyptoKey);
			if (eCCurveByObjectID == null)
			{
				throw new ArgumentException("OID is not valid");
			}
			ellipticCurveParams = new EllipticCurveParams(eCCurveByObjectID.Curve, eCCurveByObjectID.PointG, eCCurveByObjectID.NumberX, eCCurveByObjectID.NumberY, eCCurveByObjectID.Seed());
		}
		return ellipticCurveParams;
	}
}
