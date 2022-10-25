import { Meeting } from '../../../../jsbin/Modules/entity';
import { LogHelper } from '../../../../jsbin/Modules/helper';
import { GenericDictionary } from '../../../../jsbin/Modules/map';
import { PolicyResult } from './policyresult';

export class MainViewModel{

    private sResultCode: string;
    private sDateSpan: string;
    private sShowMeetingsOf: string;
    private sSearchContent: string;
    private curMeeting: Meeting;
    private szMeetings: Array<Meeting>;
    private nResultCount: number;

    static RESULT_CODE_PROP: string = 'ResultCode';
    static DATE_SPAN_PROP: string = 'DateSpan';
    static SHOW_MEETING_OF_PROP: string = 'ShowMeetingOf';
    static SEARCH_CONTENT_PROP: string = 'SearchContent';
    static MEETINGS_PROP: string = 'Meetings';
    static CUR_MEETING_PROP: string = 'CurMeeting';
    static RESULT_COUNT_PROP: string = 'ResultCount';

    //static DATE_CHANGE_EVENT: string = 'date-change';
    //static SHOW_MEETING_OF_CHANGE_EVENT: string = 'showmeetingof-change';
    //static SEARCH_CONTENT_CHANGE_EVENT: string = 'searchcontent-change';
    //static CUR_MEETING_CHANGE_EVENT: string = 'curmeeting-change';
    //static MEETINGS_CHANGE_EVENT: string = 'meetings-change';

    Watchers: GenericDictionary<Function>;

    //.Ctor
    constructor() {
        this.szMeetings = new Array<Meeting>();
        this.Watchers = new GenericDictionary<Function>();
    }

    //ResultCode property
    get ResultCode(): string {
        return this.sResultCode;
    }

    set ResultCode(_sResultCode: string) {
        this.sResultCode = _sResultCode;
        this.notify(MainViewModel.RESULT_CODE_PROP);
    }

    //DateSpan property
    get DateSpan(): string {
        return this.sDateSpan;
    }

    set DateSpan(_sDateSpan: string) {
        this.sDateSpan = _sDateSpan;
        this.notify(MainViewModel.DATE_SPAN_PROP);
    }

    //ShowMeetingOf property
    get ShowMeetingOf(): string {
        return this.sShowMeetingsOf;
    }

    set ShowMeetingOf(_sShowMeetingOf: string) {
        this.sShowMeetingsOf = _sShowMeetingOf;
        this.notify(MainViewModel.SHOW_MEETING_OF_PROP);
    }

    //SearchContent property
    get SearchContent(): string {
        return this.sSearchContent;
    }

    set SearchContent(_sSearchContent: string) {
        this.sSearchContent = _sSearchContent;
        this.notify(MainViewModel.SEARCH_CONTENT_PROP);
    }

    //Meetings property
    get Meetings(): Array<Meeting> {
        return this.szMeetings;
    }

    set Meetings(_szMeetings: Array<Meeting>) {
        this.szMeetings = _szMeetings;
        this.notify(MainViewModel.MEETINGS_PROP);
    }

    //CurMeeting property
    get CurMeeting(): Meeting {
        return this.curMeeting;
    }

    set CurMeeting(_curMeeting: Meeting) {
        this.curMeeting = _curMeeting;
        this.notify(MainViewModel.CUR_MEETING_PROP);
    }

    get ResultCount(): number{
        return this.nResultCount;
    }

    set ResultCount(count: number) {
        
        let nCount: number = Number(count);
        this.nResultCount = isNaN(nCount) ? 0 : nCount;
        this.notify(MainViewModel.RESULT_COUNT_PROP);
    }

    private notify(_sPropName: string) {

        if (this.Watchers) {
            if (_sPropName) {
                typeof this.Watchers[_sPropName] === 'function' && this.Watchers[_sPropName].call(undefined);
            }
            else {
                LogHelper.log(`notify() failed, propName: ${_sPropName}.`);
            }
        }
        else {
            LogHelper.log(`notify() failed, Watchers: ${this.Watchers}.`);
        }
    }
}

export class PolicyViewModel{
    private sResultCode: string;
    private szPolicyResults: Array<PolicyResult>;
    Watchers: GenericDictionary<Function>;

    static RESULT_CODE_PROP: string = 'ResultCode';
    static POLICY_RESULTS_PROP: string = 'PolicyResults'

    constructor(){
        this.szPolicyResults = new Array<PolicyResult>();
        this.Watchers = new GenericDictionary<Function>();
    }

    //ResultCode Property
    get ResultCode(): string{
        return this.sResultCode;
    }
    set ResultCode(_sValue: string){
        this.sResultCode = _sValue;
        this.notify(PolicyViewModel.RESULT_CODE_PROP);
    }

    //PolicyResults Property
    get PolicyResults(): Array<PolicyResult>{
        return this.szPolicyResults;
    }
    set PolicyResults(_szValue: Array<PolicyResult>){
        this.szPolicyResults = _szValue;
        this.notify(PolicyViewModel.POLICY_RESULTS_PROP);
    }


    private notify(_sPropName: string) {

        if (this.Watchers) {
            if (_sPropName) {
                this.Watchers[_sPropName].call(undefined);
            }
            else {
                LogHelper.log(`notify() failed, propName: ${_sPropName}.`);
            }
        }
        else {
            LogHelper.log(`notify() failed, Watchers: ${this.Watchers}.`);
        }
    }
}