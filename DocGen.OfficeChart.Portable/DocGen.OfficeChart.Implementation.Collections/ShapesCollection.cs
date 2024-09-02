using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class ShapesCollection : ShapeCollectionBase, IShapes, IParentApplication, IEnumerable
{
	public const string DefaultChartNameStart = "Chart ";

	public const string DefaultTextBoxNameStart = "TextBox ";

	public const string DefaultCheckBoxNameStart = "CheckBox ";

	public const string DefaultOptionButtonNameStart = "Option Button ";

	public const string DefaultComboBoxNameStart = "Drop Down ";

	public const string DefaultPictureNameStart = "Picture ";

	internal const string DefaultGroupShapeNameStart = "Group ";

	public override TBIFFRecord RecordCode => TBIFFRecord.MSODrawing;

	public override WorkbookShapeDataImpl ShapeData => base.Workbook.ShapesData;

	public ShapesCollection(IApplication application, object parent)
		: base(application, parent)
	{
	}

	[CLSCompliant(false)]
	public ShapesCollection(IApplication application, object parent, MsofbtSpgrContainer container, OfficeParseOptions options)
		: base(application, parent, container, options)
	{
	}

	protected override void InitializeCollection()
	{
		base.InitializeCollection();
	}

	public IOfficeChartShape AddChart()
	{
		ChartShapeImpl chartShapeImpl = new ChartShapeImpl(base.Application, this);
		chartShapeImpl.Name = CollectionBaseEx<IShape>.GenerateDefaultName(base.List, "Chart ");
		AddShape(chartShapeImpl);
		return chartShapeImpl;
	}

	public ITextBoxShapeEx AddTextBox()
	{
		TextBoxShapeImpl textBoxShapeImpl = base.AppImplementation.CreateTextBoxShapeImpl(this, m_sheet as WorksheetImpl);
		AddShape(textBoxShapeImpl);
		m_sheet.TypedTextBoxes.AddTextBox(textBoxShapeImpl);
		textBoxShapeImpl.Name = CollectionBaseEx<IShape>.GenerateDefaultName(this, "TextBox ");
		return textBoxShapeImpl;
	}

	public void RegenerateComboBoxNames()
	{
	}

	[CLSCompliant(false)]
	protected override ShapeImpl CreateShape(TObjType objType, MsofbtSpContainer shapeContainer, OfficeParseOptions options, List<ObjSubRecord> subRecords, int cmoIndex)
	{
		ShapeImpl shapeImpl = null;
		switch (objType)
		{
		case TObjType.otPicture:
			if (shapeImpl == null)
			{
				shapeImpl = new BitmapShapeImpl(base.Application, this, shapeContainer);
			}
			break;
		case TObjType.otChart:
		{
			shapeImpl = new ChartShapeImpl(base.Application, this, shapeContainer, options);
			string name = shapeImpl.Name;
			if (name == null || name.Length == 0)
			{
				shapeImpl.Name = CollectionBaseEx<IShape>.GenerateDefaultName(this, "Chart ");
			}
			break;
		}
		case TObjType.otText:
		{
			TextBoxShapeImpl textBoxShapeImpl = new TextBoxShapeImpl(base.Application, this, shapeContainer, options);
			m_sheet.TypedTextBoxes.AddTextBox(textBoxShapeImpl);
			shapeImpl = textBoxShapeImpl;
			break;
		}
		}
		return shapeImpl;
	}

	private ShapeImpl ChoosePictureShape(MsofbtSpContainer shapeContainer, OfficeParseOptions options, List<ObjSubRecord> subRecords, int cmoIndex)
	{
		ShapeImpl result = null;
		int i = cmoIndex;
		for (int count = subRecords.Count; i < count; i++)
		{
			ObjSubRecord objSubRecord = subRecords[i];
			if (objSubRecord.Type == TObjSubRecordType.ftPictFmla)
			{
				if (((ftPictFmla)objSubRecord).Formula == "Forms.TextBox.1")
				{
					TextBoxShapeImpl textBoxShapeImpl = new TextBoxShapeImpl(base.Application, this, shapeContainer, options);
					m_sheet.TypedTextBoxes.AddTextBox(textBoxShapeImpl);
					result = textBoxShapeImpl;
				}
				break;
			}
		}
		return result;
	}

	public bool CanInsertRowColumn(int iIndex, int iCount, bool bRow, int iMaxIndex)
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			if (!((ShapeImpl)base.InnerList[num]).CanInsertRowColumn(iIndex, iCount, bRow, iMaxIndex))
			{
				return false;
			}
		}
		return true;
	}

	public void InsertRemoveRowColumn(int iIndex, int iCount, bool bRow, bool bRemove)
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			ShapeImpl shapeImpl = (ShapeImpl)base.InnerList[num];
			if (bRemove)
			{
				shapeImpl.RemoveRowColumn(iIndex, iCount, bRow);
			}
			else
			{
				shapeImpl.InsertRowColumn(iIndex, iCount, bRow);
			}
		}
		_ = base.Worksheet;
	}

	public BitmapShapeImpl AddPicture(int iBlipId, string strPictureName)
	{
		BitmapShapeImpl bitmapShapeImpl = new BitmapShapeImpl(base.Application, this);
		bitmapShapeImpl.FileName = strPictureName;
		bitmapShapeImpl.ShapeType = OfficeShapeType.Picture;
		bitmapShapeImpl.BlipId = (uint)iBlipId;
		base.Add(bitmapShapeImpl);
		bitmapShapeImpl.IsSizeWithCell = false;
		bitmapShapeImpl.IsMoveWithCell = true;
		return bitmapShapeImpl;
	}

	public void AddPicture(BitmapShapeImpl shape)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		base.Add(shape);
	}

	public void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		IList<IShape> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			((ShapeImpl)innerList[i]).UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
		}
	}

	public override object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ShapesCollection obj = (ShapesCollection)base.Clone(parent);
		_ = obj.InnerList;
		return obj;
	}

	public void CopyMoveShapeOnRangeCopy(WorksheetImpl destSheet, Rectangle rec, Rectangle recDest, bool bIsCopy)
	{
		if (destSheet == null)
		{
			throw new ArgumentNullException("destSheet");
		}
		IList<IShape> list = base.List;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ShapeImpl shapeImpl = (ShapeImpl)list[num];
			if (shapeImpl.CanCopyShapesOnRangeCopy(rec, recDest, out var newPosition))
			{
				shapeImpl.CopyMoveShapeOnRangeCopyMove(destSheet, newPosition, bIsCopy);
			}
		}
	}

	public void SetVersion(OfficeVersion version)
	{
		UtilityMethods.GetMaxRowColumnCount(out var iRows, out var iColumns, version);
		for (int num = base.Count - 1; num >= 0; num--)
		{
			ShapeImpl shapeImpl = (ShapeImpl)base[num];
			if (shapeImpl.LeftColumn > iColumns || shapeImpl.RightColumn > iColumns || shapeImpl.TopRow > iRows || shapeImpl.BottomRow > iRows)
			{
				shapeImpl.Remove();
			}
			else if (shapeImpl.Name == null || shapeImpl.Name.Length == 0)
			{
				shapeImpl.GenerateDefaultName();
			}
		}
	}

	internal void UpdateNamedRangeIndexes(int[] arrNewIndex)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			(base[i] as ShapeImpl).UpdateNamedRangeIndexes(arrNewIndex);
		}
	}

	internal void UpdateNamedRangeIndexes(IDictionary<int, int> dicNewIndex)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			(base[i] as ShapeImpl).UpdateNamedRangeIndexes(dicNewIndex);
		}
	}

	public IShape GetShapeById(int id)
	{
		ShapeImpl result = null;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			ShapeImpl shapeImpl = (ShapeImpl)base[i];
			if (shapeImpl.ShapeId == id)
			{
				result = shapeImpl;
				break;
			}
		}
		return result;
	}

	public IShape AddAutoShapes(AutoShapeType autoShapeType, int topRow, int leftColumn, int height, int width)
	{
		return AddAutoShapes(autoShapeType, topRow - 1, 0, leftColumn - 1, 0, height, width);
	}

	internal GroupShapeImpl Group(GroupShapeImpl groupShape, IShape[] groupItems, bool isRemove)
	{
		bool flag = false;
		double leftDouble = (groupItems[0] as ShapeImpl).LeftDouble;
		double topDouble = (groupItems[0] as ShapeImpl).TopDouble;
		double num = (groupItems[0] as ShapeImpl).LeftDouble + (groupItems[0] as ShapeImpl).WidthDouble;
		double num2 = (groupItems[0] as ShapeImpl).TopDouble + (groupItems[0] as ShapeImpl).HeightDouble;
		for (int i = 0; i < groupItems.Length; i++)
		{
			ShapeImpl shapeImpl = groupItems[i] as ShapeImpl;
			flag = shapeImpl.EnableAlternateContent;
			shapeImpl.ChangeParent(groupShape);
			if (shapeImpl.LeftDouble < leftDouble)
			{
				leftDouble = shapeImpl.LeftDouble;
			}
			if (shapeImpl.TopDouble < topDouble)
			{
				topDouble = shapeImpl.TopDouble;
			}
			if (shapeImpl.LeftDouble + shapeImpl.WidthDouble > num)
			{
				num = shapeImpl.LeftDouble + shapeImpl.WidthDouble;
			}
			if (shapeImpl.TopDouble + shapeImpl.HeightDouble > num2)
			{
				num2 = shapeImpl.TopDouble + shapeImpl.HeightDouble;
			}
			if (isRemove)
			{
				Remove(shapeImpl);
			}
		}
		groupShape.LeftDouble = leftDouble;
		groupShape.TopDouble = topDouble;
		groupShape.WidthDouble = num - leftDouble;
		groupShape.HeightDouble = num2 - topDouble;
		groupShape.ShapeFrame.SetChildAnchor(groupShape.ShapeFrame.OffsetX, groupShape.ShapeFrame.OffsetY, groupShape.ShapeFrame.OffsetCX, groupShape.ShapeFrame.OffsetCY);
		if (flag)
		{
			groupShape.EnableAlternateContent = flag;
		}
		groupShape.Items = groupItems;
		return groupShape;
	}

	internal void Ungroup(IGroupShape groupShape)
	{
		Ungroup(groupShape, isAll: false);
	}

	internal void Ungroup(IGroupShape groupShape, bool isAll)
	{
		if (Contains(groupShape))
		{
			(groupShape as GroupShapeImpl).LayoutGroupShape(isAll);
			UngroupShapes(groupShape, isAll, IsRemove: true);
		}
	}

	private void UngroupShapes(IGroupShape groupShape, bool isAll, bool IsRemove)
	{
		bool flipVertical = (groupShape as GroupShapeImpl).FlipVertical;
		bool flipHorizontal = (groupShape as GroupShapeImpl).FlipHorizontal;
		for (int i = 0; i < groupShape.Items.Length; i++)
		{
			ShapeImpl shapeImpl = groupShape.Items[i] as ShapeImpl;
			if (flipVertical || flipHorizontal)
			{
				if (shapeImpl is AutoShapeImpl)
				{
					if (flipVertical)
					{
						(shapeImpl as AutoShapeImpl).ShapeExt.FlipVertical = !(shapeImpl as AutoShapeImpl).ShapeExt.FlipVertical;
					}
					if (flipHorizontal)
					{
						(shapeImpl as AutoShapeImpl).ShapeExt.FlipHorizontal = !(shapeImpl as AutoShapeImpl).ShapeExt.FlipHorizontal;
					}
				}
				else if (shapeImpl is TextBoxShapeImpl)
				{
					if (flipVertical)
					{
						(shapeImpl as TextBoxShapeImpl).FlipVertical = !(shapeImpl as TextBoxShapeImpl).FlipVertical;
					}
					if (flipHorizontal)
					{
						(shapeImpl as TextBoxShapeImpl).FlipHorizontal = !(shapeImpl as TextBoxShapeImpl).FlipHorizontal;
					}
				}
				else if (shapeImpl is BitmapShapeImpl)
				{
					if (flipVertical)
					{
						(shapeImpl as BitmapShapeImpl).FlipVertical = !(shapeImpl as BitmapShapeImpl).FlipVertical;
					}
					if (flipHorizontal)
					{
						(shapeImpl as BitmapShapeImpl).FlipHorizontal = !(shapeImpl as BitmapShapeImpl).FlipHorizontal;
					}
				}
				else if (shapeImpl is GroupShapeImpl)
				{
					if (flipVertical)
					{
						(shapeImpl as GroupShapeImpl).FlipVertical = !(shapeImpl as GroupShapeImpl).FlipVertical;
					}
					if (flipHorizontal)
					{
						(shapeImpl as GroupShapeImpl).FlipHorizontal = !(shapeImpl as GroupShapeImpl).FlipHorizontal;
					}
				}
			}
			if (isAll && shapeImpl is GroupShapeImpl)
			{
				UngroupShapes(shapeImpl as IGroupShape, isAll, IsRemove: false);
				continue;
			}
			shapeImpl.ChangeParent(this);
			if (shapeImpl.GroupFrame != null)
			{
				shapeImpl.SetPostion(shapeImpl.GroupFrame.OffsetX, shapeImpl.GroupFrame.OffsetY, shapeImpl.GroupFrame.OffsetCX, shapeImpl.GroupFrame.OffsetCY);
				shapeImpl.ShapeRotation = shapeImpl.GroupFrame.Rotation / 60000;
				shapeImpl.ShapeFrame.SetAnchor(shapeImpl.ShapeRotation * 60000, shapeImpl.GroupFrame.OffsetX, shapeImpl.GroupFrame.OffsetY, shapeImpl.GroupFrame.OffsetCX, shapeImpl.GroupFrame.OffsetCY);
			}
			AddShape(shapeImpl);
		}
		if (IsRemove)
		{
			Remove(groupShape);
		}
	}

	internal GroupShapeImpl AddGroupShape(GroupShapeImpl groupShape, ShapeImpl[] shapes)
	{
		bool flag = false;
		foreach (ShapeImpl shapeImpl in shapes)
		{
			flag = shapeImpl.EnableAlternateContent;
			shapeImpl.ChangeParent(groupShape);
			Remove(shapeImpl);
		}
		if (flag)
		{
			groupShape.EnableAlternateContent = flag;
		}
		groupShape.Items = shapes;
		AddShape(groupShape);
		return groupShape;
	}

	internal new void Remove(IShape shape)
	{
		base.Remove(shape);
	}

	internal IShape AddAutoShapes(AutoShapeType autoShapeType, int topRow, int top, int leftColumn, int left, int height, int width)
	{
		AutoShapeImpl autoShapeImpl = new AutoShapeImpl(base.Application, this);
		WorksheetImpl sheetImpl = base.Application.ActiveSheet as WorksheetImpl;
		autoShapeImpl.CreateShape(autoShapeType, sheetImpl);
		autoShapeImpl.ShapeExt.IsCreated = true;
		autoShapeImpl.ShapeExt.ClientAnchor.SetAnchor(topRow, top, leftColumn, left, height, width);
		AddShape(autoShapeImpl);
		return autoShapeImpl;
	}

	public IPictureShape AddPicture(Image image, string pictureName, ExcelImageFormat imageFormat)
	{
		int iBlipId = ShapeData.AddPicture(image, imageFormat, pictureName);
		BitmapShapeImpl bitmapShapeImpl = AddPicture(iBlipId, pictureName);
		((IShape)bitmapShapeImpl).Height = (int)Math.Round((double)image.Height * ApplicationImpl.ConvertToPixels(1.0, MeasureUnits.Inch) / (double)image.VerticalResolution);
		((IShape)bitmapShapeImpl).Width = (int)Math.Round((double)image.Width * ApplicationImpl.ConvertToPixels(1.0, MeasureUnits.Inch) / (double)image.HorizontalResolution);
		((IShape)bitmapShapeImpl).Name = pictureName;
		return bitmapShapeImpl;
	}
}
