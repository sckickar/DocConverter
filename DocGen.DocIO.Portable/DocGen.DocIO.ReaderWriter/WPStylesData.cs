using System;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

internal class WPStylesData
{
	private WPTablesData m_tablesData;

	[CLSCompliant(false)]
	internal StyleSheetInfoRecord StyleSheetInfo => m_tablesData.StyleSheetInfo;

	[CLSCompliant(false)]
	internal StyleDefinitionRecord[] StyleDefinitions => m_tablesData.StyleDefinitions;

	[CLSCompliant(false)]
	internal WPStylesData(WPTablesData tables)
	{
		m_tablesData = tables;
	}

	[CLSCompliant(false)]
	internal StyleDefinitionRecord GetStyleRecordByID(int styleID)
	{
		for (int i = 0; i < StyleDefinitions.Length; i++)
		{
			StyleDefinitionRecord styleDefinitionRecord = StyleDefinitions[i];
			if (styleDefinitionRecord.StyleId == styleID)
			{
				return styleDefinitionRecord;
			}
		}
		return null;
	}
}
