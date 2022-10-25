import { Dictionary, TagAttributes } from '../map';

declare class TagHelper {

    static NODE_NAME: string;

    //obligation xml node name
    static CLASSIFICATION_NODE_NAME: string;
    static TAG_NODE_NAME: string;

    //obligation xml node attribute name
    static TYPE_ATTR_NAME: string;
    static NAME_ATTR_NAME: string;
    static EDITABLE_ATTR_NAME: string;
    static DEFAULT_ATTR_NAME: string;
    static VALUES_ATTR_NAME: string;
    static MANDATORY_ATTR_NAME: string;
    static MULTIPSELECT_ATTR_NAME: string;
    static RELYON_ATTR_NAME: string;
    static VALUE_SEPERATOR: string;

    //obligation xml node attribute values
    static MANUAL_TYPE: string;

    //chatroom transport xml node definition
    static NL_ENFORCE: string;
    static NL_ROOM_INFO: string;
    static TAGS: string;
    static TAG: string;

    //meeting transport xml node definition
    static MEETING_CMD_NODE: string;
    static RESULT_CODE_NODE: string;
    static FILTER_NODE: string;
    static MEETINGS_NODE: string;
    static MEETING_INFO_NODE: string;
    static CLASSIFICATION_NODE: string;
    static TAGS_NODE: string;
    static TAG_NODE: string;

    //meeting transport xml attributes definition
    static OPERATION_TYPE_ATTR: string;
    static SIP_URI_ATTR: string;
    static TOKEN_ID_ATTR: string;
    static INTERVAL_ATTR: string;
    static SHOW_MEETINGS_ATTR: string;
    static MEETING_URI_ATTR: string;
    static MEETING_ENTRY_INFO_ATTR: string;
    static MEETING_CREATOR_ATTR: string;
    static MEETING_CREATE_TIME: string;
    static MEETING_TAG_NAME_ATTR: string;
    static MEETING_TAG_VALUE_ATTR: string;

    static getTagsFromObligation(sObXml: string): Dictionary;
    static checkClassificationType(_tagAttr: TagAttributes): boolean;
    static checkTagNodeAttrs(_tagAttr: TagAttributes): boolean;
    static checkXmlAttrs(_tagAttr: TagAttributes): boolean;
    static completeTagAttrs(_tagAttr: TagAttributes): void;
    static convertToLowerCase(_tagAttr: TagAttributes): void;
    static buildAutoTagsDisplayString(_autoTagDict: Dictionary): string;
    static buildRequestTagsXmlString(_sXmlTagName: string, _sNameAttrName: string, _sValueAttrName: string, _tagDict: Dictionary): string;
    static getTagsFromResponseXml(_xmlDoc: any, _sNameAttrName: string, _sValueAttrName: string): Dictionary;
    static getTagsFromAutoClassification(_totalDict: Dictionary, _obDict: Dictionary): Dictionary;
    static getSelectedTags(_totalDict: Dictionary, _autoDict: Dictionary): Dictionary;
}

declare class AjaxHelper {

    static UNSET: number;
    static OPENED: number;
    static HEADERS_RECEIVED: number;
    static LOADING: number;
    static DONE: number;

    static getXhrInstance(): XMLHttpRequest;

}

declare class StringHelper {
    static cmdFormat(sCmdTemplate: string, ...args: string[]): string;
    static trim(sTarget: string): string;
}

declare class XmlHelper {

    //xml node types
    static ELEMENT_TYPE: number;
    static TEXT_TYPE: number;
    static DOCUMENT_TYPE: number;

    static loadXMLDoc(sXml: string): any;
    static convertXmlDocumentToString(xmlDoc: any): string;
}

declare class LogHelper {
    static log(sMsg: string): void;
}

declare class HtmlHelper {

    static LOADING_STATE: string;
    static INTERACTIVE_STATE: string;
    static COMPLETE_STATE: string;

    static getSiblingElement<T extends HTMLElement>(_element: HTMLElement, _sNodeName: keyof HTMLElementTagNameMap): T;
    static insertAfter(_newElement: HTMLElement, _baseElement: HTMLElement): void;
    static clearSubNodes(_targetElement: HTMLElement): void;
    static htmlEncode(_sTarget: string): string;
    static htmlDecode(_sTarget: string): string;
}

declare class UrlHelper {
    static parseQueryString(_sUrl: string): Dictionary;
    static addTimeStamp(_sUrl: string): string;
    static checkHttpValidation(_sUrl:string):boolean;
    static checkSipValidation(_sUrl:string):boolean;
}

export { TagHelper, AjaxHelper, StringHelper, XmlHelper, LogHelper, HtmlHelper, UrlHelper };
