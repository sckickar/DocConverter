using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation;

internal abstract class WorksheetBaseImpl : CommonObject, INamedObject, IParseable, ITabSheet, IParentApplication, ICloneParent
{
	private delegate string NameGetter(ITabSheet tabSheet);

	private static readonly TBIFFRecord[] DEF_NOTMSORECORDS = new TBIFFRecord[9]
	{
		TBIFFRecord.PivotViewDefinition,
		TBIFFRecord.Note,
		TBIFFRecord.WindowTwo,
		(TBIFFRecord)2128,
		TBIFFRecord.HeaderFooterImage,
		(TBIFFRecord)237,
		TBIFFRecord.ChartUnits,
		TBIFFRecord.ChartChart,
		TBIFFRecord.DCON
	};

	public const int DEF_MAX_PASSWORDLEN = 255;

	private const ushort DEF_PASSWORD_CONST = 52811;

	[CLSCompliant(false)]
	public const int DEF_MIN_COLUMN_INDEX = int.MaxValue;

	public const int DEF_MIN_ROW_INDEX = -1;

	public const OfficeKnownColors DEF_DEFAULT_TAB_COLOR = (OfficeKnownColors)(-1);

	private static readonly Color DEF_DEFAULT_TAB_COLOR_RGB = ColorExtension.Empty;

	[Obsolete("This constant is obsolete and will be removed soon. Please, use MaxRowCount property of the IWorkbook interface. Sorry for inconvenience.")]
	public const int DEF_MAX_ROW_ONE_INDEX = 65536;

	[Obsolete("This constant is obsolete and will be removed soon. Please, use MaxColumnCount property of the IWorkbook interface. Sorry for inconvenience.")]
	public const int DEF_MAX_COLUMN_ONE_INDEX = 256;

	private const int MaxSheetNameLength = 31;

	private bool m_bParseOnDemand;

	private bool m_bParseDataOnDemand;

	protected WorkbookImpl m_book;

	private string m_strName = string.Empty;

	private bool m_bChanged = true;

	private int m_iRealIndex;

	protected int m_iMsoStartIndex = -1;

	private int m_iCurMsoIndex;

	protected OfficeParseOptions m_parseOptions;

	private List<BiffRecordRaw> m_arrMSODrawings;

	internal PicturesCollection m_pictures;

	protected List<BiffRecordRaw> m_arrRecords = new List<BiffRecordRaw>();

	protected ShapesCollection m_shapes;

	private WorksheetChartsCollection m_charts;

	private bool m_bIsSupported = true;

	private int m_iZoom = 100;

	private SheetProtectionRecord m_sheetProtection;

	protected RangeProtectionRecord m_rangeProtectionRecord;

	private PasswordRecord m_password;

	protected string m_strCodeName;

	private bool m_bParsed = true;

	private bool m_bParsing;

	private bool m_bSkipParsing;

	private WindowTwoRecord m_windowTwo;

	private PageLayoutView m_layout;

	[CLSCompliant(false)]
	protected int m_iFirstColumn = int.MaxValue;

	protected int m_iLastColumn = int.MaxValue;

	protected int m_iFirstRow = -1;

	protected int m_iLastRow = -1;

	private ChartColor m_tabColor;

	private HeaderFooterShapeCollection m_headerFooterShapes;

	private int m_iIndex;

	private OfficeSheetProtection m_parseProtection = (OfficeSheetProtection)(-1);

	internal BOFRecord m_bof;

	protected bool KeepRecord;

	private OfficeWorksheetVisibility m_visiblity;

	protected internal WorksheetDataHolder m_dataHolder;

	private bool m_bUnknownVmlShapes;

	private TextBoxCollection m_textBoxes;

	private bool m_bTransitionEvaluation;

	protected bool m_isCustomHeight;

	private BiffRecordRaw m_previousRecord;

	private bool m_bTabColorRGB;

	private string m_algorithmName;

	private byte[] m_hashValue;

	private byte[] m_saltValue;

	private uint m_spinCount;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			if (value != m_strName)
			{
				int length = value.Length;
				if (value[0] == '\'' || value[length - 1] == '\'')
				{
					throw new ArgumentOutOfRangeException("Apostrophe can't be used as first and/or last character of the worksheet's name.");
				}
				if (value.Length > 31)
				{
					value = value.Substring(0, 31);
					value = GenerateUniqueName(GetName, value);
				}
				ValueChangedEventArgs args = new ValueChangedEventArgs(m_strName, value, "Name");
				m_strName = value;
				OnNameChanged(args);
			}
		}
	}

	public bool IsSaved
	{
		get
		{
			return !m_bChanged;
		}
		set
		{
			m_bChanged = !value;
		}
	}

	protected internal WorksheetChartsCollection InnerCharts
	{
		get
		{
			CheckParseOnDemand();
			if (m_charts == null)
			{
				m_charts = new WorksheetChartsCollection(base.Application, this);
			}
			return m_charts;
		}
	}

	protected internal ShapesCollection InnerShapes
	{
		[DebuggerStepThrough]
		get
		{
			CheckParseOnDemand();
			return m_shapes;
		}
	}

	public IShapes Shapes
	{
		get
		{
			CheckParseOnDemand();
			return m_shapes;
		}
	}

	public ShapeCollectionBase InnerShapesBase
	{
		get
		{
			CheckParseOnDemand();
			return m_shapes;
		}
		internal set
		{
			m_shapes = (ShapesCollection)value;
		}
	}

	public HeaderFooterShapeCollection HeaderFooterShapes
	{
		get
		{
			CheckParseOnDemand();
			if (m_headerFooterShapes == null)
			{
				m_headerFooterShapes = new HeaderFooterShapeCollection(base.Application, this);
			}
			return m_headerFooterShapes;
		}
	}

	public HeaderFooterShapeCollection InnerHeaderFooterShapes
	{
		get
		{
			CheckParseOnDemand();
			return m_headerFooterShapes;
		}
		internal set
		{
			m_headerFooterShapes = value;
		}
	}

	public IOfficeChartShapes Charts
	{
		get
		{
			CheckParseOnDemand();
			if (m_charts == null)
			{
				m_charts = new WorksheetChartsCollection(base.Application, this);
			}
			return m_charts;
		}
	}

	public string CodeName
	{
		get
		{
			if (m_strCodeName == null)
			{
				return m_strName;
			}
			return m_strCodeName;
		}
		internal set
		{
			m_strCodeName = value;
		}
	}

	internal bool HasCodeName => m_strCodeName != null;

	[CLSCompliant(false)]
	public WindowTwoRecord WindowTwo
	{
		get
		{
			if (m_windowTwo == null)
			{
				m_windowTwo = (WindowTwoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.WindowTwo);
			}
			if (BOF != null && BOF.Type == BOFRecord.TType.TYPE_CHART)
			{
				m_windowTwo.OriginalLength = 10;
			}
			return m_windowTwo;
		}
	}

	public virtual bool ProtectContents
	{
		get
		{
			return (InnerProtection & OfficeSheetProtection.Content) != 0;
		}
		internal set
		{
			if (value)
			{
				InnerProtection |= OfficeSheetProtection.Content;
			}
			else
			{
				InnerProtection &= ~OfficeSheetProtection.Content;
			}
		}
	}

	public virtual bool ProtectDrawingObjects
	{
		get
		{
			if (ProtectContents)
			{
				return (InnerProtection & OfficeSheetProtection.Objects) == 0;
			}
			return false;
		}
	}

	public virtual bool ProtectScenarios
	{
		get
		{
			if (ProtectContents)
			{
				return (InnerProtection & OfficeSheetProtection.Scenarios) == 0;
			}
			return false;
		}
	}

	public bool IsPasswordProtected
	{
		get
		{
			CheckParseOnDemand();
			if (m_parseProtection.ToString() != "-1" && !(Workbook as WorkbookImpl).Saving)
			{
				return true;
			}
			if (m_password != null)
			{
				return m_password.IsPassword != 0;
			}
			return false;
		}
	}

	public bool IsParsed
	{
		get
		{
			return m_bParsed;
		}
		set
		{
			m_bParsed = value;
		}
	}

	public bool IsParsing
	{
		get
		{
			return m_bParsing;
		}
		set
		{
			m_bParsing = value;
		}
	}

	public bool IsSkipParsing => m_bSkipParsing;

	public bool IsSupported
	{
		get
		{
			return m_bIsSupported;
		}
		protected set
		{
			m_bIsSupported = value;
		}
	}

	public WorkbookImpl ParentWorkbook => m_book;

	public virtual int FirstRow
	{
		get
		{
			ParseData();
			return m_iFirstRow;
		}
		set
		{
			m_iFirstRow = value;
		}
	}

	[CLSCompliant(false)]
	public virtual int FirstColumn
	{
		get
		{
			ParseData();
			return m_iFirstColumn;
		}
		set
		{
			m_iFirstColumn = value;
		}
	}

	public virtual int LastRow
	{
		get
		{
			ParseData();
			return m_iLastRow;
		}
		set
		{
			m_iLastRow = value;
		}
	}

	public virtual int LastColumn
	{
		get
		{
			ParseData();
			return m_iLastColumn;
		}
		set
		{
			m_iLastColumn = value;
		}
	}

	public int Zoom
	{
		get
		{
			return m_iZoom;
		}
		set
		{
			if (value < 10 || value > 400)
			{
				throw new ArgumentOutOfRangeException("Zoom", "Zoom must be in range from 10 till 400.");
			}
			m_iZoom = value;
		}
	}

	public virtual ChartColor TabColorObject
	{
		get
		{
			if (m_tabColor == null)
			{
				m_tabColor = new ChartColor((OfficeKnownColors)(-1));
			}
			return m_tabColor;
		}
	}

	public virtual OfficeKnownColors TabColor
	{
		get
		{
			if (!(m_tabColor != null))
			{
				return (OfficeKnownColors)(-1);
			}
			return m_tabColor.GetIndexed(m_book);
		}
		set
		{
			if (m_tabColor == null)
			{
				m_tabColor = new ChartColor(OfficeKnownColors.Black);
			}
			m_tabColor.SetIndexed(value);
		}
	}

	public virtual Color TabColorRGB
	{
		get
		{
			if (!(m_tabColor != null))
			{
				return DEF_DEFAULT_TAB_COLOR_RGB;
			}
			return m_tabColor.GetRGB(m_book);
		}
		set
		{
			if (m_tabColor == null)
			{
				m_tabColor = new ChartColor(OfficeKnownColors.Black);
			}
			m_bTabColorRGB = true;
			m_tabColor.SetRGB(value, m_book);
		}
	}

	public OfficeKnownColors GridLineColor
	{
		get
		{
			return (OfficeKnownColors)m_windowTwo.HeaderColor;
		}
		set
		{
			WindowTwo.IsDefaultHeader = false;
			WindowTwo.HeaderColor = (int)value;
		}
	}

	public bool DefaultGridlineColor
	{
		get
		{
			return WindowTwo.IsDefaultHeader;
		}
		set
		{
			WindowTwo.IsDefaultHeader = value;
		}
	}

	public IWorkbook Workbook => m_book;

	public bool IsRightToLeft
	{
		get
		{
			return WindowTwo.IsArabic;
		}
		set
		{
			WindowTwo.IsArabic = value;
		}
	}

	public abstract PageSetupBaseImpl PageSetupBase { get; }

	public bool IsSelected => WindowTwo.IsSelected;

	public int Index
	{
		get
		{
			return m_iIndex;
		}
		set
		{
			m_iIndex = value;
		}
	}

	public virtual OfficeSheetProtection Protection
	{
		get
		{
			if (m_sheetProtection == null)
			{
				return OfficeSheetProtection.None;
			}
			return (OfficeSheetProtection)m_sheetProtection.ProtectedOptions;
		}
	}

	protected internal virtual OfficeSheetProtection InnerProtection
	{
		get
		{
			if (m_sheetProtection == null)
			{
				return UnprotectedOptions;
			}
			return (OfficeSheetProtection)m_sheetProtection.ProtectedOptions;
		}
		internal set
		{
			m_sheetProtection.ProtectedOptions = (int)value;
		}
	}

	protected virtual OfficeSheetProtection UnprotectedOptions => OfficeSheetProtection.None;

	internal BOFRecord BOF => m_bof;

	public OfficeWorksheetVisibility Visibility
	{
		get
		{
			return m_visiblity;
		}
		set
		{
			if (Visibility == value)
			{
				return;
			}
			OfficeWorksheetVisibility visibility = Visibility;
			m_visiblity = value;
			if (m_book.IsWorkbookOpening || value == OfficeWorksheetVisibility.Visible)
			{
				return;
			}
			int realIndex = RealIndex;
			WorkbookObjectsCollection objects = m_book.Objects;
			int i = realIndex + 1;
			for (int count = objects.Count; i < count; i++)
			{
				if (FindUnhided(objects, i))
				{
					return;
				}
			}
			for (int num = realIndex - 1; num >= 0; num--)
			{
				if (FindUnhided(objects, num))
				{
					return;
				}
			}
			m_visiblity = visibility;
			throw new NotSupportedException("A workbook must contain at least one visible worksheet.");
		}
	}

	internal WorksheetDataHolder DataHolder
	{
		get
		{
			return m_dataHolder;
		}
		set
		{
			m_dataHolder = value;
			if (value != null)
			{
				m_bParsed = false;
			}
		}
	}

	public int TopVisibleRow
	{
		get
		{
			return WindowTwo.TopRow + 1;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			WindowTwo.TopRow = (ushort)(value - 1);
		}
	}

	public int LeftVisibleColumn
	{
		get
		{
			return WindowTwo.LeftColumn + 1;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			WindowTwo.LeftColumn = (ushort)(value - 1);
		}
	}

	internal PasswordRecord Password => m_password;

	public bool UnknownVmlShapes
	{
		get
		{
			return m_bUnknownVmlShapes;
		}
		set
		{
			m_bUnknownVmlShapes = value;
		}
	}

	public TextBoxCollection TypedTextBoxes
	{
		get
		{
			if (m_textBoxes == null)
			{
				m_textBoxes = new TextBoxCollection(base.Application, this);
			}
			return m_textBoxes;
		}
	}

	internal TextBoxCollection InnerTextBoxes
	{
		get
		{
			CheckParseOnDemand();
			return m_textBoxes;
		}
	}

	public ITextBoxes TextBoxes => TypedTextBoxes;

	public int VmlShapesCount
	{
		get
		{
			int num = 0;
			int i = 0;
			for (int count = m_shapes.Count; i < count; i++)
			{
				if ((m_shapes[i] as ShapeImpl).VmlShape)
				{
					num++;
				}
			}
			return num;
		}
	}

	protected abstract OfficeSheetProtection DefaultProtectionOptions { get; }

	private bool ProtectionMeaningDirect => (DefaultProtectionOptions & OfficeSheetProtection.Content) != 0;

	internal virtual bool ContainsProtection
	{
		get
		{
			if (m_sheetProtection != null)
			{
				return m_sheetProtection.ProtectedOptions != 17408;
			}
			return false;
		}
	}

	protected SheetProtectionRecord SheetProtection => m_sheetProtection;

	public bool IsTransitionEvaluation
	{
		get
		{
			return m_bTransitionEvaluation;
		}
		set
		{
			m_bTransitionEvaluation = value;
		}
	}

	public bool ParseOnDemand
	{
		get
		{
			return m_bParseOnDemand;
		}
		set
		{
			m_bParseOnDemand = value;
		}
	}

	internal virtual bool ParseDataOnDemand
	{
		get
		{
			return m_bParseDataOnDemand;
		}
		set
		{
			m_bParseDataOnDemand = value;
		}
	}

	internal bool HasTabColorRGB => m_bTabColorRGB;

	public string AlgorithmName
	{
		get
		{
			return m_algorithmName;
		}
		set
		{
			m_algorithmName = value;
		}
	}

	public byte[] HashValue
	{
		get
		{
			return m_hashValue;
		}
		set
		{
			m_hashValue = value;
		}
	}

	public byte[] SaltValue
	{
		get
		{
			return m_saltValue;
		}
		set
		{
			m_saltValue = value;
		}
	}

	public uint SpinCount
	{
		get
		{
			return m_spinCount;
		}
		set
		{
			m_spinCount = value;
		}
	}

	public IPictures Pictures
	{
		get
		{
			CheckParseOnDemand();
			if (m_pictures == null)
			{
				m_pictures = new PicturesCollection(base.Application, this);
			}
			return m_pictures;
		}
	}

	protected internal PicturesCollection InnerPictures
	{
		get
		{
			if (m_pictures == null)
			{
				m_pictures = new PicturesCollection(base.Application, this);
			}
			return m_pictures;
		}
	}

	public int RealIndex
	{
		get
		{
			return m_iRealIndex;
		}
		set
		{
			if (m_iRealIndex != value)
			{
				int iRealIndex = m_iRealIndex;
				m_iRealIndex = value;
				OnRealIndexChanged(iRealIndex);
			}
		}
	}

	int ITabSheet.TabIndex => m_iRealIndex;

	public event ValueChangedEventHandler NameChanged;

	public WorksheetBaseImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
		InitializeCollections();
	}

	protected WorksheetBaseImpl()
	{
	}

	private bool FindVmlShape()
	{
		int i = 0;
		for (int count = m_shapes.Count; i < count; i++)
		{
			if ((m_shapes[i] as ShapeImpl).VmlShape)
			{
				return true;
			}
		}
		return false;
	}

	internal void ClearEvents()
	{
		this.NameChanged = null;
	}

	private bool FindUnhided(WorkbookObjectsCollection objects, int iIndex)
	{
		if (((WorksheetBaseImpl)objects[iIndex]).Visibility == OfficeWorksheetVisibility.Visible)
		{
			m_book.ActiveSheetIndex = iIndex;
			m_book.DisplayedTab = iIndex;
			return true;
		}
		return false;
	}

	protected virtual void FindParents()
	{
		object obj = FindParent(typeof(WorkbookImpl));
		if (obj == null)
		{
			throw new ApplicationException("Worksheet must be a member of Workbook object tree");
		}
		m_book = (WorkbookImpl)obj;
		m_iRealIndex = m_book.ObjectCount;
	}

	protected virtual void OnNameChanged(ValueChangedEventArgs args)
	{
		RaiseNameChangedEvent(args);
		SetChanged();
	}

	protected void RaiseNameChangedEvent(ValueChangedEventArgs args)
	{
		if (this.NameChanged != null)
		{
			this.NameChanged(this, args);
		}
	}

	public void SetChanged()
	{
		if (!m_book.IsWorkbookOpening)
		{
			m_book.Saved = false;
			m_book.IsCellModified = true;
			IsSaved = false;
		}
	}

	protected virtual void InitializeCollections()
	{
		m_shapes = new ShapesCollection(base.Application, this);
	}

	protected virtual void ClearAll(OfficeWorksheetCopyFlags flags)
	{
		if (m_arrMSODrawings != null)
		{
			m_arrMSODrawings.Clear();
		}
		if (m_shapes != null)
		{
			m_shapes.Clear();
		}
		if (m_charts != null)
		{
			m_charts.Clear();
		}
		if (m_headerFooterShapes != null)
		{
			m_headerFooterShapes.Clear();
		}
	}

	public virtual void Activate()
	{
		if (m_book.WindowOne.SelectedTab != RealIndex || m_book.IsWorkbookOpening)
		{
			base.AppImplementation.SetActiveWorksheet(this);
			m_book.SetActiveWorksheet(this);
			m_book.InnerWorksheetGroup.Select(this);
		}
	}

	public virtual void Select()
	{
		Activate();
	}

	public void Unselect()
	{
		Unselect(bCheckNumber: true);
	}

	public void Unselect(bool bCheckNumber)
	{
		WindowOneRecord windowOne = m_book.WindowOne;
		if (WindowTwo.IsSelected && ((bCheckNumber && windowOne.NumSelectedTabs > 1) || !bCheckNumber))
		{
			WindowTwo.IsSelected = false;
			m_book.WindowOne.NumSelectedTabs--;
			if (bCheckNumber)
			{
				m_book.InnerWorksheetGroup.Remove(this);
			}
		}
	}

	public void Protect(string password)
	{
		Protect(password, DefaultProtectionOptions);
	}

	public void Protect(string password, OfficeSheetProtection options)
	{
		if (IsPasswordProtected)
		{
			throw new ApplicationException("Sheet is already protected, before use unprotect method");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (password.Length > 255)
		{
			throw new ArgumentOutOfRangeException("Length of the password can't be more than " + 255);
		}
		_ = m_book.Version;
		_ = 3;
		ushort password2 = (ushort)((password.Length <= 0) ? 1 : GetPasswordHash(password));
		Protect(password2, options);
	}

	protected virtual OfficeSheetProtection PrepareProtectionOptions(OfficeSheetProtection options)
	{
		return options;
	}

	public void Unprotect()
	{
		m_password = null;
		m_sheetProtection = null;
	}

	public void Unprotect(string password)
	{
	}

	protected virtual void OnRealIndexChanged(int iOldIndex)
	{
	}

	public void SelectTab()
	{
		if (!WindowTwo.IsSelected)
		{
			WindowTwo.IsSelected = true;
		}
	}

	public virtual void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		m_shapes.UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
	}

	public virtual void UpdateExtendedFormatIndex(Dictionary<int, int> dictFormats)
	{
	}

	public virtual object Clone(object parent)
	{
		return Clone(parent, cloneShapes: true);
	}

	public virtual object Clone(object parent, bool cloneShapes)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		WorksheetBaseImpl worksheetBaseImpl = (WorksheetBaseImpl)MemberwiseClone();
		FileDataHolder dataHolder = new FileDataHolder(m_book);
		worksheetBaseImpl.SetParent(parent);
		worksheetBaseImpl.FindParents();
		worksheetBaseImpl.m_password = (PasswordRecord)CloneUtils.CloneCloneable(m_password);
		worksheetBaseImpl.m_windowTwo = (WindowTwoRecord)CloneUtils.CloneCloneable(m_windowTwo);
		worksheetBaseImpl.m_bof = (BOFRecord)CloneUtils.CloneCloneable(m_bof);
		worksheetBaseImpl.m_arrMSODrawings = CloneUtils.CloneCloneable(m_arrMSODrawings);
		worksheetBaseImpl.m_arrRecords = CloneUtils.CloneCloneable(m_arrRecords);
		if (m_charts != null)
		{
			worksheetBaseImpl.m_charts = new WorksheetChartsCollection(base.Application, worksheetBaseImpl);
		}
		if (cloneShapes)
		{
			CloneShapes(worksheetBaseImpl);
		}
		worksheetBaseImpl.m_headerFooterShapes = (HeaderFooterShapeCollection)CloneUtils.CloneCloneable((ICloneParent)m_headerFooterShapes, (object)worksheetBaseImpl);
		if (m_dataHolder != null)
		{
			if (worksheetBaseImpl.m_book.DataHolder == null)
			{
				worksheetBaseImpl.m_dataHolder = m_dataHolder.Clone(dataHolder);
			}
			else
			{
				worksheetBaseImpl.m_dataHolder = m_dataHolder.Clone(worksheetBaseImpl.m_book.DataHolder);
			}
		}
		return worksheetBaseImpl;
	}

	public void CloneShapes(WorksheetBaseImpl result)
	{
		result.m_shapes = (ShapesCollection)m_shapes.Clone(result);
	}

	protected internal virtual void UpdateStyleIndexes(int[] styleIndexes)
	{
		if (styleIndexes == null)
		{
			throw new ArgumentNullException("styleIndexes");
		}
	}

	internal void Protect(ushort password, OfficeSheetProtection options)
	{
		if (m_password == null)
		{
			m_password = (PasswordRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Password);
		}
		options = PrepareProtectionOptions(options);
		m_password.IsPassword = password;
		m_sheetProtection = (SheetProtectionRecord)BiffRecordFactory.GetRecord(TBIFFRecord.SheetProtection);
		m_sheetProtection.ProtectedOptions = (ushort)options;
		m_sheetProtection.ContainProtection = true;
	}

	public abstract void MarkUsedReferences(bool[] usedItems);

	public abstract void UpdateReferenceIndexes(int[] arrUpdatedIndexes);

	protected void PrepareProtection()
	{
		if (m_sheetProtection == null && m_parseProtection != 0 && m_parseProtection != (OfficeSheetProtection)(-1))
		{
			m_sheetProtection = (SheetProtectionRecord)BiffRecordFactory.GetRecord(TBIFFRecord.SheetProtection);
			m_sheetProtection.ProtectedOptions = (int)m_parseProtection;
			m_sheetProtection.ContainProtection = true;
		}
	}

	[CLSCompliant(false)]
	protected void ParseProtect(ProtectRecord protectRecord)
	{
		if (protectRecord.IsProtected)
		{
			m_parseProtection = DefaultProtectionOptions;
			if (ProtectionMeaningDirect)
			{
				m_parseProtection |= OfficeSheetProtection.Content;
			}
			else
			{
				m_parseProtection &= ~OfficeSheetProtection.Content;
			}
		}
	}

	[CLSCompliant(false)]
	protected void ParsePassword(PasswordRecord passwordRecord)
	{
		m_password = passwordRecord;
	}

	[CLSCompliant(false)]
	protected void ParseObjectProtect(ObjectProtectRecord objectProtect)
	{
		if (objectProtect.IsProtected)
		{
			if (ProtectionMeaningDirect)
			{
				m_parseProtection |= OfficeSheetProtection.Objects;
			}
			else
			{
				m_parseProtection &= ~OfficeSheetProtection.Objects;
			}
		}
	}

	[CLSCompliant(false)]
	protected void ParseScenProtect(ScenProtectRecord scenProtect)
	{
		if (scenProtect.IsProtected)
		{
			if (ProtectionMeaningDirect)
			{
				m_parseProtection |= OfficeSheetProtection.Scenarios;
			}
			else
			{
				m_parseProtection &= ~OfficeSheetProtection.Scenarios;
			}
		}
	}

	protected virtual void PrepareVariables(OfficeParseOptions options, bool bSkipParsing)
	{
		m_arrRecords.Clear();
		m_iMsoStartIndex = -1;
		m_parseOptions = options;
	}

	[CLSCompliant(false)]
	private void ParsePageLayoutView(PageLayoutView layout)
	{
		if (layout == null)
		{
			throw new ArgumentNullException("windowTwo");
		}
		WorksheetImpl worksheetImpl = this as WorksheetImpl;
		if (layout.LayoutView)
		{
			worksheetImpl.View = OfficeSheetView.PageLayout;
		}
		m_layout = layout;
	}

	[CLSCompliant(false)]
	protected virtual void ParseWindowTwo(WindowTwoRecord windowTwo)
	{
		if (windowTwo == null)
		{
			throw new ArgumentNullException("windowTwo");
		}
		m_windowTwo = windowTwo;
		if (m_windowTwo.IsSelected)
		{
			m_book.WorksheetGroup.Add(this);
		}
	}

	[CLSCompliant(false)]
	protected virtual void ParseRecord(BiffRecordRaw raw, bool bIgnoreStyles, Dictionary<int, int> hashNewXFormatIndexes)
	{
	}

	[CLSCompliant(false)]
	protected virtual void ParseDimensions(DimensionsRecord dimensions)
	{
		if (dimensions == null)
		{
			throw new ArgumentNullException("dimensions");
		}
		if (dimensions.LastColumn == 0 && dimensions.LastRow == 0)
		{
			m_iFirstColumn = int.MaxValue;
			m_iFirstRow = -1;
			m_iLastColumn = int.MaxValue;
			m_iLastRow = -1;
			return;
		}
		m_iFirstColumn = dimensions.FirstColumn + 1;
		m_iFirstRow = dimensions.FirstRow + 1;
		m_iLastColumn = Math.Min(dimensions.LastColumn, m_book.MaxColumnCount);
		if (m_iLastColumn == 0)
		{
			m_iLastColumn = 1;
		}
		m_iLastRow = Math.Min(dimensions.LastRow, m_book.MaxRowCount);
		if (m_iLastRow == 0)
		{
			m_iLastRow = 1;
		}
	}

	[CLSCompliant(false)]
	protected virtual void ParseWindowZoom(WindowZoomRecord windowZoom)
	{
		if (windowZoom == null)
		{
			throw new ArgumentNullException("windowZoom");
		}
		m_iZoom = windowZoom.Zoom;
	}

	[CLSCompliant(false)]
	protected void ParseSheetLayout(SheetLayoutRecord sheetLayout)
	{
		if (sheetLayout == null)
		{
			throw new ArgumentNullException("sheetLayout");
		}
		TabColor = (OfficeKnownColors)sheetLayout.ColorIndex;
	}

	[CLSCompliant(false)]
	public virtual void Serialize(OffsetArrayList records)
	{
		throw new NotImplementedException();
	}

	[CLSCompliant(false)]
	protected virtual void SerializeMsoDrawings(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_shapes != null && m_shapes.Count > 0 && (base.Application.SkipOnSave & OfficeSkipExtRecords.Drawings) != OfficeSkipExtRecords.Drawings)
		{
			m_shapes.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeProtection(OffsetArrayList records, bool bContentNotNecessary)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (bContentNotNecessary || ProtectContents)
		{
			if (ProtectContents)
			{
				ProtectRecord protectRecord = (ProtectRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Protect);
				protectRecord.IsProtected = true;
				records.Add(protectRecord);
			}
			if (ProtectScenarios)
			{
				ScenProtectRecord scenProtectRecord = (ScenProtectRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ScenProtect);
				scenProtectRecord.IsProtected = true;
				records.Add(scenProtectRecord);
			}
			if (ProtectDrawingObjects)
			{
				ObjectProtectRecord objectProtectRecord = (ObjectProtectRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ObjectProtect);
				objectProtectRecord.IsProtected = true;
				records.Add(objectProtectRecord);
			}
		}
		if (m_password != null && m_password.IsPassword != 1)
		{
			records.Add(m_password);
		}
	}

	[CLSCompliant(false)]
	protected void SerializeSheetProtection(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_sheetProtection != null && ContainsProtection)
		{
			SheetProtectionRecord sheetProtectionRecord = (SheetProtectionRecord)m_sheetProtection.Clone();
			sheetProtectionRecord.ProtectedOptions &= -32769;
			records.Add(sheetProtectionRecord);
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeHeaderFooterPictures(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_headerFooterShapes != null && m_headerFooterShapes.Count > 0)
		{
			m_headerFooterShapes.Serialize(records);
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeWindowTwo(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (this == m_book.ActiveSheet)
		{
			WindowTwo.IsPaged = true;
			WindowTwo.IsSelected = true;
		}
		records.Add(WindowTwo);
	}

	[CLSCompliant(false)]
	protected virtual void SerializePageLayoutView(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		WorksheetImpl worksheetImpl = this as WorksheetImpl;
		if (m_layout != null && worksheetImpl.View == OfficeSheetView.PageLayout)
		{
			m_layout.LayoutView = true;
			records.Add(m_layout);
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeMacrosSupport(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if ((base.Application.SkipOnSave & OfficeSkipExtRecords.Macros) != OfficeSkipExtRecords.Macros && m_strCodeName != null && m_book.HasMacros)
		{
			CodeNameRecord codeNameRecord = (CodeNameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.CodeName);
			codeNameRecord.CodeName = m_strCodeName;
			records.Add(codeNameRecord);
		}
	}

	[CLSCompliant(false)]
	protected void SerializeWindowZoom(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		WindowZoomRecord windowZoomRecord = (WindowZoomRecord)BiffRecordFactory.GetRecord(TBIFFRecord.WindowZoom);
		windowZoomRecord.Zoom = m_iZoom;
		records.Add(windowZoomRecord);
	}

	[CLSCompliant(false)]
	protected void SerializeSheetLayout(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_tabColor != null)
		{
			SheetLayoutRecord sheetLayoutRecord = (SheetLayoutRecord)BiffRecordFactory.GetRecord(TBIFFRecord.SheetLayout);
			sheetLayoutRecord.ColorIndex = (int)m_tabColor.GetIndexed(m_book);
			records.Add(sheetLayoutRecord);
		}
	}

	[CLSCompliant(false)]
	public static ushort GetPasswordHash(string password)
	{
		if (password == null)
		{
			return 0;
		}
		ushort num = 0;
		int i = 0;
		for (int length = password.Length; i < length; i++)
		{
			ushort uInt16FromBits = GetUInt16FromBits(RotateBits(GetCharBits15(password[i]), i + 1));
			num ^= uInt16FromBits;
		}
		return (ushort)((uint)(num ^ password.Length) ^ 0xCE4Bu);
	}

	private static bool[] GetCharBits15(char charToConvert)
	{
		bool[] array = new bool[15];
		ushort num = Convert.ToUInt16(charToConvert);
		ushort num2 = 1;
		for (int i = 0; i < 15; i++)
		{
			array[i] = (num & num2) == num2;
			num2 <<= 1;
		}
		return array;
	}

	private static ushort GetUInt16FromBits(bool[] bits)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length > 16)
		{
			throw new ArgumentOutOfRangeException("There can't be more than 16 bits");
		}
		ushort num = 0;
		ushort num2 = 1;
		int i = 0;
		for (int num3 = bits.Length; i < num3; i++)
		{
			if (bits[i])
			{
				num += num2;
			}
			num2 <<= 1;
		}
		return num;
	}

	private static bool[] RotateBits(bool[] bits, int count)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length == 0)
		{
			return bits;
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("Count can't be less than zero");
		}
		bool[] array = new bool[bits.Length];
		int i = 0;
		for (int num = bits.Length; i < num; i++)
		{
			int num2 = (i + count) % num;
			array[num2] = bits[i];
		}
		return array;
	}

	public static int Round(int value, int degree)
	{
		if (degree == 0)
		{
			throw new ArgumentOutOfRangeException("degree can't be 0");
		}
		int num = value % degree;
		return value - num + degree;
	}

	public virtual void Parse()
	{
		IsParsed = false;
		ParseData();
		bool isParsed = IsParsed;
		IsParsing = true;
		IsParsed = false;
		ExtractMSODrawing(m_iMsoStartIndex, m_parseOptions);
		IsParsing = false;
		IsParsed = isParsed;
		if (IsSupported && IsParsed)
		{
			m_arrRecords.Clear();
		}
	}

	protected internal void ParseData()
	{
		ParseData(null);
	}

	protected internal abstract void ParseData(Dictionary<int, int> dictUpdatedSSTIndexes);

	protected void ExtractMSODrawing(int startIndex, OfficeParseOptions options)
	{
		if (m_arrMSODrawings != null)
		{
			m_arrMSODrawings.Clear();
		}
		else
		{
			m_arrMSODrawings = new List<BiffRecordRaw>();
		}
		if (startIndex < 0)
		{
			return;
		}
		int i = startIndex;
		int count = m_arrRecords.Count;
		int num = 0;
		for (; i < count; i++)
		{
			TBIFFRecord typeCode = m_arrRecords[i].TypeCode;
			if (num == 0 && Array.IndexOf(DEF_NOTMSORECORDS, typeCode) != -1)
			{
				break;
			}
			switch (typeCode)
			{
			case TBIFFRecord.BOF:
				num++;
				break;
			case TBIFFRecord.EOF:
				num--;
				break;
			case TBIFFRecord.TextObject:
			{
				TextObjectRecord textObjectRecord = (TextObjectRecord)m_arrRecords[i];
				m_arrMSODrawings.Add(textObjectRecord);
				if (textObjectRecord.TextLen > 0)
				{
					i++;
					int num2 = textObjectRecord.TextLen;
					while (num2 > 0)
					{
						byte[] data = ((ContinueRecord)m_arrRecords[i]).Data;
						bool flag = data[0] != 0;
						int num3 = data.Length - 1;
						num2 -= (flag ? (num3 / 2) : num3);
						m_arrMSODrawings.Add(m_arrRecords[i]);
						i++;
					}
					int num4 = textObjectRecord.FormattingRunsLen;
					while (num4 > 0)
					{
						ContinueRecord continueRecord2 = (ContinueRecord)m_arrRecords[i];
						num4 -= continueRecord2.Length;
						m_arrMSODrawings.Add(continueRecord2);
						i++;
					}
					i--;
				}
				continue;
			}
			case TBIFFRecord.Continue:
			{
				MSODrawingRecord mSODrawingRecord = BiffRecordFactory.GetRecord(TBIFFRecord.MSODrawing) as MSODrawingRecord;
				ContinueRecord continueRecord = m_arrRecords[i] as ContinueRecord;
				mSODrawingRecord.m_data = new byte[continueRecord.m_data.Length];
				continueRecord.m_data.CopyTo(mSODrawingRecord.m_data, 0);
				mSODrawingRecord.RecordLength = continueRecord.Length;
				m_arrMSODrawings.Add(mSODrawingRecord);
				continue;
			}
			}
			m_arrMSODrawings.Add(m_arrRecords[i]);
		}
		List<MsoBase> arrStructures = CombineMsoDrawings();
		m_shapes.ParseMsoStructures(arrStructures, options);
		m_shapes.RegenerateComboBoxNames();
	}

	private List<MsoBase> CombineMsoDrawings()
	{
		List<byte[]> list = new List<byte[]>();
		List<MsoBase> list2 = new List<MsoBase>();
		int num = 0;
		int num2 = 0;
		if (m_arrMSODrawings.Count > 0)
		{
			int i = 0;
			for (int count = m_arrMSODrawings.Count; i < count; i++)
			{
				BiffRecordRaw biffRecordRaw = m_arrMSODrawings[i];
				if (num == 0 && biffRecordRaw is MSODrawingRecord)
				{
					byte[] data = biffRecordRaw.Data;
					num2 += data.Length;
					list.Add(data);
				}
				else if (biffRecordRaw.TypeCode == TBIFFRecord.BOF)
				{
					num++;
				}
				else if (biffRecordRaw.TypeCode == TBIFFRecord.EOF)
				{
					num--;
				}
			}
			MemoryStream memoryStream = new MemoryStream(CombineArrays(num2, list));
			while (memoryStream.Position < num2)
			{
				MsoBase item = MsoFactory.CreateMsoRecord(null, memoryStream, GetNextMsoData);
				list2.Add(item);
			}
		}
		return list2;
	}

	private BiffRecordRaw[] GetNextMsoData()
	{
		List<BiffRecordRaw> list = new List<BiffRecordRaw>();
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			if (m_iCurMsoIndex >= m_arrMSODrawings.Count)
			{
				throw new ApplicationException("Can't find data for MSODrawing");
			}
			if (!(m_arrMSODrawings[m_iCurMsoIndex] is MSODrawingRecord))
			{
				flag = true;
			}
			else
			{
				m_iCurMsoIndex++;
			}
		}
		while (flag && m_iCurMsoIndex < m_arrMSODrawings.Count && (num != 0 || !(m_arrMSODrawings[m_iCurMsoIndex] is MSODrawingRecord)))
		{
			list.Add(m_arrMSODrawings[m_iCurMsoIndex]);
			if (m_arrMSODrawings[m_iCurMsoIndex] is BOFRecord)
			{
				num++;
			}
			else if (m_arrMSODrawings[m_iCurMsoIndex] is EOFRecord)
			{
				num--;
			}
			m_iCurMsoIndex++;
		}
		return list.ToArray();
	}

	public static byte[] CombineArrays(int iCombinedLength, List<byte[]> arrCombined)
	{
		if (arrCombined == null || arrCombined.Count == 0)
		{
			return new byte[0];
		}
		int count = arrCombined.Count;
		byte[] array = new byte[iCombinedLength];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			byte[] array2 = arrCombined[i];
			int num2 = array2.Length;
			Buffer.BlockCopy(array2, 0, array, num, num2);
			num += num2;
		}
		return array;
	}

	public void CopyFrom(WorksheetBaseImpl worksheet, Dictionary<string, string> hashStyleNames, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicFontIndexes, OfficeWorksheetCopyFlags flags, Dictionary<int, int> hashExtFormatIndexes)
	{
		if ((flags & OfficeWorksheetCopyFlags.ClearBefore) != 0)
		{
			ClearAll(flags);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyOptions) != 0)
		{
			CopyOptions(worksheet);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyShapes) != 0)
		{
			CopyShapes(worksheet, hashWorksheetNames, dicFontIndexes);
		}
		if ((flags & OfficeWorksheetCopyFlags.CopyPageSetup) != 0)
		{
			CopyHeaderFooterImages(worksheet, hashWorksheetNames, dicFontIndexes);
		}
	}

	protected void CopyHeaderFooterImages(WorksheetBaseImpl sourceSheet, Dictionary<string, string> hashNewNames, IDictionary dicFontIndexes)
	{
		_ = PageSetupBase;
		_ = sourceSheet.PageSetupBase;
		CloneUtils.CloneCloneable((ICloneParent)sourceSheet.m_headerFooterShapes, (object)this);
	}

	protected void CopyShapes(WorksheetBaseImpl sourceSheet, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes)
	{
		ShapeCollectionBase shapeCollectionBase = sourceSheet.Shapes as ShapeCollectionBase;
		int i = 0;
		for (int count = shapeCollectionBase.Count; i < count; i++)
		{
			ShapeImpl sourceShape = (ShapeImpl)shapeCollectionBase[i];
			m_shapes.AddCopy(sourceShape, hashNewNames, dicFontIndexes);
		}
	}

	protected virtual void CopyOptions(WorksheetBaseImpl sourceSheet)
	{
		m_sheetProtection = (SheetProtectionRecord)CloneUtils.CloneCloneable(sourceSheet.m_sheetProtection);
		if (sourceSheet.m_password != null)
		{
			m_password = (PasswordRecord)sourceSheet.m_password.Clone();
		}
		m_iZoom = sourceSheet.m_iZoom;
		if (sourceSheet.m_windowTwo != null)
		{
			m_windowTwo = (WindowTwoRecord)sourceSheet.m_windowTwo.Clone();
			m_windowTwo.IsSelected = false;
			m_windowTwo.IsPaged = false;
		}
		CopyTabColor(sourceSheet);
		string strCodeName = sourceSheet.m_strCodeName;
		if (strCodeName != null && strCodeName.Length > 0)
		{
			m_strCodeName = GenerateUniqueName(GetCodeName, strCodeName);
		}
	}

	private string GenerateUniqueName(NameGetter getName, string sourceCodeName)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		ITabSheets tabSheets = m_book.TabSheets;
		bool flag = true;
		int i = 0;
		for (int count = tabSheets.Count; i < count; i++)
		{
			if (Index != i)
			{
				string text = getName(tabSheets[i]);
				if (text == sourceCodeName)
				{
					flag = false;
				}
				if (!dictionary.ContainsKey(text))
				{
					dictionary.Add(text, null);
				}
			}
		}
		if (!flag)
		{
			int num = 0;
			string text2 = sourceCodeName;
			while (dictionary.ContainsKey(text2))
			{
				num++;
				text2 = sourceCodeName + "_" + num;
				if (text2.Length > 31)
				{
					num = 0;
					text2 = (sourceCodeName = sourceCodeName.Remove(sourceCodeName.Length - 1));
				}
			}
			sourceCodeName = text2;
		}
		return sourceCodeName;
	}

	private string GetCodeName(ITabSheet tabSheet)
	{
		return tabSheet.CodeName;
	}

	private string GetName(ITabSheet tabSheet)
	{
		return tabSheet.Name;
	}

	private void CopyTabColor(WorksheetBaseImpl sourceSheet)
	{
		if (sourceSheet == null)
		{
			throw new ArgumentNullException("sourceSheet");
		}
		if (sourceSheet.m_tabColor == null)
		{
			m_tabColor = sourceSheet.m_tabColor;
			return;
		}
		if (m_tabColor == null)
		{
			m_tabColor = new ChartColor(OfficeKnownColors.Black);
		}
		m_tabColor.CopyFrom(sourceSheet.m_tabColor, callEvent: true);
	}

	private void CheckParseOnDemand()
	{
	}

	public override void Dispose()
	{
		base.Dispose();
		if (m_shapes != null)
		{
			for (int i = 0; i < m_shapes.Count; i++)
			{
				(m_shapes[i] as ShapeImpl).Dispose();
			}
			m_shapes.Clear();
			m_shapes = null;
		}
		if (this.NameChanged != null)
		{
			this.NameChanged = null;
		}
		if (m_tabColor != null)
		{
			m_tabColor.Dispose();
			m_tabColor = null;
		}
	}

	protected byte[] CreateSalt(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		byte[] array = new byte[length];
		Random random = new Random((int)DateTime.Now.Ticks);
		int maxValue = 256;
		for (int i = 0; i < length; i++)
		{
			array[i] = (byte)random.Next(maxValue);
		}
		return array;
	}

	protected virtual void SetParent(IApplication application, WorksheetImpl workSheet)
	{
		SetParent(application, (object)workSheet);
		FindParents();
		InitializeCollections();
	}
}
