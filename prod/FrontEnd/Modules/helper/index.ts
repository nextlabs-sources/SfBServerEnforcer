import { Dictionary, TagAttributes } from '../map';

class TagHelper{

    static NODE_NAME: string = 'NodeName';

    //obligation xml node name
    static CLASSIFICATION_NODE_NAME: string = 'classification';
    static TAG_NODE_NAME: string = 'tag';

    //obligation xml node attribute name
    static TYPE_ATTR_NAME: string = 'type';
    static NAME_ATTR_NAME: string = 'name';
    static EDITABLE_ATTR_NAME: string = 'editable';
    static DEFAULT_ATTR_NAME: string = 'default';
    static VALUES_ATTR_NAME: string = 'values';
    static MANDATORY_ATTR_NAME: string = 'mandatory';
    static MULTIPSELECT_ATTR_NAME: string = 'multipSelect';
    static RELYON_ATTR_NAME: string = 'relyOn';
    static VALUE_SEPERATOR: string = '|';

    //obligation xml node attribute values
    static MANUAL_TYPE: string = 'manual';

    //chatroom transport xml node definition
    static NL_ENFORCE: string = 'NLEnforce';
    static NL_ROOM_INFO: string = 'NLRoomInfo';
    static TAGS: string = 'Tags';
    static TAG: string = 'Tag';

    //meeting transport xml node definition
    static MEETING_CMD_NODE: string = 'MeetingCommand';
    static RESULT_CODE_NODE: string = 'ResultCode';
    static FILTER_NODE: string = 'Filters';
    static MEETINGS_NODE: string = 'Meetings';
    static MEETING_INFO_NODE: string = 'MeetingInfo';
    static CLASSIFICATION_NODE: string = 'Classification';
    static TAGS_NODE: string = 'Tags';
    static TAG_NODE: string = 'Tag';

    //meeting transport xml attributes definition
    static OPERATION_TYPE_ATTR: string = 'OperationType';
    static SIP_URI_ATTR: string = 'SipUri';
    static TOKEN_ID_ATTR: string = 'Id';
    static INTERVAL_ATTR: string = 'Interval';
    static SHOW_MEETINGS_ATTR: string = 'ShowMeetings';
    static MEETING_URI_ATTR: string = 'Uri';
    static MEETING_ENTRY_INFO_ATTR: string = 'EntryInfo';
    static MEETING_CREATOR_ATTR: string = 'Creator';
    static MEETING_CREATE_TIME: string = 'CreateTime';
    static MEETING_TAG_NAME_ATTR: string = 'TagName';
    static MEETING_TAG_VALUE_ATTR: string = 'TagValue';


    static getTagsFromObligation(sObXml: string): Dictionary {

        let obligationTags: Dictionary = new Dictionary();
        let obligationXmlDoc: any = XmlHelper.loadXMLDoc(sObXml);

        if (obligationXmlDoc) {
            let docElement: any = obligationXmlDoc.documentElement;
            if (docElement && docElement.childNodes) {
                for (let i: number = 0; i < docElement.childNodes.length; i++) {
                    TagHelper.recurseToGetTagFromObligation(docElement.childNodes[i], obligationTags);
                }
            }
            else {
                LogHelper.log(`getTagsFromObligation() failed, docElement is ${docElement} or docElement.childNodes is null.`);
            }
        }
        else {
            LogHelper.log(`getTagsFromObligation() failed, obligationXmlDoc is ${obligationXmlDoc}`);
        }
        

        return obligationTags;
    }

    static checkClassificationType(_tagAttr: TagAttributes): boolean {

        let bIsValid: boolean = false;

        if (_tagAttr) {
            if (_tagAttr[TagHelper.TYPE_ATTR_NAME] && StringHelper.trim(_tagAttr[TagHelper.TYPE_ATTR_NAME]).toLowerCase() === TagHelper.MANUAL_TYPE) {
                bIsValid = true;
            }
        }
        else {
            LogHelper.log(`checkClassificationType() failed, _tagAttr is null or undefined.`)
        }

        return bIsValid;
    }

    static checkTagNodeAttrs(_tagAttr: TagAttributes): boolean {

        let bIsValid: boolean = false;

        if (_tagAttr) {
            if ((_tagAttr[TagHelper.NAME_ATTR_NAME]) && (_tagAttr[TagHelper.VALUES_ATTR_NAME] !== undefined) && (_tagAttr[TagHelper.VALUES_ATTR_NAME] !== null)) {
                bIsValid = true;
            }
        }
        else {
            LogHelper.log(`checkTagNodeAttrs() failed, _tagAttr is null or undefined.`)
        }

        return bIsValid;
    }

    static checkXmlAttrs(_tagAttr: TagAttributes): boolean {

        let bIsValid: boolean = false;

        if (_tagAttr) {
            if (_tagAttr[TagHelper.NODE_NAME] && StringHelper.trim(_tagAttr[TagHelper.NODE_NAME]).toLowerCase() === TagHelper.CLASSIFICATION_NODE_NAME) {
                bIsValid = TagHelper.checkClassificationType(_tagAttr);
            }
            else if (_tagAttr[TagHelper.NODE_NAME] && StringHelper.trim(_tagAttr[TagHelper.NODE_NAME]).toLowerCase() === TagHelper.TAG_NODE_NAME) {
                bIsValid = TagHelper.checkTagNodeAttrs(_tagAttr);
            }
        }
        else {
            LogHelper.log(`checkXmlAttrs() failed, tag attribute is null or undefined.`);
        }

        return bIsValid;
    }

    static completeTagAttrs(_tagAttr: TagAttributes): void {

        if (_tagAttr) {

            //compelete default value.
            if (!_tagAttr[TagHelper.DEFAULT_ATTR_NAME]) {

                if (!_tagAttr[TagHelper.VALUES_ATTR_NAME]) {

                    _tagAttr[TagHelper.DEFAULT_ATTR_NAME] = '';
                }
                else {
                    let sValues: string = StringHelper.trim(_tagAttr[TagHelper.VALUES_ATTR_NAME]);
                    let szValues: Array<string> = sValues.split(TagHelper.VALUE_SEPERATOR);
                    _tagAttr[TagHelper.DEFAULT_ATTR_NAME] = (szValues && szValues.length > 0) ? szValues[0] : '';
                }
            }

            //compelete editable value.
            if (!_tagAttr[TagHelper.EDITABLE_ATTR_NAME]) {
                _tagAttr[TagHelper.EDITABLE_ATTR_NAME] = 'false';
            }

            //compelete rely on value.
            if (!_tagAttr[TagHelper.RELYON_ATTR_NAME]) {
                _tagAttr[TagHelper.RELYON_ATTR_NAME] = '';
            }
        }
        else {
            LogHelper.log(`completeTagAttrs() failed, tag attriubute is null or undefined.`);
        }
    }

    static convertToLowerCase(_tagAttr: TagAttributes): void {

        if (_tagAttr) {          
            for (let key in _tagAttr) {           

                if(typeof _tagAttr[key] === 'string'){
                    _tagAttr[key] = StringHelper.trim(_tagAttr[key]).toLowerCase();
                }
            }
        }
        else {
            LogHelper.log(`convertToLowerCase() failed, tag attribute is null or undefined.`);
        }

    }

    static buildRequestTagsXmlString(_sXmlTagName: string, _sNameAttrName: string, _sValueAttrName:string, _tagDict: Dictionary): string {

        let sTagsXml: string = '';
        let sTagTemplate: string = '';

        if (_sXmlTagName && _sNameAttrName && _sValueAttrName) {
            for (let key in _tagDict) {
                if (typeof key === 'string') {
                    sTagTemplate += `<${_sXmlTagName} ${_sNameAttrName}="${HtmlHelper.htmlEncode(key)}" ${_sValueAttrName}="${HtmlHelper.htmlEncode(_tagDict[key])}"/>`;
                }
                else {
                    LogHelper.log(`buildRequestTagsXmlString() find something strange, key: ${key}.`);
                }
            }
        }
        else {
            LogHelper.log(`buildRequestTagsXmlString() faild, XmlTagName: ${_sXmlTagName}, NameAttr: ${_sNameAttrName}, ValueAttr: ${_sValueAttrName}.`);
        }

        return sTagTemplate;
    }

    static buildAutoTagsDisplayString(_autoTagDict: Dictionary): string {

        let sAutoTag: string = '';

        if (_autoTagDict) {
            for (let key in _autoTagDict) {
                if (typeof key === 'string') {
                    sAutoTag += `${key} : ${_autoTagDict[key]}<br/><br/>`;
                }
                else {
                    LogHelper.log(`buildAutoTagsDisplayString() find something weired, key: ${key}.`);
                }
            }
        }
        else {
            LogHelper.log(`buildAutoTagsDisplayString() failed, auto tag dict: ${_autoTagDict}.`);
        }

        return sAutoTag;
    }

    static getTagsFromResponseXml(_xmlDoc: any, _sNameAttrName: string, _sValueAttrName: string): Dictionary {

        let tagDict: Dictionary = new Dictionary();

        if (_xmlDoc && _sNameAttrName && _sValueAttrName) {

            let TagsElement: Element = <Element>_xmlDoc.getElementsByTagName(TagHelper.TAGS)[0];

            if (TagsElement) {

                let curElement: Element = <Element>TagsElement.firstChild;

                while (curElement) {
                    if (curElement && curElement.nodeType === XmlHelper.ELEMENT_TYPE) {
                        let sNameAttrValue: string = curElement.getAttribute(_sNameAttrName);
                        let sValueAttrValue: string = curElement.getAttribute(_sValueAttrName);

                        if ((sNameAttrValue !== null && sValueAttrValue !== null) && (sValueAttrValue !== null && sValueAttrValue !== undefined)) {
                            tagDict[sNameAttrValue.toLowerCase()] = sValueAttrValue.toLowerCase();
                        }
                        else {
                            LogHelper.log(`getTagsFromResponseXml() failed, NameAttrValue: ${sNameAttrValue}, ValueAttrValue: ${sValueAttrValue}.`);
                        }
                    }

                    curElement = <Element>curElement.nextSibling;
                }
            }
            else {
                LogHelper.log(`getTagsFromResponseXml() failed, TagsNode: ${TagsElement}.`);
            }
        }
        else {
            LogHelper.log(`getTagsFromResponseXml() failed, xmlDoc: ${_xmlDoc}, NameAttr: ${_sNameAttrName}, ValueAttr: ${_sValueAttrName}.`);
        }

        return tagDict;
    }

    static getTagsFromAutoClassification(_totalDict: Dictionary, _obDict: Dictionary): Dictionary {

        let autoDict: Dictionary = new Dictionary();

        if (_totalDict && _obDict) {
            for (let key in _totalDict) {
                if (typeof key === 'string') {
                    if (_obDict[key] === null || _obDict[key] === undefined) {
                        autoDict[key] = _totalDict[key];
                    }
                }
                else {
                    LogHelper.log(`getTagsFromAutoClassification() find something weired, key: ${key}.`);
                }
            }
        }
        else {
            LogHelper.log(`getTagFromAutoClassification() failed, totalDict: ${_totalDict}, obligationDict: ${_obDict}.`);
        }

        return autoDict;
    }

    static getSelectedTags(_totalDict: Dictionary, _autoDict: Dictionary): Dictionary{

        let selectedTags: Dictionary = new Dictionary();

        let selectedTagNames = _totalDict && Object.keys(_totalDict).filter(tagName => {
            return !_autoDict ? true : (_autoDict[tagName] == null);
        });

        selectedTagNames.forEach(tagName => {
            selectedTags[tagName] = _totalDict[tagName];
        });

        return selectedTags;
    }

    private static recurseToGetTagFromObligation(xmlTagElement: any, obligationDict: Dictionary): void {

        if (!xmlTagElement) {
            return;
        }

        if (xmlTagElement.nodeType === XmlHelper.ELEMENT_TYPE && xmlTagElement.getAttribute(TagHelper.NAME_ATTR_NAME)) {

            let sRawTagName: string = xmlTagElement.getAttribute(TagHelper.NAME_ATTR_NAME);

            if (sRawTagName) {
                let sTagName: string = StringHelper.trim(sRawTagName).toLowerCase();
                obligationDict[sTagName] = '';
            }
            else {
                LogHelper.log(`recurseToGetTagFromObligation() failed, raw tag name: ${sRawTagName}.`);
            }
            
        }
        else {
            LogHelper.log(`recurseToGetTagFromObligation() failed, node type: ${xmlTagElement.nodeType}, tag name: ${xmlTagElement.getAttribute(TagHelper.NAME_ATTR_NAME)}.`);
        }

        if (xmlTagElement.childNodes) {
            for (let i: number = 0; i < xmlTagElement.childNodes.length; i++) {
                TagHelper.recurseToGetTagFromObligation(xmlTagElement.childNodes[i], obligationDict);
            }
        }
        else {
            LogHelper.log(`recurseToGetTagFromObligation() failed, xmlTagElement.childNodes is null !`);
        }
    }

}

class AjaxHelper{

    static UNSET: number = 0; //Client has been created. open() not called yet.
    static OPENED: number = 1; //open() has been called.
    static HEADERS_RECEIVED: number = 2; //send() has been called, and headers and status are available.
    static LOADING: number = 3; //Downloading; responseText holds partial data.
    static DONE: number = 4; //The operation is complete.

    private static MAX_XHR: number = 10;
    private static ajaxPool: Array<XMLHttpRequest>;

    static getXhrInstance(): XMLHttpRequest {

        let bHasAvailabelXhr: boolean = false;
        let availabelXhr: XMLHttpRequest;

        if (!AjaxHelper.ajaxPool) {
            AjaxHelper.initAjaxPool();
        }

        for (let i: number = 0; i < AjaxHelper.ajaxPool.length; i++) {
            if (AjaxHelper.isXhrAvailabel(AjaxHelper.ajaxPool[i])) {
                availabelXhr = AjaxHelper.ajaxPool[i];
                bHasAvailabelXhr = true;
                break;
            }
        }

        if (!bHasAvailabelXhr) {
            availabelXhr = AjaxHelper.createXHR();
            AjaxHelper.ajaxPool.push(availabelXhr);
        }

        return availabelXhr;
    }

    private static initAjaxPool(): void {

        AjaxHelper.ajaxPool = new Array<XMLHttpRequest>(AjaxHelper.MAX_XHR);

        for (let i: number = 0; i < AjaxHelper.MAX_XHR; i++) {
            AjaxHelper.ajaxPool[i] = AjaxHelper.createXHR();
        }
    }

    private static createXHR(): XMLHttpRequest {

        let xhr: XMLHttpRequest;

        try {
            xhr = new XMLHttpRequest();
        }
        catch (e) {
            LogHelper.log(`createXHR() failed, error message: ${e.message}.`);
        }

        return xhr;
    }

    private static isXhrAvailabel(_xhr: XMLHttpRequest): boolean {

        let bIsAvailabel: boolean = false;

        if (_xhr) {
            if ((_xhr.readyState === AjaxHelper.UNSET || _xhr.readyState === AjaxHelper.DONE) && !_xhr.onreadystatechange) {
                bIsAvailabel = true;
            }
        }
        else {
            LogHelper.log(`isXhrAvailabel() failed, xhr is ${_xhr}.`);
        }

        return bIsAvailabel;
    }
}

class StringHelper{

    static cmdFormat(sCmdTemplate:string, ...args:string[]):string{

        if(sCmdTemplate){
            for(let i = 0; i < args.length; i++ )   
            {
                sCmdTemplate = sCmdTemplate.replace('%' + i, args[i]);
            }
        }        

        return sCmdTemplate;  
    }

    static trim(sTarget:string):string{

        if(sTarget){
            if(sTarget.trim){
                sTarget = sTarget.trim();
            }
            else{
                sTarget = StringHelper.trimPolyfill(sTarget);
            }
        }

        return sTarget;
    }

    private static trimPolyfill(_sTarget:string):string{

        let trimReg = /^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g;
        _sTarget = _sTarget.replace(trimReg, '');

        return _sTarget;
    }
}

class XmlHelper{

    //xml node types
    static ELEMENT_TYPE: number = 1;
    static TEXT_TYPE: number = 3;
    static DOCUMENT_TYPE: number = 9;

    static loadXMLDoc(_sXml: string): any {
        let xmlDoc: any;
        if (_sXml) {
            try //Edge, IE 9+, Chrome 1.0+, Firefox 1.0+, Opera 8+, Safari 3.2+.
            {
                let parser: DOMParser = new DOMParser();
                xmlDoc = parser.parseFromString(_sXml, "application/xml");
            }
            catch (e) {
                alert(e.message);
            }
        }
        else {
            LogHelper.log(`loadXMLDoc() failed, Xml string: ${_sXml}.`);
        }

        return xmlDoc;
    }

    static convertXmlDocumentToString(xmlDoc: any): string {
        let xmlStr: string = "";
        if (xmlDoc) {
            try{
                let declarReg = /^<\?xml\s+version\="1\.0"(?:\s+encoding="[\w+\_\-\.]+")?(?:\s+standalone="(?:yes|no)")?\s*\?>/;
                let serializer: XMLSerializer = new XMLSerializer();
                xmlStr = serializer.serializeToString(xmlDoc);
                let bHasDelcaration: boolean = declarReg.test(xmlStr);
                if (!bHasDelcaration) {
                    xmlStr = `<?xml version = "1.0" encoding ="utf-8"?>${xmlStr}`;
                }
            }
            catch(e){
                LogHelper.log(`convertXmlDocumentToString() error: name: ${e.name}, message: ${e.message}.`);
            }
        }
        else {
            LogHelper.log('convertXmlDocumentToString() failed , xmlDoc is null or undefined !');
        }

        return xmlStr;
    }
}

class LogHelper{
    static log(sMsg: string): void {
        console.log(sMsg);
    }
}

class HtmlHelper {

    static LOADING_STATE: string = 'loading';
    static INTERACTIVE_STATE: string = 'interactive';
    static COMPLETE_STATE: string = 'complete';

    static getSiblingElement<T extends HTMLElement>(_element: HTMLElement, _sNodeName: keyof HTMLElementTagNameMap): T {

        let targetElement: T;
        let curNode: Node;
        let targetParentNode: Node;

        if (_element) {
            targetParentNode = _element.parentNode;
            curNode = targetParentNode ? targetParentNode.firstChild : null;

            while (curNode) {
                if (curNode.nodeName && StringHelper.trim(curNode.nodeName).toLowerCase() === _sNodeName) {
                    targetElement = <T>curNode;
                    break;
                }

                curNode = curNode.nextSibling;
            }
        }
        else {
            LogHelper.log(`getSiblingElement() failed, element: ${_element}.`);
        }

        return targetElement;
    }

    static insertAfter(_newElement: HTMLElement, _baseElement: HTMLElement): void {

        let parentNode: Node;
        let siblingNode: Node;

        if (_baseElement) {
            parentNode = _baseElement.parentNode;
        }
        else {
            LogHelper.log(`insertAfter() failed, _baseElement: ${_baseElement}, _newElement: ${_newElement}.`);
        }

        if (parentNode && _newElement) {
            siblingNode = _baseElement.nextSibling;
            if (siblingNode) {
                parentNode.insertBefore(_newElement, siblingNode);
            }
            else {
                LogHelper.log(`insertAfter() failed, siblingNode: ${siblingNode}.`);
            }
        }
        else {
            LogHelper.log(`insertAfter() failed, parentNode: ${parentNode}, _newElement: ${_newElement}.`);
        }
    }

    static clearSubNodes(_targetElement: HTMLElement): void {
        if (_targetElement) {
            _targetElement.textContent = '';
        }
        else {
            LogHelper.log(`clearSubNodes() failed, targetElement: ${_targetElement}.`);
        }
    }

    static htmlEncode(_sTarget: string): string {

        let sEncode: string = '';
        let container: HTMLDivElement = document.createElement("div");
        container.innerHTML = _sTarget;
        sEncode = container.innerHTML;
        sEncode = sEncode.replace("'", "&#39;");
        sEncode = sEncode.replace("\"", "&quot;");

        return sEncode;
    }

    static htmlDecode(_sTarget: string): string {

        let sDecode = '';
        let container: HTMLDivElement = document.createElement("div");
        container.innerHTML = _sTarget;
        sDecode = container.textContent;

        return sDecode;
    }
}

class UrlHelper {

    static parseQueryString(_sUrl: string): Dictionary {

        let queryDict: Dictionary = new Dictionary();
        let sQueryString: string;
        let nQuestionMarkIndex: number;
        let szParamPair: Array<string>;

        if (_sUrl) {
            nQuestionMarkIndex = _sUrl.indexOf('?');
            if (nQuestionMarkIndex > -1) {
                sQueryString = _sUrl.substring(nQuestionMarkIndex + 1);
                szParamPair = sQueryString.split('&');
                for (let i: number = 0; i < szParamPair.length; i++) {
                    let nFirstEqualMarkIndex: number = szParamPair[i].indexOf('=', 0);
                    if (nFirstEqualMarkIndex > -1) {
                        let sKey: string = szParamPair[i].substring(0, nFirstEqualMarkIndex);
                        let sValue: string = szParamPair[i].substring(nFirstEqualMarkIndex + 1);

                        if (sKey && sValue) {
                            queryDict[sKey] = sValue;
                        }
                    }
                }
            }
            else {
                LogHelper.log(`parseQueryString() failed, questionMarkIndex: ${nQuestionMarkIndex}.`);
            }
        }
        else {
            LogHelper.log(`parseQueryString() failed, url: ${_sUrl}.`);
        }

        return queryDict;
    }

    static addTimeStamp(_sUrl: string): string {

        let sReqUrl: string = _sUrl;
        let sTimeStamp: string = (new Date()).valueOf().toString();

        if (sReqUrl && (sReqUrl.indexOf('?') > -1)) {
            sReqUrl += `&t=${sTimeStamp}`;
        }
        else if (sReqUrl && sReqUrl.indexOf('?') < 0) {
            sReqUrl += `?t=${sTimeStamp}`;
        }
        else {
            LogHelper.log(`addTimeStamp() failed, url: ${_sUrl}.`);
        }

        return sReqUrl;
    }

    static checkHttpValidation(_sUrl:string):boolean{
        
        let bResult:boolean = false;
        let httpReg = /^(http|https)\:\/\/\w+/i;

        bResult = httpReg.test(_sUrl);

        return bResult;
    }

    static checkSipValidation(_sUrl:string):boolean{

        let bResult:boolean = false;
        let sipReg = /(^sip)\:\w+/i;

        bResult = sipReg.test(_sUrl);

        return bResult;        
    }
}

export { TagHelper, AjaxHelper, StringHelper, XmlHelper, LogHelper, HtmlHelper, UrlHelper };