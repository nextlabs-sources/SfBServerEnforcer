//operation type name
class MeetingResultType {
    static SEARCH_TYPE: string = "Search";
    static ClASSIFY_TYPE: string = "Classify";
    static SAVE_TYPE: string = "Save";
    static SHOW_TYPE: string = "Show";
}

//definition for ui elements id
class MeetingUI {
    static RESULT_WRAPPER: string = 'resultWrapper';
    static DATE_SELECT: string = 'dateFilterSelect';
    static SHOW_MEETING_SELECT: string = 'showMeetingsSelect';
    static MEETING_URI_INPUT: string = 'meetingUriInput';
    static SEARCH_BTN: string = 'searchBtn';
    static LIST_TABLE_BODY: string = 'listTableBody';
    static CLASSIFICATION_MASK: string = 'classificationMask';
    static CLASSIFICATION_WRAPPER: string = 'classificationWrapper';
    static CREATE_TIME_LABEL: string = 'createTimeLabel';
    static CREATOR_LABEL: string = 'creatorLabel';
    static MEETING_URI_OUTPUT: string = 'meetingUriOutput';
    static MEETING_ENTRY_LABEL: string = 'meetingEntryInfoLabel';
    static MEETING_ENTRY_OUTPUT: string = 'meetingEntryInfoOutput';
    static AUTO_TAG_WRAPPER: string = 'autoTagWrapper';
    static LINKAGE_MENU_WRAPPER: string = 'linkageMenuWrapper';
    static CANCEL_BTN: string = 'cancelBtn';
    static SAVE_BTN: string = 'saveBtn';
    static LOADING_BAR_WRAPPER: string = 'loadingBarWrapper';
    static LOADING_COLUMN_WRAPPER: string = 'loadingColumnWrapper';
    static POLICY_WRAPPER: string = 'policyWrapper';
    static POLICY_RESULT_LIST: string = 'resultList';
    static POLICY_CANCEL_BTN: string = 'policyCancelBtn';
    static POLICY_CONTINUE_BTN: string = 'policyContinueBtn';
    static POLICY_NEW_TAG_LIST: string = 'newTagList';
    static POLICY_ORIGINAL_TAG_LIST: string = 'originalTagList';
    static POLICY_NEW_LINKAGEMENU: string = 'newLinkageMenuContainer';
    static POLICY_OLD_LINKAGEMENU: string = 'oldLinkageMenuContainer';
    static MEETING_RESULT_COUNTER: string = 'resultCounter';
    static MEETING_SEARCH_RESULT_COUNTER: string = 'searchResultCounter';
    static MEETING_SEARCH_KEYWORDS: string = 'searchKeywords';
}

class MeetingUrlKey {
    static USER_KEY: string = 'user';
    static ID_KEY: string = 'ID';
    static MEETING_URI_KEY: string = 'MeetingUri';
}

class ResultCodeType {
    static SUCCEED: string = '0';
    static FAILED: string = '1';
    static UNAUTHORIZED: string = '2';
    static TOKEN_INVALID: string = '3';
}

class ResultCodeDisplayMsg {
    static SUCCEED: string = 'Operation succeed.';
    static FAILED: string = 'Query failed, please input correct meeting URI.';
    static UNAUTHORIZED: string = 'You are not authorized to classify the meeting, please contact system administrator for further help.';
    static TOKEN_INVALID: string = 'You are not authorized to classify the meeting, please contact system administrator for further help.';
    static NO_DATA: string = 'No Data Matching Your Condition';
    static ERROR: string = 'System error, please contact system administrator for further help.';
}

class PolicyEnforcement{
    static ALLOW: string = 'Enforce_Allow';
    static DENY: string = 'Enforce_Deny';
    static DONT_CARE: string = 'Enforce_DontCare';
    static UNKNOWN: string = 'Enforce_Unknow';
}

class PolicyResultXmlDefition{
    static POLICY_RESULTS_NODE_NAME: string = 'PolicyResults';
    static RESULT_CODE_NODE_NAME: string = 'ResultCode';
    static JOIN_RESULTS_NODE_NAME: string = 'JoinResult';
    static RESULT_NODE_NAME: string = 'Result';
    static ENFORCEMENT_ATTR_NAME: string = 'Enforcement';
    static PARTICIPANT_ATTR_NAME: string = 'Participant';
}

export { MeetingResultType, MeetingUI, MeetingUrlKey, ResultCodeType, ResultCodeDisplayMsg, PolicyEnforcement, PolicyResultXmlDefition };