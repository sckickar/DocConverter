using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public interface IWarning
{
	bool ShowWarnings(List<WarningInfo> warnings);
}
