import {Dictionary} from '../map';

class NLCategory{
    name:string;
    uri:string;
    needEnforce:string;
    forceEnforce:string;
    textEnableEnforce:string;
    textEnforceDescYes:string;
    textEnforceDescNo:string;
    classification:string;

    //.Ctor
    constructor(
        sName:string,
        sURI:string,
        sNeedEnforce:string,
        sForceEnforce:string,
        sTextEnableEnforce:string,
        sTextEnforceDescYes:string,
        sTextEnforceDescNo:string,
        sClassification:string
    ){
        this.name = sName;
        this.uri = sURI;
        this.needEnforce = sNeedEnforce;
        this.forceEnforce = sForceEnforce;
        this.textEnableEnforce = sTextEnableEnforce;
        this.textEnforceDescYes = sTextEnforceDescYes;
        this.textEnforceDescNo = sTextEnforceDescNo;
        this.classification = sClassification;
    }

    NeedEnforce():boolean{
        return this.needEnforce.toLowerCase() === 'true';
    }

    EditAble():boolean{
        return this.forceEnforce.toLowerCase() === 'false';
    }
}

class Meeting{
    Uri:string = "";
    EntryInfo:string = "";
    Creator:string = "";
    CreateTime:string = "";
    ClassifyOb:string = "";
    Tags:Dictionary;

    constructor(){
        this.Tags = new Dictionary();
    }
}

class LinkageData{

    NodeName: string = ''//aim for xml file

    TypeAttr: string;
    NameAttr: string;
    ValuesAttr: string;
    DefaultAttr: string;
    EditableAttr: string;
    RelyOnAttr: string;
    ParentData: LinkageData;
    ChildNodes: LinkageData[];

    constructor(
        _sNodeName: string,
        _sTypeAttr: string,
        _sNameAttr: string,
        _sValuesAttr: string,
        _sDefaultAttr: string,
        _sEditableAttr: string,
        _sRelyOnAttr: string,
        _parentData: LinkageData,
        _childNodes?: LinkageData[]) {

        this.NodeName = _sNodeName;
        this.TypeAttr = _sTypeAttr;
        this.NameAttr = _sNameAttr;
        this.ValuesAttr = _sValuesAttr;
        this.DefaultAttr = _sDefaultAttr;
        this.EditableAttr = _sEditableAttr;
        this.RelyOnAttr = _sRelyOnAttr;
        this.ParentData = _parentData;
        this.ChildNodes = _childNodes;
    };
}

export { NLCategory, Meeting, LinkageData};