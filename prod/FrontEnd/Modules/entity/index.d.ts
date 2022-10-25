import {Dictionary} from '../map';

declare class NLCategory{
    name:string;
    uri:string;
    needEnforce:string;
    forceEnforce:string;
    textEnableEnforce:string;
    textEnforceDescYes:string;
    textEnforceDescNo:string;
    classification: string;

    constructor(
        sName: string,
        sURI: string,
        sNeedEnforce: string,
        sForceEnforce: string,
        sTextEnableEnforce: string,
        sTextEnforceDescYes: string,
        sTextEnforceDescNo: string,
        sClassification: string
    )

    NeedEnforce():boolean;
    EditAble():boolean;
}

declare class Meeting{
    Uri:string;
    EntryInfo:string;
    Creator:string;
    CreateTime:string;
    ClassifyOb:string;
    Tags:Dictionary;
}

declare class LinkageData {

    NodeName: string;

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
        _childNodes?: LinkageData[]);
}

export { NLCategory, Meeting, LinkageData };