import { LogHelper } from '../../../../jsbin/Modules/helper';

export class ServiceProvider{

    //instance members
    private m_external: any = <any>window.external;

    //public methods
    getTagsInJSON(): string{
        let sResult: string = '';
        sResult = this.m_external.GetTagsInJSON();
        return sResult;
    }

    saveTagsInJSON(sTags: string): void{
        this.m_external.SaveTagsInJSON(sTags);
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

    saveTagChanges(): void{
        this.m_external.SaveTagChanges();
    }

        getSchemasInJSON(): string{
        let sResult: string = '';
        
        sResult = this.m_external.GetSchemasInJSON();
        
        return sResult;
    }

    updateSchemaListFromDB(): void{
        this.m_external.UpdateSchemaListFromDatabase();
    }

    addSchema(sSchema: string): void{
        if(sSchema){
            this.m_external.CacheSchema(sSchema);
        }
        else{
            LogHelper.log(`addSchema failed, schema string: ${sSchema}`);
        }
    }

    saveSchemaChanges(): void{
        this.m_external.SaveSchemaChanges();
    }    
}