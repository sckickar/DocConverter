using System;

namespace DocGen.Pdf.Security;

internal class ECDSAAlgorithm : IDSASigner
{
	private EllipticKeyParam ecKey;

	private SecureRandomAlgorithm ecRandomNumber;

	public string AlgorithmName => "ECDSA";

	public void Initialize(bool isSigning, ICipherParam parameters)
	{
		if (isSigning)
		{
			if (parameters is SecureParamNumber)
			{
				SecureParamNumber secureParamNumber = (SecureParamNumber)parameters;
				ecRandomNumber = secureParamNumber.Random;
				parameters = secureParamNumber.Parameters;
			}
			else
			{
				ecRandomNumber = new SecureRandomAlgorithm();
			}
			if (!(parameters is ECPrivateKey))
			{
				throw new Exception("EC private key required for signing");
			}
			ecKey = (ECPrivateKey)parameters;
		}
		else
		{
			if (!(parameters is ECPublicKeyParam))
			{
				throw new Exception("EC public key required for verification");
			}
			ecKey = (ECPublicKeyParam)parameters;
		}
	}

	public Number[] GenerateSignature(byte[] data)
	{
		Number numberX = ecKey.Parameters.NumberX;
		Number number = CalculateMessageBit(numberX, data);
		Number number2 = null;
		Number number3 = null;
		do
		{
			Number number4 = null;
			while (true)
			{
				number4 = new Number(numberX.BitLength, ecRandomNumber);
				if (number4.SignValue != 0 && number4.CompareTo(numberX) < 0)
				{
					number2 = ecKey.Parameters.PointG.Multiply(number4).PointX.ToIntValue().Mod(numberX);
					if (number2.SignValue != 0)
					{
						break;
					}
				}
			}
			Number key = ((ECPrivateKey)ecKey).Key;
			number3 = number4.ModInverse(numberX).Multiply(number.Add(key.Multiply(number2).Mod(numberX))).Mod(numberX);
		}
		while (number3.SignValue == 0);
		return new Number[2] { number2, number3 };
	}

	public bool ValidateSignature(byte[] message, Number number3, Number number4)
	{
		Number numberX = ecKey.Parameters.NumberX;
		if (number3.SignValue < 1 || number4.SignValue < 1 || number3.CompareTo(numberX) >= 0 || number4.CompareTo(numberX) >= 0)
		{
			return false;
		}
		Number number5 = CalculateMessageBit(numberX, message);
		Number val = number4.ModInverse(numberX);
		Number number6 = number5.Multiply(val).Mod(numberX);
		Number number7 = number3.Multiply(val).Mod(numberX);
		EllipticPoint pointG = ecKey.Parameters.PointG;
		EllipticPoint pointQ = ((ECPublicKeyParam)ecKey).PointQ;
		EllipticPoint ellipticPoint = ECMath.AddCurve(pointG, number6, pointQ, number7);
		if (ellipticPoint.IsInfinity)
		{
			return false;
		}
		return ellipticPoint.PointX.ToIntValue().Mod(numberX).Equals(number3);
	}

	private Number CalculateMessageBit(Number number1, byte[] data)
	{
		int num = data.Length * 8;
		Number number2 = new Number(1, data);
		if (number1.BitLength < num)
		{
			number2 = number2.ShiftRight(num - number1.BitLength);
		}
		return number2;
	}
}
