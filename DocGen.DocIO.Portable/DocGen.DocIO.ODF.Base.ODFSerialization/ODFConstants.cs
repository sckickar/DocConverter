namespace DocGen.DocIO.ODF.Base.ODFSerialization;

internal class ODFConstants
{
	internal const int FirstVisibleChar = 32;

	internal static readonly char[] allowedChars = new char[3] { '\n', '\r', '\t' };

	internal const string OfficeNamespace = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";

	internal const string MetaNamespace = "urn:oasis:names:tc:opendocument:xmlns:meta:1.0";

	internal const string ConfigNamespace = "urn:oasis:names:tc:opendocument:xmlns:config:1.0";

	internal const string TextNamespace = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

	internal const string TableNamespace = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";

	internal const string DrawNamespace = "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0";

	internal const string PresentationNamespace = "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0";

	internal const string Drawing3DNamespace = "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0";

	internal const string ChartNamespace = "urn:oasis:names:tc:opendocument:xmlns:chart:1.0";

	internal const string FormNamespace = "urn:oasis:names:tc:opendocument:xmlns:form:1.0";

	internal const string DBNamespace = "urn:oasis:names:tc:opendocument:xmlns:database:1.0";

	internal const string ScriptNamespace = "urn:oasis:names:tc:opendocument:xmlns:script:1.0";

	internal const string OFNamespace = "urn:oasis:names:tc:opendocument:xmlns:of:1.2";

	internal const string StyleNamespace = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";

	internal const string NumberNamespace = "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0";

	internal const string AnimationNamespace = "urn:oasis:names:tc:opendocument:xmlns:animation:1.0";

	internal const string ManifestNamespace = "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";

	internal const string FONamespace = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";

	internal const string SVGNamespace = "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0";

	internal const string SMILNamespace = "urn:oasis:names:tc:opendocument:xmlns:smil-compatible:1.0";

	internal const string XlinkNamespace = "http://www.w3.org/1999/xlink";

	internal const string DCNamespace = "http://purl.org/dc/elements/1.1/";

	internal const string MathNamespace = "http://www.w3.org/1998/Math/MathML";

	internal const string XHTMLNamespace = "http://www.w3.org/1999/xhtml";

	internal const string ExcelMimetypeValue = "application/vnd.oasis.opendocument.spreadsheet";

	internal const string DocMimetypeValue = "application/vnd.oasis.opendocument.text";

	internal const string ManifestPartName = "META-INF/manifest.xml";

	internal const string ContentPartName = "content.xml";

	internal const string MetaPartName = "meta.xml";

	internal const string MimetypePartName = "mimetype";

	internal const string StylesPartName = "styles.xml";

	internal const string SettingsPartName = "settings.xml";

	internal const string DocContent = "document-content";

	internal const string DocStyles = "document-styles";

	internal const string DocMeta = "document-meta";

	internal const string DocSettings = "document-settings";

	internal const string ManifestLocalName = "manifest";

	internal const string EntryTagName = "file-entry";

	internal const string PathTagName = "full-path";

	internal const string MediaTagName = "media-type";

	internal const string MediaPath = "media/image";

	internal const string XmlNamespacePrefix = "xmlns";

	internal const string TableLocalName = "table";

	internal const string OfficeLocalName = "office";

	internal const string CommonStylesLocalName = "styles";

	internal const string AutoStylesLocalName = "automatic-styles";

	internal const string MasterStylesLocalName = "master-styles";

	internal const string StyleLocalName = "style";

	internal const string DrawLocalName = "draw";

	internal const string FOLocalName = "fo";

	internal const string XlinkLocalName = "xlink";

	internal const string DCLocalName = "dc";

	internal const string NumberLocalName = "number";

	internal const string SVGLocalName = "svg";

	internal const string OFLocalName = "of";

	internal const string AnimationLocalName = "anim";

	internal const string ChartLocalName = "chart";

	internal const string Drawing3DLocalName = "dr3d";

	internal const string MetaLocalName = "meta";

	internal const string PresentationLocalName = "presentation";

	internal const string TextLocalName = "text";

	internal const string ConfigLocalName = "onfig";

	internal const string DBLocalName = "db";

	internal const string FormLocalName = "form";

	internal const string ScriptLocalName = "script";

	internal const string XHTMLLocalName = "xhtml";

	internal const string SMILLocalName = "smil";

	internal const string BodyLocalName = "body";

	internal const string PageLayoutNameTag = "page-layout-name";

	internal const string BookmarkStartTag = "bookmark-start";

	internal const string BookmarkEndTag = "bookmark-end";

	internal const string FontFaceDeclsTag = "font-face-decls";

	internal const string FontFaceTag = "font-face";

	internal const string NameTag = "name";

	internal const string FontFamilyTag = "font-family";

	internal const string FamilyGenericTag = "font-family-generic";

	internal const string FontPitchTag = "font-pitch";

	internal const string FamilyTag = "family";

	internal const string VAlign = "vertical-align";

	internal const string BackColor = "background-color";

	internal const string BorderTop = "border-top";

	internal const string BorderBottom = "border-bottom";

	internal const string BorderLeft = "border-left";

	internal const string BorderRight = "border-right";

	internal const string TxtPropertiesTag = "text-properties";

	internal const string FontNameTag = "font-name";

	internal const string FontSizeTag = "font-size";

	internal const string FontSizeAsianTag = "font-size-asian";

	internal const string FontSizeComplexTag = "font-size-complex";

	internal const string ColorTag = "color";

	internal const string FontWeightTag = "font-weight";

	internal const string FontStyleTag = "font-style";

	internal const string FontReliefTag = "font-relief";

	internal const string LetterKerningTag = "letter-kerning";

	internal const string LineThroughTypeTag = "text-line-through-type";

	internal const string MasterPagTag = "master-page";

	internal const string HeaderTag = "header";

	internal const string HeaderLeftTag = "header-left";

	internal const string FooterTag = "footer";

	internal const string FooterLeftTag = "footer-left";

	internal const string PageLayoutTag = "page-layout";

	internal const string PageLayoutPropertiesTag = "page-layout-properties";

	internal const string PageWidthTag = "page-width";

	internal const string PageHeightTag = "page-height";

	internal const string PageOrientationTag = "print-orientation";

	internal const string MarginTopTag = "margin-top";

	internal const string MarginLeftTag = "margin-left";

	internal const string MarginRightTag = "margin-right";

	internal const string MarginBottomTag = "margin-bottom";

	internal const string TableCenteringTag = "table-centering";

	internal const string PrintPageOrderTag = "print-page-order";

	internal const string HeaderStyleTag = "header-style";

	internal const string FooterStyleTag = "footer-style";

	internal const string HeaderFooterPropertiesTag = "header-footer-properties";

	internal const string MinHeightTag = "min-height";

	internal const string FirstPageNumberTag = "first-page-number";

	internal const string ScaleToTag = "scale-to";

	internal const string TablePropTag = "table-properties";

	internal const string TableColumnProp = "table-column-properties";

	internal const string TableRowProp = "table-row-properties";

	internal const string DisplayTag = "display";

	internal const string WritingModeTag = "writing-mode";

	internal const string ColumnWidthTag = "column-width";

	internal const string RowHeightTag = "row-height";

	internal const string ParentStyleTag = "parent-style-name";

	internal const string DateStyle = "date-style";

	internal const string SectionProps = "section-properties";

	internal const string DefCellStyle = "CE1";

	internal const string HyperlinkPrefix = "../";

	internal const string GraphicFillColor = "#5b9bd5";

	internal const string GraphicStrokeColor = "#41719c";

	internal const string ParaTag = "p";

	internal const string FrameTag = "frame";

	internal const string ImageTag = "image";

	internal const string OrderIndex = "z-index";

	internal const string WidthTag = "width";

	internal const string HeightTag = "height";

	internal const string RelativeHeightTag = "rel-height";

	internal const string RelativeWidthTag = "rel-width";

	internal const string HRefTag = "href";
}
