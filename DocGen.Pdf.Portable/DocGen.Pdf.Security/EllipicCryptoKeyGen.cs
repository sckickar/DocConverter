using System;

namespace DocGen.Pdf.Security;

internal class EllipicCryptoKeyGen : ICipherKeyGen
{
	private readonly string algorithm;

	private EllipticCurveParams parameters;

	private DerObjectID publicCyptoKey;

	private SecureRandomAlgorithm randomNumber;

	internal EllipicCryptoKeyGen()
		: this("EC")
	{
	}

	internal EllipicCryptoKeyGen(string algorithm)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		this.algorithm = EllipticKeyParam.VerifyAlgorithmName(algorithm);
	}

	public void Init(KeyGenParam parameters)
	{
		if (parameters is EllipticCryptoParam)
		{
			EllipticCryptoParam ellipticCryptoParam = (EllipticCryptoParam)parameters;
			publicCyptoKey = ellipticCryptoParam.PublicKeyParamSet;
			this.parameters = ellipticCryptoParam.DomainParameters;
		}
		else
		{
			ECX9Field eCCurveByObjectID = GetECCurveByObjectID(parameters.Strength switch
			{
				192 => ECDSAOIDs.ECPC192v1, 
				224 => ECSecIDs.ECSECP224r1, 
				239 => ECDSAOIDs.ECPC239v1, 
				256 => ECDSAOIDs.ECPC256v1, 
				384 => ECSecIDs.ECSECP384r1, 
				521 => ECSecIDs.ECSECP521r1, 
				_ => throw new PdfException("unknown key size."), 
			});
			this.parameters = new EllipticCurveParams(eCCurveByObjectID.Curve, eCCurveByObjectID.PointG, eCCurveByObjectID.NumberX, eCCurveByObjectID.NumberY, eCCurveByObjectID.Seed());
		}
		randomNumber = parameters.Random;
	}

	public ECCipherKeyParam GenerateKeyPair()
	{
		Number numberX = parameters.NumberX;
		Number number;
		do
		{
			number = new Number(numberX.BitLength, randomNumber);
		}
		while (number.SignValue == 0 || number.CompareTo(numberX) >= 0);
		EllipticPoint pointQ = parameters.PointG.Multiply(number);
		if (publicCyptoKey != null)
		{
			return new ECCipherKeyParam(new ECPublicKeyParam(algorithm, pointQ, publicCyptoKey), new ECPrivateKey(algorithm, number, publicCyptoKey));
		}
		return new ECCipherKeyParam(new ECPublicKeyParam(algorithm, pointQ, parameters), new ECPrivateKey(algorithm, number, parameters));
	}

	internal static ECX9Field GetECCurveByObjectID(DerObjectID objectIds)
	{
		ECX9Field byOid = ECX962Curves.GetByOid(objectIds);
		if (byOid == null)
		{
			byOid = SECGCurves.GetByOid(objectIds);
			if (byOid == null)
			{
				byOid = ECNamedCurves.GetByOid(objectIds);
				if (byOid == null)
				{
					byOid = ECBrainpoolAlgorithm.GetByOid(objectIds);
				}
			}
		}
		return byOid;
	}
}
