import {NLCategory} from '../../../../jsbin/Modules/entity';
//<reference path="../entity"/>

declare global{

    // Room Management request URL and command
    let _requestsHeader:string;
    let _requestsShell:string;
    let _RMHandlerUrl:string;
    let _ResourceUrl:string;
    let _RMCommand_AccessRight:string;
    let _RMCommand_Landing:string;
    let _RMCommand_NewRoomInfo:string;
    let _RMCommand_CheckRoomName:string;
    let _RMCommand_CheckManagers:string;
    let _RMCommand_CheckMembers:string;
    let _RMCommand_CreateRoom: string;

    //UI Resources
    let txt_CreateRoomTitle: string;
    let txt_CreateRoomSectionTitle: string;
    let txt_CreateRoomDescr: string;
    let W_NotifyMembers_Basic: string;
    let W_ConfirmCancelText: string;
    let W_NoPermissionToCreate: string;
    let W_NotifyMembers_True: string;
    let W_NotifyMembers_False: string;
    let W_DeteteConfirmation: string;
    let W_ConfirmOKText: string;
    let W_Create_success: string;
    let W_InvalidManagers: string;
    let W_MgrDupADDisplayName: string;
    let W_InvalidMembers: string;
    let W_MemDupADDisplayName: string;
    let W_Delete_success: string;
    let W_Update_Permission_Failure: string;
    let W_Update_success: string;
    let W_RoomStaleState: string;



    //letiable defined in page script tag

    let sIsExternal: string;
    let isLandingRequest: string;
    let validRoomRequestFormat: string;
    let requestedRoomId: string;
    let rmWebUrl: string;
    let pageLang: string;


    //helper function
    function TimerHandler(...arg: any[]): any;
    function EscapeXmlSensitveChar(sTarget: string): string;
    function UnescapeXmlChars(sTarget: string): string;

    //classes
    class MainForm{
        connection:Connection;
        dataUser:UserData;
        nTimeoutID:number;
        webTicketManager:any;
        RequestInProgress:number;
        
        // Room management list
        myRoomArray:Array<any>;
        addInArray:Array<any>;
        categoryArray: Array<any>;

        //Html Elements
        tbl_RaceWarning: HTMLTableElement;

        // data
        userName:string;
        oldRoomName:string;
        lastMembers:string;
        lastLbCateIndex:number;
        timeStamp:string;

        Modify_RoomUpdated:string;
        Modify_RoomDeleted:string;
        Modify_Failed_InvalidInput:string;
        Modify_Failed_NoPermission:string;
        Modify_Failed_StaleState:string;
        Not_PC_Enabled:string;

        // Deal with browser differences on the TextArea control
        ResetTimer():void;
        InitForm(dataUser:UserData):void;
        TurnOffTaResize(textAreaElement:HTMLTextAreaElement):void;
        AuthTypesReceived(authType:any, exception:any):void;
        UpdateRoomResource():void;
        OnUpdateResourceCallback():void;
        HandleSignIn(fromExpiry:boolean):void;
        HandleExpiry():void;
        isBrowserIWACapable():boolean;
        SignOut():void;
        SendDiffAccountSignIn():void;
        AcquireTicketCallback(webTicketInfo:any, exception:any):void;
        PreSignInValidation():boolean;
        ShowAuthenticationError(errorMsg:string):void;
        ClearSignInMessages():void;
        SendIWASignIn():void;
        RedirectTo(sUrl:string):void;
        OnError(nErrorCode:number, exception:any):void;
        UpdateDisplayName(xmlConent:any):void;
        OnSuccess():void;

        // Chatroom management js programs start from here
        ShowOperationMessage(sMsg:string, bIsSuccess:boolean):void;
        BlankOperationMessage():void;
        GetSignoutInfo():void;
        GetLandingInfo():void;
        OnRM_LandingCallback():void;
        AddToDivRoomList(sName:string, sRoomGuid:string, sDesc:string):void;
        SetSpanText(spanElement:HTMLSpanElement, sText:string):void;
        RoomAccessFromLanding(sRoomGuid:string):void;
        OnMyRoomSelectChange():void;
        ShowCreatePage():void;
        ShowLandingPage():void;
        InitNewRoomPage():void;
        OnRM_GetNewRoomInfoCallback():void;
        cleanUpRoomPage():void;
        clearServerMessages():void;
        wireUpAddIns(xmlConent:any):void;
        wireUpCategories(xmlContent:any):void;
        PrivacySelected(sChoice:string):void;
        SetMembersOnPrivacy(sChoice:string):void;
        SetNotificationOnPrivacy(sChoice:string):void;
        ChosenCategory():string;
        CheckRoomName():void;
        CheckPrincipalNames(sFrom:string):void;
        On_RMRoomCheckingCallBack():void;
        On_RMManagerCheckingCallBack():void;
        On_RMMemberCheckingCallBack():void;
        UpdateRoomNameMsg(xmlItem:any, bShowSuccess:boolean):void;
        DisplayPeopleCheckingResults(xmlItem:any, sPrefixInv:string, divErrInv:HTMLDivElement, spnMsgInv:HTMLSpanElement, sPrefixDup:string, tableErrDup:HTMLTableElement, spnMsgDup:HTMLSpanElement, txtBox:HTMLTextAreaElement):void
        NormalizeNames(sOriName:string):string;
        ClientSideValidate(bForEdit:boolean):boolean;
        CreateRoom():void;
        On_RMCreateRoomCallBack():void;
        GetLandingFromRoomPage():void;
        validationFailed(msgDiv:HTMLDivElement, msgLabel:HTMLLabelElement, sMsg:string):void;
        IsManOrMemToReset():boolean;
        CategoryChange():void;
        EnableTextArea(textAreaElement:HTMLTextAreaElement, bOption:boolean):void;
        Action():void;
        QueryRoomAccess(sRoomId:string):void;
        On_RMAccessRightCallBack():void;
        UnableToProcessRequest():void;
        ProcessFailedRequest(xmlContent:any):void;
        ShowEditRoom(xmlContent:any):void;
        SetDescriptionCaretPosition(nPos:number):void;
        SetEditbaleAddInSelection(sAddInId:string):void;
        SetRoomReadOnly(bOption:boolean):void;
        ShowReadOnlyRoom(xmlContent:any):void;
        DeleteChanged():void;
        EnableRoomCtrls():void;
        DisableForSubmit(bOption:boolean):void;
        DisableForDelete(bOption:boolean):void;
        SendDeletionRequest():void;
        CommitChanges():void;
        On_RMModifyRoomCallBack():void;
        ResultCodeToTxt(xmlElement:any):string;
        SyncRoom():void;
        On_RMSyncRoomCallBack():void;
        UpdateUIText():void;
        GetElementsByClassName(sClassName:string):Array<any>;
        CountDescription():void;
        ToggleSpinningWait(bOption:boolean):void;
        ButtonMouseOver(btn:HTMLElement):void;
        ButtonMouseOut(btn:HTMLElement):void;
        ButtonMouseDown(btn:HTMLElement):void;
        ButtonMouseUp(btn:HTMLElement):void;
        PwdKeyUp(evt:any):void;
        WarnOnLeavingPage():string;
        RoomCancelClick():void;
        ShowPrompt(element:HTMLElement, bOption:boolean):void;
        HideCategory(bOption:boolean):void;
        SetDescriptionDisplay():void;

        //NL Handlers
        NLShowCreatePage():void;
        NLInitNewRoomPage():void;
        OnRM_NLGetNewRoomInfoCallback():void;
        NLCreateRoom():void;
        NLCategoryChange():void;
        NLShowEditRoom(xmlConent:any):void;
        NLShowReadOnlyRoom(xmlContent:any):void;
        NLCommitChanges():void;
        On_NLRMCreateRoomCallBack():void;
        On_NLRMModifyRoomCallBack():void;
        NLResultCodeToTxt(xmlElement:any):string;
        GetRoomUpdatedMessage(xmlContent: any, sRoomName: string): string;
        ShowNxlElementAccordCateEnforceInfo(nlCategory: NLCategory): void;
        AppendMessageWithDenyInfo(xmlConent:any, oldMsg:string):void;
    }

    class Connection{
        httpRequest:any;
        ResponseStatus:number;
        ResponseSubStatus:number;

        Initialize(own:any):void;
        GetResponseXML():any;
        GetResponseHeader(sHeaderName:string):string;
        SendHttpRequest(sHttpMethod:string, sUrl:string, sData:string, webTicket:any):void;
        OnReadyStateChanged():void;
        HttpRequestFailed(nErrorCode:number, exception:any):void;
        HttpRequestSucceeded():void;
        _CreateXMLHttpRequestObject():any;
    }

    class UserData{
        Uri:string;
        UserName:string;
        SignInAs:string;
        Language:string;
        WebTicket:any;
        Status:number;
    }

    namespace RequestInProgress {
        let None: number;
        let UpdateResource: number;
        let RM_Landing: number;
        let RM_GetNewRoomInfo: number;
        let RM_CheckManagers: number;
        let RM_CheckMembers: number;
        let RM_CreateRoom: number;
        let RM_CheckRoomName: number;
        let RM_AccessRight: number;
        let RM_ModifyRoom: number;
        let RM_SyncRoom: number;
        let RM_Signout: number;
    }

}

export {MainForm, Connection, UserData, RequestInProgress};