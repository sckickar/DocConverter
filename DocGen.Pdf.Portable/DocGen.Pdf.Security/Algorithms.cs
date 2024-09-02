using System;

namespace DocGen.Pdf.Security;

internal class Algorithms : Asn1Encode
{
	private Asn1Sequence m_sequence;

	private DerObjectID m_objectID;

	private Asn1Encode m_parameters;

	private bool m_parametersDefined;

	public virtual DerObjectID ObjectID => m_objectID;

	public Asn1Encode Parameters => m_parameters;

	internal Algorithms(Asn1Identifier id, Asn1 asn1)
	{
		m_sequence = new Asn1Sequence();
		m_sequence.Objects.Add(id);
		m_sequence.Objects.Add(asn1);
	}

	internal static Algorithms GetAlgorithms(object obj)
	{
		if (obj == null || obj is Algorithms)
		{
			return (Algorithms)obj;
		}
		if (obj is DerObjectID)
		{
			return new Algorithms((DerObjectID)obj);
		}
		if (obj is string)
		{
			return new Algorithms((string)obj);
		}
		return new Algorithms(Asn1Sequence.GetSequence(obj));
	}

	internal Algorithms(DerObjectID objectID)
	{
		m_objectID = objectID;
	}

	internal Algorithms(string objectID)
	{
		m_objectID = new DerObjectID(objectID);
	}

	internal Algorithms(DerObjectID objectID, Asn1Encode parameters)
	{
		m_objectID = objectID;
		m_parameters = parameters;
		m_parametersDefined = true;
	}

	internal Algorithms(Asn1Sequence sequence)
	{
		if (sequence.Count < 1 || sequence.Count > 2)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_objectID = DerObjectID.GetID(sequence[0]);
		m_parametersDefined = sequence.Count == 2;
		if (m_parametersDefined)
		{
			m_parameters = sequence[1];
		}
	}

	internal byte[] AsnEncode()
	{
		return m_sequence.AsnEncode();
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_objectID);
		if (m_parametersDefined)
		{
			if (m_parameters != null)
			{
				asn1EncodeCollection.Add(m_parameters);
			}
			else
			{
				asn1EncodeCollection.Add(DerNull.Value);
			}
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
