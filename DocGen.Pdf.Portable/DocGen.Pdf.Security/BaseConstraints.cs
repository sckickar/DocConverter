using System;

namespace DocGen.Pdf.Security;

internal class BaseConstraints : Asn1Encode
{
	private DerBoolean m_isCertificate;

	private DerInteger m_pathLength;

	internal Number PathLenConstraint
	{
		get
		{
			if (m_pathLength != null)
			{
				return m_pathLength.Value;
			}
			return null;
		}
	}

	internal bool IsCertificate
	{
		get
		{
			if (m_isCertificate != null)
			{
				return m_isCertificate.IsTrue;
			}
			return false;
		}
	}

	internal static BaseConstraints GetConstraints(object obj)
	{
		if (obj == null || obj is BaseConstraints)
		{
			return (BaseConstraints)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new BaseConstraints((Asn1Sequence)obj);
		}
		if (obj is X509Extension)
		{
			return GetConstraints(X509Extension.ConvertValueToObject((X509Extension)obj));
		}
		throw new ArgumentException("Invalid entry");
	}

	private BaseConstraints(Asn1Sequence sequence)
	{
		if (sequence.Count <= 0)
		{
			return;
		}
		if (sequence[0] is DerBoolean)
		{
			m_isCertificate = DerBoolean.GetBoolean(sequence[0]);
		}
		else
		{
			m_pathLength = DerInteger.GetNumber(sequence[0]);
		}
		if (sequence.Count > 1)
		{
			if (m_isCertificate == null)
			{
				throw new ArgumentException("Invalid length in sequence");
			}
			m_pathLength = DerInteger.GetNumber(sequence[1]);
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		if (m_isCertificate != null)
		{
			asn1EncodeCollection.Add(m_isCertificate);
		}
		if (m_pathLength != null)
		{
			asn1EncodeCollection.Add(m_pathLength);
		}
		return new DerSequence(asn1EncodeCollection);
	}

	public override string ToString()
	{
		if (m_pathLength == null)
		{
			return "BasicConstraints: isCa(" + IsCertificate + ")";
		}
		return "BasicConstraints: isCa(" + IsCertificate + "), pathLenConstraint = " + m_pathLength.Value;
	}
}
