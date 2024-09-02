using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

[CLSCompliant(false)]
public class ShapeObjectTextCollection
{
	private Dictionary<int, WTextBox> m_textTable = new Dictionary<int, WTextBox>();

	public void AddTextBox(int shapeId, WTextBox textBox)
	{
		if (m_textTable == null)
		{
			m_textTable = new Dictionary<int, WTextBox>();
		}
		m_textTable.Add(shapeId, textBox);
	}

	public WTextBox GetTextBox(int shapeId)
	{
		WTextBox result = null;
		if (m_textTable.ContainsKey(shapeId))
		{
			result = m_textTable[shapeId];
			m_textTable.Remove(shapeId);
		}
		return result;
	}

	internal void Close()
	{
		if (m_textTable != null)
		{
			m_textTable.Clear();
			m_textTable = null;
		}
	}
}
