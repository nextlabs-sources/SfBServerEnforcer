declare class Dictionary{
    [index:string]:string;
}

declare class GenericDictionary<TValue>{
    [index: string]: TValue;
}

declare class TagAttributes extends Dictionary{
    [index: string]: string;
}

declare class KeyValuePair<TValue>{

    Key: string;
    Value: TValue;

    constructor(_sKey: string, _Value: TValue);
}

export {Dictionary, GenericDictionary, TagAttributes, KeyValuePair};

