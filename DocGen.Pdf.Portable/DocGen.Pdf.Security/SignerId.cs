namespace DocGen.Pdf.Security;

internal class SignerId : X509CertificateHelper
{
	public override int GetHashCode()
	{
		int num = 0;
		if (base.KeyIdentifier != null)
		{
			int num2 = base.KeyIdentifier.Length;
			num = num2 + 1;
			while (--num2 >= 0)
			{
				num *= 257;
				num ^= base.KeyIdentifier[num2];
			}
		}
		Number serialNumber = base.SerialNumber;
		if (serialNumber != null)
		{
			num ^= serialNumber.GetHashCode();
		}
		X509Name issuer = base.Issuer;
		if (issuer != null)
		{
			num ^= issuer.GetHashCode();
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return false;
		}
		if (!(obj is SignerId signerId))
		{
			return false;
		}
		if (AreEqual(base.KeyIdentifier, signerId.KeyIdentifier))
		{
			return AreEqual(base.Issuer, signerId.Issuer);
		}
		return false;
	}

	private bool AreEqual(X509Name issuer1, X509Name issuer2)
	{
		return issuer1?.Equivalent(issuer1) ?? (issuer2 == null);
	}

	private bool AreEqual(byte[] a, byte[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		int num = a.Length;
		if (num != b.Length)
		{
			return false;
		}
		while (num != 0)
		{
			num--;
			if (a[num] != b[num])
			{
				return false;
			}
		}
		return true;
	}
}
