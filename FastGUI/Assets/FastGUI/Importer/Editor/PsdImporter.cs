using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Display the import panel
/// </summary>
[ExecuteInEditMode]
public class PsdImporter : EditorWindow
{
    #region Variables

    #region Public_Const

    /// <summary>
    /// Atlas name used for the import
    /// </summary>
    public static string NSettingsAtlasName
    {
        get
        {
            return string.Format("{0}_Atlas", Path.GetFileNameWithoutExtension(PsdApplicationFolder));
        }
    }

    // Public_Const
    #endregion

    #region Public_Static

    /// <summary>
    /// References and output from the importer
    /// </summary>
    public static PsdImportedOutput ActualOutput;

    /// <summary>
    /// Folder used by PsdAtlasManager
    /// </summary>
    public static string SourceFolder = string.Empty;

    /// <summary>
    /// The PSD folder to be loaded
    /// </summary>
    public static Object ObjPSDFolderToLoad;

    /// <summary>
    /// The folder where pre made fonts are stored
    /// </summary>
    public static Object ObjFontFolder;

    /// <summary>
    /// Asset path of the font folder
    /// </summary>
    public static string FontAssetFolderPath = string.Empty;

    /// <summary>
    /// Physical path to font folder
    /// </summary>
    public static string FontApplicationFolder = string.Empty;
    
    // Public_Static
    #endregion

    /// <summary>
    /// The asset path of the PSD folder
    /// </summary>
    private string PsdAssetFolderPath;

    /// <summary>
    /// Path to the assets folder
    /// </summary>
    private readonly string applicationDataPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));

    /// <summary>
    /// Physical file path to the PSD asset folder being parsed
    /// </summary>
    private static string PsdApplicationFolder;

    /// <summary>
    /// Holds the name of the last folder an Atlas check was done on. This is used if the panel imports more than one PSD, and on the first PSD
    /// </summary>
    private string LastFolderChecked;

    /// <summary>
    /// The UIPanel which will be updated from the import
    /// </summary>
    private UIPanel TargetNguiPanel;

    /// <summary>
    /// Holds reference to the last panel updated. Used for multiple imports and on first import
    /// </summary>
    private UIPanel LastCheckedNguiRootPanel;

    /// <summary>
    /// When true, a target panel has been selected
    /// </summary>
    private bool HasTargetPanel;

    /// <summary>
    /// The UIRoot of the target panel
    /// </summary>
    private UIRoot TargetUIRoot;

    /// <summary>
    /// When true, we have found a UIRoot
    /// </summary>
    private bool HasRoot;

    /// <summary>
    /// The UIAtlas to store the imported PSD slices
    /// </summary>
    private UIAtlas TargetAtlas;

    /// <summary>
    /// True if the folder has an XML file
    /// </summary>
    private bool HasXMLFile;

    /// <summary>
    /// Name of the prefab to be loaded
    /// </summary>
    private string STR_ObjPSDFolderToLoadPrefab;

    /// <summary>
    /// True if we have an images folder
    /// </summary>
    private bool HasImagesFolder;

    /// <summary>
    /// Height and width of the PSD
    /// </summary>
    private int PsdHeight, PsdWidth;

    /// <summary>
    /// List of XML nodes in the PSD XML file
    /// </summary>
    private XMLNodeList XmlObjectNodeList;

    /// <summary>
    /// Total number of items in the XML
    /// </summary>
    private static float TotalXmlItens = 1;

    /// <summary>
    /// The GUI background color
    /// </summary>
    private Color GuiBkgColor;

    /// <summary>
    /// When true, we need to output debug info
    /// </summary>
    private bool DebugOutput;

    /// <summary>
    /// Current object name
    /// </summary>
    private static string CurrentObjectName;

    /// <summary>
    /// Current index
    /// </summary>
    private static float CurrentItemIndex = 1;

    // Variables
    #endregion

    #region Constants

    private const string STR_PsdImportedOutputprefab = "PsdImportedOutput.prefab";
    private const string STR_OutputPath = "/Output/";
    private const string STR_SourcePath = "/Source/";
    private const string STR_ImagesPath = "/Images/";
    private const string STR_FastGUIName = "FastGUI";
    private const string STR_ImportedPSDFolder = "Imported PSD Folder:";
    private const string STR_ParentPanel = "Parent Panel:";
    private const string STR_TargetAtlas = "Target Atlas:";
    private const string STR_NPSD_Dataxml = "NPSD_Data.xml";
    private const string STR_Npsd0layer = "npsd>0>Layer";
    private const string STR_Npsd0 = "npsd>0";
    private const string STR_Width = "@DocWidth";
    private const string STR_Height = "@DocHeight";
    private const string STR_Output = "Output";
    private const string STR_UpdateTheUIRoot = "Update the UIRoot";
    private const string STR_ImportIt = "Import it!";
    private const string STR_FontsPrefabFolder = "Fonts Prefab Folder:";
    private const string STR_PSDImportProgress = "PSD Import Progress";
    private const string STR_AtType = "@type";
    private const string STR_Name0 = "ObjectName>0";
    private const string STR__text = "_text";
    private const string STR_XmlNode_ANCHOR = "ANCHOR";
    private const string STR_XmlNode_CLIPPING = "CLIPPING";
    private const string STR_XmlNode_IMAGEBUTTON = "IMAGEBUTTON";
    private const string STR_XmlNode_SPRITE = "SPRITE";
    private const string STR_XmlNode_SLICED_SPRITE = "SLICED_SPRITE";
    private const string STR_XmlNode_SLICED_IMAGEBUTTON = "SLICED_IMAGEBUTTON";
    private const string STR_XmlNode_TEXT_LABEL = "TEXT_LABEL";
    private const string STR_XmlNode_INPUT_TEXT = "INPUT_TEXT";
    private const string STR_XmlNode_CHECKBOX = "CHECKBOX";
    private const string STR_XmlNode_SLIDER = "SLIDER";
    private const string STR_XmlNode_PROGRESSBAR = "PROGRESSBAR";
    private const string STR_Path0 = "path>0";
    private const string STR_XmlNode_EmptyWidget = "EmptyWidget";
    private const string STR_XmlNode_EmptyPanel = "EmptyPanel";

    // Constants
    #endregion

    #region Init_ShowWindow

    /// <summary>
    /// Show the importer panel
    /// </summary>
    [MenuItem("NGUI/FastGUI/Import NGUI Screen")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<PsdImporter>();
    }

    // Init_ShowWindow
    #endregion

    #region OnEnable

    void OnEnable()
    {
        GuiBkgColor = GUI.backgroundColor;
    }

    // OnEnable
    #endregion

    #region OnGUI

    /// <summary>
    /// Draw the window
    /// </summary>
    void OnGUI()
    {
        // header
        GUILayout.Label(STR_FastGUIName, EditorStyles.boldLabel);

        // draw a line
        DrawSeparator();

        #region PSD_Folder

        // PSD folder
        GUILayout.BeginHorizontal();
        GUILayout.Label(STR_ImportedPSDFolder);
        ObjPSDFolderToLoad = EditorGUILayout.ObjectField(ObjPSDFolderToLoad, typeof(Object), false);

        // confirm an XML file exists in the folder, if not, then reset object to null
        if ((ObjPSDFolderToLoad != null) && (AssetDatabase.GetAssetPath(ObjPSDFolderToLoad).IndexOf(".xml") > -1))
            ObjPSDFolderToLoad = null;

        PsdAssetFolderPath = AssetDatabase.GetAssetPath(ObjPSDFolderToLoad);
        PsdApplicationFolder = applicationDataPath + PsdAssetFolderPath;
        GUILayout.EndHorizontal();

        // PSD_Folder
        #endregion

        #region Atlas_Images_Source_Test

        // check if the folder has an existing atlas (it was previously imported)
        GUILayout.BeginVertical();
        if ((PsdApplicationFolder != null) && (LastFolderChecked != PsdApplicationFolder))
        {
            ResetVariables();

            // check if we have an existing "output" folder
            string OutputFolder = string.Format("{0}{1}", PsdApplicationFolder, STR_OutputPath);
            if (Directory.Exists(OutputFolder))
            {
                if (Directory.GetFiles(OutputFolder, STR_PsdImportedOutputprefab, SearchOption.TopDirectoryOnly).Length > 0)
                    ActualOutput = AssetDatabase.LoadAssetAtPath(string.Format("{0}{1}{2}", PsdAssetFolderPath, STR_OutputPath, STR_PsdImportedOutputprefab), typeof(PsdImportedOutput)) as PsdImportedOutput;
            }

            // check if we have a source folder
            string SourceFolder = string.Format("{0}{1}", PsdApplicationFolder, STR_SourcePath);
            if ((Directory.Exists(SourceFolder)) && (ObjPSDFolderToLoad != null))
            {
                STR_ObjPSDFolderToLoadPrefab = string.Format("{0}.prefab", ObjPSDFolderToLoad.name);
                if (Directory.GetFiles(SourceFolder, STR_ObjPSDFolderToLoadPrefab, SearchOption.TopDirectoryOnly).Length > 0)
                    TargetAtlas = AssetDatabase.LoadAssetAtPath(string.Format("{0}{1}", SourceFolder, STR_ObjPSDFolderToLoadPrefab), typeof(UIAtlas)) as UIAtlas;
            }

            // check if we have an images folder
            string ImagesFolder = string.Format("{0}{1}", PsdApplicationFolder, STR_ImagesPath);
            if (Directory.Exists(ImagesFolder))
                HasImagesFolder = (Directory.GetFiles(ImagesFolder, "*.png", SearchOption.TopDirectoryOnly).Length > 0);
            else
                HasImagesFolder = false;

            // set last flags
            LastFolderChecked = PsdApplicationFolder;
            HasXMLFile = CheckHasXML();
        }
        GUILayout.EndVertical();

        // Atlas_Images_Source_Test
        #endregion

        #region Parent_Panel

        // UIPanel setup
        GUILayout.BeginHorizontal();
        GUILayout.Label(STR_ParentPanel);
        TargetNguiPanel = EditorGUILayout.ObjectField(TargetNguiPanel, typeof(UIPanel), true) as UIPanel;
        GUILayout.EndHorizontal();

        // set flag for has panel
        HasTargetPanel = (TargetNguiPanel != null);

        // get the panel's root
        if ((HasTargetPanel) && ((LastCheckedNguiRootPanel != TargetNguiPanel) || (TargetUIRoot == null)))
        {
            // set last checked
            LastCheckedNguiRootPanel = TargetNguiPanel;

            // get the UIRoot of the panel
            TargetUIRoot = (TargetNguiPanel != null) ?
                GetUIRoot(TargetNguiPanel.gameObject) :
                null;

            HasRoot = (TargetUIRoot != null);
        }

        // Parent_Panel
        #endregion

        #region Target_Atlas

        GUILayout.BeginHorizontal();
        GUILayout.Label(STR_TargetAtlas);
        TargetAtlas = EditorGUILayout.ObjectField(TargetAtlas, typeof(UIAtlas), false) as UIAtlas;
        GUILayout.EndHorizontal();

        // Target_Atlas
        #endregion

        #region Font_Folder

        // Font folder
        GUILayout.BeginHorizontal();
        GUILayout.Label(STR_FontsPrefabFolder);
        ObjFontFolder = EditorGUILayout.ObjectField(ObjFontFolder, typeof(Object), false);

        // confirm we have prefabs in the folder
        if ((ObjFontFolder != null) && (AssetDatabase.GetAssetPath(ObjFontFolder).IndexOf(".prefab") > -1))
            ObjFontFolder = null;

        // set path strings
        if (ObjFontFolder != null)
        {
            FontAssetFolderPath = AssetDatabase.GetAssetPath(ObjFontFolder);
            FontApplicationFolder = applicationDataPath + FontAssetFolderPath;
        }

        GUILayout.EndHorizontal();

        // Font_Folder
        #endregion

        #region Debug_Checkbox

        // Debug button

        // draw a line
        DrawSeparator();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Write Debug Info to Console:");
        DebugOutput = EditorGUILayout.Toggle(DebugOutput);

        GUILayout.EndHorizontal();

        // Debug_Checkbox
        #endregion

        #region Action_Buttons

        GUILayout.Space(10);

        // when true, the UIRoot has the same dimensions as the the PSD
        bool IsRootSized = false;

        // if we have a root and folder, then confirm the root height matches the PSD
        if ((TargetUIRoot != null) && (ObjPSDFolderToLoad != null))
        {
            IsRootSized = (TargetUIRoot.manualHeight == PsdHeight && TargetUIRoot.minimumHeight >= PsdHeight);

            if (!IsRootSized)
            {
                GUI.backgroundColor = new Color(171f / 255, 26f / 255, 37f / 255, 1);
                if (GUILayout.Button(STR_UpdateTheUIRoot))
                {
                    UpdateUIRootSize();
                }
                GUI.backgroundColor = GuiBkgColor;
            }
        }

        if (HasXMLFile && HasRoot && HasImagesFolder && IsRootSized && HasTargetPanel)
        {
            GUI.backgroundColor = new Color(17f / 255, 146f / 255, 156f / 255, 1);
            if (GUILayout.Button(STR_ImportIt))
            {
                ParsePsdFolder();
            }
            GUI.backgroundColor = GuiBkgColor;

        }
        else
        {
            // update text displayed in debugger portion of window
            UpdateDebugger();
        }

        // Action_Buttons
        #endregion
    }

    // OnGUI
    #endregion

    #region CheckHasXML

    /// <summary>
    /// Checks if the folder has an XML file. If so, read the file
    /// </summary>
    /// <returns></returns>
    private bool CheckHasXML()
    {
        if (string.IsNullOrEmpty(PsdApplicationFolder))
            return false;

        // if we found the XML file, then read it
        if (Directory.GetFiles(PsdApplicationFolder, STR_NPSD_Dataxml, SearchOption.TopDirectoryOnly).Length > 0)
        {
            ReadXML();
            return true;
        }

        // nothing found
        return false;
    }

    // CheckHasXML
    #endregion

    #region ReadXML

    /// <summary>
    /// Reads the XML file
    /// </summary>
    private void ReadXML()
    {
        // read the XML file
        string XmlData;
        using (StreamReader reader = new StreamReader(string.Format("{0}/{1}", PsdAssetFolderPath, STR_NPSD_Dataxml)))
        {
            XmlData = reader.ReadToEnd();
        }

        // init the xml parser, then parse the doc
        XMLParser parser = new XMLParser();
        XMLNode node = parser.Parse(XmlData);

        // get the list of objects
        XmlObjectNodeList = node.GetNodeList(STR_Npsd0layer);

        // get width and height of PSD
        PsdWidth = int.Parse(node.GetNode(STR_Npsd0)[STR_Width].ToString());
        PsdHeight = int.Parse(node.GetNode(STR_Npsd0)[STR_Height].ToString());

        // get total number of nodes / items
        TotalXmlItens = XmlObjectNodeList.Count;
    }

    // ReadXML
    #endregion

    #region GetUIRoot

    /// <summary>
    /// Returns the UIRoot for the panel
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private UIRoot GetUIRoot(GameObject target)
    {
        // check if the UIRoot is on the target
        if (target.GetComponent<UIRoot>())
            return target.GetComponent<UIRoot>();

        // exit if no parent
        if (target.transform.parent == null)
            return null;

        // use the parent if it has a UIRoot
        if (target.transform.parent.gameObject.GetComponent<UIRoot>())
            return target.transform.parent.gameObject.GetComponent<UIRoot>();

        // parent doesn't have root, so need to recurse to next parent
        return GetUIRoot(target.transform.parent.gameObject);
    }

    // GetUIRoot
    #endregion

    #region DrawSeparator

    /// <summary>
    /// Draws a separator in the panel
    /// </summary>
    private void DrawSeparator()
    {
        GUILayout.Space(12f);

        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = EditorGUIUtility.whiteTexture;
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.color = new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
            GUI.color = Color.white;
        }
    }

    // DrawSeparator
    #endregion

    #region UpdateUIRootSize

    /// <summary>
    /// Changes the UIRoot size to match the PSD
    /// </summary>
    private void UpdateUIRootSize()
    {
        // exit if no transform
        if (TargetUIRoot.transform == null)
            return;

        // calculate new heights
        float calcActiveHeight = PsdHeight;
        TargetUIRoot.minimumHeight = PsdHeight;
        TargetUIRoot.manualHeight = PsdHeight;
        TargetUIRoot.manualHeight = PsdHeight;
        TargetUIRoot.scalingStyle = UIRoot.Scaling.PixelPerfect;

        // if we have a valid height
        if (calcActiveHeight > 0f)
        {
            float size = 2f / calcActiveHeight;

            Vector3 ls = TargetUIRoot.transform.localScale;

            // adjust local scale accordingly
            if (!(Mathf.Abs(ls.x - size) <= float.Epsilon) ||
                !(Mathf.Abs(ls.y - size) <= float.Epsilon) ||
                !(Mathf.Abs(ls.z - size) <= float.Epsilon))
            {
                TargetUIRoot.transform.localScale = new Vector3(size, size, size);
            }
        }
    }


    // UpdateUIRootSize
    #endregion

    #region UpdateDebugger

    /// <summary>
    /// Updates debugger panel text
    /// </summary>
    private void UpdateDebugger()
    {
        DrawSeparator();

        GUILayout.Label(STR_Output, EditorStyles.boldLabel);

        // check if XML file is missing
        if ((!HasXMLFile) && (ObjPSDFolderToLoad != null))
            GUILayout.Label(string.Format("{0} can't be found inside the folder: {1}\n", STR_NPSD_Dataxml, ObjPSDFolderToLoad.name), EditorStyles.wordWrappedMiniLabel);

        // check if images folder is missing
        if ((!HasImagesFolder) && (ObjPSDFolderToLoad != null))
            GUILayout.Label(string.Format("Image Folder can't be found inside the folder: {0}\n", ObjPSDFolderToLoad.name), EditorStyles.wordWrappedMiniLabel);

        // check if panel is missing a UIRoot
        if ((!HasRoot) && (TargetNguiPanel != null))
            GUILayout.Label(string.Format("Can't find the UIRoot of panel: {0}", TargetNguiPanel.name), EditorStyles.wordWrappedMiniLabel);

        // check if no panel
        if (!HasTargetPanel)
            GUILayout.Label("You must select one target Panel", EditorStyles.wordWrappedMiniLabel);

        // check if we have an object to load
        if (ObjPSDFolderToLoad == null)
            GUILayout.Label("You must select one valid PSD export folder", EditorStyles.wordWrappedMiniLabel);

        // check target atlas
        if (TargetAtlas == null)
            GUILayout.Label("A new Atlas will be created", EditorStyles.wordWrappedMiniLabel);

        // fonts
        if (ObjFontFolder == null)
            GUILayout.Label("Default Font will be used", EditorStyles.wordWrappedMiniLabel);

        DrawSeparator();
    }

    // UpdateDebugger
    #endregion

    #region ParseeTargetFolder

    /// <summary>
    /// Parse the XML file and create the prefabs for each control type
    /// </summary>
    private void ParsePsdFolder()
    {
        // get the UIRoot of the NGUI panel
        TargetUIRoot = GetUIRoot(TargetNguiPanel.gameObject);

        // set NGUI atlas
        if (TargetAtlas != null)
            NGUISettings.atlas = TargetAtlas;

        // Make sure we've populated the object node list
        if (XmlObjectNodeList == null)
            ReadXML();

        // fail if node list is still null
        if (XmlObjectNodeList == null)
        {
            Debug.LogError("Could not find any nodes in the PSD XML File. Please re-run the Photoshop plugin");
            return;
        }

        // make sure we have the output prefab, if not then create it
        if (ActualOutput == null)
            CreateOutputPrefab();

        CurrentItemIndex = 1;
        Transform LastAnchor = null;
        string LastAnchorPath = null;

        try
        {
            // loop through each node
            foreach (XMLNode tNode in XmlObjectNodeList)
            {
                // get the type
                string ItemType = tNode[STR_AtType].ToString();
                CurrentItemIndex += 1.0f;

                // get the object name
                CurrentObjectName = tNode.GetNode(STR_Name0)[STR__text].ToString();

                // update the progress bar
                UpdateProgress(DebugOutput);
                
                switch (ItemType)
                {
                    case STR_XmlNode_ANCHOR:
                        //LastXmlPath = tNode.GetNode(STR_Path0)[STR__text].ToString();
                        LastAnchor = PsdWidgeteer.CreateAnchor(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight);
                        LastAnchorPath = string.Format("{0}/{1}", tNode.GetNode(STR_Path0)[STR__text], tNode.GetNode(STR_Name0)[STR__text]);
                        break;

                    case STR_XmlNode_CLIPPING:
                        //LastXmlPath = tNode.GetNode(STR_Path0)[STR__text].ToString();
                        LastAnchor = PsdWidgeteer.CreateClippingPanel(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight);
                        LastAnchorPath = string.Format("{0}/{1}", tNode.GetNode(STR_Path0)[STR__text], tNode.GetNode(STR_Name0)[STR__text]);
                        break;

                    case STR_XmlNode_IMAGEBUTTON:
                        PsdWidgeteer.CreateImageButton(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath); //(tNode, TargetNguiPanel, LastAnchor, LastXmlPath,);
                        break;

                    case STR_XmlNode_TEXT_LABEL:
                        PsdWidgeteer.CreateTextLabel(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_INPUT_TEXT:
                        PsdWidgeteer.CreateInputLabel(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_SPRITE:
                        PsdWidgeteer.CreateSprite(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_CHECKBOX:
                        PsdWidgeteer.CreateCheckBox(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_SLIDER:
                        PsdWidgeteer.CreateSlider(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_PROGRESSBAR:
                        PsdWidgeteer.CreateProgressbar(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_SLICED_SPRITE:
                        PsdWidgeteer.CreateSlicedSprite(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_SLICED_IMAGEBUTTON:
                        PsdWidgeteer.CreateSlicedImageButton(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_EmptyWidget:
                        PsdWidgeteer.CreateEmptyWidget(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;

                    case STR_XmlNode_EmptyPanel:
                        PsdWidgeteer.CreateEmptyPanel(tNode, TargetNguiPanel, LastAnchor, LastAnchorPath, ActualOutput, PsdWidth, PsdHeight, PsdAssetFolderPath);
                        break;
                }
            }

        }

        finally
        {
            // make sure we also clear the progress bar
            EditorUtility.ClearProgressBar();

            // set pixel perfect
            TargetUIRoot.scalingStyle = UIRoot.Scaling.PixelPerfect;

            // reset our variables
            ResetPublicVariables();
            ResetVariables();
        }
    }

    // ParseeTargetFolder
    #endregion

    #region CreateOutputPrefab

    /// <summary>
    /// Create the output prefab
    /// </summary>
    private void CreateOutputPrefab()
    {
        // create the output folder if we don't have one already
        string OutputFolder = string.Format("{0}/Output/", PsdApplicationFolder);
        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);
        AssetDatabase.Refresh();

        // create an empty prefab to hold everything
        string AssetPrefabPath = string.Format("{0}/Output/PsdImportedOutput.prefab", PsdAssetFolderPath);
        Object EmptyPrefab = PrefabUtility.CreateEmptyPrefab(AssetPrefabPath);

        // Create a new game object for the atlas
        GameObject go = new GameObject("PsdImportedOutput");
        go.AddComponent<PsdImportedOutput>();

        // Update the prefab
        PrefabUtility.ReplacePrefab(go, EmptyPrefab);

        // remove from the scene, save and refresh
        DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // load asset into the object
        ActualOutput = AssetDatabase.LoadAssetAtPath(AssetPrefabPath, typeof(PsdImportedOutput)) as PsdImportedOutput;
        ActualOutput.references = new Dictionary<int, int>();
    }

    // CreateOutputPrefab
    #endregion

    #region ResetVariables

    /// <summary>
    /// Resets all variables
    /// </summary>
    private void ResetPublicVariables()
    {
        ObjPSDFolderToLoad = null;
        TargetNguiPanel = null;
        TargetUIRoot = null;
        TargetAtlas = null;
        ObjFontFolder = null;
        FontApplicationFolder = string.Empty;
        FontAssetFolderPath = string.Empty;
    }

    /// <summary>
    /// Resets all variables
    /// </summary>
    private void ResetVariables()
    {
        // clear ngui settings
        NGUISettings.SetString(NSettingsAtlasName, string.Empty);
        ActualOutput = null;
        NGUISettings.atlas = null;
        NGUISettings.fontData = null;
        NGUISettings.fontTexture = null;
        TargetAtlas = null;
        SourceFolder = string.Empty;
    }

    // ResetVariables
    #endregion

    #region UpdateProgress

    /// <summary>
    /// Updates the progress bar
    /// </summary>
    /// <param name="debugOutput"></param>
    public static void UpdateProgress(bool debugOutput)
    {
        string ProgressText = string.Format("Object: {0} ({1}/{2})", CurrentObjectName, CurrentItemIndex, TotalXmlItens);
        EditorUtility.DisplayProgressBar(
            STR_PSDImportProgress,
            ProgressText,
            CurrentItemIndex / TotalXmlItens);
        if (debugOutput)
            Debug.Log(string.Format("Importing {0}", ProgressText));
    }

    // UpdateProgress
    #endregion
}

