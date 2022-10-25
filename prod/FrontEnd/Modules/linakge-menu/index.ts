import { Dictionary, TagAttributes } from '../map';
import { LinkageData } from '../entity';
import { LogHelper, TagHelper, StringHelper, XmlHelper, HtmlHelper } from '../helper';

interface IDataSourceFormat {
    Format(sDataSource: string, sDataMIMEType?: string): LinkageData;
}

interface IStyleModify {
    ModifySelectStyle(_selectElement: HTMLSelectElement): void;
    ModifyParagrahStyle(_paraElement: HTMLParagraphElement): void;
    ModifyDivParentStyle(_containerElement: HTMLElement): void;
    ModifyEditableBoxStyle(_inputElement: HTMLInputElement): void;
}

interface IEditable {
    AddEditableBoxToSelect(_selectElement: HTMLSelectElement): HTMLInputElement;
}

class XmlDataSourceFormat implements IDataSourceFormat {

    //implementations
    Format(sDataSource: string, sDataMIMEType: string = 'text/xml'): LinkageData {

        let xmlDoc: any;
        let xmlDocumentElement: any;
        let xmlLinkageData: LinkageData;

        xmlDoc = XmlHelper.loadXMLDoc(sDataSource);
        xmlDocumentElement = xmlDoc ? xmlDoc.documentElement : null;

        if (xmlDocumentElement) {
            xmlLinkageData = this.getLinkageData(xmlDocumentElement, null);
        }
        else {
            LogHelper.log(`Format() failed, DataSource string: ${sDataSource}, xmlDoc: ${xmlDoc}, xmlDocumentElement: ${xmlDocumentElement}.`);
        }

        return xmlLinkageData;
    }

    private getLinkageData(_xmlNode: any, _parentData: LinkageData): LinkageData {

        let xmlLinkageData: LinkageData;

        if (!_xmlNode || _xmlNode.nodeType === XmlHelper.TEXT_TYPE) {
            return null;
        }

        let sNodeName: string = _xmlNode.nodeName;
        let sType: string = _xmlNode.getAttribute(TagHelper.TYPE_ATTR_NAME);
        let sTagName: string = _xmlNode.getAttribute(TagHelper.NAME_ATTR_NAME);
        let sTagValues: string = _xmlNode.getAttribute(TagHelper.VALUES_ATTR_NAME);
        let sEditable: string = _xmlNode.getAttribute(TagHelper.EDITABLE_ATTR_NAME);
        let sDefault: string = _xmlNode.getAttribute(TagHelper.DEFAULT_ATTR_NAME);
        let sRelyOn: string = _xmlNode.getAttribute(TagHelper.RELYON_ATTR_NAME);

        let tagAttrs: TagAttributes = this.createTagAttributesObj(sNodeName, sType, sTagName, sTagValues, sEditable, sDefault, sRelyOn);

        if (TagHelper.checkXmlAttrs(tagAttrs)) {

            TagHelper.completeTagAttrs(tagAttrs);
            TagHelper.convertToLowerCase(tagAttrs);

            xmlLinkageData = new LinkageData(
                tagAttrs[TagHelper.NODE_NAME],
                tagAttrs[TagHelper.TYPE_ATTR_NAME],
                tagAttrs[TagHelper.NAME_ATTR_NAME],
                tagAttrs[TagHelper.VALUES_ATTR_NAME],
                tagAttrs[TagHelper.DEFAULT_ATTR_NAME],
                tagAttrs[TagHelper.EDITABLE_ATTR_NAME],
                tagAttrs[TagHelper.RELYON_ATTR_NAME],
                _parentData
            );
            let elementNodes: Array<any> = this.getElementNodes(_xmlNode);
            let linkageChildNodes: Array<LinkageData> = new Array<LinkageData>(elementNodes.length);

            for (let i = 0; i < elementNodes.length; i++) {
                linkageChildNodes[i] = this.getLinkageData(elementNodes[i], xmlLinkageData);
            }

            xmlLinkageData.ChildNodes = linkageChildNodes;
        }

        return xmlLinkageData;
    }

    private getElementNodes(_xmlNode: any): Array<any> {

        let elementNodes: Array<any> = new Array<any>();

        if (_xmlNode) {
            let curNode: any = _xmlNode.firstChild;
            while (curNode) {
                if (curNode.nodeType === XmlHelper.ELEMENT_TYPE) {
                    elementNodes.push(curNode);
                }
                curNode = curNode.nextSibling;
            }
        }
        else {
            LogHelper.log(`getElementNodes() failed, _xmlNode: ${_xmlNode}.`);
        }

        return elementNodes;
    }

    private createTagAttributesObj(
        _sNodeName: string,
        _sType: string,
        _sTagName: string,
        _sTagValues: string,
        _sEditable: string,
        _sDefault: string,
        _sRelyon: string): TagAttributes {

        let tagAttr: TagAttributes = new TagAttributes();

        tagAttr[TagHelper.NODE_NAME] = _sNodeName;
        tagAttr[TagHelper.TYPE_ATTR_NAME] = _sType;
        tagAttr[TagHelper.NAME_ATTR_NAME] = _sTagName;
        tagAttr[TagHelper.VALUES_ATTR_NAME] = _sTagValues;
        tagAttr[TagHelper.DEFAULT_ATTR_NAME] = _sDefault;
        tagAttr[TagHelper.EDITABLE_ATTR_NAME] = _sEditable;
        tagAttr[TagHelper.RELYON_ATTR_NAME] = _sRelyon;

        return tagAttr;
    }

}

class LinkageMenu implements IEditable {

    private styleModifier: IStyleModify;
    private dataSourceFormatter: IDataSourceFormat;
    private curLinkageData: LinkageData;
    private sDataSource: string;
    private sDataSourceType: string;
    private menuContainer: HTMLElement;

    //.Ctor
    constructor(_modifier: IStyleModify, _formatter: IDataSourceFormat, _container: HTMLElement, _sDataSource: string) {

        this.styleModifier = _modifier;
        this.dataSourceFormatter = _formatter;

        if (_sDataSource) {
            this.sDataSource = _sDataSource;
            this.curLinkageData = this.dataSourceFormatter.Format(this.sDataSource);
        }
        else {
            LogHelper.log(`LinkageMenu constructor() failed, _sDataSource: ${_sDataSource}.`);
        }

        if (_container) {
            this.menuContainer = _container;
        }
        else {
            LogHelper.log(`LinkageMenu constructor() failed, menuContainer is ${this.menuContainer}`);
        }
    }

    bind() {
        if (this.menuContainer && this.curLinkageData) {
            if (this.curLinkageData.NodeName === TagHelper.CLASSIFICATION_NODE_NAME) {
                for (let i: number = 0; i < this.curLinkageData.ChildNodes.length; i++) {
                    this.recurseToBindData(this.menuContainer, this.curLinkageData.ChildNodes[i], '');
                }
            }
        }
        else {
            LogHelper.log(`bind() failed, menuContainer: ${this.menuContainer}, curLinkageData: ${this.curLinkageData}.`);
        }
    }

    //get selected tags in the form of dictionary.
    public getSelectTagDict(): Dictionary {

        let selectTagDict: Dictionary = new Dictionary();

        if (this.menuContainer) {

            let szSelectElement: NodeListOf<HTMLSelectElement> = this.menuContainer.getElementsByTagName('select');

            if (szSelectElement && szSelectElement.length > 0) {

                for (let i: number = 0; i < szSelectElement.length; i++) {

                    let szParaElement: NodeListOf<HTMLParagraphElement> = szSelectElement[i].parentElement.getElementsByTagName('p');

                    if (szParaElement && szParaElement.length > 0) {
                        let paraElement: HTMLParagraphElement = <HTMLParagraphElement>szParaElement[0];
                        let selectElement: HTMLSelectElement = <HTMLSelectElement>szSelectElement[i];

                        if (paraElement && selectElement) {

                            let sTagName: string = StringHelper.trim(paraElement.textContent);
                            let sTagValue: string = StringHelper.trim((<HTMLOptionElement>selectElement.options[selectElement.selectedIndex]).value);
                            selectTagDict[sTagName] = sTagValue;
                        }
                    }
                    else {
                        LogHelper.log(`getSelectTagDict() failed, szParaElement: ${szParaElement}.`);
                    }
                    
                }
            }
            else {
                LogHelper.log(`getSelectTagDict() failed, szSelectElement: ${szSelectElement}.`);
            }

        }
        else {
            LogHelper.log(`getSelectTagDict() failed, menu container is null or empty.`);
        }

        return selectTagDict;
    }

    //set the menu according to the selected tags in the form of dictionary.
    public setSelectTagDict(_rootElement: HTMLElement, _selectTagDict: Dictionary): void {

        if (_selectTagDict) {
            //let rootElement: HTMLDivElement = <HTMLDivElement>document.getElementById('linkageMenuWrapper');
            if (_rootElement) {
                this.recurseToSetTagValue(_rootElement, _selectTagDict);
            }
            else {
                LogHelper.log(`setSelectTagDict() failed, rootElement is null or undefined.`);
            }
        }
        else {
            LogHelper.log(`setSelectTagDict() failed, _selectTagDict is null or undefined.`);
        }

    }

    //protected functions
    protected onSelectChange(evt: Event): void {

        if (evt) {

            let selectElement: HTMLSelectElement = evt.srcElement ? <HTMLSelectElement>evt.srcElement : null;

            if (selectElement) {

                let sParentSelectValue: string = (<HTMLOptionElement>selectElement.options[selectElement.selectedIndex]).value;
                let siblingParaElement: HTMLParagraphElement = HtmlHelper.getSiblingElement<HTMLParagraphElement>(selectElement, 'p');
                let childDivContainer: HTMLDivElement = HtmlHelper.getSiblingElement<HTMLDivElement>(selectElement, 'div');

                if (childDivContainer) {
                    this.clearSubMenu(childDivContainer);
                }

                if (sParentSelectValue && siblingParaElement) {

                    let sTagName: string = siblingParaElement.textContent;

                    if (sTagName) {
                        let curNodeLinkageData_: LinkageData = this.recurseToGetCurNode(sTagName, this.curLinkageData);
                        if (curNodeLinkageData_ && curNodeLinkageData_.ChildNodes) {
                            for (let i = 0; i < curNodeLinkageData_.ChildNodes.length; i++) {
                                this.recurseToBindData(childDivContainer, curNodeLinkageData_.ChildNodes[i], sParentSelectValue);
                            }
                        }
                    }
                    else {
                        LogHelper.log(`onSelectChange() failed, tagName: ${sTagName}.`);
                    }
                }
                else {
                    LogHelper.log(`onSelectChange() failed, sParentSelectValue: ${sParentSelectValue}, siblingParaElement: ${siblingParaElement}.`);
                }
            }
            else {
                LogHelper.log("onSelectChange() failed, source select is null or undefined !");
            }
        }
        else {
            LogHelper.log("onSelectChange() failed, evt is null or undefined !");
        }
    }

    //implementations
    AddEditableBoxToSelect(_selectElement: HTMLSelectElement): HTMLInputElement {

        let container: HTMLDivElement;
        let editableBox: HTMLInputElement;

        if (_selectElement) {
            container = <HTMLDivElement>_selectElement.parentElement;
            editableBox = document.createElement('input');
            container.appendChild(editableBox);

            editableBox.type = 'text';
            editableBox.value = (<HTMLOptionElement>_selectElement.options[_selectElement.selectedIndex]).value;
            editableBox.textContent = (<HTMLOptionElement>_selectElement.options[_selectElement.selectedIndex]).value;

            _selectElement.addEventListener('change', this.selectChangeHandlerForEditableBox, false);
            editableBox.addEventListener('blur', this.editableBoxBlurHandler, false);
        }
        else {
            LogHelper.log(`AddEditableBoxToSelect() failed, _selectElement: ${_selectElement}.`);
        }

        return editableBox;

    }

    private editableBoxBlurHandler(evt: FocusEvent): void {

        if (evt) {
            let editableBox: HTMLInputElement = <HTMLInputElement>evt.srcElement;
            let selectElement: HTMLSelectElement = HtmlHelper.getSiblingElement<HTMLSelectElement>(editableBox, 'select');
            if (editableBox && editableBox.value && selectElement) {

                (<HTMLOptionElement>selectElement.options[selectElement.selectedIndex]).value = editableBox.value;
                (<HTMLOptionElement>selectElement.options[selectElement.selectedIndex]).textContent = editableBox.value;

                //simulate change
                let evt: Event = document.createEvent('Event');
                evt.initEvent('change', true, true);
                selectElement.dispatchEvent(evt);
            }
            else {
                LogHelper.log(`editableBoxBlurHandler() failed, editableBox: ${editableBox}, selectElement: ${selectElement}.`);
            }
        }
        else {
            LogHelper.log(`editableBoxBlurHandler() failed, evt: ${evt}.`);
        }
    }

    private selectChangeHandlerForEditableBox(evt: Event): void {

        let selectElement: HTMLSelectElement;
        let siblingEditableBox: HTMLInputElement;

        if (evt) {
            selectElement = <HTMLSelectElement>evt.srcElement;
            if (selectElement) {
                siblingEditableBox = HtmlHelper.getSiblingElement<HTMLInputElement>(selectElement, 'input');
                if (siblingEditableBox) {
                    siblingEditableBox.value = (<HTMLOptionElement>selectElement.options[selectElement.selectedIndex]).value;
                }
                else {
                    LogHelper.log(`selectChangeHandlerForEditableBox() failed, siblingEditableBox: ${siblingEditableBox}.`);
                }
            }
            else {
                LogHelper.log(`selectChangeHandlerForEditableBox() failed, srcElement: ${evt.srcElement}.`);
            }
        }
        else {
            LogHelper.log(`selectChangeHandlerForEditableBox() failed, evt: ${evt}.`);
        }

    }

    private isValueExistInArray(_sValue: string, _szRelyOn: Array<string>): boolean {

        let bCheckResult: boolean = false;

        if (_sValue && _szRelyOn) {
            for (let i: number = 0; i < _szRelyOn.length; i++) {
                if (_sValue === _szRelyOn[i]) {
                    bCheckResult = true;
                    break;
                }
            }
        }
        else {
            LogHelper.log(`isValueExistInArray() failed, selectValue: ${_sValue}.`);
        }

        return bCheckResult;
    }

    private createParaElement(_sTagName: string): HTMLParagraphElement {
        let paraElement: HTMLParagraphElement = document.createElement("p");
        paraElement.textContent = _sTagName;
        return paraElement;
    }

    private createSelectElement(_szTagValues: Array<string>, _sDefaultValue: string): HTMLSelectElement {
        let selectElement: HTMLSelectElement = document.createElement("select");
        if (_szTagValues) {
            for (let i = 0; i < _szTagValues.length; i++) {
                selectElement.options.add(new Option(_szTagValues[i], _szTagValues[i]));
            }
        }
        if (_sDefaultValue) {
            for (let j = 0; j < selectElement.options.length; j++) {
                if ((<HTMLOptionElement>selectElement.options[j]).value === _sDefaultValue) {
                    (<HTMLOptionElement>selectElement.options[j]).selected = true;
                }
            }
        }
        else {
            LogHelper.log("default value is null or empty , set the first option as default !");
            (<HTMLOptionElement>selectElement.options[0]).selected = true;
        }
        return selectElement;
    }

    private modifyControlStyle(_paraElement: HTMLParagraphElement, _selectElement: HTMLSelectElement, _containerElement: HTMLElement, _editableInputElement?: HTMLInputElement): void {

        if (this.styleModifier) {
            this.styleModifier.ModifyParagrahStyle(_paraElement);
            this.styleModifier.ModifySelectStyle(_selectElement);
            this.styleModifier.ModifyDivParentStyle(_containerElement);

            if (_editableInputElement) {
                this.styleModifier.ModifyEditableBoxStyle(_editableInputElement);
            }
        }
        else {
            LogHelper.log(`modifyControlStyle() failed, styleModifier is null or undefined.`);
        }
    }

    private recurseToGetCurNode(_sTagName: string, _linkageData: LinkageData): LinkageData {

        if (_linkageData) {

            if (_linkageData.NameAttr === _sTagName) {
                return _linkageData;
            }
            else if (_linkageData.ChildNodes && _linkageData.ChildNodes.length > 0) {
                for (let i: number = 0; i < _linkageData.ChildNodes.length; i++) {
                    let resultNode: LinkageData = this.recurseToGetCurNode(_sTagName, _linkageData.ChildNodes[i]);
                    if (resultNode) {
                        return resultNode;
                    }
                }
            }
        }
        else {
            LogHelper.log(`recurseToGetCurNode() failed, linkageData: ${_linkageData}`);
        }
    }

    private recurseToBindData(_container: HTMLElement, _linkageData: LinkageData, _sParentSelectValue: string): void {

        let szValues: Array<string>;
        let szRelyon: Array<string>;

        let paraElement: HTMLParagraphElement;
        let selectElement: HTMLSelectElement;
        let editableBoxElement: HTMLInputElement;
        let divParentElement: HTMLDivElement = document.createElement("div");


        if (_container && _linkageData) {

            //this is a sub-node of a tag node and needs checking relyon values.
            if (_linkageData.ParentData && (_linkageData.ParentData.NodeName === TagHelper.TAG_NODE_NAME) && (_linkageData.NodeName === TagHelper.TAG_NODE_NAME)) {

                szValues = _linkageData.ValuesAttr.split(TagHelper.VALUE_SEPERATOR);
                szRelyon = _linkageData.RelyOnAttr.split(TagHelper.VALUE_SEPERATOR);

                if (this.isValueExistInArray(_sParentSelectValue, szRelyon)) {

                    paraElement = this.createParaElement(_linkageData.NameAttr);
                    selectElement = this.createSelectElement(szValues, _linkageData.DefaultAttr);

                    divParentElement.appendChild(paraElement);
                    divParentElement.appendChild(selectElement);
                    _container.appendChild(divParentElement);

                    if (_linkageData.EditableAttr === 'true') {
                        editableBoxElement = this.AddEditableBoxToSelect(selectElement);
                    }

                    this.modifyControlStyle(paraElement, selectElement, divParentElement, editableBoxElement);
                }

            }
            //this is a sub-node of a classification node and does not need checking relyon values.
            else if (_linkageData.ParentData && (_linkageData.ParentData.NodeName === TagHelper.CLASSIFICATION_NODE_NAME) && (_linkageData.NodeName === TagHelper.TAG_NODE_NAME)) {

                szValues = _linkageData.ValuesAttr.split(TagHelper.VALUE_SEPERATOR);

                paraElement = this.createParaElement(_linkageData.NameAttr);
                selectElement = this.createSelectElement(szValues, _linkageData.DefaultAttr);

                divParentElement.appendChild(paraElement);
                divParentElement.appendChild(selectElement);
                _container.appendChild(divParentElement);

                if (_linkageData.EditableAttr === 'true') {
                    editableBoxElement = this.AddEditableBoxToSelect(selectElement);
                }

                this.modifyControlStyle(paraElement, selectElement, divParentElement, editableBoxElement);
            }
            //invalid sub-node, doing nothing but logging.
            else {
                LogHelper.log(`recurseToBindData failed, parentNode: ${_linkageData.ParentData}, parentNodeName: ${_linkageData.ParentData ? _linkageData.ParentData.NodeName:null}, currentNodeName: ${_linkageData.NodeName}.`);
            }

            //add select onchange event listener.
            if (selectElement) {
                selectElement.addEventListener('change', (evt: Event) => { this.onSelectChange(evt) }, false);
                //selectElement.onchange = (evt: Event) => { this.onSelectChange(evt) };//closure£¬ guarantee this in onSelectChange() point to the instance of current class.
            }

            if (_linkageData.ChildNodes) {

                let childDivContainer: HTMLDivElement = document.createElement("div");
                divParentElement.appendChild(childDivContainer);

                if (selectElement) {
                    for (let i: number = 0; i < _linkageData.ChildNodes.length; i++) {
                        this.recurseToBindData(childDivContainer, _linkageData.ChildNodes[i], (<HTMLOptionElement>selectElement.options[selectElement.selectedIndex]).value);
                    }
                }
            }

        }
        else {
            LogHelper.log(`recurseToBindData failed failed, container: ${_container}, linkageData: ${_linkageData}.`);
        }
    }

    private recurseToSetTagValue(_parentNode: HTMLElement, _tagDict: Dictionary): void {

        let DIV: string = 'div';
        let SELECT: string = 'select';
        let PARA: string = 'p';

        if (!_parentNode || !_tagDict) {
            return;
        }

        for (let i: number = 0; i < _parentNode.childNodes.length; i++) {

            let curNode: Node = _parentNode.childNodes[i];

            if (curNode && curNode.nodeName && (curNode.nodeName.toLowerCase() === PARA)) {

                let sTagName: string = curNode.textContent;
                let sTagValue: string = _tagDict[sTagName];

                if (sTagValue !== null && sTagValue !== undefined) {
                    let selectElement: HTMLSelectElement = curNode.nextSibling ? <HTMLSelectElement>curNode.nextSibling : null;
                    if (selectElement) {
                        this.setOptionForSelect(selectElement, sTagValue);
                    }
                    else {
                        LogHelper.log(`recurseToSetTagValue() failed, selectElement: ${selectElement}.`);
                    }
                }
                else {
                    LogHelper.log(`recurseToSetTagValue() failed, tag name: ${sTagName}, tag value: ${sTagValue}.`);
                }

            }
            else if (curNode && curNode.nodeName && (curNode.nodeName.toLowerCase() === DIV)) {
                this.recurseToSetTagValue(<HTMLDivElement>curNode, _tagDict);
            }

        }

    }

    private setOptionForSelect(_selectElement: HTMLSelectElement, _sValue: string): void {

        let bIsValueExist: boolean = false;
        let siblingParaElement: HTMLParagraphElement;

        if (_selectElement && _sValue) {

            for (let i: number = 0; i < _selectElement.options.length; i++) {
                if ((<HTMLOptionElement>_selectElement.options[i]).value === _sValue) {
                    _selectElement.selectedIndex = i;
                    bIsValueExist = true;
                    break;
                }
            }

            if (!bIsValueExist) {
                _selectElement.options.add(new Option(_sValue, _sValue));
                _selectElement.selectedIndex = _selectElement.options.length - 1;

                siblingParaElement = HtmlHelper.getSiblingElement<HTMLParagraphElement>(_selectElement, 'p');
                if (siblingParaElement) {
                    let sTagName: string = siblingParaElement.textContent;
                    this.appendValueToNode(sTagName, _sValue);
                }
            }

            try {
                let evtInit: EventInit = {  bubbles: true, cancelable: true };
                let evt: Event = new Event('change', evtInit);
                _selectElement.dispatchEvent(evt);
            }
            catch (e) {
                LogHelper.log(`Event not supported, use createEvent instead.`);
                let evt: Event = document.createEvent('Event');
                evt.initEvent('change', true, true);
                _selectElement.dispatchEvent(evt);
            }
        }
        else {
            LogHelper.log(`setOptionForSelect() failed, _selectElement: ${_selectElement}, _sValue: ${_sValue}.`);
        }

    }

    private appendValueToNode(_sTagName: string, _sTagValue: string): void {

        let curLinkageNode: LinkageData;

        if (_sTagName) {
            curLinkageNode = this.recurseToGetCurNode(_sTagName, this.curLinkageData);
            if (curLinkageNode) {
                curLinkageNode.ValuesAttr += TagHelper.VALUE_SEPERATOR + _sTagValue;
            }
            else {
                LogHelper.log(`appendValueToNode() failed, LinkageNode of ${_sTagName} not found.`);
            }
        }
        else {
            LogHelper.log(`appendValueToNode() failed, _sTagName: ${_sTagName}.`);
        }

    }

    private clearSubMenu(_container: HTMLDivElement): void {
        if (_container) {
            while (_container.childNodes.length > 0) {
                _container.removeChild(_container.firstChild);
            }
        }
        else {
            LogHelper.log(`clearSubMenu() failed, container: ${_container}.`);
        }
    }

}

export { IDataSourceFormat, IStyleModify, XmlDataSourceFormat, LinkageMenu };