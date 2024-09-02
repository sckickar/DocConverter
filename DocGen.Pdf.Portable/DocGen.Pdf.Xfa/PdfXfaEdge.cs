using System;
using System.Globalization;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaEdge
{
	private float m_thikness = 1f;

	private PdfXfaBorderStyle m_borderStyle;

	private PdfColor m_borderColor = new PdfColor(DocGen.Drawing.Color.Black);

	private PdfXfaVisibility m_visibility;

	private string m_presenceTxt;

	public PdfColor Color
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			m_borderColor = value;
		}
	}

	public PdfXfaVisibility Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			m_visibility = value;
		}
	}

	public float Thickness
	{
		get
		{
			return m_thikness;
		}
		set
		{
			m_thikness = value;
		}
	}

	public PdfXfaBorderStyle BorderStyle
	{
		get
		{
			return m_borderStyle;
		}
		set
		{
			m_borderStyle = value;
		}
	}

	internal void Read(XmlNode node, PdfXfaEdge edge)
	{
		if (node.Attributes != null)
		{
			if (node.Attributes["stroke"] != null)
			{
				ReadStroke(node.Attributes["stroke"].Value, edge);
			}
			if (node.Attributes["thickness"] != null)
			{
				edge.Thickness = ConvertToPoint(node.Attributes["thickness"].Value);
			}
			if (node.Attributes["presence"] != null)
			{
				edge.m_presenceTxt = node.Attributes["presence"].Value;
				switch (edge.m_presenceTxt)
				{
				case "hidden":
					edge.Visibility = PdfXfaVisibility.Hidden;
					break;
				case "visible":
					edge.Visibility = PdfXfaVisibility.Visible;
					break;
				case "invisible":
					edge.Visibility = PdfXfaVisibility.Invisible;
					break;
				case "inactive":
					edge.Visibility = PdfXfaVisibility.Inactive;
					break;
				default:
					edge.Visibility = PdfXfaVisibility.Visible;
					break;
				}
			}
		}
		if (node["color"] != null && node["color"].Attributes["value"] != null)
		{
			string[] array = node["color"].Attributes["value"].Value.Split(',');
			edge.Color = new PdfColor(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2]));
		}
	}

	private float ConvertToPoint(string value)
	{
		float result = float.NaN;
		if (value.Contains("pt"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
		}
		else if (value.Contains("m"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
			result *= 2.8346457f;
		}
		else if (value.Contains("in"))
		{
			result = Convert.ToSingle(value.Trim('i', 'n'), CultureInfo.InvariantCulture);
			result *= 72f;
		}
		return result;
	}

	private void ReadStroke(string value, PdfXfaEdge edge)
	{
		switch (value)
		{
		case "solid":
			edge.BorderStyle = PdfXfaBorderStyle.Solid;
			break;
		case "dashed":
			edge.BorderStyle = PdfXfaBorderStyle.Dashed;
			break;
		case "dotted":
			edge.BorderStyle = PdfXfaBorderStyle.Dotted;
			break;
		case "dashDot":
			edge.BorderStyle = PdfXfaBorderStyle.DashDot;
			break;
		case "dashDotDot":
			edge.BorderStyle = PdfXfaBorderStyle.DashDotDot;
			break;
		case "lowered":
			edge.BorderStyle = PdfXfaBorderStyle.Lowered;
			break;
		case "raised":
			edge.BorderStyle = PdfXfaBorderStyle.Raised;
			break;
		case "etched":
			edge.BorderStyle = PdfXfaBorderStyle.Etched;
			break;
		case "embossed":
			edge.BorderStyle = PdfXfaBorderStyle.Embossed;
			break;
		default:
			edge.BorderStyle = PdfXfaBorderStyle.None;
			break;
		}
	}

	internal void Save(XmlNode node)
	{
		if (BorderStyle != 0)
		{
			string text = BorderStyle.ToString();
			text = char.ToLower(text[0]) + text.Substring(1);
			if (node.Attributes != null)
			{
				if (node.Attributes["stroke"] != null)
				{
					node.Attributes["stroke"].Value = text;
				}
				else
				{
					SetNewAttribute(node, "stroke", text);
				}
			}
			else
			{
				SetNewAttribute(node, "stroke", text);
			}
		}
		if (!float.IsNaN(Thickness))
		{
			if (node.Attributes["thickness"] != null)
			{
				node.Attributes["thickness"].Value = Thickness + "pt";
			}
			else
			{
				SetNewAttribute(node, "thickness", Thickness + "pt");
			}
		}
		if ((m_presenceTxt != null || m_presenceTxt != "") && Visibility != 0 && m_presenceTxt != Visibility.ToString().ToLower())
		{
			if (node.Attributes["presence"] != null)
			{
				node.Attributes["presence"].Value = Visibility.ToString().ToLower();
			}
			else
			{
				SetNewAttribute(node, "presence", Visibility.ToString().ToLower());
			}
		}
		_ = Color;
		string value = Color.R + "," + Color.G + "," + Color.B;
		if (node["color"] != null)
		{
			if (node["color"].Attributes["value"] != null)
			{
				node["color"].Attributes["value"].Value = value;
			}
			else
			{
				SetNewAttribute(node["color"], "value", value);
			}
		}
		else
		{
			XmlNode xmlNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, "color", "");
			SetNewAttribute(xmlNode, "value", value);
			node.AppendChild(xmlNode);
		}
	}

	private void SetNewAttribute(XmlNode node, string name, string value)
	{
		XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(name);
		xmlAttribute.Value = value;
		node.Attributes.Append(xmlAttribute);
	}

	internal object Clone()
	{
		return MemberwiseClone();
	}
}
