using System;

namespace DocGen.Pdf.Security;

internal class MessageStamp : Asn1Sequence
{
	internal Asn1Sequence m_sequence;

	private Algorithms m_hashAlgorithm;

	private byte[] m_hash;

	internal string HashAlgorithm => m_hashAlgorithm.ObjectID.ID;

	internal byte[] HashedMessage => m_hash;

	internal MessageStamp(string id, byte[] hash)
	{
		Asn1Identifier id2 = new Asn1Identifier(id);
		base.Objects.Add(new Algorithms(id2, DerNull.Value));
		base.Objects.Add(Asn1.FromByteArray(hash));
	}

	internal static MessageStamp GetMessageStamp(object obj)
	{
		if (obj == null || obj is MessageStamp)
		{
			return (MessageStamp)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new MessageStamp((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence " + obj.GetType().FullName);
	}

	private MessageStamp(Asn1Sequence sequence)
	{
		if (sequence.Count != 2)
		{
			throw new ArgumentException("Invalid length in sequence", "sequence");
		}
		m_hashAlgorithm = Algorithms.GetAlgorithms(sequence[0]);
		m_hash = Asn1Octet.GetOctetString(sequence[1]).GetOctets();
	}
}
