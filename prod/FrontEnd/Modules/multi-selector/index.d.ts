export class MultiSelector{

    static ItemSelectedEvent: string;
    static ItemRemovedEvent: string;
    static SelectorFocusEvent: string;
    static SelectorBlurEvent: string;

    constructor(container: HTMLElement, dataArray: string[]);

    get selectedValues(): string[];

    on(evtType: 'selected' | 'removed' | 'focused' | 'blurred', listener: (sender: HTMLElement, msg: string) => any, scope?: any): void;
    off(evtType: 'selected' | 'removed' | 'focused' | 'blurred'): void;
    setSelectValues(values: string[]): void;
}