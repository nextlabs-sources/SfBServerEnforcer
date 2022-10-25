import { IDataSourceFormat, XmlDataSourceFormat } from '../linakge-menu';
import { LinkageData } from '../entity';
import { TagHelper, HtmlHelper, LogHelper } from '../helper';
import { Util } from '../util';

export class TreeView{

    //memebers
    private m_showWidget: boolean;
    private m_formatter: IDataSourceFormat;
    private m_data: string;
    private m_formatData: LinkageData;
    private m_container: HTMLElement;
    private m_handlers: any;
    private m_lastClickedWidget: HTMLElement;

    //static memebers
    static selectEvent: string = 'select';
    static expandEvent: string = 'expand';
    static addEvent: string = 'add';
    static deleteEvent: string = 'delete';
    static modifyEvent: string = 'modify';

    //attribute definitions
    static State: string = "state";
    static TAG_NAME: string = "tagname";
    static TAG_VALUE: string = "tagvalues";
    static DEFAULT_VALUE: string = "defaultvalue";
    static EDITABLE: string = "editable";
    static MULTI_SELECT: string = "multiselect";
    static MANDATORY: string = "mandatory";
    static RELY_ON: string = 'relyon';

    //info on hover
    private m_add_info: string = 'add a sub-node';
    private m_delete_info: string = 'delete current node and its sub-nodes';
    private m_modify_info: string = 'modify current node';

    //.ctor
    constructor(data:string, container: HTMLElement, showWidget: boolean = false){
        this.m_showWidget = showWidget;
        this.m_formatter = new XmlDataSourceFormat();
        this.m_data = data;
        this.m_container = container;
        this.m_formatData = this.m_formatter.Format(data);
        this.m_handlers = {};
        this.render(this);
    }

    //public methods
    on(evtType: 'select'|'expand'|'add'|'delete'|'modify', listener: (this: any, sender: HTMLElement, args: string) => void): void{
        if(evtType && listener){
            this.m_handlers[evtType] = listener;
        }
    }

    off(evtType: 'select'|'expand'|'add'|'delete'|'modify'): void{
        if(evtType){
            if(this.m_handlers[evtType]){
                delete this.m_handlers[evtType];
            }
        }
    }

    unfolderClickHandler(this: TreeView, evt: Event): any{

        let scope: TreeView = this;
        let curIconEl: HTMLElement = <HTMLElement>evt.currentTarget;
        let hiddenWrapperEl: HTMLElement = curIconEl ? <HTMLElement>curIconEl.previousElementSibling : undefined;
        let isExpand: string = curIconEl ? curIconEl.getAttribute('data-expand') : '';

        if(scope.m_lastClickedWidget){
            //alert(`curWidget: ${scope.m_curWidget.getAttribute('data-expand')}`);
            let lastHiddenWrapperEl: HTMLElement = <HTMLElement>scope.m_lastClickedWidget.previousElementSibling;
            if(lastHiddenWrapperEl){
                lastHiddenWrapperEl.classList.add('hidden');
                scope.m_lastClickedWidget.setAttribute('data-expand', 'false');
            }
        }

        scope.m_lastClickedWidget = curIconEl;//alert(`hidden: ${scope.m_curWidget.getAttribute('data-expand')}`);

        if(hiddenWrapperEl){
            if(isExpand === 'true'){
                hiddenWrapperEl.classList.add('hidden');
                curIconEl.setAttribute('data-expand', 'false');
            }
            else{
                hiddenWrapperEl.classList.remove('hidden');
                curIconEl.setAttribute('data-expand', 'true');
            }
        }
        else{
            alert('hiddenWrapper is null');
        }
        evt.stopPropagation();
    }

    //private methods
    private render(scope: TreeView): void{
        let treeNodes: HTMLDivElement[] = [];

        if(scope.m_formatData.NodeName === TagHelper.CLASSIFICATION_NODE_NAME){
            Util.forEach(scope.m_formatData.ChildNodes, function(tag: LinkageData){
                treeNodes.push(scope.renderNode(tag));
            }, scope);
        }

        Util.forEach(treeNodes, function(el: HTMLElement){
            if(el){
                scope.m_container.appendChild(el);

                //add click handler for expand, select, add, delete, modify
                let treeExpandEls: HTMLCollectionOf<HTMLElement> = <HTMLCollectionOf<HTMLElement>>el.getElementsByClassName('tree-expand');
                let treeTextEls: HTMLCollectionOf<HTMLElement> = <HTMLCollectionOf<HTMLElement>>el.getElementsByClassName('tree-text');
                let addEls: HTMLCollectionOf<HTMLElement> = <HTMLCollectionOf<HTMLElement>>el.getElementsByClassName('plus');
                let crossEls: HTMLCollectionOf<HTMLElement> = <HTMLCollectionOf<HTMLElement>>el.getElementsByClassName('red-cross');
                let editEls: HTMLCollectionOf<HTMLElement> = <HTMLCollectionOf<HTMLElement>>el.getElementsByClassName('edit');
                let unfolderEls: HTMLCollectionOf<HTMLElement> = <HTMLCollectionOf<HTMLElement>>el.getElementsByClassName('unfolder');

                if(treeExpandEls){
                    Util.forEach(treeExpandEls, function(el: HTMLElement){
                        if(el){
                            el.addEventListener('click', function(evt: Event){
                                scope.expandClickHandler.call(scope, evt);
                            }, false);
                        }
                    }, scope);
                }

                if(treeTextEls){
                    Util.forEach(treeTextEls, function(el: HTMLElement){
                        if(el){
                            el.addEventListener('click', function(evt: Event){
                                scope.textClickHandler.call(scope, evt);
                            }, false);
                        }
                    }, scope);
                }

                if(addEls){
                    Util.forEach(addEls, function(el: HTMLElement){
                        let iconWrapperEl: HTMLElement = el ? el.parentElement : undefined;
                        if(iconWrapperEl){
                            iconWrapperEl.addEventListener('click', function(evt: Event){
                                scope.addClickHandler.call(scope, evt);
                            }, false);
                        }
                    }, scope);
                }

                if(crossEls){
                    Util.forEach(crossEls, function(el: HTMLElement){
                        let iconWrapperEl: HTMLElement = el ? el.parentElement : undefined;
                        if(iconWrapperEl){
                            iconWrapperEl.addEventListener('click', function(evt: Event){
                                scope.deleteClickHandler.call(scope, evt);
                            }, false);
                        }
                    }, scope);
                }

                if(editEls){
                    Util.forEach(editEls, function(el: HTMLElement){
                        let iconWrapperEl: HTMLElement = el ? el.parentElement : undefined;                        
                        if(iconWrapperEl){
                            iconWrapperEl.addEventListener('click', function(evt: Event){
                                scope.modifyClickHandler.call(scope, evt);
                            }, false);
                        }
                    }, scope);
                }

                if(unfolderEls){
                    Util.forEach(unfolderEls, function(unfolderEl: HTMLElement){
                        let icon: HTMLElement = unfolderEl ? unfolderEl.parentElement : undefined;
                        if(icon){
                            icon.addEventListener('click', function(evt: Event): any{
                                scope.unfolderClickHandler.call(scope, evt);
                            }, false);
                        }
                    }, scope);
                }
            }
        }, scope);
    }

    private renderNode(tag: LinkageData): HTMLDivElement{
        
        let scope: TreeView = this;
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

        //render current node and sub nodes
        if(tag && tag.NameAttr){
            treeExpand.textContent = '+';
            treeExpand.setAttribute('data-expand', 'false');
            
            treeText.textContent = tag.NameAttr;
            treeText.setAttribute('data-item', this.parseTagToJSON(tag));

            if(!scope.m_showWidget){
                treeWidget.classList.add('hidden');
            }
            else{
                scope.buildWidgets(treeWidget);
            }

            if(tag.ChildNodes && tag.ChildNodes.length > 0){
                let treeChildNode: HTMLDivElement = document.createElement('div');
                treeChildNode.className = 'tree-child-node hidden';
                Util.forEach(tag.ChildNodes, function(subTag: LinkageData){
                    treeChildNode.appendChild(scope.renderNode(subTag));
                }, scope);

                treeNode.appendChild(treeChildNode);
            }
            else{
                treeExpand.classList.add('hidden');
            }
        }
        else{

        }

        return treeNode;
    }

    private parseTagToJSON(tag: LinkageData): string{
        
        let result: string = '';
        let jsonObj: any = {};

        jsonObj[TreeView.TAG_NAME] = tag.NameAttr;
        jsonObj[TreeView.TAG_VALUE] = tag.ValuesAttr;
        jsonObj[TreeView.DEFAULT_VALUE] = tag.DefaultAttr;
        jsonObj[TreeView.EDITABLE] = tag.EditableAttr;
        jsonObj[TreeView.RELY_ON] = tag.RelyOnAttr;

        try{
            result = JSON.stringify(jsonObj);
        }
        catch(e){
            LogHelper.log(`parseTagToJSON failed, json object: ${jsonObj}`);
        }

        return result;
    }

    private buildWidgets(widgetContainer: HTMLElement): void{

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
        unfolderEl.className = 'icon unfolder';

        plusWrapper.className = 'icon-wrapper';
        crossWrapper.className = 'icon-wrapper';
        hamburgerWrapper.className = 'icon-wrapper';
        hiddenWrapperEl.className = 'hidden-wrapper hidden';
        unfolderWrapper.className = 'icon-wrapper';

        plusWrapper.title = this.m_add_info;
        crossWrapper.title = this.m_delete_info;
        hamburgerWrapper.title = this.m_modify_info;

        unfolderWrapper.setAttribute('data-expand', 'false');

        plusWrapper.appendChild(plusEl);
        crossWrapper.appendChild(crossEl);
        hamburgerWrapper.appendChild(hamburgerEl);

        hiddenWrapperEl.appendChild(plusWrapper);
        hiddenWrapperEl.appendChild(crossWrapper);
        hiddenWrapperEl.appendChild(hamburgerWrapper);
        unfolderWrapper.appendChild(unfolderEl);

        widgetContainer.appendChild(hiddenWrapperEl);
        widgetContainer.appendChild(unfolderWrapper);
    }

    //event handlers
    private expandClickHandler(this:TreeView, evt: Event): any{
        
        if(!evt){
            return false;
        }
        
        let scope: TreeView = this;
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

            //emit expandEvent
            if(scope.m_handlers[TreeView.expandEvent]){
                scope.m_handlers[TreeView.expandEvent].call(scope, expandEl, 'expand click');
            }
        }

        evt.stopPropagation();
    }

    private textClickHandler(this: TreeView, evt: Event): any{
        
        if(!evt){
            return false;
        }

        let scope: TreeView = this;
        let treeTextEl: HTMLElement = <HTMLElement>evt.target;
        
        if(treeTextEl){
            let data: string = treeTextEl.getAttribute('data-item');

            //emit selectEvent
            if(scope.m_handlers[TreeView.selectEvent])
            {
                scope.m_handlers[TreeView.selectEvent].call(scope, treeTextEl, data);
            }
        }

        evt.stopPropagation();
    }

    private addClickHandler(this: TreeView, evt: Event): any{
        
        if(!evt){
            return false;
        }
        let scope: TreeView = this;
        let plusWrapperEl: HTMLElement = <HTMLElement>evt.currentTarget;
        let hiddenWrapperEl: HTMLElement = plusWrapperEl ? plusWrapperEl.parentElement : undefined;
        let treeWidgetEl: HTMLElement = hiddenWrapperEl ? hiddenWrapperEl.parentElement : undefined;
        let treeTextEl: HTMLElement = treeWidgetEl ? <HTMLElement>treeWidgetEl.previousElementSibling : undefined;

        if(treeTextEl){
            let data: string = treeTextEl.getAttribute('data-item');
            if(scope.m_handlers[TreeView.addEvent]){
                scope.m_handlers[TreeView.addEvent].call(scope, treeTextEl, data);
            }
        }

        evt.stopPropagation();
    }

    private deleteClickHandler(this: TreeView, evt: Event): any{

        if(!evt){
            return false;
        }
        let scope: TreeView = this;
        let deleteWrapperEl: HTMLElement = <HTMLElement>evt.currentTarget;
        let hiddenWrapperEl: HTMLElement = deleteWrapperEl ? deleteWrapperEl.parentElement : undefined;
        let treeWidgetEl: HTMLElement = hiddenWrapperEl ? hiddenWrapperEl.parentElement : undefined;
        let treeTextEl: HTMLElement = treeWidgetEl ? <HTMLElement>treeWidgetEl.previousElementSibling : undefined;

        if(treeTextEl){
            let data: string = treeTextEl.getAttribute('data-item');
            if(scope.m_handlers[TreeView.deleteEvent]){
                scope.m_handlers[TreeView.deleteEvent].call(scope, treeTextEl, data);
            }
        }

        evt.stopPropagation();

    }

    private modifyClickHandler(this: TreeView, evt: Event): any{

        if(!evt){
            return false;
        }
        let scope: TreeView = this;
        let hamburgerWrapperEl: HTMLElement = <HTMLElement>evt.currentTarget;
        let hiddenWrapperEl: HTMLElement = hamburgerWrapperEl ? hamburgerWrapperEl.parentElement : undefined;
        let treeWidgetEl: HTMLElement = hiddenWrapperEl ? hiddenWrapperEl.parentElement : undefined;
        let treeTextEl: HTMLElement = treeWidgetEl ? <HTMLElement>treeWidgetEl.previousElementSibling : undefined;

        if(treeTextEl){
            let data: string = treeTextEl.getAttribute('data-item');
            if(scope.m_handlers[TreeView.modifyEvent]){
                scope.m_handlers[TreeView.modifyEvent].call(scope, treeTextEl, data);
            }
        }

        evt.stopPropagation();

    }
}