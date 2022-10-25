import { MainViewModel, PolicyViewModel } from './mainviewmodel';
import { DataModel, PolicyDataModel } from './datamodel';
import { MeetingResultType, PolicyEnforcement } from './meetingconstants';
import { LogHelper } from '../../../../jsbin/Modules/helper';
import { PolicyResult } from './policyresult';

class ViewModelAdapter {

    Adapt(_dataModel: DataModel, _viewModel: MainViewModel): void {

        if (_dataModel && _viewModel) {
            _viewModel.DateSpan = _dataModel.DateSpan;
            _viewModel.ShowMeetingOf = _dataModel.ShowMeetingOf;
            _viewModel.ResultCode = _dataModel.ResultCode;
            _viewModel.SearchContent = _dataModel.SipUri;
            _viewModel.ResultCount = _dataModel.Meetings.filter(meeting => meeting.Uri).length;

            switch (_dataModel.OperationType) {
                case MeetingResultType.SEARCH_TYPE: {
                    if (_dataModel.Meetings && _dataModel.Meetings.length === 1) {
                        _viewModel.CurMeeting = _dataModel.Meetings[0];
                    }
                    break;
                }
                case MeetingResultType.SHOW_TYPE: {
                    _viewModel.Meetings = _dataModel.Meetings;
                    break;
                }
                default: {
                    break;
                }
            }
        }
        else {
            LogHelper.log(`Adapt() failed, dataModel: ${_dataModel}, viewModel: ${_viewModel}.`);
        }
    }

    AdaptPolicyViewModel(_dataModel: PolicyDataModel, _viewModel: PolicyViewModel): void {
        if(_dataModel && _viewModel){
            _viewModel.ResultCode = _dataModel.ResultCode;

            let szPolicyResults: Array<PolicyResult> = new Array<PolicyResult>();

            for(let i: number = 0; i < _dataModel.PolicyResults.length; i++){
                if(_dataModel.PolicyResults[i].Enforcement === PolicyEnforcement.DENY){
                    szPolicyResults.push(_dataModel.PolicyResults[i]);
                }
            }

            if(szPolicyResults.length > 0){
                _viewModel.PolicyResults = szPolicyResults;
            }
            else{
                _viewModel.PolicyResults = undefined;
            }
        }
        else{
            LogHelper.log(`AdaptPolicyViewModel failed, dataModel: ${_dataModel}, viewModel: ${_viewModel}.`);
        }
    }
}

export { ViewModelAdapter };