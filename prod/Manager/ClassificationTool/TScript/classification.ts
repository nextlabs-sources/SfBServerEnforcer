import { TagService } from './tagservice';
import { TagClassMap, TagIdMap, IconClassMap, EntityState } from './uiresource';
import { LogHelper, HtmlHelper } from '../../../../jsbin/Modules/helper';
import { Util } from '../../../../jsbin/Modules/util';

class Classification{
    
    //memebers
    private m_tagService: TagService = undefined;

    //.ctor
    constructor(bDebug: boolean){
        if (bDebug){
            this.addEventListeners();
        }else{
            this.m_tagService = new TagService();
            //this.initData.call(this);
        }
    }

    //public methods
    public initData(): void{
        try{
            let sTags: string = this.m_tagService.getTagsInJSON();
            if(sTags != null && sTags != undefined){
                this.displayTags(sTags);
                this.addEventListeners();
            }
            else{
                throw Error('tags in json is null');
            }
        }
        catch(e){
            alert(`initData failed, name: ${e.name}, message: ${e.message}.`);
        }
    }

    //private methods
    private addEventListeners(): void{
        
        let scope: Classification = this;

        let redCrosses: HTMLCollectionOf<Element> = document.getElementsByClassName(TagClassMap.tagDelete);
        let tagRows: HTMLCollectionOf<Element> = document.getElementsByClassName(TagClassMap.tagListItem);
        let tagAddNewBtn: HTMLElement = document.getElementById(TagIdMap.tagAddNewBtn);
        let tagOkBtn: HTMLElement = document.getElementById(TagIdMap.tagOkBtn);
        let tagCancelBtn: HTMLElement = document.getElementById(TagIdMap.tagCancelBtn);
        let tagApplyBtn: HTMLElement = document.getElementById(TagIdMap.tagApplyBtn);
        let tagSaveBtn: HTMLElement = document.getElementById(TagIdMap.tagSaveBtn);
        let tagDefaultSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagDefaultSelect);
        const tagNameBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagNameBox);
        const tagValueBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagValuesBox);
        const tagEditableSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagEditableSelect);

        //add click handler for btns
        tagAddNewBtn.addEventListener('click', function(evt: Event){
            scope.addNewBtnClickHandlerEx.call(scope, evt);
        }, false);

        tagOkBtn.addEventListener('click', function(evt: Event){
            scope.okBtnClickHandler.call(scope, evt);
        }, false);

        tagCancelBtn.addEventListener('click', function(evt: Event){
            scope.cancelBtnClickHandler.call(scope, evt);
        }, false);

        tagApplyBtn.addEventListener('click', function(evt: Event){
            scope.applyBtnClickHandler.call(scope, evt);
        }, false);

        tagSaveBtn.addEventListener('click', function(evt: Event){
            scope.saveBtnClickHandler.call(scope, evt);
        }, false);

        tagNameBox.oninput = this.valueOnchangeHandler.bind(this);
        tagValueBox.oninput = this.valueOnchangeHandler.bind(this);
        tagDefaultSelectEl.onchange = this.valueOnchangeHandler.bind(this);

        tagDefaultSelectEl.addEventListener('mouseenter', function(evt: MouseEvent): any{
            scope.defaultSelectMouseEnterHandler.call(scope, evt);
        }, false);

        tagEditableSelect.onchange = this.valueOnchangeHandler.bind(this);
    }

    private displayTags(sTags: string): void{
        let tagsArray: Array<any> = <Array<any>>JSON.parse(sTags);
        let tagListBody: HTMLDivElement = <HTMLDivElement>document.getElementsByClassName('tag-list-body')[0];
        for(let i:number = 0; i < tagsArray.length; i++){
            let tagRow: HTMLDivElement = this.createTagRow(tagsArray[i]);
            if(tagRow && tagRow.nodeType === 1){
                tagListBody.appendChild(tagRow);
            }
        }
    }

    private createTagRow(tagObj: any): HTMLDivElement{

        let rowDiv: HTMLDivElement = document.createElement('div');
        let iconDiv: HTMLDivElement = document.createElement('div');
        let tagSpan: HTMLSpanElement = document.createElement('span');
        let crossSpan: HTMLSpanElement = document.createElement('span');

        rowDiv.classList.add('tag-list-item');
        iconDiv.classList.add('tag-delete');
        tagSpan.classList.add('tag-name');
        crossSpan.classList.add('red-cross');

        tagSpan.textContent = HtmlHelper.htmlDecode(tagObj[TagService.TAG_NAME]);
        tagSpan.setAttribute('data-tag', JSON.stringify(tagObj));
        iconDiv.appendChild(crossSpan);
        rowDiv.appendChild(tagSpan);
        rowDiv.appendChild(iconDiv);

        let scope: Classification = this;
        //add event handler for red-cross click
        crossSpan.addEventListener('click', function(evt: Event){
            scope.tagDeleteClickHandler.call(scope, evt);
        }, false);

        //add event handler for tagrow click
        rowDiv.addEventListener('click', function(evt){
            scope.tagRowClickHandler.call(scope, evt);
        }, false);

        return rowDiv;
    }

    private eliminateClickState(this: Classification): void{
        let tags: HTMLCollectionOf<Element> = document.getElementsByClassName(TagClassMap.tagListItem);
        Util.forEach(tags, function(el: Element){
            if(el && el.nodeType === 1){
                el.classList.remove(TagClassMap.tagListItemClick);
            }
            else{
                LogHelper.log(`eliminateClickState() -> forEach() failed, el: ${el}`);
            }
        }, this);
    }

    private constructEmptyTag(): any{
        let tag: any = {};

        tag[TagService.TAG_NAME] = '';
        tag[TagService.TAG_VALUE] = '';
        tag[TagService.DEFAULT_VALUE] = '';
        tag[TagService.EDITABLE] = 'false';
        tag[TagService.MULTI_SELECT] = 'false';
        tag[TagService.MANDATORY] = 'false';
        tag[TagService.State] = EntityState.Added;

        return tag;
    }

    private displayTagDetail(tagObj: any): void{
        let scope: Classification = this;
        let tagNameBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagNameBox);
        let tagValuesBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagValuesBox);
        let defaultValueSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagDefaultSelect);
        let editableSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagEditableSelect);

        if(tagNameBox && tagValuesBox && defaultValueSelect && editableSelect){

            this.clearTagDetails();

            //append new value
            tagNameBox.value = HtmlHelper.htmlDecode(tagObj[TagService.TAG_NAME]);
            tagValuesBox.value = HtmlHelper.htmlDecode(tagObj[TagService.TAG_VALUE]);

            let valuesArray: string[] = HtmlHelper.htmlDecode(tagObj[TagService.TAG_VALUE]).split('|');
            Util.forEach(valuesArray, function(sValue: string){
                let option: HTMLOptionElement = document.createElement('option');
                option.value = sValue;
                option.textContent = sValue;
                option.className = TagClassMap.tagDetailValue;
                defaultValueSelect.appendChild(option);
            }, scope);

            let editableArray: string[] = ['true', 'false'];
            Util.forEach(editableArray, function(sEditable: string){
                let option: HTMLOptionElement = document.createElement('option');
                option.value = sEditable;
                option.textContent = sEditable;
                option.className = TagClassMap.tagDetailValue;
                editableSelect.appendChild(option);
            }, scope);

            this.matchOption(defaultValueSelect, HtmlHelper.htmlDecode(tagObj[TagService.DEFAULT_VALUE]));
            this.matchOption(editableSelect, HtmlHelper.htmlDecode(tagObj[TagService.EDITABLE]));
        }
        else{
            LogHelper.log(`displayTagDetail failed, tagNameBox: ${tagNameBox}, tagValuesBox: ${tagValuesBox}, defaultValueSelect: ${defaultValueSelect}, editableSelect: ${editableSelect}`);
        }
    }

    //the parameter should be htmlDecoded
    private matchOption(el: HTMLSelectElement, sValue: string): void{
        Util.forEach(el.options, function(option: HTMLOptionElement){
            if(option.value.toLowerCase() === sValue.toLowerCase()){
                option.selected = true;
                return;
            }
        }, this);
    }

    private clearTagDetails(): void{

        let tagNameBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagNameBox);
        let tagValuesBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagValuesBox);
        let defaultValueSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagDefaultSelect);
        let editableSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagEditableSelect);

        if(tagNameBox && tagValuesBox && defaultValueSelect && editableSelect){
            tagNameBox.value = '';
            tagValuesBox.value = '';
            defaultValueSelect.textContent = '';
            editableSelect.textContent = '';  
        }
        else{
            LogHelper.log(`clearTagDetails failed, tagNameBox: ${tagNameBox}, tagValuesBox: ${tagValuesBox}, defaultValueSelect: ${defaultValueSelect}, editableSelect: ${editableSelect}`);
        }
    }

    private displayClassificationView(): void{

        let tagPanelBodyEl: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagPanelBody)[0];
        let tagPanelFooterEl: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagPanelFoot)[0];
        
        if(tagPanelBodyEl && tagPanelFooterEl){
            tagPanelBodyEl.classList.add('block-show');
            tagPanelFooterEl.classList.add('block-show');
        }
    }

    private hideClassificationView(): void{

        let tagPanelBodyEl: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagPanelBody)[0];
        let tagPanelFooterEl: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagPanelFoot)[0];
        
        if(tagPanelBodyEl && tagPanelFooterEl){
            tagPanelBodyEl.classList.remove('block-show');
            tagPanelFooterEl.classList.remove('block-show');
        }
    }

    //event handlers
    private tagDeleteClickHandler(this:Classification, evt: Event): any{
        if(!evt){
            return false;//stop from bubbling to the tag-row
        }

        let curTarget: HTMLElement = <HTMLElement>evt.target;
        let crossWrapper: HTMLElement = curTarget ? curTarget.parentElement : undefined;
        let tagRow: HTMLElement = crossWrapper ? crossWrapper.parentElement : undefined;
        let tagListBody: HTMLElement = tagRow ? tagRow.parentElement : undefined;
        let tagNameSpan: HTMLElement = undefined;

        if(tagListBody && tagRow){
            tagNameSpan = <HTMLElement>tagRow.getElementsByClassName(TagClassMap.tagName)[0];

            try{
                let oldTagObj: any = JSON.parse(tagNameSpan.getAttribute('data-tag'));
                oldTagObj[TagService.State] = EntityState.Deleted;
                let sNewTag: string = JSON.stringify(oldTagObj);
                this.m_tagService.addTag(sNewTag);
            }
            catch(e){
                LogHelper.log(`tagDeleteClickHandler failed, error: ${e.message}`);
            }

            tagListBody.removeChild(tagRow);
            this.cancelBtnClickHandler(undefined);
        }
        else{
            LogHelper.log(`tagDeleteClickHandler failed, tagListBody: ${tagListBody}, tagRow: ${tagRow}`);
        }

        evt.stopPropagation();//stop from bubbling to the tag-row
    }

    private tagRowClickHandler(this: Classification, evt: Event): any{
        if(!evt){
            return false;
        }

        let scope: Classification = this;
        let curTarget: HTMLElement = <HTMLElement>evt.target;
        let parentTarget: HTMLElement = curTarget ? curTarget.parentElement : undefined;
        const applyBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(TagIdMap.tagApplyBtn);

        if(parentTarget && curTarget.classList.contains(TagClassMap.tagName) && applyBtn){
            //change visual state to click
            applyBtn.disabled = false;
            scope.eliminateClickState.call(scope);
            parentTarget.classList.add(TagClassMap.tagListItemClick);

            //get data in data-tag attribute
            let tagNameSpan: HTMLElement = <HTMLElement>parentTarget.getElementsByClassName(TagClassMap.tagName)[0];
            if(tagNameSpan){
                let sTag: string = tagNameSpan.getAttribute('data-tag');
                if(sTag){
                    let tagObj: any = JSON.parse(sTag);
                    if(tagObj){
                        scope.displayTagDetail(tagObj);
                        scope.displayClassificationView();
                    }
                    else{
                        LogHelper.log(`tagRowClickHandler failed, tagObject: ${tagObj}`);
                    }
                }
                else{
                    LogHelper.log(`tagRowClickHandler failed, tag in json: ${sTag}`);
                }
            }
            else{
                LogHelper.log(`tagRowClickHandler failed, tagNameSpan: ${tagNameSpan}`);
            }
        }
        else{
            LogHelper.log(`tagRowClickHandler failed, target: ${curTarget}`);
        }

        evt.stopPropagation();
    }

    private addNewBtnClickHandlerEx(this: Classification, evt: Event): void{
        let scope: Classification = this;
        scope.eliminateClickState.call(scope);
        scope.hideClassificationView();
        let tagdefault = scope.constructEmptyTag();
        scope.displayTagDetail(tagdefault);
        scope.displayClassificationView();

        /*
        let curNewTagSpan: HTMLElement = scope.getCurNewClassificationTagSpan();
        if (scope.isValid(curNewTagSpan)){
            let tagClickEvt: Event = Util.createEvent('click');
            curNewTagSpan.dispatchEvent(tagClickEvt);
            scope.displayClassificationView();
        }
        else{
            scope.addNewBtnClickHandler(evt);
        }
        */
    }

    private addNewBtnClickHandler(this: Classification, evt: Event): void{

        let scope: Classification = this;
        let emptyTag: any = this.constructEmptyTag();
        let tagElement: HTMLDivElement = this.createTagRow(emptyTag);
        let tagListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagListBody)[0];
        let tagNameSpan: HTMLElement = undefined;

        if(tagListBody && tagElement){
            tagElement.classList.add(TagClassMap.tagListItemClick);
            let tagNameSpan = <HTMLElement>tagElement.getElementsByClassName(TagClassMap.tagName)[0];
            let tagClickEvt: Event = Util.createEvent('click');
            tagListBody.appendChild(tagElement);
            tagNameSpan.dispatchEvent(tagClickEvt);
            scope.displayClassificationView();
        }
        else{
            LogHelper.log(`addNewBtnClickHandler failed, tagListBody: ${tagListBody}, tagElement: ${tagElement}`);
        }
    }

    private okBtnClickHandler(this: Classification, evt: Event): void{   
        let scope: Classification = this;
        let curTagSpan: HTMLElement = undefined;

        // Get user input info
        let tagNameBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagNameBox);
        let tagValuesBox: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagValuesBox);
        let defaultValueSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagDefaultSelect);
        let editableSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(TagIdMap.tagEditableSelect);
        let emptyTag: any = scope.constructEmptyTag();
        if(tagNameBox && tagValuesBox && defaultValueSelect && editableSelect && emptyTag){

            if(this.checkClassificationNameValidation(tagNameBox.value)){
                document.getElementById('input-check-msg').classList.add('hide');
            }
            else{
                document.getElementById('input-check-msg').classList.remove('hide');
                return;
            }

            emptyTag[TagService.TAG_NAME] = HtmlHelper.htmlEncode(tagNameBox.value).toLowerCase().trim();
            emptyTag[TagService.TAG_VALUE] = HtmlHelper.htmlEncode(tagValuesBox.value).toLowerCase().trim();
            let nDefaultValueIndex = defaultValueSelect.selectedIndex; // -1, means unselected
            emptyTag[TagService.DEFAULT_VALUE] = (0 <= nDefaultValueIndex) ? HtmlHelper.htmlEncode((<HTMLOptionElement>defaultValueSelect.options[defaultValueSelect.selectedIndex]).value).toLowerCase().trim() : "";
            emptyTag[TagService.EDITABLE] = (<HTMLOptionElement>editableSelect.options[editableSelect.selectedIndex]).value.toLowerCase().trim();
            emptyTag[TagService.State] = EntityState.Added;
            // Check input information
            if (0 === emptyTag[TagService.TAG_NAME].length){
                scope.showAlertMessage("Tag name canot be empty");
                return ;
            }

            // Get current list item, if it is new, create one
            let jsonOldTagInfo: any = undefined;
            let bModified: boolean = false;
            let curTag: HTMLElement = scope.getCurSelectedClassificationTag();
            if (scope.isValid(curTag)){
                curTagSpan = <HTMLElement>curTag.getElementsByClassName(TagClassMap.tagName)[0];
                if (0 !== curTagSpan.textContent.localeCompare(emptyTag[TagService.TAG_NAME])){
                    bModified = true;
                    if (scope.isExistTagName(emptyTag[TagService.TAG_NAME])){
                        // Tag name changed, but new tag name already exist.
                        scope.showAlertMessage("Tag name:[" + emptyTag[TagService.TAG_NAME] + "] already exist");
                        return ;
                    }
                    // Get old tag info
                    try {
                        jsonOldTagInfo = JSON.parse(curTagSpan.getAttribute('data-tag'));
                    } catch (e) {
                        jsonOldTagInfo = scope.constructEmptyTag();
                        jsonOldTagInfo[TagService.TAG_NAME] = curTagSpan.textContent;
                        LogHelper.log(`okBtnClickHandler failed, analysis old tag info, error: ${e.message}`);
                    }
                }
            }else{
                if (scope.isExistTagName(emptyTag[TagService.TAG_NAME])){
                    scope.showAlertMessage("Tag name:[" + emptyTag[TagService.TAG_NAME] + "] already exist");
                    return ;
                }else{
                    let tagElement: HTMLDivElement = this.createTagRow(emptyTag);
                    tagElement.classList.add(TagClassMap.tagListItemClick);
                    curTagSpan = <HTMLElement>tagElement.getElementsByClassName(TagClassMap.tagName)[0];
                    let tagListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagListBody)[0];
                    tagListBody.appendChild(tagElement); 
                }
            }
            
            // Apply user input info
            try{
                // Add current tag info, if it is modify, add new and remove old
                this.m_tagService.addTag(JSON.stringify(emptyTag));
                emptyTag[TagService.State] = EntityState.Unchanged;
                if(jsonOldTagInfo && bModified){
                    jsonOldTagInfo[TagService.State] = EntityState.Deleted;
                    this.m_tagService.addTag(JSON.stringify(jsonOldTagInfo));
                }
                //scope.hideClassificationView();
            }
            catch(e){
                LogHelper.log(`okBtnClickHandler failed, error: ${e.message}`);
                emptyTag[TagService.State] = EntityState.Unchanged;
            }

            curTagSpan.setAttribute('data-tag', JSON.stringify(emptyTag));
            curTagSpan.textContent = HtmlHelper.htmlDecode(emptyTag[TagService.TAG_NAME]);
            //this.clearTagDetails();
        }
        else{
            LogHelper.log(`okBtnClickHandler failed, tagNameBox: ${tagNameBox}, tagValuesBox: ${tagValuesBox}, defaultValueSelect: ${defaultValueSelect}, editableSelect: ${editableSelect}, curTagSpan: ${curTagSpan}`);
        }

    }

    private cancelBtnClickHandler(this: Classification, evt: Event): void{

        let scope: Classification = this;

        let tagListBodyEl: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagListBody)[0];
        let curTagEl: HTMLElement = tagListBodyEl ? <HTMLElement>document.getElementsByClassName(TagClassMap.tagListItemClick)[0] : undefined;//alert(`curtag: ${curTag}`);
        let curNameSpanEl: HTMLElement = curTagEl ? <HTMLElement>curTagEl.getElementsByClassName(TagClassMap.tagName)[0] : undefined;//alert(`curspan: ${curSpan}`);

        if(tagListBodyEl && curTagEl){
            if(!curNameSpanEl || !curNameSpanEl.textContent){
                tagListBodyEl.removeChild(curTagEl);
            }
        }

        scope.clearTagDetails();
        scope.eliminateClickState.call(this);
        scope.hideClassificationView();
    }

    private applyBtnClickHandler(this: Classification, evt: Event): void{
        let scope: Classification = this;
        const applyBtn: HTMLButtonElement = <HTMLButtonElement>evt.target;
        applyBtn.disabled = true;
        return scope.okBtnClickHandler(evt);
    }

    private saveBtnClickHandler(this: Classification, evt: Event): void{

        this.m_tagService.saveChanges();
    }

    private defaultSelectMouseEnterHandler(this: Classification, evt: MouseEvent): void{
        
        let defaultValueSelectEl: HTMLSelectElement = evt ? <HTMLSelectElement>evt.currentTarget : undefined;
        let valuesInputEl: HTMLInputElement = <HTMLInputElement>document.getElementById(TagIdMap.tagValuesBox);

        if(defaultValueSelectEl && valuesInputEl){
            let nOldSelected = defaultValueSelectEl.selectedIndex; // -1, means unselected
            let strOldSelectedValue = (0 <= nOldSelected) ? HtmlHelper.htmlEncode((<HTMLOptionElement>defaultValueSelectEl.options[nOldSelected]).value).toLowerCase().trim() : "";

            let nNewSelectedIndex = 0;  // default, using the first one
            let nIndex = -1;
            defaultValueSelectEl.textContent = '';  // clean old content
            let inputValues: string = valuesInputEl.value.trim();
            Util.forEach(inputValues.split('|'), function(value: string){
                if(value && value.trim()){
                    let option: HTMLOptionElement = document.createElement('option');
                    option.value = value.trim();
                    option.textContent = value;
                    option.className = TagClassMap.tagDetailValue;
                    defaultValueSelectEl.appendChild(option);
                    ++nIndex;
                    if (0 === strOldSelectedValue.localeCompare(option.value.toLowerCase())){
                        nNewSelectedIndex = nIndex;
                    }
                }
            }, this);
            if (0 === defaultValueSelectEl.options.length){
                defaultValueSelectEl.selectedIndex = -1;
            }else{
                defaultValueSelectEl.selectedIndex = nNewSelectedIndex;
            }
        }
        else{
            LogHelper.log(`defaultSelectMouseEnterHandler failed, defalut selector: ${defaultValueSelectEl}, value input element: ${valuesInputEl}`);
        }
    }

    /*** Tools */
    private showAlertMessage(this: Classification, strAlertMessage: string){
        alert(strAlertMessage)
    }
    private getCurSelectedClassificationTag(this: Classification): HTMLElement{
        let curClassificationTag: HTMLElement = undefined;
        let tagListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagListBody)[0];     
        curClassificationTag = tagListBody ? <HTMLElement>tagListBody.getElementsByClassName(TagClassMap.tagListItemClick)[0] : undefined;
        return curClassificationTag
    }

    private isExistTagName(this: Classification, strInTagName: string): boolean{
        let scope: Classification = this;
        let bExist: boolean = false;

        let strInLowercaseTagName = strInTagName.toLowerCase();

        let tagListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagListBody)[0];
        let tagElementList: HTMLCollectionOf<Element> = <HTMLCollectionOf<Element>>tagListBody.getElementsByClassName(TagClassMap.tagListItem);

        let tagElement: HTMLElement = null;
        let tagSpan: HTMLElement = null;
        let strCurTagName: string = "";
        for (let i:number = 0; i<tagElementList.length; ++i) {
            tagElement = <HTMLElement>tagElementList[i];
            tagSpan = tagElement ? <HTMLElement>tagElement.getElementsByClassName(TagClassMap.tagName)[0] : undefined;
            if (scope.isValid(tagSpan)){
                strCurTagName = tagSpan.textContent.trim();   
                if (0 === strCurTagName.toLowerCase().localeCompare(strInLowercaseTagName)){
                    bExist = true;
                    break;
                }
            }
            tagElement = null;
            tagSpan = null;
            strCurTagName = "";
        }
        return bExist;
    }

    private getCurNewClassificationTagSpan(this: Classification): HTMLElement{
        let scope: Classification = this;

        let tagListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(TagClassMap.tagListBody)[0];
        let tagElementList: HTMLCollectionOf<Element> = <HTMLCollectionOf<Element>>tagListBody.getElementsByClassName(TagClassMap.tagListItem);

        let tagElement: HTMLElement = null;
        let tagSpan: HTMLElement
        for (let i:number = 0; i<tagElementList.length; ++i) {
            tagElement = <HTMLElement>tagElementList[i];
            tagSpan = tagElement ? <HTMLElement>tagElement.getElementsByClassName(TagClassMap.tagName)[0] : undefined;
            if (scope.isValid(tagSpan)){
                let strTagName: string = tagSpan.textContent.trim();   
                if (0 === strTagName.length){
                    break;
                }
            }
            tagElement = null;
            tagSpan = null;
        }
        return tagSpan;
    }

    private isValid(this: Classification, obIn: any): boolean
    {
        return ((null !== obIn) && (undefined !== obIn)); // !obIn: null, undefined, o, false, "", ... logic false
    }

    private checkClassificationNameValidation(name: string): boolean{

        let isValid = true;

        if(typeof name !== 'string' || name.length > 255){
            isValid = false;
        }

        return isValid;
    }

    private valueOnchangeHandler(this:Classification, evt: Event): void {
        const applyBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(TagIdMap.tagApplyBtn);
        applyBtn.disabled = false;        
    }
}

//entry point, called from c#
//must be attached to window in case that it can't be accessed in closure
(<any>window).init = function(){
    try{
        let main: Classification = new Classification(false);    
        main.initData();
    }
    catch(e){
        alert(`initData failed, error: ${e.message}`);
    }
    finally{
        let maskEl: HTMLElement = <HTMLElement>document.getElementsByClassName('mask')[0];
        if(maskEl){
            maskEl.style.display = 'none';
        }
    }
};

(<any>window).NLDebug = function(){
    let main: Classification = new Classification(true); 
}
