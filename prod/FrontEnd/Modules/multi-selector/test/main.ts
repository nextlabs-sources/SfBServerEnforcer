import { MultiSelector } from '../../multi-selector';

let data: string [] = ['Charizard', 'Blastoise', 'Venusaur'];
let container: HTMLElement = document.getElementById('wrapper');
let select: MultiSelector = new MultiSelector(container, data);

select.on('selected', function(sender: HTMLElement, msg: string): void{
    console.log(sender);
    console.log(msg);
    console.log(select.selectedValues.join('|'));
}, undefined);

select.on('removed', function(sender: HTMLElement, msg: string): void{
    console.log(sender);
    console.log(msg);
}, undefined);

select.on('focused', function(sender: HTMLElement, msg: string): void{
    console.log(sender);
    console.log(msg);
}, undefined);

select.on('blurred', function(sender: HTMLElement, msg: string): void{
    console.log(sender);
    console.log(msg);
}, undefined);

select.setSelectValues(['Charizard', 'Blastoise']);