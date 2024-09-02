using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class TextBoxShapeImpl : TextBoxShapeBase, ITextBoxShapeEx, ITextBoxShape, ITextBox, IParentApplication, IShape
{
	private const int ShapeInstance = 202;

	private const int ShapeVersion = 2;

	internal const string EmbedString = "Forms.TextBox.1";

	private Rectangle m_2007Coordinates = new Rectangle(0, 1, 2076450, 1557338);

	private string m_id;

	private string m_type;

	private string m_textLink;

	private bool m_isCreated;

	private TextBodyPropertiesHolder textBodyPropertiesHolder;

	private bool m_bFlipHorizontal;

	private bool m_bFlipVertical;

	private bool m_isLineProperties;

	private bool m_isFill;

	private bool m_isNoFill;

	internal bool IsLineProperties
	{
		get
		{
			return m_isLineProperties;
		}
		set
		{
			m_isLineProperties = value;
		}
	}

	internal bool IsFill
	{
		get
		{
			return m_isFill;
		}
		set
		{
			m_isFill = value;
		}
	}

	internal bool IsNoFill
	{
		get
		{
			return m_isNoFill;
		}
		set
		{
			m_isNoFill = value;
		}
	}

	public Rectangle Coordinates2007
	{
		get
		{
			return m_2007Coordinates;
		}
		set
		{
			m_2007Coordinates = value;
		}
	}

	public string FieldId
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public string FieldType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public string TextLink
	{
		get
		{
			return m_textLink;
		}
		set
		{
			if (value != null && value.Length > 0 && value[0] == '=')
			{
				m_textLink = value;
				return;
			}
			throw new ArgumentException("Refrence is not valid");
		}
	}

	internal bool IsCreated
	{
		get
		{
			return m_isCreated;
		}
		set
		{
			m_isCreated = value;
		}
	}

	internal TextBodyPropertiesHolder TextBodyPropertiesHolder
	{
		get
		{
			if (textBodyPropertiesHolder == null)
			{
				textBodyPropertiesHolder = new TextBodyPropertiesHolder();
			}
			return textBodyPropertiesHolder;
		}
	}

	internal bool FlipHorizontal
	{
		get
		{
			return m_bFlipHorizontal;
		}
		set
		{
			m_bFlipHorizontal = value;
		}
	}

	internal bool FlipVertical
	{
		get
		{
			return m_bFlipVertical;
		}
		set
		{
			m_bFlipVertical = value;
		}
	}

	public TextBoxShapeImpl(IApplication application, object parent, WorksheetImpl sheet)
		: base(application, parent)
	{
		base.ShapeType = OfficeShapeType.TextBox;
		Fill.ForeColor = ColorExtension.White;
		Line.ForeColor = ColorExtension.DarkGray;
		Line.BackColor = ColorExtension.DarkGray;
		m_sheet = sheet;
	}

	[CLSCompliant(false)]
	public TextBoxShapeImpl(IApplication application, object parent, MsofbtSpContainer shapeContainer, OfficeParseOptions options)
		: base(application, parent, shapeContainer, options)
	{
		base.ShapeType = OfficeShapeType.TextBox;
	}

	private void InitializeShape()
	{
		base.ShapeType = OfficeShapeType.TextBox;
		m_bUpdateLineFill = true;
	}

	protected override void OnPrepareForSerialization()
	{
		m_shape = (MsofbtSp)MsoFactory.GetRecord(MsoRecords.msofbtSp);
		m_shape.Version = 2;
		m_shape.Instance = 202;
		m_shape.IsHaveAnchor = true;
		m_shape.IsHaveSpt = true;
	}

	[CLSCompliant(false)]
	protected override void SerializeShape(MsofbtSpgrContainer spgrContainer)
	{
		if (spgrContainer == null)
		{
			throw new ArgumentNullException("spgrContainer");
		}
		MsofbtSpContainer msofbtSpContainer = (MsofbtSpContainer)MsoFactory.GetRecord(MsoRecords.msofbtSpContainer);
		MsofbtClientData msofbtClientData = (MsofbtClientData)MsoFactory.GetRecord(MsoRecords.msofbtClientData);
		ftCmo ftCmo = null;
		if (base.Obj == null)
		{
			OBJRecord oBJRecord = (OBJRecord)BiffRecordFactory.GetRecord(TBIFFRecord.OBJ);
			ftCmo = new ftCmo();
			ftCmo.ObjectType = TObjType.otText;
			ftCmo.Printable = true;
			ftCmo.Locked = true;
			ftCmo.AutoLine = true;
			ftEnd record = new ftEnd();
			oBJRecord.AddSubRecord(ftCmo);
			oBJRecord.AddSubRecord(record);
			msofbtClientData.AddRecord(oBJRecord);
		}
		else
		{
			ftCmo = base.Obj.RecordsList[0] as ftCmo;
			msofbtClientData.AddRecord(base.Obj);
		}
		ftCmo.ID = ((base.OldObjId != 0) ? ((ushort)base.OldObjId) : ((ushort)base.ParentWorkbook.CurrentObjectId));
		msofbtSpContainer.AddItem(m_shape);
		MsofbtOPT msofbtOPT = SerializeOptions(msofbtSpContainer);
		if (msofbtOPT.Properties.Length != 0)
		{
			msofbtSpContainer.AddItem(msofbtOPT);
		}
		msofbtSpContainer.AddItem(base.ClientAnchor);
		msofbtSpContainer.AddItem(msofbtClientData);
		if (base.Text.Length > 0)
		{
			msofbtSpContainer.AddItem(GetClientTextBoxRecord(msofbtSpContainer));
		}
		spgrContainer.AddItem(msofbtSpContainer);
	}

	[CLSCompliant(false)]
	protected override MsofbtOPT CreateDefaultOptions()
	{
		MsofbtOPT msofbtOPT = base.CreateDefaultOptions();
		msofbtOPT.Version = 3;
		msofbtOPT.Instance = 2;
		return msofbtOPT;
	}

	public override IShape Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes, bool addToCollections)
	{
		TextBoxShapeImpl textBoxShapeImpl = (TextBoxShapeImpl)base.Clone(parent, hashNewNames, dicFontIndexes, addToCollections);
		if (addToCollections)
		{
			(textBoxShapeImpl.Worksheet.TextBoxes as TextBoxCollection).AddTextBox(textBoxShapeImpl);
		}
		return textBoxShapeImpl;
	}
}
