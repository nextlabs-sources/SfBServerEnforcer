import { SchemaService } from './schemaservice';
import { TagService } from './tagservice';
import { SchemaClassMap, SchemaIdMap, EntityState } from './uiresource';
import { LogHelper, HtmlHelper, TagHelper, XmlHelper } from '../../../../jsbin/Modules/helper';
import { Util } from '../../../../jsbin/Modules/util';
import { TreeView } from '../../../../jsbin/Modules/treeview';
import { ServiceProvider } from "./serviceprovider";
import { LinkageMenu, IDataSourceFormat, XmlDataSourceFormat, IStyleModify } from '../../../../jsbin/Modules/linakge-menu';
import { HierarchicalStructureStyleModify } from './schemapreviewstyle';

class Schema{
    //members
    private m_serviceProvider: ServiceProvider = undefined;
    private m_curTreeview: TreeView = undefined;
    private m_curInsertTreeTextEl: HTMLElement = undefined;
    private m_curModifyTreeTextEl: HTMLElement = undefined;

    //static members
    static TREE_VIEW: string = '0';
    static CREATE_VIEW: string = '1';
    static NONE_VIEW: string = '-1';
    static INSERT_ROOT_TITLE_MSG = 'Add classification element(s)';
    static INSERT_ELEMENT_TITLE_MSG = 'Add classification element(s) under';
    static DATA_SCHEMA_ATTR = 'data-schema';

    //.ctor
    constructor(){
        this.m_serviceProvider = new ServiceProvider();
        //this.initData.call(this);
    }

    public initData(): void{
        try{
            let sSchemas: string = this.m_serviceProvider.getSchemasInJSON();
            if(sSchemas != null && sSchemas != undefined){
                this.displaySchemas(sSchemas);
                this.addEventListeners();
            }
            else{
                throw Error('schemas in json are null');
            }
        }
        catch(e){
            alert(`initData failed, name: ${e.name}, message: ${e.message}.`);
        }
    }

    //private methods
    private addEventListeners(): void{

        let scope: Schema = this;

        scope.addEventListenersForSchemaTitleSection.call(scope);
        scope.addEventListenersForSchemaCreateSection.call(scope);
        scope.addEventListenersForSchemaFooterSection.call(scope);
        scope.addEventListenersForInsertSection.call(scope);
        scope.addEventListenersForModifySection.call(scope);
    }

    private addEventListenersForModifySection(): void{

        let scope: Schema = this;
        let modifyPromptEl: HTMLElement = document.getElementById(SchemaIdMap.modifyPrompt); 
        let tagSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.tagSelector);
        let defaultValueSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.defalutValueSelector);
        let relyOnSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.relyOnSelector);        
        let modifySaveBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(SchemaIdMap.modifySaveBtn);
        let modifyCancelBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(SchemaIdMap.modifyCancelBtn);
        
        tagSelectEl.addEventListener('change', function(evt: Event): any{

            let selectedOption: HTMLOptionElement = <HTMLOptionElement>tagSelectEl.options[tagSelectEl.selectedIndex];
            let selectedPureTagJSON: string = selectedOption ? selectedOption.value : '';
            let selectedPureTagObj: any = undefined;
            let selectedTagValues: string = '';
            let selectedTagValueArray: string [] = undefined;
            let selectedDefaultValue: string = '';

            defaultValueSelectEl.textContent = '';

            if(selectedPureTagJSON){
                try{
                    selectedPureTagObj = JSON.parse(selectedPureTagJSON);
                    selectedTagValues = selectedPureTagObj ? selectedPureTagObj[TagService.TAG_VALUE] : '';
                    selectedTagValueArray = selectedTagValues ? selectedTagValues.split('|') : undefined;
                    selectedDefaultValue = selectedPureTagObj ? selectedPureTagObj[TagService.DEFAULT_VALUE] : '';

                    if(selectedTagValueArray && selectedTagValueArray.length > 0){
                        
                        scope.renderValuesOptions(defaultValueSelectEl, selectedTagValueArray);
                        
                        if(selectedDefaultValue){
                            scope.matchTagPropertyWithOption(TagService.DEFAULT_VALUE, selectedDefaultValue, defaultValueSelectEl);
                        }
                        else{
                            defaultValueSelectEl.selectedIndex = defaultValueSelectEl.options.length - 1;                            
                        }

                        defaultValueSelectEl.disabled = false;
                    }
                    else{
                        defaultValueSelectEl.disabled = true;
                    }
                }
                catch(e){
                    alert(`addEventListenersForModifySection failed, error: ${e.message}`);
                }
            }
            else{
                alert(`addEventListenersForModifySection failed, tag in json: ${selectedPureTagJSON}`);
            }

        }, false);

        modifySaveBtn.addEventListener('click', function(evt: Event): any{
            
            let curModifyTreeTextEl: HTMLElement = scope.m_curModifyTreeTextEl;
            let selectedOption: HTMLOptionElement = <HTMLOptionElement>tagSelectEl.options[tagSelectEl.selectedIndex];
            let selectedPureTagJSON: string = selectedOption ? selectedOption.value : '';
            let selectedPureTagObj: any = undefined;
            let selectedRelyOnValues: string = scope.getRelyOnValues(relyOnSelectEl);
            let selectedDefalutValue: string = defaultValueSelectEl.disabled ? '' : (<HTMLOptionElement>defaultValueSelectEl.options[defaultValueSelectEl.selectedIndex]).value;

            if(selectedPureTagJSON){
                try{
                    selectedPureTagObj = JSON.parse(selectedPureTagJSON);
                    selectedPureTagObj[TagService.DEFAULT_VALUE] = selectedDefalutValue;
                    selectedPureTagObj[TagService.RELY_ON] = selectedRelyOnValues;

                    let tagBeSavedJSON: string = JSON.stringify(selectedPureTagObj);//alert(tagBeSavedJSON);
                    curModifyTreeTextEl.setAttribute('data-item', tagBeSavedJSON);
                    scope.saveSchemaData();
                }   
                catch(e){
                    alert(`save modified tag failed, error: ${e.message}`);
                }
            }
            else{
                //save failed
            }

            modifyPromptEl.classList.remove('show');

        }, false);

        modifyCancelBtn.addEventListener('click', function(evt: Event): any{
            modifyPromptEl.classList.remove('show');
            scope.clearSchemaModifySection();
        }, false);
    }

    private addEventListenersForInsertSection(): void{

        let scope: Schema = this;
        let multiInsertBtn: HTMLElement = document.getElementById(SchemaIdMap.addRow);
        let insertSaveBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(SchemaIdMap.insertSaveBtn);
        let insertCancelBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(SchemaIdMap.insertCancelBtn);

        multiInsertBtn.addEventListener('click', function(evt: Event): any{
            scope.multiInsertClickHandler.call(scope, evt);
        }, false);

        insertSaveBtn.addEventListener('click', function(evt: Event): any{
            scope.insertSaveClickHandler.call(scope, evt);
        }, false);

        insertCancelBtn.addEventListener('click', function(evt: Event): any{
            scope.insertCancelClickHandler.call(scope, evt);
        }, false);

    }

    private addEventListenersForSchemaTitleSection(): void{

        let scope: Schema = this;
        let addNewTagBtn: HTMLElement = document.getElementById(SchemaIdMap.addNewTagBtn);
        let delAllTagsBtn: HTMLElement = document.getElementById(SchemaIdMap.delAllTagsBtn);
        let modifySchemaBtn: HTMLElement = document.getElementById(SchemaIdMap.modifySchemaBtn);

        addNewTagBtn.addEventListener('click', function(evt: Event){
            
            let insertPrompt: HTMLElement = document.getElementById(SchemaIdMap.insertPrompt);
            let insertContent: HTMLElement = insertPrompt ? <HTMLElement>insertPrompt.getElementsByClassName(SchemaClassMap.schemaInsertContent)[0] : undefined;
            let insertTitle: HTMLElement = insertPrompt ? <HTMLElement>insertPrompt.getElementsByClassName(SchemaClassMap.schemaInsertTitle)[0] : undefined;
            let addRow: HTMLElement = document.getElementById(SchemaIdMap.addRow);

            if(insertContent && addRow && insertTitle){

                scope.m_curInsertTreeTextEl = undefined;
                insertTitle.textContent = `${Schema.INSERT_ROOT_TITLE_MSG}`;
                insertTitle.setAttribute('data-item', '');

                insertPrompt.classList.add('show');
                let newRow: HTMLElement = scope.createTagRow();
                insertContent.insertBefore(newRow, addRow);
            }            

        }, false);

        delAllTagsBtn.addEventListener('click', function(evt: Event){
            let treeViewContainer: HTMLElement = document.getElementById(SchemaIdMap.treeviewContainer);
            if(treeViewContainer){
                treeViewContainer.textContent = '';
            }
            else{
                LogHelper.log(`delete all tags failed`);
            }
        }, false);

        modifySchemaBtn.addEventListener('click', function(evt: Event){
            
            let curSelectSchemaEl: HTMLElement = scope.getCurSelectedSchemaElement();
            let schemaNameSpan: HTMLElement = curSelectSchemaEl ? <HTMLElement>curSelectSchemaEl.getElementsByClassName('schema-name')[0] : undefined;
            let schemaNameInputEl: HTMLInputElement = <HTMLInputElement>document.getElementById(SchemaIdMap.schemaNameInput);
            let schemaDescInputEl: HTMLTextAreaElement = <HTMLTextAreaElement>document.getElementById(SchemaIdMap.schemaDescText);

            scope.clearSchemaCreateSection();
            scope.displayView(Schema.CREATE_VIEW);

            if(schemaNameSpan && schemaNameInputEl && schemaDescInputEl){
                try{
                    let schemaObj: any = JSON.parse(schemaNameSpan.getAttribute('data-schema'));
                    schemaNameInputEl.value = HtmlHelper.htmlDecode(schemaObj[SchemaService.SCHEMA_NAME]);
                    schemaNameInputEl.disabled = true;
                    schemaDescInputEl.value = HtmlHelper.htmlDecode(schemaObj[SchemaService.SCHEMA_DESCRIPTION]);
                }
                catch(e){
                    alert(`modifySchemaBtn -> parse schema json failed, error: ${e.message}`);
                }
            }
            else{
                LogHelper.log(`modifySchemaBtn -> click handler failed, schemaNameSpan: ${schemaNameSpan}, schemaNameInputEl: ${schemaNameInputEl}, schemaDescInputEl: ${schemaDescInputEl}`);
            }

        }, false);
    }

    private addEventListenersForSchemaFooterSection(): void{

        let scope: Schema = this;
        let schemaSaveBtn: HTMLElement = document.getElementById(SchemaIdMap.schemaSaveBtn);
        let exportBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(SchemaIdMap.schemaExportBtn);
        let schemaPreviewBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(SchemaIdMap.schemaPreviewBtn);
        let schemaPreviewCancelBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(SchemaIdMap.schemaPreviewCancelBtn);

        schemaSaveBtn.addEventListener('click', function(evt: Event){
            scope.saveClickHandler.call(scope, evt);
        }, false);

        exportBtn.addEventListener('click', function(evt: Event): any{
            scope.exportClickHandler.call(scope, evt);
        }, false);

        schemaPreviewBtn.addEventListener('click', function(evt: Event): any{
            scope.previewClickHandler.call(scope, evt);
        }, false);

        schemaPreviewCancelBtn.addEventListener('click', function(evt: Event): any{
            scope.previewCancelClickHandler.call(scope, evt);
        }, false);
    }

    private addEventListenersForSchemaCreateSection(): void{

        let scope: Schema = this;
        let schemaAddNewBtn: HTMLElement = document.getElementById(SchemaIdMap.schemaAddNewBtn);
        let schemaOkBtn: HTMLElement = document.getElementById(SchemaIdMap.schemaOkBtn);
        let schemaCancelBtn: HTMLElement = document.getElementById(SchemaIdMap.schemaCancelBtn);
        let schemaApplyBtn: HTMLElement = document.getElementById(SchemaIdMap.schemaApplyBtn);
        let schemaNameInputEl: HTMLInputElement = <HTMLInputElement>document.getElementById(SchemaIdMap.schemaNameInput);

        schemaOkBtn.addEventListener('click', function(evt: Event){
            scope.okClickHandler.call(scope, evt);
        }, false);

        schemaCancelBtn.addEventListener('click', function(evt: Event){
            scope.cancelClickHandler.call(scope, evt);
        }, false);

        schemaApplyBtn.addEventListener('click', function(evt: Event){
            scope.applyClickHandler.call(scope, evt);
        }, false);

        schemaAddNewBtn.addEventListener('click', function(evt: Event){
            scope.addNewSchemaClickHandlerEx.call(scope, evt);
        }, false);

        schemaNameInputEl.addEventListener('blur', function(evt: Event){
            scope.schemaNameInputBlurHandlerClick.call(scope, evt);
            evt.stopPropagation();
        }, false);
    }

    private displaySchemas(sSchemas: string): void{
        let schemaArray: Array<any> = <Array<any>>JSON.parse(sSchemas);
        let schemaListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaListBody)[0];

        for(let i: number = 0; i < schemaArray.length; i++){
            let schemaRow: HTMLElement = this.createSchemaRow(schemaArray[i]);
            if(schemaRow){
                schemaListBody.appendChild(schemaRow);
            }
        }
    }

    private createSchemaRow(schemaObj: any): HTMLDivElement{

        let rowEl: HTMLDivElement = document.createElement('div');
        let iconWrapper: HTMLDivElement = document.createElement('div');
        let schemaEl: HTMLSpanElement = document.createElement('span');
        let crossEl: HTMLSpanElement = document.createElement('span');

        rowEl.classList.add('schema-list-item');
        iconWrapper.classList.add('schema-delete');
        schemaEl.classList.add('schema-name');
        crossEl.classList.add('red-cross');

        schemaEl.textContent = HtmlHelper.htmlDecode(schemaObj[SchemaService.SCHEMA_NAME]);
        schemaEl.setAttribute('data-schema', JSON.stringify(schemaObj));
        iconWrapper.appendChild(crossEl);
        rowEl.appendChild(schemaEl);
        rowEl.appendChild(iconWrapper);

        let scope: Schema = this;
        //add event handler for red-cross click
        crossEl.addEventListener('click', function(evt: Event){
            scope.schemaDeleteClickHandler.call(scope, evt);
        }, false);

        //add event handler for row click
        rowEl.addEventListener('click', function(evt){
            scope.schemaRowClickHandler.call(scope, evt);
        }, false);

        return rowEl;
    }

    private constructEmptySchema(): any{
        let schemaObj: any = {};
        schemaObj[SchemaService.SCHEMA_NAME] = '';
        schemaObj[SchemaService.SCHEMA_DATA] = '';
        schemaObj[SchemaService.STATE] = EntityState.Added;
        return schemaObj;
    }

    private displayView(opType: string): void{

        let schemaPanelBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaPanelBody)[0];
        let schemaPanelContentEls: HTMLCollectionOf<HTMLElement> = schemaPanelBody ? <HTMLCollectionOf<HTMLElement>>schemaPanelBody.getElementsByClassName(SchemaClassMap.schemaPanelContent) : undefined;

        Util.forEach(schemaPanelContentEls, function(el: HTMLElement){
            let operationType: string = el.getAttribute('data-view');
            if(operationType && operationType === opType){
                el.classList.add(SchemaClassMap.curView);//if display: none somewhere has higher priority than curView style, nothing displays
            }
            else if(operationType){
                el.classList.remove(SchemaClassMap.curView);
            }
        }, this);
    }

    private eliminateClickState(this: Schema): void{
        let schemaListBodyEl: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaListBody)[0];
        let schemaCurItemEls: HTMLElement = schemaListBodyEl ? <HTMLElement>schemaListBodyEl.getElementsByClassName(SchemaClassMap.schemaListItemClick)[0] : undefined;

        if(schemaCurItemEls){
            schemaCurItemEls.classList.remove(SchemaClassMap.schemaListItemClick);
        }
    }

    private displayTreeViewSection(schemaObj: any): void{
        let scope: Schema = this;
        let treeviewContainer: HTMLElement = <HTMLElement>document.getElementById(SchemaIdMap.treeviewContainer);       
        
        try{
            let treeview: TreeView = new TreeView(schemaObj[SchemaService.SCHEMA_DATA], treeviewContainer, true);

            scope.m_curTreeview = treeview;

            treeview.on('add', function(sender: HTMLElement, msg: string){
                scope.treeviewAddClickHandler.call(scope, sender, msg);
            });
            treeview.on('delete', function(sender: HTMLElement, msg: string){
                scope.treeviewDeleteClickHandler.call(scope, sender, msg);
            });
            treeview.on('modify', function(sender: HTMLElement, msg: string){
                scope.treeviewModifyClickHandler.call(scope, sender, msg);
            });
            treeview.on('select', function(sender: HTMLElement, msg: string){
                //alert('select: ' + sender.getAttribute('data-item'));
            });
        }
        catch(e){
            alert(`treeview init failed, error: ${e.message}`);
            LogHelper.log(`displayTreeViewSection failed, error: ${e.message}`);
        }

        this.displayView(Schema.TREE_VIEW);
    }

    private clearTreeViewSection(): void{
        
        let scope: Schema = this;
        let schemaNameSpan: HTMLElement = document.getElementById(SchemaIdMap.schemaNameSpan);
        let schemaDescEl: HTMLElement = document.getElementById(SchemaIdMap.schemaDescPara);
        let schemaTreeviewContainer: HTMLElement = document.getElementById(SchemaIdMap.treeviewContainer);

        scope.m_curTreeview = undefined;

        if(schemaNameSpan && schemaTreeviewContainer && schemaDescEl){
            schemaNameSpan.textContent = '';
            schemaDescEl.textContent = '';
            schemaTreeviewContainer.textContent = '';
        }
        else{
            LogHelper.log(`clearTreeViewSection failed, schema name span: ${schemaNameSpan}, schema container: ${schemaTreeviewContainer}`);
        }
    }

    private clearSchemaCreateSection(): void{
        
        let schemaNameInput: HTMLInputElement = <HTMLInputElement>document.getElementById(SchemaIdMap.schemaNameInput);
        let schemaDescInput: HTMLTextAreaElement = <HTMLTextAreaElement>document.getElementById(SchemaIdMap.schemaDescText);

        if(schemaNameInput && schemaDescInput){
            schemaNameInput.value = '';
            schemaNameInput.disabled = false;
            schemaDescInput.value = '';
        }
        else{
            LogHelper.log(`clearSchemaCreateSection failed`);
        }
    }

    private clearSchemaInsertSection(): void{

        let scope: Schema = this;
        let insertPromptEl: HTMLElement = document.getElementById(SchemaIdMap.insertPrompt);
        let insertContentEl: HTMLElement = insertPromptEl ? <HTMLElement>insertPromptEl.getElementsByClassName(SchemaClassMap.schemaInsertContent)[0] : undefined;
        let insertTitleEl: HTMLElement = insertPromptEl ? <HTMLElement>insertPromptEl.getElementsByClassName(SchemaClassMap.schemaInsertTitle)[0] : undefined;
        let contentRowEls: HTMLCollectionOf<HTMLElement> = insertContentEl ? <HTMLCollectionOf<HTMLElement>>insertContentEl.getElementsByClassName('row') : undefined;

        if(insertTitleEl){
            insertTitleEl.textContent = '';
            insertTitleEl.setAttribute('data-item', '');
        }

        if(contentRowEls && contentRowEls.length > 0){
            let curEl: Element = contentRowEls[0];
            while(curEl){
                let nextSibNode: Element = <Element>curEl.nextElementSibling;
                if(!curEl.getAttribute('id')){
                    insertContentEl.removeChild(curEl);
                }
                curEl = nextSibNode;
            }
        }
    }

    private clearSchemaModifySection(): void{

        let tagSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.tagSelector);
        let defaultValueSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.defalutValueSelector);
        let parentTagInputEl: HTMLInputElement = <HTMLInputElement>document.getElementById(SchemaIdMap.parentTagInput);
        let relyOnSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.relyOnSelector);        

        tagSelectEl.textContent = '';
        defaultValueSelectEl.textContent = '';
        parentTagInputEl.value = '';
        parentTagInputEl.disabled = false;
        relyOnSelectEl.textContent = '';
        relyOnSelectEl.disabled = false;
    }

    private deleteCurSchemaInList(): void{
        let schemaListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaListBody)[0];
        let curSchemaEl: HTMLElement = schemaListBody ? <HTMLElement>schemaListBody.getElementsByClassName(SchemaClassMap.schemaListItemClick)[0] : undefined;
        let curSchemaNameEl: HTMLElement = curSchemaEl ? <HTMLElement>curSchemaEl.getElementsByClassName('schema-name')[0] : undefined;

        if(!curSchemaNameEl || !curSchemaEl.textContent.trim()){
            schemaListBody.removeChild(curSchemaEl);
        }
    }

    private getCurViewElement(): HTMLElement{
        
        let curEl: HTMLElement = undefined;
        let schemaPanelBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaPanelBody)[0];
        let schemaPanelContentEls: HTMLCollectionOf<HTMLElement> = schemaPanelBody ? <HTMLCollectionOf<HTMLElement>>schemaPanelBody.getElementsByClassName(SchemaClassMap.schemaPanelContent) : undefined;

        Util.forEach(schemaPanelContentEls, function(el: HTMLElement): void{
            if(el && el.classList.contains(SchemaClassMap.curView)){
                curEl = el;
                return;
            }
        }, this);

        return curEl;
    }

    private getCurSelectedSchemaElement(): HTMLElement{
        let curSchemaEl: HTMLElement = undefined;
        let schemaListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaListBody)[0];
        
        curSchemaEl = schemaListBody ? <HTMLElement>schemaListBody.getElementsByClassName(SchemaClassMap.schemaListItemClick)[0] : undefined;

        return curSchemaEl;
    }

    private matchTagNameOption(target: any, selectEl: HTMLSelectElement): boolean{
        let isMatch: boolean = false;
        let options: HTMLOptionsCollection = selectEl.options;

        for(let i: number = 0; i < options.length; i++){
            let targetTagName: string = target[TagService.TAG_NAME];
            let optionTagName: string = options[i].textContent;
            if(targetTagName === optionTagName){
                isMatch = true;
                selectEl.selectedIndex = i;
                let changeEvt: Event = Util.createEvent('change');
                selectEl.dispatchEvent(changeEvt);
                break;
            }
        }

        if(!isMatch){
            let newOption: HTMLOptionElement = document.createElement('option');
            newOption.textContent = target[TagService.TAG_NAME];
            newOption.value = JSON.stringify(target);
            selectEl.appendChild(newOption);
            selectEl.selectedIndex = selectEl.options.length - 1;
            let changeEvt: Event = Util.createEvent('change');
            selectEl.dispatchEvent(changeEvt);
        }

        return isMatch;
    }

    private matchTagMultiSelectWithOption(szSelectedValue: string[], selectEl: HTMLSelectElement): boolean{
        if(!selectEl){
            return false;
        }

        let options: HTMLOptionsCollection = selectEl.options;
        try{
            for (let i: number = 0; i < szSelectedValue.length; ++i){
                let strCurSelectValue: string = szSelectedValue[i].trim().toLowerCase();
                for(let j: number = 0; j < options.length; ++j){
                    let strCurValue: string = (<HTMLOptionElement>options[j]).value.trim().toLowerCase();                                    
                    if(strCurValue === strCurSelectValue){
                        options[j].selected = true;
                        break;
                    }
                }
            }
        }
        catch(e){
            alert(`matchTagPropertyWithOption -> parse tag json failed, error: ${e.message}`);
        }
        return true;
    }

    private matchTagPropertyWithOption(propName: string, propValue: any, selectEl: HTMLSelectElement): boolean{
        
        if(!selectEl){
            return false;
        }
        
        let isMatch: boolean = false;
        let options: HTMLOptionsCollection = selectEl.options;

        try{
            for(let i: number = 0; i < options.length; i++){
                let curValue: string = (<HTMLOptionElement>options[i]).value;
                if(curValue === propValue){
                    isMatch = true;
                    selectEl.selectedIndex = i;
                    break;
                }
            }
        }
        catch(e){
            alert(`matchTagPropertyWithOption -> parse tag json failed, error: ${e.message}`);
        }

        if(!isMatch){
            let newOption: HTMLOptionElement = document.createElement('option');
            newOption.textContent = propValue;
            newOption.value = propValue;
            selectEl.appendChild(newOption);
            selectEl.selectedIndex = selectEl.options.length - 1;
        }

        return isMatch;
    }

    private renderTagNameOptions(selectEl: HTMLSelectElement, tagsArray: any[]): void{
        if(!selectEl || !tagsArray){
            return;
        }

        Util.forEach(tagsArray, function(tag: any){
            let option: HTMLOptionElement = document.createElement('option');
            option.className = SchemaClassMap.insertOption;
            option.textContent = HtmlHelper.htmlDecode(tag[TagService.TAG_NAME]);
            try{
                option.value = JSON.stringify(tag);
                selectEl.appendChild(option);
            }
            catch(e){
                alert(`renderTagNameOptions failed, error: ${e.message}`);
            }
        }, this);

        selectEl.selectedIndex = -1;
    }

    private renderValuesOptions(selectEl: HTMLSelectElement, valuesArray: string[]): void{
        if(!selectEl || !valuesArray){
            return;
        }

        Util.forEach(valuesArray, function(value: string){
            let option: HTMLOptionElement = document.createElement('option');
            option.className = SchemaClassMap.insertOption;
            option.textContent = HtmlHelper.htmlDecode(value);
            option.value = value;
            selectEl.appendChild(option);
        }, this);

        //selectEl.selectedIndex = -1;
    }

    private addNewTreeNode(treeTextEl: HTMLElement, tag: any): void{
        
        if(!tag){
            return;
        }

        if(treeTextEl && treeTextEl.classList.contains('tree-text')){
            let treeExpandEl: HTMLElement = <HTMLElement>treeTextEl.previousElementSibling;
            let treeContentEl: HTMLElement = treeTextEl.parentElement;
            let treeChildNodeEl: HTMLElement = treeContentEl ? <HTMLElement>treeContentEl.nextElementSibling : undefined;
            let treeNodeEl: HTMLElement = treeContentEl ? treeContentEl.parentElement : undefined;

            if(treeNodeEl){
                if(treeChildNodeEl){
                    treeChildNodeEl.classList.remove('hidden');
                    treeChildNodeEl.appendChild(this.buildTreeNode(tag));
                }
                else{
                    treeChildNodeEl = document.createElement('div');
                    treeChildNodeEl.classList.add('tree-child-node');
                    treeChildNodeEl.appendChild(this.buildTreeNode(tag));
                    treeNodeEl.appendChild(treeChildNodeEl);
                }

                if(treeExpandEl){
                    treeExpandEl.classList.remove('hidden');
                    treeExpandEl.textContent = '-';
                    treeExpandEl.setAttribute('data-expand', 'true');
                }
            }
        }
        else{//add node to the root xml node
            let treeviewContainerEl: HTMLElement = document.getElementById(SchemaIdMap.treeviewContainer);
            if(treeviewContainerEl){
                treeviewContainerEl.appendChild(this.buildTreeNode(tag));
            }
        }
    }

    private modifyTreeNode(treeTextEl: HTMLElement, tag: any): void{
        if(!treeTextEl || !tag){
            return;
        }

        let treeContentEl: HTMLElement = treeTextEl.parentElement;
        let treeNodeEl: HTMLElement = treeContentEl ? treeContentEl.parentElement : undefined;
        let treeNodeParentEl: HTMLElement = treeNodeEl ? treeNodeEl.parentElement : undefined;

        let oldTagJSON: string = treeTextEl.getAttribute('data-item');
        let oldTagObj: any = undefined;
        let newTagJSON: string = '';

        try{
            oldTagObj = JSON.parse(oldTagJSON);
            newTagJSON = JSON.stringify(tag);
        }
        catch(e){
            LogHelper.log(`modifyTreeNode failed, error: ${e.message}`);
        }

        if(oldTagObj){
            if(oldTagObj[TagService.TAG_NAME] === tag[TagService.TAG_NAME]){
                treeTextEl.setAttribute('data-item', newTagJSON);
            }
            else{
                if(treeNodeParentEl){
                    let newTreeNodeEl: HTMLElement = this.buildTreeNode(tag);
                    let treeNodeSiblingEl: HTMLElement = <HTMLElement>treeNodeEl.nextElementSibling;
                    treeNodeParentEl.removeChild(treeNodeEl);
                    if(treeNodeSiblingEl){
                        treeNodeParentEl.insertBefore(newTreeNodeEl, treeNodeSiblingEl);
                    }
                    else{
                        treeNodeParentEl.appendChild(newTreeNodeEl);
                    }
                }
            }
        }

    }

    private buildTreeNode(tag: any): HTMLElement{

        if(!tag){
            return;
        }
        let scope: Schema = this;
        let treeNode: HTMLDivElement = document.createElement('div');
        let treeContent: HTMLDivElement = document.createElement('div');
        let treeExpand: HTMLDivElement = document.createElement('div');
        let treeText: HTMLDivElement = document.createElement('div');
        let treeWidget: HTMLDivElement = document.createElement('div');

        //set element styles
        treeNode.className = 'tree-node';
        treeContent.className = 'tree-content';
        treeExpand.className = 'tree-expand';
        treeText.className = 'tree-text';
        treeWidget.className = 'tree-widget';

        //append elements
        treeContent.appendChild(treeExpand);
        treeContent.appendChild(treeText);
        treeContent.appendChild(treeWidget);
        treeNode.appendChild(treeContent);

        //render node
        if(tag && tag[TagService.TAG_NAME]){
            treeExpand.textContent = '+';
            treeExpand.setAttribute('data-expand', 'false');
            treeExpand.classList.add('hidden');
            
            treeText.textContent = HtmlHelper.htmlDecode(tag[TagService.TAG_NAME]);
            treeText.setAttribute('data-item', JSON.stringify(tag));

            this.buildWidgets(treeWidget);
        }

        treeExpand.addEventListener('click', function(evt){
            scope.treeviewExpandClickHandler.call(scope, evt);
        }, false);

        return treeNode;
    }

    private buildWidgets(widgetContainer: HTMLElement): void{

        let scope: Schema = this;
        let plusEl: HTMLElement = document.createElement('span');
        let crossEl: HTMLElement = document.createElement('span');
        let hamburgerEl: HTMLElement = document.createElement('span');
        let unfolderEl: HTMLElement = document.createElement('span');        

        let plusWrapper: HTMLElement = document.createElement('div');
        let crossWrapper: HTMLElement = document.createElement('div');
        let hamburgerWrapper: HTMLElement = document.createElement('div');
        let hiddenWrapperEl: HTMLElement = document.createElement('div');
        let unfolderWrapper: HTMLElement = document.createElement('div');        

        plusEl.className = 'icon plus';
        crossEl.className = 'icon red-cross';
        hamburgerEl.className = 'icon edit';
        unfolderEl.className = 'icon unfolder'

        plusWrapper.className = 'icon-wrapper';
        crossWrapper.className = 'icon-wrapper';
        hamburgerWrapper.className = 'icon-wrapper';
        hiddenWrapperEl.className = 'hidden-wrapper hidden';
        unfolderWrapper.className = 'icon-wrapper';

        unfolderWrapper.setAttribute('data-expand', 'false');

        plusWrapper.addEventListener('click', function(evt){
            if(!evt){
                return;
            }
            let curIconWrapperEl: HTMLElement = <HTMLElement>evt.currentTarget;
            let curHiddenWrapperEl: HTMLElement = curIconWrapperEl ? curIconWrapperEl.parentElement : undefined;
            let treeWidgetEl: HTMLElement = curHiddenWrapperEl ? curHiddenWrapperEl.parentElement : undefined;
            let treeTextEl: HTMLElement = treeWidgetEl ? <HTMLElement>treeWidgetEl.previousElementSibling : undefined;

            let data: string = treeTextEl ? treeTextEl.getAttribute('data-item') : '';

            scope.treeviewAddClickHandler.call(scope, treeTextEl, data);

            evt.stopPropagation();
        },false);

        crossWrapper.addEventListener('click', function(evt){
            if(!evt){
                return;
            }
            let curIconWrapperEl: HTMLElement = <HTMLElement>evt.currentTarget;
            let curHiddenWrapperEl: HTMLElement = curIconWrapperEl ? curIconWrapperEl.parentElement : undefined;
            let treeWidgetEl: HTMLElement = curHiddenWrapperEl ? curHiddenWrapperEl.parentElement : undefined;
            let treeTextEl: HTMLElement = treeWidgetEl ? <HTMLElement>treeWidgetEl.previousElementSibling : undefined;
            let data: string = treeTextEl ? treeTextEl.getAttribute('data-item') : '';

            scope.treeviewDeleteClickHandler.call(scope, treeTextEl, data);

            evt.stopPropagation();
        },false);

        hamburgerWrapper.addEventListener('click', function(evt){
            if(!evt){
                return;
            }
            let curIconWrapperEl: HTMLElement = <HTMLElement>evt.currentTarget;
            let curHiddenWrapperEl: HTMLElement = curIconWrapperEl ? curIconWrapperEl.parentElement : undefined;
            let treeWidgetEl: HTMLElement = curHiddenWrapperEl ? curHiddenWrapperEl.parentElement : undefined;
            let treeTextEl: HTMLElement = treeWidgetEl ? <HTMLElement>treeWidgetEl.previousElementSibling : undefined;
            let data: string = treeTextEl ? treeTextEl.getAttribute('data-item') : '';

            scope.treeviewModifyClickHandler.call(scope, treeTextEl, data);

            evt.stopPropagation();
        },false);

        unfolderWrapper.addEventListener('click', function(evt: Event): any{
            if(scope.m_curTreeview){
                scope.m_curTreeview.unfolderClickHandler(evt);
            }
        }, false);        

        plusWrapper.appendChild(plusEl);
        crossWrapper.appendChild(crossEl);
        hamburgerWrapper.appendChild(hamburgerEl);
        unfolderWrapper.appendChild(unfolderEl);

        hiddenWrapperEl.appendChild(plusWrapper);
        hiddenWrapperEl.appendChild(crossWrapper);
        hiddenWrapperEl.appendChild(hamburgerWrapper);

        widgetContainer.appendChild(hiddenWrapperEl);
        widgetContainer.appendChild(unfolderWrapper);
    }

    private getRelyOnValues(el: HTMLSelectElement): string{
        if(!el){
            return;
        }

        let values: string[] = [];

        Util.forEach(el.options, function(el: HTMLOptionElement){
            if(el.selected){
                values.push(el.value);
            }
        }, this);

        return values.join('|');
    }

    private parseXmlNode(xmlNode: Element, treeNode: Element): void{
        
        if(!xmlNode || !treeNode){
            return;
        }

        let tagXml: string = '<Tag/>';
        let treeContentEl: Element = <Element>treeNode.firstChild;
        let treeExpandEl: Element = treeContentEl ? <Element>treeContentEl.firstChild : undefined;
        let treeTextEl: Element = treeExpandEl ? <Element>treeExpandEl.nextSibling : undefined;
        let treeChildNodeEl: Element = treeContentEl ? <Element>treeContentEl.nextSibling : undefined;
        let tagObj: any = {};
        let xmlDoc: Document = undefined;
        let xmlEl: Element = undefined;

        try{
            xmlDoc = XmlHelper.loadXMLDoc(tagXml);
            xmlEl = xmlDoc ? xmlDoc.documentElement : undefined;
            tagObj = JSON.parse(treeTextEl.getAttribute('data-item'));//alert(treeTextEl.getAttribute('data-item'));

            if(tagObj){
                xmlEl.setAttribute(TagHelper.NAME_ATTR_NAME, tagObj[TagService.TAG_NAME]);
                xmlEl.setAttribute(TagHelper.VALUES_ATTR_NAME, tagObj[TagService.TAG_VALUE]);
                xmlEl.setAttribute(TagHelper.DEFAULT_ATTR_NAME, tagObj[TagService.DEFAULT_VALUE]);
                xmlEl.setAttribute(TagHelper.EDITABLE_ATTR_NAME, tagObj[TagService.EDITABLE]);
                xmlEl.setAttribute(TagHelper.RELYON_ATTR_NAME, tagObj[TagService.RELY_ON]);
                xmlNode.appendChild(xmlEl);
            }
        }
        catch(e){
            LogHelper.log(`appendXmlNode -> loadXMLDoc failed, error: ${e.message}`);
        }

        if(treeChildNodeEl){
            let curNode: Element = <Element>treeChildNodeEl.firstChild;
            while(curNode){
                if(curNode.nodeType === 1 && curNode.classList.contains('tree-node')){
                    this.parseXmlNode(xmlEl, curNode);
                }
                curNode = <Element>curNode.nextSibling;
            }
        }
    }

    private saveSchemaSummary(this: Schema, schemaXML: string): void{
        // Get user input info
        let xmlTemplate: string = '<?xml version="1.0" encoding="UTF-8"?><Classification type="manual"></Classification>';
        let schemaNameInputEl: HTMLInputElement = <HTMLInputElement>document.getElementById(SchemaIdMap.schemaNameInput);
        let schemaDescInputEl: HTMLTextAreaElement = <HTMLTextAreaElement>document.getElementById(SchemaIdMap.schemaDescText);
        let newSchemaObj: any = {};
        let oldSchemaObj: any = {};
        let curSchemaNameSpan: HTMLElement = undefined;

        if(schemaNameInputEl && schemaDescInputEl){
            
            newSchemaObj[SchemaService.SCHEMA_NAME] = HtmlHelper.htmlEncode(schemaNameInputEl.value.toLowerCase()).trim();
            newSchemaObj[SchemaService.SCHEMA_DATA] = schemaXML || xmlTemplate;
            newSchemaObj[SchemaService.SCHEMA_DESCRIPTION] = HtmlHelper.htmlEncode(schemaDescInputEl.value.toLowerCase()).trim();
            newSchemaObj[SchemaService.STATE] = EntityState.Added;

            // Check is new or exist schema
            let curSelectedEl: HTMLElement = this.getCurSelectedSchemaElement();
            if (!curSelectedEl){
                let emptySchema: any = this.constructEmptySchema();
                curSelectedEl = this.addSchemaItem(emptySchema);
            }
            curSchemaNameSpan = <HTMLElement>curSelectedEl.getElementsByClassName(SchemaClassMap.schemaName)[0];
            if (!curSchemaNameSpan){
                LogHelper.log(`saveSchemaSummary failed, curSchemaNameSpan is invalid`);
                return ;
            }

            // Save schema info
            try{
                
                let oldSchemaString: string = curSchemaNameSpan.getAttribute('data-schema');
                
                if(oldSchemaString){
                    oldSchemaObj = JSON.parse(oldSchemaString);
                    if(oldSchemaObj[SchemaService.STATE] !== EntityState.Added){
                        newSchemaObj[SchemaService.STATE] = EntityState.Modified;
                    }
                    this.m_serviceProvider.addSchema(JSON.stringify(newSchemaObj));
                    newSchemaObj[SchemaService.STATE] = EntityState.Unchanged;
                }

                curSchemaNameSpan.textContent = HtmlHelper.htmlDecode(newSchemaObj[SchemaService.SCHEMA_NAME]);
                curSchemaNameSpan.setAttribute('data-schema', JSON.stringify(newSchemaObj));
            }
            catch(e){
                LogHelper.log(`saveSchemaSummary failed, error: ${e.message}`);
            }
        }
        else{
            LogHelper.log(`saveSchemaSummary failed`);
        }

        this.eliminateClickState.call(this);
        this.clearSchemaCreateSection();

        //display treeview section
        let clickEvt: Event = Util.createEvent('click');
        curSchemaNameSpan.dispatchEvent(clickEvt);
        this.displayView(Schema.TREE_VIEW);
    }

    private saveSchemaData(): void{
        let scope: Schema = this;
        let xmlTemplate: string = '<?xml version="1.0" encoding="UTF-8"?><Classification type="manual"></Classification>';
        let xmlDoc: Document = XmlHelper.loadXMLDoc(xmlTemplate);
        let rootXmlNode: Element = xmlDoc ? xmlDoc.documentElement : undefined;
        let treeviewContainer: HTMLElement = <HTMLElement>document.getElementById(SchemaIdMap.treeviewContainer);
        let curSelectedSchema: HTMLElement = scope.getCurSelectedSchemaElement();
        let schemaNameSpan:　HTMLElement = curSelectedSchema ? <HTMLElement>curSelectedSchema.getElementsByClassName(SchemaClassMap.schemaName)[0] : undefined;
        let oldSchemaJSON: string = schemaNameSpan ? schemaNameSpan.getAttribute('data-schema') : '';
        let oldSchemaObj: any = undefined;

        try{
            oldSchemaObj = JSON.parse(oldSchemaJSON);
        }
        catch(e){
            LogHelper.log(`saveSchemaData -> parse schema json failed, error: ${e.message}`);
        }

        let curNode: Element = treeviewContainer ? <Element>treeviewContainer.firstChild : undefined;
        while(curNode){
            if(curNode.nodeType === 1 && curNode.classList.contains('tree-node')){
                scope.parseXmlNode(rootXmlNode, curNode);
            }
            curNode = <HTMLElement>curNode.nextSibling;
        }

        let dataXml: string = XmlHelper.convertXmlDocumentToString(xmlDoc);//alert(dataXml);
        
        if(oldSchemaObj && schemaNameSpan){
            oldSchemaObj[SchemaService.SCHEMA_DATA] = dataXml;

            if(oldSchemaObj[SchemaService.STATE] !== EntityState.Added){
                oldSchemaObj[SchemaService.STATE] = EntityState.Modified;
                scope.m_serviceProvider.addSchema(JSON.stringify(oldSchemaObj));
                oldSchemaObj[SchemaService.STATE] = EntityState.Unchanged;
                schemaNameSpan.setAttribute('data-schema', JSON.stringify(oldSchemaObj));
            }
        }
    }

    private createTagRow(): HTMLElement{
        
        let scope: Schema = this;
        let rowEl: HTMLElement = document.createElement('div');
        let tagColEl: HTMLElement = document.createElement('div');
        let defaultColEl: HTMLElement = document.createElement('div');
        let relyOnColEl: HTMLElement = document.createElement('div');
        let tagSelectEl: HTMLSelectElement = document.createElement('select');
        let defaultValueSelectEl: HTMLSelectElement = document.createElement('select');
        let relyonSelectEl: HTMLSelectElement = document.createElement('select');

        rowEl.className = 'row';
        tagColEl.className = 'col';
        defaultColEl.className = 'col';
        relyOnColEl.className = 'col';

        tagColEl.appendChild(tagSelectEl);
        defaultColEl.appendChild(defaultValueSelectEl);
        relyOnColEl.appendChild(relyonSelectEl);

        rowEl.appendChild(tagColEl);
        rowEl.appendChild(defaultColEl);
        rowEl.appendChild(relyOnColEl);

        let totalTagsInfo: string = scope.m_serviceProvider.getTagsInJSON();
        let totalTagsArray: any[] = undefined;

        try{
            totalTagsArray = JSON.parse(totalTagsInfo);
            scope.renderTagNameOptions(tagSelectEl, totalTagsArray);
            tagSelectEl.addEventListener('change', function(evt: Event): any{
                scope.tagSelectChangeHandler.call(scope, evt, defaultValueSelectEl, relyonSelectEl);
            }, false);

            let evt: Event = Util.createEvent('change');
            tagSelectEl.selectedIndex = tagSelectEl.options.length - 1;
            tagSelectEl.dispatchEvent(evt);
        }
        catch(e){
            alert(`createTagRow -> parse tags json failed, error: ${e.message}`);
        }

        return rowEl;
    }

    private getExportPolicyModelJSON(): string{

        let scope: Schema = this;
        let result: string = '';

        let pm: any = {};
        pm.policyModels = [];
        pm.components = [];
        pm.policyTree = {};
        pm.importedPolicyIds = [];
        pm.overrideDuplicates = false;
        pm.componentToSubCompMap = {};

        let policyModel: any = {};
        policyModel.id = +new Date();
        policyModel.name = 'sfb_schemas';
        policyModel.shortName = policyModel.name.toLowerCase();
        policyModel.type = 'RESOURCE';
        policyModel.status = 'ACTIVE';
        policyModel.attributes = [];
        policyModel.actions = [];
        policyModel.obligations = [];

        let obligation: any = {};
        obligation.id = +new Date();
        obligation.name = 'SFB Manual Classify';
        obligation.shortName = 'sfb_manual_classify';
        obligation.runAt = 'PEP';
        obligation.parameters = [];

        let parameterForceClassify: any = {};
        parameterForceClassify.id = +new Date();
        parameterForceClassify.name = 'Force_Classify';
        parameterForceClassify.shortName = parameterForceClassify.name.toLowerCase();
        parameterForceClassify.type = 'LIST';
        parameterForceClassify.defaultValue = 'Yes';
        parameterForceClassify.listValues = 'Yes,No';
        parameterForceClassify.hidden = false;
        parameterForceClassify.editable = false;
        parameterForceClassify.mandatory = false;

        let parameterManualClassify: any = {};
        parameterManualClassify.id = +new Date();
        parameterManualClassify.name = 'Data';
        parameterManualClassify.shortName = parameterManualClassify.name.toLowerCase();
        parameterManualClassify.type = 'LIST';
        parameterManualClassify.defaultValue = '';
        parameterManualClassify.listValues = scope.getSchemaNames();
        parameterManualClassify.hidden = true;
        parameterManualClassify.editable = false;
        parameterManualClassify.mandatory = false;

        obligation.parameters.push(parameterForceClassify);
        obligation.parameters.push(parameterManualClassify);
        policyModel.obligations.push(obligation);
        pm.policyModels.push(policyModel);

        try{
            result = JSON.stringify(pm);
        }
        catch(e)
        {
            alert(`export failed, error: ${e.message}`);
        }

        return result;
    }

    private getSchemaNames(): string{
        
        let scope: Schema = this;
        let nameArray: string[] = [];
        let schemaJSON: string = scope.m_serviceProvider.getSchemasInJSON();
        let schemaArray: any[] = undefined;
        
        try{
            schemaArray = JSON.parse(schemaJSON);
            Util.forEach(schemaArray, function(schema: any): void{
                nameArray.push(HtmlHelper.htmlDecode(schema[SchemaService.SCHEMA_NAME]));
            }, scope);
        }
        catch(e){
            alert(`getSchemaNames failed, error: ${e.message}`);
        }

        return nameArray.join(',');
    }

    private download(content: string, fileName: string, mimeType:string = 'text/plain'): void{

        if(!content || !fileName){
            return;
        }

        if(window.navigator.msSaveBlob){
            let blob: Blob = new Blob([content], fileName);
            window.navigator.msSaveBlob(blob, fileName);
        }
        else{
            let anchor: HTMLAnchorElement = document.createElement('a');
            let clickEvt: Event = Util.createEvent('click');
            anchor.href = `data:${mimeType},${content}`;
            anchor.download = fileName;
            anchor.dispatchEvent(clickEvt);
        }
    }

    private checkSchemaNameValidation(schemaName: string): boolean{
        let hasInvalidChar: boolean = false;
        if ((0 === schemaName.length) || (255 < schemaName.length)){
            hasInvalidChar = true;
        }else{
            let hasInvalidReg = /[^\w\d\_]/g; 
            hasInvalidChar = hasInvalidReg.test(schemaName);
        }
        return !hasInvalidChar;
    }

    //event handlers
    private addNewSchemaClickHandlerEx(this: Schema, evt: Event): any{        
        this.eliminateClickState.call(this);
        this.displayView(Schema.CREATE_VIEW);
    }

    private addNewSchemaClickHandler(this: Schema, evt: Event): any{

        let emptySchema: any = this.constructEmptySchema();
        let schemaRowEl: HTMLElement = this.createSchemaRow(emptySchema);
        let schemaListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaListBody)[0];
        
        this.eliminateClickState.call(this);

        this.displayView(Schema.CREATE_VIEW);

        if(schemaRowEl && schemaListBody){
            schemaRowEl.classList.add(SchemaClassMap.schemaListItemClick);
            schemaListBody.appendChild(schemaRowEl);
        }
        else{
            LogHelper.log(`addNewSchemaClickHandler failed, new row: ${schemaRowEl}, schema list body: ${schemaListBody}`);
        }
    }

    private okClickHandler(this: Schema, evt: Event): any{
        let scope: Schema = this;
        let nameInputEl: HTMLInputElement = <HTMLInputElement>document.getElementById(SchemaIdMap.schemaNameInput);
        
        if(nameInputEl){
            let strSchemaName = nameInputEl.value.trim();
            // Schema name must be valid and not repeat
            if(scope.checkSchemaNameValidation(strSchemaName) && (!scope.isRepeatSchemaNameEx(strSchemaName))){

                const curSchemaWrap: HTMLElement = this.getCurSelectedSchemaElement();
                const curSchemaSpan: HTMLElement = curSchemaWrap ? <HTMLElement>curSchemaWrap.getElementsByClassName('schema-name')[0] : null;
                const schemaData = curSchemaSpan ? curSchemaSpan.getAttribute('data-schema') : '{}';

                scope.saveSchemaSummary(JSON.parse(schemaData).data);
            }
            else{
                //alert('save schema failed.');
                LogHelper.log(`okClickHandler failed, input value invalid: ${nameInputEl.value}`);
            }
        }
        else{
            LogHelper.log(`okClickHandler failed, input element null`);
        }
    }

    private cancelClickHandler(this: Schema, evt: Event): any{

        let scope: Schema = this;

        scope.clearSchemaCreateSection();
        scope.deleteCurSchemaInList();
        scope.displayView(Schema.NONE_VIEW);
    }

    private applyClickHandler(this: Schema, evt: Event): any{
        let scope: Schema = this;
        scope.okClickHandler(evt);
    }    

    private saveClickHandler(this: Schema, evt: Event): any{
        this.m_serviceProvider.saveSchemaChanges();
    }

    private schemaRowClickHandler(this: Schema, evt: Event): any{

        if(!evt){
            return false;
        }

        this.clearTreeViewSection();

        let curTarget: HTMLElement = <HTMLElement>evt.target;
        let parentTarget: HTMLElement = (curTarget && curTarget.nodeName.toLowerCase() === 'span') ? curTarget.parentElement : undefined;
        let schemaNameEl: HTMLElement = document.getElementById(SchemaIdMap.schemaNameSpan);
        let schemaDescEl: HTMLElement = document.getElementById(SchemaIdMap.schemaDescPara);

        if(parentTarget && schemaNameEl && schemaDescEl){
            //change visual state to click
            this.eliminateClickState.call(this);
            parentTarget.classList.add(SchemaClassMap.schemaListItemClick);

            //get data in data-schema attribute
            let schemaNameSpan: HTMLElement = <HTMLElement>parentTarget.getElementsByClassName(SchemaClassMap.schemaName)[0];
            if(schemaNameSpan){
                let schema: string = schemaNameSpan.getAttribute('data-schema');

                //alert(schema);

                if(schema){
                    let schemaObj: any = JSON.parse(schema);
                    if(schemaObj){
                        schemaNameEl.textContent = HtmlHelper.htmlDecode(schemaObj[SchemaService.SCHEMA_NAME]);
                        schemaDescEl.textContent = HtmlHelper.htmlDecode(schemaObj[SchemaService.SCHEMA_DESCRIPTION]);
                        this.displayTreeViewSection(schemaObj);
                    }
                    else{
                        LogHelper.log(`schemaRowClickHandler failed, schemaObj: ${schemaObj}`);
                    }
                }
                else{
                    LogHelper.log(`schemaRowClickHandler failed, schema in json: ${schema}`);
                }
            }
            else{
                LogHelper.log(`schemaRowClickHandler failed, schemaNameSpan: ${schemaNameSpan}`);
            }
        }
        else{
            LogHelper.log(`schemaRowClickHandler failed, target: ${curTarget}`);
        }

        evt.stopPropagation();

    }

    private schemaDeleteClickHandler(this: Schema, evt: Event): any{
        if(!evt){
            return false;//stop from bubbling to the schema-row
        }
        let scope: Schema = this;
        let curTarget: HTMLElement = <HTMLElement>evt.target;
        let crossWrapper: HTMLElement = curTarget ? curTarget.parentElement : undefined;
        let schemaRow: HTMLElement = crossWrapper ? crossWrapper.parentElement : undefined;
        let schemaListBody: HTMLElement = schemaRow ? schemaRow.parentElement : undefined;
        let schemaNameSpan: HTMLElement = undefined;

        if(schemaListBody && schemaRow){
            schemaNameSpan = <HTMLElement>schemaRow.getElementsByClassName(SchemaClassMap.schemaName)[0];

            try{
                let oldSchemaObj: any = JSON.parse(schemaNameSpan.getAttribute('data-schema'));
                oldSchemaObj[SchemaService.STATE] = EntityState.Deleted;
                let sNewSchema: string = JSON.stringify(oldSchemaObj);
                scope.m_serviceProvider.addSchema(sNewSchema);
            }
            catch(e){
                alert(`schemaDeleteClickHandler failed, error: ${e.message}`);
            }

            schemaListBody.removeChild(schemaRow);
            scope.clearSchemaCreateSection();
            scope.clearTreeViewSection();
            scope.displayView(Schema.NONE_VIEW);
        }
        else{
            LogHelper.log(`schemaDeleteClickHandler failed, schemaListBody: ${schemaListBody}, schemaRow: ${schemaRow}`);
        }

        evt.stopPropagation();//stop from bubbling to the schema-row
    }

    private treeviewAddClickHandler(this: Schema, sender: HTMLElement, msg: string): void{
        
        if(!sender || !msg){
            return;
        }

        let scope: Schema = this;
        let insertPrompt: HTMLElement = document.getElementById(SchemaIdMap.insertPrompt);
        let insertContent: HTMLElement = insertPrompt ? <HTMLElement>insertPrompt.getElementsByClassName(SchemaClassMap.schemaInsertContent)[0] : undefined;
        let insertTitle: HTMLElement = insertPrompt ? <HTMLElement>insertPrompt.getElementsByClassName(SchemaClassMap.schemaInsertTitle)[0] : undefined;
        let addRow: HTMLElement = document.getElementById(SchemaIdMap.addRow);

        if(insertContent && addRow && insertTitle){

            scope.m_curInsertTreeTextEl = sender;
            let pureTagObj: any = JSON.parse(msg);
            insertTitle.textContent = `${Schema.INSERT_ELEMENT_TITLE_MSG} "${HtmlHelper.htmlDecode(pureTagObj[TagService.TAG_NAME])}"`;
            insertTitle.setAttribute('data-item', msg);

            insertPrompt.classList.add('show');
            let newRow: HTMLElement = scope.createTagRow();
            insertContent.insertBefore(newRow, addRow);
        }
    }

    private treeviewDeleteClickHandler(this: Schema, sender: HTMLElement, msg: string): void{
        if(!sender || !msg){
            return;
        }

        let scope: Schema = this;

        if(sender.classList.contains('tree-text'))
        {
            let treeExpandEl: HTMLElement = <HTMLElement>sender.previousElementSibling;
            let treeContentEl: HTMLElement = sender.parentElement;
            let treeNodeEl: HTMLElement = treeContentEl ? treeContentEl.parentElement : undefined;
            let treeNodeParentEl: HTMLElement = treeNodeEl ? treeNodeEl.parentElement : undefined;

            if(treeNodeParentEl){
                treeNodeParentEl.removeChild(treeNodeEl);
            }
        }
        scope.saveSchemaData();
    }

    private treeviewModifyClickHandler(this: Schema, sender: HTMLElement, msg: string): void{
        if(!sender || !msg){
            return;
        }        

        let scope: Schema = this;
        let modifyPrompt: HTMLElement = document.getElementById(SchemaIdMap.modifyPrompt);
        let treeContentEl: HTMLElement = sender.parentElement;
        let treeNodeEl: HTMLElement = treeContentEl ? treeContentEl.parentElement : undefined;
        let parentTreeChildNodeEl: HTMLElement = treeNodeEl ? treeNodeEl.parentElement : undefined;
        let parentTreeContentEl: HTMLElement = parentTreeChildNodeEl ? <HTMLElement>parentTreeChildNodeEl.previousElementSibling : undefined;
        let parentTreeTextEl: HTMLElement = parentTreeContentEl ? <HTMLElement>parentTreeContentEl.getElementsByClassName('tree-text')[0] : undefined;

        let tagSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.tagSelector);
        let defaultValueSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.defalutValueSelector);
        let parentTagInputEl: HTMLInputElement = <HTMLInputElement>document.getElementById(SchemaIdMap.parentTagInput);
        let relyOnSelectEl: HTMLSelectElement = <HTMLSelectElement>document.getElementById(SchemaIdMap.relyOnSelector);        

        scope.m_curModifyTreeTextEl = sender;
        scope.clearSchemaModifySection();
        modifyPrompt.classList.add('show');

        try{

            let curPureTagJSON: string = sender.getAttribute('data-item');
            let curTagObj: any = JSON.parse(curPureTagJSON);
            let curValues: string = curTagObj ? curTagObj[TagService.TAG_VALUE] : '';
            let curTagDefaultValue: string = curTagObj ? curTagObj[TagService.DEFAULT_VALUE] : '';
            let curValueArray: string[] = curValues.split('|');

            let totalTagsJSON: string = scope.m_serviceProvider.getTagsInJSON();
            let totalTagArray: any[] = JSON.parse(totalTagsJSON);

            scope.renderTagNameOptions(tagSelectEl, totalTagArray);
            scope.renderValuesOptions(defaultValueSelectEl, curValueArray);

            scope.matchTagNameOption(curTagObj, tagSelectEl);
            scope.matchTagPropertyWithOption(TagService.DEFAULT_VALUE, curTagDefaultValue, defaultValueSelectEl);

            parentTagInputEl.disabled = true;
            tagSelectEl.disabled = true;

            if(parentTreeTextEl){

                let parentPureTagJSON: string = parentTreeTextEl.getAttribute('data-item');
                let parentTagObj: any = JSON.parse(parentPureTagJSON);
                let parentValues: string = parentTagObj ? parentTagObj[TagService.TAG_VALUE] : '';
                let parentTagName: string = parentTagObj ? parentTagObj[TagService.TAG_NAME] : '';
                let parentValueArray: string[] = parentValues.split('|');

                parentTagInputEl.value = parentTagName ? HtmlHelper.htmlDecode(parentTagName) : '';

                if(parentValues !== null && parentValues !== undefined){
                    scope.renderValuesOptions(relyOnSelectEl, parentValueArray); // set option values

                    // Set default values
                    let strCurRelyOnValues = curTagObj ? curTagObj[TagService.RELY_ON] : '';
                    let szCurRelyOnValues: string[] = strCurRelyOnValues.split('|');
                    scope.matchTagMultiSelectWithOption(szCurRelyOnValues, relyOnSelectEl)
                    relyOnSelectEl.disabled = false;
                }
                else{
                    relyOnSelectEl.textContent = '';
                    relyOnSelectEl.disabled = true;
                }
            }
            else{
                parentTagInputEl.value = '';
                relyOnSelectEl.textContent = '';
                relyOnSelectEl.disabled = true;
            }
        }
        catch(e){
                alert(`treeviewModifyClickHandler failed, error: ${e.message}`);
        }
    }

    private treeviewExpandClickHandler(this:Schema, evt: Event): any{
        
        if(!evt){
            return false;
        }
        
        let scope: Schema = this;
        let expandEl: HTMLElement = <HTMLElement>evt.target;
        let treeContentEl: HTMLElement = expandEl ? expandEl.parentElement : undefined;
        let treeChildNodeEl: HTMLElement = treeContentEl ? <HTMLElement>treeContentEl.nextElementSibling : undefined;

        if(expandEl){
            let isExpand: string = expandEl.getAttribute('data-expand');
            if(isExpand === 'false'){
                expandEl.textContent = '-';
                expandEl.setAttribute('data-expand', 'true');
                treeChildNodeEl.classList.remove('hidden');
            }
            else{
                expandEl.textContent = '+';
                expandEl.setAttribute('data-expand', 'false');
                treeChildNodeEl.classList.add('hidden');
            }
        }

        evt.stopPropagation();
    }

    private tagSelectChangeHandler(this: Schema, evt: Event, defaultValueSelectEl: HTMLSelectElement, relyonSelectEl: HTMLSelectElement): any{
        
        if(!evt || !defaultValueSelectEl || !relyonSelectEl){
            return;
        }

        defaultValueSelectEl.textContent = '';
        relyonSelectEl.textContent = '';

        let scope: Schema = this;
        let insertPromptEl: HTMLElement = document.getElementById(SchemaIdMap.insertPrompt);
        let insertTitleEl: HTMLElement = insertPromptEl ? <HTMLElement>insertPromptEl.getElementsByClassName(SchemaClassMap.schemaInsertTitle)[0] : undefined;
        let selectEl: HTMLSelectElement = <HTMLSelectElement>evt.target;    
        let selectedOption: HTMLOptionElement = <HTMLOptionElement>selectEl.options[selectEl.selectedIndex];
        let pureTagJSON: string = selectedOption ? selectedOption.value : '';

        if(pureTagJSON){//alert(pureTagJSON);
            let tagObj: any = JSON.parse(pureTagJSON);
            let values: string = tagObj ? tagObj[TagService.TAG_VALUE] : '';
            let defaultValue: string = tagObj ? tagObj[TagService.DEFAULT_VALUE] : '';
            let parentPureTagJSON: string = insertTitleEl ? insertTitleEl.getAttribute('data-item') : '';
            let relyOnValues: string = '';
            let valuesArray: string[] = values.split('|');

            scope.renderValuesOptions(defaultValueSelectEl, valuesArray);

            if(parentPureTagJSON){
                try{
                    let parentTagObj: any = JSON.parse(parentPureTagJSON);
                    relyOnValues = parentTagObj ? parentTagObj[TagService.TAG_VALUE] : '';

                    if(relyOnValues){
                        relyonSelectEl.disabled = false;
                        let relyonValuesArray: string[] = relyOnValues.split('|');
                        scope.renderValuesOptions(relyonSelectEl, relyonValuesArray);
                    }
                    else{
                        relyonSelectEl.disabled = true;
                    }
                }
                catch(e){
                    alert(`tagSelectChangeHandler parse json failed, error: ${e.message}`);
                }
            }
            else{
                LogHelper.log(`tagSelectChangeHandler -> parse parent tag failed`);
                relyonSelectEl.disabled = true;
            }

            if(defaultValue){
                defaultValueSelectEl.disabled = false;
                scope.matchTagPropertyWithOption(TagService.DEFAULT_VALUE, defaultValue, defaultValueSelectEl);
            }
            else{
                defaultValueSelectEl.selectedIndex = defaultValueSelectEl.options.length - 1;
            }
        }
        else{
            LogHelper.log(`tagNameSelector addEventListener failed, tag: ${pureTagJSON}`);
        }        
    }

    private multiInsertClickHandler(this: Schema, evt: Event): any{
        
        let scope: Schema = this;
        let insertPrompt: HTMLElement = document.getElementById(SchemaIdMap.insertPrompt);
        let insertContent: HTMLElement = insertPrompt ? <HTMLElement>insertPrompt.getElementsByClassName(SchemaClassMap.schemaInsertContent)[0] : undefined;
        let addRow: HTMLElement = document.getElementById(SchemaIdMap.addRow);

        let newRow: HTMLElement = scope.createTagRow();
        insertContent.insertBefore(newRow, addRow);
    }

    private insertSaveClickHandler(this: Schema, evt: Event): any{
        
        let scope: Schema = this;
        let insertPromptEl: HTMLElement = document.getElementById(SchemaIdMap.insertPrompt);
        let insertContentEl: HTMLElement = insertPromptEl ? <HTMLElement>insertPromptEl.getElementsByClassName(SchemaClassMap.schemaInsertContent)[0] : undefined;
        let curRowEl: HTMLElement = insertContentEl ? <HTMLElement>insertContentEl.firstElementChild : undefined;
        let tagArray: any[] = [];

        while(curRowEl){
            if(!curRowEl.getAttribute('id')){
                let selectEls: NodeListOf<HTMLSelectElement> = <NodeListOf<HTMLSelectElement>>curRowEl.getElementsByTagName('select');
                let tagSelectEl: HTMLSelectElement = selectEls.item(0);
                let defaultValueSelectEl: HTMLSelectElement = selectEls.item(1);
                let relyonSelectEl: HTMLSelectElement = selectEls.item(2);

                if(tagSelectEl && defaultValueSelectEl && relyonSelectEl){
                    let pureTagJSON: string = (<HTMLOptionElement>tagSelectEl.options[tagSelectEl.selectedIndex]).value;
                    let defaultValue: string = (<HTMLOptionElement>defaultValueSelectEl.options[defaultValueSelectEl.selectedIndex]).value;
                    let relyonValues: string = scope.getRelyOnValues(relyonSelectEl);

                    try{
                        let tagObj: any = JSON.parse(pureTagJSON);
                        tagObj[TagService.DEFAULT_VALUE] = defaultValue;
                        tagObj[TagService.RELY_ON] = relyonValues;
                        tagArray.push(tagObj);
                    }
                    catch(e){
                        alert(`insertSaveClickHandler -> parse json failed, error: ${e.message}`);
                    }
                }
            }
            curRowEl = <HTMLElement>curRowEl.nextElementSibling;
        }

        if(tagArray.length > 0){
            Util.forEach(tagArray, function(curTagObj: any): void{
                scope.addNewTreeNode(scope.m_curInsertTreeTextEl, curTagObj);
            }, scope);

            scope.saveSchemaData();
        }

        if(insertPromptEl){
            insertPromptEl.classList.remove('show');
            scope.clearSchemaInsertSection();
        }
    }

    private insertCancelClickHandler(this: Schema, evt: Event): any{
        let scope: Schema = this;
        let insertPromptEl: HTMLElement = document.getElementById(SchemaIdMap.insertPrompt);
        if(insertPromptEl){
            insertPromptEl.classList.remove('show');
        }
        scope.clearSchemaInsertSection();
    }

    private previewClickHandler(this: Schema, evt: Event): any{
        let scope: Schema = this;
        let curSchemaSpan:　HTMLElement = undefined;
        let curSelectSchemaEl: HTMLElement = scope.getCurSelectedSchemaElement();
        if (curSelectSchemaEl){
            curSchemaSpan = <HTMLElement>curSelectSchemaEl.getElementsByClassName(SchemaClassMap.schemaName)[0];
        }

        let jsonCurSchemaData: JSON = null;
        let strSchemaData = "";
        if (curSchemaSpan){
            jsonCurSchemaData = scope.getSchemaData(curSchemaSpan);
            strSchemaData = jsonCurSchemaData[SchemaService.SCHEMA_DATA];
            // alert('Click preview, current schema name:[' + strCurSchemaName + ']SchemaName1:[' + strSchemaNameInAttr + '].\nSchemanData:[' + strSchemaData + ']');
        }

        // Show classification schema hierarchical structure UI 
        let obHierarchicalStructurePanel: HTMLElement = document.getElementById(SchemaIdMap.schemaHierarchicalStructurePanel);
        if (obHierarchicalStructurePanel){
            let obHierarchicalStructureContianer: HTMLElement = <HTMLElement>obHierarchicalStructurePanel.getElementsByClassName(SchemaClassMap.schemaHierarchicalStructureContainer)[0];
            if (obHierarchicalStructureContianer){
                scope.maskLayerHelper(true);
                scope.buildLinkageMenu(obHierarchicalStructureContianer, strSchemaData);
                obHierarchicalStructurePanel.classList.remove('hidden');
            }else{
                LogHelper.log(`previewClickHandler failed, cannot find hierarchical structure container, element id: ${SchemaIdMap.schemaHierarchicalStructureContainer}`);
            }
        }else{
            LogHelper.log(`previewClickHandler failed, cannot find hierarchical structure panel, element id: ${SchemaIdMap.schemaHierarchicalStructurePanel}`)
        }
    }

    private previewCancelClickHandler(this: Schema, evt: Event): any{
        let scope: Schema = this;
        let obHierarchicalStructurePanel: HTMLElement = document.getElementById(SchemaIdMap.schemaHierarchicalStructurePanel);
        if (obHierarchicalStructurePanel){
            obHierarchicalStructurePanel.classList.add('hidden');
            let obHierarchicalStructureContianer: HTMLElement = <HTMLElement>obHierarchicalStructurePanel.getElementsByClassName(SchemaClassMap.schemaHierarchicalStructureContainer)[0];
            if (obHierarchicalStructureContianer){
                HtmlHelper.clearSubNodes(obHierarchicalStructureContianer);
            }else{
                LogHelper.log(`previewCancelClickHandler failed, cannot find hierarchical structure container, element id: ${SchemaIdMap.schemaHierarchicalStructureContainer}`);
            }
        }else{
            LogHelper.log(`previewCancelClickHandler failed, cannot find hierarchical structure panel, element id: ${SchemaIdMap.schemaHierarchicalStructurePanel}`)
        }
        scope.maskLayerHelper(false);
    }

    private exportClickHandler(this: Schema, evt: Event): any{
        let scope: Schema = this;
        let policyModelJSON: string = scope.getExportPolicyModelJSON();
        let fileName: string = 'sfb_policymodel_' + +new Date() + '.bin';
        scope.download(policyModelJSON, fileName);
    }

    private schemaNameInputBlurHandlerClick(this: Schema, evt: Event): any{

        let scope: Schema = this;
        let bIsNameValid: boolean = false;
        let bIsNameRepeat: boolean = false;
        let inputEl: HTMLInputElement = evt ? <HTMLInputElement>evt.target : undefined;
        let promptMsgEl: HTMLElement = document.getElementById('input-check-msg');

        let strSchemaName = inputEl.value.trim();
        if(inputEl){
            bIsNameValid = scope.checkSchemaNameValidation(strSchemaName);
            bIsNameRepeat = scope.isRepeatSchemaNameEx(strSchemaName);
        }
        else{
            LogHelper.log(`schemaNameInputBlurHandlerClick failed, input element: ${inputEl}`);
        }

        if(promptMsgEl){
            if(!bIsNameValid){
                promptMsgEl.textContent = 'Only alphanumeric character and underscore("_") allowed and cannot be empty. The shema name max length is 255.';
                promptMsgEl.style.display = 'inline-block';
            }else if(bIsNameRepeat){
                promptMsgEl.textContent = 'The schema name:[' + strSchemaName + '] already exist';
                promptMsgEl.style.display = 'inline-block';
            }
            else{
                promptMsgEl.style.display = 'none';
            }
        }
    }

    // Tools
    //linkage menu operation
    private buildLinkageMenu(this: Schema, obContainer: HTMLElement, strObXml: string): LinkageMenu {
        let obLinkageMenu: LinkageMenu = null;
        if (obContainer && strObXml) {
            let styleModifier: IStyleModify = new HierarchicalStructureStyleModify();
            let dataSourceFormatter: IDataSourceFormat = new XmlDataSourceFormat();
   
            if (styleModifier && dataSourceFormatter && obContainer) {
                obLinkageMenu = new LinkageMenu(styleModifier, dataSourceFormatter, obContainer, strObXml);
                obLinkageMenu.bind();
            }
            else {
                LogHelper.log(`buildLinkageMenu() failed, styleModifier: ${styleModifier}, dataSourceFormatter: ${dataSourceFormatter}, container: ${obContainer}.`);
            }
        }
        else {
            LogHelper.log(`buildLinkageMenu() failed, ObXml: ${strObXml}.`);
        }     
        return obLinkageMenu;
    }
    private maskLayerHelper(this: Schema, bShowMask: boolean): void{
        let maskEl: HTMLElement = <HTMLElement>document.getElementsByClassName('mask')[0];
        if(maskEl){
            if(bShowMask){
                maskEl.classList.remove('hidden');
            }else{
                maskEl.classList.add('hidden');
            }
        }
    }
    private getSchemaData(this: Schema, curSchemaSpan: HTMLElement): JSON{
        let jsoinData: JSON = this.constructEmptySchema();
        if (curSchemaSpan){
            try {
                jsoinData = JSON.parse(curSchemaSpan.getAttribute(Schema.DATA_SCHEMA_ATTR));
            } catch (e) {
                LogHelper.log(`getSchemaData failed, exception: ${e.message}`);
            }            
        }
        return jsoinData;
    }
    private addSchemaItem(this: Schema, jsonSchemaInfo: any): HTMLElement{
        let schemaRowEl: HTMLElement = this.createSchemaRow(jsonSchemaInfo);
        let schemaListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaListBody)[0];      
        if(schemaRowEl && schemaListBody){
            schemaRowEl.classList.add(SchemaClassMap.schemaListItemClick);
            schemaListBody.appendChild(schemaRowEl);
        }
        else{
            schemaRowEl = undefined;
            LogHelper.log(`addSchemaItem failed, new row: ${schemaRowEl}, schema list body: ${schemaListBody}`);
        }
        return schemaRowEl;
    }

    private isRepeatSchemaNameEx(this: Schema, strInSchemaName: String)
    {
        let scope: Schema = this;
        let schemaNameSpan:　HTMLElement = undefined;
        let strCurSchemaName: String = ""
        let curSelectedSchema: HTMLElement = scope.getCurSelectedSchemaElement(); 
        if (curSelectedSchema){
            schemaNameSpan = <HTMLElement>curSelectedSchema.getElementsByClassName(SchemaClassMap.schemaName)[0];
            strCurSchemaName = schemaNameSpan.textContent.trim();
        }
        if (0 == strCurSchemaName.toLowerCase().localeCompare(strInSchemaName.toLowerCase().trim())){
            return false;
        }else{
            return scope.isExistSchemaName(strInSchemaName);
        }     
    }

    private isExistSchemaName(this: Schema, strInSchemaName: String): boolean{
        let scope: Schema = this;
        let bExist: boolean = false;

        let strInLowercaseSchemaName = strInSchemaName.toLowerCase();

        let schemaListBody: HTMLElement = <HTMLElement>document.getElementsByClassName(SchemaClassMap.schemaListBody)[0];        
        let schemaList: HTMLCollectionOf<Element> = <HTMLCollectionOf<Element>>schemaListBody.getElementsByClassName(SchemaClassMap.schemaListItem);

        let schemaListItem: HTMLElement = null;
        let schemaListSpan: HTMLElement = null;
        let strCurSchemaName: string = "";
        for (let i:number = 0; i<schemaList.length; ++i) {
            schemaListItem = <HTMLElement>schemaList[i];
            schemaListSpan = schemaList ? <HTMLElement>schemaListItem.getElementsByClassName(SchemaClassMap.schemaName)[0] : undefined;
            if (schemaListSpan){
                strCurSchemaName = schemaListSpan.textContent.trim();   
                if (0 === strCurSchemaName.toLowerCase().localeCompare(strInLowercaseSchemaName)){
                    bExist = true;
                    break;
                }
            }
            schemaListItem = null;
            schemaListSpan = null;
            strCurSchemaName = "";
        }
        return bExist;
    }
}
//entry point, called from c#
//must be attached to window in case that it can't be accessed in closure
(<any>window).init = function(){
    try{
        let main: Schema = new Schema();    
        main.initData();
    }
    catch(e){
        alert(`initData failed, error: ${e.message}`);
    }
    finally{        
        let maskEl: HTMLElement = <HTMLElement>document.getElementsByClassName('mask')[0];
        if(maskEl){
            maskEl.classList.add('hidden');
        }
    }
}