using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class BaseContainer : BaseEscherRecord
{
	private ContainerCollection m_childrenContainers;

	internal ContainerCollection Children => m_childrenContainers;

	internal BaseContainer(WordDocument doc)
		: base(doc)
	{
		m_childrenContainers = new ContainerCollection(doc);
	}

	internal BaseContainer(MSOFBT type, WordDocument doc)
		: base(doc)
	{
		m_childrenContainers = new ContainerCollection(doc);
		base.Header.IsContainer = true;
		base.Header.Type = type;
	}

	internal static bool RemoveContainerBySpid(BaseContainer baseContainer, int spid)
	{
		bool flag = false;
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = baseContainer.Children.Count; i < count; i++)
		{
			baseEscherRecord = baseContainer.Children[i] as BaseEscherRecord;
			if (flag)
			{
				break;
			}
			if (baseEscherRecord is MsofbtSpContainer)
			{
				if ((baseEscherRecord as MsofbtSpContainer).Shape.ShapeId == spid)
				{
					baseContainer.Children.Remove(baseEscherRecord);
					flag = true;
					break;
				}
			}
			else if (baseEscherRecord is BaseContainer)
			{
				flag = RemoveContainerBySpid(baseEscherRecord as BaseContainer, spid);
			}
		}
		return flag;
	}

	internal void SynchronizeIdent(WTextBoxCollection autoShapeCollection, ref int txbxShapeId, ref int pictShapeId, ref int txId, ref int textColIndex)
	{
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			baseEscherRecord = Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSp)
			{
				MsofbtSp msofbtSp = baseEscherRecord as MsofbtSp;
				SyncSpRecord(msofbtSp, autoShapeCollection, ref txbxShapeId, ref pictShapeId, ref textColIndex);
			}
			if (baseEscherRecord is MsofbtOPT)
			{
				SyncOPTTxid(baseEscherRecord as MsofbtOPT, ref txId);
			}
			if (baseEscherRecord is MsofbtClientTextbox)
			{
				(baseEscherRecord as MsofbtClientTextbox).Txid = txId;
			}
			if (baseEscherRecord is BaseContainer)
			{
				(baseEscherRecord as BaseContainer).SynchronizeIdent(autoShapeCollection, ref txbxShapeId, ref pictShapeId, ref txId, ref textColIndex);
			}
		}
	}

	internal int GetSpid()
	{
		int num = 0;
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			baseEscherRecord = Children[i] as BaseEscherRecord;
			if (num != 0)
			{
				break;
			}
			if (baseEscherRecord is MsofbtSp)
			{
				num = (baseEscherRecord as MsofbtSp).ShapeId;
				break;
			}
			if (baseEscherRecord is BaseContainer)
			{
				num = (baseEscherRecord as BaseContainer).GetSpid();
			}
		}
		return num;
	}

	internal bool SetSpid(int spid)
	{
		bool flag = false;
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			baseEscherRecord = Children[i] as BaseEscherRecord;
			if (flag)
			{
				break;
			}
			if (baseEscherRecord is MsofbtSp)
			{
				(baseEscherRecord as MsofbtSp).ShapeId = spid;
				flag = true;
				break;
			}
			if (baseEscherRecord is BaseContainer)
			{
				flag = (baseEscherRecord as BaseContainer).SetSpid(spid);
			}
		}
		return flag;
	}

	internal BaseEscherRecord FindContainerByMsofbt(MSOFBT msofbt)
	{
		for (int i = 0; i < m_childrenContainers.Count; i++)
		{
			BaseEscherRecord baseEscherRecord = m_childrenContainers[i] as BaseEscherRecord;
			if (baseEscherRecord.Header.Type == msofbt)
			{
				return baseEscherRecord;
			}
		}
		return null;
	}

	internal BaseEscherRecord FindContainerByType(Type type)
	{
		for (int i = 0; i < m_childrenContainers.Count; i++)
		{
			BaseEscherRecord baseEscherRecord = m_childrenContainers[i] as BaseEscherRecord;
			if (baseEscherRecord.GetType() == type)
			{
				return baseEscherRecord;
			}
		}
		return null;
	}

	internal BaseContainer FindParentContainer(BaseContainer baseContainer)
	{
		for (int i = 0; i < m_childrenContainers.Count; i++)
		{
			BaseContainer baseContainer2 = m_childrenContainers[i] as BaseContainer;
			if (baseContainer2 == baseContainer)
			{
				return this;
			}
			if (baseContainer2 != null)
			{
				BaseContainer baseContainer3 = baseContainer2.FindParentContainer(baseContainer);
				if (baseContainer3 != null)
				{
					return baseContainer3;
				}
			}
		}
		return null;
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_childrenContainers.Read(stream, base.Header.Length);
	}

	protected override void WriteRecordData(Stream stream)
	{
		m_childrenContainers.Write(stream);
	}

	internal override BaseEscherRecord Clone()
	{
		_ = base.Header.Type;
		BaseEscherRecord baseEscherRecord = base.Header.CreateRecordFromHeader();
		foreach (BaseEscherRecord child in Children)
		{
			child.Header.CreateRecordFromHeader().Header = child.Header.Clone();
			(baseEscherRecord as BaseContainer).Children.Add(child.Clone());
		}
		baseEscherRecord.m_doc = m_doc;
		return baseEscherRecord;
	}

	internal virtual void CloneRelationsTo(WordDocument doc)
	{
	}

	internal void RemoveBaseContainerOle()
	{
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			baseEscherRecord = Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSpContainer)
			{
				if ((baseEscherRecord as MsofbtSpContainer).Shape.IsOle)
				{
					(baseEscherRecord as MsofbtSpContainer).RemoveSpContainerOle();
				}
			}
			else if (baseEscherRecord is BaseContainer)
			{
				(baseEscherRecord as BaseContainer).RemoveBaseContainerOle();
			}
		}
	}

	private void SyncOPTTxid(MsofbtOPT optRecord, ref int txId)
	{
		int num = txId;
		if (optRecord.Properties.ContainsKey(128))
		{
			FOPTEBid obj = optRecord.Properties[128] as FOPTEBid;
			txId += 65536;
			obj.Value = (uint)txId;
		}
		if (optRecord.Properties.ContainsKey(267) && (this as MsofbtSpContainer).Shape.ShapeType == EscherShapeType.msosptHostControl)
		{
			FOPTEBid obj2 = optRecord.Properties[267] as FOPTEBid;
			if (num == txId)
			{
				txId += 65536;
			}
			obj2.Value = (uint)txId;
		}
		if (optRecord.Properties.ContainsKey(138))
		{
			_ = optRecord.Properties[138];
			optRecord.Properties.Remove(138);
		}
	}

	private void SyncSpRecord(MsofbtSp msofbtSp, WTextBoxCollection autoShapeCollection, ref int txbxShapeId, ref int pictShapeId, ref int textColIndex)
	{
		if (msofbtSp.ShapeType == EscherShapeType.msosptPictureFrame)
		{
			msofbtSp.ShapeId = pictShapeId;
			pictShapeId++;
			return;
		}
		msofbtSp.ShapeId = txbxShapeId;
		txbxShapeId++;
		if (autoShapeCollection != null && this is MsofbtSpContainer && (this as MsofbtSpContainer).ShapeOptions != null && ((this as MsofbtSpContainer).ShapeOptions.Txid != null || (this as MsofbtSpContainer).Shape.ShapeType == EscherShapeType.msosptHostControl) && autoShapeCollection.Count > 0)
		{
			(autoShapeCollection[textColIndex] as WTextBox).TextBoxSpid = msofbtSp.ShapeId;
			textColIndex++;
		}
	}

	internal override void Close()
	{
		if (m_childrenContainers == null || m_childrenContainers.Count == 0)
		{
			m_childrenContainers = null;
			return;
		}
		object obj = null;
		int i = 0;
		for (int count = m_childrenContainers.Count; i < count; i++)
		{
			obj = m_childrenContainers[i];
			if (obj is BaseContainer)
			{
				(obj as BaseContainer).Close();
			}
			else if (obj is BaseEscherRecord)
			{
				(obj as BaseEscherRecord).Close();
			}
			else if (obj is BaseWordRecord)
			{
				(obj as BaseWordRecord).Close();
			}
		}
	}
}
