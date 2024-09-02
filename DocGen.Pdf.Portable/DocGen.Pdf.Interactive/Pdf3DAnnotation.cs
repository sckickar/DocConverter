using System;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DAnnotation : PdfFileAnnotation
{
	private Pdf3DActivation m_activation;

	private Pdf3DBase m_u3d;

	private PdfTemplate m_apperance;

	public Pdf3DViewCollection Views => m_u3d.Stream.Views;

	public int DefaultView
	{
		get
		{
			return m_u3d.Stream.DefaultView;
		}
		set
		{
			m_u3d.Stream.DefaultView = value;
			NotifyPropertyChanged("DefaultView");
		}
	}

	public Pdf3DAnnotationType Type
	{
		get
		{
			return m_u3d.Stream.Type;
		}
		set
		{
			m_u3d.Stream.Type = value;
			NotifyPropertyChanged("Type");
		}
	}

	public string OnInstantiate
	{
		get
		{
			return m_u3d.Stream.OnInstantiate;
		}
		set
		{
			m_u3d.Stream.OnInstantiate = value;
			NotifyPropertyChanged("OnInstantiate");
		}
	}

	public Pdf3DActivation Activation
	{
		get
		{
			return m_activation;
		}
		set
		{
			m_activation = value;
			NotifyPropertyChanged("Activation");
		}
	}

	public override string FileName
	{
		get
		{
			return m_u3d.FileName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FileName");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("FileName can't be empty");
			}
			if (m_u3d != null && m_u3d.FileName != value)
			{
				m_u3d.FileName = value;
			}
			NotifyPropertyChanged("FileName");
		}
	}

	public Pdf3DAnimation Animation
	{
		get
		{
			return m_u3d.Stream.Animation;
		}
		set
		{
			if (m_u3d != null && m_u3d.Stream != null)
			{
				m_u3d.Stream.Animation = value;
				NotifyPropertyChanged("Animation");
			}
		}
	}

	public Pdf3DAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	public Pdf3DAnnotation(RectangleF rectangle, Stream data)
		: base(rectangle)
	{
		if (data == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_u3d = new Pdf3DBase(data);
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("3D"));
	}

	protected override void Save()
	{
		base.Save();
		base.Dictionary.SetProperty("3DD", new PdfReferenceHolder(m_u3d));
		if (m_activation != null)
		{
			base.Dictionary["3DA"] = new PdfReferenceHolder(m_activation);
		}
		if (m_apperance != null)
		{
			base.Dictionary["AP /N"] = new PdfReferenceHolder(m_apperance);
		}
	}
}
