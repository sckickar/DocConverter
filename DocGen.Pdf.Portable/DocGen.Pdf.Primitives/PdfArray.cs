using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Primitives;

internal class PdfArray : IPdfPrimitive, IEnumerable, IPdfChangable
{
	public const string StartMark = "[";

	public const string EndMark = "]";

	private List<IPdfPrimitive> m_elements;

	private bool m_bChanged;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	private PdfCrossTable m_crossTable;

	private PdfArray m_clonedObject;

	private bool isFont;

	private bool skip;

	internal IPdfPrimitive this[int index]
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

	internal List<IPdfPrimitive> Elements => m_elements;

	internal PdfCrossTable CrossTable => m_crossTable;

	public IPdfPrimitive ClonedObject => m_clonedObject;

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

	internal bool Skip
	{
		get
		{
			return skip;
		}
		set
		{
			skip = value;
		}
	}

	public bool Changed => m_bChanged;

	internal PdfArray()
	{
		m_elements = new List<IPdfPrimitive>();
	}

	internal PdfArray(PdfArray array)
	{
		m_elements = new List<IPdfPrimitive>(array.m_elements);
	}

	internal PdfArray(List<PdfArray> array)
		: this()
	{
		foreach (PdfArray item in array)
		{
			Add(new PdfArray(item));
		}
	}

	internal PdfArray(int[] array)
		: this()
	{
		foreach (int value in array)
		{
			Add(new PdfNumber(value));
		}
	}

	internal PdfArray(float[] array)
		: this()
	{
		foreach (float value in array)
		{
			Add(new PdfNumber(value));
		}
	}

	public PdfArray(double[] array)
		: this()
	{
		foreach (double value in array)
		{
			Add(new PdfNumber(value));
		}
	}

	public static PdfArray FromRectangle(RectangleF rectangle)
	{
		return new PdfArray(new float[4] { rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom });
	}

	public static PdfArray FromRectangle(Rectangle rectangle)
	{
		return new PdfArray(new int[4] { rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom });
	}

	internal void Add(IPdfPrimitive element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("obj");
		}
		m_elements.Add(element);
		MarkChanged();
	}

	internal void Add(params IPdfPrimitive[] list)
	{
		foreach (IPdfPrimitive pdfPrimitive in list)
		{
			if (pdfPrimitive == null)
			{
				throw new ArgumentNullException("list");
			}
			m_elements.Add(pdfPrimitive);
		}
		if (list.Length != 0)
		{
			MarkChanged();
		}
	}

	internal bool Contains(IPdfPrimitive element)
	{
		return m_elements.Contains(element);
	}

	internal void Insert(int index, IPdfPrimitive element)
	{
		m_elements.Insert(index, element);
		MarkChanged();
	}

	internal int IndexOf(IPdfPrimitive element)
	{
		return m_elements.IndexOf(element);
	}

	internal void Remove(IPdfPrimitive element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		m_elements.Remove(element);
		MarkChanged();
	}

	internal void ReArrange(int[] orderArray)
	{
		int num = orderArray.Length;
		PdfReferenceHolder[] array = new PdfReferenceHolder[Count];
		int[] array2 = new int[Count];
		for (int i = 0; i < Count; i++)
		{
			array[i] = m_elements[i] as PdfReferenceHolder;
		}
		if (num <= Count)
		{
			if (PdfLoadedPageCollection.m_repeatIndex != 0)
			{
				for (int j = 0; j < PdfLoadedPageCollection.m_repeatIndex; j++)
				{
					m_elements[j] = array[orderArray[j]];
					array2[orderArray[j]] = 1;
				}
			}
			else
			{
				for (int k = 0; k < num; k++)
				{
					m_elements[k] = array[orderArray[k]];
					array2[orderArray[k]] = 1;
				}
			}
		}
		if (num > Count)
		{
			for (int l = 0; l < Count; l++)
			{
				m_elements[l] = array[orderArray[l]];
				array2[orderArray[l]] = 1;
			}
		}
		if (Count != num)
		{
			int num2 = ((PdfLoadedPageCollection.m_nestedPages != 1) ? PdfLoadedPageCollection.m_parentKidsCount : PdfLoadedPageCollection.m_parentKidsCounttemp);
			for (int m = 0; m < num2; m++)
			{
				if (array2[m] == 0)
				{
					if (PdfLoadedPageCollection.m_repeatIndex != 0)
					{
						RemoveAt(PdfLoadedPageCollection.m_repeatIndex);
					}
					else
					{
						RemoveAt(num);
					}
				}
			}
		}
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

	public RectangleF ToRectangle()
	{
		if (Count < 4)
		{
			throw new InvalidOperationException("Can't convert to rectangle.");
		}
		float floatValue = GetNumber(0).FloatValue;
		float floatValue2 = GetNumber(1).FloatValue;
		float floatValue3 = GetNumber(2).FloatValue;
		float floatValue4 = GetNumber(3).FloatValue;
		float x = Math.Min(floatValue, floatValue3);
		float y = Math.Min(floatValue2, floatValue4);
		float width = Math.Abs(floatValue - floatValue3);
		float height = Math.Abs(floatValue2 - floatValue4);
		return new RectangleF(x, y, width, height);
	}

	public virtual void Save(IPdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.Write("[");
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if (!Skip || !(PdfCrossTable.Dereference(this[i]) is PdfDictionary { isSkip: not false }))
			{
				this[i].Save(writer);
				if (i + 1 != count)
				{
					writer.Write(" ");
				}
			}
		}
		writer.Write("]");
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
		if (freezer is PdfParser || freezer == this)
		{
			m_bChanged = false;
		}
	}

	private PdfNumber GetNumber(int index)
	{
		return (PdfCrossTable.Dereference(this[index]) as PdfNumber) ?? throw new InvalidOperationException("Can't convert to rectangle.");
	}

	public IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		if (m_clonedObject != null && m_clonedObject.CrossTable == crossTable && !IsFont)
		{
			return m_clonedObject;
		}
		m_clonedObject = null;
		PdfArray pdfArray = new PdfArray();
		foreach (IPdfPrimitive element in m_elements)
		{
			pdfArray.Add(element.Clone(crossTable));
		}
		pdfArray.m_crossTable = crossTable;
		m_clonedObject = pdfArray;
		return pdfArray;
	}
}
