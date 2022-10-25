import { IStyleModify } from '../../../../jsbin/Modules/linakge-menu';

export class SFBMeetingStyle implements IStyleModify {

    /**
     * 
     * implementations
     */
    ModifySelectStyle(_selectElement: HTMLSelectElement): void {
        _selectElement.className = 'select-tag';
    }

    ModifyParagrahStyle(_paraElement: HTMLParagraphElement): void {
        _paraElement.className = 'p-tag';
    }

    ModifyDivParentStyle(_containerElement: HTMLElement): void {
        _containerElement.style.position = 'relative';
    }

    ModifyEditableBoxStyle(_inputElement: HTMLInputElement): void {
        _inputElement.style.width = '205px';
        _inputElement.style.height = '25.5px';
        _inputElement.style.position = 'absolute';
        _inputElement.style.left = '252px';
        _inputElement.style.top = '2px';
        _inputElement.style.border = '0px solid #efefef';
        _inputElement.style.fontSize = '14px';
        _inputElement.style.paddingLeft = '10px';
    }
}