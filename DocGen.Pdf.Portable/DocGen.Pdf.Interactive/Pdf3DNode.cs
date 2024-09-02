using System;
using System.Text;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DNode : IPdfWrapper
{
	private bool m_visible;

	private string m_name;

	private float m_opacity;

	private float[] m_matrix;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			m_visible = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public float Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			m_opacity = value;
		}
	}

	public float[] Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			m_matrix = value;
			if (m_matrix != null && m_matrix.Length < 12)
			{
				throw new ArgumentOutOfRangeException("Matrix.Length", "Matrix array must have at least 12 elements.");
			}
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DNode()
	{
		Initialize();
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("3DNode"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		Dictionary.SetProperty("O", new PdfNumber(m_opacity / 100f));
		Dictionary.SetProperty("V", new PdfBoolean(m_visible));
		if (m_name != null && m_name.Length > 0)
		{
			Dictionary.SetProperty("N", new PdfName(m_name));
		}
		if (m_matrix != null && m_matrix.Length >= 12)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("[{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} {6:0.####} {7:0.####} {8:0.####} {9:0.####} {10:0.####} {11:0.####}]\n", m_matrix[0], m_matrix[1], m_matrix[2], m_matrix[3], m_matrix[4], m_matrix[5], m_matrix[6], m_matrix[7], m_matrix[8], m_matrix[9], m_matrix[10], m_matrix[11]);
			Dictionary.SetProperty("M", new PdfName(stringBuilder.ToString()));
		}
	}
}
