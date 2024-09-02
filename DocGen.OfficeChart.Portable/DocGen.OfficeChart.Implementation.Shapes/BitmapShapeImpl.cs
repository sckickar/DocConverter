using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;
using DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class BitmapShapeImpl : ShapeImpl, IShape, IParentApplication, IDisposable, IPictureShape
{
	public const int ShapeInstance = 75;

	private uint m_uiBlipId;

	private string m_strBlipFileName;

	private MsofbtBSE m_picture;

	private Image m_bitmap;

	private Stream m_bitmapStream;

	private Stream m_streamBlipSubNodes;

	private Stream m_streamShapeProperties;

	private Stream m_svgData;

	private string m_svgRelId;

	private string m_svgPicturePath;

	private bool m_isSVGUpdated;

	private Stream m_srcRectStream;

	private string m_strMacro;

	private bool m_bDDE;

	private bool m_bCamera;

	private long m_offsetX;

	private long m_offsetY;

	private long m_extentsX;

	private long m_extentsY;

	private bool m_flipVertical;

	private bool m_flipHorizontal;

	private string m_externalLink;

	private bool m_isSvgExternalLink;

	internal new Dictionary<string, Stream> m_preservedElements;

	protected MsoOptions[] cropOptions = new MsoOptions[4]
	{
		MsoOptions.CropFromBottom,
		MsoOptions.CropFromLeft,
		MsoOptions.CropFromRight,
		MsoOptions.CropFromTop
	};

	private int m_cropLeftOffset;

	private int m_cropRightOffset;

	private int m_cropBottomOffset;

	private int m_cropTopOffset;

	private bool m_hasTransparentDetails;

	private int m_amount = 100000;

	private int m_threshold;

	private bool m_grayScale;

	private bool m_isUseAlpha = true;

	private List<ChartColor> m_duoTone;

	private List<ChartColor> m_colorChange;

	private CameraTool m_camera;

	public bool HasTransparency
	{
		get
		{
			return m_hasTransparentDetails;
		}
		set
		{
			m_hasTransparentDetails = value;
		}
	}

	public string FileName
	{
		get
		{
			return m_strBlipFileName;
		}
		set
		{
			m_strBlipFileName = value;
		}
	}

	internal string ExternalLink
	{
		get
		{
			return m_externalLink;
		}
		set
		{
			m_externalLink = value;
		}
	}

	internal bool IsSvgExternalLink
	{
		get
		{
			return m_isSvgExternalLink;
		}
		set
		{
			m_isSvgExternalLink = value;
		}
	}

	[CLSCompliant(false)]
	public uint BlipId
	{
		get
		{
			return m_uiBlipId;
		}
		set
		{
			m_uiBlipId = value;
			m_picture = m_shapes.ShapeData.Pictures[(int)(value - 1)];
			m_bitmap = m_picture.PictureRecord.Picture;
			m_bitmapStream = m_picture.PictureRecord.PictureStream;
		}
	}

	internal int CropLeftOffset
	{
		get
		{
			return m_cropLeftOffset;
		}
		set
		{
			m_cropLeftOffset = value;
		}
	}

	internal int CropRightOffset
	{
		get
		{
			return m_cropRightOffset;
		}
		set
		{
			m_cropRightOffset = value;
		}
	}

	internal int CropBottomOffset
	{
		get
		{
			return m_cropBottomOffset;
		}
		set
		{
			m_cropBottomOffset = value;
		}
	}

	internal int CropTopOffset
	{
		get
		{
			return m_cropTopOffset;
		}
		set
		{
			m_cropTopOffset = value;
		}
	}

	public Image Picture
	{
		get
		{
			if (m_bitmapStream != null)
			{
				m_bitmap = Image.FromStream(m_bitmapStream);
			}
			return m_bitmap;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Bitmap");
			}
			WorkbookShapeDataImpl shapeData = m_shapes.ShapeData;
			shapeData.RemovePicture(BlipId, removeImage: true);
			int blipId = shapeData.AddPicture(value, ExcelImageFormat.Png, Name);
			BlipId = (uint)blipId;
		}
	}

	public Stream BlipSubNodesStream
	{
		get
		{
			return m_streamBlipSubNodes;
		}
		set
		{
			m_streamBlipSubNodes = value;
		}
	}

	[Obsolete]
	public Stream ShapePropertiesStream
	{
		get
		{
			return m_streamShapeProperties;
		}
		set
		{
			m_streamShapeProperties = value;
		}
	}

	public Stream SvgData
	{
		get
		{
			return m_svgData;
		}
		set
		{
			if (value != null && value.Length > 0)
			{
				m_svgData = value;
				if (m_svgRelId != null)
				{
					m_svgRelId = null;
					m_isSVGUpdated = true;
				}
			}
		}
	}

	internal string SvgRelId
	{
		get
		{
			return m_svgRelId;
		}
		set
		{
			m_svgRelId = value;
		}
	}

	internal bool IsSvgUpdated
	{
		get
		{
			return m_isSVGUpdated;
		}
		set
		{
			m_isSVGUpdated = value;
		}
	}

	internal string SvgPicturePath
	{
		get
		{
			return m_svgPicturePath;
		}
		set
		{
			m_svgPicturePath = value;
		}
	}

	internal new Dictionary<string, Stream> PreservedElements
	{
		get
		{
			if (m_preservedElements == null)
			{
				m_preservedElements = new Dictionary<string, Stream>();
			}
			return m_preservedElements;
		}
	}

	public Stream SourceRectStream
	{
		get
		{
			return m_srcRectStream;
		}
		set
		{
			m_srcRectStream = value;
		}
	}

	public override int Instance
	{
		get
		{
			if (m_shape == null)
			{
				return 75;
			}
			return m_shape.Instance;
		}
	}

	public string Macro
	{
		get
		{
			return m_strMacro;
		}
		set
		{
			m_strMacro = value;
		}
	}

	public bool IsDDE
	{
		get
		{
			return m_bDDE;
		}
		set
		{
			m_bDDE = value;
		}
	}

	public bool IsCamera
	{
		get
		{
			return m_bCamera;
		}
		set
		{
			m_bCamera = value;
		}
	}

	internal long OffsetX
	{
		get
		{
			return m_offsetX;
		}
		set
		{
			m_offsetX = value;
		}
	}

	internal long OffsetY
	{
		get
		{
			return m_offsetY;
		}
		set
		{
			m_offsetY = value;
		}
	}

	internal long ExtentsX
	{
		get
		{
			return m_extentsX;
		}
		set
		{
			m_extentsX = value;
		}
	}

	internal long ExtentsY
	{
		get
		{
			return m_extentsY;
		}
		set
		{
			m_extentsY = value;
		}
	}

	internal bool FlipVertical
	{
		get
		{
			return m_flipVertical;
		}
		set
		{
			m_flipVertical = value;
		}
	}

	internal bool FlipHorizontal
	{
		get
		{
			return m_flipHorizontal;
		}
		set
		{
			m_flipHorizontal = value;
		}
	}

	internal bool IsUseAlpha
	{
		get
		{
			return m_isUseAlpha;
		}
		set
		{
			m_isUseAlpha = value;
		}
	}

	internal List<ChartColor> DuoTone
	{
		get
		{
			if (m_duoTone == null)
			{
				m_duoTone = new List<ChartColor>();
			}
			return m_duoTone;
		}
	}

	internal List<ChartColor> ColorChange
	{
		get
		{
			if (m_colorChange == null)
			{
				m_colorChange = new List<ChartColor>();
			}
			return m_colorChange;
		}
	}

	internal int Amount
	{
		get
		{
			return m_amount;
		}
		set
		{
			m_amount = value;
		}
	}

	internal int Threshold
	{
		get
		{
			return m_threshold;
		}
		set
		{
			m_threshold = value;
		}
	}

	internal bool GrayScale
	{
		get
		{
			return m_grayScale;
		}
		set
		{
			m_grayScale = value;
		}
	}

	internal CameraTool Camera
	{
		get
		{
			return m_camera;
		}
		set
		{
			m_camera = value;
		}
	}

	public BitmapShapeImpl(IApplication application, object parent)
		: this(application, parent, IncludeShapeOptions: true)
	{
	}

	public BitmapShapeImpl(IApplication application, object parent, bool IncludeShapeOptions)
		: base(application, parent)
	{
		m_bSupportOptions = true;
		if (IncludeShapeOptions)
		{
			m_bUpdateLineFill = true;
			Fill.Visible = false;
			Line.Visible = false;
		}
		base.ShapeType = OfficeShapeType.Picture;
	}

	[CLSCompliant(false)]
	public BitmapShapeImpl(IApplication application, object parent, MsoBase[] records, int index)
		: base(application, parent, records, index)
	{
		m_bSupportOptions = true;
		base.ShapeType = OfficeShapeType.Picture;
		m_bitmap = m_picture.PictureRecord.Picture;
	}

	[CLSCompliant(false)]
	public BitmapShapeImpl(IApplication application, object parent, MsofbtSpContainer container)
		: base(application, parent, container, OfficeParseOptions.Default)
	{
		m_bSupportOptions = true;
		base.ShapeType = OfficeShapeType.Picture;
		if (m_picture != null)
		{
			m_bitmap = m_picture.PictureRecord.Picture;
		}
	}

	[CLSCompliant(false)]
	protected override bool ParseOption(MsofbtOPT.FOPTE option)
	{
		if (base.ParseOption(option))
		{
			return true;
		}
		switch (option.Id)
		{
		case MsoOptions.BlipId:
			ParseBlipId(option);
			return true;
		case MsoOptions.BlipName:
			ParseBlipName(option);
			return true;
		case MsoOptions.CropFromTop:
		case MsoOptions.CropFromBottom:
		case MsoOptions.CropFromLeft:
		case MsoOptions.CropFromRight:
			ParseCropRectangle(option);
			return true;
		default:
			return false;
		}
	}

	protected void ParseCropRectangle(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		if (Array.IndexOf(cropOptions, option.Id) < 0)
		{
			throw new ArgumentOutOfRangeException("Crop option expected");
		}
		switch (option.Id)
		{
		case MsoOptions.CropFromBottom:
			m_cropBottomOffset = option.Int32Value + option.Int32Value / 2;
			break;
		case MsoOptions.CropFromLeft:
			m_cropLeftOffset = option.Int32Value + option.Int32Value / 2;
			break;
		case MsoOptions.CropFromRight:
			m_cropRightOffset = option.Int32Value + option.Int32Value / 2;
			break;
		case MsoOptions.CropFromTop:
			m_cropTopOffset = option.Int32Value + option.Int32Value / 2;
			break;
		}
	}

	[CLSCompliant(false)]
	protected virtual void ParseBlipId(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		if (option.Id != MsoOptions.BlipId)
		{
			throw new ArgumentOutOfRangeException("BlipId option expected");
		}
		m_uiBlipId = option.UInt32Value;
		IList pictures = m_shapes.ShapeData.Pictures;
		m_picture = ((m_uiBlipId != 0) ? ((MsofbtBSE)pictures[(int)(m_uiBlipId - 1)]) : null);
	}

	[CLSCompliant(false)]
	protected virtual void ParseBlipName(MsofbtOPT.FOPTE option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		if (option.Id != MsoOptions.BlipName)
		{
			throw new ArgumentOutOfRangeException("BlipName option expected");
		}
		if (option.AdditionalData != null)
		{
			byte[] additionalData = option.AdditionalData;
			m_strBlipFileName = Encoding.Unicode.GetString(additionalData, 0, additionalData.Length);
		}
	}

	[CLSCompliant(false)]
	protected override bool ExtractNecessaryOption(MsofbtOPT.FOPTE option)
	{
		if (base.ExtractNecessaryOption(option))
		{
			return true;
		}
		switch (option.Id)
		{
		case MsoOptions.BlipId:
			ParseBlipId(option);
			return true;
		case MsoOptions.BlipName:
			ParseBlipName(option);
			return true;
		default:
			return false;
		}
	}

	public new void Dispose()
	{
		base.Dispose();
	}

	[CLSCompliant(false)]
	protected override void SerializeShape(MsofbtSpgrContainer spgrContainer)
	{
		MsofbtSpContainer msofbtSpContainer = (MsofbtSpContainer)MsoFactory.GetRecord(MsoRecords.msofbtSpContainer);
		msofbtSpContainer.AddItem(m_shape);
		SerializeOptions(msofbtSpContainer);
		SerializeClientAnchor(msofbtSpContainer);
		SerializeClientData(msofbtSpContainer);
		spgrContainer.AddItem(msofbtSpContainer);
	}

	protected override void OnPrepareForSerialization()
	{
		if (m_shape == null)
		{
			m_shape = (MsofbtSp)MsoFactory.GetRecord(MsoRecords.msofbtSp);
			m_shape.Instance = 75;
		}
		m_shape.IsHaveAnchor = true;
		m_shape.IsHaveSpt = true;
	}

	private void SerializeOptions(MsofbtSpContainer spContainer)
	{
		MsofbtOPT msofbtOPT = m_options;
		if (msofbtOPT == null)
		{
			msofbtOPT = CreateDefaultOptions();
			SerializeOptionSorted(msofbtOPT, MsoOptions.NoLineDrawDash, 524296u);
			SerializeOptionSorted(msofbtOPT, MsoOptions.NoFillHitTest, 1048576u);
		}
		if (m_bUpdateLineFill)
		{
			msofbtOPT = SerializeMsoOptions(msofbtOPT);
		}
		MsofbtOPT.FOPTE fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = MsoOptions.BlipId;
		fOPTE.UInt32Value = m_uiBlipId;
		fOPTE.IsValid = true;
		fOPTE.IsComplex = false;
		msofbtOPT.AddOptionSorted(fOPTE);
		fOPTE = new MsofbtOPT.FOPTE();
		fOPTE.Id = MsoOptions.BlipName;
		if (m_strBlipFileName != null)
		{
			if (m_strBlipFileName[m_strBlipFileName.Length - 1] != 0)
			{
				m_strBlipFileName += "\0";
			}
			fOPTE.UInt32Value = (uint)(m_strBlipFileName.Length * 2);
			fOPTE.IsValid = true;
			fOPTE.IsComplex = true;
			fOPTE.AdditionalData = Encoding.Unicode.GetBytes(m_strBlipFileName);
			msofbtOPT.AddOptionSorted(fOPTE);
		}
		SerializeShapeName(msofbtOPT);
		SerializeName(msofbtOPT, MsoOptions.AlternativeText, AlternativeText);
		SerializeShapeVisibility(msofbtOPT);
		msofbtOPT.Version = 3;
		msofbtOPT.Instance = 2;
		spContainer.AddItem(msofbtOPT);
	}

	private void SerializeClientAnchor(MsofbtSpContainer spContainer)
	{
		if (spContainer == null)
		{
			throw new ArgumentNullException("spContainer");
		}
		spContainer.AddItem(base.ClientAnchor);
	}

	private void SerializeClientData(MsofbtSpContainer spContainer)
	{
		if (!base.IsShortVersion)
		{
			if (spContainer == null)
			{
				throw new ArgumentNullException("spContainer");
			}
			MsofbtClientData msofbtClientData = (MsofbtClientData)MsoFactory.GetRecord(MsoRecords.msofbtClientData);
			OBJRecord oBJRecord = base.Obj;
			ftCmo ftCmo;
			if (oBJRecord == null)
			{
				oBJRecord = (OBJRecord)BiffRecordFactory.GetRecord(TBIFFRecord.OBJ);
				ftCmo = new ftCmo();
				ftCmo.ObjectType = TObjType.otPicture;
				ftCmo.Printable = true;
				ftCmo.Locked = true;
				ftCmo.AutoFill = true;
				ftCmo.AutoLine = true;
				ftEnd record = new ftEnd();
				oBJRecord.AddSubRecord(ftCmo);
				oBJRecord.AddSubRecord(record);
			}
			else
			{
				ftCmo = (ftCmo)oBJRecord.Records[0];
			}
			ftCmo.ID = ((base.OldObjId != 0) ? ((ushort)base.OldObjId) : ((ushort)base.ParentWorkbook.CurrentObjectId));
			msofbtClientData.AddRecord(oBJRecord);
			spContainer.AddItem(msofbtClientData);
		}
	}

	public override void RegisterInSubCollection()
	{
		m_shapes.WorksheetBase.InnerPictures.AddPicture(this);
	}

	protected override void OnDelete()
	{
		OnDelete(removeImage: true);
	}

	protected void OnDelete(bool removeImage)
	{
		base.OnDelete();
		if (BlipId != 0)
		{
			base.ParentShapes.Workbook.ShapesData.RemovePicture(BlipId, removeImage);
			m_uiBlipId = 0u;
		}
		((PicturesCollection)m_shapes.Worksheet.Pictures).RemovePicture(this);
	}

	public void Remove(bool removeImage)
	{
		OnDelete(removeImage);
		m_shapes.Remove(this);
	}

	public override IShape Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes, bool addToCollection)
	{
		bool flag = true;
		ShapeCollectionBase shapeCollectionBase = (ShapeCollectionBase)CommonObject.FindParent(parent, typeof(ShapeCollectionBase), bSubTypes: true);
		WorksheetBaseImpl worksheetBaseImpl;
		if (shapeCollectionBase != null)
		{
			worksheetBaseImpl = shapeCollectionBase.WorksheetBase;
		}
		else
		{
			worksheetBaseImpl = CommonObject.FindParent(parent, typeof(WorksheetBaseImpl), bSubTypes: true) as WorksheetBaseImpl;
			flag = false;
		}
		WorkbookImpl parentWorkbook = base.ParentWorkbook;
		WorkbookImpl parentWorkbook2 = worksheetBaseImpl.ParentWorkbook;
		int num = (int)BlipId;
		if (flag)
		{
			flag = !(shapeCollectionBase is HeaderFooterShapeCollection);
		}
		WorkbookShapeDataImpl workbookShapeDataImpl = (flag ? parentWorkbook2.ShapesData : parentWorkbook2.HeaderFooterData);
		if (parentWorkbook2 != parentWorkbook && num > 0)
		{
			MsofbtBSE picture = (flag ? parentWorkbook.ShapesData : parentWorkbook.HeaderFooterData).GetPicture(num);
			num = workbookShapeDataImpl.AddPicture((MsofbtBSE)picture.Clone());
		}
		BitmapShapeImpl bitmapShapeImpl;
		if (flag || !addToCollection)
		{
			bitmapShapeImpl = (BitmapShapeImpl)MemberwiseClone();
			bitmapShapeImpl.SetParent(parent);
			bitmapShapeImpl.SetParents();
			bitmapShapeImpl.AttachEvents();
			bitmapShapeImpl.CopyFrom(this, hashNewNames, dicFontIndexes);
			bitmapShapeImpl.CloneLineFill(this);
			if (m_duoTone != null)
			{
				bitmapShapeImpl.m_duoTone = new List<ChartColor>();
				for (int i = 0; i < m_duoTone.Count; i++)
				{
					bitmapShapeImpl.m_duoTone.Add(m_duoTone[i].Clone());
				}
			}
			if (m_colorChange != null)
			{
				bitmapShapeImpl.m_colorChange = new List<ChartColor>();
				for (int j = 0; j < m_colorChange.Count; j++)
				{
					bitmapShapeImpl.m_colorChange.Add(m_colorChange[j].Clone());
				}
			}
			if (addToCollection)
			{
				worksheetBaseImpl.InnerShapes.AddPicture(bitmapShapeImpl);
			}
			if (num > 0)
			{
				bitmapShapeImpl.BlipId = (uint)num;
				workbookShapeDataImpl.Pictures[num - 1].RefCount++;
			}
			if (m_camera != null)
			{
				bitmapShapeImpl.Camera = m_camera.Clone(bitmapShapeImpl);
			}
		}
		else
		{
			if (flag)
			{
				throw new NotImplementedException();
			}
			bitmapShapeImpl = worksheetBaseImpl.HeaderFooterShapes.SetPicture(Name, Picture, num, bIncludeOptions: false, base.PreserveStyleString) as BitmapShapeImpl;
			bitmapShapeImpl.m_options = (MsofbtOPT)CloneUtils.CloneCloneable(m_options);
			bitmapShapeImpl.m_srcRectStream = CloneUtils.CloneStream(m_srcRectStream);
			bitmapShapeImpl.m_streamBlipSubNodes = CloneUtils.CloneStream(m_streamBlipSubNodes);
			bitmapShapeImpl.m_preservedElements = CloneUtils.CloneHash(PreservedElements);
			bitmapShapeImpl.AttachEvents();
		}
		if (base.ImageRelation != null)
		{
			bitmapShapeImpl.ImageRelation = (Relation)base.ImageRelation.Clone();
		}
		return bitmapShapeImpl;
	}

	[CLSCompliant(false)]
	protected override bool UpdateMso(MsoBase mso)
	{
		if (base.UpdateMso(mso))
		{
			return true;
		}
		if (mso is MsofbtBSE)
		{
			m_picture = mso as MsofbtBSE;
			m_bitmap = m_picture.PictureRecord.Picture;
			return true;
		}
		return false;
	}

	public override void GenerateDefaultName()
	{
		Name = CollectionBaseEx<IShape>.GenerateDefaultName(m_shapes, "Picture ");
	}

	[CLSCompliant(false)]
	public void SetBlipId(uint newId)
	{
		m_uiBlipId = newId;
	}
}
