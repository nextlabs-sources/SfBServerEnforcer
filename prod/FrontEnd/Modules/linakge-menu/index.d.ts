import { Dictionary } from '../map';
import { LinkageData } from '../entity';

interface IDataSourceFormat {
    Format(sDataSource: string, sDataMIMEType?: string): LinkageData;
}

interface IStyleModify {
    ModifySelectStyle(selectElement: HTMLSelectElement): void;
    ModifyParagrahStyle(paraElement: HTMLParagraphElement): void;
    ModifyDivParentStyle(divElement: HTMLDivElement): void;
    ModifyEditableBoxStyle(inputElement: HTMLInputElement): void;
}

interface IEditable {
    AddEditableBoxToSelect(_selectElement: HTMLSelectElement): HTMLInputElement;
}

declare class XmlDataSourceFormat implements IDataSourceFormat {
    Format(sDataSource: string, sDataMIMEType?: string): LinkageData;
}

declare class LinkageMenu implements IEditable{

    constructor(_modifier: IStyleModify, _formatter: IDataSourceFormat, _container: HTMLElement, _sDataSource: string);

    bind(): void;
    getSelectTagDict(): Dictionary;
    setSelectTagDict(_rootElement: HTMLElement, _selectTagDict: Dictionary): void;
    AddEditableBoxToSelect(_selectElement: HTMLSelectElement): HTMLInputElement;

    protected onSelectChange(evt: Event): void;
}
export { IDataSourceFormat, IStyleModify, XmlDataSourceFormat, LinkageMenu, IEditable };