export class Dictionary{
    [index:string]:string;
}

export class GenericDictionary<TValue>{
    [index: string]: TValue;
}

export class TagAttributes{
    [index: string]: string;
}

export class KeyValuePair<TValue>{

    Key: string;
    Value: TValue;

    constructor(_sKey: string, _Value: TValue) {
        this.Key = _sKey;
        this.Value = _Value;
    }
}