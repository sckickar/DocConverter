namespace DocGen.Office;

public interface IOfficeMathMatrix : IOfficeMathFunctionBase, IOfficeMathEntity
{
	MathVerticalAlignment VerticalAlignment { get; set; }

	float ColumnWidth { get; set; }

	SpacingRule ColumnSpacingRule { get; set; }

	IOfficeMathMatrixColumns Columns { get; }

	float ColumnSpacing { get; set; }

	bool HidePlaceHolders { get; set; }

	IOfficeMathMatrixRows Rows { get; }

	float RowSpacing { get; set; }

	SpacingRule RowSpacingRule { get; set; }

	IOfficeRunFormat ControlProperties { get; set; }
}
