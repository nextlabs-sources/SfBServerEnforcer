export class Util{
    static forEach(arrays: Array<any>|HTMLCollection, callback: Function, scope:any): void;
    static delayExecute(func: Function, scope: any): void;
    static createEvent(evtName: string, canBubble?: boolean, canCancel?: boolean): Event;
}
