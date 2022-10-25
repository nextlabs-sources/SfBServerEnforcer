import { LogHelper } from '../../../../jsbin/Modules/helper';
export class SchemaService{
    //memebers
    private m_external: any = window.external;

    //static members
    static SCHEMA_NAME: string = 'schemaname';
    static SCHEMA_DATA: string = 'data';
    static SCHEMA_DESCRIPTION: string = 'description';
    static STATE: string = 'state';

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

    saveChanges(): void{
        this.m_external.SaveSchemaChanges();
    }
}