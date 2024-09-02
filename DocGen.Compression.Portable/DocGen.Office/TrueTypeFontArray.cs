using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Office;

internal class TrueTypeFontArray : IEnumerable
{
	public const string StartMark = "[";

	public const string EndMark = "]";

	private List<ITrueTypeFontPrimitive> m_elements;

	private bool m_bChanged;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	private TrueTypeFontArray m_clonedObject;

	private bool isFont;

	internal ITrueTypeFontPrimitive this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index", "The index can't be less then zero or greater then Count.");
			}
			return m_elements[index];
		}
	}

	internal int Count => m_elements.Count;

	public ObjectStatus Status
	{
		get
		{
			return m_status;
		}
		set
		{
			m_status = value;
		}
	}

	public bool IsSaving
	{
		get
		{
			return m_isSaving;
		}
		set
		{
			m_isSaving = value;
		}
	}

	public int ObjectCollectionIndex
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal List<ITrueTypeFontPrimitive> Elements => m_elements;

	internal bool IsFont
	{
		get
		{
			return isFont;
		}
		set
		{
			isFont = value;
		}
	}

	public bool Changed => m_bChanged;

	internal TrueTypeFontArray()
	{
		m_elements = new List<ITrueTypeFontPrimitive>();
	}

	internal TrueTypeFontArray(TrueTypeFontArray array)
	{
		m_elements = new List<ITrueTypeFontPrimitive>(array.m_elements);
	}

	internal TrueTypeFontArray(List<TrueTypeFontArray> array)
		: this()
	{
		foreach (TrueTypeFontArray item in array)
		{
			_ = item;
		}
	}

	internal TrueTypeFontArray(int[] array)
		: this()
	{
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
		}
	}

	internal TrueTypeFontArray(float[] array)
		: this()
	{
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
		}
	}

	public TrueTypeFontArray(double[] array)
		: this()
	{
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
		}
	}

	public static TrueTypeFontArray FromRectangle(RectangleF rectangle)
	{
		return new TrueTypeFontArray(new float[4] { rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom });
	}

	public static TrueTypeFontArray FromRectangle(Rectangle rectangle)
	{
		return new TrueTypeFontArray(new int[4] { rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom });
	}

	internal void Add(ITrueTypeFontPrimitive element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("obj");
		}
		m_elements.Add(element);
		MarkChanged();
	}

	internal void Add(params ITrueTypeFontPrimitive[] list)
	{
		foreach (ITrueTypeFontPrimitive trueTypeFontPrimitive in list)
		{
			if (trueTypeFontPrimitive == null)
			{
				throw new ArgumentNullException("list");
			}
			m_elements.Add(trueTypeFontPrimitive);
		}
		if (list.Length != 0)
		{
			MarkChanged();
		}
	}

	internal bool Contains(ITrueTypeFontPrimitive element)
	{
		return m_elements.Contains(element);
	}

	internal void Insert(int index, ITrueTypeFontPrimitive element)
	{
		m_elements.Insert(index, element);
		MarkChanged();
	}

	internal int IndexOf(ITrueTypeFontPrimitive element)
	{
		return m_elements.IndexOf(element);
	}

	internal void Remove(ITrueTypeFontPrimitive element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		m_elements.Remove(element);
		MarkChanged();
	}

	internal void RemoveAt(int index)
	{
		m_elements.RemoveAt(index);
		MarkChanged();
	}

	internal void Clear()
	{
		m_elements.Clear();
		MarkChanged();
	}

	public IEnumerator GetEnumerator()
	{
		return m_elements.GetEnumerator();
	}

	public void MarkChanged()
	{
		m_bChanged = true;
	}

	public void FreezeChanges(object freezer)
	{
	}
}
