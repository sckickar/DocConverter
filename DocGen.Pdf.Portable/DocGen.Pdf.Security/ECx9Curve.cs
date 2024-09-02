using System;

namespace DocGen.Pdf.Security;

internal class ECx9Curve : Asn1Encode
{
	private readonly EllipticCurves m_curve;

	private readonly byte[] m_seed;

	private readonly DerObjectID m_fieldID;

	public EllipticCurves Curve => m_curve;

	public ECx9Curve(EllipticCurves curve, byte[] seed)
	{
		if (curve == null)
		{
			throw new ArgumentNullException("curve");
		}
		m_curve = curve;
		m_seed = Asn1Constants.Clone(seed);
		if (curve is FiniteCurves)
		{
			m_fieldID = ECDSAOIDs.X90UniqueID;
			return;
		}
		if (curve is Field2MCurves)
		{
			m_fieldID = ECDSAOIDs.X90RecordID;
			return;
		}
		throw new ArgumentException("EllipticCurves is not implemented");
	}

	public ECx9Curve(ECx9FieldObject fieldID, Asn1Sequence sequence)
	{
		if (fieldID == null)
		{
			throw new ArgumentNullException("fieldID");
		}
		if (sequence == null)
		{
			throw new ArgumentNullException("sequence");
		}
		m_fieldID = fieldID.Identifier;
		if (m_fieldID.Equals(ECDSAOIDs.X90UniqueID))
		{
			Number value = ((DerInteger)fieldID.Parameters).Value;
			ECx9FieldObjectID eCx9FieldObjectID = new ECx9FieldObjectID(value, (Asn1Octet)sequence[0]);
			ECx9FieldObjectID eCx9FieldObjectID2 = new ECx9FieldObjectID(value, (Asn1Octet)sequence[1]);
			m_curve = new FiniteCurves(value, eCx9FieldObjectID.Value.ToIntValue(), eCx9FieldObjectID2.Value.ToIntValue());
		}
		else if (m_fieldID.Equals(ECDSAOIDs.X90RecordID))
		{
			DerSequence derSequence = (DerSequence)fieldID.Parameters;
			int intValue = ((DerInteger)derSequence[0]).Value.IntValue;
			DerObjectID obj = (DerObjectID)derSequence[1];
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (obj.Equals(ECDSAOIDs.X90TNObjID))
			{
				num = ((DerInteger)derSequence[2]).Value.IntValue;
			}
			else
			{
				DerSequence obj2 = (DerSequence)derSequence[2];
				num = ((DerInteger)obj2[0]).Value.IntValue;
				num2 = ((DerInteger)obj2[1]).Value.IntValue;
				num3 = ((DerInteger)obj2[2]).Value.IntValue;
			}
			ECx9FieldObjectID eCx9FieldObjectID3 = new ECx9FieldObjectID(intValue, num, num2, num3, (Asn1Octet)sequence[0]);
			ECx9FieldObjectID eCx9FieldObjectID4 = new ECx9FieldObjectID(intValue, num, num2, num3, (Asn1Octet)sequence[1]);
			m_curve = new Field2MCurves(intValue, num, num2, num3, eCx9FieldObjectID3.Value.ToIntValue(), eCx9FieldObjectID4.Value.ToIntValue());
		}
		if (sequence.Count == 3)
		{
			m_seed = ((DerBitString)sequence[2]).GetBytes();
		}
	}

	public byte[] GetSeed()
	{
		return Asn1Constants.Clone(m_seed);
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		if (m_fieldID.Equals(ECDSAOIDs.X90UniqueID) || m_fieldID.Equals(ECDSAOIDs.X90RecordID))
		{
			asn1EncodeCollection.Add(new ECx9FieldObjectID(m_curve.ElementA).GetAsn1());
			asn1EncodeCollection.Add(new ECx9FieldObjectID(m_curve.ElementB).GetAsn1());
		}
		if (m_seed != null)
		{
			asn1EncodeCollection.Add(new DerBitString(m_seed));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
