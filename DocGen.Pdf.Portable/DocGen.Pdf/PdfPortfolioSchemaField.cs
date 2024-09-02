using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPortfolioSchemaField : IPdfWrapper
{
	private bool m_editable;

	private string m_name;

	private int m_order;

	private bool m_visible = true;

	private PdfPortfolioSchemaFieldType m_type;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public bool Editable
	{
		get
		{
			return m_editable;
		}
		set
		{
			m_editable = value;
			m_dictionary.SetBoolean("E", m_editable);
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
			m_dictionary.SetProperty("N", new PdfString(m_name));
		}
	}

	public int Order
	{
		get
		{
			return m_order;
		}
		set
		{
			m_order = value;
			m_dictionary.SetNumber("O", m_order);
		}
	}

	public PdfPortfolioSchemaFieldType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
			if (m_type == PdfPortfolioSchemaFieldType.String)
			{
				m_dictionary.SetName("Subtype", "S");
			}
			else if (m_type == PdfPortfolioSchemaFieldType.Date)
			{
				m_dictionary.SetName("Subtype", "D");
			}
			else if (m_type == PdfPortfolioSchemaFieldType.Number)
			{
				m_dictionary.SetName("Subtype", "N");
			}
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			m_visible = value;
			m_dictionary.SetBoolean("V", m_visible);
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfPortfolioSchemaField()
	{
		Initialize();
	}

	internal PdfPortfolioSchemaField(PdfDictionary schemaField)
	{
		m_dictionary = schemaField;
		if (m_dictionary.ContainsKey("N"))
		{
			Name = (m_dictionary["N"] as PdfString).Value;
		}
		if (m_dictionary.ContainsKey("O"))
		{
			Order = (m_dictionary["O"] as PdfNumber).IntValue;
		}
		if (m_dictionary.ContainsKey("V"))
		{
			Visible = (m_dictionary["V"] as PdfBoolean).Value;
		}
		if (m_dictionary.ContainsKey("E"))
		{
			Editable = (m_dictionary["E"] as PdfBoolean).Value;
		}
		if (m_dictionary.ContainsKey("Subtype"))
		{
			string value = (m_dictionary["Subtype"] as PdfName).Value;
			if (value.Equals("S"))
			{
				Type = PdfPortfolioSchemaFieldType.String;
			}
			else if (value.Equals("Size"))
			{
				Type = PdfPortfolioSchemaFieldType.Size;
			}
			else if (value.Equals("N"))
			{
				Type = PdfPortfolioSchemaFieldType.Number;
			}
			else if (value.Equals("D"))
			{
				Type = PdfPortfolioSchemaFieldType.Date;
			}
		}
	}

	private void Initialize()
	{
		m_dictionary.SetProperty("Type", new PdfName("CollectionField"));
		m_dictionary.SetBoolean("V", value: true);
	}
}
