#target photoshop

app.bringToFront();

// save ruler info so it can be restored later
var OrigRulerUnits = preferences.rulerUnits;
preferences.rulerUnits = Units.PIXELS;
preferences.typeUnits = TypeUnits.PIXELS;

// settings for the active document
var AppActiveDoc     = app.activeDocument;
var ActiveDocWidth    = AppActiveDoc.width.value;
var ActiveDocHeight   = AppActiveDoc.height.value;

// global variables
var DocName = "";
var TargetImageFolders;
var CenteredLayerData;
var CurrenTextLayerIndex = 0;
var LayersCount = AppActiveDoc.layers.length;

// windo variables
var win, windowResource;
var createProgressWindow, progressWindow;

// xml string
var xmlString ="<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

// add header 
xmlString += "<npsd DocWidth=\"" + ActiveDocWidth + "\" DocHeight=\"" + ActiveDocHeight + "\">\n";

// fix bounds and center of this layer
function FixBounds(doc, layer) 
{
    this.layerWidth = layer.bounds[2].value - layer.bounds[0].value;
    this.layerHeight = layer.bounds[3].value - layer.bounds[1].value;

    this.middleCenterX = this.layerWidth / 2 + layer.bounds[0].value;
    this.middleCenterY = this.layerHeight / 2 + layer.bounds[1].value;

    this.center = this.middleCenterX + ", " + this.middleCenterY;


    return this;
}


 

function ActiveLayerID()
{
   var ref = new ActionReference();
   ref.putEnumerated(app.charIDToTypeID('Lyr '), app.charIDToTypeID('Ordn'), app.charIDToTypeID('Trgt'));
    return executeActionGet(ref).getInteger(app.charIDToTypeID('LyrI'));
}

function CreateDocFromLayer( CurrentDoc, NewName, CurrenTextLayer )
{
    // create a new docu
    var NewLayerDoc = app.documents.add( CurrentDoc.width, CurrentDoc.height, CurrentDoc.resolution, NewName , NewDocumentMode.RGB, DocumentFill.TRANSPARENT); 
    NewLayerDoc.activeLayer.isBackgroundLayer = false;
    
    // set active doc so we are in the correct one
    app.activeDocument = CurrentDoc; 
    
    // duplicate layer
    CurrenTextLayer.duplicate( NewLayerDoc ); 
    
    // new layer doc is now active
    app.activeDocument = NewLayerDoc;
    return NewLayerDoc;
}
function RemoveLayerSetsFromObject(TasrgetObject, DoInvisibleRemoval)
{
    var SetsToRemove = [];

    var LayersLen = TasrgetObject.layers[0].layers.length;
    
    for(var i = 0; i < LayersLen; i++)
    {
        if(TasrgetObject.layers[0].layers[i].typename == "LayerSet")
        {
            SetsToRemove.push(TasrgetObject.layers[0].layers[i]);
        }
    
        else
        {
            if ((DoInvisibleRemoval) && (TasrgetObject.layers[0].layers[i].visible == false))
                    SetsToRemove.push(TasrgetObject.layers[0].layers[i]);
        }
    }

    var RemoveLen= SetsToRemove.length;
    for(var i = 0; i < RemoveLen; i++)
    {
        SetsToRemove[i].remove();
    }
}

// Save doc as a PNG
function SaveAndClose(DocToSave)
{
    DocToSave.trim(TrimType.TRANSPARENT);
    
    DocToSave.saveAs(new File(TargetImageFolders + DocToSave.name + ".png"), new PNGSaveOptions(), true, Extension.LOWERCASE);
    DocToSave.close(SaveOptions.DONOTSAVECHANGES);
}



function ExportSpriteToPNG(TargetObject, ParentPath, LayerType)
{
    var ParentName = GeParentName (ParentPath);
    var TargetObjectName = TargetObject.name;    
    var CurrenTextLayerID = ActiveLayerID();

    // get source name from parent
    TargetObjectName = (ParentName=="") ? 
        TargetObject.name :
        ParentName+"_"+TargetObject.name;

    var NewLayeredDoc = CreateDocFromLayer(AppActiveDoc, TargetObjectName, TargetObject);
    CenteredLayerData = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);
    
    // remove the layer sets
    RemoveLayerSetsFromObject(NewLayeredDoc, true)
    
    CurrenTextLayerIndex++;
    xmlString += "<Layer type='" + LayerType +"'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <ImagePath>"+ TargetObjectName+".png"+"</ImagePath>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n"
    xmlString += "        <LayerWidth>"+ CenteredLayerData.layerWidth +"</LayerWidth>\n"
    xmlString += "        <LayerHeight>"+ CenteredLayerData.layerHeight +"</LayerHeight>\n"
    xmlString += "</Layer> \n";
    
    // save as png, then close this doc
    SaveAndClose(NewLayeredDoc);
}

// export empty widget
function ExportEmptyWidget(TargetObject, ParentPath)
{
    var ParentName = GeParentName (ParentPath);
    var TargetObjectName = TargetObject.name;    
    var CurrenTextLayerID = ActiveLayerID();

    // get source name from parent
    TargetObjectName = (ParentName=="") ? 
        TargetObject.name :
        ParentName+"_"+TargetObject.name;

    var NewLayeredDoc = CreateDocFromLayer(AppActiveDoc, TargetObjectName, TargetObject);
    CenteredLayerData = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);
    
    // remove the layer sets
    RemoveLayerSetsFromObject(NewLayeredDoc, true)
    
    CurrenTextLayerIndex++;
    xmlString += "<Layer type='EmptyWidget'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n"
    xmlString += "        <LayerWidth>"+ CenteredLayerData.layerWidth +"</LayerWidth>\n"
    xmlString += "        <LayerHeight>"+ CenteredLayerData.layerHeight +"</LayerHeight>\n"
    xmlString += "</Layer> \n";
    
    // close this doc (Don't save)
    NewLayeredDoc.close(SaveOptions.DONOTSAVECHANGES);
}

// export empty panel
function ExportEmptyPanel(TargetObject, ParentPath)
{
    var ParentName = GeParentName (ParentPath);
    var TargetObjectName = TargetObject.name;    
    var CurrenTextLayerID = ActiveLayerID();

    // get source name from parent
    TargetObjectName = (ParentName=="") ? 
        TargetObject.name :
        ParentName+"_"+TargetObject.name;

    var NewLayeredDoc = CreateDocFromLayer(AppActiveDoc, TargetObjectName, TargetObject);
    CenteredLayerData = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);
    
    // remove the layer sets
    RemoveLayerSetsFromObject(NewLayeredDoc, true)
    
    CurrenTextLayerIndex++;
    xmlString += "<Layer type='EmptyPanel'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n"
    xmlString += "        <LayerWidth>"+ CenteredLayerData.layerWidth +"</LayerWidth>\n"
    xmlString += "        <LayerHeight>"+ CenteredLayerData.layerHeight +"</LayerHeight>\n"
    xmlString += "</Layer> \n";
    
    // close this doc (Don't save)
    NewLayeredDoc.close(SaveOptions.DONOTSAVECHANGES);
}

// Export sprite data
function ExportSpriteData(TargetObject, ParentPath)
{
    ExportSpriteToPNG(TargetObject, ParentPath, "SPRITE");
}

// Export sliced sprite
function ExportSlicedSpriteData(TargetObject, ParentPath)
{
    ExportSpriteToPNG(TargetObject, ParentPath, "SLICED_SPRITE");
}

// Export an anchor
function ExportClipAnchor(CurrentDoc, TargetObject, ParentPath)
{
    // get layer id, then center and slice this layer
    var CurrenTextLayerID = ActiveLayerID();
    CenteredLayerData = FixBounds(CurrentDoc, TargetObject);
    
    CurrenTextLayerIndex++;
    xmlString += "<Layer type='ANCHOR'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n";
    xmlString += "</Layer> \n";
}


function ExportButton(TargetObject, ParentPath, LayerType)
{
    // get layer id
    var CurrenTextLayerID = ActiveLayerID();
    
    // push layer group to new doc, then center and slice
    var NewLayeredDoc = CreateDocFromLayer(AppActiveDoc, TargetObject.name, TargetObject);
    CenteredLayerData = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);
    
    CurrenTextLayerIndex+=NewLayeredDoc.layerSets[0].layerSets.length;
    
    xmlString += "<Layer type='" + LayerType +"'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n";
    xmlString += "        <ImageStates> \n";
    
    var NewLayeredDocLayerLen = NewLayeredDoc.layerSets[0].layerSets.length;
    for(var i = 0; i < NewLayeredDocLayerLen; i++)
    {
        
        if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "idle")
        {
            NewLayeredDoc.layerSets[0].layerSets[i].name = TargetObject.name+"_idle";
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <idle>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</idle>\n";
        }
    
        else if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "pressed")
        {
            NewLayeredDoc.layerSets[0].layerSets[i].name = TargetObject.name+"_pressed";
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <pressed>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</pressed>\n";
        }
    
        else if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "hover")
        {
            NewLayeredDoc.layerSets[0].layerSets[i].name = TargetObject.name+"_hover";
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <hover>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</hover>\n";
        }
    
        else if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "disabled")
        {
            NewLayeredDoc.layerSets[0].layerSets[i].name = TargetObject.name+"disabled";
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <disabled>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</disabled>\n";
        }
    }

    xmlString += "        </ImageStates>\n";
    xmlString += "</Layer> \n";
    NewLayeredDoc.close(SaveOptions.DONOTSAVECHANGES);
}


// Export a button
function ExportImageButton(TargetObject, ParentPath)
{
    ExportButton(TargetObject, ParentPath, "IMAGEBUTTON");
}

// Export a sliced button
function ExportSlicedButton(TargetObject, ParentPath)
{
    ExportButton(TargetObject, ParentPath, "SLICED_IMAGEBUTTON");
}

// Export a checkbox
function ExportCheckBox(TargetObject, ParentPath)
{
    // get layer id
    var CurrenTextLayerID = ActiveLayerID();
    
    // push layer group to new doc, center and slice
    var NewLayeredDoc = CreateDocFromLayer(AppActiveDoc, TargetObject.name, TargetObject);
    CenteredLayerData = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);
    
    // first part of XML
    xmlString += "<Layer type='CHECKBOX'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n";
    xmlString += "        <ImageStates> \n";
    
    var NewLayeredDocLen = NewLayeredDoc.layerSets[0].layerSets.length;
    CurrenTextLayerIndex+=NewLayeredDocLen;
    
    // loop through each group
    for(var i = 0; i < NewLayeredDocLen; i++)
    {
        if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "checkmark")
        {
            var tName = TargetObject.name+"_checkmark";
            NewLayeredDoc.layerSets[0].layerSets[i].name = tName;
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <checkmark>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</checkmark>\n";
        }
    
        else if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "background")
        {
            var tName = TargetObject.name+"_background";
            NewLayeredDoc.layerSets[0].layerSets[i].name = tName;
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <background>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</background>\n";
        }
    }

    xmlString += "        </ImageStates>\n";
    xmlString += "</Layer> \n";
    
    // close doc
    NewLayeredDoc.close(SaveOptions.DONOTSAVECHANGES);
}

// Export a text label
function ExportLabel(TargetObject, ParentPath)
{
    // get the layer id
    var CurrenTextLayerID = ActiveLayerID();
    //$.writeln ("Layer ID: "+ CurrenTextLayerID);    
    
    // push layer group to a new doc, then slice and center
    var NewLayeredDoc  = CreateDocFromLayer(AppActiveDoc, TargetObject.name, TargetObject);
    CenteredLayerData   = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);

    var TextPrefix = NewLayeredDoc.layers[0].name.substring(0,4) ;
    var TextLayer  = NewLayeredDoc.layers[0].layers[0];

    // txt_ is text label, inp_ is input label
    var TextType = TextPrefix == "txt_" ? "TEXT_LABEL" : "INPUT_TEXT";
        
        
     // exit if not text
     if( TextLayer.kind != LayerKind.TEXT )
     {
             // close the new doc
        NewLayeredDoc.close(SaveOptions.DONOTSAVECHANGES);
        return;
      }
     
    // get the text size and info
    CurrenTextLayerIndex+=1;
    var TheText   = TextLayer.textItem;
    var TextSize   = Math.round( new Number( TheText.size ));
    
    // font name is second segment of the layer name
    var GroupName = NewLayeredDoc.layers[0].name.substring(4);
    var FontIndexEnd = GroupName.indexOf (" ");
    var FontName = GroupName.substring(0, FontIndexEnd); // - 1);
   var ObjectName = TextPrefix + GroupName.substring(FontIndexEnd + 1);


    // build the XML
    xmlString += "<Layer type='"+ TextType +"'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n";
    xmlString += "        <LabelName>"+ ObjectName +"</LabelName>\n";
    xmlString += "        <FontName>"+ FontName +"</FontName>\n";
    xmlString += "        <FontTextSize>"+ TextSize +"</FontTextSize>\n";
    xmlString += "        <FontColor>"+ TheText.color.rgb.red +";"+ TheText.color.rgb.green +";"+ TheText.color.rgb.blue +"</FontColor>\n";
    xmlString += "        <TheText>"+ TheText.contents +"</TheText>\n";
    xmlString += "</Layer>\n";

    // close the new doc
    NewLayeredDoc.close(SaveOptions.DONOTSAVECHANGES);
}


// Export a slider
function ExportUISlider(TargetObject, ParentPath, LayerType)
{
    // get layer id
    var CurrenTextLayerID = ActiveLayerID();
    
    // push layer group to new doc
    var NewLayeredDoc = CreateDocFromLayer(AppActiveDoc, TargetObject.name, TargetObject);
    CenteredLayerData = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);
    
    // write first part of XML
    xmlString += "<Layer type='" + LayerType +"'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n";
    xmlString += "        <ImageStates> \n";
    
    var NewLayeredDocLayerLen = NewLayeredDoc.layerSets[0].layerSets.length;
    CurrenTextLayerIndex += NewLayeredDocLayerLen;
    for(var i = 0; i < NewLayeredDocLayerLen; i++)
    {
        if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "background")
        {
            var tName = TargetObject.name+"_background";
            NewLayeredDoc.layerSets[0].layerSets[i].name = tName;
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <background>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</background>\n";
        }
    
        else if(NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "foreground")
        {
            var tName = TargetObject.name+"_foreground";
            NewLayeredDoc.layerSets[0].layerSets[i].name = tName;
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <foreground>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</foreground>\n";
        }

        else if (NewLayeredDoc.layerSets[0].layerSets[i].name.toLowerCase() == "thumb")
        {
            var tName = TargetObject.name+"_thumb";
            NewLayeredDoc.layerSets[0].layerSets[i].name = tName;
            ExportSprite(NewLayeredDoc, NewLayeredDoc.layerSets[0].layerSets[i]);
            xmlString += "                <thumb>"+ NewLayeredDoc.layerSets[0].layerSets[i].name+".png"+"</thumb>\n";
        }
    
}

    // final xml and close this new doc
    xmlString += "        </ImageStates>\n";
    xmlString += "</Layer> \n";
    NewLayeredDoc.close(SaveOptions.DONOTSAVECHANGES);
}



// Export a slider
function ExportSlider(TargetObject, ParentPath)
{
    ExportUISlider(TargetObject, ParentPath, "SLIDER");
}

// Export a progress bar
function ExportProgressBar(TargetObject, ParentPath)
{
    ExportUISlider(TargetObject, ParentPath, "PROGRESSBAR");
}

function ExportClippingPanel(CurrentDoc, TargetObject, ParentPath)
{
    // inc count, and get layer data
    CurrenTextLayerIndex++;
    var CurrenTextLayerID = ActiveLayerID();
    CenteredLayerData = FixBounds(CurrentDoc, TargetObject);
    
    var tClippingAreaLayer = null;
    var len = TargetObject.layerSets.length-1;
    for(var i = len; i >= 0 ; i--)
    {
        // exit if we founnd "clipping-area"
        // if no layer was found, get the last layer in the group to be the area...
        if(TargetObject.layerSets[i].name.toLowerCase() == "clipping-area")
            break;
    }
    var tClippingData = FixBounds(CurrentDoc, tClippingAreaLayer);


    xmlString += "<Layer type='CLIPPING'> \n";
    xmlString += "        <ObjectName>"+TargetObject.name+"</ObjectName>\n";
    xmlString += "        <ParentName>"+ ParentPath +"</ParentName>\n";
    xmlString += "        <X>"+ CenteredLayerData.middleCenterX +"</X>\n";
    xmlString += "        <Y>"+ CenteredLayerData.middleCenterY +"</Y>\n";
    xmlString += "        <clippingX>"+ tClippingData.layerWidth +"</clippingX>\n";
    xmlString += "        <clippingY>"+ tClippingData.layerHeight +"</clippingY>\n";
    xmlString += "        <LayerID>"+ CurrenTextLayerID +"</LayerID>\n";
    xmlString += "</Layer> \n";
}

// Get the parent name
function GeParentName(ParentPath)
{
    var pathLen = ParentPath.length - 1;    
    if(ParentPath.substring(0, pathLen) == "/")
        ParentPath = ParentPath.substring(0, pathLen);
        
    var lastIndex = ParentPath.lastIndexOf ("/");
    if(lastIndex > -1)
        ParentPath = ParentPath.substring ((lastIndex+1), ParentPath.length);

    return ParentPath;
}

// Export a sprite
function ExportSprite(CurrentDoc, TargetObject)
{
    // push this layer to a new doc, slice it, then save as a png
    var NewLayeredDoc = CreateDocFromLayer(CurrentDoc, TargetObject.name, TargetObject);
    CenteredLayerData = FixBounds(NewLayeredDoc, NewLayeredDoc.layers[0]);
    
    SaveAndClose(NewLayeredDoc);
}

// Read a group from the image
function ReadImageGroup(CurrentGroup, ParentPath)
{
    // get the layers in the group
    LayersCount += CurrentGroup.layerSets.length;
    
    // loop through group layers
    var groupLen = CurrentGroup.layerSets.length-1;
    for(var i = (groupLen); i >= 0 ; i--)
    {
        // get the current group layer
         var objActiveLayer = CurrentGroup.layerSets[i];
        AppActiveDoc.activeLayer = objActiveLayer;
        
        // update the progress bar
        progressWindow.bar.value = (CurrenTextLayerIndex/LayersCount)*100;
        progressWindow.update();
              
// TODO: switch here              

        // handle long names first
        if(objActiveLayer.name.substring(0, 8) == "ibtnslc_" )
        {
            ExportSlicedButton(objActiveLayer, ParentPath);
        }

        else
        {
            // switch on 5 chars
            switch (objActiveLayer.name.substring(0, 5))
            {
                case "ibtn_":
                    ExportImageButton(objActiveLayer, ParentPath);
                    break;
                    
                case "pgsb_":
                    ExportProgressBar(objActiveLayer, ParentPath);
                    break;
                    
                case "clip_":
                    ExportClippingPanel(CurrentGroup, objActiveLayer, ParentPath);
                    break;
                    
                // added - Philip - 04 April 2014
                case "wdgt_":
                    ExportEmptyWidget(objActiveLayer, ParentPath);
                    break;
                    
                // exhausted all the 5 chars, now work on the 4 chars
                default:
                    switch (objActiveLayer.name.substring(0, 4))
                    {
                            case "txt_":
                            case "inp_":
                                ExportLabel(objActiveLayer, ParentPath);
                                break;
                            
                            case "ckb_":
                                ExportCheckBox(objActiveLayer, ParentPath);                            
                                break;
                                
                            case "sld_":
                                ExportSlider(objActiveLayer, ParentPath);
                                break;
                                
                            case "slc_":
                                ExportSlicedSpriteData(objActiveLayer, ParentPath);
                                break;
                                
                             // added - Philip - 22 April 2014
                            case "pnl_":
                                ExportEmptyPanel(objActiveLayer, ParentPath);
                                break;
                                
                            // everything else is just a sprite
                            default:
                                ExportSpriteData(objActiveLayer, ParentPath);
                                break;
                    }
            }
        }
    }
}



windowResource = "dialog {  \
    orientation: 'column', \
    alignChildren: ['fill', 'top'],  \
    preferredSize:[140, 60], \
    text: 'NGUI PSD Photoshop Exporter',  \
    margins:15, \
    \
    bottomGroup: Group{ \
        cancelButton: Button { text: 'Cancel', properties:{name:'cancel'}, size: [120,24], alignment:['right', 'center'] }, \
        exporTextLayers: Button { text: 'Export Layers', properties:{name:'exporTextLayers'}, size: [120,24], alignment:['right', 'center'] }, \
    }\
}"

win = new Window(windowResource);

win.bottomGroup.cancelButton.onClick = function() 
{
  return win.close();
};
win.bottomGroup.exporTextLayers.onClick = function() 
{
    // start the export
    StartExporting();
};

// Begin exporting
function StartExporting()
{
    win.close();
    var exportFolderName    = Folder.selectDialog ("Select the target folder:");

    // exit if nothing found
    if (! exportFolderName)
        return;

    // show the progress window
    progressWindow = createProgressWindow("Exporting...", undefined, 0, 100); 
    progressWindow.show();
    
    DocName = app.activeDocument.name.match(/([^\.]+)/)[1];

    // create the target folder
    var TargetFolder = new Folder(exportFolderName+'/'+DocName+'/');
    TargetFolder.create();     
    
    // create the images folder
    var folder = new Folder(TargetFolder+"/Images");
    folder.create();
    TargetImageFolders = TargetFolder+"/Images/" ;
    
    // Read the image
    ReadImageGroup(AppActiveDoc, "");
    
    // Export XML file folder
    var XMLFilePath = TargetFolder + "/";

    // create a reference to a file for output
    var xmlFile = new File(XMLFilePath.toString().match(/([^\.]+)/)[1] + "NPSD_Data.xml");
    
    // open the file, write the data, then close the file
    xmlFile.open('w');
    xmlFile.writeln(xmlString + "</npsd>");
    xmlFile.close();
    
    // reset ruler units
    preferences.rulerUnits = OrigRulerUnits;

    // completed
    alert("Export Complete!" + "\n" + "Your widgets and sprites have been successfully exported");
}

createProgressWindow = function(title, message, min, max) 
{
  var win;
  win = new Window('palette', title);
  win.bar = win.add('progressbar', undefined, min, max);
  win.bar.preferredSize = [300, 20];
  win.stProgress = win.add("statictext");
  win.stProgress.preferredSize.width = 200;
 
  return win;
};


win.show();
