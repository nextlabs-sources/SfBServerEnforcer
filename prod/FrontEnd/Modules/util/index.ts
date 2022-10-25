import { LogHelper } from '../helper';

class Util{
    static forEach(arrays: Array<any>|HTMLCollection, callback: Function, scope?:any): void{
        if(arrays && callback){
            for(let i:number = 0; i < arrays.length; i++){
                callback.call(scope, arrays[i]);
            }
        }
        else{
            LogHelper.log(`forEach failed, array: ${arrays}`);
        }
    }

    static delayExecute(func: Function, scope: any): void{
        setTimeout(function() {
            func.call(scope);
        }, 0);
    }

    static createEvent(evtName: string, canBubble: boolean = true, canCancel: boolean = true): Event{
        let evt: Event = undefined;

        if(evtName){
            try{
                evt = new Event(evtName, { bubbles: canBubble, cancelable: canCancel});
            }
            catch(e){
                evt = document.createEvent('Event');
                evt.initEvent(evtName, canBubble, canCancel);
            }
        }
        else{
            LogHelper.log(`createEvent failed, evtName: ${evtName}`);
        }

        return evt;
    }
}

export { Util };