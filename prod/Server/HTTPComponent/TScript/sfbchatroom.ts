///<reference path="mainform.d.ts"/>
import { StringHelper, XmlHelper, LogHelper, HtmlHelper, TagHelper } from '../../../../jsbin/Modules/helper';
import { NLCategory } from '../../../../jsbin/Modules/entity';
import { LinkageMenu, IDataSourceFormat, XmlDataSourceFormat, IStyleModify } from '../../../../jsbin/Modules/linakge-menu';
import { ChatRoomStyleModify } from './sfbchatroomstyle';
import { Dictionary } from '../../../../jsbin/Modules/map';


let _RMCommand_CreateRoomNL:string = "<RMCommand Type=\"CreateRoom\" RoomName=\"%0\" RoomDesc=\"%1\" "  +
                            "Privacy=\"%2\" AddIn=\"%3\" CategoryUri=\"%4\" Managers=\"%5\" " +
                            "Members=\"%6\" Notification=\"%7\">" +
                            "<NLEnforce NLNeedEnforce=\"%8\"><Tags>%9</Tags></NLEnforce>" +
                            "</RMCommand>";

let _RMCommand_ModifyRoomNL:string = "<RMCommand Type=\"ModifyRoom\" RoomName=\"%0\" RoomDesc=\"%1\" " +
                            "Privacy=\"%2\" AddIn=\"%3\" CategoryUri=\"%4\" Managers=\"%5\" " +
                            "Members=\"%6\" Notification=\"%7\" DeleteRoom=\"%8\" " +
                            "OldRoomName=\"%9\" RoomId=\"%10\" TimeStamp=\"%11\">" +
                            "<NLEnforce NLNeedEnforce=\"%12\"><Tags>%13</Tags></NLEnforce>" +
                            "</RMCommand>";

let W_Deny_Member_Text:string = " Following members are removed from member list by NextLabs Enforcer:'%0'";
let W_Deny_Manager_Text:string = " Following managers are removed from manager list by NextLabs Enforcer:'%0'";

let nIndex: number = 0;
let _NlCategoryArray: Array<NLCategory> = new Array<NLCategory>();

let menu: LinkageMenu;

//old UI id
let CATEGORY_SELECT: string = 'lbCategory';

//New UI id
let LINKAGE_MENU_TABLE: string = 'linkageMenuTable';
let LINKAGE_MENU_TD: string = 'linkageMenuTd';
let AUTO_TAG_SPAN: string = 'autoTagSpan';

//UI display content
let CLASSIFICATION_TITLE = 'Classification';
let CLASSIFICATION_DESC = 'Classification can be used to safeguard your room access based on members attributes.';

//chatroom transport xml attribute name definitions
let TAG_NODE: string = 'tag';
let TAG_NAME: string = 'name';
let TAG_VALUE: string = 'value';


MainForm.prototype.NLShowCreatePage = function (): void {
    // Get the initial info for create page
    this.NLInitNewRoomPage();

    // Better UX on loading new room wait
    this.ToggleSpinningWait(true);
};

MainForm.prototype.NLInitNewRoomPage = function (): void {
    if (this.dataUser.WebTicket == null) {
        // Todo: Create an error message is webticket is somehow null here
    }
    else {
        this.clearServerMessages();
        requestedRoomId = ''; //set it to empty when creating a room

        let sReqUserInfo: string = _requestsHeader + StringHelper.cmdFormat(_requestsShell, _RMCommand_NewRoomInfo);
        this.RequestInProgress = RequestInProgress.RM_GetNewRoomInfo;
        window.setTimeout(TimerHandler(this.connection, this.connection.SendHttpRequest, "POST",
            _RMHandlerUrl, sReqUserInfo, this.dataUser.WebTicket), 0);
    }
};

MainForm.prototype.OnRM_NLGetNewRoomInfoCallback = function (): void {

    // turn off loading new room wait
    this.ToggleSpinningWait(false);
    this.RequestInProgress = RequestInProgress.None;

    // Make it an editable room
    this.SetRoomReadOnly(false);
    this.EnableRoomCtrls();

    // Read the response
    let xmlContent: any = this.connection.GetResponseXML();

    // Populate the landing room list with the response feedback
    if (!xmlContent) {
        // ToDo: display error message on empty xml
        return;
    }

    getChatRoomEnforceInfoFromXml(xmlContent);

    // Clean up the existing data.
    this.cleanUpRoomPage();

    //clearLinkageMenuData();
    //clearAutoTagLabelData();

    let nameNode: any = xmlContent.getElementsByTagName('UserName')[0];
    this.userName = nameNode.firstChild.nodeValue;

    // Wire up the categories
    this.wireUpCategories(xmlContent);

    // check if user has no create right from any category.
    if (0 == this.categoryArray.length) {
        this.ShowOperationMessage(W_NoPermissionToCreate, false);
        return;
    }
    else {
        this.HideCategory(1 == this.categoryArray.length);

        let sObXml: string = _NlCategoryArray[0].classification;
        let menuContainer: HTMLElement = document.getElementById(LINKAGE_MENU_TD);

        HtmlHelper.clearSubNodes(menuContainer);
        toggleMenuTable(_NlCategoryArray, true, sObXml);
        menu = buildLinkageMenu(sObXml);
    }

    // Wire up the Add-ins
    this.wireUpAddIns(xmlContent);

    // Hide the "create a new room" button
    this.btnCreateANewRoom.style.display = 'none';

    // Change the sub-title text to 'create a new room'
    this.lbl_SkypeSubTitle.innerHTML = txt_CreateRoomTitle;
    this.lbl_SkypeSubTitle.style.display = '';
    this.lbl_CreateRoomTitle.innerHTML = txt_CreateRoomSectionTitle;
    this.lbl_CreateRoomSubTitle.innerHTML = txt_CreateRoomDescr;

    // Hide everyone else
    this.landingDiv.style.display = 'none';

    // Display the Create part
    this.newRoomDiv.style.display = '';

    // wipe out the old name
    this.oldRoomName = '';

    this.lbl_radInherit.innerHTML = W_NotifyMembers_Basic;

    if (this.categoryArray.length == 1) {
        let category = xmlContent.getElementsByTagName('Categories')[0].childNodes[0];
        let catInviteSetting = category.childNodes[3].firstChild.nodeValue.toLowerCase();
        this.lbl_radInherit.innerHTML = catInviteSetting == "true" ? W_NotifyMembers_True : W_NotifyMembers_False;
        nIndex = 1;
        dropLable();
    }
    else {
        shiftLable();
    }

    // Hide the controls not used for room creation
    this.idBtnModify.style.display = 'none';
    this.idBtnRefresh.style.display = 'none';
    this.cbDeleteRoom.style.display = 'none';
    this.lblDeleteRoom.style.display = 'none';
    this.idBtnCreate.style.display = '';
    this.idBtnCreate.style.background = '#fdfdfd';
    this.idBtnCancel.innerHTML = W_ConfirmCancelText;
    this.idBtnCancel.style.background = '#fdfdfd';

    //show nxl controls
    let selCategory: NLCategory;

    if (this.categoryArray.length == 1) {
        selCategory = _NlCategoryArray[0];
    }
    this.ShowNxlElementAccordCateEnforceInfo(selCategory);
};

MainForm.prototype.NLCreateRoom = function (): void {

    this.clearServerMessages();

    // Validate input first
    if (!this.ClientSideValidate(false)) {
        return; // Do nothing if basic validation fails
    }

    // get name
    let name: string = EscapeXmlSensitveChar(StringHelper.trim(this.tbRoomName.value));

    // get description
    let sDescription: string = EscapeXmlSensitveChar(this.taRoomDesc.value);

    // get privacy
    let sPrivacy: string;

    if (this.radOpen.checked) {
        sPrivacy = 'Open';
    }
    else if (this.radClosed.checked) {
        sPrivacy = 'Closed';
    }
    else {
        sPrivacy = 'Secret';
    }

    // get add-in
    let sAddIn: string;

    if (this.lbAddIns.selectedIndex === -1) {
        sAddIn = '';
    }
    else {
        sAddIn = this.lbAddIns.options[this.lbAddIns.selectedIndex].value;
    }

    // category
    let sCategoryUri: string = this.ChosenCategory();

    // get managers
    let sNormMans: string = this.NormalizeNames(this.taManagers.value);
    this.taManagers.value = sNormMans;
    let sManagers: string = EscapeXmlSensitveChar(sNormMans);

    // get members
    let sMembers: string = (this.radOpen.checked) ? '' : StringHelper.trim(this.taMembers.value);
    let sNormMems: string = this.NormalizeNames(sMembers);
    this.taMembers.value = sNormMems;
    sMembers = EscapeXmlSensitveChar(sNormMems);

    // get notification
    let sNotify: string = this.radInherit.checked ? "inherit" : "false";

    //get need enforce [added by nl]
    let sNeedEnforce: string = (<HTMLInputElement>document.getElementById("cbRoomNeedEnforce")).checked ? "true" : "false";

    //get selected tags string in xml
    let selectedTagDict: Dictionary;
    let sTagXml: string = '';

    if (menu) {
        selectedTagDict = menu.getSelectTagDict();
        sTagXml = TagHelper.buildRequestTagsXmlString(TAG_NODE, TAG_NAME, TAG_VALUE, selectedTagDict);
    }

    // set parameters and progress status
    this.RequestInProgress = RequestInProgress.RM_CreateRoom;
    let params: string = StringHelper.cmdFormat(_RMCommand_CreateRoomNL, name, sDescription, sPrivacy,
        sAddIn, sCategoryUri, sManagers, sMembers, sNotify, sNeedEnforce, sTagXml);
    let sReqUserInfo: string = _requestsHeader + StringHelper.cmdFormat(_requestsShell, params);
    window.setTimeout(TimerHandler(this.connection, this.connection.SendHttpRequest,
        "POST", _RMHandlerUrl, sReqUserInfo, this.dataUser.WebTicket), 0);

    this.ToggleSpinningWait(true);
    this.DisableForSubmit(true);
};

MainForm.prototype.NLCategoryChange = function (): void {

    //call old handler
    this.CategoryChange();

    //NL code
    let nCategorySelectedIndex: number = this.lbCategory.selectedIndex;
    if (nCategorySelectedIndex + 1 <= _NlCategoryArray.length) {
        let selectedCategory: NLCategory = _NlCategoryArray[nCategorySelectedIndex];

        this.ShowNxlElementAccordCateEnforceInfo(selectedCategory);

        let sObXml: string = _NlCategoryArray[nCategorySelectedIndex].classification;
        let menuContainer: HTMLElement = document.getElementById(LINKAGE_MENU_TD);
        let autoTagEl: HTMLSpanElement = document.getElementById(AUTO_TAG_SPAN);

        if(autoTagEl) {
            autoTagEl.textContent = '';
        }
        else {
            LogHelper.log(`NLCategoryChange failed, can not find auto classified tag element <span>`);
        }

        HtmlHelper.clearSubNodes(menuContainer);
        toggleMenuTable(_NlCategoryArray, false, sObXml);

        menu = undefined;
        menu = buildLinkageMenu(sObXml);
    }
};

MainForm.prototype.NLShowEditRoom = function (xmlContent: any): void {

    //call old function
    this.ShowEditRoom(xmlContent);
    shiftLable();
    //set enforcer value
    let nodeNLEnforce: any = xmlContent.getElementsByTagName('NLEnforce')[0];

    let sNeedEnforce: string = nodeNLEnforce.getAttribute('NeedEnforce');
    let sForceEnforce: string = nodeNLEnforce.getAttribute('ForceEnforce');
    let sTextEnableEnforce: string = nodeNLEnforce.childNodes[0].firstChild.nodeValue;
    let sTextEnforceDescYes: string = nodeNLEnforce.childNodes[1].firstChild.nodeValue;
    let sTextEnforceDescNo: string = nodeNLEnforce.childNodes[2].firstChild.nodeValue;
    let sClassification: string = nodeNLEnforce.childNodes[3].firstChild.nodeValue;//get default classification string in xml

    //tags process
    let totalDict: Dictionary = TagHelper.getTagsFromResponseXml(xmlContent, TAG_NAME, TAG_VALUE);
    let obligationDict: Dictionary = TagHelper.getTagsFromObligation(sClassification);
    let autoTagDict: Dictionary = TagHelper.getTagsFromAutoClassification(totalDict, obligationDict);
    let sAutoTag: string = TagHelper.buildAutoTagsDisplayString(autoTagDict);
    let menuContainer: HTMLElement = document.getElementById(LINKAGE_MENU_TD);
    let autoTagContainer: HTMLElement = document.getElementById(AUTO_TAG_SPAN);

    HtmlHelper.clearSubNodes(menuContainer);
    toggleMenuTable(_NlCategoryArray, false, sClassification, sAutoTag);

    //add auto tags
    if (autoTagContainer) {
        autoTagContainer.textContent = '';
        autoTagContainer.innerHTML = sAutoTag;
    }
    else {
        LogHelper.log(`NLShowEditRoom() failed, auto tag container: ${autoTagContainer}.`);
    }

    //add linkage menu
    menu = null;
    menu = buildLinkageMenu(sClassification);

    if (menu) {
        menu.setSelectTagDict(menuContainer, totalDict);
    }
    else {
        LogHelper.log(`NLShowEditRoom() failed, menu: ${menu}.`);
    }

    let nlCategory: NLCategory = new NLCategory("tempName", "tempUri", sNeedEnforce, sForceEnforce, sTextEnableEnforce, sTextEnforceDescYes, sTextEnforceDescNo, sClassification);
    this.ShowNxlElementAccordCateEnforceInfo(nlCategory);
};

MainForm.prototype.NLShowReadOnlyRoom = function (xmlContent: any): void {

    //call old function
    this.ShowReadOnlyRoom(xmlContent);
    shiftLable();

    //set enforcer value
    let nodeNLEnforce: any = xmlContent.getElementsByTagName('NLEnforce')[0];

    let sNeedEnforce: string = nodeNLEnforce.getAttribute('NeedEnforce');
    let sForceEnforce: string = nodeNLEnforce.getAttribute('ForceEnforce');
    let sTextEnableEnforce: string = nodeNLEnforce.childNodes[0].firstChild.nodeValue;
    let sTextEnforceDescYes: string = nodeNLEnforce.childNodes[1].firstChild.nodeValue;
    let sTextEnforceDescNo: string = nodeNLEnforce.childNodes[2].firstChild.nodeValue;
    let sClassification: string = nodeNLEnforce.childNodes[3].firstChild.nodeValue;//get default classification string in xml

    // alert(name + "," + needEnforce + "," + editAble + "," + textEnableEnforce + "," + textEnforceDescYes + "," + textEnforceDescNo);
    let nlCateInfo = new NLCategory("tempName", "tempUri", sNeedEnforce, sForceEnforce, sTextEnableEnforce, sTextEnforceDescYes, sTextEnforceDescNo, sClassification);

    this.ShowNxlElementAccordCateEnforceInfo(nlCateInfo);
};

MainForm.prototype.NLCommitChanges = function (): void {

    this.clearServerMessages();

    if (!this.cbDeleteRoom.checked && !this.ClientSideValidate(true)) {
        return;
    }

    // get the room name
    let sRoomName: string = StringHelper.trim(this.tbRoomName.value);

    // Get confirmation next
    let sConfirm: string;
    if (this.cbDeleteRoom.checked) {
        sConfirm = StringHelper.cmdFormat(W_DeteteConfirmation, W_ConfirmOKText, W_ConfirmCancelText);
        if (!confirm(sConfirm)) {
            return;
        }
    }

    // Escape it after the confirmation
    sRoomName = EscapeXmlSensitveChar(sRoomName);
    if (this.cbDeleteRoom.checked) {
        // Handle deletion first
        this.SendDeletionRequest();
        return;
    }

    // get description
    let sDescription: string = EscapeXmlSensitveChar(this.taRoomDesc.value);

    // get privacy
    let sPrivacy: string;
    if (this.radOpen.checked)
        sPrivacy = 'Open';
    else if (this.radClosed.checked)
        sPrivacy = 'Closed';
    else
        sPrivacy = 'Secret';

    // get add-in
    let sAddIn: string;
    if (this.lbAddIns.selectedIndex === -1)
        sAddIn = '';
    else
        sAddIn = this.lbAddIns.options[this.lbAddIns.selectedIndex].value;

    // category
    let sCategoryUri: string = this.ChosenCategory();

    // get managers
    let sNormMans: string = this.NormalizeNames(this.taManagers.value);
    this.taManagers.value = sNormMans;
    let sManagers: string = EscapeXmlSensitveChar(sNormMans);

    // get members
    let sMembers: string = (this.radOpen.checked) ? '' : StringHelper.trim(this.taMembers.value);
    let sNormMems: string = this.NormalizeNames(sMembers);
    this.taMembers.value = sNormMems;
    sMembers = EscapeXmlSensitveChar(sNormMems);

    // get notification
    let sNotify: string = this.radInherit.checked ? "inherit" : "false";

    // get the deletion
    let sIsDeleted: string = this.cbDeleteRoom.checked ? "true" : "false";

    //get need enforce [added by nl]
    let sNeedEnforce: string = (<HTMLInputElement>document.getElementById("cbRoomNeedEnforce")).checked ? "true" : "false";

    //get selected tags xml string
    let tagDict: Dictionary;
    let sTagXml: string = '';

    if (menu) {
        tagDict = menu.getSelectTagDict();
        sTagXml = TagHelper.buildRequestTagsXmlString(TAG_NODE, TAG_NAME, TAG_VALUE, tagDict);
    }

    // set parameters and progress status
    this.RequestInProgress = RequestInProgress.RM_ModifyRoom;
    let params: string = StringHelper.cmdFormat(_RMCommand_ModifyRoomNL, sRoomName, sDescription, sPrivacy,
        sAddIn, sCategoryUri, sManagers, sMembers, sNotify, sIsDeleted,
        EscapeXmlSensitveChar(this.oldRoomName), requestedRoomId,
        this.timeStamp, sNeedEnforce, sTagXml);

    let sReqUserInfo: string = _requestsHeader + StringHelper.cmdFormat(_requestsShell, params);
    window.setTimeout(TimerHandler(this.connection, this.connection.SendHttpRequest,
        "POST", _RMHandlerUrl, sReqUserInfo, this.dataUser.WebTicket), 0);

    this.ToggleSpinningWait(true);
    this.DisableForSubmit(true);

};

MainForm.prototype.On_NLRMCreateRoomCallBack = function (): void {

    this.ToggleSpinningWait(false);
    this.DisableForSubmit(false);
    this.RequestInProgress = RequestInProgress.None;

    // if successful update the 
    let xmlContent: any = this.connection.GetResponseXML();

    // Populate the landing room list with the response feedback
    if (!xmlContent) {
        // ToDo: display error message on empty xml
        return;
    }

    // check creation result
    let successElement: any = xmlContent.getElementsByTagName('Success')[0];
    if ('true' === successElement.firstChild.nodeValue.toLowerCase()) {

        let sSuccessMsg: string = StringHelper.cmdFormat(W_Create_success, StringHelper.trim(this.tbRoomName.value));//default message

        //get membeers denied by SFB Server Enforcer
        sSuccessMsg = this.AppendMessageWithDenyInfo(xmlContent, sSuccessMsg);

        this.ShowOperationMessage(sSuccessMsg, true);

        this.GetLandingFromRoomPage();
    }
    else {
        let xmlErrItem: any;
        try {
            xmlErrItem = xmlContent.getElementsByTagName('RoomResult')[0];
            this.UpdateRoomNameMsg(xmlErrItem, false);
        }
        catch (e) {
            ; // do nothing if such error report does not exist.
        }

        try {
            xmlErrItem = xmlContent.getElementsByTagName('ManResult')[0];
            this.DisplayPeopleCheckingResults(xmlErrItem, W_InvalidManagers, this.tbl_ManError,
                this.spn_ManError, W_MgrDupADDisplayName, this.tbl_ManDupError,
                this.spn_ManDupError, this.taManagers);
        }
        catch (e) {
            LogHelper.log(`On_NLRMCreateRoomCallBack failed, ${e.message}`); // do nothing if such error report does not exist.
        }

        try {
            xmlErrItem = xmlContent.getElementsByTagName('MemResult')[0];
            this.DisplayPeopleCheckingResults(xmlErrItem, W_InvalidMembers, this.tbl_MemError,
                this.spn_MemError, W_MemDupADDisplayName, this.tbl_MemDupError,
                this.spn_MemDupError, this.taMembers);
        }
        catch (e) {
            LogHelper.log(`On_NLRMCreateRoomCallBack failed, ${e.message}`); // do nothing if such error report does not exist.
        }
        return;
    }

};

MainForm.prototype.On_NLRMModifyRoomCallBack = function (): void {

    this.ToggleSpinningWait(false);
    this.DisableForSubmit(false);
    this.RequestInProgress = RequestInProgress.None;

    // if successful update the 
    let xmlContent: any = this.connection.GetResponseXML();

    // Populate the landing room list with the response feedback
    if (!xmlContent) {
        // ToDo: display error message on empty xml
        return;
    }

    let resultCodeElement: any = xmlContent.getElementsByTagName('ResultCode')[0];

    // See if it runs into a stale state
    if (resultCodeElement && resultCodeElement.firstChild && (resultCodeElement.firstChild.nodeValue === this.Modify_Failed_StaleState)) {
        // Show the refresh button and hide the commit button
        // Uncheck the delete button.
        this.idBtnRefresh.style.display = '';
        this.idBtnModify.style.display = 'none';
        this.DisableForDelete(false);
        this.cbDeleteRoom.checked = false;

        this.SetSpanText(this.spn_RaceWarning, W_RoomStaleState);
        this.ShowPrompt(this.tbl_RaceWarning, true);

        return;
    }

    // See if the input validation failed
    if (resultCodeElement && resultCodeElement.firstChild && resultCodeElement.firstChild.nodeValue === this.Modify_Failed_InvalidInput) {
        let xmlErrItem: any = xmlContent.getElementsByTagName('RoomResult')[0];
        if (xmlErrItem) {
            this.UpdateRoomNameMsg(xmlErrItem, false);
        }

        xmlErrItem = xmlContent.getElementsByTagName('ManResult')[0];
        if (xmlErrItem) {
            this.DisplayPeopleCheckingResults(xmlErrItem, W_InvalidManagers, this.tbl_ManError,
                this.spn_ManError, W_MgrDupADDisplayName, this.tbl_ManDupError,
                this.spn_ManDupError, this.taManagers);
        }

        xmlErrItem = xmlContent.getElementsByTagName('MemResult')[0];
        if (xmlErrItem) {
            this.DisplayPeopleCheckingResults(xmlErrItem, W_InvalidMembers, this.tbl_MemError,
                this.spn_MemError, W_MemDupADDisplayName, this.tbl_MemDupError,
                this.spn_MemDupError, this.taMembers);
        }

        return; // Give users a chance to correct their input
    }

    // Validation succeeded. Check modification result
    let sResultMsg: string = this.NLResultCodeToTxt(xmlContent);
    let successElement: any = xmlContent.getElementsByTagName('Success')[0];
    if ('true' == successElement.firstChild.nodeValue.toLowerCase()) {
        this.ShowOperationMessage(sResultMsg, true);
    }
    else {
        this.ShowOperationMessage(sResultMsg, false);
    }

    this.GetLandingFromRoomPage();

};

MainForm.prototype.NLResultCodeToTxt = function (xmlElement: any): string {

    let sResult: string = '';
    // Get the room name
    let sRoomName: string = StringHelper.trim(this.tbRoomName.value);

    // Get the result code
    let resultCodeElement: any = xmlElement.getElementsByTagName('ResultCode')[0];
    let sResultCode: string = resultCodeElement.firstChild.nodeValue;

    switch (sResultCode) {
        case this.Modify_RoomUpdated:
            sResult = this.GetRoomUpdatedMessage(xmlElement, sRoomName);
            break;

        case this.Modify_RoomDeleted:
            sRoomName = this.oldRoomName; // make sure we show the original name
            sResult = StringHelper.cmdFormat(W_Delete_success, sRoomName);
            break;

        case this.Modify_Failed_NoPermission:
            sResult = StringHelper.cmdFormat(W_Update_Permission_Failure, sRoomName);
            break;
    }
    return sResult;
};

MainForm.prototype.GetRoomUpdatedMessage = function (xmlContent: any, sRoomName: string): string {

    let sUpdateMsg: string = StringHelper.cmdFormat(W_Update_success, sRoomName);//default message

    sUpdateMsg = this.AppendMessageWithDenyInfo(xmlContent, sUpdateMsg);

    return sUpdateMsg;
};

MainForm.prototype.ShowNxlElementAccordCateEnforceInfo = function (selectedCategory: NLCategory): any {

    if (!selectedCategory) {//hide all

        document.getElementById("cbRoomNeedEnforce").style.display = 'none';
        document.getElementById("lblNxlControlRoom").style.display = 'none';

        document.getElementById("imgRoomEnforce").style.display = 'none';
        document.getElementById("imgRoomNotEnforce").style.display = 'none';
        document.getElementById("lblNxlRoomEnforceStatusDesc").style.display = 'none';
    }
    else {//show/hide accroding to selCategory

        document.getElementById("cbRoomNeedEnforce").style.display = selectedCategory.EditAble() ? "" : "none";
        document.getElementById("lblNxlControlRoom").style.display = selectedCategory.EditAble() ? "" : "none";
        (<HTMLInputElement>document.getElementById("cbRoomNeedEnforce")).checked = selectedCategory.NeedEnforce();
        document.getElementById("lblNxlControlRoom").innerText = selectedCategory.textEnableEnforce;


        document.getElementById("imgRoomEnforce").style.display = selectedCategory.NeedEnforce() ? "" : "none";
        document.getElementById("imgRoomNotEnforce").style.display = selectedCategory.NeedEnforce() ? "none" : "none";
        document.getElementById("lblNxlRoomEnforceStatusDesc").style.display = (selectedCategory.EditAble() || !selectedCategory.NeedEnforce()) ? "none" : "";
        document.getElementById("lblNxlRoomEnforceStatusDesc").innerText = selectedCategory.NeedEnforce() ? selectedCategory.textEnforceDescYes : selectedCategory.textEnforceDescNo;
    }

};

MainForm.prototype.AppendMessageWithDenyInfo = function (xmlContent: any, sOldMsg: string): any {

    let sNewMsg: string = sOldMsg;

    //get membeers denied by SFB Server Enforcer
    let xmlNxlResultItem: any = xmlContent.getElementsByTagName('DenyMember')[0];

    if (xmlNxlResultItem && xmlNxlResultItem.firstChild) {
        let sDenyMember: string = xmlNxlResultItem.firstChild.nodeValue;
        if (sDenyMember) {
            sNewMsg = sNewMsg + StringHelper.cmdFormat(W_Deny_Member_Text, sDenyMember);
        }
    }

    let xmlNxlDenyManagerItem: any = xmlContent.getElementsByTagName('DenyManager')[0];

    if (xmlNxlDenyManagerItem && xmlNxlDenyManagerItem.firstChild) {
        let sDenyManager: string = xmlNxlDenyManagerItem.firstChild.nodeValue;
        if (sDenyManager) {
            sNewMsg = sNewMsg + StringHelper.cmdFormat(W_Deny_Manager_Text, sDenyManager);
        }
    }

    return sNewMsg;

};

//main function
(function NlMain() {

    /**
    custom event ready.
    triggered when the dom structure loaded.
    insert the classification table after the tbl_Category table when ready.
    */
    document.addEventListener('ready', function (evt: Event) {
        insertMenuTable();
        insertAutoTag();
    }, true);

    document.addEventListener('readystatechange', function (evt: Event) {
        if (evt && evt.currentTarget) {
            let doc = <HTMLDocument>evt.currentTarget;
            if (doc.readyState === HtmlHelper.INTERACTIVE_STATE) {
                try {
                    let evtInit: EventInit = { bubbles: true, cancelable: true };
                    let readyEvent: Event = new Event('ready', evtInit);
                    doc.dispatchEvent(readyEvent);
                }
                catch (e) {
                    LogHelper.log(`Event not supported, use createEvent instead.`);
                    let readyEvent: Event = document.createEvent('Event');
                    readyEvent.initEvent('ready', true, true);
                    doc.dispatchEvent(readyEvent);
                }
            }
        }
        else {
            LogHelper.log(`addEventListeners() failed, readystatechange event, `)
        }
    }, true);
})();

//private functions
function getChatRoomEnforceInfoFromXml(xmlContent: any): void {

    _NlCategoryArray.splice(0, _NlCategoryArray.length);//clear old data

    let nodeListCate: any = xmlContent.getElementsByTagName('CategoryTag');

    for (let cateIndex: number = 0; cateIndex < nodeListCate.length; cateIndex++) {
        let nodeCate: any = nodeListCate[cateIndex];
        if (nodeCate.childNodes.length >= 5) {
            let sName: string = nodeCate.childNodes[0].firstChild.nodeValue;
            let sUri: string = nodeCate.childNodes[1].firstChild.nodeValue;

            let nodeNLEnforce: any = nodeCate.childNodes[4];

            let sNeedEnforce: string = nodeNLEnforce.getAttribute('NeedEnforce');
            let sForceEnforce: string = nodeNLEnforce.getAttribute('ForceEnforce');
            let sTextEnableEnforce: string = nodeNLEnforce.childNodes[0].firstChild.nodeValue;
            let sTextEnforceDescYes: string = nodeNLEnforce.childNodes[1].firstChild.nodeValue;
            let sTextEnforceDescNo: string = nodeNLEnforce.childNodes[2].firstChild.nodeValue;
            let sClassification: string = nodeNLEnforce.childNodes[3].firstChild.nodeValue;

            // alert(name + "," + needEnforce + "," + editAble + "," + textEnableEnforce + "," + textEnforceDescYes + "," + textEnforceDescNo);
            _NlCategoryArray.push(new NLCategory(sName, sUri, sNeedEnforce, sForceEnforce, sTextEnableEnforce, sTextEnforceDescYes, sTextEnforceDescNo, sClassification));
        }
    }
}

function shiftLable(): void {
    let td2: any = document.getElementById("div_CategoryDescr").parentNode;
    let td: any = document.getElementById("lblNxlRoomEnforceStatusDesc").parentNode;
    let tr: any = td.parentNode;
    let ta: any = tr.parentNode;
    let td1: any = document.getElementById("td_MgrLabel");
    let tr1: any = td1.parentNode;
    let ta1: any = tr1.parentNode;
    ta1.insertBefore(tr, tr1);
}

function dropLable(): void {
    let td2: any = document.getElementById("cbDeleteRoom").parentNode;
    let tr2: any = td2.parentNode;
    let td: any = document.getElementById("lblNxlRoomEnforceStatusDesc").parentNode;
    let tr: any = td.parentNode;
    let ta: any = tr.parentNode;
    let td1: any = document.getElementById("td_MgrLabel");
    let tr1: any = td1.parentNode;
    let ta1: any = tr1.parentNode;
    ta.insertBefore(tr, tr2);
    ta.insertBefore(tr2, tr);
}

function insertMenuTable(): void {

    let sBaseElementName: string = 'tbl_Category';
    let sNewTableName: string = LINKAGE_MENU_TABLE;

    let baseElement: HTMLElement = document.getElementById(sBaseElementName);
    let newTableElement: HTMLTableElement = buildMenuWrapper();

    if (baseElement && newTableElement) {
        HtmlHelper.insertAfter(newTableElement, baseElement);
    }
    else {
        LogHelper.log(`insertMenuTable() failed, baseElement: ${baseElement}, tableElement: ${newTableElement}.`);
        LogHelper.log(`readyState: ${document.readyState}.`);
    }

};


//build classification table
function buildMenuWrapper(): HTMLTableElement {

    let tableElement: HTMLTableElement;
    let captionElement: HTMLTableCaptionElement;
    let titleSpanElement: HTMLSpanElement;
    let descSpanElement: HTMLSpanElement;
    let trElement: HTMLTableRowElement;
    let tdElement: HTMLTableDataCellElement;

    tableElement = buildMenuTable();
    captionElement = buildTableCaption();
    titleSpanElement = buildCaptionTitleSpan();
    descSpanElement = buildCaptionDescSpan();
    trElement = buildMenuTr();
    tdElement = buildMenuTd();

    captionElement.appendChild(titleSpanElement);
    captionElement.appendChild(descSpanElement);

    tdElement.id = LINKAGE_MENU_TD;

    trElement.appendChild(tdElement);

    tableElement.appendChild(captionElement);
    tableElement.appendChild(trElement);

    return tableElement;
}

function buildMenuTable(): HTMLTableElement {

    let tableElement: HTMLTableElement;

    tableElement = document.createElement('table');
    tableElement.id = LINKAGE_MENU_TABLE;
    tableElement.style.width = '100%';
    tableElement.style.tableLayout = 'fixed';
    tableElement.style.border = 'thin solid gray';
    tableElement.style.margin = '10px 0px';//updown leftright
    tableElement.style.textAlign = 'left';

    return tableElement;
}

function buildMenuTr(): HTMLTableRowElement {

    let tableRowElement: HTMLTableRowElement;

    tableRowElement = document.createElement('tr');

    return tableRowElement;
}

function buildMenuTd(): HTMLTableDataCellElement {

    let tableDataElement: HTMLTableDataCellElement;

    tableDataElement = document.createElement('td');

    return tableDataElement;
}

function buildTableCaption(): HTMLTableCaptionElement {

    let tableCapElement: HTMLTableCaptionElement;

    tableCapElement = document.createElement('caption');
    tableCapElement.style.margin = '5px 0px';
    tableCapElement.style.textAlign = 'left';

    return tableCapElement;
}

function buildCaptionTitleSpan(): HTMLSpanElement {

    let capSpanElement: HTMLSpanElement;

    capSpanElement = document.createElement('span');
    capSpanElement.style.display = "block";
    capSpanElement.style.fontSize = "11px";
    capSpanElement.style.fontFamily = "Segoe UI Semibold";
    capSpanElement.style.fontWeight = "bold";
    capSpanElement.style.color = "#444444";
    capSpanElement.textContent = CLASSIFICATION_TITLE;

    return capSpanElement;
}

function buildCaptionDescSpan(): HTMLSpanElement {

    let capSpanElement: HTMLSpanElement;

    capSpanElement = document.createElement('span');
    capSpanElement.style.display = "block";
    capSpanElement.style.fontSize = "11px";
    capSpanElement.style.fontFamily = "Segoe UI";
    capSpanElement.style.fontWeight = "normal";
    capSpanElement.style.color = "#5e5e5e";
    capSpanElement.style.margin = '5px 0px';
    capSpanElement.textContent = CLASSIFICATION_DESC;

    return capSpanElement;
}

//menu table view state operation
function toggleMenuTable(_szCategories: Array<NLCategory>, _bIsCreateRoom: boolean, _sObXml: string, _sAutoTag?: string): void {

    let categorySelect: HTMLSelectElement;
    let nLength: number;

    categorySelect = <HTMLSelectElement>document.getElementById(CATEGORY_SELECT);

    if (_szCategories && categorySelect) {

        nLength = _szCategories.length;

        if (_bIsCreateRoom) {
            switch (nLength) {
                case 1: {
                    categorySelect.selectedIndex = nLength - 1;

                    if (_szCategories[categorySelect.selectedIndex].classification) {
                        displayMenuTable();
                    }
                    else {
                        hideMenuTable();
                    }
                    break;
                }
                default: {
                    hideMenuTable();
                }
            }
        }
        else {//modify room or change category
            if (_sObXml) {
                displayMenuTable();
            }
            else if (_sAutoTag) {
                displayMenuTable();
            }
            else {
                hideMenuTable();
            }
        }
    }
    else {
        LogHelper.log(`toggleMenuTable() failed, Categories: ${_szCategories}, categorySelect: ${categorySelect}.`);
    }
}

function displayMenuTable(): void {

    let tableElement: HTMLTableElement;

    tableElement = <HTMLTableElement>document.getElementById(LINKAGE_MENU_TABLE);

    if (tableElement) {
        tableElement.style.display = 'table';
    }
    else {
        LogHelper.log(`displayMenuTable() failed, tableElement: ${tableElement}.`);
    }
}

function hideMenuTable(): void {

    let tableElement: HTMLTableElement;

    tableElement = <HTMLTableElement>document.getElementById(LINKAGE_MENU_TABLE);

    if (tableElement) {
        tableElement.style.display = 'none';
    }
    else {
        LogHelper.log(`hideMenuTable() failed, tableElement: ${tableElement}.`);
    }
}

//linkage menu operation
function buildLinkageMenu(_sObXml: string): LinkageMenu {

    let menu: LinkageMenu;
    let container: HTMLElement;
    let styleModifier: IStyleModify;
    let dataSourceFormatter: IDataSourceFormat;

    if (_sObXml) {
        styleModifier = new ChatRoomStyleModify();
        dataSourceFormatter = new XmlDataSourceFormat();
        container = document.getElementById(LINKAGE_MENU_TD);

        if (styleModifier && dataSourceFormatter && container) {
            menu = new LinkageMenu(styleModifier, dataSourceFormatter, container, _sObXml);
            menu.bind();
        }
        else {
            LogHelper.log(`buildLinkageMenu() failed, styleModifier: ${styleModifier}, dataSourceFormatter: ${dataSourceFormatter}, container: ${container}.`);
        }
    }
    else {
        LogHelper.log(`buildLinkageMenu() failed, ObXml: ${_sObXml}.`);
    }


    return menu;
}

//build auto tags
function buildAutoTagSpan(): HTMLSpanElement {

    let spanElement: HTMLSpanElement;

    spanElement = document.createElement('span');

    return spanElement;
}

function buildAutoTagTr(): HTMLTableRowElement {

    let trElement: HTMLTableRowElement = buildMenuTr();
    let tdElement: HTMLTableDataCellElement = buildMenuTd();
    let spanElement: HTMLSpanElement = buildAutoTagSpan();

    spanElement.id = AUTO_TAG_SPAN;
    spanElement.style.display = "block";
    spanElement.style.fontSize = "11px";
    spanElement.style.fontFamily = "Segoe UI";
    spanElement.style.fontWeight = "normal";
    spanElement.style.color = "#5e5e5e";
    spanElement.style.margin = '5px 10px';

    tdElement.appendChild(spanElement);
    trElement.appendChild(tdElement);

    return trElement;
}

function insertAutoTag(): void {

    let trElement: HTMLTableRowElement = buildAutoTagTr();
    let linkageTableMenu: HTMLTableElement = <HTMLTableElement>document.getElementById(LINKAGE_MENU_TABLE);
    let linkageTdElement: HTMLTableDataCellElement = <HTMLTableDataCellElement>document.getElementById(LINKAGE_MENU_TD);
    let linkageTrElement: HTMLTableRowElement = linkageTdElement ? <HTMLTableRowElement>linkageTdElement.parentNode : undefined;

    if (linkageTableMenu && linkageTrElement) {
        linkageTableMenu.insertBefore(trElement, linkageTrElement);
    }
    else {
        LogHelper.log(`insertAutoTag() failed, menu td: ${linkageTdElement}, menu table: ${linkageTableMenu}.`);
    }
}