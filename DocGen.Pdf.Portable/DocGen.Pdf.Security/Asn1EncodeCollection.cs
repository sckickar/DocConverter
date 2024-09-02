using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class Asn1EncodeCollection : IEnumerable
{
	private List<object> m_encodableObjects = new List<object>();

	internal Asn1Encode this[int index] => (Asn1Encode)m_encodableObjects[index];

	internal int Count => m_encodableObjects.Count;

	internal Asn1EncodeCollection(params Asn1Encode[] vector)
	{
		Add(vector);
	}

	internal static Asn1EncodeCollection FromEnumerable(IEnumerable e)
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		foreach (Asn1Encode item in e)
		{
			asn1EncodeCollection.Add(item);
		}
		return asn1EncodeCollection;
	}

	internal void Add(params Asn1Encode[] objs)
	{
		foreach (Asn1Encode item in objs)
		{
			m_encodableObjects.Add(item);
		}
	}

	public IEnumerator GetEnumerator()
	{
		return m_encodableObjects.GetEnumerator();
	}
}
