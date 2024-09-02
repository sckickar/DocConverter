using System;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfBookmark : PdfBookmarkBase
{
	private PdfDestination m_destination;

	private PdfNamedDestination m_namedDestination;

	private PdfColor m_color;

	private PdfTextStyle m_textStyle;

	private PdfBookmark m_previous;

	private PdfBookmark m_next;

	private PdfBookmarkBase m_parent;

	private PdfAction m_action;

	public virtual PdfDestination Destination
	{
		get
		{
			return m_destination;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Destination");
			}
			m_destination = value;
			base.Dictionary.SetProperty("Dest", value);
		}
	}

	public virtual PdfNamedDestination NamedDestination
	{
		get
		{
			return m_namedDestination;
		}
		set
		{
			if (m_namedDestination != value)
			{
				m_namedDestination = value;
				PdfDictionary pdfDictionary = new PdfDictionary();
				pdfDictionary.SetProperty("D", new PdfString(m_namedDestination.Title));
				pdfDictionary.SetProperty("S", new PdfName("GoTo"));
				base.Dictionary.SetProperty("A", new PdfReferenceHolder(pdfDictionary));
			}
		}
	}

	public virtual string Title
	{
		get
		{
			PdfString pdfString = base.Dictionary["Title"] as PdfString;
			string result = null;
			if (pdfString != null)
			{
				result = pdfString.Value;
			}
			return result;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Title");
			}
			base.Dictionary.SetString("Title", value);
		}
	}

	public virtual PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			if (m_color != value)
			{
				m_color = value;
				UpdateColor();
			}
		}
	}

	public virtual PdfTextStyle TextStyle
	{
		get
		{
			return m_textStyle;
		}
		set
		{
			if (m_textStyle != value)
			{
				m_textStyle = value;
				UpdateTextStyle();
			}
		}
	}

	public PdfAction Action
	{
		get
		{
			return m_action;
		}
		set
		{
			if (m_action != value)
			{
				m_action = value;
				base.Dictionary.SetProperty("A", new PdfReferenceHolder(m_action.Dictionary));
			}
		}
	}

	public new bool IsExpanded
	{
		get
		{
			return base.IsExpanded;
		}
		set
		{
			base.IsExpanded = value;
		}
	}

	internal virtual PdfBookmark Previous
	{
		get
		{
			return m_previous;
		}
		set
		{
			if (m_previous != value)
			{
				m_previous = value;
				base.Dictionary.SetProperty("Prev", new PdfReferenceHolder(value));
			}
		}
	}

	internal virtual PdfBookmarkBase Parent => m_parent;

	internal virtual PdfBookmark Next
	{
		get
		{
			return m_next;
		}
		set
		{
			if (m_next != value)
			{
				m_next = value;
				base.Dictionary.SetProperty("Next", new PdfReferenceHolder(value));
			}
		}
	}

	internal PdfBookmark(string title, PdfBookmarkBase parent, PdfBookmark previous, PdfBookmark next)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		m_parent = parent;
		base.Dictionary.SetProperty("Parent", new PdfReferenceHolder(parent));
		Previous = previous;
		Next = next;
		Title = title;
	}

	internal PdfBookmark(string title, PdfBookmarkBase parent, PdfBookmark previous, PdfBookmark next, PdfDestination dest)
		: this(title, parent, previous, next)
	{
		if (dest == null)
		{
			throw new ArgumentNullException("destination");
		}
		Destination = dest;
	}

	internal PdfBookmark(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
	}

	internal void SetParent(PdfBookmarkBase parent)
	{
		m_parent = parent;
	}

	private void UpdateColor()
	{
		PdfDictionary dictionary = base.Dictionary;
		if (dictionary["C"] is PdfArray && m_color.IsEmpty)
		{
			dictionary.Remove("C");
		}
		else
		{
			dictionary["C"] = m_color.ToArray();
		}
	}

	private void UpdateTextStyle()
	{
		if (m_textStyle == PdfTextStyle.Regular)
		{
			base.Dictionary.Remove("F");
		}
		else
		{
			base.Dictionary.SetNumber("F", (int)m_textStyle);
		}
	}
}
