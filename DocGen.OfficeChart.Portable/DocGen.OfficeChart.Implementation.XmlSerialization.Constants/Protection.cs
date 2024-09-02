namespace DocGen.OfficeChart.Implementation.XmlSerialization.Constants;

internal sealed class Protection
{
	public const string SheetProtectionTag = "sheetProtection";

	public const string PasswordAttribute = "password";

	public const string ContentAttribute = "content";

	public const string ObjectsAttribute = "objects";

	public const string ScenariosAttribute = "scenarios";

	public const string FormatCellsAttribute = "formatCells";

	public const string FormatColumnsAttribute = "formatColumns";

	public const string FormatRowsAttribute = "formatRows";

	public const string InsertColumnsAttribute = "insertColumns";

	public const string InsertRowsAttribute = "insertRows";

	public const string InsertHyperlinksAttribute = "insertHyperlinks";

	public const string DeleteColumnsAttribute = "deleteColumns";

	public const string DeleteRowsAttribute = "deleteRows";

	public const string SelectLockedCells = "selectLockedCells";

	public const string SortAttribute = "sort";

	public const string AutoFilterAttribute = "autoFilter";

	public const string SelectUnlockedCells = "selectUnlockedCells";

	public const string PivotTablesAttribute = "pivotTables";

	public const string SheetAttribute = "sheet";

	public const string AlgorithmName = "algorithmName";

	public const string HashValue = "hashValue";

	public const string SpinCount = "spinCount";

	public const string SaltValue = "saltValue";

	public const string WorkbookAlgorithmName = "workbookAlgorithmName";

	public const string WorkbookHashValue = "workbookHashValue";

	public const string WorkbookSpinCount = "workbookSpinCount";

	public const string WorkbookSaltValue = "workbookSaltValue";

	public static readonly string[] ChartProtectionAttributes = new string[2] { "content", "objects" };

	public static readonly bool[] ChartDefaultValues = new bool[2];

	public static readonly string[] ProtectionAttributes = new string[16]
	{
		"sheet", "objects", "scenarios", "formatCells", "formatColumns", "formatRows", "insertColumns", "insertRows", "insertHyperlinks", "deleteColumns",
		"deleteRows", "selectLockedCells", "sort", "autoFilter", "pivotTables", "selectUnlockedCells"
	};

	public static readonly OfficeSheetProtection[] ProtectionFlags = new OfficeSheetProtection[16]
	{
		OfficeSheetProtection.Content,
		OfficeSheetProtection.Objects,
		OfficeSheetProtection.Scenarios,
		OfficeSheetProtection.FormattingCells,
		OfficeSheetProtection.FormattingColumns,
		OfficeSheetProtection.FormattingRows,
		OfficeSheetProtection.InsertingColumns,
		OfficeSheetProtection.InsertingRows,
		OfficeSheetProtection.InsertingHyperlinks,
		OfficeSheetProtection.DeletingColumns,
		OfficeSheetProtection.DeletingRows,
		OfficeSheetProtection.LockedCells,
		OfficeSheetProtection.Sorting,
		OfficeSheetProtection.Filtering,
		OfficeSheetProtection.UsingPivotTables,
		OfficeSheetProtection.UnLockedCells
	};

	public static readonly bool[] DefaultValues = new bool[16]
	{
		false, false, false, true, true, true, true, true, true, true,
		true, false, true, true, false, false
	};

	public const string WorkbookProtectionTag = "workbookProtection";

	public const string LockStructureTag = "lockStructure";

	public const string LockWindowsTag = "lockWindows";

	public const string WorkbookPassword = "workbookPassword";

	private Protection()
	{
	}
}
