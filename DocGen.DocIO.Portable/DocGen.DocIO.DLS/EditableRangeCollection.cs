using System;

namespace DocGen.DocIO.DLS;

internal class EditableRangeCollection : CollectionImpl
{
	internal EditableRange this[string id] => FindById(id);

	internal EditableRangeCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	internal EditableRange FindById(string id)
	{
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			EditableRange editableRange = base.InnerList[i] as EditableRange;
			if (editableRange.Id.Equals(id))
			{
				return editableRange;
			}
		}
		return null;
	}

	internal void RemoveAt(int index)
	{
		EditableRange editableRange = base.InnerList[index] as EditableRange;
		Remove(editableRange);
	}

	internal void Remove(EditableRange editableRange)
	{
		base.InnerList.Remove(editableRange);
		EditableRangeStart editableRangeStart = editableRange.EditableRangeStart;
		EditableRangeEnd editableRangeEnd = editableRange.EditableRangeEnd;
		editableRangeStart?.RemoveSelf();
		editableRangeEnd?.RemoveSelf();
	}

	internal void Add(EditableRange editableRange)
	{
		base.InnerList.Add(editableRange);
	}

	internal void AttachEditableRangeStart(EditableRangeStart editableRangeStart)
	{
		EditableRange editableRange = this[editableRangeStart.Id];
		if (editableRange != null)
		{
			editableRangeStart.SetId(editableRangeStart.Id + Guid.NewGuid());
			editableRangeStart.RemoveSelf();
		}
		else
		{
			editableRange = new EditableRange(editableRangeStart);
			Add(editableRange);
		}
	}

	internal void AttacheEditableRangeEnd(EditableRangeEnd editableRangeEnd)
	{
		EditableRange editableRange = this[editableRangeEnd.Id];
		if (editableRange == null)
		{
			return;
		}
		EditableRangeEnd editableRangeEnd2 = editableRange.EditableRangeEnd;
		if (editableRangeEnd2 != null)
		{
			editableRangeEnd.RemoveSelf();
			if (editableRange.EditableRangeEnd == null)
			{
				editableRange.SetEnd(editableRangeEnd2);
			}
		}
		else
		{
			editableRange.SetEnd(editableRangeEnd);
		}
	}
}
