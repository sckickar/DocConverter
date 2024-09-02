namespace DocGen.DocIO.DLS;

internal class EditableRange
{
	private EditableRangeStart m_editableRangeStart;

	private EditableRangeEnd m_editableRangeEnd;

	internal string Id => m_editableRangeStart.Id;

	internal EditableRangeStart EditableRangeStart => m_editableRangeStart;

	internal EditableRangeEnd EditableRangeEnd => m_editableRangeEnd;

	internal EditableRange(EditableRangeStart start)
		: this(start, null)
	{
	}

	internal EditableRange(EditableRangeStart start, EditableRangeEnd end)
	{
		m_editableRangeStart = start;
		m_editableRangeEnd = end;
	}

	internal void SetStart(EditableRangeStart start)
	{
		m_editableRangeStart = start;
	}

	internal void SetEnd(EditableRangeEnd end)
	{
		m_editableRangeEnd = end;
	}
}
