using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Interactive;

public class PdfFileLinkAnnotation : PdfActionLinkAnnotation
{
	private PdfLaunchAction m_action;

	public string FileName
	{
		get
		{
			return m_action.FileName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FileName");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("FileName - string can not be empty");
			}
			if (m_action.FileName != value)
			{
				m_action.FileName = value;
			}
		}
	}

	public override PdfAction Action
	{
		get
		{
			return base.Action;
		}
		set
		{
			base.Action = value;
			m_action.Next = value;
		}
	}

	public PdfFileLinkAnnotation(RectangleF rectangle, string fileName)
		: base(rectangle)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (fileName.Length == 0)
		{
			throw new ArgumentException("fileName - string can not be empty");
		}
		m_action = new PdfLaunchAction(fileName);
	}

	protected override void Save()
	{
		base.Save();
		base.Dictionary.SetProperty("A", m_action);
		if (base.Action != null)
		{
			base.Dictionary.SetProperty("A", base.Action);
		}
	}
}
