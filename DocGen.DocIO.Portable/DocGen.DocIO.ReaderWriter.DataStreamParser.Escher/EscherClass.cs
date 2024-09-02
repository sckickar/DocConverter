using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class EscherClass
{
	private const int DEF_MAIN_DRAWING_ID = 1;

	private const int DEF_HF_DRAWING_ID = 2;

	private const int DEF_MAIN_SPID = 1024;

	private const int DEF_HF_SPID = 2048;

	private const int DEF_MAIN_SPIDMAX = 2050;

	private const int DEF_HF_SPIDMAX = 3074;

	internal MsofbtDggContainer m_msofbtDggContainer;

	internal ContainerCollection m_dgContainers;

	private Dictionary<int, BaseContainer> m_containers;

	private MsofbtSpContainer m_backgroundContainer;

	private WordDocument m_doc;

	internal WordDocument Document => m_doc;

	internal Dictionary<int, BaseContainer> Containers
	{
		get
		{
			if (m_containers == null)
			{
				m_containers = new Dictionary<int, BaseContainer>();
			}
			return m_containers;
		}
	}

	internal MsofbtSpContainer BackgroundContainer
	{
		get
		{
			if (m_backgroundContainer == null)
			{
				m_backgroundContainer = GetBackgroundContainerValue();
			}
			return m_backgroundContainer;
		}
	}

	internal EscherClass(WordDocument doc)
	{
		m_doc = doc;
		m_dgContainers = new ContainerCollection(m_doc);
		CreateDefaultDgg();
	}

	internal EscherClass(Stream tableStream, Stream docStream, long dggInfoOffset, long dggInfoLength, WordDocument doc)
		: this(doc)
	{
		tableStream.Position = dggInfoOffset;
		if (dggInfoLength != 0L)
		{
			Read(tableStream, dggInfoLength, docStream);
		}
	}

	internal void Read(Stream tableStream, long dggInfoLength, Stream docStream)
	{
		long num = tableStream.Position + dggInfoLength;
		m_msofbtDggContainer = _MSOFBH.ReadHeaderWithRecord(tableStream, m_doc) as MsofbtDggContainer;
		if (m_msofbtDggContainer == null)
		{
			throw new ArgumentException("First Escher record is not DggContainer.");
		}
		while (tableStream.Position < num)
		{
			int shapeDocType = tableStream.ReadByte();
			if (!(_MSOFBH.ReadHeaderWithRecord(tableStream, m_doc) is MsofbtDgContainer msofbtDgContainer))
			{
				throw new ArgumentException("Expected DgContainer records only.");
			}
			msofbtDgContainer.ShapeDocType = (ShapeDocType)shapeDocType;
			m_dgContainers.Add(msofbtDgContainer);
		}
		FillCollectionForSearch();
		ReadContainersData(docStream);
	}

	internal void ReadContainersData(Stream Stream)
	{
		foreach (MsofbtDgContainer dgContainer in m_dgContainers)
		{
			ReadBseData(dgContainer, Stream);
		}
	}

	internal void WriteContainersData(Stream stream)
	{
		if (m_msofbtDggContainer == null || m_msofbtDggContainer.BstoreContainer == null)
		{
			return;
		}
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = m_msofbtDggContainer.BstoreContainer.Children.Count; i < count; i++)
		{
			baseEscherRecord = m_msofbtDggContainer.BstoreContainer.Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtBSE)
			{
				(baseEscherRecord as MsofbtBSE).Write(stream);
			}
			else
			{
				baseEscherRecord.WriteMsofbhWithRecord(stream);
			}
		}
	}

	internal uint WriteContainers(Stream stream)
	{
		if (m_msofbtDggContainer == null)
		{
			return 0u;
		}
		long position = stream.Position;
		InitWriting();
		WriteDggContainer(stream);
		WriteDgContainers(stream);
		return (uint)(stream.Position - position);
	}

	internal void AddContainerForSubDocument(WordSubdocument documentType, BaseEscherRecord baseEscherRecord)
	{
		if (m_msofbtDggContainer == null)
		{
			CreateDefaultDgg();
		}
		CreateDgForSubDocuments();
		ShapeDocType shapeDocType = ConvertToShapeDocType(documentType);
		FindDgContainerForSubDocType(shapeDocType).PatriarchGroupContainer.Children.Add(baseEscherRecord);
		AddParentContainer(baseEscherRecord as BaseContainer);
		FillCollectionForSearch(baseEscherRecord as BaseContainer);
	}

	internal MsofbtDgContainer FindDgContainerForSubDocType(ShapeDocType ShapeDocType)
	{
		foreach (MsofbtDgContainer dgContainer in m_dgContainers)
		{
			if (dgContainer.ShapeDocType == ShapeDocType)
			{
				return dgContainer;
			}
		}
		return null;
	}

	internal void InitShapeSpids()
	{
	}

	internal void RemoveHeaderContainer()
	{
		if (m_dgContainers.Count > 1)
		{
			if ((m_dgContainers[1] as MsofbtDgContainer).ShapeDocType != ShapeDocType.HeaderFooter)
			{
				throw new ArgumentException("Expected header drawing, but got something else.");
			}
			m_dgContainers.RemoveAt(1);
		}
	}

	internal BaseContainer FindParentContainer(BaseContainer baseContainer)
	{
		foreach (BaseContainer dgContainer in m_dgContainers)
		{
			BaseContainer baseContainer2 = dgContainer.FindParentContainer(baseContainer);
			if (baseContainer2 != null)
			{
				return baseContainer2;
			}
		}
		return null;
	}

	internal int GetTxid(int spid)
	{
		return FindInDgContainers(spid).Txid;
	}

	internal int GetShapeOrderIndex(int spId)
	{
		int result = -1;
		if (Containers != null && Containers.ContainsKey(spId) && Containers[spId] is MsofbtSpContainer)
		{
			int[] array = new int[Containers.Keys.Count];
			Containers.Keys.CopyTo(array, 0);
			result = Array.IndexOf(array, spId);
		}
		return result;
	}

	internal void SetTxid(int spid, int txid)
	{
		MsofbtSpContainer msofbtSpContainer = FindInDgContainers(spid);
		msofbtSpContainer.Txid = txid;
		if (msofbtSpContainer.FindContainerByMsofbt(MSOFBT.msofbtClientTextbox) is MsofbtClientTextbox msofbtClientTextbox)
		{
			msofbtClientTextbox.Txid = txid;
		}
	}

	internal BaseContainer FindContainerBySpid(int spid)
	{
		MsofbtSpContainer msofbtSpContainer = FindInDgContainers(spid);
		if (msofbtSpContainer == null)
		{
			return null;
		}
		if (msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptMin && msofbtSpContainer.Shape.IsGroup)
		{
			return FindParentContainer(msofbtSpContainer);
		}
		return msofbtSpContainer;
	}

	internal MsofbtSpContainer FindInDgContainers(int spid)
	{
		foreach (BaseContainer dgContainer in m_dgContainers)
		{
			MsofbtSpContainer msofbtSpContainer = FindContainerAmongChildren(dgContainer, spid);
			if (msofbtSpContainer != null)
			{
				return msofbtSpContainer;
			}
		}
		return null;
	}

	internal EscherShapeType GetBaseEscherRecordType(MsofbtSpContainer spCon)
	{
		return spCon.Shape.ShapeType;
	}

	internal int CloneContainerBySpid(WordDocument destDoc, WordSubdocument docType, int spid, int newSpid)
	{
		BaseContainer baseContainer = null;
		if (destDoc.Escher.Containers.ContainsKey(newSpid))
		{
			baseContainer = destDoc.Escher.Containers[newSpid];
		}
		while (baseContainer != null && destDoc.Escher.Containers.ContainsKey(newSpid))
		{
			baseContainer = destDoc.Escher.Containers[newSpid];
			newSpid++;
		}
		if (Containers.ContainsKey(spid))
		{
			BaseContainer baseContainer2 = (BaseContainer)Containers[spid].Clone();
			baseContainer2.SetSpid(newSpid);
			destDoc.Escher.AddContainerForSubDocument(docType, baseContainer2);
			baseContainer2.CloneRelationsTo(destDoc);
			return newSpid;
		}
		return -1;
	}

	internal void RemoveEscherOle()
	{
		foreach (BaseContainer dgContainer in m_dgContainers)
		{
			dgContainer.RemoveBaseContainerOle();
		}
	}

	internal void RemoveContainerBySpid(int spid, bool isHeaderContainer)
	{
		MsofbtDgContainer msofbtDgContainer = FindDgContainerForSubDocType(isHeaderContainer ? ShapeDocType.HeaderFooter : ShapeDocType.Main);
		bool flag = false;
		BaseEscherRecord baseEscherRecord = null;
		Containers.Remove(spid);
		int i = 0;
		for (int count = msofbtDgContainer.Children.Count; i < count; i++)
		{
			if (flag)
			{
				break;
			}
			baseEscherRecord = msofbtDgContainer.Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSpContainer)
			{
				if ((baseEscherRecord as MsofbtSpContainer).Shape.ShapeId == spid)
				{
					msofbtDgContainer.Children.Remove(baseEscherRecord);
					flag = true;
				}
			}
			else if (baseEscherRecord is BaseContainer)
			{
				flag = BaseContainer.RemoveContainerBySpid(baseEscherRecord as BaseContainer, spid);
			}
		}
	}

	internal void RemoveBStoreByPid(int pib)
	{
		foreach (BaseEscherRecord child in m_msofbtDggContainer.Children)
		{
			if (child is MsofbtBstoreContainer && pib <= (child as MsofbtBstoreContainer).Children.Count)
			{
				(child as MsofbtBstoreContainer).Children.RemoveAt(pib - 1);
				break;
			}
		}
	}

	internal void ModifyBStoreByPid(int pib, MsofbtBSE bse)
	{
		foreach (BaseEscherRecord child in m_msofbtDggContainer.Children)
		{
			if (child is MsofbtBstoreContainer && pib <= (child as MsofbtBstoreContainer).Children.Count)
			{
				((child as MsofbtBstoreContainer).Children[pib - 1] as MsofbtBSE).Blip.ImageRecord = bse.Blip.ImageRecord;
				break;
			}
		}
	}

	internal MsofbtSpContainer GetBackgroundContainerValue()
	{
		MsofbtDgContainer msofbtDgContainer = FindDgContainerForSubDocType(ShapeDocType.Main);
		if (msofbtDgContainer != null)
		{
			foreach (BaseEscherRecord child in msofbtDgContainer.Children)
			{
				if (child is MsofbtSpContainer)
				{
					return child as MsofbtSpContainer;
				}
			}
		}
		return null;
	}

	internal bool CheckBStoreContByPid(int pib)
	{
		bool result = false;
		if (m_msofbtDggContainer.BstoreContainer != null)
		{
			MsofbtBstoreContainer bstoreContainer = m_msofbtDggContainer.BstoreContainer;
			if (bstoreContainer.Children.Count >= pib && ((MsofbtBSE)bstoreContainer.Children[pib - 1]).Blip != null)
			{
				result = true;
			}
		}
		return result;
	}

	private void ReadBseData(BaseContainer baseContainer, Stream stream)
	{
		for (int i = 0; i < baseContainer.Children.Count; i++)
		{
			BaseEscherRecord baseEscherRecord = baseContainer.Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSpContainer)
			{
				MsofbtSpContainer msofbtSpContainer = baseEscherRecord as MsofbtSpContainer;
				if (msofbtSpContainer.Shape != null)
				{
					int blipId = GetBlipId(msofbtSpContainer);
					if (blipId >= 0)
					{
						MsofbtBSE msofbtBSE = m_msofbtDggContainer.BstoreContainer.Children[blipId] as MsofbtBSE;
						if (msofbtBSE.Fbse.m_size == 0)
						{
							continue;
						}
						msofbtBSE.Read(stream);
						msofbtSpContainer.Bse = msofbtBSE;
					}
				}
			}
			if (baseEscherRecord is BaseContainer)
			{
				ReadBseData(baseEscherRecord as BaseContainer, stream);
			}
		}
	}

	private void InitWriting()
	{
		MsofbtDgg dgg = m_msofbtDggContainer.Dgg;
		int num = 0;
		dgg.Fidcls.Clear();
		for (int i = 0; i < m_dgContainers.Count; i++)
		{
			MsofbtDgContainer msofbtDgContainer = m_dgContainers[i] as MsofbtDgContainer;
			msofbtDgContainer.InitWriting();
			if (msofbtDgContainer.ShapeDocType == ShapeDocType.HeaderFooter && msofbtDgContainer.Dg.ShapeCount <= 1)
			{
				RemoveContainerBySpid(2048, isHeaderContainer: true);
				m_dgContainers.RemoveAt(i);
				i--;
				num--;
			}
			else
			{
				int shapeCount = GetShapeCount(msofbtDgContainer.PatriarchGroupContainer);
				m_msofbtDggContainer.Dgg.Fidcls.Add(new FIDCL(msofbtDgContainer.Dg.DrawingId, shapeCount + 1));
				num += shapeCount + 1;
			}
		}
		dgg.DrawingCount = m_dgContainers.Count;
		dgg.ShapeCount = num;
		dgg.SpidMax = (m_dgContainers.Count + 1) * 1024 + 2;
	}

	private void WriteDggContainer(Stream stream)
	{
		m_msofbtDggContainer.WriteMsofbhWithRecord(stream);
	}

	private void WriteDgContainers(Stream stream)
	{
		foreach (MsofbtDgContainer dgContainer in m_dgContainers)
		{
			stream.WriteByte((byte)dgContainer.ShapeDocType);
			dgContainer.WriteMsofbhWithRecord(stream);
		}
	}

	internal void CreateDgForSubDocuments()
	{
		if (FindDgContainerForSubDocType(ShapeDocType.Main) == null)
		{
			CreateDgForSubDocument(ShapeDocType.Main, 1, 1024);
		}
		if (FindDgContainerForSubDocType(ShapeDocType.HeaderFooter) == null)
		{
			CreateDgForSubDocument(ShapeDocType.HeaderFooter, 2, 2048);
		}
	}

	private void CreateDefaultDgg()
	{
		m_msofbtDggContainer = new MsofbtDggContainer(m_doc);
		MsofbtDgg item = new MsofbtDgg(m_doc);
		m_msofbtDggContainer.Children.Add(item);
		m_msofbtDggContainer.Children.Add(new MsofbtBstoreContainer(m_doc));
	}

	private void CreateDgForSubDocument(ShapeDocType shapeDocType, int drawingId, int shapeId)
	{
		MsofbtDgContainer msofbtDgContainer = new MsofbtDgContainer(m_doc);
		msofbtDgContainer.ShapeDocType = shapeDocType;
		m_dgContainers.Add(msofbtDgContainer);
		MsofbtDg msofbtDg = new MsofbtDg(m_doc);
		msofbtDg.DrawingId = drawingId;
		msofbtDg.ShapeCount = 1;
		msofbtDg.SpidLast = shapeId;
		msofbtDgContainer.Children.Add(msofbtDg);
		MsofbtSpgrContainer msofbtSpgrContainer = new MsofbtSpgrContainer(m_doc);
		msofbtDgContainer.Children.Add(msofbtSpgrContainer);
		MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(m_doc);
		msofbtSpgrContainer.Children.Add(msofbtSpContainer);
		MsofbtSpgr item = new MsofbtSpgr(m_doc);
		msofbtSpContainer.Children.Add(item);
		MsofbtSp msofbtSp = new MsofbtSp(m_doc);
		msofbtSp.ShapeId = shapeId;
		msofbtSp.IsGroup = true;
		msofbtSp.IsPatriarch = true;
		msofbtSp.ShapeType = EscherShapeType.msosptMin;
		msofbtSpContainer.Children.Add(msofbtSp);
		Containers.Add(msofbtSpContainer.Shape.ShapeId, msofbtSpContainer);
		if (shapeDocType == ShapeDocType.Main)
		{
			MsofbtSpContainer msofbtSpContainer2 = new MsofbtSpContainer(m_doc);
			msofbtDgContainer.Children.Add(msofbtSpContainer2.CreateRectangleContainer());
			Containers.Add(msofbtSpContainer2.Shape.ShapeId, msofbtSpContainer);
		}
		msofbtDgContainer.Children.Add(new MsofbtSolverContainer(m_doc));
	}

	private static MsofbtSpContainer FindContainerAmongChildren(BaseContainer parentContainer, int spid)
	{
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = parentContainer.Children.Count; i < count; i++)
		{
			baseEscherRecord = parentContainer.Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSp)
			{
				if ((baseEscherRecord as MsofbtSp).ShapeId == spid)
				{
					return parentContainer as MsofbtSpContainer;
				}
			}
			else if (baseEscherRecord is BaseContainer)
			{
				MsofbtSpContainer msofbtSpContainer = FindContainerAmongChildren(baseEscherRecord as BaseContainer, spid);
				if (msofbtSpContainer != null)
				{
					return msofbtSpContainer;
				}
			}
		}
		return null;
	}

	private void AddShapeBse(MsofbtSpContainer spContainer)
	{
		if ((!spContainer.IsWatermark || spContainer.Pib == -1) && spContainer.Bse != null)
		{
			if (m_msofbtDggContainer.BstoreContainer == null)
			{
				m_msofbtDggContainer.Children.Add(new MsofbtBstoreContainer(m_doc));
			}
			m_msofbtDggContainer.BstoreContainer.Children.Add(spContainer.Bse);
			if (spContainer.Shape.ShapeType == EscherShapeType.msosptPictureFrame)
			{
				spContainer.Pib = m_msofbtDggContainer.BstoreContainer.Children.Count;
			}
			else if (spContainer.ShapeOptions.Properties[390] is FOPTEBid fOPTEBid)
			{
				fOPTEBid.Value = (uint)m_msofbtDggContainer.BstoreContainer.Children.Count;
			}
		}
	}

	internal static ShapeDocType ConvertToShapeDocType(WordSubdocument docType)
	{
		return docType switch
		{
			WordSubdocument.Main => ShapeDocType.Main, 
			WordSubdocument.HeaderFooter => ShapeDocType.HeaderFooter, 
			_ => throw new Exception("Windows.Media for " + docType.ToString() + " document is not available"), 
		};
	}

	private void AddParentContainer(BaseContainer baseContainer)
	{
		if (baseContainer is MsofbtSpContainer)
		{
			AddShapeBse(baseContainer as MsofbtSpContainer);
		}
		for (int i = 0; i < baseContainer.Children.Count; i++)
		{
			BaseEscherRecord baseEscherRecord = baseContainer.Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSpContainer)
			{
				AddShapeBse(baseEscherRecord as MsofbtSpContainer);
			}
			else if (baseEscherRecord is BaseContainer)
			{
				AddParentContainer(baseEscherRecord as BaseContainer);
			}
		}
	}

	private MsofbtGeneral CreateGeneralData()
	{
		MsofbtGeneral msofbtGeneral = new MsofbtGeneral(m_doc);
		msofbtGeneral.Data = new byte[16]
		{
			255, 255, 0, 0, 0, 0, 255, 0, 128, 128,
			128, 0, 247, 0, 0, 16
		};
		return msofbtGeneral;
	}

	private void InitFidcl(int dgid, int cspidCur)
	{
		FIDCL fIDCL = new FIDCL(dgid, cspidCur);
		if (m_msofbtDggContainer.Dgg.Fidcls.Count == 0 || !FindFIDCLDgid(dgid, fIDCL))
		{
			m_msofbtDggContainer.Dgg.Fidcls.Add(fIDCL);
		}
	}

	private bool FindFIDCLDgid(int dgid, FIDCL fidclObj)
	{
		int num = 0;
		foreach (FIDCL fidcl in m_msofbtDggContainer.Dgg.Fidcls)
		{
			if (fidcl.m_dgid == dgid)
			{
				m_msofbtDggContainer.Dgg.Fidcls[num] = fidclObj;
				return true;
			}
			num++;
		}
		return false;
	}

	private int GetShapeCount(BaseContainer baseContainer)
	{
		int num = 0;
		foreach (BaseEscherRecord child in baseContainer.Children)
		{
			if (child is MsofbtSpContainer)
			{
				num++;
			}
			else if (child is BaseContainer)
			{
				num += GetShapeCount(child as BaseContainer);
			}
		}
		return num;
	}

	private int GetBlipId(MsofbtSpContainer spContainer)
	{
		uint propertyValue = spContainer.GetPropertyValue(390);
		if (propertyValue != uint.MaxValue)
		{
			return (int)(propertyValue - 1);
		}
		uint propertyValue2 = spContainer.GetPropertyValue(260);
		if (propertyValue2 != uint.MaxValue)
		{
			return (int)(propertyValue2 - 1);
		}
		return -1;
	}

	private void FillCollectionForSearch()
	{
		foreach (MsofbtDgContainer dgContainer in m_dgContainers)
		{
			FillCollectionForSearch(dgContainer);
		}
	}

	internal void FillCollectionForSearch(BaseContainer baseContainer)
	{
		if (baseContainer is MsofbtSpContainer)
		{
			AddSpContToSearchCol(baseContainer as MsofbtSpContainer);
			return;
		}
		foreach (BaseEscherRecord child in baseContainer.Children)
		{
			if (child is BaseContainer)
			{
				FillCollectionForSearch(child as BaseContainer);
			}
			if (child is MsofbtSpgrContainer)
			{
				MsofbtSpgrContainer msofbtSpgrContainer = child as MsofbtSpgrContainer;
				if (!Containers.ContainsKey(msofbtSpgrContainer.Shape.ShapeId))
				{
					Containers.Add(msofbtSpgrContainer.Shape.ShapeId, msofbtSpgrContainer);
				}
			}
		}
	}

	private void AddSpContToSearchCol(MsofbtSpContainer spContainer)
	{
		if (!Containers.ContainsKey(spContainer.Shape.ShapeId))
		{
			if (spContainer.Shape.IsGroup)
			{
				Containers.Add(spContainer.Shape.ShapeId, FindParentContainer(spContainer));
			}
			else
			{
				Containers.Add(spContainer.Shape.ShapeId, spContainer);
			}
		}
	}

	internal void Close()
	{
		if (m_containers != null)
		{
			m_containers.Clear();
			m_containers = null;
		}
		m_doc = null;
		int i = 0;
		for (int count = m_dgContainers.Count; i < count; i++)
		{
			(m_dgContainers[i] as MsofbtDgContainer).Close();
		}
		if (m_msofbtDggContainer != null)
		{
			m_msofbtDggContainer.Close();
			m_msofbtDggContainer = null;
		}
		if (m_backgroundContainer != null)
		{
			m_backgroundContainer.Close();
			m_backgroundContainer = null;
		}
	}
}
