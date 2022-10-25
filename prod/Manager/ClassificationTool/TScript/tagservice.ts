import { LogHelper } from '../../../../jsbin/Modules/helper';

class TagService{

    //static members
    static State: string = "state";
    static TAG_NAME: string = "tagname";
    static TAG_VALUE: string = "tagvalues";
    static DEFAULT_VALUE: string = "defaultvalue";
    static EDITABLE: string = "editable";
    static MULTI_SELECT: string = "multiselect";
    static MANDATORY: string = "mandatory";
    static RELY_ON: string = 'relyon';

    //instance members
    private m_external: any = <any>window.external;

    //public methods
    getTagsInJSON(): string{
        let sResult: string = '';
        sResult = this.m_external.GetTagsInJSON();
        return sResult;
    }

    updateTagListFromDB(): void{
        this.m_external.UpdateTagListFromDatabase();
    }

    addTag(sTag: string): void{
        if(sTag){
            this.m_external.CacheTag(sTag);
        }
        else{
            LogHelper.log(`addTag failed, tag: ${sTag}`);
        }
    }

    saveChanges(): void{
        this.m_external.SaveChanges();
    }
}

export{ TagService };