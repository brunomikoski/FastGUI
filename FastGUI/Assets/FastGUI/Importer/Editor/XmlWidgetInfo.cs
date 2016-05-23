/// <summary>
/// Extracts XML data from an XMLNode
/// </summary>
public class XmlWidgetInfo
{
    #region Constants

    private const string STR__text = "_text";
    private const string STR_Name0 = "ObjectName>0";
    private const string STR_ParentName0 = "ParentName>0";
    private const string STR_PosX0 = "X>0";
    private const string STR_PosY0 = "Y>0";
    private const string STR_ClippingX0 = "clippingX>0";
    private const string STR_LayerID0 = "LayerID>0";
    private const string STR_ClippingY0 = "clippingY>0";
    private const string STR_ImageStates0 = "ImageStates>0";
    private const string STR_Pressed0 = "pressed>0";
    private const string STR_Idle0 = "idle>0";
    private const string STR_Hover0 = "hover>0";
    private const string STR_Disabled0 = "disabled>0";
    private const string STR_FontName0 = "FontName>0";
    private const string STR_FontColor0 = "FontColor>0";
    private const string STR_TheText0 = "TheText>0";
    private const string STR_LabelName0 = "LabelName>0";
    private const string STR_ImagePath0 = "ImagePath>0";
    private const string STR_Background0 = "background>0";
    private const string STR_Checkmark0 = "checkmark>0";
    private const string STR_Foreground0 = "foreground>0";
    private const string STR_Thumb0 = "thumb>0";
    private const string STR_LayerWidth0 = "LayerWidth>0";
    private const string STR_LayerHeight0 = "LayerHeight>0";

    // Constants
    #endregion

    #region Properties

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Parent name
    /// </summary>
    public string ParentName { get; set; }

    /// <summary>
    /// Pos X
    /// </summary>
    public float PosX { get; set; }

    /// <summary>
    /// Pos Y
    /// </summary>
    public float PosY { get; set; }

    /// <summary>
    /// Layer ID
    /// </summary>
    public int LayerID { get; set; }

    /// <summary>
    /// Clip X
    /// </summary>
    public float ClipX { get; set; }

    /// <summary>
    /// Clip Y
    /// </summary>
    public float ClipY { get; set; }

    /// <summary>
    /// Button Pressed State
    /// </summary>
    public string ButtonPressedState { get; set; }

    /// <summary>
    /// ButtonIdleState
    /// </summary>
    public string ButtonIdleState { get; set; }

    /// <summary>
    /// Button hover state
    /// </summary>
    public string ButtonHoverState { get; set; }

    /// <summary>
    /// Button disabled state
    /// </summary>
    public string ButtonDisabledState { get; set; }

    /// <summary>
    /// Font name
    /// </summary>
    public string FontName { get; set; }

    /// <summary>
    /// Font color
    /// </summary>
    public string FontColor { get; set; }

    /// <summary>
    /// Text content
    /// </summary>
    public string TextContent { get; set; }

    /// <summary>
    /// Name of the label
    /// </summary>
    public string LabelName { get; set; }

    /// <summary>
    /// Source
    /// </summary>
    public string ImagePath { get; set; }

    /// <summary>
    /// Background image
    /// </summary>
    public string Background { get; set; }

    /// <summary>
    /// Checkmark image
    /// </summary>
    public string Checkmark { get; set; }

    /// <summary>
    /// Foreground image
    /// </summary>
    public string Foreground { get; set; }

    /// <summary>
    /// Thumb
    /// </summary>
    public string Thumb { get; set; }

    /// <summary>
    /// Sprite width
    /// </summary>
    public int SpriteWidth { get; set; }

    /// <summary>
    /// Sprite Height
    /// </summary>
    public int SpriteHeight { get; set; }
    
    // Properties
    #endregion

    #region Init

    /// <summary>
    /// Initializes a new instance of the XmlWidgetInfo class.
    /// </summary>
    /// <param name="xmlNode"></param>
    public XmlWidgetInfo(XMLNode xmlNode)
    {
        // get the XML properties
        Name = GetNodeString(xmlNode, STR_Name0, STR__text);
        ParentName = GetNodeString(xmlNode, STR_ParentName0, STR__text);

        // floats
        PosX = FloatFromString(GetNodeString(xmlNode, STR_PosX0, STR__text));
        PosY = FloatFromString(GetNodeString(xmlNode, STR_PosY0, STR__text));
        ClipX = FloatFromString(GetNodeString(xmlNode, STR_ClippingX0, STR__text));
        ClipY = FloatFromString(GetNodeString(xmlNode, STR_ClippingY0, STR__text));

        // int
        LayerID = IntFromString(GetNodeString(xmlNode, STR_LayerID0, STR__text));

        // strings
        ButtonPressedState = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Pressed0, STR__text);    // pressed
        ButtonIdleState = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Idle0, STR__text);           // idle
        ButtonHoverState = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Hover0, STR__text);         // hover
        ButtonDisabledState = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Disabled0, STR__text);   // disabled

        // labels
        FontName = GetNodeString(xmlNode, STR_FontName0, STR__text);
        FontColor = GetNodeString(xmlNode, STR_FontColor0, STR__text);
        TextContent = GetNodeString(xmlNode, STR_TheText0, STR__text);
        LabelName = GetNodeString(xmlNode, STR_LabelName0, STR__text);

        // sprite source
        ImagePath = GetNodeString(xmlNode, STR_ImagePath0, STR__text);
        SpriteWidth = IntFromString(GetNodeString(xmlNode, STR_LayerWidth0, STR__text));
        SpriteHeight = IntFromString(GetNodeString(xmlNode, STR_LayerHeight0, STR__text));

        // checkbox & sliders
        Background = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Background0, STR__text);
        Checkmark = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Checkmark0, STR__text);
        Foreground = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Foreground0, STR__text);
        Thumb = GetNestedNodeString(xmlNode, STR_ImageStates0, STR_Thumb0, STR__text);
    }

    // Init
    #endregion

    #region GetNodeString

    /// <summary>
    /// Get the string value from the xml node
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="node"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetNodeString(XMLNode xmlNode, string node, string key)
    {
        XMLNode tempNode = xmlNode.GetNode(node);
        return ((tempNode != null) && (tempNode.ContainsKey(key))) ?
            tempNode[key].ToString() :
            null;
    }

    // GetNodeString
    #endregion

    #region GetNestedNodeString

    /// <summary>
    /// Get the string value from the xml node
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="key"></param>
    /// <param name="nodeChild"></param>
    /// <param name="nodeRoot"></param>
    /// <returns></returns>
    private string GetNestedNodeString(XMLNode xmlNode, string nodeRoot, string nodeChild, string key)
    {
        // exit if no nested node
        if (xmlNode.GetNode(nodeRoot) == null)
            return null;

        XMLNode tempNode = xmlNode.GetNode(nodeRoot).GetNode(nodeChild);
        return ((tempNode != null) && (tempNode.ContainsKey(key))) ?
            tempNode[key].ToString() :
            null;
    }

    // GetNestedNodeString
    #endregion

    #region FloatFromString

    private float FloatFromString(string floatStr)
    {
        return (string.IsNullOrEmpty(floatStr)) ? 0 : float.Parse(floatStr);
    }

    // FloatFromString
    #endregion

    #region IntFromString

    private int IntFromString(string intStr)
    {
        return (string.IsNullOrEmpty(intStr)) ? 0 : int.Parse(intStr);
    }

    // IntFromString
    #endregion
}
