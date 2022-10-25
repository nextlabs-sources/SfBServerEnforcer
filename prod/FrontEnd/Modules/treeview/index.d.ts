export class TreeView{
    static selectEvent: string;
    static expandEvent: string;
    static addEvent: string;
    static deleteEvent: string;
    static modifyEvent: string;

    constructor(data:string, container: HTMLElement, showWidget?: boolean);

    on(evtType: 'select'|'expand'|'add'|'delete'|'modify', listener: (this: any, sender: HTMLElement, args: string) => void): void;
    off(evtType: 'select'|'expand'|'add'|'delete'|'modify'): void;
    unfolderClickHandler(this: TreeView, evt: Event): any;
}