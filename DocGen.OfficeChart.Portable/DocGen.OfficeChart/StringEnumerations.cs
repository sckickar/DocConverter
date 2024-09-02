using System.Collections.Generic;

namespace DocGen.OfficeChart;

internal class StringEnumerations
{
	private const string Solid = "solid";

	private const string Dash = "dash";

	private const string DashDot = "dashDot";

	private const string LongDash = "lgDash";

	private const string SystemDash = "sysDash";

	private const string SystemDot = "sysDot";

	private const string LongDashDot = "lgDashDot";

	private const string LongDashDotDot = "lgDashDotDot";

	private Dictionary<string, OfficeShapeDashLineStyle> s_dicLineStyleXmlToEnum = new Dictionary<string, OfficeShapeDashLineStyle>();

	private Dictionary<OfficeShapeDashLineStyle, string> s_dicLineStyleEnumToXml = new Dictionary<OfficeShapeDashLineStyle, string>();

	internal Dictionary<string, OfficeShapeDashLineStyle> LineDashTypeXmltoEnum => s_dicLineStyleXmlToEnum;

	internal Dictionary<OfficeShapeDashLineStyle, string> LineDashTypeEnumToXml => s_dicLineStyleEnumToXml;

	internal StringEnumerations()
	{
		s_dicLineStyleXmlToEnum.Add("solid", OfficeShapeDashLineStyle.Solid);
		s_dicLineStyleXmlToEnum.Add("dash", OfficeShapeDashLineStyle.Dashed);
		s_dicLineStyleXmlToEnum.Add("dashDot", OfficeShapeDashLineStyle.Dash_Dot);
		s_dicLineStyleXmlToEnum.Add("lgDash", OfficeShapeDashLineStyle.Medium_Dashed);
		s_dicLineStyleXmlToEnum.Add("sysDash", OfficeShapeDashLineStyle.Dotted);
		s_dicLineStyleXmlToEnum.Add("sysDot", OfficeShapeDashLineStyle.Dotted_Round);
		s_dicLineStyleXmlToEnum.Add("lgDashDot", OfficeShapeDashLineStyle.Medium_Dash_Dot);
		s_dicLineStyleXmlToEnum.Add("lgDashDotDot", OfficeShapeDashLineStyle.Dash_Dot_Dot);
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Solid, "solid");
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Dashed, "dash");
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Dash_Dot, "dashDot");
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Dotted, "sysDash");
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Dotted_Round, "sysDot");
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Medium_Dashed, "lgDash");
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Medium_Dash_Dot, "lgDashDot");
		s_dicLineStyleEnumToXml.Add(OfficeShapeDashLineStyle.Dash_Dot_Dot, "lgDashDotDot");
	}
}
