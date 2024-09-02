using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DocGen.Pdf.Security;

internal class X509Certificate : X509ExtensionBase
{
	private X509CertificateStructure m_c;

	private BaseConstraints m_basicConstraints;

	private bool[] m_keyUsage;

	private bool m_hashValueSet;

	private int m_hashValue;

	internal virtual X509CertificateStructure CertificateStructure => m_c;

	public virtual bool IsValidNow => IsValid(DateTime.UtcNow);

	public virtual int Version => m_c.Version;

	public virtual Number SerialNumber => m_c.SerialNumber.Value;

	public virtual X509Name IssuerDN => m_c.Issuer;

	public virtual X509Name SubjectDN => m_c.Subject;

	public virtual DateTime NotBefore => m_c.StartDate.ToDateTime();

	public virtual DateTime NotAfter => m_c.EndDate.ToDateTime();

	public virtual string SigAlgName => new SignerUtilities().GetEncoding(m_c.SignatureAlgorithm.ObjectID);

	public virtual string SigAlgOid => m_c.SignatureAlgorithm.ObjectID.ID;

	public virtual DerBitString IssuerUniqueID => m_c.TbsCertificate.IssuerUniqueID;

	public virtual DerBitString SubjectUniqueID => m_c.TbsCertificate.SubjectUniqueID;

	protected X509Certificate()
	{
	}

	internal X509Certificate(X509CertificateStructure c)
	{
		m_c = c;
		try
		{
			Asn1Octet extension = GetExtension(new DerObjectID("2.5.29.19"));
			if (extension != null)
			{
				m_basicConstraints = BaseConstraints.GetConstraints(Asn1.FromByteArray(extension.GetOctets()));
			}
		}
		catch (Exception ex)
		{
			throw new Exception("cannot construct BasicConstraints: " + ex);
		}
		try
		{
			Asn1Octet extension2 = GetExtension(new DerObjectID("2.5.29.15"));
			if (extension2 != null)
			{
				DerBitString @string = DerBitString.GetString(Asn1.FromByteArray(extension2.GetOctets()));
				byte[] bytes = @string.GetBytes();
				int num = bytes.Length * 8 - @string.ExtraBits;
				m_keyUsage = new bool[(num < 9) ? 9 : num];
				for (int i = 0; i != num; i++)
				{
					m_keyUsage[i] = (bytes[i / 8] & (128 >> i % 8)) != 0;
				}
			}
			else
			{
				m_keyUsage = null;
			}
		}
		catch (Exception ex2)
		{
			throw new Exception("cannot construct KeyUsage: " + ex2);
		}
	}

	public virtual bool IsValid(DateTime time)
	{
		if (time.CompareTo(NotBefore) >= 0)
		{
			return time.CompareTo(NotAfter) <= 0;
		}
		return false;
	}

	public virtual void CheckValidity()
	{
		CheckValidity(DateTime.UtcNow);
	}

	public virtual void CheckValidity(DateTime time)
	{
		if (time.CompareTo(NotAfter) > 0)
		{
			throw new Exception("certificate expired on " + m_c.EndDate.GetTime());
		}
		if (time.CompareTo(NotBefore) < 0)
		{
			throw new Exception("certificate not valid until " + m_c.StartDate.GetTime());
		}
	}

	public virtual byte[] GetTbsCertificate()
	{
		return m_c.TbsCertificate.GetDerEncoded();
	}

	public virtual byte[] GetSignature()
	{
		return m_c.Signature.GetBytes();
	}

	public virtual byte[] GetSigAlgParams()
	{
		if (m_c.SignatureAlgorithm.Parameters != null)
		{
			return m_c.SignatureAlgorithm.Parameters.GetDerEncoded();
		}
		return null;
	}

	public virtual bool[] GetKeyUsage()
	{
		if (m_keyUsage != null)
		{
			return (bool[])m_keyUsage.Clone();
		}
		return null;
	}

	public virtual IList GetExtendedKeyUsage()
	{
		Asn1Octet extension = GetExtension(new DerObjectID("2.5.29.37"));
		if (extension == null)
		{
			return null;
		}
		try
		{
			Asn1Sequence sequence = Asn1Sequence.GetSequence(Asn1.FromByteArray(extension.GetOctets()));
			List<string> list = new List<string>();
			foreach (DerObjectID item in sequence)
			{
				list.Add(item.ID);
			}
			return list;
		}
		catch (Exception innerException)
		{
			throw new Exception("error processing extended key usage extension", innerException);
		}
	}

	internal List<string> GetOids(bool critical)
	{
		X509Extensions x509Extensions = GetX509Extensions();
		if (x509Extensions != null)
		{
			List<string> list = new List<string>();
			{
				foreach (DerObjectID extensionOid in x509Extensions.ExtensionOids)
				{
					if (x509Extensions.GetExtension(extensionOid).IsCritical == critical)
					{
						list.Add(extensionOid.ID);
					}
				}
				return list;
			}
		}
		return null;
	}

	public virtual int GetBasicConstraints()
	{
		if (m_basicConstraints != null && m_basicConstraints.IsCertificate)
		{
			if (m_basicConstraints.PathLenConstraint == null)
			{
				return int.MaxValue;
			}
			return m_basicConstraints.PathLenConstraint.IntValue;
		}
		return -1;
	}

	public virtual ICollection GetSubjectAlternativeNames()
	{
		return GetAlternativeNames("2.5.29.17");
	}

	public virtual ICollection GetIssuerAlternativeNames()
	{
		return GetAlternativeNames("2.5.29.18");
	}

	protected virtual ICollection GetAlternativeNames(string oid)
	{
		if (GetExtension(new DerObjectID(oid)) == null)
		{
			return null;
		}
		return new List<object>();
	}

	protected override X509Extensions GetX509Extensions()
	{
		if (m_c.Version != 3)
		{
			return null;
		}
		return m_c.TbsCertificate.Extensions;
	}

	public virtual CipherParameter GetPublicKey()
	{
		return CreateKey(m_c.SubjectPublicKeyInfo);
	}

	internal CipherParameter CreateKey(PublicKeyInformation keyInfo)
	{
		Algorithms algorithm = keyInfo.Algorithm;
		DerObjectID objectID = algorithm.ObjectID;
		if (objectID.ID.Equals(PKCSOIDs.RsaEncryption.ID) || objectID.ID.Equals(X509Objects.IdEARsa.ID))
		{
			RSAPublicKey publicKey = RSAPublicKey.GetPublicKey(keyInfo.GetPublicKey());
			return new RsaKeyParam(isPrivate: false, publicKey.Modulus, publicKey.PublicExponent);
		}
		if (objectID.Equals(ECDSAOIDs.IdECPublicKey))
		{
			ECX962Params eCX962Params = new ECX962Params(algorithm.Parameters.GetAsn1());
			ECX9Field eCX9Field = ((!eCX962Params.IsNamedCurve) ? new ECX9Field((Asn1Sequence)eCX962Params.Parameters) : EllipicCryptoKeyGen.GetECCurveByObjectID((DerObjectID)eCX962Params.Parameters));
			Asn1Octet sequence = new DerOctet(keyInfo.PublicKey.GetBytes());
			EllipticPoint point = new ECx9Point(eCX9Field.Curve, sequence).Point;
			if (eCX962Params.IsNamedCurve)
			{
				return new ECPublicKeyParam("EC", point, (DerObjectID)eCX962Params.Parameters);
			}
			EllipticCurveParams parameters = new EllipticCurveParams(eCX9Field.Curve, eCX9Field.PointG, eCX9Field.NumberX, eCX9Field.NumberY, eCX9Field.Seed());
			return new ECPublicKeyParam(point, parameters);
		}
		throw new Exception("algorithm identifier in key not recognised: " + objectID);
	}

	public virtual byte[] GetEncoded()
	{
		return m_c.GetDerEncoded();
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is X509Certificate x509Certificate))
		{
			return false;
		}
		return m_c.Equals(x509Certificate.m_c);
	}

	public override int GetHashCode()
	{
		lock (this)
		{
			if (!m_hashValueSet)
			{
				m_hashValue = m_c.GetHashCode();
				m_hashValueSet = true;
			}
		}
		return m_hashValue;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string newLine = Environment.NewLine;
		stringBuilder.Append("  [0]         Version: ").Append(Version).Append(newLine);
		stringBuilder.Append("         SerialNumber: ").Append(SerialNumber).Append(newLine);
		stringBuilder.Append("             IssuerDN: ").Append(IssuerDN).Append(newLine);
		stringBuilder.Append("           Start Date: ").Append(NotBefore).Append(newLine);
		stringBuilder.Append("           Final Date: ").Append(NotAfter).Append(newLine);
		stringBuilder.Append("            SubjectDN: ").Append(SubjectDN).Append(newLine);
		stringBuilder.Append("           Public Key: ").Append(GetPublicKey()).Append(newLine);
		stringBuilder.Append("  Signature Algorithm: ").Append(SigAlgName).Append(newLine);
		GetSignature();
		X509Extensions extensions = m_c.TbsCertificate.Extensions;
		if (extensions != null)
		{
			IEnumerator enumerator = extensions.ExtensionOids.GetEnumerator();
			if (enumerator.MoveNext())
			{
				stringBuilder.Append("       Extensions: \n");
			}
			do
			{
				DerObjectID derObjectID = (DerObjectID)enumerator.Current;
				X509Extension extension = extensions.GetExtension(derObjectID);
				if (extension.Value != null)
				{
					Asn1 obj = Asn1.FromByteArray(extension.Value.GetOctets());
					stringBuilder.Append("                       critical(").Append(extension.IsCritical).Append(") ");
					try
					{
						if (derObjectID.Equals(X509Extensions.BasicConstraints))
						{
							stringBuilder.Append(BaseConstraints.GetConstraints(obj));
						}
					}
					catch (Exception)
					{
						stringBuilder.Append(derObjectID.ID);
						stringBuilder.Append(" value = ").Append("*****");
					}
				}
				stringBuilder.Append(newLine);
			}
			while (enumerator.MoveNext());
		}
		return stringBuilder.ToString();
	}

	public virtual void Verify(CipherParameter key)
	{
		string iD = m_c.SignatureAlgorithm.ObjectID.ID;
		ISigner signer = new SignerUtilities().GetSigner(iD);
		CheckSignature(key, signer);
	}

	protected virtual void CheckSignature(CipherParameter publicKey, ISigner signature)
	{
		if (!IsAlgIDEqual(m_c.SignatureAlgorithm, m_c.TbsCertificate.Signature))
		{
			throw new Exception("signature algorithm in TBS cert not same as outer cert");
		}
		_ = m_c.SignatureAlgorithm.Parameters;
		signature.Initialize(isSigning: false, publicKey);
		byte[] tbsCertificate = GetTbsCertificate();
		signature.BlockUpdate(tbsCertificate, 0, tbsCertificate.Length);
		byte[] signature2 = GetSignature();
		if (!signature.ValidateSignature(signature2))
		{
			throw new Exception("Public key presented not for certificate signature");
		}
	}

	private static bool IsAlgIDEqual(Algorithms id1, Algorithms id2)
	{
		if (!id1.ObjectID.Equals(id2.ObjectID))
		{
			return false;
		}
		Asn1Encode parameters = id1.Parameters;
		Asn1Encode parameters2 = id2.Parameters;
		if (parameters == null == (parameters2 == null))
		{
			return object.Equals(parameters, parameters2);
		}
		if (parameters != null)
		{
			return parameters.GetAsn1() is Asn1Null;
		}
		return parameters2.GetAsn1() is Asn1Null;
	}
}
