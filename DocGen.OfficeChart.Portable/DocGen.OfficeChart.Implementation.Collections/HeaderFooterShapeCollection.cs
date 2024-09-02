using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class HeaderFooterShapeCollection : ShapeCollectionBase
{
	public override TBIFFRecord RecordCode => TBIFFRecord.HeaderFooterImage;

	public override WorkbookShapeDataImpl ShapeData => base.Workbook.HeaderFooterData;

	public HeaderFooterShapeCollection(IApplication application, object parent)
		: base(application, parent)
	{
	}

	[CLSCompliant(false)]
	public HeaderFooterShapeCollection(IApplication application, object parent, MsofbtSpgrContainer container, OfficeParseOptions options)
		: base(application, parent, container, options)
	{
	}

	[CLSCompliant(false)]
	protected override ShapeImpl CreateShape(TObjType objType, MsofbtSpContainer shapeContainer, OfficeParseOptions options, List<ObjSubRecord> subRecords, int cmoIndex)
	{
		ShapeImpl result = null;
		if (objType == TObjType.otPicture)
		{
			result = new BitmapShapeImpl(base.Application, this, shapeContainer);
		}
		return result;
	}

	[CLSCompliant(false)]
	protected override void CreateData(Stream stream, MsofbtDgContainer dgContainer, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		stream.Write(HeaderFooterImageRecord.DEF_WORKSHEET_RECORD_START, 0, HeaderFooterImageRecord.DEF_WORKSHEET_RECORD_START.Length);
		base.CreateData(stream, dgContainer, arrBreaks, arrRecords);
	}

	[CLSCompliant(false)]
	protected override ShapeImpl AddShape(MsofbtSpContainer shapeContainer, OfficeParseOptions options)
	{
		ShapeImpl shapeImpl = null;
		_ = shapeContainer.ItemsList;
		shapeImpl = CreateShape(TObjType.otPicture, shapeContainer, options, null, -1);
		return AddShape(shapeImpl);
	}

	protected override void RegisterInWorksheet()
	{
		base.WorksheetBase.InnerHeaderFooterShapes = this;
	}

	[CLSCompliant(false)]
	public void Parse(HeaderFooterImageRecord record, OfficeParseOptions options)
	{
		ParseMsoStructures(record.StructuresList, options);
	}

	public ShapeImpl SetPicture(string strShapeName, Image image)
	{
		return SetPicture(strShapeName, image, -1);
	}

	public ShapeImpl SetPicture(string strShapeName, Image image, int iIndex)
	{
		return SetPicture(strShapeName, image, iIndex, bIncludeOptions: true, null);
	}

	public ShapeImpl SetPicture(string strShapeName, Image image, int iIndex, bool bIncludeOptions, string preservedStyles)
	{
		if (strShapeName == null)
		{
			throw new ArgumentNullException("strShapeName");
		}
		if (strShapeName.Length == 0)
		{
			throw new ArgumentException("strShapeName - string cannot be empty.");
		}
		ShapeImpl result = null;
		BitmapShapeImpl bitmapShapeImpl = base[strShapeName] as BitmapShapeImpl;
		bool flag = bitmapShapeImpl != null;
		WorkbookShapeDataImpl shapeData = ShapeData;
		if (bitmapShapeImpl != null)
		{
			uint blipId = bitmapShapeImpl.BlipId;
			shapeData.RemovePicture(blipId, removeImage: true);
		}
		if (image != null)
		{
			if (!flag)
			{
				bitmapShapeImpl = new BitmapShapeImpl(base.Application, this, bIncludeOptions);
			}
			int blipId2 = ((iIndex != -1) ? iIndex : shapeData.AddPicture(image, ExcelImageFormat.Original, strShapeName));
			bitmapShapeImpl.BlipId = (uint)blipId2;
			bitmapShapeImpl.SetName(strShapeName);
			bitmapShapeImpl.IsShortVersion = true;
			bitmapShapeImpl.ClientAnchor.TopRow = image.Height;
			bitmapShapeImpl.ClientAnchor.LeftColumn = image.Width;
			bitmapShapeImpl.VmlShape = true;
			if (preservedStyles != null && preservedStyles.Length > 0)
			{
				bitmapShapeImpl.PreserveStyleString = preservedStyles;
			}
			result = AddShape(bitmapShapeImpl);
		}
		else if (flag)
		{
			Remove(bitmapShapeImpl);
		}
		return result;
	}
}
