using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Handles creating the NGUI widgets from the XML node
/// </summary>
public class PsdWidgeteer
{
    #region Enums

    /// <summary>
    /// Image button states
    /// </summary>
    private enum EButtonState
    {
        Idle = 0,
        Pressed,
        Hover,
        Disabled
    }

    // Enums
    #endregion

    #region AdjustPosition

    /// <summary>
    /// Adjusts the position of a component
    /// </summary>
    /// <param name="pX"></param>
    /// <param name="pY"></param>
    /// <param name="targetHeight"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="targetWidth"></param>
    /// <returns></returns>
    private static Vector3 AdjustPosition(float pX, float pY, float targetWidth, float targetHeight, UIPanel targetRootPanel)
    {
        return new Vector3(
            ((pX - (targetWidth / 2)) / (targetHeight / 2)) + targetRootPanel.transform.position.x,
            (((targetHeight / 2) - pY) / (targetHeight / 2)) + targetRootPanel.transform.position.y,
            0);
    }



    // AdjustPosition
    #endregion

    #region AddTexture2DToAtlas

    /// <summary>
    /// Adds a texture to the atlas, the sprite data
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    private static UISpriteData AddTexture2DToAtlas(Texture2D texture)
    {

        UIAtlasMaker.AddOrUpdate(NGUISettings.atlas, texture);

        // NGUI v3 now shows its own progress panel, so we have to clear it when it finishes
        EditorUtility.ClearProgressBar();

        // now re-show our progress bar
        PsdImporter.UpdateProgress(false);

        // return the sprite
        return NGUISettings.atlas.GetSprite(texture.name);
    }

    // AddTexture2DToAtlas
    #endregion

    #region CreateNguiChildBaseGo

    /// <summary>
    /// Creates a game object to hold all the child components
    /// </summary>
    /// <param name="widgetInfo"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="actualOutput"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <returns></returns>
    private static GameObject CreateNguiChildBaseGo(XmlWidgetInfo widgetInfo, float targetWidth, float targetHeight, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput)
    {
        // get the last root
        Transform lastRoot = targetRootPanel.transform;
        if (widgetInfo.ParentName.Equals(lastAnchorPath))
        {
            lastRoot = lastAnchor;
        }
        else
        {
            if (targetRootPanel.transform.FindChild(widgetInfo.ParentName))
                lastRoot = targetRootPanel.transform.FindChild(widgetInfo.ParentName);
        }

        GameObject NewGo = new GameObject(widgetInfo.Name);
        NewGo.transform.parent = lastRoot;
        NewGo.transform.position = AdjustPosition(widgetInfo.PosX, widgetInfo.PosY, targetWidth, targetHeight, targetRootPanel);
        NewGo.transform.localScale = Vector3.one;
        NewGo.layer = NewGo.transform.parent.gameObject.layer;
        actualOutput.references.Add(widgetInfo.LayerID, NewGo.GetInstanceID());

        // return the new object
        return NewGo;
    }

    // CreateNguiChildBaseGo
    #endregion

    #region GetFont

    /// <summary>
    /// Returns the font, if one is not found, returns the default font instead
    /// </summary>
    /// <param name="fontName"></param>
    /// <returns></returns>
    private static UIFont GetFont(string fontName)
    {
        // default to NGUI font
        string DefaultPath = string.Format("{0}/FastGUI/Defaults For Import/Font", Application.dataPath);
        string SearchPath = DefaultPath;

        // check if we have a fonts path specified
        if (!string.IsNullOrEmpty(PsdImporter.FontApplicationFolder))
            SearchPath = string.Format("{0}/", PsdImporter.FontApplicationFolder);

        // get all fonts in the search path
        string[] FontPaths = Directory.GetFiles(SearchPath, string.Format("{0}.prefab", fontName), SearchOption.AllDirectories);

        // if we have no fonts, then revert to default path
        if (((FontPaths == null) || (FontPaths.Length == 0)) && (!SearchPath.Equals(DefaultPath)))
        {
            // revert
            SearchPath = DefaultPath;

            // get all fonts in the search path
            FontPaths = Directory.GetFiles(SearchPath, string.Format("{0}.prefab", fontName), SearchOption.AllDirectories);

            // if nothing is still found, then fail
            if ((FontPaths == null) || (FontPaths.Length == 0))
            {
                Debug.LogWarning(string.Format("Could not find any fonts for import. Searching for: {0}", fontName));
                return null;
            }
        }

        if ((FontPaths != null) && (FontPaths.Length > 0))
        {
            // use the first instance found
            UIFont tGO = AssetDatabase.LoadAssetAtPath(FontPaths[0].Substring(FontPaths[0].IndexOf("Assets/")), typeof(UIFont)) as UIFont;

            // if null, then throw error
            if (tGO == null)
                Debug.LogWarning(string.Format("Could not load font: {0}", fontName));

            return tGO;
        }

        else
        {
            Debug.LogWarning(string.Format("Could not load font: '{0}'. No fonts found", fontName));
            return null;
        }
    }

    // GetFont
    #endregion

    #region GetSpriteData

    /// <summary>
    /// Loads a sprite image, saves to the atlas, and returns the sprite data
    /// </summary>
    /// <param name="psdAssetFolderPath"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    private static UISpriteData GetSpriteData(string psdAssetFolderPath, string filename)
    {
        // Create an atlas if one does not exist
        if (NGUISettings.atlas == null)
            NGUISettings.atlas = PsdAtlasManager.CreateNewAtlas(psdAssetFolderPath);

        // get the image
        Texture2D NewTexture = AssetDatabase.LoadAssetAtPath(string.Format("{0}/Images/{1}", psdAssetFolderPath, filename), typeof(Texture2D)) as Texture2D;
        if (NewTexture == null)
            return null;

        // get the sprite, add to atlas if null
        UISpriteData NewSprite = NGUISettings.atlas.GetSprite(NewTexture.name);
        if (NewSprite == null)
            NewSprite = AddTexture2DToAtlas(NewTexture);

        // return the sprite
        return NewSprite;
    }

    // GetSpriteData
    #endregion

    #region AddSpriteToGo

    /// <summary>
    /// Adds a sprite to the GO
    /// </summary>
    /// <param name="baseGO">GO to attach sprite to</param>
    /// <param name="psdAssetFolderPath"></param>
    /// <param name="spriteSourceName"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    private static UISprite AddSpriteToGo(GameObject baseGO, string psdAssetFolderPath, string spriteSourceName, UIPanel targetRootPanel, float posX, float posY, float targetWidth, float targetHeight)
    {
        // load the image
        UISpriteData ChildWorkingSprite = GetSpriteData(psdAssetFolderPath, spriteSourceName);
        if (ChildWorkingSprite == null)
            return null;

        // add a sprite
        UISprite ChildSpriteWidget = baseGO.AddComponent<UISprite>();
        ChildSpriteWidget.atlas = NGUISettings.atlas;
        ChildSpriteWidget.spriteName = ChildWorkingSprite.name;
        ChildSpriteWidget.pivot = NGUISettings.pivot;
        ChildSpriteWidget.depth = NGUITools.CalculateNextDepth(targetRootPanel.gameObject);
        ChildSpriteWidget.transform.localScale = Vector3.one;
        ChildSpriteWidget.transform.position = AdjustPosition(posX, posY, targetWidth, targetHeight, targetRootPanel);
        ChildSpriteWidget.width = ChildWorkingSprite.width;
        ChildSpriteWidget.height = ChildWorkingSprite.height;
        ChildSpriteWidget.MakePixelPerfect();

        return ChildSpriteWidget;
    }

    // AddSpriteToGo
    #endregion

    #region CreateChildSprite

    /// <summary>
    /// Creates a child game object and attaches a UISprite to it
    /// </summary>
    /// <param name="parentGO"></param>
    /// <param name="psdAssetFolderPath"></param>
    /// <param name="spriteSourceName"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="newChildName">name of the new child</param>
    /// <param name="anchorToParent">when true, anchors the child to the parent</param>
    /// <returns></returns>
    private static UISprite CreateChildSprite(GameObject parentGO, string newChildName, string psdAssetFolderPath, string spriteSourceName, UIPanel targetRootPanel, float posX, float posY, float targetWidth, float targetHeight, bool anchorToParent = true)
    {
        // create a child sprite to hold the Checked state
        GameObject SpriteChild = NGUITools.AddChild(parentGO);
        SpriteChild.name = newChildName;

        // add a sprite
        UISprite ChildSpriteWidget = AddSpriteToGo(SpriteChild, psdAssetFolderPath, spriteSourceName, targetRootPanel, posX, posY, targetWidth, targetHeight);
        if (ChildSpriteWidget == null)
            return null;

        // anchor the child to the parent
        if (anchorToParent)
            ChildSpriteWidget.SetAnchor(parentGO);

        return ChildSpriteWidget;
    }

    // CreateChildSprite
    #endregion

    #region CreateAnchor

    /// <summary>
    /// Creates an anchor
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetHeight"></param>
    /// <param name="targetWidth"></param>
    /// <returns></returns>
    public static Transform CreateAnchor(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight)
    {
        // create a new widget info and use that
        return CreateAnchor(targetRootPanel, lastAnchor, lastAnchorPath, actualOutput, targetWidth, targetHeight, new XmlWidgetInfo(xmlNode));
    }

    /// <summary>
    /// Creates an anchor
    /// </summary>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetHeight"></param>
    /// <param name="targetWidth"></param>
    /// <param name="widgetInfo"></param>
    /// <returns></returns>
    public static Transform CreateAnchor(UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, XmlWidgetInfo widgetInfo)
    {
        Transform lastRoot = targetRootPanel.transform;

        if (widgetInfo.ParentName == lastAnchorPath)
        {
            lastRoot = lastAnchor;
        }
        else
        {
            if (targetRootPanel.transform.FindChild(widgetInfo.ParentName))
                lastRoot = targetRootPanel.transform.FindChild(widgetInfo.ParentName);
        }

        GameObject NewGO;
        if (actualOutput.references.ContainsKey(widgetInfo.LayerID))
        {
            NewGO = EditorUtility.InstanceIDToObject(actualOutput.references[widgetInfo.LayerID]) as GameObject;
            if (NewGO == null)
            {
                actualOutput.references.Remove(widgetInfo.LayerID);
                return CreateAnchor(targetRootPanel, lastAnchor, lastAnchorPath, actualOutput, targetWidth, targetHeight, widgetInfo);
            }
        }
        else
        {
            NewGO = new GameObject(widgetInfo.Name);
            actualOutput.references.Add(widgetInfo.LayerID, NewGO.GetInstanceID());
        }


        // adjust position and scale
        NewGO.transform.parent = lastRoot;
        NewGO.transform.position = AdjustPosition(widgetInfo.PosX, widgetInfo.PosY, targetWidth, targetHeight, targetRootPanel);
        NewGO.transform.localScale = Vector3.one;

        return NewGO.transform;
    }

    // CreateAnchor
    #endregion

    #region CreateClippingPlane

    /// <summary>
    /// Creates a clipping plane
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetHeight"></param>
    /// <param name="targetWidth"></param>
    /// <returns></returns>
    public static Transform CreateClippingPanel(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight)
    {
        // get the XML properties
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // get the anchor panel
        Transform NewContainer = CreateAnchor(targetRootPanel, lastAnchor, lastAnchorPath, actualOutput, targetWidth, targetHeight, WidgetInfo);

        // create a new panel
        UIPanel NewPanel = NewContainer.gameObject.AddComponent<UIPanel>();
        NewPanel.clipping = UIDrawCall.Clipping.SoftClip;
        NewPanel.baseClipRegion = new Vector4(0, 0, WidgetInfo.ClipX, WidgetInfo.ClipY);
        NewPanel.clipSoftness = new Vector2(5, 5);

        return NewContainer;
    }

    // CreateClippingPlane
    #endregion

    #region CreateImageButton

    /// <summary>
    /// Creates an image button
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    /// <returns></returns>
    public static GameObject CreateImageButton(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // Create an atlas if one does not exist
        if (NGUISettings.atlas == null)
            NGUISettings.atlas = PsdAtlasManager.CreateNewAtlas(psdAssetFolderPath);

        // get the button pressed states
        Dictionary<EButtonState, string> ButtonStatesDict = new Dictionary<EButtonState, string>();
        if (! string.IsNullOrEmpty(WidgetInfo.ButtonPressedState))
            ButtonStatesDict.Add(EButtonState.Pressed, WidgetInfo.ButtonPressedState);
        if (!string.IsNullOrEmpty(WidgetInfo.ButtonIdleState))
            ButtonStatesDict.Add(EButtonState.Idle, WidgetInfo.ButtonIdleState);
        if (! string.IsNullOrEmpty(WidgetInfo.ButtonHoverState))
            ButtonStatesDict.Add(EButtonState.Hover, WidgetInfo.ButtonHoverState);
        if (!string.IsNullOrEmpty(WidgetInfo.ButtonDisabledState))
            ButtonStatesDict.Add(EButtonState.Disabled, WidgetInfo.ButtonDisabledState);

        // create the button
        GameObject NewGo = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);
        UIImageButton NewImageButton = NewGo.AddComponent<UIImageButton>();

        // add the UISprite widget
        UISprite SpriteWidget = NewGo.AddComponent<UISprite>();
        SpriteWidget.atlas = NGUISettings.atlas;
        SpriteWidget.pivot = NGUISettings.pivot;
        SpriteWidget.depth = NGUITools.CalculateNextDepth(targetRootPanel.gameObject);

        SpriteWidget.MakePixelPerfect();
        NGUITools.AddWidgetCollider(NewImageButton.gameObject);
        NewImageButton.target = SpriteWidget;


        // set the UIImage button sprites
        bool WidthHeightSet = false;
        foreach (EButtonState ButtonKey in ButtonStatesDict.Keys)
        {
            // get the sprite, add to atlas if null
            UISpriteData ButtonSprite = GetSpriteData(psdAssetFolderPath, ButtonStatesDict[ButtonKey]);
            if (ButtonSprite == null)
                continue;

            // set sprite for the image button
            switch (ButtonKey)
            {
                case EButtonState.Idle:
                    NewImageButton.normalSprite = ButtonSprite.name;
                    break;
                case EButtonState.Pressed:
                    NewImageButton.pressedSprite = ButtonSprite.name;
                    break;
                case EButtonState.Hover:
                    NewImageButton.hoverSprite = ButtonSprite.name;
                    break;
                case EButtonState.Disabled:
                    NewImageButton.disabledSprite = ButtonSprite.name;
                    break;
            }

            // set dimensions, if not already set
            // NOTE: we check first, because setting the width/height performs a lot of calcs and redraws, so it's best to only set once
            if (! WidthHeightSet)
            {
                WidthHeightSet = true;
                SpriteWidget.width = ButtonSprite.width;
                SpriteWidget.height = ButtonSprite.height;
            }
        }

        // set the normal sprite
        SpriteWidget.spriteName = NewImageButton.normalSprite;

        return NewGo;
    }

    // CreateImageButton
    #endregion

    #region CreateTextLabel

    /// <summary>
    /// Creates a text label
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="psdAssetFolderPath"></param>
    /// <param name="targetHeight"></param>
    /// <param name="targetWidth"></param>
    public static GameObject CreateTextLabel(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // create the base GO
        GameObject LabelGO = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);

        // rename the Label
        LabelGO.name = WidgetInfo.LabelName;

        // Create Label:
        UILabel NewLabel = LabelGO.AddComponent<UILabel>();

        string[] FontColor = WidgetInfo.FontColor.Split(';');
        NewLabel.color = new Color(float.Parse(FontColor[0]) / 255f, float.Parse(FontColor[1]) / 255f, float.Parse(FontColor[2]) / 255f);
        NewLabel.bitmapFont = GetFont(WidgetInfo.FontName);
        NewLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
        NewLabel.text = WidgetInfo.TextContent;
        NewLabel.depth = NGUITools.CalculateNextDepth(targetRootPanel.gameObject);
        NewLabel.pivot = UIWidget.Pivot.Center;

        NewLabel.MakePixelPerfect();
        return LabelGO;
    }

    // CreateTextLabel
    #endregion

    #region CreateInputLabel

    /// <summary>
    /// Creates an input label
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static void CreateInputLabel(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // create the label
        GameObject LabelGO = CreateTextLabel(xmlNode, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput, targetWidth, targetHeight, psdAssetFolderPath);
        UILabel TheLabel = LabelGO.GetComponent<UILabel>();

        // add input
        UIInput NewInputLabel = LabelGO.AddComponent<UIInput>();
        NewInputLabel.label = TheLabel;
        NewInputLabel.activeTextColor = TheLabel.color;

        // add the box collider
        NGUITools.AddWidgetCollider(LabelGO);
    }

    // CreateInputLabel
    #endregion

    #region CreateSprite

    /// <summary>
    /// Creates a sprite
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    /// <returns></returns>
    public static GameObject CreateSprite(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // create the base GO
        GameObject NewGo = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);

        // Create an atlas if one does not exist
        if (NGUISettings.atlas == null)
            NGUISettings.atlas = PsdAtlasManager.CreateNewAtlas(psdAssetFolderPath);

        // load the image
        UISpriteData WorkingSprite = GetSpriteData(psdAssetFolderPath, WidgetInfo.ImagePath);
        if (WorkingSprite == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.ImagePath));
            return null;
        }

        // add a sprite
        UISprite SpriteWidget = NewGo.AddComponent<UISprite>();
        SpriteWidget.name = string.Format("Sprite - {0}", WidgetInfo.Name);
        SpriteWidget.atlas = NGUISettings.atlas;
        SpriteWidget.spriteName = WorkingSprite.name;
        SpriteWidget.pivot = NGUISettings.pivot;
        SpriteWidget.depth = NGUITools.CalculateNextDepth(targetRootPanel.gameObject);
        SpriteWidget.transform.localScale = Vector3.one;
        SpriteWidget.transform.position = AdjustPosition(WidgetInfo.PosX, WidgetInfo.PosY, targetWidth, targetHeight, targetRootPanel);
        SpriteWidget.width = WorkingSprite.width;
        SpriteWidget.height = WorkingSprite.height;

        // check if this is a background sprite (size = psd)
        if ((targetWidth.Equals(WidgetInfo.SpriteWidth)) && (targetHeight.Equals(WidgetInfo.SpriteHeight)))
            SpriteWidget.SetAnchor(NewGo.transform.parent.gameObject, 0,0,0,0);

        SpriteWidget.MakePixelPerfect();

        return NewGo;
    }

    // CreateSprite
    #endregion

    #region CreateEmptyWidget

    /// <summary>
    /// Creates an empty widget
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static GameObject CreateEmptyWidget(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // create the base GO
        GameObject NewGo = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);

        // add a widget
        UIWidget Widget = NewGo.AddComponent<UIWidget>();
        Widget.name = string.Format("Widget - {0}", WidgetInfo.Name);
        Widget.pivot = NGUISettings.pivot;
        Widget.depth = NGUITools.CalculateNextDepth(targetRootPanel.gameObject);
        Widget.transform.localScale = Vector3.one;
        Widget.transform.position = AdjustPosition(WidgetInfo.PosX, WidgetInfo.PosY, targetWidth, targetHeight, targetRootPanel);
        Widget.width = WidgetInfo.SpriteWidth;
        Widget.height = WidgetInfo.SpriteHeight;

        // return the new widget
        return NewGo;
    }

    // CreateEmptyWidget
    #endregion

    #region CreateEmptyPanel

    /// <summary>
    /// Creates an empty panel
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static GameObject CreateEmptyPanel(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // create the base GO
        GameObject NewGo = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);

        // add a panel
        UIPanel Panel = NewGo.AddComponent<UIPanel>();
        Panel.name = string.Format("Panel - {0}", WidgetInfo.Name);
        Panel.depth = NGUITools.CalculateNextDepth(targetRootPanel.gameObject);
        Panel.transform.localScale = Vector3.one;
        Panel.transform.localPosition = Vector3.zero; // position = AdjustPosition(WidgetInfo.PosX, WidgetInfo.PosY, targetWidth, targetHeight, targetRootPanel);

        // return the new widget
        return NewGo;
    }

    // CreateEmptyPanel
    #endregion

    #region CreateCheckbox

    /// <summary>
    /// Creates a checkbox
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static void CreateCheckBox(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // create the base GO
        GameObject NewGO = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);

        // Create checkbox
        UIToggle NewToggle = NewGO.AddComponent<UIToggle>();

        // Create an atlas if one does not exist
        if (NGUISettings.atlas == null)
            NGUISettings.atlas = PsdAtlasManager.CreateNewAtlas(psdAssetFolderPath);

        // load the image
        UISpriteData WorkingSprite = GetSpriteData(psdAssetFolderPath, WidgetInfo.Background);
        if (WorkingSprite == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.ImagePath));
            return;
        }

        // add a sprite
        UISprite SpriteWidget = NewGO.AddComponent<UISprite>();
        SpriteWidget.atlas = NGUISettings.atlas;
        SpriteWidget.spriteName = WorkingSprite.name;
        SpriteWidget.pivot = NGUISettings.pivot;
        SpriteWidget.depth = NGUITools.CalculateNextDepth(targetRootPanel.gameObject);
        SpriteWidget.transform.localScale = Vector3.one;
        SpriteWidget.transform.position = AdjustPosition(WidgetInfo.PosX, WidgetInfo.PosY, targetWidth, targetHeight, targetRootPanel);
        SpriteWidget.width = WorkingSprite.width;
        SpriteWidget.height = WorkingSprite.height;
        SpriteWidget.MakePixelPerfect();

        // create a child sprite to hold the Checked state
        UISprite ChildSpriteWidget = CreateChildSprite(
            NewGO,
            string.Format("{0} - Checked Sprite", NewGO.name),
            psdAssetFolderPath,
            WidgetInfo.Checkmark,
            targetRootPanel,
            WidgetInfo.PosX,
            WidgetInfo.PosY,
            targetWidth,
            targetHeight);

        // fail if nothing returned
        if (ChildSpriteWidget == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.ImagePath));
            return;
        }

        // set the toggle sprite
        NewToggle.activeSprite = ChildSpriteWidget;

        // add the box collider
        NGUITools.AddWidgetCollider(NewGO);
    }

    // CreateCheckbox
    #endregion

    #region CreateSlider

    /// <summary>
    /// Creates a slider
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static void CreateSlider(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // create the base GO
        GameObject NewGO = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);

        // add the slider to the parent
        UISlider SliderGO = NewGO.AddComponent<UISlider>();

        // add a sprite to the parent
        UISprite BackgroundSprite = AddSpriteToGo(NewGO, psdAssetFolderPath, WidgetInfo.Background, targetRootPanel, WidgetInfo.PosX, WidgetInfo.PosY, targetWidth, targetHeight);

        // fail if nothing returned
        if (BackgroundSprite == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.Background));
            return;
        }

        // add the box collider
        NGUITools.AddWidgetCollider(NewGO);


        // create the foreground child object
        UISprite ForegroundChild = CreateChildSprite(
            NewGO,
            string.Format("{0} - Foreground", WidgetInfo.Name),
            psdAssetFolderPath,
            WidgetInfo.Foreground,
            targetRootPanel,
            WidgetInfo.PosX,
            WidgetInfo.PosY,
            targetWidth,
            targetHeight);

        // fail if nothing returned
        if (ForegroundChild == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.Foreground));
            return;
        }


        // create the thumb child object
        UISprite ThumbChild = CreateChildSprite(
            NewGO,
            string.Format("{0} - Thumb", WidgetInfo.Name),
            psdAssetFolderPath,
            WidgetInfo.Thumb,
            targetRootPanel,
            WidgetInfo.PosX,
            WidgetInfo.PosY,
            targetWidth,
            targetHeight,
            false);

        // fail if nothing returned
        if (ThumbChild == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.Thumb));
            return;
        }

        // set slider properties
        SliderGO.backgroundWidget = BackgroundSprite;
        SliderGO.foregroundWidget = ForegroundChild;
        SliderGO.thumb = ThumbChild.transform;
    }

    // CreateSlider
    #endregion

    #region CreateProgressbar

    /// <summary>
    /// Creates a progressbar
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static void CreateProgressbar(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the widget info
        XmlWidgetInfo WidgetInfo = new XmlWidgetInfo(xmlNode);

        // create the base GO
        GameObject NewGO = CreateNguiChildBaseGo(WidgetInfo, targetWidth, targetHeight, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput);

        // add the slider to the parent
        UIProgressBar ProgressGO = NewGO.AddComponent<UIProgressBar>();

        // add a sprite to the parent
        UISprite BackgroundSprite = AddSpriteToGo(NewGO, psdAssetFolderPath, WidgetInfo.Background, targetRootPanel, WidgetInfo.PosX, WidgetInfo.PosY, targetWidth, targetHeight);

        // fail if nothing returned
        if (BackgroundSprite == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.Background));
            return;
        }

        // add the box collider
        NGUITools.AddWidgetCollider(NewGO);


        // create the foreground child object
        UISprite ForegroundChild = CreateChildSprite(
            NewGO,
            string.Format("{0} - Foreground", WidgetInfo.Name),
            psdAssetFolderPath,
            WidgetInfo.Foreground,
            targetRootPanel,
            WidgetInfo.PosX,
            WidgetInfo.PosY,
            targetWidth,
            targetHeight);

        // fail if nothing returned
        if (ForegroundChild == null)
        {
            Debug.LogWarning(string.Format("Could not load sprite: '{0}' with file name '{1}'", WidgetInfo.Name, WidgetInfo.Foreground));
            return;
        }


        // set slider properties
        ProgressGO.backgroundWidget = BackgroundSprite;
        ProgressGO.foregroundWidget = ForegroundChild;
    }

    // CreateProgressbar
    #endregion

    #region CreateSlicedSprite

    /// <summary>
    /// Creates a sliced sprite
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static void CreateSlicedSprite(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        GameObject NewSprite = CreateSprite(xmlNode, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput, targetWidth, targetHeight, psdAssetFolderPath);

        // set to sliced
        if (NewSprite != null)
            NewSprite.GetComponent<UISprite>().type = UISprite.Type.Sliced;
    }

    // CreateSlicedSprite
    #endregion

    #region CreateSlicedImageButton

    /// <summary>
    /// Creates a sliced sprite image button
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="targetRootPanel"></param>
    /// <param name="lastAnchor"></param>
    /// <param name="lastAnchorPath"></param>
    /// <param name="actualOutput"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <param name="psdAssetFolderPath"></param>
    public static void CreateSlicedImageButton(XMLNode xmlNode, UIPanel targetRootPanel, Transform lastAnchor, string lastAnchorPath, PsdImportedOutput actualOutput, float targetWidth, float targetHeight, string psdAssetFolderPath)
    {
        // get the button
        GameObject NewButton = CreateImageButton(xmlNode, targetRootPanel, lastAnchor, lastAnchorPath, actualOutput, targetWidth, targetHeight, psdAssetFolderPath);

        // add sliced
        if (NewButton != null)
            NewButton.GetComponent<UISprite>().type = UISprite.Type.Sliced;
    }

    // CreateSlicedImageButton
    #endregion

}
