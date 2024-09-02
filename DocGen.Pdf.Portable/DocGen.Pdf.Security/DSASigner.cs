using System;

namespace DocGen.Pdf.Security;

internal class DSASigner : ISigner
{
	private readonly IMessageDigest algorithm;

	private readonly IDSASigner signer;

	private bool isSign;

	public string AlgorithmName => algorithm.AlgorithmName + "with" + signer.AlgorithmName;

	internal DSASigner(IDSASigner signer, IMessageDigest digest)
	{
		algorithm = digest;
		this.signer = signer;
	}

	public void Initialize(bool isSigning, ICipherParam parameters)
	{
		isSign = isSigning;
		CipherParameter cipherParameter = ((!(parameters is SecureParamNumber)) ? ((CipherParameter)parameters) : ((CipherParameter)((SecureParamNumber)parameters).Parameters));
		if (isSigning && !cipherParameter.IsPrivate)
		{
			throw new Exception("Needs Private Key");
		}
		if (!isSigning && cipherParameter.IsPrivate)
		{
			throw new Exception("Needs Public Key.");
		}
		Reset();
		signer.Initialize(isSigning, parameters);
	}

	public void Update(byte input)
	{
		algorithm.Update(input);
	}

	public void BlockUpdate(byte[] input, int offset, int length)
	{
		algorithm.BlockUpdate(input, offset, length);
	}

	public byte[] GenerateSignature()
	{
		if (!isSign)
		{
			throw new InvalidOperationException("DSASigner not initialised");
		}
		byte[] array = new byte[algorithm.MessageDigestSize];
		algorithm.DoFinal(array, 0);
		Number[] array2 = signer.GenerateSignature(array);
		return DerEncode(array2[0], array2[1]);
	}

	public bool ValidateSignature(byte[] signature)
	{
		if (isSign)
		{
			throw new InvalidOperationException("DSASigner not initialised");
		}
		byte[] array = new byte[algorithm.MessageDigestSize];
		algorithm.DoFinal(array, 0);
		try
		{
			Number[] array2 = DerDecode(signature);
			return signer.ValidateSignature(array, array2[0], array2[1]);
		}
		catch (Exception)
		{
			return false;
		}
	}

	public void Reset()
	{
		algorithm.Reset();
	}

	private byte[] DerEncode(Number number1, Number number2)
	{
		return new DerSequence(new DerInteger(number1), new DerInteger(number2)).GetDerEncoded();
	}

	private Number[] DerDecode(byte[] encoding)
	{
		Asn1Sequence asn1Sequence = (Asn1Sequence)Asn1.FromByteArray(encoding);
		return new Number[2]
		{
			((DerInteger)asn1Sequence[0]).Value,
			((DerInteger)asn1Sequence[1]).Value
		};
	}
}
