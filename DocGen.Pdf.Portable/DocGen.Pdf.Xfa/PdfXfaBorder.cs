using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaBorder
{
	private PdfXfaHandedness m_handedness;

	private PdfXfaVisibility m_visibility;

	private PdfXfaEdge m_left;

	private PdfXfaEdge m_right;

	private PdfXfaEdge m_bottom;

	private PdfXfaEdge m_top;

	private PdfColor m_color;

	private float m_width = 0.5f;

	private PdfXfaBorderStyle m_borderStyle;

	private string m_presenceTxt;

	private PdfXfaBrush m_fillColor;

	public PdfXfaBrush FillColor
	{
		get
		{
			return m_fillColor;
		}
		set
		{
			if (value != null)
			{
				m_fillColor = value;
			}
		}
	}

	public PdfXfaHandedness Handedness
	{
		get
		{
			return m_handedness;
		}
		set
		{
			m_handedness = value;
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

	public PdfXfaEdge LeftEdge
	{
		get
		{
			return m_left;
		}
		set
		{
			if (value != null)
			{
				m_left = value;
			}
		}
	}

	public PdfXfaEdge RightEdge
	{
		get
		{
			return m_right;
		}
		set
		{
			if (value != null)
			{
				m_right = value;
			}
		}
	}

	public PdfXfaEdge TopEdge
	{
		get
		{
			return m_top;
		}
		set
		{
			if (value != null)
			{
				m_top = value;
			}
		}
	}

	public PdfXfaEdge BottomEdge
	{
		get
		{
			return m_bottom;
		}
		set
		{
			if (value != null)
			{
				m_bottom = value;
			}
		}
	}

	public PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public PdfXfaBorderStyle Style
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

	public PdfXfaBorder()
	{
	}

	public PdfXfaBorder(PdfColor color)
	{
		Color = color;
	}

	internal void DrawBorder(PdfGraphics graphics, RectangleF bounds)
	{
		if (Visibility != PdfXfaVisibility.Hidden)
		{
			PdfPen flattenPen = GetFlattenPen();
			PdfBrush brush = GetBrush(bounds);
			graphics.DrawRectangle(flattenPen, brush, bounds);
		}
		if (LeftEdge == null || RightEdge == null)
		{
			return;
		}
		PdfPen pdfPen = PdfPens.Black;
		if (LeftEdge.Visibility != PdfXfaVisibility.Hidden)
		{
			if (!LeftEdge.Color.IsEmpty)
			{
				pdfPen = new PdfPen(LeftEdge.Color);
			}
			if (LeftEdge.Thickness > 0f)
			{
				pdfPen.Width = LeftEdge.Thickness;
			}
			else
			{
				pdfPen.Width = Width;
			}
			DrawEdge(graphics, pdfPen, bounds.Location, new PointF(bounds.X, bounds.Y + bounds.Height));
		}
		if (RightEdge.Visibility != PdfXfaVisibility.Hidden)
		{
			pdfPen = PdfPens.Black;
			if (!RightEdge.Color.IsEmpty)
			{
				pdfPen = new PdfPen(RightEdge.Color);
			}
			if (RightEdge.Thickness > 0f)
			{
				pdfPen.Width = RightEdge.Thickness;
			}
			else
			{
				pdfPen.Width = Width;
			}
			DrawEdge(graphics, pdfPen, new PointF(bounds.X + bounds.Width, bounds.Y), new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height));
		}
		if (TopEdge != null && TopEdge.Visibility != PdfXfaVisibility.Hidden)
		{
			pdfPen = PdfPens.Black;
			if (!TopEdge.Color.IsEmpty)
			{
				pdfPen = new PdfPen(TopEdge.Color);
			}
			if (TopEdge.Thickness > 0f)
			{
				pdfPen.Width = TopEdge.Thickness;
			}
			else
			{
				pdfPen.Width = Width;
			}
			DrawEdge(graphics, pdfPen, new PointF(bounds.X, bounds.Y), new PointF(bounds.X + bounds.Width, bounds.Y));
		}
		if (BottomEdge != null && BottomEdge.Visibility != PdfXfaVisibility.Hidden)
		{
			pdfPen = new PdfPen(BottomEdge.Color);
			if (BottomEdge.Thickness > 0f)
			{
				pdfPen.Width = BottomEdge.Thickness;
			}
			else
			{
				pdfPen.Width = Width;
			}
			DrawEdge(graphics, pdfPen, new PointF(bounds.X, bounds.Y + bounds.Height), new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height));
		}
	}

	private void DrawEdge(PdfGraphics graphics, PdfPen pen, PointF point, PointF point1)
	{
		graphics.Save();
		graphics.DrawLine(pen, point, point1);
		graphics.Restore();
	}

	internal PdfBorderStyle GetBorderStyle()
	{
		PdfBorderStyle result = PdfBorderStyle.Solid;
		switch (Style)
		{
		case PdfXfaBorderStyle.Dashed:
		case PdfXfaBorderStyle.Dotted:
		case PdfXfaBorderStyle.DashDot:
		case PdfXfaBorderStyle.DashDotDot:
			result = PdfBorderStyle.Dashed;
			break;
		case PdfXfaBorderStyle.Embossed:
			result = PdfBorderStyle.Beveled;
			break;
		}
		return result;
	}

	internal void ApplyAcroBorder(PdfStyledField field)
	{
		if (Color != PdfColor.Empty && (Color.R != 0 || Color.G != 0 || Color.B != 0))
		{
			field.BorderColor = Color;
		}
		switch (Style)
		{
		case PdfXfaBorderStyle.Dashed:
		case PdfXfaBorderStyle.Dotted:
		case PdfXfaBorderStyle.DashDot:
		case PdfXfaBorderStyle.DashDotDot:
			field.BorderStyle = PdfBorderStyle.Dashed;
			break;
		case PdfXfaBorderStyle.Embossed:
			field.BorderStyle = PdfBorderStyle.Beveled;
			break;
		case PdfXfaBorderStyle.Solid:
			field.BorderStyle = PdfBorderStyle.Solid;
			break;
		}
		if (Width != 0f && Width > 1f)
		{
			field.BorderWidth = Width;
		}
		if (FillColor != null && FillColor is PdfXfaSolidBrush)
		{
			field.BackColor = (FillColor as PdfXfaSolidBrush).Color;
		}
		else
		{
			field.BackColor = PdfColor.Empty;
		}
	}

	internal PdfBrush GetBrush(RectangleF bounds)
	{
		PdfBrush result = null;
		if (FillColor != null)
		{
			if (FillColor is PdfXfaSolidBrush)
			{
				result = new PdfSolidBrush((FillColor as PdfXfaSolidBrush).Color);
			}
			else if (FillColor is PdfXfaLinearBrush)
			{
				PdfLinearGradientMode mode = PdfLinearGradientMode.Horizontal;
				PdfXfaLinearBrush pdfXfaLinearBrush = FillColor as PdfXfaLinearBrush;
				switch (pdfXfaLinearBrush.Type)
				{
				case PdfXfaLinearType.BottomToTop:
				case PdfXfaLinearType.TopToBottom:
					mode = PdfLinearGradientMode.Vertical;
					break;
				case PdfXfaLinearType.LeftToRight:
				case PdfXfaLinearType.RightToLeft:
					mode = PdfLinearGradientMode.Horizontal;
					break;
				}
				result = ((pdfXfaLinearBrush.Type != PdfXfaLinearType.RightToLeft && pdfXfaLinearBrush.Type != PdfXfaLinearType.BottomToTop) ? new PdfLinearGradientBrush(bounds, pdfXfaLinearBrush.EndColor, pdfXfaLinearBrush.StartColor, mode) : new PdfLinearGradientBrush(bounds, pdfXfaLinearBrush.StartColor, pdfXfaLinearBrush.EndColor, mode));
			}
			else if (FillColor is PdfXfaRadialBrush)
			{
				PdfXfaRadialBrush pdfXfaRadialBrush = FillColor as PdfXfaRadialBrush;
				if (!(bounds.Height > bounds.Width))
				{
					_ = bounds.Width;
					_ = bounds.Height;
				}
				else
				{
					_ = bounds.Height;
					_ = bounds.Width;
				}
				float num = ((bounds.Height > bounds.Width) ? bounds.Height : bounds.Width);
				PointF pointF = new PointF(bounds.Location.X + bounds.Width / 2f, bounds.Location.Y + bounds.Height / 2f);
				result = ((pdfXfaRadialBrush.Type != 0) ? new PdfRadialGradientBrush(pointF, num, pointF, 0f, pdfXfaRadialBrush.StartColor, pdfXfaRadialBrush.EndColor) : new PdfRadialGradientBrush(pointF, 0f, pointF, num, pdfXfaRadialBrush.StartColor, pdfXfaRadialBrush.EndColor));
			}
		}
		return result;
	}

	internal PdfPen GetFlattenPen()
	{
		PdfPen result = null;
		if (!Color.IsEmpty)
		{
			result = new PdfPen(Color, Width);
		}
		else if (FillColor != null && Width > 0f)
		{
			result = new PdfPen(new PdfColor(DocGen.Drawing.Color.Black), Width);
		}
		if (LeftEdge != null && RightEdge == null && TopEdge == null && BottomEdge == null)
		{
			result = ((LeftEdge.Color.IsEmpty || LeftEdge.Visibility == PdfXfaVisibility.Hidden || LeftEdge.Visibility == PdfXfaVisibility.Invisible) ? null : ((LeftEdge.Thickness > 0f) ? new PdfPen(LeftEdge.Color, LeftEdge.Thickness) : ((!(Width > 0f)) ? null : new PdfPen(LeftEdge.Color, Width))));
		}
		return result;
	}

	internal PdfPen GetPen()
	{
		if (Color != PdfColor.Empty)
		{
			return new PdfPen(Color);
		}
		return null;
	}

	internal void Read(XmlNode node)
	{
		XmlAttributeCollection attributes = node.Attributes;
		if (attributes != null)
		{
			if (attributes["hand"] != null)
			{
				switch (attributes["hand"].Value)
				{
				case "left":
					m_handedness = PdfXfaHandedness.Left;
					break;
				case "right":
					m_handedness = PdfXfaHandedness.Right;
					break;
				case "even":
					m_handedness = PdfXfaHandedness.Even;
					break;
				}
			}
			if (attributes["presence"] != null)
			{
				m_presenceTxt = attributes["presence"].Value;
				switch (m_presenceTxt)
				{
				case "hidden":
					m_visibility = PdfXfaVisibility.Hidden;
					break;
				case "visible":
					m_visibility = PdfXfaVisibility.Visible;
					break;
				case "invisible":
					m_visibility = PdfXfaVisibility.Invisible;
					break;
				case "inactive":
					m_visibility = PdfXfaVisibility.Inactive;
					break;
				}
			}
		}
		XmlNodeList childNodes = node.ChildNodes;
		bool flag = false;
		if (childNodes != null)
		{
			if (childNodes.Count == 1 && childNodes[0].Name == "edge")
			{
				if (childNodes[0].Attributes.Count != 0 || childNodes[0].InnerText != string.Empty)
				{
					PdfXfaEdge pdfXfaEdge = new PdfXfaEdge();
					pdfXfaEdge.Thickness = 0f;
					pdfXfaEdge.Read(childNodes[0], pdfXfaEdge);
					if (pdfXfaEdge.BorderStyle != 0)
					{
						pdfXfaEdge.Thickness = 1f;
					}
					Color = pdfXfaEdge.Color;
					Style = pdfXfaEdge.BorderStyle;
					Width = pdfXfaEdge.Thickness;
					Visibility = pdfXfaEdge.Visibility;
					flag = true;
				}
			}
			else if (childNodes.Count > 1)
			{
				flag = true;
				foreach (XmlNode item in childNodes)
				{
					if (item.Name == "edge")
					{
						PdfXfaEdge pdfXfaEdge2 = new PdfXfaEdge();
						pdfXfaEdge2.Thickness = 0f;
						pdfXfaEdge2.Read(item, pdfXfaEdge2);
						if (pdfXfaEdge2.BorderStyle != 0)
						{
							pdfXfaEdge2.Thickness = 1f;
						}
						if (LeftEdge == null)
						{
							LeftEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
						else if (RightEdge == null)
						{
							RightEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
						else if (TopEdge == null)
						{
							TopEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
						else if (BottomEdge == null)
						{
							BottomEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
					}
				}
			}
		}
		ReadFillBrush(node);
		if (!flag)
		{
			PdfXfaEdge pdfXfaEdge3 = new PdfXfaEdge();
			Color = pdfXfaEdge3.Color;
			Style = pdfXfaEdge3.BorderStyle;
			Width = pdfXfaEdge3.Thickness;
		}
	}

	internal void ReadFillBrush(XmlNode node)
	{
		if (node["fill"] == null)
		{
			return;
		}
		XmlNode xmlNode = node["fill"];
		if (xmlNode["color"] != null && (xmlNode["linear"] != null || xmlNode["radial"] != null))
		{
			PdfColor pdfColor = PdfColor.Empty;
			PdfColor empty = PdfColor.Empty;
			if (xmlNode["color"].Attributes["value"] != null)
			{
				string[] array = xmlNode["color"].Attributes["value"].Value.Split(',');
				if (ValidateColor(array))
				{
					pdfColor = new PdfColor(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2]));
				}
			}
			if (xmlNode["linear"] != null)
			{
				PdfXfaLinearType type = PdfXfaLinearType.LeftToRight;
				if (xmlNode["linear"].Attributes["type"] != null)
				{
					string value = xmlNode["linear"].Attributes["type"].Value;
					type = GetLinearType(value);
				}
				if (xmlNode["linear"]["color"] != null)
				{
					string[] array2 = xmlNode["linear"]["color"].Attributes["value"].Value.Split(',');
					if (ValidateColor(array2))
					{
						empty = new PdfColor(byte.Parse(array2[0], CultureInfo.InvariantCulture), byte.Parse(array2[1], CultureInfo.InvariantCulture), byte.Parse(array2[2], CultureInfo.InvariantCulture));
						FillColor = new PdfXfaLinearBrush(empty, pdfColor, type);
					}
				}
				else
				{
					FillColor = new PdfXfaLinearBrush(new PdfColor(DocGen.Drawing.Color.Black), pdfColor, type);
				}
			}
			else
			{
				if (xmlNode["radial"] == null)
				{
					return;
				}
				PdfXfaRadialType type2 = PdfXfaRadialType.CenterToEdge;
				if (xmlNode["radial"].Attributes["type"] != null)
				{
					string value2 = xmlNode["radial"].Attributes["type"].Value;
					type2 = GetRadialType(value2);
				}
				if (xmlNode["radial"]["color"] != null)
				{
					string[] array3 = xmlNode["radial"]["color"].Attributes["value"].Value.Split(',');
					if (ValidateColor(array3))
					{
						empty = new PdfColor(byte.Parse(array3[0]), byte.Parse(array3[1]), byte.Parse(array3[2]));
						FillColor = new PdfXfaRadialBrush(pdfColor, empty, type2);
					}
				}
			}
		}
		else if (xmlNode["color"] != null && xmlNode["color"].Attributes["value"] != null)
		{
			string[] array4 = xmlNode["color"].Attributes["value"].Value.Split(',');
			if (ValidateColor(array4))
			{
				FillColor = new PdfXfaSolidBrush(new PdfColor(byte.Parse(array4[0]), byte.Parse(array4[1]), byte.Parse(array4[2])));
			}
		}
	}

	private bool ValidateColor(string[] words)
	{
		bool result = true;
		for (int i = 0; i < words.Length; i++)
		{
			if (words[i].Contains("-"))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private PdfXfaLinearType GetLinearType(string type)
	{
		PdfXfaLinearType result = PdfXfaLinearType.LeftToRight;
		switch (type)
		{
		case "toBottom":
			result = PdfXfaLinearType.TopToBottom;
			break;
		case "toLeft":
			result = PdfXfaLinearType.RightToLeft;
			break;
		case "toRight":
			result = PdfXfaLinearType.LeftToRight;
			break;
		case "toTop":
			result = PdfXfaLinearType.BottomToTop;
			break;
		}
		return result;
	}

	private PdfXfaRadialType GetRadialType(string type)
	{
		PdfXfaRadialType result = PdfXfaRadialType.CenterToEdge;
		if (!(type == "toCenter"))
		{
			if (type == "toEdge")
			{
				result = PdfXfaRadialType.CenterToEdge;
			}
		}
		else
		{
			result = PdfXfaRadialType.EdgeToCenter;
		}
		return result;
	}

	internal static PdfXfaBorder ReadBorder(XmlNode node)
	{
		PdfXfaBorder pdfXfaBorder = new PdfXfaBorder();
		XmlAttributeCollection attributes = node.Attributes;
		if (attributes != null)
		{
			if (attributes["hand"] != null)
			{
				switch (attributes["hand"].Value)
				{
				case "left":
					pdfXfaBorder.m_handedness = PdfXfaHandedness.Left;
					break;
				case "right":
					pdfXfaBorder.m_handedness = PdfXfaHandedness.Right;
					break;
				case "even":
					pdfXfaBorder.m_handedness = PdfXfaHandedness.Even;
					break;
				}
			}
			if (attributes["presence"] != null)
			{
				pdfXfaBorder.m_presenceTxt = attributes["presence"].Value;
				switch (pdfXfaBorder.m_presenceTxt)
				{
				case "hidden":
					pdfXfaBorder.m_visibility = PdfXfaVisibility.Hidden;
					break;
				case "visible":
					pdfXfaBorder.m_visibility = PdfXfaVisibility.Visible;
					break;
				case "invisible":
					pdfXfaBorder.m_visibility = PdfXfaVisibility.Invisible;
					break;
				case "inactive":
					pdfXfaBorder.m_visibility = PdfXfaVisibility.Inactive;
					break;
				}
			}
		}
		XmlNodeList childNodes = node.ChildNodes;
		if (childNodes != null)
		{
			if (childNodes.Count == 1)
			{
				PdfXfaEdge pdfXfaEdge = new PdfXfaEdge();
				pdfXfaEdge.Read(childNodes[0], pdfXfaEdge);
				pdfXfaBorder.Color = pdfXfaEdge.Color;
				pdfXfaBorder.Style = pdfXfaEdge.BorderStyle;
				pdfXfaBorder.Width = pdfXfaEdge.Thickness;
			}
			else
			{
				foreach (XmlNode item in childNodes)
				{
					if (item.Name == "edge")
					{
						PdfXfaEdge pdfXfaEdge2 = new PdfXfaEdge();
						pdfXfaEdge2.Read(item, pdfXfaEdge2);
						if (pdfXfaBorder.LeftEdge == null)
						{
							pdfXfaBorder.LeftEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
						else if (pdfXfaBorder.RightEdge == null)
						{
							pdfXfaBorder.RightEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
						else if (pdfXfaBorder.TopEdge == null)
						{
							pdfXfaBorder.TopEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
						else if (pdfXfaBorder.BottomEdge == null)
						{
							pdfXfaBorder.BottomEdge = pdfXfaEdge2.Clone() as PdfXfaEdge;
						}
					}
				}
			}
		}
		if (node["fill"] != null && node["fill"]["color"] != null && node["fill"]["color"].Attributes["value"] != null)
		{
			string[] array = node["fill"]["color"].Attributes["value"].Value.Split(',');
			pdfXfaBorder.FillColor = new PdfXfaSolidBrush(new PdfColor(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2])));
		}
		return pdfXfaBorder;
	}

	internal void Save(XmlNode node)
	{
		XmlAttributeCollection attributes = node.Attributes;
		if (attributes["hand"] != null)
		{
			attributes["hand"].Value = Handedness.ToString().ToLower();
		}
		else
		{
			SetNewAttribute(node, "hand", Handedness.ToString().ToLower());
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
		if (LeftEdge != null || RightEdge != null || BottomEdge != null || TopEdge != null)
		{
			List<XmlNode> list = new List<XmlNode>();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == "edge")
				{
					list.Add(childNode);
				}
			}
			if (list.Count <= 0)
			{
				return;
			}
			if (LeftEdge != null)
			{
				LeftEdge.Save(list[0]);
			}
			if (RightEdge != null)
			{
				if (list.Count > 1)
				{
					RightEdge.Save(list[1]);
				}
				else
				{
					CreateNewEdgeNode(node, RightEdge);
				}
			}
			if (BottomEdge != null)
			{
				foreach (XmlNode childNode2 in node.ChildNodes)
				{
					if (childNode2.Name == "edge")
					{
						list.Add(node);
					}
				}
				if (list.Count > 2)
				{
					BottomEdge.Save(list[2]);
				}
				else if (list.Count > 1)
				{
					CreateNewEdgeNode(node, BottomEdge);
				}
				else
				{
					if (RightEdge == null)
					{
						CreateNewEdgeNode(node, new PdfXfaEdge());
					}
					CreateNewEdgeNode(node, BottomEdge);
				}
			}
			if (TopEdge == null)
			{
				return;
			}
			foreach (XmlNode childNode3 in node.ChildNodes)
			{
				if (childNode3.Name == "edge")
				{
					list.Add(node);
				}
			}
			if (list.Count > 3)
			{
				TopEdge.Save(list[3]);
				return;
			}
			if (RightEdge == null)
			{
				CreateNewEdgeNode(node, new PdfXfaEdge());
			}
			if (BottomEdge == null)
			{
				CreateNewEdgeNode(node, new PdfXfaEdge());
			}
			CreateNewEdgeNode(node, TopEdge);
		}
		else
		{
			PdfXfaEdge pdfXfaEdge = new PdfXfaEdge();
			pdfXfaEdge.BorderStyle = Style;
			pdfXfaEdge.Color = Color;
			pdfXfaEdge.Visibility = Visibility;
			pdfXfaEdge.Thickness = Width;
			if (node["edge"] != null)
			{
				pdfXfaEdge.Save(node["edge"]);
			}
			else
			{
				CreateNewEdgeNode(node, pdfXfaEdge);
			}
		}
	}

	private void CreateNewEdgeNode(XmlNode node, PdfXfaEdge edge)
	{
		XmlNode xmlNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, "edge", "");
		edge.Save(xmlNode);
		node.AppendChild(xmlNode);
	}

	private void SetNewAttribute(XmlNode node, string name, string value)
	{
		XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(name);
		xmlAttribute.Value = value;
		node.Attributes.Append(xmlAttribute);
	}
}
