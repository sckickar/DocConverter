using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Parser;

[CLSCompliant(false)]
internal class BiffRecordFactory
{
	private const int DEF_RESERVE_SIZE = 200;

	private static Dictionary<int, BiffRecordRaw> m_dict;

	static BiffRecordFactory()
	{
		m_dict = new Dictionary<int, BiffRecordRaw>(200);
		FillFactory();
	}

	private static void FillFactory()
	{
		m_dict[449] = new RecalcIdRecord();
		m_dict[448] = new UnknownBeginRecord();
		m_dict[29] = new SelectionRecord();
		m_dict[42] = new PrintHeadersRecord();
		m_dict[516] = new LabelRecord();
		m_dict[10] = new EOFRecord();
		m_dict[34] = new DateWindow1904Record();
		m_dict[4197] = new ChartSiIndexRecord();
		m_dict[4099] = new ChartSeriesRecord();
		m_dict[4121] = new ChartPieRecord();
		m_dict[4130] = new ChartFormatLinkRecord();
		m_dict[4125] = new ChartAxisRecord();
		m_dict[4166] = new ChartAxesUsedRecord();
		m_dict[92] = new WriteAccessRecord();
		m_dict[566] = new TableRecord();
		m_dict[18] = new ProtectRecord();
		m_dict[189] = new MulRKRecord();
		m_dict[236] = new MSODrawingRecord();
		BiffRecordRaw biffRecordRaw = new MarginRecord();
		biffRecordRaw.SetRecordCode(38);
		m_dict[38] = biffRecordRaw;
		biffRecordRaw = new MarginRecord();
		biffRecordRaw.SetRecordCode(41);
		m_dict[41] = biffRecordRaw;
		biffRecordRaw = new MarginRecord();
		biffRecordRaw.SetRecordCode(39);
		m_dict[39] = biffRecordRaw;
		biffRecordRaw = new MarginRecord();
		biffRecordRaw.SetRecordCode(40);
		m_dict[40] = biffRecordRaw;
		m_dict[434] = new DValRecord();
		m_dict[549] = new DefaultRowHeightRecord();
		m_dict[1048] = new CustomPropertyRecord();
		m_dict[2129] = new ChartWrapperRecord();
		m_dict[4165] = new ChartSertocrtRecord();
		m_dict[4135] = new ChartObjectLinkRecord();
		m_dict[4198] = new ChartGelFrameRecord();
		m_dict[4098] = new ChartChartRecord();
		m_dict[4191] = new Chart3DDataFormatRecord();
		m_dict[218] = new BookBoolRecord();
		m_dict[445] = new UnkMacrosDisable();
		m_dict[6] = new FormulaRecord();
		m_dict[47] = new FilePassRecord();
		m_dict[4129] = new ChartAxisLineFormatRecord();
		m_dict[2057] = new BOFRecord();
		m_dict[129] = new WSBoolRecord();
		m_dict[26] = new VerticalPageBreaksRecord();
		m_dict[28] = new NoteRecord();
		m_dict[432] = new CondFMTRecord();
		m_dict[4097] = new ChartUnitsRecord();
		m_dict[4170] = new ChartSerParentRecord();
		m_dict[4171] = new ChartSerAuxTrendRecord();
		m_dict[4158] = new ChartRadarRecord();
		m_dict[4175] = new ChartPosRecord();
		m_dict[2155] = new ChartDataLabelsRecord();
		m_dict[4108] = new ChartAttachedLabelRecord();
		m_dict[2205] = new ChartAttachedLabelLayoutRecord();
		m_dict[2215] = new ChartPlotAreaLayoutRecord();
		m_dict[517] = new BoolErrRecord();
		m_dict[95] = new SaveRecalcRecord();
		m_dict[19] = new PasswordRecord();
		m_dict[93] = new OBJRecord();
		m_dict[440] = new HLinkRecord();
		m_dict[4193] = new ChartBoppopRecord();
		m_dict[4161] = new ChartAxisParentRecord();
		m_dict[2135] = new ChartAxisDisplayUnitsRecord();
		m_dict[433] = new CFRecord();
		m_dict[2171] = new CFExRecord();
		m_dict[2170] = new CF12Record();
		m_dict[2169] = new CondFmt12Record();
		m_dict[12] = new CalcCountRecord();
		m_dict[61] = new WindowOneRecord();
		m_dict[161] = new PrintSetupRecord();
		m_dict[523] = new IndexRecord();
		m_dict[235] = new MSODrawingGroupRecord();
		m_dict[2150] = new HeaderFooterImageRecord();
		m_dict[1054] = new FormatRecord();
		m_dict[4164] = new ChartShtpropsRecord();
		m_dict[4199] = new ChartBoppCustomRecord();
		m_dict[89] = new XCTRecord();
		m_dict[352] = new UseSelFSRecord();
		m_dict[214] = new RStringRecord();
		m_dict[222] = new OleSizeRecord();
		m_dict[437] = new DConBinRecord();
		m_dict[66] = new CodepageRecord();
		m_dict[4132] = new ChartDefaultTextRecord();
		m_dict[4102] = new ChartDataFormatRecord();
		m_dict[4122] = new ChartAreaRecord();
		m_dict[134] = new WriteProtection();
		m_dict[431] = new ProtectionRev4Record();
		m_dict[444] = new PasswordRev4Record();
		m_dict[190] = new MulBlankRecord();
		m_dict[253] = new LabelSSTRecord();
		m_dict[353] = new DSFRecord();
		m_dict[82] = new DConNameRecord();
		m_dict[4133] = new ChartTextRecord();
		m_dict[4149] = new ChartPlotAreaRecord();
		m_dict[4103] = new ChartLineFormatRecord();
		m_dict[4117] = new ChartLegendRecord();
		m_dict[15] = new RefModeRecord();
		m_dict[14] = new PrecisionRecord();
		m_dict[99] = new ObjectProtectRecord();
		m_dict[27] = new HorizontalPageBreaksRecord();
		m_dict[141] = new HideObjRecord();
		m_dict[23] = new ExternSheetRecord();
		m_dict[4123] = new ChartScatterRecord();
		m_dict[4107] = new ChartPieFormatRecord();
		m_dict[4105] = new ChartMarkerFormatRecord();
		m_dict[2132] = new ChartBegDispUnitRecord();
		m_dict[4119] = new ChartBarRecord();
		m_dict[4106] = new ChartAreaFormatRecord();
		m_dict[4177] = new ChartAIRecord();
		m_dict[64] = new BackupRecord();
		m_dict[25] = new WindowProtectRecord();
		m_dict[438] = new TextObjectRecord();
		m_dict[430] = new SupBookRecord();
		m_dict[24] = new NameRecord();
		m_dict[211] = new HasBasicRecord();
		m_dict[91] = new FileSharingRecord();
		m_dict[215] = new DBCellRecord();
		m_dict[125] = new ColumnInfoRecord();
		m_dict[153] = new DxGCol();
		m_dict[4163] = new ChartLegendxnRecord();
		m_dict[2133] = new ChartEndDispUnitRecord();
		m_dict[233] = new BitmapRecord();
		m_dict[1212] = new SharedFormulaRecord();
		m_dict[446] = new DVRecord();
		m_dict[4174] = new ChartIfmtRecord();
		m_dict[4192] = new ChartFbiRecord();
		m_dict[4195] = new ChartDatRecord();
		m_dict[4147] = new BeginRecord();
		m_dict[0] = new UnknownRecord();
		m_dict[2146] = new SheetLayoutRecord();
		m_dict[520] = new RowRecord();
		m_dict[226] = new InterfaceEndRecord();
		m_dict[128] = new GutsRecord();
		m_dict[130] = new GridsetRecord();
		m_dict[155] = new FilterModeRecord();
		m_dict[140] = new CountryRecord();
		m_dict[4109] = new ChartSeriesTextRecord();
		m_dict[133] = new BoundSheetRecord();
		m_dict[574] = new WindowTwoRecord();
		m_dict[317] = new TabIdRecord();
		m_dict[77] = new PrinterSettingsRecord();
		m_dict[193] = new MMSRecord();
		biffRecordRaw = new HeaderFooterRecord();
		biffRecordRaw.SetRecordCode(20);
		m_dict[20] = biffRecordRaw;
		biffRecordRaw = new HeaderFooterRecord();
		biffRecordRaw.SetRecordCode(21);
		m_dict[21] = biffRecordRaw;
		m_dict[156] = new FnGroupCountRecord();
		m_dict[255] = new ExtSSTRecord();
		m_dict[4095] = new ExtSSTInfoSubRecord();
		m_dict[22] = new ExternCountRecord();
		m_dict[224] = new ExtendedFormatRecord();
		m_dict[2172] = new ExtendedFormatCRC();
		m_dict[2173] = new ExtendedXFRecord();
		m_dict[2172] = new ExtendedFormatCRC();
		m_dict[2173] = new ExtendedXFRecord();
		m_dict[4148] = new EndRecord();
		m_dict[16] = new DeltaRecord();
		m_dict[85] = new DefaultColWidthRecord();
		m_dict[80] = new DCONRecord();
		m_dict[60] = new ContinueRecord();
		m_dict[442] = new CodeNameRecord();
		m_dict[4118] = new ChartSeriesListRecord();
		m_dict[4196] = new ChartPlotGrowthRecord();
		m_dict[4120] = new ChartLineRecord();
		m_dict[513] = new BlankRecord();
		m_dict[519] = new StringRecord();
		m_dict[252] = new SSTRecord();
		biffRecordRaw = new SheetCenterRecord();
		biffRecordRaw.SetRecordCode(131);
		m_dict[131] = biffRecordRaw;
		biffRecordRaw = new SheetCenterRecord();
		biffRecordRaw.SetRecordCode(132);
		m_dict[132] = biffRecordRaw;
		m_dict[2048] = new QuickTipRecord();
		m_dict[43] = new PrintGridlinesRecord();
		m_dict[90] = new CRNRecord();
		m_dict[51] = new PrintedChartSizeRecord();
		m_dict[4126] = new ChartTickRecord();
		m_dict[4160] = new ChartRadarAreaRecord();
		m_dict[4146] = new ChartFrameRecord();
		m_dict[4134] = new ChartFontxRecord();
		m_dict[4116] = new ChartChartFormatRecord();
		m_dict[239] = new UnknownMarkerRecord();
		m_dict[96] = new TemplateRecord();
		m_dict[659] = new StyleRecord();
		m_dict[2194] = new StyleExtRecord();
		m_dict[638] = new RKRecord();
		m_dict[439] = new RefreshAllRecord();
		m_dict[2152] = new RangeProtectionRecord();
		m_dict[515] = new NumberRecord();
		m_dict[351] = new LabelRangesRecord();
		m_dict[225] = new InterfaceHdrRecord();
		m_dict[81] = new DConRefRecord();
		m_dict[4127] = new ChartValueRangeRecord();
		m_dict[4189] = new ChartSerFmtRecord();
		m_dict[2134] = new ChartAxisOffsetRecord();
		m_dict[545] = new ArrayRecord();
		m_dict[160] = new WindowZoomRecord();
		m_dict[221] = new ScenProtectRecord();
		m_dict[65] = new PaneRecord();
		m_dict[146] = new PaletteRecord();
		m_dict[49] = new FontRecord();
		m_dict[512] = new DimensionsRecord();
		m_dict[4159] = new ChartSurfaceRecord();
		m_dict[4187] = new ChartSerAuxErrBarRecord();
		m_dict[4157] = new ChartDropBarRecord();
		m_dict[4124] = new ChartChartLineRecord();
		m_dict[13] = new CalcModeRecord();
		m_dict[158] = new AutoFilterRecord();
		m_dict[144] = new SortRecord();
		m_dict[2151] = new SheetProtectionRecord();
		m_dict[229] = new MergeCellsRecord();
		m_dict[17] = new IterationRecord();
		m_dict[35] = new ExternNameRecord();
		m_dict[4168] = new ChartSbaserefRecord();
		m_dict[4156] = new ChartPicfRecord();
		m_dict[4128] = new ChartCatserRangeRecord();
		m_dict[4194] = new ChartAxcextRecord();
		m_dict[4176] = new ChartAlrunsRecord();
		m_dict[4154] = new Chart3DRecord();
		m_dict[157] = new AutoFilterInfoRecord();
		m_dict[127] = new ImageDataRecord();
		m_dict[2206] = new UnknownRecord();
		m_dict[2188] = new CompatibilityRecord();
		m_dict[2166] = new UnknownRecord();
		m_dict[2204] = new HeaderAndFooterRecord();
		biffRecordRaw = new PageLayoutView();
		biffRecordRaw.SetRecordCode(2187);
		m_dict[2187] = biffRecordRaw;
	}

	public static BiffRecordRaw GetRecord(TBIFFRecord type)
	{
		return GetRecord((int)type);
	}

	public static BiffRecordRaw GetRecord(int type)
	{
		object obj = (m_dict.ContainsKey(type) ? m_dict[type] : null);
		ICloneable cloneable = null;
		if (obj != null)
		{
			cloneable = obj as ICloneable;
		}
		else if (m_dict.ContainsKey(0))
		{
			UnknownRecord obj2 = (UnknownRecord)m_dict[0];
			obj2.RecordCode = type;
			cloneable = obj2;
		}
		if (cloneable != null)
		{
			return cloneable.Clone() as BiffRecordRaw;
		}
		return null;
	}

	public static BiffRecordRaw GetUntypedRecord(Stream stream)
	{
		int itemSize;
		return new UnknownRecord(stream, out itemSize);
	}

	public static BiffRecordRaw GetUntypedRecord(BinaryReader reader)
	{
		int itemSize;
		return new UnknownRecord(reader, out itemSize);
	}

	public static BiffRecordRaw GetRecord(TBIFFRecord type, BinaryReader reader, byte[] arrBuffer)
	{
		return GetRecord((int)type, reader, arrBuffer);
	}

	public static BiffRecordRaw GetRecord(int type, BinaryReader reader, byte[] arrBuffer)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return GetRecord(type);
	}

	public static BiffRecordRaw GetRecord(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		short type = provider.ReadInt16(iOffset);
		iOffset += 2;
		BiffRecordRaw record = GetRecord(type);
		int iLength = (record.Length = provider.ReadInt16(iOffset));
		iOffset += 2;
		record.ParseStructure(provider, iOffset, iLength, version);
		return record;
	}

	public static int ExtractRecordType(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Stream baseStream = reader.BaseStream;
		long position = baseStream.Position;
		short num = reader.ReadInt16();
		baseStream.Position = position;
		if (num == 0)
		{
			throw new ApplicationException("Cannot find record identifier in stream!");
		}
		return num;
	}

	public static int ExtractRecordType(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanSeek || !stream.CanRead)
		{
			throw new ApplicationException("Stream must permit seeking and reading operations");
		}
		_ = stream.Position;
		int num = (stream.ReadByte() & 0xFF) + ((stream.ReadByte() & 0xFF) << 8);
		if (num == 0)
		{
			throw new ApplicationException("Cannot find record identifier in stream!");
		}
		if (!m_dict.ContainsKey(num))
		{
			num = 0;
		}
		return num;
	}
}
