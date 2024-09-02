using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

namespace DocGen.DocIO.DLS;

public class WControlField : WField
{
	private int m_storagePicLocation;

	private OLEObject m_oleObject;

	public override EntityType EntityType => EntityType.ControlField;

	internal int StoragePicLocation
	{
		get
		{
			return m_storagePicLocation;
		}
		set
		{
			m_storagePicLocation = value;
		}
	}

	internal OLEObject OleObject
	{
		get
		{
			if (m_oleObject == null)
			{
				m_oleObject = new OLEObject();
			}
			return m_oleObject;
		}
	}

	internal WControlField(IWordDocument doc)
		: base(doc)
	{
		m_paraItemType = ParagraphItemType.ControlField;
	}

	protected override object CloneImpl()
	{
		WControlField wControlField = (WControlField)base.CloneImpl();
		if (m_oleObject != null)
		{
			wControlField.m_oleObject = m_oleObject.Clone();
			wControlField.m_storagePicLocation = WOleObject.NextOleObjId;
			wControlField.m_oleObject.Storage.StorageName = m_storagePicLocation.ToString();
		}
		return wControlField;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if (doc != null)
		{
			string text = OleObject.AddOleObjectToCollection(doc.OleObjectCollection, m_storagePicLocation.ToString());
			if (!string.IsNullOrEmpty(text))
			{
				OleObject.Storage.StorageName = text;
			}
		}
	}

	internal override void Close()
	{
		if (m_oleObject != null)
		{
			m_oleObject.Close();
			m_oleObject = null;
		}
		base.Close();
	}
}
