namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal sealed class Vml
{
	public enum SelectionTypes
	{
		Single,
		Multi,
		Extend
	}

	public enum DropStyles
	{
		Combo,
		ComboEdit,
		Simple
	}

	public const string VNamespace = "urn:schemas-microsoft-com:vml";

	public const string ONamespace = "urn:schemas-microsoft-com:office:office";

	public const string XNamespace = "urn:schemas-microsoft-com:office:excel";

	public const string VPreffix = "v";

	public const string OPreffix = "o";

	public const string XPreffix = "x";

	public const string XmlTagName = "xml";

	public const string ShapeTypeTagName = "shapetype";

	public const string ShapeTypeIdFormat = "_x0000_t{0}";

	public const string ShapeIdFormat = "_x0000_s{0}";

	public const string ShapeIdAttributeName = "id";

	public const string SpIdAttributeName = "spid";

	public const string CoordSizeAttributeName = "coordsize";

	public const string CommentCoordSize = "21600,21600";

	public const string SptAttriubteName = "spt";

	public const string PathAttributeName = "path";

	public const string CommentPathValue = "m,l,21600r21600,l21600,xe";

	public const string BitmapPathValue = "m@4@5l@4@11@9@11@9@5xe";

	public const string ClientDataTagName = "ClientData";

	public const string ObjectTypeAttribute = "ObjectType";

	public const string MoveWithCellsTagName = "MoveWithCells";

	public const string SizeWithCellsTagName = "SizeWithCells";

	public const string AnchorTagName = "Anchor";

	public const string ShapeTagName = "shape";

	public const string ShapeLayoutTagName = "shapelayout";

	public const string TypeAttributeName = "type";

	public const string LegacyDrawing = "legacyDrawing";

	public const string LegacyDrawingHF = "legacyDrawingHF";

	public const string RowTagName = "Row";

	public const string ColumnTagName = "Column";

	public const string StyleAttribute = "style";

	public const string FillColorAttribute = "fillcolor";

	public const string ShadowTagName = "shadow";

	public const string ShadowOnAttribute = "on";

	public const string ShadowObscuredAttribute = "obscured";

	public const string ShadowColorAttribute = "color";

	public const string CommentAlignment = "\n  <v:textbox style='mso-direction-alt:auto'>\n   <div style='text-align:left'></div>\n  </v:textbox>\n";

	public const string InsetModeAttribute = "insetmode";

	public const string TextBoxTagName = "textbox";

	public const string DivTagName = "div";

	public const string VisibilityAttribute = "visibility";

	public const string VisibilityHiddenValue = "hidden";

	public const string LockText = "LockText";

	public const string TextHAlign = "TextHAlign";

	public const string TextVAlign = "TextVAlign";

	public const string ImageDataTag = "imagedata";

	public const string RelationId = "relid";

	public const string LayoutFlow = "layout-flow";

	public const string LayoutFlowVertical = "vertical";

	public const string MsoLayoutFlow = "mso-layout-flow-alt";

	public const string MsoLayoutFlowTopToBottom = "top-to-bottom";

	public const string MsoLayoutFlowBottomToTop = "bottom-to-top";

	public const string MsoFitShapeToText = "mso-fit-shape-to-text";

	public const string TrueExpression = "t";

	public const string FalseExpression = "f";

	public const string FormulasTagName = "formulas";

	public const string SingleFormulaTagName = "f";

	public const string EquationTagName = "eqn";

	public const string ShapePathTagName = "path";

	public const string Extrusionok = "extrusionok";

	public const string GradientShapeOk = "gradientshapeok";

	public const string ConnectType = "connecttype";

	public const string PreferRelative = "preferrelative";

	public const string FilledAttribute = "filled";

	public const string StrokedAttribute = "stroked";

	public const string Stroke = "stroke";

	public const string JoinStyle = "joinstyle";

	public const string Lock = "lock";

	public const string Ext = "ext";

	public const string AspectRatio = "aspectratio";

	public const string Checked = "Checked";

	public const string FontTag = "font";

	public const string Face = "face";

	public const string Size = "size";

	public const string Color = "color";

	public const string ColorAttribute = "color";

	public const string Checkbox = "Checkbox";

	public const string OptionButton = "Radio";

	public const string Drop = "Drop";

	public const string AutoLineTag = "AutoLine";

	public const string AutoFillTag = "AutoFill";

	public const string FormulaLink = "FmlaLink";

	public const string FirstButton = "FirstButton";

	public const string ScrollPosition = "Val";

	public const string ScrollMinimum = "Min";

	public const string FocusPositionAttribute = "focusposition";

	public const string FocusSizeAttribute = "focussize";

	public const string GradientOneColorAttributeValueStart = "fill";

	public const string ColorsAttribute = "colors";

	public const string ScrollMaximum = "Max";

	public const string ScrollIncrement = "Inc";

	public const string ScrollPageIncrement = "Page";

	public const string ScrollBarWidth = "Dx";

	public const string NoThreeD = "NoThreeD";

	public const string NoThreeD2 = "NoThreeD2";

	public const string ListSourceRange = "FmlaRange";

	public const string SelectedItem = "Sel";

	public const string SelectionType = "SelType";

	public const string CallbackType = "LCT";

	public const string NormalLCT = "Normal";

	public const string DropStyle = "DropStyle";

	public const string DropLines = "DropLines";

	public const string MarginLeft = "margin-left";

	public const string MarginTop = "margin-top";

	public const string Width = "width";

	public const string Height = "height";

	public const string Millimeters = "mm";

	public const string AutoPicture = "AutoPict";

	public const string CF = "CF";

	public const string OleObjects = "oleObjects";

	public const string OleObject = "oleObject";

	public const string ProgramID = "progId";

	public const string DevAspect = "dvAspect";

	public const string ShapeID = "shapeId";

	public const string FormulaMacro = "FmlaMacro";

	public const string StrokeColorAttribute = "strokecolor";

	public const string MethodAttribute = "method";

	public const string MethodNoneValue = "none";

	public const string FillTag = "fill";

	public const string Color2Attribute = "color2";

	public const string LinkAttribute = "link";

	public const string SolidFillTag = "solid";

	public const string TextureAttributeValue = "tile";

	public const string PictureAttributeValue = "frame";

	public const string PatternAttributeValue = "pattern";

	public const string GradientTypeTagValue = "gradient";

	public const string GradientRadialTypeTagValue = "gradientRadial";

	public const string GradientCenterTypeTagValue = "gradientCenter";

	public const string GradientDarkFillValue = "fill darken";

	public const string GradientLightFillValue = "fill lighten";

	public const string OpacityAttribute = "opacity";

	public const string Opacity2Attribute = "opacity2";

	public const string AngleAttribute = "angle";

	public const string RotateAttribute = "rotate";

	public const string StrokeWeightAttribute = "strokeweight";

	public const string FocusAttribute = "focus";

	public const string RelationIDAttribute = "relid";

	public const string TitleAttibute = "title";

	public const string FillTypeAttribute = "filltype";

	public const string DashStyleAttribute = "dashstyle";

	public const string LineStyleAttribute = "linestyle";

	public const string SolidFillTypeAttributeValue = "solid";

	public const string AlternateTextAttribute = "alt";

	public const string PictureObjectTypeAttributeValue = "Pict";

	public const string LinkAttributeValue = "[{0}]!''''";

	public const string ReColorAttribute = "recolor";

	public const string PathAttribute = "path";

	public const string ExtrusionOkAttribute = "extrusionok";

	public const string StrokeOkAttribute = "strokeok";

	public const string FillOkAttribute = "fillok";

	public const string ConnectTypeAttribute = "connecttype";

	public const string LockTypeAttribute = "lock";

	public const string ExtAttribute = "ext";

	public const string ShapeTypeAttribute = "shapetype";

	public const string ShadowOkAttribute = "shadowok";

	public const string OleUpdateAttribute = "oleUpdate";

	public const char RGBColorPrefixChar = '#';

	public const char IndexedColorPrefix = '[';

	public const char SizeInPointsPrefix = 'p';

	public const string SizeInPoints = "pt";

	public const int OpacityDegree = 65536;

	public const int DegreeDivider = 255;

	public const double DarkLimit = 0.5;
}
