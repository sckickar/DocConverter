using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class PageSetupBaseImpl : CommonObject, IPageSetupBase, IParentApplication, IBiffStorage
{
	protected enum THeaderSide
	{
		Left,
		Center,
		Right
	}

	public sealed class PaperSizeEntry
	{
		public double Width;

		public double Height;

		private PaperSizeEntry()
		{
		}

		public PaperSizeEntry(double width, double height, MeasureUnits units)
		{
			Width = ApplicationImpl.ConvertUnitsStatic(width, units, MeasureUnits.Point);
			Height = ApplicationImpl.ConvertUnitsStatic(height, units, MeasureUnits.Point);
		}
	}

	public const double DEFAULT_TOPMARGIN = 1.0;

	public const double DEFAULT_BOTTOMMARGIN = 1.0;

	public const double DEFAULT_LEFTMARGIN = 0.75;

	public const double DEFAULT_RIGHTMARGIN = 0.75;

	private static readonly string[] DEF_HEADER_NAMES = new string[3] { "LH", "CH", "RH" };

	private static readonly string[] DEF_FOOTER_NAMES = new string[3] { "LF", "CF", "RF" };

	protected bool m_bHCenter;

	protected bool m_bVCenter;

	[CLSCompliant(false)]
	protected PrinterSettingsRecord m_unknown;

	[CLSCompliant(false)]
	protected PrintSetupRecord m_setup;

	[CLSCompliant(false)]
	protected double m_dBottomMargin = 1.0;

	[CLSCompliant(false)]
	protected double m_dLeftMargin = 0.75;

	[CLSCompliant(false)]
	protected double m_dRightMargin = 0.75;

	[CLSCompliant(false)]
	protected double m_dTopMargin = 1.0;

	[CLSCompliant(false)]
	protected string[] m_arrHeaders = new string[3]
	{
		string.Empty,
		string.Empty,
		string.Empty
	};

	[CLSCompliant(false)]
	protected string[] m_arrFooters = new string[3]
	{
		string.Empty,
		string.Empty,
		string.Empty
	};

	private WorksheetBaseImpl m_sheet;

	[CLSCompliant(false)]
	protected BitmapRecord m_backgroundImage;

	private bool m_bFitToPage;

	internal Dictionary<OfficePaperSize, double> dictPaperWidth = new Dictionary<OfficePaperSize, double>();

	internal Dictionary<OfficePaperSize, double> dictPaperHeight = new Dictionary<OfficePaperSize, double>();

	[CLSCompliant(false)]
	protected HeaderAndFooterRecord m_headerFooter;

	private int m_headerStringLimit = 255;

	private int m_footerStringLimit = 255;

	public virtual bool IsFitToPage
	{
		get
		{
			return m_bFitToPage;
		}
		set
		{
			m_bFitToPage = value;
		}
	}

	public int FitToPagesTall
	{
		get
		{
			return m_setup.FitHeight;
		}
		set
		{
			ushort num = (ushort)value;
			if (m_setup.FitHeight != num)
			{
				m_setup.FitHeight = num;
				SetChanged();
			}
			if (!m_sheet.ParentWorkbook.IsWorkbookOpening)
			{
				IsFitToPage = value > 0 || FitToPagesWide > 0;
			}
		}
	}

	public int FitToPagesWide
	{
		get
		{
			return m_setup.FitWidth;
		}
		set
		{
			ushort num = (ushort)value;
			if (m_setup.FitWidth != num)
			{
				m_setup.FitWidth = num;
				SetChanged();
			}
			if (!m_sheet.ParentWorkbook.IsWorkbookOpening)
			{
				IsFitToPage = value > 0 || FitToPagesTall > 0;
			}
		}
	}

	public bool IsNotValidSettings
	{
		get
		{
			return m_setup.IsNotValidSettings;
		}
		internal set
		{
			m_setup.IsNotValidSettings = value;
		}
	}

	public bool AutoFirstPageNumber
	{
		get
		{
			return !m_setup.IsUsePage;
		}
		set
		{
			m_setup.IsUsePage = !value;
		}
	}

	public bool BlackAndWhite
	{
		get
		{
			return m_setup.IsNoColor;
		}
		set
		{
			if (m_setup.IsNoColor != value)
			{
				m_setup.IsNoColor = value;
				SetChanged();
			}
		}
	}

	public double BottomMargin
	{
		get
		{
			return m_dBottomMargin;
		}
		set
		{
			if (m_dBottomMargin != value)
			{
				m_dBottomMargin = value;
				SetChanged();
			}
		}
	}

	public string CenterFooter
	{
		get
		{
			return m_arrFooters[1];
		}
		set
		{
			if (m_arrFooters[0] != value)
			{
				if (value != "" && CenterFooter.Length == 0)
				{
					m_footerStringLimit -= 2;
				}
				else if (CenterFooter.Length != 0 && value == "")
				{
					m_footerStringLimit += 2;
				}
				if (LeftFooter.Length + value.Length + RightFooter.Length > m_footerStringLimit)
				{
					throw new ArgumentOutOfRangeException("value", "The string is too long. Reduce the number of characters used.");
				}
				m_arrFooters[1] = value;
				SetChanged();
			}
		}
	}

	public Image CenterFooterImage
	{
		get
		{
			return ((BitmapShapeImpl)m_sheet.HeaderFooterShapes[DEF_FOOTER_NAMES[1]])?.Picture;
		}
		set
		{
			m_sheet.HeaderFooterShapes.SetPicture(DEF_FOOTER_NAMES[1], value);
		}
	}

	public Image CenterHeaderImage
	{
		get
		{
			return ((BitmapShapeImpl)m_sheet.HeaderFooterShapes[DEF_HEADER_NAMES[1]])?.Picture;
		}
		set
		{
			m_sheet.HeaderFooterShapes.SetPicture(DEF_HEADER_NAMES[1], value);
		}
	}

	public string CenterHeader
	{
		get
		{
			return m_arrHeaders[1];
		}
		set
		{
			if (m_arrHeaders[1] != value)
			{
				if (value != "" && CenterHeader.Length == 0)
				{
					m_headerStringLimit -= 2;
				}
				else if (CenterHeader.Length != 0 && value == "")
				{
					m_headerStringLimit += 2;
				}
				if (LeftHeader.Length + value.Length + RightHeader.Length > m_headerStringLimit)
				{
					throw new ArgumentOutOfRangeException("value", "The string is too long. Reduce the number of characters used.");
				}
				m_arrHeaders[1] = value;
				SetChanged();
			}
		}
	}

	public bool CenterHorizontally
	{
		get
		{
			return m_bHCenter;
		}
		set
		{
			if (m_bHCenter != value)
			{
				m_bHCenter = value;
				SetChanged();
			}
		}
	}

	public bool CenterVertically
	{
		get
		{
			return m_bVCenter;
		}
		set
		{
			if (m_bVCenter != value)
			{
				m_bVCenter = value;
				SetChanged();
			}
		}
	}

	public int Copies
	{
		get
		{
			return m_setup.Copies;
		}
		set
		{
			if (value < 1)
			{
				if (!((WorkbookImpl)m_sheet.Workbook).IsWorkbookOpening)
				{
					throw new ArgumentOutOfRangeException("Number of copies can not be less then 1");
				}
				value = 1;
			}
			if (m_setup.Copies != (ushort)value)
			{
				m_setup.Copies = (ushort)value;
				m_setup.IsNotValidSettings = false;
				SetChanged();
			}
		}
	}

	public bool Draft
	{
		get
		{
			return m_setup.IsDraft;
		}
		set
		{
			if (m_setup.IsDraft != value)
			{
				m_setup.IsDraft = value;
				SetChanged();
			}
		}
	}

	public short FirstPageNumber
	{
		get
		{
			return m_setup.PageStart;
		}
		set
		{
			if (m_setup.PageStart != value)
			{
				m_setup.PageStart = value;
				AutoFirstPageNumber = false;
				SetChanged();
			}
		}
	}

	public double FooterMargin
	{
		get
		{
			return m_setup.FooterMargin;
		}
		set
		{
			if (m_setup.FooterMargin != value)
			{
				m_setup.FooterMargin = value;
				SetChanged();
			}
		}
	}

	public double HeaderMargin
	{
		get
		{
			return m_setup.HeaderMargin;
		}
		set
		{
			if (m_setup.HeaderMargin != value)
			{
				m_setup.HeaderMargin = value;
				SetChanged();
			}
		}
	}

	public string LeftFooter
	{
		get
		{
			return m_arrFooters[0];
		}
		set
		{
			if (m_arrFooters[0] != value)
			{
				if (value != "" && LeftFooter.Length == 0)
				{
					m_footerStringLimit -= 2;
				}
				else if (LeftFooter.Length != 0 && value == "")
				{
					m_footerStringLimit += 2;
				}
				if (value.Length + CenterFooter.Length + RightFooter.Length > m_footerStringLimit)
				{
					throw new ArgumentOutOfRangeException("value", "The string is too long. Reduce the number of characters used.");
				}
				m_arrFooters[0] = value;
				SetChanged();
			}
		}
	}

	public string LeftHeader
	{
		get
		{
			return m_arrHeaders[0];
		}
		set
		{
			if (m_arrHeaders[0] != value)
			{
				if (value != "" && LeftHeader.Length == 0)
				{
					m_headerStringLimit -= 2;
				}
				else if (LeftHeader.Length != 0 && value == "")
				{
					m_headerStringLimit += 2;
				}
				if (value.Length + CenterHeader.Length + RightHeader.Length > m_headerStringLimit)
				{
					throw new ArgumentOutOfRangeException("value", "The string is too long. Reduce the number of characters used.");
				}
				m_arrHeaders[0] = value;
				SetChanged();
			}
		}
	}

	public Image LeftFooterImage
	{
		get
		{
			return ((BitmapShapeImpl)m_sheet.HeaderFooterShapes[DEF_FOOTER_NAMES[0]])?.Picture;
		}
		set
		{
			m_sheet.HeaderFooterShapes.SetPicture(DEF_FOOTER_NAMES[0], value);
		}
	}

	public Image LeftHeaderImage
	{
		get
		{
			return ((BitmapShapeImpl)m_sheet.HeaderFooterShapes[DEF_HEADER_NAMES[0]])?.Picture;
		}
		set
		{
			m_sheet.HeaderFooterShapes.SetPicture(DEF_HEADER_NAMES[0], value);
		}
	}

	public double LeftMargin
	{
		get
		{
			return m_dLeftMargin;
		}
		set
		{
			if (m_dLeftMargin != value)
			{
				m_dLeftMargin = value;
				SetChanged();
			}
		}
	}

	public OfficeOrder Order
	{
		get
		{
			if (!m_setup.IsLeftToRight)
			{
				return OfficeOrder.DownThenOver;
			}
			return OfficeOrder.OverThenDown;
		}
		set
		{
			bool flag = value == OfficeOrder.OverThenDown;
			if (m_setup.IsLeftToRight != flag)
			{
				m_setup.IsLeftToRight = flag;
				SetChanged();
			}
		}
	}

	public OfficePageOrientation Orientation
	{
		get
		{
			if (!m_setup.IsNotLandscape)
			{
				return OfficePageOrientation.Landscape;
			}
			return OfficePageOrientation.Portrait;
		}
		set
		{
			m_setup.IsNotLandscape = value == OfficePageOrientation.Portrait;
			m_setup.IsNotValidSettings = false;
			m_setup.IsNoOrientation = false;
			SetChanged();
		}
	}

	public OfficePaperSize PaperSize
	{
		get
		{
			return (OfficePaperSize)m_setup.PaperSize;
		}
		set
		{
			m_setup.PaperSize = (ushort)value;
			m_setup.IsNotValidSettings = false;
			SetChanged();
		}
	}

	public OfficePrintLocation PrintComments
	{
		get
		{
			if (!m_setup.IsNotes)
			{
				return OfficePrintLocation.PrintNoComments;
			}
			if (m_setup.IsPrintNotesAsDisplayed)
			{
				return OfficePrintLocation.PrintInPlace;
			}
			return OfficePrintLocation.PrintSheetEnd;
		}
		set
		{
			switch (value)
			{
			case OfficePrintLocation.PrintNoComments:
				m_setup.IsNotes = false;
				break;
			case OfficePrintLocation.PrintInPlace:
				m_setup.IsNotes = true;
				m_setup.IsPrintNotesAsDisplayed = true;
				break;
			case OfficePrintLocation.PrintSheetEnd:
				m_setup.IsNotes = true;
				m_setup.IsPrintNotesAsDisplayed = false;
				break;
			}
			SetChanged();
		}
	}

	public OfficePrintErrors PrintErrors
	{
		get
		{
			return m_setup.PrintErrors;
		}
		set
		{
			if (m_setup.PrintErrors != value)
			{
				m_setup.PrintErrors = value;
				SetChanged();
			}
		}
	}

	public bool PrintNotes
	{
		get
		{
			return m_setup.IsNotes;
		}
		set
		{
			if (m_setup.IsNotes != value)
			{
				m_setup.IsNotes = value;
				SetChanged();
			}
		}
	}

	public int PrintQuality
	{
		get
		{
			return m_setup.HResolution;
		}
		set
		{
			m_setup.HResolution = (ushort)value;
			m_setup.VResolution = (ushort)value;
			m_setup.IsNotValidSettings = false;
			SetChanged();
		}
	}

	public string RightFooter
	{
		get
		{
			return m_arrFooters[2];
		}
		set
		{
			if (m_arrFooters[2] != value)
			{
				if (value != "" && RightFooter.Length == 0)
				{
					m_footerStringLimit -= 2;
				}
				else if (RightFooter.Length != 0 && value == "")
				{
					m_footerStringLimit += 2;
				}
				if (LeftFooter.Length + CenterFooter.Length + value.Length > m_footerStringLimit)
				{
					throw new ArgumentOutOfRangeException("value", "The string is too long. Reduce the number of characters used.");
				}
				m_arrFooters[2] = value;
				SetChanged();
			}
		}
	}

	public Image RightFooterImage
	{
		get
		{
			return ((BitmapShapeImpl)m_sheet.HeaderFooterShapes[DEF_FOOTER_NAMES[2]])?.Picture;
		}
		set
		{
			m_sheet.HeaderFooterShapes.SetPicture(DEF_FOOTER_NAMES[2], value);
		}
	}

	public string RightHeader
	{
		get
		{
			return m_arrHeaders[2];
		}
		set
		{
			if (m_arrHeaders[2] != value)
			{
				if (value != "" && RightHeader.Length == 0)
				{
					m_headerStringLimit -= 2;
				}
				else if (RightHeader.Length != 0 && value == "")
				{
					m_headerStringLimit += 2;
				}
				if (LeftHeader.Length + CenterHeader.Length + value.Length > m_headerStringLimit)
				{
					throw new ArgumentOutOfRangeException("value", "The string is too long. Reduce the number of characters used.");
				}
				m_arrHeaders[2] = value;
				SetChanged();
			}
		}
	}

	public Image RightHeaderImage
	{
		get
		{
			return ((BitmapShapeImpl)m_sheet.HeaderFooterShapes[DEF_HEADER_NAMES[2]])?.Picture;
		}
		set
		{
			m_sheet.HeaderFooterShapes.SetPicture(DEF_HEADER_NAMES[2], value);
		}
	}

	public double RightMargin
	{
		get
		{
			return m_dRightMargin;
		}
		set
		{
			if (m_dRightMargin != value)
			{
				m_dRightMargin = value;
				SetChanged();
			}
		}
	}

	public double TopMargin
	{
		get
		{
			return m_dTopMargin;
		}
		set
		{
			if (m_dTopMargin != value)
			{
				m_dTopMargin = value;
				SetChanged();
			}
		}
	}

	public int Zoom
	{
		get
		{
			return m_setup.Scale;
		}
		set
		{
			if (value < 10 || value > 400)
			{
				throw new ArgumentOutOfRangeException("Zoom value must be beetween 10 and 400 percent.");
			}
			m_setup.Scale = (ushort)value;
			m_setup.IsNotValidSettings = false;
			SetChanged();
		}
	}

	public Image BackgoundImage
	{
		get
		{
			if (m_backgroundImage == null)
			{
				return null;
			}
			return m_backgroundImage.Picture;
		}
		set
		{
			if (value == null)
			{
				m_backgroundImage = null;
				return;
			}
			if (m_backgroundImage == null)
			{
				m_backgroundImage = (BitmapRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Bitmap);
			}
			m_backgroundImage.Picture = value;
		}
	}

	public double PageWidth
	{
		get
		{
			PaperSizeEntry paperSizeEntry = (base.AppImplementation.DicPaperSizeTable.ContainsKey((int)PaperSize) ? base.AppImplementation.DicPaperSizeTable[(int)PaperSize] : base.AppImplementation.DicPaperSizeTable[9]);
			if (Orientation != OfficePageOrientation.Portrait)
			{
				return paperSizeEntry.Height;
			}
			return paperSizeEntry.Width;
		}
	}

	public double PageHeight
	{
		get
		{
			PaperSizeEntry paperSizeEntry = (base.AppImplementation.DicPaperSizeTable.ContainsKey((int)PaperSize) ? base.AppImplementation.DicPaperSizeTable[(int)PaperSize] : base.AppImplementation.DicPaperSizeTable[9]);
			if (Orientation != OfficePageOrientation.Portrait)
			{
				return paperSizeEntry.Width;
			}
			return paperSizeEntry.Height;
		}
	}

	public int HResolution
	{
		get
		{
			return m_setup.HResolution;
		}
		set
		{
			m_setup.HResolution = (ushort)value;
		}
	}

	public int VResolution
	{
		get
		{
			return m_setup.VResolution;
		}
		set
		{
			m_setup.VResolution = (ushort)value;
		}
	}

	public string FullHeaderString
	{
		get
		{
			return CreateHeaderFooterString(m_arrHeaders);
		}
		set
		{
			m_arrHeaders = ParseHeaderFooterString(value);
		}
	}

	public string FullFooterString
	{
		get
		{
			return CreateHeaderFooterString(m_arrFooters);
		}
		set
		{
			m_arrFooters = ParseHeaderFooterString(value);
		}
	}

	public bool AlignHFWithPageMargins
	{
		get
		{
			return m_headerFooter.AlignHFWithPageMargins;
		}
		set
		{
			m_headerFooter.AlignHFWithPageMargins = value;
		}
	}

	public bool DifferentFirstPageHF
	{
		get
		{
			return m_headerFooter.DifferentFirstPageHF;
		}
		set
		{
			m_headerFooter.DifferentFirstPageHF = value;
		}
	}

	public bool DifferentOddAndEvenPagesHF
	{
		get
		{
			return m_headerFooter.DifferentOddAndEvenPagesHF;
		}
		set
		{
			m_headerFooter.DifferentOddAndEvenPagesHF = value;
		}
	}

	public bool HFScaleWithDoc
	{
		get
		{
			return m_headerFooter.HFScaleWithDoc;
		}
		set
		{
			m_headerFooter.HFScaleWithDoc = value;
		}
	}

	public TBIFFRecord TypeCode => TBIFFRecord.Unknown;

	public int RecordCode => 0;

	public bool NeedDataArray => false;

	public long StreamPos
	{
		get
		{
			return -1L;
		}
		set
		{
		}
	}

	private void FillMaxPaperSize(ApplicationImpl application)
	{
		dictPaperWidth.Add(OfficePaperSize.A2Paper, application.ConvertUnits(420.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A2Paper, application.ConvertUnits(594.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A3ExtraPaper, application.ConvertUnits(322.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A3ExtraPaper, application.ConvertUnits(445.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A3ExtraTransversePaper, application.ConvertUnits(332.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A3ExtraTransversePaper, application.ConvertUnits(445.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A3TransversePaper, application.ConvertUnits(297.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A3TransversePaper, application.ConvertUnits(420.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A4ExtraPaper, application.ConvertUnits(236.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A4ExtraPaper, application.ConvertUnits(332.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A4PlusPaper, application.ConvertUnits(210.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A4PlusPaper, application.ConvertUnits(330.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A4TransversePaper, application.ConvertUnits(210.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A4TransversePaper, application.ConvertUnits(297.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A5ExtraPpaper, application.ConvertUnits(174.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A5ExtraPpaper, application.ConvertUnits(235.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.A5TransversePaper, application.ConvertUnits(148.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.A5TransversePaper, application.ConvertUnits(210.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.InviteEnvelope, application.ConvertUnits(220.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.InviteEnvelope, application.ConvertUnits(220.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.ISOB4, application.ConvertUnits(250.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.ISOB4, application.ConvertUnits(353.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.ISOB5ExtraPaper, application.ConvertUnits(210.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.ISOB5ExtraPaper, application.ConvertUnits(276.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.JapaneseDoublePostcard, application.ConvertUnits(200.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.JapaneseDoublePostcard, application.ConvertUnits(148.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.JISB5TransversePaper, application.ConvertUnits(182.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.JISB5TransversePaper, application.ConvertUnits(257.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.LegalExtraPaper9275By15, 9.275);
		dictPaperHeight.Add(OfficePaperSize.LegalExtraPaper9275By15, 15.0);
		dictPaperWidth.Add(OfficePaperSize.LetterExtraPaper9275By12, 9.275);
		dictPaperHeight.Add(OfficePaperSize.LetterExtraPaper9275By12, 12.0);
		dictPaperWidth.Add(OfficePaperSize.LetterExtraTransversePaper, 9.275);
		dictPaperHeight.Add(OfficePaperSize.LetterExtraTransversePaper, 12.0);
		dictPaperWidth.Add(OfficePaperSize.LetterPlusPaper, 8.5);
		dictPaperHeight.Add(OfficePaperSize.LetterPlusPaper, 12.69);
		dictPaperWidth.Add(OfficePaperSize.LetterTransversePaper, 8.275);
		dictPaperHeight.Add(OfficePaperSize.LetterTransversePaper, 11.0);
		dictPaperWidth.Add(OfficePaperSize.Paper10x14, 10.0);
		dictPaperHeight.Add(OfficePaperSize.Paper10x14, 14.0);
		dictPaperWidth.Add(OfficePaperSize.Paper11x17, 11.0);
		dictPaperHeight.Add(OfficePaperSize.Paper11x17, 17.0);
		dictPaperWidth.Add(OfficePaperSize.PaperA3, application.ConvertUnits(297.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperA3, application.ConvertUnits(420.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperA4, application.ConvertUnits(210.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperA4, application.ConvertUnits(297.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperA4Small, application.ConvertUnits(210.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperA4Small, application.ConvertUnits(297.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperA5, application.ConvertUnits(148.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperA5, application.ConvertUnits(210.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperB4, application.ConvertUnits(250.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperB4, application.ConvertUnits(353.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperB5, application.ConvertUnits(176.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperB5, application.ConvertUnits(250.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperCsheet, 17.0);
		dictPaperHeight.Add(OfficePaperSize.PaperCsheet, 22.0);
		dictPaperWidth.Add(OfficePaperSize.PaperDsheet, 22.0);
		dictPaperHeight.Add(OfficePaperSize.PaperDsheet, 34.0);
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelope10, 4.125);
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelope10, 9.5);
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelope11, 4.5);
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelope11, 10.375);
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelope12, 4.75);
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelope12, 11.0);
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelope14, 5.0);
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelope14, 11.5);
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelope9, 3.875);
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelope9, 8.875);
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeB4, application.ConvertUnits(250.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeB4, application.ConvertUnits(353.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeB5, application.ConvertUnits(176.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeB5, application.ConvertUnits(250.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeB6, application.ConvertUnits(176.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeB6, application.ConvertUnits(125.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeC3, application.ConvertUnits(324.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeC3, application.ConvertUnits(458.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeC4, application.ConvertUnits(229.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeC4, application.ConvertUnits(324.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeC5, application.ConvertUnits(162.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeC5, application.ConvertUnits(229.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeC6, application.ConvertUnits(114.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeC6, application.ConvertUnits(162.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeC65, application.ConvertUnits(114.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeC65, application.ConvertUnits(229.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeDL, application.ConvertUnits(110.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeDL, application.ConvertUnits(220.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeItaly, application.ConvertUnits(110.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeItaly, application.ConvertUnits(230.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopeMonarch, 3.875);
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopeMonarch, 7.5);
		dictPaperWidth.Add(OfficePaperSize.PaperEnvelopePersonal, 3.625);
		dictPaperHeight.Add(OfficePaperSize.PaperEnvelopePersonal, 6.5);
		dictPaperWidth.Add(OfficePaperSize.PaperEsheet, 34.0);
		dictPaperHeight.Add(OfficePaperSize.PaperEsheet, 34.0);
		dictPaperWidth.Add(OfficePaperSize.PaperExecutive, 7.5);
		dictPaperHeight.Add(OfficePaperSize.PaperExecutive, 7.5);
		dictPaperWidth.Add(OfficePaperSize.PaperFanfoldLegalGerman, 8.5);
		dictPaperHeight.Add(OfficePaperSize.PaperFanfoldLegalGerman, 13.0);
		dictPaperWidth.Add(OfficePaperSize.PaperFanfoldStdGerman, 8.5);
		dictPaperHeight.Add(OfficePaperSize.PaperFanfoldStdGerman, 12.0);
		dictPaperWidth.Add(OfficePaperSize.PaperFanfoldUS, 14.875);
		dictPaperHeight.Add(OfficePaperSize.PaperFanfoldUS, 11.0);
		dictPaperWidth.Add(OfficePaperSize.PaperFolio, 8.5);
		dictPaperHeight.Add(OfficePaperSize.PaperFolio, 13.0);
		dictPaperWidth.Add(OfficePaperSize.PaperLedger, 17.0);
		dictPaperHeight.Add(OfficePaperSize.PaperLedger, 11.0);
		dictPaperWidth.Add(OfficePaperSize.PaperLegal, 8.5);
		dictPaperHeight.Add(OfficePaperSize.PaperLegal, 14.0);
		dictPaperWidth.Add(OfficePaperSize.PaperLetter, 8.5);
		dictPaperHeight.Add(OfficePaperSize.PaperLetter, 11.0);
		dictPaperWidth.Add(OfficePaperSize.PaperLetterSmall, 8.5);
		dictPaperHeight.Add(OfficePaperSize.PaperLetterSmall, 11.0);
		dictPaperWidth.Add(OfficePaperSize.PaperNote, 8.5);
		dictPaperHeight.Add(OfficePaperSize.PaperNote, 11.0);
		dictPaperWidth.Add(OfficePaperSize.PaperQuarto, application.ConvertUnits(215.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.PaperQuarto, application.ConvertUnits(275.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.PaperStatement, 5.5);
		dictPaperHeight.Add(OfficePaperSize.PaperStatement, 8.5);
		dictPaperWidth.Add(OfficePaperSize.PaperTabloid, 11.0);
		dictPaperHeight.Add(OfficePaperSize.PaperTabloid, 17.0);
		dictPaperWidth.Add(OfficePaperSize.StandardPaper10By11, 10.0);
		dictPaperHeight.Add(OfficePaperSize.StandardPaper10By11, 11.0);
		dictPaperWidth.Add(OfficePaperSize.StandardPaper15By11, 15.0);
		dictPaperHeight.Add(OfficePaperSize.StandardPaper15By11, 11.0);
		dictPaperWidth.Add(OfficePaperSize.StandardPaper9By11, 9.0);
		dictPaperHeight.Add(OfficePaperSize.StandardPaper9By11, 11.0);
		dictPaperWidth.Add(OfficePaperSize.SuperASuperAA4Paper, application.ConvertUnits(227.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.SuperASuperAA4Paper, application.ConvertUnits(356.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.SuperBSuperBA3Paper, application.ConvertUnits(305.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperHeight.Add(OfficePaperSize.SuperBSuperBA3Paper, application.ConvertUnits(487.0, MeasureUnits.Millimeter, MeasureUnits.Inch));
		dictPaperWidth.Add(OfficePaperSize.TabloidExtraPaper, 11.69);
		dictPaperHeight.Add(OfficePaperSize.TabloidExtraPaper, 18.0);
	}

	public PageSetupBaseImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_setup = (PrintSetupRecord)BiffRecordFactory.GetRecord(TBIFFRecord.PrintSetup);
		m_headerFooter = new HeaderAndFooterRecord();
		FillMaxPaperSize(application as ApplicationImpl);
		FindParents();
	}

	protected virtual void FindParents()
	{
		m_sheet = CommonObject.FindParent(base.Parent, typeof(WorksheetBaseImpl), bSubTypes: true) as WorksheetBaseImpl;
		if (m_sheet == null)
		{
			throw new ArgumentNullException("Parent worksheet.");
		}
	}

	protected string[] ParseHeaderFooterString(string strToSplit)
	{
		if (strToSplit == null)
		{
			throw new ArgumentNullException("strToSplit");
		}
		string[] array = new string[3]
		{
			string.Empty,
			string.Empty,
			string.Empty
		};
		int length = strToSplit.Length;
		if (length == 0)
		{
			return array;
		}
		int num = strToSplit.IndexOf("&L");
		int num2 = strToSplit.IndexOf("&C");
		int num3 = strToSplit.IndexOf("&R");
		if (num == num2 && num2 == num3 && num3 == -1)
		{
			array[1] = strToSplit;
			return array;
		}
		if (num >= 0)
		{
			int num4 = length;
			if (num2 > num)
			{
				num4 = num2;
			}
			else if (num3 > num)
			{
				num4 = num3;
			}
			array[0] = strToSplit.Substring(num + 2, num4 - num - 2);
		}
		if (num2 >= 0)
		{
			int num5 = length;
			if (num3 > num2)
			{
				num5 = num3;
			}
			array[1] = strToSplit.Substring(num2 + 2, num5 - num2 - 2);
			if (num2 > 0 && num < 0)
			{
				array[0] = strToSplit.Substring(0, num2);
			}
		}
		if (num3 >= 0)
		{
			int num6 = length;
			array[2] = strToSplit.Substring(num3 + 2, num6 - num3 - 2);
			if (num3 > 0 && num2 < 0 && num < 0)
			{
				array[1] = strToSplit.Substring(0, num3);
			}
		}
		return array;
	}

	protected string CreateHeaderFooterString(string[] parts)
	{
		if (parts == null)
		{
			throw new ArgumentNullException("parts");
		}
		if (parts.Length < 3 || parts.Length > 3)
		{
			throw new ArgumentException("Parts array must have only three elements", "parts");
		}
		string text = string.Empty;
		if (parts[0] != null && parts[0].Length > 0)
		{
			text = text + "&L" + parts[0];
		}
		if (parts[1] != null && parts[1].Length > 0)
		{
			text = text + "&C" + parts[1];
		}
		if (parts[2] != null && parts[2].Length > 0)
		{
			text = text + "&R" + parts[2];
		}
		return text;
	}

	[CLSCompliant(false)]
	public virtual void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_setup == null)
		{
			throw new ArgumentNullException("m_Setup");
		}
		SerializeStartRecords(records);
		HeaderFooterRecord headerFooterRecord = (HeaderFooterRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Header);
		headerFooterRecord.Value = CreateHeaderFooterString(m_arrHeaders);
		records.Add(headerFooterRecord);
		HeaderFooterRecord headerFooterRecord2 = (HeaderFooterRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Footer);
		headerFooterRecord2.Value = CreateHeaderFooterString(m_arrFooters);
		records.Add(headerFooterRecord2);
		SheetCenterRecord sheetCenterRecord = (SheetCenterRecord)BiffRecordFactory.GetRecord(TBIFFRecord.HCenter);
		sheetCenterRecord.IsCenter = (m_bHCenter ? ((ushort)1) : ((ushort)0));
		records.Add(sheetCenterRecord);
		SheetCenterRecord sheetCenterRecord2 = (SheetCenterRecord)BiffRecordFactory.GetRecord(TBIFFRecord.VCenter);
		sheetCenterRecord2.IsCenter = (m_bVCenter ? ((ushort)1) : ((ushort)0));
		records.Add(sheetCenterRecord2);
		SerializeMargin(records, TBIFFRecord.LeftMargin, m_dLeftMargin, 0.75);
		SerializeMargin(records, TBIFFRecord.RightMargin, m_dRightMargin, 0.75);
		SerializeMargin(records, TBIFFRecord.TopMargin, m_dTopMargin, 1.0);
		SerializeMargin(records, TBIFFRecord.BottomMargin, m_dBottomMargin, 1.0);
		if (m_unknown != null)
		{
			records.Add(m_unknown);
		}
		records.Add(m_setup);
		if (m_headerFooter != null && m_headerFooter.Length != -1)
		{
			records.Add(m_headerFooter);
		}
		SerializeEndRecords(records);
	}

	[CLSCompliant(false)]
	protected virtual void SerializeStartRecords(OffsetArrayList records)
	{
	}

	[CLSCompliant(false)]
	protected virtual void SerializeEndRecords(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_backgroundImage != null)
		{
			records.Add(m_backgroundImage);
		}
	}

	public virtual int Parse(IList<BiffRecordRaw> data, int position)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (position < 0 || position > data.Count - 1)
		{
			throw new ArgumentOutOfRangeException("position", "Value cannot be less than 0 and greater than data.Count - 1");
		}
		int count = data.Count;
		while (position < count)
		{
			BiffRecordRaw record = data[position];
			if (!ParseRecord(record))
			{
				position--;
				break;
			}
			position++;
		}
		return position;
	}

	[CLSCompliant(false)]
	protected virtual bool ParseRecord(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		MarginRecord marginRecord = null;
		switch (record.TypeCode)
		{
		case TBIFFRecord.Header:
		{
			HeaderFooterRecord headerFooterRecord2 = (HeaderFooterRecord)record;
			m_arrHeaders = ParseHeaderFooterString(headerFooterRecord2.Value);
			break;
		}
		case TBIFFRecord.Footer:
		{
			HeaderFooterRecord headerFooterRecord = (HeaderFooterRecord)record;
			m_arrFooters = ParseHeaderFooterString(headerFooterRecord.Value);
			break;
		}
		case TBIFFRecord.HCenter:
		{
			SheetCenterRecord sheetCenterRecord2 = (SheetCenterRecord)record;
			m_bHCenter = sheetCenterRecord2.IsCenter != 0;
			break;
		}
		case TBIFFRecord.VCenter:
		{
			SheetCenterRecord sheetCenterRecord = (SheetCenterRecord)record;
			m_bVCenter = sheetCenterRecord.IsCenter != 0;
			break;
		}
		case TBIFFRecord.PrintSetup:
			m_setup = (PrintSetupRecord)record;
			break;
		case TBIFFRecord.Bitmap:
			m_backgroundImage = (BitmapRecord)record;
			break;
		case TBIFFRecord.LeftMargin:
			marginRecord = (MarginRecord)record;
			m_dLeftMargin = marginRecord.Margin;
			break;
		case TBIFFRecord.RightMargin:
			marginRecord = (MarginRecord)record;
			m_dRightMargin = marginRecord.Margin;
			break;
		case TBIFFRecord.TopMargin:
			marginRecord = (MarginRecord)record;
			m_dTopMargin = marginRecord.Margin;
			break;
		case TBIFFRecord.BottomMargin:
			marginRecord = (MarginRecord)record;
			m_dBottomMargin = marginRecord.Margin;
			break;
		case TBIFFRecord.PrinterSettings:
			m_unknown = (PrinterSettingsRecord)record;
			break;
		case TBIFFRecord.HeaderFooter:
			m_headerFooter = (HeaderAndFooterRecord)record;
			break;
		default:
			return false;
		}
		return true;
	}

	[CLSCompliant(false)]
	protected BiffRecordRaw GetOrCreateRecord(IList data, ref int pos, TBIFFRecord type)
	{
		BiffRecordRaw biffRecordRaw = (BiffRecordRaw)data[pos];
		if (biffRecordRaw.TypeCode != type)
		{
			biffRecordRaw = BiffRecordFactory.GetRecord(type);
		}
		else
		{
			pos++;
		}
		return biffRecordRaw;
	}

	[CLSCompliant(false)]
	protected BiffRecordRaw GetRecordUpdatePos(IList data, ref int pos)
	{
		return (BiffRecordRaw)data[pos++];
	}

	[CLSCompliant(false)]
	protected BiffRecordRaw GetRecordUpdatePos(IList data, ref int pos, TBIFFRecord type)
	{
		BiffRecordRaw biffRecordRaw = null;
		do
		{
			biffRecordRaw = (BiffRecordRaw)data[pos];
			pos++;
			if (pos >= data.Count)
			{
				biffRecordRaw = null;
				break;
			}
		}
		while (biffRecordRaw.TypeCode != type);
		return biffRecordRaw;
	}

	private void SerializeMargin(OffsetArrayList records, TBIFFRecord code, double marginValue, double defaultValue)
	{
		if (marginValue != defaultValue)
		{
			MarginRecord marginRecord = (MarginRecord)BiffRecordFactory.GetRecord(code);
			marginRecord.Margin = marginValue;
			records.Add(marginRecord);
		}
	}

	protected void SetChanged()
	{
		m_sheet.SetChanged();
	}

	public virtual int GetStoreSize(OfficeVersion version)
	{
		int num = 12 + m_setup.GetStoreSize(version) + 4 + ((m_unknown != null) ? (m_unknown.GetStoreSize(version) + 4) : 0) + ((m_dBottomMargin != 1.0) ? 12 : 0) + ((m_dTopMargin != 1.0) ? 12 : 0) + ((m_dRightMargin != 0.75) ? 12 : 0) + ((m_dLeftMargin != 0.75) ? 12 : 0);
		int length = FullHeaderString.Length;
		int length2 = FullFooterString.Length;
		if (length > 0)
		{
			num += length * 2 + 3;
		}
		num += 4;
		if (length2 > 0)
		{
			num += length2 * 2 + 3;
		}
		if (m_backgroundImage != null)
		{
			num += m_backgroundImage.GetStoreSize(version) + 4;
		}
		if (m_headerFooter != null && m_headerFooter.Length > 0)
		{
			num += m_headerFooter.GetStoreSize(version) + 4;
		}
		return num;
	}

	public override void Dispose()
	{
		base.Dispose();
		if (m_setup != null)
		{
			m_setup = null;
		}
		if (m_unknown != null && m_sheet == null)
		{
			m_unknown.Dispose();
		}
		if (dictPaperHeight != null)
		{
			dictPaperHeight.Clear();
			dictPaperHeight = null;
		}
		if (dictPaperWidth != null)
		{
			dictPaperWidth.Clear();
			dictPaperWidth = null;
		}
		GC.SuppressFinalize(this);
	}
}
