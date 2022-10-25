import { Meeting } from '../../../../jsbin/Modules/entity';
import { PolicyResult } from './policyresult';

class DataModel {

    //MeetingCommands Attributes
    OperationType: string;
    SipUri: string;
    TokenId: string;

    ResultCode: string;

    //Filters Attributes
    DateSpan: string;
    ShowMeetingOf: string;

    Meetings: Array<Meeting>;

    constructor() {
        this.Meetings = new Array<Meeting>();
    }
}

class PolicyDataModel{
    ResultCode: string;
    PolicyResults: Array<PolicyResult>;

    constructor(){
        this.PolicyResults = new Array<PolicyResult>();
    }
}

export { DataModel, PolicyDataModel };