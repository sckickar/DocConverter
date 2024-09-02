namespace DocGen.Office;

internal class ReferenceRecordsCollection : CollectionBase<ReferenceRecord>
{
	private VbaProject m_parent;

	internal ReferenceRecord Add(VbaReferenceType type)
	{
		ReferenceRecord referenceRecord = null;
		switch (type)
		{
		case VbaReferenceType.ReferenceControl:
			referenceRecord = new ReferenceControlRecord();
			break;
		case VbaReferenceType.ReferenceOriginal:
			referenceRecord = new ReferenceOriginalRecord();
			break;
		case VbaReferenceType.ReferenceProject:
			referenceRecord = new ReferenceProjectRecord();
			break;
		case VbaReferenceType.ReferenceRegister:
			referenceRecord = new ReferenceRegisterRecord();
			break;
		}
		Add(referenceRecord);
		return referenceRecord;
	}

	internal void Dispose()
	{
		Clear();
	}

	internal ReferenceRecordsCollection Clone(VbaProject parent)
	{
		ReferenceRecordsCollection referenceRecordsCollection = (ReferenceRecordsCollection)Clone();
		referenceRecordsCollection.m_parent = parent;
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			ReferenceRecord referenceRecord = base.InnerList[i];
			referenceRecordsCollection.Add(referenceRecord.Clone());
		}
		return referenceRecordsCollection;
	}
}
