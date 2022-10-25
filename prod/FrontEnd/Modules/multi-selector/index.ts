import { Util } from '../util';
import { LogHelper } from '../helper';

export class MultiSelector{
    
    //members
    private m_container: HTMLElement;
    private m_dataArray: string[];
    private m_handlers:any = {};

    //event types
    static ItemSelectedEvent: string = 'selected';
    static ItemRemovedEvent: string = 'removed';
    static SelectorFocusEvent: string = 'focused';
    static SelectorBlurEvent: string = 'blurred';

    //ctor
    constructor(container: HTMLElement, dataArray: string[]){
        this.m_container = container;
        this.m_dataArray = dataArray;
        this.m_handlers[MultiSelector.ItemSelectedEvent] = [];
        this.m_handlers[MultiSelector.ItemRemovedEvent] = [];
        this.m_handlers[MultiSelector.SelectorFocusEvent] = [];
        this.m_handlers[MultiSelector.SelectorBlurEvent] = [];

        this.render();
    }

    //properities
    get selectedValues(): string[]{

        let selectedValues: string[] = [];
        let mainWrapperEl: HTMLElement = <HTMLElement>document.getElementsByClassName('main-wrapper')[0];
        let selectedTagEls: HTMLCollectionOf<HTMLElement> = mainWrapperEl ? <HTMLCollectionOf<HTMLElement>>mainWrapperEl.getElementsByClassName('selected-tag') : undefined;

        if(selectedTagEls && selectedTagEls.length > 0){
            Util.forEach(selectedTagEls, function(el: HTMLElement): void{
                if(el){
                    selectedValues.push(el.getAttribute('data-value'));
                }
            });
        }
        else{
            LogHelper.log(`selectedValues failed, no tag selected`);
        }

        return selectedValues;
    }

    //public functions
    on(evtType: 'selected' | 'removed' | 'focused' | 'blurred', listener: (sender: HTMLElement, msg: string) => any, scope?: any): void{
        this.m_handlers[evtType].push({'listener': listener, 'scope': scope});
    }

    off(evtType: 'selected' | 'removed' | 'focused' | 'blurred'): void{
        this.m_handlers[evtType].length = 0;
    }

    setSelectValues(values: string[]): void{

        if(!values){
            LogHelper.log(`setSelectValues failed, values array: ${values}`);
            return;
        }

        let scope: MultiSelector = this;
        let valuesArray: string[] = values.slice(0);
        let dropdownListEl: HTMLElement = scope.m_container ? <HTMLElement>scope.m_container.getElementsByClassName('dropdown-list')[0] : undefined;

        if(dropdownListEl){

            let curEl: HTMLElement = <HTMLElement>dropdownListEl.firstElementChild;

            while(curEl){
                if(curEl.nodeType === 1 && curEl.nodeName.toLowerCase() === 'div'){
                    let curDataValue: string = curEl.getAttribute('data-value');
                    let isMatched: number = valuesArray.indexOf(curDataValue);
                    if(isMatched > -1){
                        let evt: Event = Util.createEvent('click', true);
                        curEl.dispatchEvent(evt);
                    }
                }
                curEl = <HTMLElement>curEl.nextElementSibling;
            }
        }

    }

    //private functions
    private render(): void{
        let scope: MultiSelector = this;
        let mainWrapperEl: HTMLElement;
        let dropdownListEl: HTMLElement;

        scope.renderBasicStructure(scope.m_container);

        mainWrapperEl = <HTMLElement>scope.m_container.getElementsByClassName('main-wrapper')[0];
        dropdownListEl = mainWrapperEl ? <HTMLElement>mainWrapperEl.getElementsByClassName('dropdown-list')[0] : undefined;

        if(dropdownListEl){
            
            scope.renderDropDownList(dropdownListEl, scope.m_dataArray);

            mainWrapperEl.addEventListener('focus', function(evt: Event): any{
                scope.selectorFocusHandler.call(scope, evt);
            }, false);

            mainWrapperEl.addEventListener('blur', function(evt: Event): any{
                scope.selectorBlurHandler.call(scope, evt);
            }, false);

            dropdownListEl.addEventListener('click', function(evt: Event): any{
                scope.dropdownListItemClickHandler.call(scope, evt);
            }, false);
        }
    }

    private renderBasicStructure(container: HTMLElement): void{
        if(!container){
            return;
        }

        let mainWrapperEl: HTMLElement = document.createElement('div');
        let selectorWrapperEl: HTMLElement = document.createElement('div');
        let selectedTagWrapperEl: HTMLElement = document.createElement('div');
        let dropdownIconWrapperEl: HTMLElement = document.createElement('div');
        let dropDownIconEl: HTMLElement = document.createElement('span');
        let dropDownListEl: HTMLElement = document.createElement('div');

        mainWrapperEl.classList.add('main-wrapper');
        selectorWrapperEl.classList.add('selector-wrapper');
        selectedTagWrapperEl.classList.add('selected-tags-wrapper');
        dropdownIconWrapperEl.classList.add('tri-drop-icon-wrapper');
        dropdownIconWrapperEl.classList.add('dropdown-icon');
        dropDownIconEl.classList.add('tri-drop');
        dropDownListEl.classList.add('dropdown-list');

        mainWrapperEl.tabIndex = -1;

        dropdownIconWrapperEl.appendChild(dropDownIconEl);

        selectorWrapperEl.appendChild(selectedTagWrapperEl);
        selectorWrapperEl.appendChild(dropdownIconWrapperEl);

        mainWrapperEl.appendChild(selectorWrapperEl);
        mainWrapperEl.appendChild(dropDownListEl);

        container.appendChild(mainWrapperEl);
    }

    private renderDropDownList(dropdownListEl: HTMLElement, dataArray: string[]): void{
        
        let scope: MultiSelector = this;

        if(dropdownListEl && dataArray){
            Util.forEach(dataArray, function(tagValue: string): void{
                let dropdownItemEl: HTMLElement = scope.createDropdownItem(tagValue);
                if(dropdownItemEl){
                    dropdownListEl.appendChild(dropdownItemEl);
                }
            }, scope);
        }
        else{
            LogHelper.log(`renderDropDownList failed, dropdownList: ${dropdownListEl}, dataArray: ${dataArray}`);
        }
    }

    private createSelectedTag(tagValue: string): HTMLElement{

        let scope: MultiSelector = this;
        let tagEl: HTMLElement = document.createElement('div');
        let iconWrapperEl: HTMLElement = document.createElement('div');
        let deleteEl: HTMLElement = document.createElement('span');

        tagEl.classList.add('selected-tag')
        tagEl.textContent = tagValue;
        tagEl.setAttribute('data-value', tagValue);

        deleteEl.classList.add('delete');
        iconWrapperEl.classList.add('tag-delete-icon-wrapper');//need reconsidering
        iconWrapperEl.addEventListener('click', function(evt: Event): any{
            scope.deleteIconClickHandler.call(scope, evt);
        }, false);

        iconWrapperEl.appendChild(deleteEl);
        tagEl.appendChild(iconWrapperEl);
        return tagEl;
    }

    private createDropdownItem(tagValue: string): HTMLElement{
        let dropdownItemEl: HTMLElement = document.createElement('div');
        dropdownItemEl.classList.add('dropdown-item')
        dropdownItemEl.textContent = tagValue;
        dropdownItemEl.setAttribute('data-value', tagValue);
        return dropdownItemEl;
    }

    //event handlers
    private selectorFocusHandler(this: MultiSelector, evt: Event): any{
        
        let scope: MultiSelector = this;
        let mainWrapperEl: HTMLElement = evt ? <HTMLElement>evt.currentTarget : undefined;
        let dropDownIconWrapperEl: HTMLElement = mainWrapperEl ? <HTMLElement>mainWrapperEl.getElementsByClassName('tri-drop-icon-wrapper')[0] : undefined;
        let dropdownListEl: HTMLElement = mainWrapperEl ? <HTMLElement>mainWrapperEl.getElementsByClassName('dropdown-list')[0] : undefined;

        if(scope.m_handlers[MultiSelector.SelectorFocusEvent] && scope.m_handlers[MultiSelector.SelectorFocusEvent].length > 0){
            Util.forEach(scope.m_handlers[MultiSelector.SelectorFocusEvent], function(callback: any): void{
                let listener: Function = <Function>callback['listener'];
                let context: any = callback['scope'];
                if(listener){
                    listener.call(context, mainWrapperEl, 'focused');
                }
            }, scope);
        }
    }

    private selectorBlurHandler(this: MultiSelector, evt: Event): any{

        let scope: MultiSelector = this;
        let mainWrapperEl: HTMLElement = evt ? <HTMLElement>evt.currentTarget : undefined;
        let dropDownIconWrapperEl: HTMLElement = mainWrapperEl ? <HTMLElement>mainWrapperEl.getElementsByClassName('tri-drop-icon-wrapper')[0] : undefined;
        let dropdownListEl: HTMLElement = mainWrapperEl ? <HTMLElement>mainWrapperEl.getElementsByClassName('dropdown-list')[0] : undefined;

        if(scope.m_handlers[MultiSelector.SelectorBlurEvent] && scope.m_handlers[MultiSelector.SelectorBlurEvent].length > 0){
            Util.forEach(scope.m_handlers[MultiSelector.SelectorBlurEvent], function(callback: any): void{
                let listener: Function = <Function>callback['listener'];
                let context: any = callback['scope'];
                if(listener){
                    listener.call(context, mainWrapperEl, 'blurred');
                }
            }, scope);
        }
    }

    private dropdownListItemClickHandler(this: MultiSelector, evt: Event): any{
        
        let scope: MultiSelector = this;
        let curItemEl: HTMLElement = evt ? <HTMLElement>evt.target : undefined;
        let dropdownListEl: HTMLElement = curItemEl ? curItemEl.parentElement : undefined;
        let mainWrapperEl: HTMLElement = dropdownListEl ? dropdownListEl.parentElement : undefined;
        let selectedTagWrapperEl: HTMLElement = mainWrapperEl ? <HTMLElement>mainWrapperEl.getElementsByClassName('selected-tags-wrapper')[0] : undefined;
        
        if(curItemEl && selectedTagWrapperEl){
            let curValue: string = curItemEl.getAttribute('data-value');
            let newSelectedTag: HTMLElement = scope.createSelectedTag(curValue);

            selectedTagWrapperEl.appendChild(newSelectedTag);
            curItemEl.classList.add('dropdown-hide');

            if(scope.m_handlers[MultiSelector.ItemSelectedEvent] && scope.m_handlers[MultiSelector.ItemSelectedEvent].length > 0){
                Util.forEach(scope.m_handlers[MultiSelector.ItemSelectedEvent], function(callback: any): void{
                    let listener: Function = <Function>callback['listener'];
                    let context: any = callback['scope'];
                    if(listener){
                        listener.call(context, curItemEl, curValue);
                    }
                }, scope);
            }
        }

        evt.stopPropagation();
    }

    private deleteIconClickHandler(this: MultiSelector, evt: Event): any{

        let scope: MultiSelector = this;
        let deleteIconEl: HTMLElement = evt ? <HTMLElement>evt.currentTarget : undefined;
        let selectedTagEl: HTMLElement = deleteIconEl ? deleteIconEl.parentElement : undefined;
        let selectedTagWrapperEl: HTMLElement = selectedTagEl ? selectedTagEl.parentElement : undefined;
        let mainWrapperEl: HTMLElement = <HTMLElement>document.getElementsByClassName('main-wrapper')[0];
        let dropDownListEl: HTMLElement = mainWrapperEl ? <HTMLElement>mainWrapperEl.getElementsByClassName('dropdown-list')[0] : undefined;

        let deletedValue: string = selectedTagEl ? selectedTagEl.getAttribute('data-value') : '';

        if(selectedTagWrapperEl){
            selectedTagWrapperEl.removeChild(selectedTagEl);

            if(scope.m_handlers[MultiSelector.ItemRemovedEvent] && scope.m_handlers[MultiSelector.ItemRemovedEvent].length > 0){
                Util.forEach(scope.m_handlers[MultiSelector.ItemRemovedEvent], function(callback: any): void{
                    let listener: Function = <Function>callback['listener'];
                    let context: any = callback['scope'];
                    if(listener){
                        listener.call(context, selectedTagEl, deletedValue);
                    }
                }, scope);
            }
        }

        if(dropDownListEl){
            Util.forEach(dropDownListEl.children, function(listItemEl: HTMLElement): void{
                if(listItemEl && listItemEl.nodeType === 1){
                    if(listItemEl.getAttribute('data-value') === deletedValue){
                        listItemEl.classList.remove('dropdown-hide');
                        return;
                    }
                }
            }, scope);
        }        

    }
}

