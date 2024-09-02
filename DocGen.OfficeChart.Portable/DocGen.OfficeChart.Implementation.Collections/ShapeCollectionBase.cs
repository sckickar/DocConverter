using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Implementation.Collections;

internal abstract class ShapeCollectionBase : CollectionBaseEx<IShape>
{
	public const int DEF_ID_PER_GROUP = 1024;

	public const int DEF_SHAPES_ROUND_VALUE = 1024;

	private MsofbtSpContainer m_groupInfo;

	protected WorksheetBaseImpl m_sheet;

	private int m_iCollectionIndex;

	private int m_iLastId;

	private int m_iStartId;

	private List<MsofbtRegroupItems> m_arrRegroundItems = new List<MsofbtRegroupItems>();

	private Stream m_layoutStream;

	public int ShapesCount
	{
		get
		{
			int count = base.Count;
			if (count <= 0)
			{
				return 0;
			}
			return count + 1;
		}
	}

	public int ShapesTotalCount
	{
		get
		{
			int num = 0;
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				ShapeImpl shapeImpl = (ShapeImpl)this[i];
				num += shapeImpl.ShapeCount;
			}
			if (num <= 0)
			{
				return 0;
			}
			return num + 1;
		}
	}

	public WorksheetBaseImpl WorksheetBase => m_sheet;

	public WorksheetImpl Worksheet => m_sheet as WorksheetImpl;

	public WorkbookImpl Workbook => m_sheet.ParentWorkbook;

	public new IShape this[int index] => base.List[index];

	public IShape this[string strShapeName]
	{
		get
		{
			if (strShapeName == null)
			{
				throw new ArgumentNullException("strShapeName");
			}
			if (strShapeName.Length == 0)
			{
				throw new ArgumentException("Shape name cannot be null.");
			}
			IList<IShape> list = base.List;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				IShape shape = list[i];
				if (shape.Name == strShapeName)
				{
					return shape;
				}
			}
			return null;
		}
	}

	internal Stream ShapeLayoutStream
	{
		get
		{
			return m_layoutStream;
		}
		set
		{
			m_layoutStream = value;
		}
	}

	public abstract TBIFFRecord RecordCode { get; }

	public abstract WorkbookShapeDataImpl ShapeData { get; }

	internal int CollectionIndex
	{
		get
		{
			return m_iCollectionIndex;
		}
		set
		{
			m_iCollectionIndex = value;
		}
	}

	internal int LastId
	{
		get
		{
			return m_iLastId;
		}
		set
		{
			m_iLastId = value;
		}
	}

	internal int StartId
	{
		get
		{
			return m_iStartId;
		}
		set
		{
			m_iStartId = value;
		}
	}

	public ShapeCollectionBase(IApplication application, object parent)
		: base(application, parent)
	{
		InitializeCollection();
	}

	[CLSCompliant(false)]
	public ShapeCollectionBase(IApplication application, object parent, MsofbtSpgrContainer container, OfficeParseOptions options)
		: this(application, parent)
	{
		Parse(container, options);
	}

	protected virtual void InitializeCollection()
	{
		SetParents();
	}

	protected void SetParents()
	{
		m_sheet = FindParent(typeof(WorksheetBaseImpl), bCheckSubclasses: true) as WorksheetBaseImpl;
		if (m_sheet == null)
		{
			throw new ArgumentNullException("Can't find parent worksheet.");
		}
	}

	private void Parse(MsofbtSpgrContainer container, OfficeParseOptions options)
	{
		List<MsoBase> itemsList = container.ItemsList;
		MsofbtSpContainer groupDescription = (MsofbtSpContainer)itemsList[0];
		ParseGroupDescription(groupDescription);
		int i = 1;
		for (int count = itemsList.Count; i < count; i++)
		{
			AddShape(itemsList[i], options);
		}
	}

	private void ParseGroupDescription(MsofbtSpContainer groupDescription)
	{
		m_groupInfo = (MsofbtSpContainer)groupDescription.Clone();
		if (m_groupInfo.Items[1] is MsofbtSp msofbtSp)
		{
			m_iStartId = msofbtSp.ShapeId;
		}
	}

	public void ParseMsoStructures(List<MsoBase> arrStructures, OfficeParseOptions options)
	{
		int i = 0;
		for (int count = arrStructures.Count; i < count; i++)
		{
			MsoBase msoBase = arrStructures[i];
			if (msoBase.MsoRecordType == MsoRecords.msofbtDgContainer)
			{
				ParseMsoDgContainer((MsofbtDgContainer)msoBase, options);
				continue;
			}
			throw new ArgumentOutOfRangeException("Unexcpected MsoDrawing record");
		}
	}

	private void ParseMsoDgContainer(MsofbtDgContainer dgContainer, OfficeParseOptions options)
	{
		List<MsoBase> itemsList = dgContainer.ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			MsoBase msoBase = itemsList[i];
			switch (msoBase.MsoRecordType)
			{
			case MsoRecords.msofbtDg:
				ParseMsoDg((MsofbtDg)msoBase);
				break;
			case MsoRecords.msofbtSpgrContainer:
				Parse((MsofbtSpgrContainer)msoBase, options);
				break;
			case MsoRecords.msofbtRegroupItems:
				m_arrRegroundItems.Add((MsofbtRegroupItems)msoBase);
				break;
			}
		}
	}

	private void ParseMsoDg(MsofbtDg dgRecord)
	{
		CollectionIndex = dgRecord.Instance;
		m_iLastId = dgRecord.LastId;
	}

	public IShape AddCopy(ShapeImpl sourceShape)
	{
		return AddCopy(sourceShape, null, null);
	}

	public IShape AddCopy(ShapeImpl sourceShape, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes)
	{
		IShape shape = sourceShape.Clone(this, hashNewNames, dicFontIndexes, addToCollections: true);
		(shape as ShapeImpl).ShapeId = 0;
		return shape;
	}

	public IShape AddCopy(IShape sourceShape)
	{
		return AddCopy((ShapeImpl)sourceShape);
	}

	public IShape AddCopy(IShape sourceShape, Dictionary<string, string> hashNewNames, List<int> arrFontIndexes)
	{
		return AddCopy((ShapeImpl)sourceShape, hashNewNames, arrFontIndexes);
	}

	public ShapeImpl AddShape(ShapeImpl newShape)
	{
		if (newShape == null)
		{
			throw new ArgumentNullException("newShape");
		}
		base.Add(newShape);
		return newShape;
	}

	[CLSCompliant(false)]
	protected ShapeImpl AddShape(MsoBase shape, OfficeParseOptions options)
	{
		if (shape is MsofbtSpContainer)
		{
			return AddShape(shape as MsofbtSpContainer, options);
		}
		if (shape is MsofbtSpgrContainer)
		{
			return AddGroupShape(shape as MsofbtSpgrContainer, options);
		}
		ShapeImpl newShape = new ShapeImpl(base.Application, this, shape, options);
		return AddShape(newShape);
	}

	protected ShapeImpl AddGroupShape(MsofbtSpgrContainer shapes, OfficeParseOptions options)
	{
		ShapeImpl newShape = CreateGroupShape(shapes, options);
		return AddShape(newShape);
	}

	protected ShapeImpl CreateGroupShape(MsofbtSpgrContainer shapes, OfficeParseOptions options)
	{
		ShapeImpl shapeImpl = new ShapeImpl(base.Application, this, shapes, options);
		foreach (MsoBase items in shapes.ItemsList)
		{
			if (items is MsofbtSpContainer)
			{
				shapeImpl.ChildShapes.Add(AddChildShapes(items as MsofbtSpContainer, options));
			}
			else if (items is MsofbtSpgrContainer)
			{
				shapeImpl.ChildShapes.Add(CreateGroupShape(items as MsofbtSpgrContainer, options));
			}
			else
			{
				shapeImpl.ChildShapes.Add(new ShapeImpl(base.Application, this, items, options));
			}
		}
		return shapeImpl;
	}

	[CLSCompliant(false)]
	protected ShapeImpl AddChildShapes(MsofbtSpContainer shapeContainer, OfficeParseOptions options)
	{
		ShapeImpl shapeImpl = null;
		List<MsoBase> itemsList = shapeContainer.ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			if (!(itemsList[i] is MsofbtClientData))
			{
				continue;
			}
			List<ObjSubRecord> recordsList = (itemsList[i] as MsofbtClientData).ObjectRecord.RecordsList;
			int j = 0;
			for (int count2 = recordsList.Count; j < count2; j++)
			{
				if (recordsList[j].Type == TObjSubRecordType.ftCmo)
				{
					ftCmo ftCmo = recordsList[j] as ftCmo;
					shapeImpl = CreateShape(ftCmo.ObjectType, shapeContainer, options, recordsList, ftCmo.ID);
					break;
				}
			}
			break;
		}
		if (shapeImpl == null)
		{
			shapeImpl = new ShapeImpl(base.Application, this, shapeContainer, OfficeParseOptions.Default);
		}
		return shapeImpl;
	}

	[CLSCompliant(false)]
	protected virtual ShapeImpl AddShape(MsofbtSpContainer shapeContainer, OfficeParseOptions options)
	{
		ShapeImpl shapeImpl = null;
		List<MsoBase> itemsList = shapeContainer.ItemsList;
		int i = 0;
		for (int count = itemsList.Count; i < count; i++)
		{
			if (!(itemsList[i] is MsofbtClientData))
			{
				continue;
			}
			List<ObjSubRecord> recordsList = (itemsList[i] as MsofbtClientData).ObjectRecord.RecordsList;
			int j = 0;
			for (int count2 = recordsList.Count; j < count2; j++)
			{
				if (recordsList[j].Type == TObjSubRecordType.ftCmo)
				{
					ftCmo ftCmo = recordsList[j] as ftCmo;
					shapeImpl = CreateShape(ftCmo.ObjectType, shapeContainer, options, recordsList, j);
					break;
				}
			}
			break;
		}
		if (shapeImpl == null)
		{
			shapeImpl = new ShapeImpl(base.Application, this, shapeContainer, OfficeParseOptions.Default);
		}
		return AddShape(shapeImpl);
	}

	[CLSCompliant(false)]
	protected virtual ShapeImpl CreateShape(TObjType objType, MsofbtSpContainer shapeContainer, OfficeParseOptions options, List<ObjSubRecord> subRecords, int cmoIndex)
	{
		throw new NotImplementedException("This method must be overriden in child classes");
	}

	public new void Remove(IShape shape)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			if (this[i] == shape)
			{
				RemoveAt(i);
				break;
			}
		}
	}

	public override object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ConstructorInfo? constructor = GetType().GetConstructor(new Type[2]
		{
			typeof(IApplication),
			typeof(object)
		});
		if (constructor == null)
		{
			throw new ApplicationException("Cannot find required constructor.");
		}
		ShapeCollectionBase shapeCollectionBase = constructor.Invoke(new object[2] { base.Application, parent }) as ShapeCollectionBase;
		shapeCollectionBase.m_iCollectionIndex = m_iCollectionIndex;
		shapeCollectionBase.m_iLastId = m_iLastId;
		shapeCollectionBase.m_iStartId = m_iStartId;
		shapeCollectionBase.RegisterInWorksheet();
		List<IShape> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			IShape shape = innerList[i];
			if (shape is ShapeImpl)
			{
				shape = (IShape)((ShapeImpl)shape).Clone(shapeCollectionBase);
			}
			else if (shape is ICloneable)
			{
				shape = (IShape)((ICloneable)shape).Clone();
			}
		}
		shapeCollectionBase.SetParent(parent);
		shapeCollectionBase.SetParents();
		shapeCollectionBase.m_groupInfo = (MsofbtSpContainer)CloneUtils.CloneCloneable(m_groupInfo);
		return shapeCollectionBase;
	}

	protected virtual void RegisterInWorksheet()
	{
		WorksheetBase.InnerShapesBase = this;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (ShapesCount == 0)
		{
			return;
		}
		MsofbtDgContainer msofbtDgContainer = (MsofbtDgContainer)MsoFactory.GetRecord(MsoRecords.msofbtDgContainer);
		MsofbtDg msofbtDg = (MsofbtDg)MsoFactory.GetRecord(MsoRecords.msofbtDg);
		MsofbtSpgrContainer msofbtSpgrContainer = (MsofbtSpgrContainer)MsoFactory.GetRecord(MsoRecords.msofbtSpgrContainer);
		MsofbtSpContainer msofbtSpContainer = (MsofbtSpContainer)MsoFactory.GetRecord(MsoRecords.msofbtSpContainer);
		MsofbtSpgr itemToAdd = (MsofbtSpgr)MsoFactory.GetRecord(MsoRecords.msofbtSpgr);
		MsofbtSp msofbtSp = (MsofbtSp)MsoFactory.GetRecord(MsoRecords.msofbtSp);
		msofbtSp.IsGroup = true;
		msofbtSp.IsPatriarch = true;
		msofbtSp.ShapeId = m_iStartId;
		msofbtDgContainer.AddItem(msofbtDg);
		foreach (MsofbtRegroupItems arrRegroundItem in m_arrRegroundItems)
		{
			msofbtDgContainer.AddItem(arrRegroundItem);
		}
		msofbtDgContainer.AddItem(msofbtSpgrContainer);
		msofbtSpgrContainer.AddItem(msofbtSpContainer);
		msofbtSpContainer.AddItem(itemToAdd);
		msofbtSpContainer.AddItem(msofbtSp);
		List<IShape> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			ShapeImpl obj = innerList[i] as ShapeImpl;
			obj.PrepareForSerialization();
			obj.Serialize(msofbtSpgrContainer);
		}
		List<int> list = new List<int>();
		List<List<BiffRecordRaw>> list2 = new List<List<BiffRecordRaw>>();
		msofbtDg.ShapesNumber = (uint)ShapesTotalCount;
		msofbtDg.LastId = m_iLastId;
		if (m_iCollectionIndex > 0)
		{
			msofbtDg.Instance = m_iCollectionIndex;
		}
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Position = 8L;
		CreateData(memoryStream, msofbtDgContainer, list, list2);
		if (list.Count != list2.Count)
		{
			throw new ArgumentException("Breaks and records do not fit each other.");
		}
		int num = 0;
		TBIFFRecord recordCode = RecordCode;
		if (list.Count > 0)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				int num2 = list[j];
				List<BiffRecordRaw> value = list2[j];
				int num3 = num2 - num;
				if (num2 > 8224)
				{
					while (num3 > 0)
					{
						int num4 = Math.Min(num3, 8224);
						BiffRecordRaw record = BiffRecordFactory.GetRecord(TBIFFRecord.Continue);
						(record as ContinueRecord).SetLength(num4);
						WriteData(record, memoryStream, num, num4);
						num3 -= num4;
						num += num4;
						records.Add(record);
					}
				}
				else
				{
					BiffRecordRaw record = BiffRecordFactory.GetRecord(recordCode);
					WriteData(record, memoryStream, num, num3);
					records.Add(record);
				}
				num = num2;
				records.AddList(value);
			}
		}
		else
		{
			BiffRecordRaw record = BiffRecordFactory.GetRecord(recordCode);
			int num5 = (int)(memoryStream.Length - 8);
			byte[] array = new byte[num5];
			memoryStream.Position = 8L;
			memoryStream.Read(array, 0, num5);
			record.Data = array;
			((ILengthSetter)record).SetLength(num5);
			records.Add(record);
		}
	}

	private void WriteData(BiffRecordRaw record, MemoryStream buffer, int iCurPos, int size)
	{
		byte[] array = new byte[size];
		buffer.Position = iCurPos + 8;
		buffer.Read(array, 0, size);
		record.Data = array;
		((ILengthSetter)record).SetLength(size);
	}

	[CLSCompliant(false)]
	protected virtual void CreateData(Stream stream, MsofbtDgContainer dgContainer, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		dgContainer.FillArray(stream, 8, arrBreaks, arrRecords);
	}
}
