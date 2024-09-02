using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation;

internal class WorkbookShapeDataImpl : CommonObject, ICloneParent
{
	protected class BlipParams
	{
		public int Instance;

		public byte ReqMac;

		public byte ReqWin32;

		public int SubRecordType;

		public BlipParams(int inst, byte mac, byte win32, int subrec)
		{
			Instance = inst;
			ReqMac = mac;
			ReqWin32 = win32;
			SubRecordType = subrec;
		}
	}

	private static readonly MsoBlipType[] METAFILEBLIPS;

	private List<MsofbtBSE> m_arrPictures = new List<MsofbtBSE>();

	private List<MsoBase> m_arrDGRecords = new List<MsoBase>();

	private WorkbookImpl m_book;

	private Dictionary<ArrayWrapper, MsofbtBSE> m_dicImageIdToImage = new Dictionary<ArrayWrapper, MsofbtBSE>();

	private WorkbookImpl.ShapesGetterMethod m_shapeGetter;

	private int m_iLastCollectionId;

	private static readonly Dictionary<MsoBlipType, BlipParams> s_hashBlipTypeToParams;

	private MsofbtDgg m_preservedDgg;

	private string[] m_indexedpixel_notsupport = new string[6] { "Format1bppIndexed", "Format4bppIndexed", "Format8bppIndexed", "b96b3cac-0728-11d3-9d7b-0000f81ef32e", "b96b3cad-0728-11d3-9d7b-0000f81ef32e", "b96b3caa-0728-11d3-9d7b-0000f81ef32e" };

	public List<MsofbtBSE> Pictures => m_arrPictures;

	protected bool NeedMsoDrawingGroup
	{
		get
		{
			_ = m_book.Objects;
			foreach (ShapeCollectionBase item in m_book.EnumerateShapes(m_shapeGetter))
			{
				if (item != null && item.Count > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal MsofbtDgg PreservedClusters => m_preservedDgg;

	static WorkbookShapeDataImpl()
	{
		METAFILEBLIPS = new MsoBlipType[3]
		{
			MsoBlipType.msoblipEMF,
			MsoBlipType.msoblipWMF,
			MsoBlipType.msoblipPICT
		};
		s_hashBlipTypeToParams = new Dictionary<MsoBlipType, BlipParams>();
		s_hashBlipTypeToParams.Add(MsoBlipType.msoblipEMF, new BlipParams(980, 4, 2, 61466));
		s_hashBlipTypeToParams.Add(MsoBlipType.msoblipWMF, new BlipParams(534, 4, 3, 61467));
		s_hashBlipTypeToParams.Add(MsoBlipType.msoblipPNG, new BlipParams(1760, 6, 6, 61470));
		s_hashBlipTypeToParams.Add(MsoBlipType.msoblipJPEG, new BlipParams(1130, 5, 5, 61469));
	}

	public WorkbookShapeDataImpl(IApplication application, object parent, WorkbookImpl.ShapesGetterMethod shapeGetter)
		: base(application, parent)
	{
		if (shapeGetter == null)
		{
			throw new ArgumentNullException("shapeGetter");
		}
		m_shapeGetter = shapeGetter;
		SetParents();
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("parent", "Can't find parent workbook");
		}
	}

	[CLSCompliant(false)]
	public void ParseDrawGroup(MSODrawingGroupRecord drawGroup)
	{
		if (drawGroup == null)
		{
			throw new ArgumentNullException("drawGroup");
		}
		List<MsoBase> itemsList = ((MsofbtDggContainer)drawGroup.StructuresList[0]).ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			MsoBase msoBase = itemsList[i];
			switch (msoBase.MsoRecordType)
			{
			case MsoRecords.msofbtBstoreContainer:
			{
				MsofbtBstoreContainer bStore = (MsofbtBstoreContainer)msoBase;
				ParsePictures(bStore);
				break;
			}
			case MsoRecords.msofbtDgg:
				m_preservedDgg = (MsofbtDgg)msoBase;
				break;
			default:
				m_arrDGRecords.Add(msoBase);
				break;
			case MsoRecords.msofbtOPT:
				break;
			}
		}
	}

	private void ParsePictures(MsofbtBstoreContainer bStore)
	{
		if (bStore == null)
		{
			throw new ArgumentNullException("bStore");
		}
		List<MsoBase> itemsList = bStore.ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			MsofbtBSE item = itemsList[i] as MsofbtBSE;
			m_arrPictures.Add(item);
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		m_arrDGRecords.Clear();
		m_arrPictures.Clear();
		m_arrDGRecords = null;
		m_arrPictures = null;
		m_dicImageIdToImage.Clear();
		m_dicImageIdToImage = null;
		if (m_preservedDgg != null)
		{
			m_preservedDgg.Dispose();
		}
		m_book = null;
		m_bIsDisposed = true;
	}

	[CLSCompliant(false)]
	public void SerializeMsoDrawingGroup(OffsetArrayList records, TBIFFRecord recordCode, IdReserver shapeIds)
	{
		bool needMsoDrawingGroup = NeedMsoDrawingGroup;
		if (!needMsoDrawingGroup && m_preservedDgg == null)
		{
			return;
		}
		MSODrawingGroupRecord mSODrawingGroupRecord = (MSODrawingGroupRecord)BiffRecordFactory.GetRecord(recordCode);
		MsofbtDggContainer msofbtDggContainer = new MsofbtDggContainer(null);
		MsofbtDgg msofbtDgg = new MsofbtDgg(msofbtDggContainer);
		msofbtDggContainer.AddItem(msofbtDgg);
		FillMsoDgg(msofbtDgg, m_shapeGetter, shapeIds);
		int num = ((m_arrPictures != null) ? m_arrPictures.Count : 0);
		if (num > 0)
		{
			MsofbtBstoreContainer msofbtBstoreContainer = new MsofbtBstoreContainer(msofbtDggContainer);
			msofbtDggContainer.AddItem(msofbtBstoreContainer);
			for (int i = 0; i < num; i++)
			{
				MsofbtBSE itemToAdd = m_arrPictures[i];
				msofbtBstoreContainer.AddItem(itemToAdd);
			}
		}
		if (needMsoDrawingGroup)
		{
			SerializeDrawingGroupOptions(msofbtDggContainer);
		}
		msofbtDggContainer.AddItems(m_arrDGRecords);
		mSODrawingGroupRecord.AddStructure(msofbtDggContainer);
		records.Add(mSODrawingGroupRecord);
	}

	private void SerializeDrawingGroupOptions(MsofbtDggContainer dggContainer)
	{
		MsofbtOPT msofbtOPT = new MsofbtOPT(dggContainer);
		SerializeDefaultOptions(msofbtOPT);
		dggContainer.AddItem(msofbtOPT);
	}

	private void SerializeDefaultOptions(MsofbtOPT options)
	{
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = MsoOptions.SizeTextToFitShape;
		fOPTE.IsComplex = false;
		fOPTE.IsValid = false;
		fOPTE.UInt32Value = 524296u;
		options.AddOptionsOrReplace(fOPTE);
		fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = MsoOptions.ForeColor;
		fOPTE.IsComplex = false;
		fOPTE.IsValid = false;
		fOPTE.UInt32Value = 134217793u;
		options.AddOptionsOrReplace(fOPTE);
		fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = MsoOptions.LineColor;
		fOPTE.IsComplex = false;
		fOPTE.IsValid = false;
		fOPTE.UInt32Value = 134217792u;
		options.AddOptionsOrReplace(fOPTE);
	}

	[CLSCompliant(false)]
	protected void FillMsoDgg(MsofbtDgg dgg, WorkbookImpl.ShapesGetterMethod shapeGetter, IdReserver shapeIds)
	{
		if (dgg == null)
		{
			throw new ArgumentNullException("dgg");
		}
		if (m_preservedDgg != null)
		{
			CopyData(m_preservedDgg, dgg);
			return;
		}
		uint num = 0u;
		uint num2 = 0u;
		uint num3 = 1024u;
		uint num4 = 0u;
		WorkbookObjectsCollection objects = m_book.Objects;
		int i = 0;
		for (int count = objects.Count; i < count; i++)
		{
			WorksheetBaseImpl sheet = objects[i] as WorksheetBaseImpl;
			ShapeCollectionBase shapeCollectionBase = shapeGetter(sheet);
			_ = shapeCollectionBase.Worksheet;
			if (shapeCollectionBase.ShapesCount != 0)
			{
				num4++;
				num2++;
				num += (uint)shapeCollectionBase.ShapesCount;
				uint shapesCount = (uint)shapeCollectionBase.ShapesCount;
				if (shapeIds == null)
				{
					dgg.AddCluster(num4, shapesCount);
				}
				uint num5 = num3 % 1024;
				if (num5 != 0)
				{
					num3 = num3 - num5 + 1024;
				}
				num3 += shapesCount;
			}
		}
		if (shapeIds != null)
		{
			int maximumId = shapeIds.MaximumId;
			num3 = (uint)maximumId;
			for (int j = 1024; j < maximumId; j += 1024)
			{
				int num6 = shapeIds.ReservedBy(j);
				int additionalShapesNumber = shapeIds.GetAdditionalShapesNumber(num6);
				dgg.AddCluster((uint)num6, (uint)additionalShapesNumber);
			}
		}
		dgg.TotalShapes = num;
		dgg.TotalDrawings = num2;
		dgg.IdMax = num3;
	}

	private void CopyData(MsofbtDgg source, MsofbtDgg destination)
	{
		MsofbtDgg.ClusterID[] clusterIDs = source.ClusterIDs;
		foreach (MsofbtDgg.ClusterID clusterID in clusterIDs)
		{
			destination.AddCluster(clusterID.GroupId, clusterID.Number);
		}
		destination.IdMax = source.IdMax;
		destination.TotalDrawings = source.TotalDrawings;
		destination.TotalShapes = source.TotalShapes;
	}

	public int AddPicture(Image image, ExcelImageFormat imageFormat, string strPictureName)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		MsofbtBSE msofbtBSE = new MsofbtBSE(null);
		msofbtBSE.BlipName = strPictureName;
		msofbtBSE.BlipType = ImageFormatToBlipType(image.RawFormat, imageFormat);
		msofbtBSE.BlipUsage = MsoBlipUsage.msoblipUsageDefault;
		BlipParams blipParams = GetBlipParams(msofbtBSE);
		msofbtBSE.RequiredMac = blipParams.ReqMac;
		byte instance = (msofbtBSE.RequiredWin32 = blipParams.ReqWin32);
		msofbtBSE.Instance = instance;
		msofbtBSE.Version = 2;
		msofbtBSE.RefCount = 1u;
		IPictureRecord pictureRecord2;
		if (!IsBitmapBlip(msofbtBSE.BlipType))
		{
			IPictureRecord pictureRecord = new MsoMetafilePicture(msofbtBSE);
			pictureRecord2 = pictureRecord;
		}
		else
		{
			IPictureRecord pictureRecord = new MsoBitmapPicture(msofbtBSE);
			pictureRecord2 = pictureRecord;
		}
		IPictureRecord pictureRecord3 = pictureRecord2;
		(pictureRecord3 as MsoBase).Instance = blipParams.Instance;
		(pictureRecord3 as MsoBase).MsoRecordType = (MsoRecords)blipParams.SubRecordType;
		pictureRecord3.Picture = image;
		msofbtBSE.PictureRecord = pictureRecord3;
		return AddPicture(msofbtBSE);
	}

	[CLSCompliant(false)]
	public int AddPicture(MsofbtBSE picture)
	{
		int num = (picture.Index = m_arrPictures.Count);
		m_arrPictures.Add(picture);
		return num + 1;
	}

	[CLSCompliant(false)]
	public MsofbtBSE GetPicture(int iPictureId)
	{
		return m_arrPictures[iPictureId - 1];
	}

	[CLSCompliant(false)]
	public void RemovePicture(uint id, bool removeImage)
	{
		if (id < 1 || id > m_arrPictures.Count)
		{
			return;
		}
		MsofbtBSE msofbtBSE = m_arrPictures[(int)(id - 1)];
		msofbtBSE.RefCount--;
		if (!(msofbtBSE.RefCount == 0 && removeImage))
		{
			return;
		}
		m_arrPictures.RemoveAt((int)(id - 1));
		ArrayWrapper key = new ArrayWrapper(msofbtBSE.PictureRecord.RgbUid);
		if (m_dicImageIdToImage.ContainsKey(key))
		{
			m_dicImageIdToImage.Remove(key);
		}
		int i = (int)(id - 1);
		for (int count = m_arrPictures.Count; i < count; i++)
		{
			m_arrPictures[i].Index--;
		}
		WorkbookObjectsCollection objects = m_book.Objects;
		int j = 0;
		for (int count2 = objects.Count; j < count2; j++)
		{
			WorksheetBaseImpl sheet = objects[j] as WorksheetBaseImpl;
			ShapeCollectionBase shapeCollectionBase = m_shapeGetter(sheet);
			int k = 0;
			for (int count3 = shapeCollectionBase.Count; k < count3; k++)
			{
				if (shapeCollectionBase[k] is BitmapShapeImpl bitmapShapeImpl && bitmapShapeImpl.BlipId > id)
				{
					bitmapShapeImpl.SetBlipId(bitmapShapeImpl.BlipId - 1);
				}
			}
		}
	}

	public void Clear()
	{
		if (m_dicImageIdToImage != null)
		{
			m_dicImageIdToImage.Clear();
		}
		if (m_arrDGRecords != null)
		{
			m_arrDGRecords.Clear();
		}
		if (m_arrPictures != null)
		{
			m_arrPictures.Clear();
		}
	}

	public object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		WorkbookShapeDataImpl obj = (WorkbookShapeDataImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.SetParents();
		obj.m_arrPictures = CloneUtils.CloneCloneable(m_arrPictures);
		obj.m_arrDGRecords = CloneUtils.CloneCloneable(m_arrDGRecords);
		obj.m_dicImageIdToImage = CloneUtils.CloneHash(m_dicImageIdToImage);
		return obj;
	}

	public int RegisterShapes()
	{
		m_iLastCollectionId++;
		return m_iLastCollectionId;
	}

	public void ClearPreservedClusters()
	{
		m_preservedDgg = null;
	}

	public static MsoBlipType ImageFormatToBlipType(ImageFormat format)
	{
		if (format.Equals(ImageFormat.Bmp))
		{
			return MsoBlipType.msoblipDIB;
		}
		if (format.Equals(ImageFormat.Jpeg))
		{
			return MsoBlipType.msoblipJPEG;
		}
		if (format.Equals(ImageFormat.Png))
		{
			return MsoBlipType.msoblipPNG;
		}
		if (format.Equals(ImageFormat.Emf))
		{
			return MsoBlipType.msoblipEMF;
		}
		return MsoBlipType.msoblipPNG;
	}

	public static MsoBlipType ImageFormatToBlipType(ImageFormat format, ExcelImageFormat imageFormat)
	{
		MsoBlipType result = ImageFormatToBlipType(format);
		if (imageFormat == ExcelImageFormat.Original)
		{
			return result;
		}
		return (MsoBlipType)imageFormat;
	}

	public static bool IsBitmapBlip(MsoBlipType blipType)
	{
		return Array.IndexOf(METAFILEBLIPS, blipType) == -1;
	}

	[CLSCompliant(false)]
	protected static BlipParams GetBlipParams(MsofbtBSE bse)
	{
		MsoBlipType blipType = bse.BlipType;
		if (!s_hashBlipTypeToParams.ContainsKey(blipType))
		{
			return s_hashBlipTypeToParams[MsoBlipType.msoblipPNG];
		}
		return s_hashBlipTypeToParams[blipType];
	}
}
