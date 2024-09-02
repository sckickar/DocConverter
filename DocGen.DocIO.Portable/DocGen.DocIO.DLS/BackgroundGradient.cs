using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class BackgroundGradient : XDLSSerializableBase
{
	internal const uint DEF_VERTICAL_ANGLE = 4289069056u;

	internal const uint DEF_DIAGONALUP_ANGLE = 4286119936u;

	internal const uint DEF_DIAGONALDOWN_ANGLE = 4292018176u;

	internal const uint DEF_SHADEUP_VARIANT = 100u;

	internal const uint DEF_SHADEOUT_VARIANT = 4294967246u;

	internal const uint DEF_SHADEMIDDLE_VARIANT = 50u;

	private BackgroundFillType m_fillType;

	private Color m_fillColor = Color.White;

	private Color m_fillBackColor = Color.White;

	private GradientShadingStyle m_shadingStyle;

	private GradientShadingVariant m_shadingVariant;

	private EscherClass m_escher;

	public Color Color1
	{
		get
		{
			return m_fillColor;
		}
		set
		{
			m_fillColor = value;
		}
	}

	public Color Color2
	{
		get
		{
			return m_fillBackColor;
		}
		set
		{
			m_fillBackColor = value;
		}
	}

	public GradientShadingStyle ShadingStyle
	{
		get
		{
			return m_shadingStyle;
		}
		set
		{
			m_shadingStyle = value;
		}
	}

	public GradientShadingVariant ShadingVariant
	{
		get
		{
			return m_shadingVariant;
		}
		set
		{
			m_shadingVariant = value;
		}
	}

	public BackgroundGradient()
		: base(null, null)
	{
		m_fillColor = Color.White;
		m_fillBackColor = Color.Black;
	}

	internal BackgroundGradient(WordDocument doc, MsofbtSpContainer container)
		: base(doc, null)
	{
		m_escher = doc.Escher;
		GetGradientData(container);
	}

	public BackgroundGradient Clone()
	{
		return (BackgroundGradient)base.CloneImpl();
	}

	internal new void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
	}

	private void GetGradientData(MsofbtSpContainer container)
	{
		m_fillType = container.GetBackgroundFillType();
		m_fillColor = container.GetBackgroundColor(isPictureBackground: false);
		m_fillBackColor = container.GetBackgroundColor(isPictureBackground: true);
		m_shadingStyle = container.GetGradientShadingStyle(m_fillType);
		m_shadingVariant = container.GetGradientShadingVariant(m_shadingStyle);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (m_fillColor != Color.White)
		{
			writer.WriteValue("FillColor", m_fillColor);
		}
		if (m_fillBackColor != Color.White)
		{
			writer.WriteValue("FillBackgroundColor", m_fillBackColor);
		}
		if (m_shadingStyle != 0)
		{
			writer.WriteValue("GradientShadingStyle", m_shadingStyle);
		}
		if (m_shadingVariant != 0)
		{
			writer.WriteValue("GradientShadingVariant", m_shadingVariant);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("FillColor"))
		{
			m_fillColor = reader.ReadColor("FillColor");
		}
		if (reader.HasAttribute("FillBackgroundColor"))
		{
			m_fillBackColor = reader.ReadColor("FillBackgroundColor");
		}
		if (reader.HasAttribute("GradientShadingStyle"))
		{
			m_shadingStyle = (GradientShadingStyle)(object)reader.ReadEnum("GradientShadingStyle", typeof(GradientShadingStyle));
		}
		if (reader.HasAttribute("GradientShadingVariant"))
		{
			m_shadingVariant = (GradientShadingVariant)(object)reader.ReadEnum("GradientShadingVariant", typeof(GradientShadingVariant));
		}
	}

	internal override void Close()
	{
		base.Close();
		if (m_escher != null)
		{
			m_escher.Close();
			m_escher = null;
		}
	}

	internal bool Compare(BackgroundGradient backgroundGradient)
	{
		if (ShadingStyle != backgroundGradient.ShadingStyle || ShadingVariant != backgroundGradient.ShadingVariant || Color1.ToArgb() != backgroundGradient.Color1.ToArgb() || Color2.ToArgb() != backgroundGradient.Color2.ToArgb())
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)ShadingStyle + ";");
		stringBuilder.Append((int)ShadingVariant + ";");
		stringBuilder.Append(Color1.ToArgb() + ";");
		stringBuilder.Append(Color2.ToArgb() + ";");
		return stringBuilder;
	}
}
