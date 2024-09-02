using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class X509Extensions : Asn1Encode
{
	internal static readonly DerObjectID SubjectDirectoryAttributes = new DerObjectID("2.5.29.9");

	internal static readonly DerObjectID SubjectKeyIdentifier = new DerObjectID("2.5.29.14");

	internal static readonly DerObjectID KeyUsage = new DerObjectID("2.5.29.15");

	internal static readonly DerObjectID PrivateKeyUsagePeriod = new DerObjectID("2.5.29.16");

	internal static readonly DerObjectID SubjectAlternativeName = new DerObjectID("2.5.29.17");

	internal static readonly DerObjectID IssuerAlternativeName = new DerObjectID("2.5.29.18");

	internal static readonly DerObjectID BasicConstraints = new DerObjectID("2.5.29.19");

	internal static readonly DerObjectID CrlNumber = new DerObjectID("2.5.29.20");

	internal static readonly DerObjectID ReasonCode = new DerObjectID("2.5.29.21");

	internal static readonly DerObjectID InstructionCode = new DerObjectID("2.5.29.23");

	internal static readonly DerObjectID InvalidityDate = new DerObjectID("2.5.29.24");

	internal static readonly DerObjectID DeltaCrlIndicator = new DerObjectID("2.5.29.27");

	internal static readonly DerObjectID IssuingDistributionPoint = new DerObjectID("2.5.29.28");

	internal static readonly DerObjectID CertificateIssuer = new DerObjectID("2.5.29.29");

	internal static readonly DerObjectID NameConstraints = new DerObjectID("2.5.29.30");

	internal static readonly DerObjectID CrlDistributionPoints = new DerObjectID("2.5.29.31");

	internal static readonly DerObjectID CertificatePolicies = new DerObjectID("2.5.29.32");

	internal static readonly DerObjectID PolicyMappings = new DerObjectID("2.5.29.33");

	internal static readonly DerObjectID AuthorityKeyIdentifier = new DerObjectID("2.5.29.35");

	internal static readonly DerObjectID PolicyConstraints = new DerObjectID("2.5.29.36");

	internal static readonly DerObjectID ExtendedKeyUsage = new DerObjectID("2.5.29.37");

	internal static readonly DerObjectID FreshestCrl = new DerObjectID("2.5.29.46");

	internal static readonly DerObjectID InhibitAnyPolicy = new DerObjectID("2.5.29.54");

	internal static readonly DerObjectID AuthorityInfoAccess = new DerObjectID("1.3.6.1.5.5.7.1.1");

	internal static readonly DerObjectID SubjectInfoAccess = new DerObjectID("1.3.6.1.5.5.7.1.11");

	internal static readonly DerObjectID LogoType = new DerObjectID("1.3.6.1.5.5.7.1.12");

	internal static readonly DerObjectID BiometricInfo = new DerObjectID("1.3.6.1.5.5.7.1.2");

	internal static readonly DerObjectID QCStatements = new DerObjectID("1.3.6.1.5.5.7.1.3");

	internal static readonly DerObjectID AuditIdentity = new DerObjectID("1.3.6.1.5.5.7.1.4");

	internal static readonly DerObjectID NoRevAvail = new DerObjectID("2.5.29.56");

	internal static readonly DerObjectID TargetInformation = new DerObjectID("2.5.29.55");

	private readonly Dictionary<DerObjectID, X509Extension> m_extensions = new Dictionary<DerObjectID, X509Extension>();

	private readonly IList m_ordering;

	internal IEnumerable ExtensionOids => new EnumerableProxy(m_ordering);

	internal static X509Extensions GetInstance(Asn1Tag obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetSequence(obj, explicitly));
	}

	internal static X509Extensions GetInstance(object obj)
	{
		if (obj == null || obj is X509Extensions)
		{
			return (X509Extensions)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new X509Extensions(obj as Asn1Sequence);
		}
		if (obj is Asn1Tag)
		{
			return GetInstance(((Asn1Tag)obj).GetObject());
		}
		throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
	}

	private X509Extensions(Asn1Sequence seq)
	{
		m_ordering = new List<DerObjectID>();
		foreach (Asn1Encode item in seq)
		{
			Asn1Sequence sequence = Asn1Sequence.GetSequence(item.GetAsn1());
			if (sequence.Count < 2 || sequence.Count > 3)
			{
				throw new ArgumentException("Bad sequence size: " + sequence.Count);
			}
			DerObjectID iD = DerObjectID.GetID(sequence[0].GetAsn1());
			bool critical = sequence.Count == 3 && DerBoolean.GetBoolean(sequence[1].GetAsn1()).IsTrue;
			Asn1Octet octetString = Asn1Octet.GetOctetString(sequence[sequence.Count - 1].GetAsn1());
			m_extensions.Add(iD, new X509Extension(critical, octetString));
			m_ordering.Add(iD);
		}
	}

	internal X509Extension GetExtension(DerObjectID oid)
	{
		if (m_extensions.ContainsKey(oid))
		{
			return m_extensions[oid];
		}
		return null;
	}

	internal X509Extensions(IDictionary extensions)
		: this(null, extensions)
	{
	}

	internal X509Extensions(IList ordering, IDictionary extensions)
	{
		if (ordering == null)
		{
			List<object> list = new List<object>();
			foreach (object key2 in extensions.Keys)
			{
				list.Add(key2);
			}
			m_ordering = list;
		}
		else
		{
			m_ordering = ordering;
		}
		foreach (DerObjectID item in m_ordering)
		{
			m_extensions.Add(item, (X509Extension)extensions[item]);
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		foreach (DerObjectID item in m_ordering)
		{
			X509Extension x509Extension = m_extensions[item];
			Asn1EncodeCollection asn1EncodeCollection2 = new Asn1EncodeCollection(item);
			if (x509Extension.IsCritical)
			{
				asn1EncodeCollection2.Add(DerBoolean.True);
			}
			asn1EncodeCollection2.Add(x509Extension.Value);
			asn1EncodeCollection.Add(new DerSequence(asn1EncodeCollection2));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
