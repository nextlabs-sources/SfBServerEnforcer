﻿import { IStyleModify } from '../../../../jsbin/Modules/linakge-menu';

class HierarchicalStructureStyleModify implements IStyleModify {

    /**
     * 
     * implementations
     */
    ModifySelectStyle(_selectElement: HTMLSelectElement): void {
        _selectElement.style.width = '200px';
        _selectElement.style.height = '40px';       // Right part editbox/combox height
        _selectElement.style.marginLeft = '10px';
        _selectElement.style.paddingLeft = '0px';
        _selectElement.style.marginRight = '10px';
        _selectElement.style.fontSize = '16px';
    }

    ModifyParagrahStyle(_paraElement: HTMLParagraphElement): void {
        _paraElement.style.width = '200px';
        _paraElement.style.height = '18px';
        _paraElement.style.lineHeight = '16px';
        _paraElement.style.fontSize = '16px';
        _paraElement.style.fontFamily = 'Segoe UI Semibold';
        _paraElement.style.fontWeight = 'bold';
        _paraElement.style.textAlign = 'left';
        _paraElement.style.margin = '0px';
        _paraElement.style.paddingLeft = '10px';
        _paraElement.style.overflow = 'hidden';
        _paraElement.style.display = 'inline-block';
    }

    ModifyDivParentStyle(_containerElement: HTMLElement): void {
        _containerElement.style.position = 'relative';
        _containerElement.style.marginTop = '10px';
        _containerElement.style.marginBottom = '10px';
    }

    ModifyEditableBoxStyle(_inputElement: HTMLInputElement): void {
        _inputElement.style.width = '155px';
        _inputElement.style.height = '16px';
        _inputElement.style.position = 'absolute';
        _inputElement.style.left = '221px';
        _inputElement.style.top = '1px';
        _inputElement.style.border = '0px solid #efefef';
        _inputElement.style.fontSize = '16px';
        _inputElement.style.paddingLeft = '5px';
    }

}

export { HierarchicalStructureStyleModify };