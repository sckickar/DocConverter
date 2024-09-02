using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Constants;

internal sealed class Pane
{
	public enum ActivePane
	{
		bottomLeft = 2,
		bottomRight = 0,
		topLeft = 3,
		topRight = 1
	}

	public const string TagName = "pane";

	public const string XSplit = "xSplit";

	public const string YSplit = "ySplit";

	public const string TopLeftCell = "topLeftCell";

	public const string Active = "activePane";

	public const string State = "state";

	public const string StateFrozen = "frozen";

	public const string StateFrozenSplit = "frozenSplit";

	public const string StateSplit = "split";

	public const string Selection = "selection";

	public const string ActiveCell = "activeCell";

	public const string Sqref = "sqref";

	public static readonly Dictionary<string, ActivePane> PaneStrings;

	static Pane()
	{
		PaneStrings = new Dictionary<string, ActivePane>();
		PaneStrings.Add("bottomLeft", ActivePane.bottomLeft);
		PaneStrings.Add("bottomRight", ActivePane.bottomRight);
		PaneStrings.Add("topLeft", ActivePane.topLeft);
		PaneStrings.Add("topRight", ActivePane.topRight);
	}
}
