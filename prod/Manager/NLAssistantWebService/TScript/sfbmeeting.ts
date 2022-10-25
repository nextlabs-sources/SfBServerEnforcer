import { MainViewModel, PolicyViewModel } from './mainviewmodel';
import { LogHelper, AjaxHelper, UrlHelper, XmlHelper, TagHelper, StringHelper, HtmlHelper } from '../../../../jsbin/Modules/helper';
import { Meeting } from '../../../../jsbin/Modules/entity';
import { DataModel, PolicyDataModel } from './datamodel';
import { MeetingUI, MeetingResultType, MeetingUrlKey, ResultCodeType, ResultCodeDisplayMsg, PolicyEnforcement, PolicyResultXmlDefition } from './meetingconstants';
import { ViewModelAdapter } from './viewmodeladapter';
import { IDataSourceFormat, IStyleModify, XmlDataSourceFormat, LinkageMenu } from '../../../../jsbin/Modules/linakge-menu';
import { Dictionary } from '../../../../jsbin/Modules/map';
import { SFBMeetingStyle } from './sfbmeetingstyle';
import { PolicyResult } from './policyresult';

class SFBMeeting {

    private mainViewModel: MainViewModel;
    private dataModel: DataModel;
    private adapter: ViewModelAdapter;
    private menu: LinkageMenu;
    private queryString: Dictionary;
    private formatter: IDataSourceFormat;
    private modifier: IStyleModify;
    private strSaveRequestXmlCopy: string = '';
    private policyViewModel: PolicyViewModel;
    private autoTagDictForPolicy: Dictionary;
    private originalTotalTagDict: Dictionary;
    private newTotalTagDict: Dictionary;
    private originalSelectedTags: Dictionary;
    private newSelectedTags: Dictionary;
    private originalClassifyOb: string;

    private sCmdTemplate: string = '<MeetingCommand OperationType="%0" SipUri="%1" Id="%2">' +
                                        '<ResultCode>%3</ResultCode>' +
                                        '<Classification>%4</Classification>' +
                                        '<Filters Interval="%5" ShowMeetings="%6"></Filters>' +
                                        '<Meetings>' +
                                            '<MeetingInfo Uri="%7" EntryInfo="%8" Creator="%9" CreateTime="%10">' +
                                            '<Tags>%11</Tags>' +
                                            '</MeetingInfo>' +
                                        '</Meetings>' +
                                    '</MeetingCommand>';


    constructor() {
        this.mainViewModel = new MainViewModel();
        this.policyViewModel = new PolicyViewModel();
        this.addWatchersToViewModel();
        this.adapter = new ViewModelAdapter();
        this.queryString = UrlHelper.parseQueryString(window.location.href);
        this.formatter = new XmlDataSourceFormat();
        this.modifier = new SFBMeetingStyle();

        this.addEventListenersForUI();
        this.initMeetingList();
    }

    //Initialization
    private addWatchersToViewModel(): void {
        this.mainViewModel.Watchers[MainViewModel.RESULT_CODE_PROP] = () => { this.resultCodeWatcher();};
        this.mainViewModel.Watchers[MainViewModel.DATE_SPAN_PROP] = () => { this.dateSpanWatcher(); };
        this.mainViewModel.Watchers[MainViewModel.SHOW_MEETING_OF_PROP] = () => { this.showMeetingsOfWatcher(); };
        this.mainViewModel.Watchers[MainViewModel.SEARCH_CONTENT_PROP] = () => { this.searchContentWatcher(); };
        this.mainViewModel.Watchers[MainViewModel.MEETINGS_PROP] = () => { this.meetingsWatcher(); };
        this.mainViewModel.Watchers[MainViewModel.CUR_MEETING_PROP] = () => { this.curMeetingWatcher(); };
        this.policyViewModel.Watchers[PolicyViewModel.RESULT_CODE_PROP] = () => { this.policyResultCodeWatcher(); };
        this.policyViewModel.Watchers[PolicyViewModel.POLICY_RESULTS_PROP] = () => { this.policyResultsWatcher(); };
        this.mainViewModel.Watchers[MainViewModel.RESULT_COUNT_PROP] = () => { this.resultCounterWatcher(); };
    }

    private addEventListenersForUI(): void {

        let searchBtn: HTMLInputElement = <HTMLInputElement>document.getElementById(MeetingUI.SEARCH_BTN);
        let saveBtn: HTMLInputElement = <HTMLInputElement>document.getElementById(MeetingUI.SAVE_BTN);
        let cancelBtn: HTMLInputElement = <HTMLInputElement>document.getElementById(MeetingUI.CANCEL_BTN);
        let dateSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(MeetingUI.DATE_SELECT);
        let showSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(MeetingUI.SHOW_MEETING_SELECT);
        let policyCancelBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(MeetingUI.POLICY_CANCEL_BTN);
        let policyContinueBtn: HTMLButtonElement = <HTMLButtonElement>document.getElementById(MeetingUI.POLICY_CONTINUE_BTN);

        searchBtn.addEventListener('click', (evt: Event) => { this.searchClickHandler(evt); }, false);
        saveBtn.addEventListener('click', (evt: Event) => { this.saveClickHandler(evt); }, false);
        cancelBtn.addEventListener('click', (evt: Event) => { this.cancelClickHandler(evt); }, false);
        dateSelect.addEventListener('change', (evt: Event) => { this.dateChangeHandler(evt); }, false);
        showSelect.addEventListener('change', (evt: Event) => { this.showChangeHandler(evt); }, false);
        policyCancelBtn.addEventListener('click', (evt: Event) => { this.policyCancelClickHandler(evt); }, false);
        policyContinueBtn.addEventListener('click', (evt: Event) => { this.policyContinueClickHandler(evt); });
    }


    //Event Handlers
    private searchClickHandler(evt: Event): void {

        let sReqXml: string = '';
        let sSipUri: string = this.queryString[MeetingUrlKey.USER_KEY];
        let sTokenId: string = this.queryString[MeetingUrlKey.ID_KEY];

        this.showLoadingAnimation();

        if (sSipUri && sTokenId ) {

            let sSearchUri: string = (<HTMLInputElement>document.getElementById(MeetingUI.MEETING_URI_INPUT)).value;
            
            if(UrlHelper.checkSipValidation(sSearchUri)){
                sReqXml = StringHelper.cmdFormat(this.sCmdTemplate, MeetingResultType.SEARCH_TYPE, sSipUri, sTokenId, '', '', '', '', sSearchUri, '', '', '', '');
            }
            else if(UrlHelper.checkHttpValidation(sSearchUri)){
                sReqXml = StringHelper.cmdFormat(this.sCmdTemplate, MeetingResultType.SEARCH_TYPE, sSipUri, sTokenId, '', '', '', '', '', sSearchUri, '', '', '');
            }
            else{
                this.hideLoadingAnimation();
                this.showPrompt(ResultCodeType.FAILED);
                LogHelper.log(`input uri invalid, uri: ${sSearchUri}.`);
            }

            this.sendRequestXml(sReqXml);
        }
        else {
            LogHelper.log(`searchClickHandler() failed, sip: ${sSipUri}, token: ${sTokenId}.`);
            this.hideLoadingAnimation();
        }
    }

    private saveClickHandler(evt: Event): void {

        let sSipUri: string = this.queryString[MeetingUrlKey.USER_KEY];
        let sTokenId: string = this.queryString[MeetingUrlKey.ID_KEY];

        this.showLoadingAnimation();

        if (sSipUri && sTokenId) {

            let sMeetingUri: string = (<HTMLInputElement>document.getElementById(MeetingUI.MEETING_URI_OUTPUT)).value;

            if (this.menu) {
                let tagDict: Dictionary = this.menu.getSelectTagDict();
                let sTagXml: string = TagHelper.buildRequestTagsXmlString(TagHelper.TAG_NODE, TagHelper.MEETING_TAG_NAME_ATTR, TagHelper.MEETING_TAG_VALUE_ATTR, tagDict);
                let sReqXml: string = StringHelper.cmdFormat(this.sCmdTemplate, MeetingResultType.SAVE_TYPE, sSipUri, sTokenId, '', '', '', '', sMeetingUri, '', '', '', sTagXml);
                this.strSaveRequestXmlCopy = sReqXml;
                this.newSelectedTags = tagDict;

                //build request xml for querying policy
                let mergedDict: Dictionary = this.mergeDictionary(this.autoTagDictForPolicy, tagDict);
                
                //save new tags for displaying in policy prompt
                this.newTotalTagDict = mergedDict;

                let sTagXmlWithAutoTag: string = TagHelper.buildRequestTagsXmlString(TagHelper.TAG_NODE, TagHelper.MEETING_TAG_NAME_ATTR, TagHelper.MEETING_TAG_VALUE_ATTR, mergedDict);
                let sReqXmlWithAutoTag: string = StringHelper.cmdFormat(this.sCmdTemplate, MeetingResultType.SAVE_TYPE, sSipUri, sTokenId, '', '', '', '', sMeetingUri, '', '', '', sTagXmlWithAutoTag)
                this.sendParticipantRecheckRequest(sReqXmlWithAutoTag);
                this.mainViewModel.CurMeeting = null;
                this.autoTagDictForPolicy = null;
            }
        }
        else {
            LogHelper.log(`saveClickHandler() failed, sip: ${sSipUri}, token: ${sTokenId}.`);
            this.hideLoadingAnimation();
        }
    }

    private cancelClickHandler(evt: Event): void {
        this.mainViewModel.CurMeeting = null;
    }

    private classifyClickHandler(evt: Event): void {

        let sSipUri: string = this.queryString[MeetingUrlKey.USER_KEY];
        let sTokenId: string = this.queryString[MeetingUrlKey.ID_KEY];

        this.showLoadingAnimation();

        if (sSipUri && sTokenId) {

            if (evt && evt.srcElement) {
                let classifyBtn: HTMLInputElement = <HTMLInputElement>evt.srcElement;
                let sMeetingUri: string = classifyBtn.getAttribute('uri');
                if (sMeetingUri) {

                    let sReqXml: string = StringHelper.cmdFormat(this.sCmdTemplate, MeetingResultType.SEARCH_TYPE, sSipUri, sTokenId, '', '', '', '', sMeetingUri, '', '', '', '');

                    this.sendRequestXml(sReqXml);
                }
            }
            else {
                LogHelper.log(`classifyClickHandler() failed, evt: ${evt}.`);
                this.hideLoadingAnimation();
            }
        }
        else {
            LogHelper.log(`classifyClickHandler() failed, sip: ${sSipUri}, token: ${sTokenId}.`);
            this.hideLoadingAnimation();
        }
    }

    private dateChangeHandler(evt: Event): void {

        let sSipUri: string = this.queryString[MeetingUrlKey.USER_KEY];
        let sTokenId: string = this.queryString[MeetingUrlKey.ID_KEY];

        this.showLoadingAnimation();        

        if (sSipUri && sTokenId) {

            let dateSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(MeetingUI.DATE_SELECT);
            let showSelect: HTMLSelectElement = <HTMLSelectElement>document.getElementById(MeetingUI.SHOW_MEETING_SELECT);

            let sInterval: string = (<HTMLOptionElement>dateSelect.options[dateSelect.selectedIndex]).value;
            let sShowMeeting: string = (<HTMLOptionElement>showSelect.options[showSelect.selectedIndex]).value;

            let sReqXml: string = StringHelper.cmdFormat(this.sCmdTemplate, MeetingResultType.SHOW_TYPE, sSipUri, sTokenId, '', '', sInterval, sShowMeeting, '', '', '', '', '');

            this.sendRequestXml(sReqXml);
        }
        else {
            LogHelper.log(`dateChangeHandler() failed, sip: ${sSipUri}, token: ${sTokenId}.`);
            this.hideLoadingAnimation();
        }
    }

    private showChangeHandler(evt: Event): void {
        this.dateChangeHandler(evt);
    }

    private policyCancelClickHandler(evt: Event){
        this.policyViewModel.PolicyResults = null;
    }

    private policyContinueClickHandler(evt: Event){
        if(this.strSaveRequestXmlCopy){
            this.sendRequestXml(this.strSaveRequestXmlCopy);
            this.strSaveRequestXmlCopy = '';
        }
        else{
            LogHelper.log(`policyContinueClickHandler failed, RequestXml: ${this.strSaveRequestXmlCopy}`);
        }
        this.policyViewModel.PolicyResults = null;
    }


	//UI watchers
    private resultCodeWatcher(): void {

        let resultDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.RESULT_WRAPPER);

        if (resultDiv && this.dataModel) {
            this.showPrompt(this.dataModel.ResultCode);
        }
        else {
            LogHelper.log(`ResultCodeWatcher() failed, resultDivElement: ${resultDiv}, dataModel: ${this.dataModel}.`);
        }
    }

    private dateSpanWatcher(): void {

	}

    private showMeetingsOfWatcher(): void {

	}

    private searchContentWatcher(): void {

	}

    private meetingsWatcher(): void {

        let listTableBodyElement: HTMLTableSectionElement = <HTMLTableSectionElement>document.getElementById(MeetingUI.LIST_TABLE_BODY);

        if (listTableBodyElement) {
            listTableBodyElement.textContent = '';
            this.buildMeetingList(this.mainViewModel.Meetings);
        }
        else {
            LogHelper.log(`MeetingsWatcher() failed, listTableBody: ${listTableBodyElement}.`);
        }
    }

    private curMeetingWatcher(): void {

        let autoTagDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.AUTO_TAG_WRAPPER);
        let createTimeLabel: HTMLLabelElement = <HTMLLabelElement>document.getElementById(MeetingUI.CREATE_TIME_LABEL);
        let creatorLabel: HTMLLabelElement = <HTMLLabelElement>document.getElementById(MeetingUI.CREATOR_LABEL);
        let meetingUriOutput: HTMLInputElement = <HTMLInputElement>document.getElementById(MeetingUI.MEETING_URI_OUTPUT);
        let meetingEntryInfoLabel: HTMLLabelElement = <HTMLLabelElement>document.getElementById(MeetingUI.MEETING_ENTRY_LABEL); 
        let meetingEntryInfoOutputl: HTMLInputElement = <HTMLInputElement>document.getElementById(MeetingUI.MEETING_ENTRY_OUTPUT); 
        let linkageMenuDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.LINKAGE_MENU_WRAPPER);
        let classificationDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.CLASSIFICATION_WRAPPER);
        let classificatioMaskDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.CLASSIFICATION_MASK);

        if (this.mainViewModel.CurMeeting) {

            classificatioMaskDiv.style.display = 'block';
            classificationDiv.style.display = 'block';
            createTimeLabel.textContent = this.mainViewModel.CurMeeting.CreateTime;
            creatorLabel.textContent = this.mainViewModel.CurMeeting.Creator;
            meetingUriOutput.value = this.mainViewModel.CurMeeting.Uri;

            //hide entry info elements if it's null/undefined/empty.
            if (this.mainViewModel.CurMeeting.EntryInfo) {
                meetingEntryInfoOutputl.value = this.mainViewModel.CurMeeting.EntryInfo;
                meetingEntryInfoOutputl.style.display = 'block';
                meetingEntryInfoLabel.style.display = 'block';
            }
            else {
                meetingEntryInfoLabel.style.display = 'none';
                meetingEntryInfoOutputl.style.display = 'none';
            }

            let totalDict: Dictionary = this.mainViewModel.CurMeeting.Tags;
            let obligationDict: Dictionary = TagHelper.getTagsFromObligation(this.mainViewModel.CurMeeting.ClassifyOb);
            let autoTagDict: Dictionary = TagHelper.getTagsFromAutoClassification(totalDict, obligationDict);

            //save auto tag dictionary for querying policy
            this.autoTagDictForPolicy = autoTagDict;

            //save original tags for displaying policy prompt
            this.originalTotalTagDict = totalDict;

            this.originalSelectedTags = TagHelper.getSelectedTags(totalDict, autoTagDict);

            autoTagDiv.textContent = '';
            this.buildAutoTagDisplayHTML(autoTagDict);

            this.menu = null;
            this.menu = new LinkageMenu(this.modifier, this.formatter, linkageMenuDiv, this.mainViewModel.CurMeeting.ClassifyOb);
            this.menu.bind();
            this.menu.setSelectTagDict(linkageMenuDiv, totalDict);

            this.originalClassifyOb = this.mainViewModel.CurMeeting.ClassifyOb;
        }
        //clear contents of elements.
        else {
            classificationDiv.style.display = 'none';
            classificatioMaskDiv.style.display = 'none';
            autoTagDiv.textContent = '';
            createTimeLabel.textContent = '';
            creatorLabel.textContent = '';
            meetingUriOutput.value = '';
            meetingEntryInfoOutputl.value = '';
            linkageMenuDiv.textContent = '';
        }
    }

    private policyResultsWatcher(): void{
        let policyWrapperDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.POLICY_WRAPPER);
        let policyResultList: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.POLICY_RESULT_LIST);
        let classificatioMaskDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.CLASSIFICATION_MASK);
        let participantStringArray: string[] = [];

        policyResultList.textContent = '';

        if(this.policyViewModel.PolicyResults && this.policyViewModel.PolicyResults.length > 0){
            classificatioMaskDiv.style.display = 'block';
            policyWrapperDiv.style.display = 'block';
            let szPolicyResults: Array<PolicyResult> = this.policyViewModel.PolicyResults;
            for(let i:number = 0; i < szPolicyResults.length; i++){
                //policyResultList.appendChild(this.buildPolicyResultRow(szPolicyResults[i].Participant));
                participantStringArray.push(szPolicyResults[i].Participant);
            }
            policyResultList.appendChild(this.buildPolicyResultRow(participantStringArray.join('; ')));
        }
        else{
            //LogHelper.log(`policyResultsWatcher failed, policy results: ${this.policyViewModel.PolicyResults}`);
            policyWrapperDiv.style.display = 'none';
            classificatioMaskDiv.style.display = 'none';
        }
    }

    private policyResultCodeWatcher(): void{
        let resultDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.RESULT_WRAPPER);

        if (resultDiv && this.policyViewModel.ResultCode) {
            //this.showPrompt(this.policyViewModel.ResultCode);
        }
        else {
            LogHelper.log(`ResultCodeWatcher() failed, resultDivElement: ${resultDiv}, resultCode: ${this.policyViewModel.ResultCode}.`);
        }        
    }

    private resultCounterWatcher(): void{
        let el: HTMLSpanElement = <HTMLSpanElement>document.getElementById(MeetingUI.MEETING_RESULT_COUNTER);
        el && (el.textContent = this.mainViewModel.ResultCount.toString());
    }


    //inner tools
    private setOption(_selectElement: HTMLSelectElement, _sValue: string): void {
        if (_selectElement && _sValue) {
            let szOptions: HTMLOptionsCollection = _selectElement.options;
            for (let i: number = 0; i < szOptions.length; i++) {
                if (szOptions[i]) {
                    let option: HTMLOptionElement = <HTMLOptionElement>szOptions[i];
                    if (option.value && (StringHelper.trim(option.value).toLowerCase() === StringHelper.trim(_sValue).toLowerCase())) {
                        _selectElement.selectedIndex = i;
                    }
                }
            }
        }
        else {
            LogHelper.log(`setOption() failed, selectElement: ${_selectElement}, value: ${_sValue}.`);
        }
    }

    private sendRequestXml(_sRequestXml: string): void {

        let xhr: XMLHttpRequest = AjaxHelper.getXhrInstance();

        if (xhr) {

            //xhr.overrideMimeType('text/xml');//can't override if xhr is loading or done.
            //xhr.addEventListener('readystatechange', (evt: Event) => { this.ajaxReadyStateChangeHandler(evt); }, true);//can hardly be removed.
            xhr.onreadystatechange = (evt: Event) => { this.ajaxReadyStateChangeHandler(evt); };//can be removed later.

            let sReqUrl: string = this.getMeetingHandlerRequestUrl();
            if (sReqUrl && _sRequestXml) {
                xhr.open('post', sReqUrl, true);
                xhr.send(_sRequestXml);
            }
            else {
                LogHelper.log(`sendRequstXml() failed, RequestUrl: ${sReqUrl}, RequestXml: ${_sRequestXml}.`);
            }
        }
        else {
            LogHelper.log(`sendRequestXml() failed, xhr: ${xhr}.`);
        }
    }

    private sendParticipantRecheckRequest(_sReqXml: string): void {
        let xhr:XMLHttpRequest = AjaxHelper.getXhrInstance();
        xhr.onreadystatechange = (evt:Event) => { this.participantRecheckAjaxReadyStateChangeHandler(evt); };
        if(xhr){
            let sReqUrl: string = this.getParticipantHandlerRequestUrl();
            xhr.open('post', sReqUrl, true);
            xhr.send(_sReqXml);
        }
        else{
            LogHelper.log(`sendParticipantRecheckRequest() failed, xhr: ${xhr}.`);
        }
    }

    private participantRecheckAjaxReadyStateChangeHandler(evt: Event): void {
        
        let xhr: XMLHttpRequest = evt ? <XMLHttpRequest>evt.currentTarget : undefined;

        if(xhr){
            if(xhr.readyState === AjaxHelper.DONE && xhr.status === 200){
                let xmlDoc: any = xhr.responseXML;
                if(xmlDoc){
                    let policyDataModel: PolicyDataModel = this.parsePolicyResponseXml(xmlDoc);
                    this.adapter.AdaptPolicyViewModel(policyDataModel, this.policyViewModel);
                    if(!this.policyViewModel.PolicyResults || this.policyViewModel.PolicyResults.length === 0){
                        if(this.strSaveRequestXmlCopy){
                            this.sendRequestXml(this.strSaveRequestXmlCopy);
                            this.strSaveRequestXmlCopy = '';
                        }
                        else{
                            LogHelper.log(`policyContinueClickHandler failed, RequestXml: ${this.strSaveRequestXmlCopy}`);
                        }
                    }
                    else{
                        this.hideLoadingAnimation();
                        //this.buildOriginalTagList(this.originalTotalTagDict);
                        //this.buildNewTagList(this.newTotalTagDict);

                        //if the meeting hasn't been classified yet, leave the old linkage menu blank
                        if(Object.keys(this.originalTotalTagDict).length) {
                            this.buildOldLinkMenu(this.originalSelectedTags);
                        }
                        else {
                            let wrap = document.getElementById(MeetingUI.POLICY_OLD_LINKAGEMENU);
                            wrap.textContent = '';
                        }

                        this.buildNewLinkMenu(this.newSelectedTags);
                        this.originalTotalTagDict = null;
                        this.newTotalTagDict = null;
                    }
                    for(let i: number = 0; i < policyDataModel.PolicyResults.length; i++){
                        LogHelper.log(`Enforcement: ${policyDataModel.PolicyResults[i].Enforcement}, Participant: ${policyDataModel.PolicyResults[i].Participant}`);
                    }
                }
                else{
                    LogHelper.log(`participantRecheckAjaxReadyStateChangeHandler() failed, responseXML: ${xmlDoc}`);
                }

                xhr.onreadystatechange = null;
            }
        }
        else{
            LogHelper.log(`participantRecheckAjaxReadyStateChangeHandler() failed, xhr: ${xhr}.`);
        }
    }

    private ajaxReadyStateChangeHandler(evt: Event): void {

        let xhr: XMLHttpRequest = evt ? <XMLHttpRequest>evt.currentTarget : undefined;

        if (xhr) {
            if (xhr.readyState === AjaxHelper.DONE && xhr.status === 200) {

                this.hideLoadingAnimation();
                let xmlDoc: any = xhr.responseXML;

                if (xmlDoc) {
                    this.dataModel = this.parseResponseXml(xmlDoc);
                    this.adapter.Adapt(this.dataModel, this.mainViewModel);
                }
                else {
                    LogHelper.log(`ajaxReadyStateChangeHandler() failed, xmlDoc: ${xmlDoc}.`);
                }

                xhr.onreadystatechange = null;//remove listener to ensure the xhr reusable.

                //refresh if tags update.
                //must be placed after xhr.onreadystatechange = null to prevent the new xhr.readystate handler from being disposed.
                if (this.dataModel.OperationType === MeetingResultType.SAVE_TYPE) {
                    this.refreshMeetingList();
                }
            }
        }
        else {
            LogHelper.log(`ajaxReadyStateChangeHandler() failed, xhr: ${xhr}.`);
        }
    }

    private parseResponseXml(_xmlDoc: any): DataModel {

        let dataModel: DataModel = new DataModel();

        if (_xmlDoc) {

            let docElement: any = _xmlDoc.documentElement;

            if (docElement) {
                dataModel.OperationType = <string>docElement.getAttribute(TagHelper.OPERATION_TYPE_ATTR);
                dataModel.SipUri = <string>docElement.getAttribute(TagHelper.SIP_URI_ATTR);
                dataModel.TokenId = <string>docElement.getAttribute(TagHelper.TOKEN_ID_ATTR);

                let resultCodeNode: any = docElement.getElementsByTagName(TagHelper.RESULT_CODE_NODE)[0];
                let filtersNode: any = docElement.getElementsByTagName(TagHelper.FILTER_NODE)[0];
                let meetingNodes: Array<any> = docElement.getElementsByTagName(TagHelper.MEETING_INFO_NODE);

                dataModel.ResultCode = resultCodeNode.textContent;
                dataModel.DateSpan = filtersNode.getAttribute(TagHelper.INTERVAL_ATTR);
                dataModel.ShowMeetingOf = filtersNode.getAttribute(TagHelper.SHOW_MEETINGS_ATTR);

                for (let i: number = 0; i < meetingNodes.length; i++) {
                    let meeting: Meeting = new Meeting();

                    meeting.Uri = meetingNodes[i].getAttribute(TagHelper.MEETING_URI_ATTR);
                    meeting.EntryInfo = meetingNodes[i].getAttribute(TagHelper.MEETING_ENTRY_INFO_ATTR);
                    meeting.Creator = meetingNodes[i].getAttribute(TagHelper.MEETING_CREATOR_ATTR);
                    meeting.CreateTime = meetingNodes[i].getAttribute(TagHelper.MEETING_CREATE_TIME);
                    meeting.ClassifyOb = meetingNodes[i].getElementsByTagName(TagHelper.CLASSIFICATION_NODE)[0].textContent;

                    let tags: Array<any> = meetingNodes[i].getElementsByTagName(TagHelper.TAG_NODE);
                    for (let j: number = 0; j < tags.length; j++) {
                        let sTagName: string = tags[j].getAttribute(TagHelper.MEETING_TAG_NAME_ATTR);
                        let sTagValue: string = tags[j].getAttribute(TagHelper.MEETING_TAG_VALUE_ATTR);
                        if (sTagName && (sTagValue !== null && sTagValue !== undefined)) {
                            meeting.Tags[StringHelper.trim(sTagName).toLowerCase()] = StringHelper.trim(sTagValue).toLowerCase();
                        }
                    }

                    this.decodeHtmlStringInDictionary(meeting.Tags);
                    dataModel.Meetings.push(meeting);
                }
            }
            else {
                LogHelper.log(`parseResponseXml() failed, documentElement: ${docElement}.`);
            }
        }
        else {
            LogHelper.log(`parseResponseXml() failed, responseXmlDoc: ${_xmlDoc}.`);
        }

        return dataModel;
    }

    private getMeetingHandlerRequestUrl(): string {

        let sReqUrl: string = this.getCurrentAppUrl();
        let sReqHandler: string = 'nxl.mh';

        if (sReqUrl) {
            sReqUrl = `${sReqUrl}/${sReqHandler}`;
            sReqUrl = UrlHelper.addTimeStamp(sReqUrl);
        }
        else {
            LogHelper.log(`getMeetingHandlerRequestUrl() failed, AppUrl: ${sReqUrl}.`);
        }

        return sReqUrl;
    }

    private getParticipantHandlerRequestUrl(): string{
        
        let sReqUrl: string = this.getCurrentAppUrl();
        let sReqHandler: string = 'nxl.ph';

        if (sReqUrl) {
            sReqUrl = `${sReqUrl}/${sReqHandler}`;
            sReqUrl = UrlHelper.addTimeStamp(sReqUrl);
        }
        else {
            LogHelper.log(`getMeetingHandlerRequestUrl() failed, AppUrl: ${sReqUrl}.`);
        }

        return sReqUrl;
    }

    private getCurrentAppUrl():string{
        
        let sReqUrl: string;
        let sPageFlag: string = '/Pages';
        let sPathName: string = window.location.href;
        let nPageFlagIndex: number = sPathName.indexOf(sPageFlag, 0);

        if (nPageFlagIndex > -1) {
            let sVirtualDir: string = sPathName.substring(0, nPageFlagIndex);
            sReqUrl = sVirtualDir;
        }
        else {
            LogHelper.log(`getAjaxRequestUrl() failed, nPageFlagIndex: ${nPageFlagIndex}.`);
        }

        return sReqUrl;
    }

    private showPrompt(_sResultCode: string): void {

        let resultDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.RESULT_WRAPPER);

        this.setPromptMsg(_sResultCode);

        if(_sResultCode !== ResultCodeType.SUCCEED){
            resultDiv.style.opacity = '1';
        }

        setTimeout(() => { this.hidePrompt(); }, 3000);
    }

    private hidePrompt():void{
        let resultDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.RESULT_WRAPPER);
        resultDiv.style.opacity = '0';
    }

    private setPromptMsg(_sResultCode:string):void{

        let resultDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.RESULT_WRAPPER);

        switch (_sResultCode) {
            case ResultCodeType.SUCCEED: {
                //LogHelper.log(`operation succeed.`);
                //resultDiv.textContent = ResultCodeDisplayMsg.SUCCEED;
                break;
            }
            case ResultCodeType.FAILED: {
                resultDiv.className = 'col-md-10 bg-danger';
                resultDiv.textContent = ResultCodeDisplayMsg.FAILED;
                break;
            }
            case ResultCodeType.UNAUTHORIZED: {
                resultDiv.className = 'col-md-10 bg-warning';
                resultDiv.textContent = ResultCodeDisplayMsg.UNAUTHORIZED;
                break;
            }
            case ResultCodeType.TOKEN_INVALID: {
                resultDiv.className = 'col-md-10 bg-warning';
                resultDiv.textContent = ResultCodeDisplayMsg.TOKEN_INVALID;
                break;
            }
            default: {
                LogHelper.log(`showPrompt() failed, resultCode: ${_sResultCode}.`);
                break;
            }
        }
    }

    private buildMeetingList(_meetings: Array<Meeting>): void {

        let listTableBody: HTMLTableSectionElement = <HTMLTableSectionElement>document.getElementById(MeetingUI.LIST_TABLE_BODY);

        if (listTableBody && _meetings) {
            if (_meetings.length > 0) {
                for (let i: number = 0; i < _meetings.length; i++) {
                    if(_meetings[i].Uri) {
                        this.buildMeetingTr(listTableBody, _meetings[i]);
                    }
                    else{
                        LogHelper.log(`invalid meeting uri: ${_meetings[i].Uri}`);
                    }
                }
            }
            else {
                this.buildNoDataTr(listTableBody);
            }
        }
        else {
            LogHelper.log(`buildMeetingList() failed, listTableBody: ${listTableBody}, meetings: ${_meetings}.`);
        }
    }

    private buildMeetingTr(_parentElement: HTMLElement, _meeting: Meeting):void {

        let tr: HTMLTableRowElement = document.createElement("tr");
        let uriTd: HTMLTableDataCellElement = document.createElement("td");
        let timeTd: HTMLTableDataCellElement = document.createElement("td");
        let tagTd: HTMLTableDataCellElement = document.createElement("td");
        let classifyTd: HTMLTableDataCellElement = document.createElement("td");
        let classifyBtn: HTMLAnchorElement = document.createElement("a");

        classifyBtn.type = "button";
        classifyBtn.textContent = (_meeting.Tags && Object.keys(_meeting.Tags).length) ? "Modify Classification" : "Classify";
        classifyBtn.name = "classifyBtn";
        classifyBtn.className = "btn btn-sm classify-btn";
        classifyBtn.setAttribute('uri', _meeting.Uri);

        classifyBtn.addEventListener('click', (evt: Event) => { this.classifyClickHandler(evt); }, false);

        uriTd.textContent = _meeting.Uri;
        timeTd.textContent = _meeting.CreateTime;
        tagTd.textContent = this.buildTagDisplayString(_meeting.Tags);

        classifyTd.appendChild(classifyBtn);
        tr.appendChild(classifyTd);
        tr.appendChild(timeTd);
        tr.appendChild(tagTd);
        tr.appendChild(uriTd);
        
        _parentElement.appendChild(tr);
    }

    private buildTagDisplayString(_tagDict: Dictionary): string {

        let sTag: string = '';

        if (_tagDict) {
            for (let key in _tagDict) {
                sTag += `${key}=${_tagDict[key]} `;
            }
        }
        else {
            LogHelper.log(`buildTagDisplayString() failed, tagDict: ${_tagDict}.`);
        }

        return sTag;
    }

    private buildNoDataTr(_parentElement: HTMLElement): void {

        let tr: HTMLTableRowElement = document.createElement("tr");
        let td: HTMLTableDataCellElement = document.createElement("td");
        td.style.height = "30px";
        td.style.lineHeight = "30px";
        td.style.verticalAlign = "middle";
        td.style.textAlign = "center";
        td.textContent = ResultCodeDisplayMsg.NO_DATA;
        td.colSpan = 4;
        tr.appendChild(td);
        _parentElement.appendChild(tr);
    }

    private showLoadingAnimation(): void {
        let loadingBarDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.LOADING_BAR_WRAPPER);
        loadingBarDiv.style.display = 'block';
    }

    private hideLoadingAnimation(): void {
        let loadingBarDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.LOADING_BAR_WRAPPER);
        loadingBarDiv.style.display = 'none';
    }

    private refreshMeetingList(): void {
        this.dateChangeHandler(null);
    }

    private buildAutoTagDisplayHTML(_autoTagDict:Dictionary):void{
        
        let autoTagDiv: HTMLDivElement = <HTMLDivElement>document.getElementById(MeetingUI.AUTO_TAG_WRAPPER);
        
        if(_autoTagDict){
            for(let key in _autoTagDict){
                autoTagDiv.appendChild(this.buildAutoTagItemTemplate(key, _autoTagDict[key]));
            }
        }
        else{
            LogHelper.log(`buildAutoTagDisplayHTML() failed, autoTagDict: ${_autoTagDict}.`);
        }
    }

    private buildAutoTagItemTemplate(_sName: string, _sValue:string): HTMLParagraphElement{
        
        if(_sName && (_sValue !== null && _sValue !== undefined)){
            let containerPara:HTMLParagraphElement = document.createElement("p");
            let nameSpan:HTMLSpanElement = document.createElement("span");
            let valueSpan:HTMLSpanElement = document.createElement("span");

            nameSpan.className = "auto-tagname-span";
            valueSpan.className = "auto-tagvalue-span";

            nameSpan.textContent = StringHelper.trim(_sName).toLowerCase();
            valueSpan.textContent = StringHelper.trim(_sValue).toLowerCase();

            containerPara.appendChild(nameSpan);
            containerPara.appendChild(valueSpan);

            return containerPara;
        }
        else{
            LogHelper.log(`buildAutoTagItemTemplate() failed, TagName: ${_sName}, TagValue: ${_sValue}.`);
        }
    }

    private watchMeetingUri(_queryString:Dictionary):void{
        
        if(_queryString){
            let sSipUri: string = this.queryString[MeetingUrlKey.USER_KEY];
            let sTokenId: string = this.queryString[MeetingUrlKey.ID_KEY];
            let sMeetingUri:string = this.queryString[MeetingUrlKey.MEETING_URI_KEY];

            if(sSipUri && sTokenId && sMeetingUri){

                let sReqXml: string = StringHelper.cmdFormat(this.sCmdTemplate, MeetingResultType.SEARCH_TYPE, sSipUri, sTokenId, '', '', '', '', sMeetingUri, '', '', '', '');

                this.sendRequestXml(sReqXml);
                this.showLoadingAnimation();
            }
        }
        else{
            LogHelper.log(`watchMeetingUri() failed, queryString: ${_queryString}.`);
        }
    }

    private initMeetingList():void{
        this.watchMeetingUri(this.queryString);
        this.dateChangeHandler(null);
    }

    private decodeHtmlStringInDictionary(_dict:Dictionary):void{

        if(_dict){
            for(let key in _dict){
                if(_dict.hasOwnProperty(key)){
                    _dict[HtmlHelper.htmlDecode(key)] = HtmlHelper.htmlDecode(_dict[key]);
                    if(HtmlHelper.htmlDecode(key) !== key){
                        delete _dict[key];
                    }
                }
            }
        }
        else{
            LogHelper.log(`decodeHtmlStringInDictionary() failed, dictionary: ${_dict}.`);
        }
    }

    private buildPolicyResultRow(_sParticipants: string): HTMLDivElement{
        
        let rowDiv: HTMLDivElement = document.createElement('div');
        rowDiv.className = 'policy-result-row';
        rowDiv.textContent = _sParticipants;

        return rowDiv;
    }

    private parsePolicyResponseXml(_xmlDoc: any): PolicyDataModel {
        let policyModel: PolicyDataModel = new PolicyDataModel();

        if(_xmlDoc){
            let docElement: any = _xmlDoc.documentElement;
            if(docElement){
                let resultCodeElement: any = docElement.getElementsByTagName(PolicyResultXmlDefition.RESULT_CODE_NODE_NAME)[0];
                let joinResultsElement: any = docElement.getElementsByTagName(PolicyResultXmlDefition.JOIN_RESULTS_NODE_NAME)[0];

                if(resultCodeElement){
                    policyModel.ResultCode = resultCodeElement.textContent;
                }

                if(joinResultsElement){
                    let resultElements: Array<any> = joinResultsElement.getElementsByTagName(PolicyResultXmlDefition.RESULT_NODE_NAME);
                    if(resultElements){
                        for(let i:number = 0; i < resultElements.length; i++){
                            let tempPolicyResult: PolicyResult = new PolicyResult();
                            tempPolicyResult.Enforcement = resultElements[i].getAttribute(PolicyResultXmlDefition.ENFORCEMENT_ATTR_NAME);
                            tempPolicyResult.Participant = resultElements[i].getAttribute(PolicyResultXmlDefition.PARTICIPANT_ATTR_NAME);
                            policyModel.PolicyResults.push(tempPolicyResult);
                        }
                    }
                    else{
                        LogHelper.log(`parsePolicyResponseXml failed, resultElements: ${resultElements}`);
                    }
                }
                else{
                    LogHelper.log(`parsePolicyResponseXml failed, joinResultElement: ${joinResultsElement}`);
                }
            }
            else{
                LogHelper.log(`parsePolicyResponseXml failed, docElement: ${docElement}`);
            }
        }
        else{
            LogHelper.log(`parsePolicyResponseXml failed, xmlDoc: ${_xmlDoc}`);
        }

        return policyModel;
    }
    
    private mergeDictionary(_autoTagDict: Dictionary, _manualTagDict: Dictionary): Dictionary{

        let mergedDict: Dictionary = new Dictionary();

        for(let autoTagName in _autoTagDict){
            if(_autoTagDict[autoTagName]){
                mergedDict[StringHelper.trim(autoTagName).toLowerCase()] = StringHelper.trim(_autoTagDict[autoTagName]).toLowerCase();
            }
        }

        for(let manualTagName in _manualTagDict){
            if(_manualTagDict[manualTagName]){
                mergedDict[StringHelper.trim(manualTagName).toLowerCase()] = StringHelper.trim(_manualTagDict[manualTagName]).toLowerCase();
            }
        }

        return mergedDict;
    }

    private buildNewTagList(_tagDict: Dictionary): void{

        let newTagList: HTMLUListElement = <HTMLUListElement>document.getElementById(MeetingUI.POLICY_NEW_TAG_LIST);

        if(newTagList){

            newTagList.textContent = '';

            if(_tagDict){
                for(let tagName in _tagDict){
                    let sFormatTag: string = this.formatPolicyTagDisplayString(tagName, _tagDict[tagName]);
                    newTagList.appendChild(this.buildTagListItem(sFormatTag, 'list-no-dot policy-tag-list-item'));
                }
            }
            else{
                LogHelper.log(`buildNewTagList failed, tagDict: ${_tagDict}`);
            }
        }
        else{
            LogHelper.log(`buildNewTagList failed, newTagList: ${newTagList}`);
        }
    }

    private buildOriginalTagList(_tagDict: Dictionary): void{

        let originalTagList: HTMLUListElement = <HTMLUListElement>document.getElementById(MeetingUI.POLICY_ORIGINAL_TAG_LIST);

        if(originalTagList){

            originalTagList.textContent = '';

            if(_tagDict){
                for(let tagName in _tagDict){
                    let sFormatTag: string = this.formatPolicyTagDisplayString(tagName, _tagDict[tagName]);
                    originalTagList.appendChild(this.buildTagListItem(sFormatTag, 'list-no-dot policy-tag-list-item'));
                }
            }
            else{
                LogHelper.log(`buildOriginalTagList failed, tagDict: ${_tagDict}`);
            }
        }
        else{
            LogHelper.log(`buildOriginalTagList failed, originalTagList: ${originalTagList}`);
        }
    }

    private buildTagListItem(_sTag: string, _sClassName: string): HTMLLIElement{
        let li:HTMLLIElement = document.createElement('li');

        li.className = _sClassName;
        li.textContent = _sTag;

        return li;
    }

    private formatPolicyTagDisplayString(_sTagName: string, _sTagValue: string): string{
        let sTag: string = '';

        sTag = `${_sTagName}: ${_sTagValue}`;

        return sTag;
    }

    private buildNewLinkMenu(newSelectedTagDict: Dictionary): void{
        let el = document.getElementById(MeetingUI.POLICY_NEW_LINKAGEMENU);
        el.textContent = '';

        let menu = new LinkageMenu(this.modifier, this.formatter, el, this.originalClassifyOb);
        menu.bind();
        menu.setSelectTagDict(el, newSelectedTagDict);
        this.disableAllSelectElement(el);
    }

    private buildOldLinkMenu(oldSelectedTagDict: Dictionary): void{
        let el = document.getElementById(MeetingUI.POLICY_OLD_LINKAGEMENU);
        el.textContent = '';

        let menu = new LinkageMenu(this.modifier, this.formatter, el, this.originalClassifyOb);
        menu.bind();
        menu.setSelectTagDict(el, oldSelectedTagDict);
        this.disableAllSelectElement(el);
    }

    private disableAllSelectElement(rootEl: HTMLElement): void{

        if(rootEl){
            const els: NodeListOf<HTMLSelectElement> = rootEl.getElementsByTagName('select');
            if(els && els.length > 0){
                for(let i = 0; i < els.length; i++){
                    const el = els.item(i);
                    el && (el.disabled = true);
                }
            }
        }
        else{
            LogHelper.log('disableAllSelectElement failed.');
        }
    }
}

let sfbMeeting: SFBMeeting = new SFBMeeting();
