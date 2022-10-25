import { TreeView } from '../../treeview';

let data: string = "<Classification type = \"manual\">" + 
                        "<Tag name=\"itar\" editable=\"false\" default=\"yes\" values=\"yes|no\">" + 
                            "<Tag name=\"level\" editable=\"false\" default=\"1\" values=\"1|2|3|4|5\" relyOn=\"yes\">" + 
                                "<Tag name=\"classify\" editable=\"false\" default=\"yes\" values=\"yes|no\" relyOn=\"4|5\"/>" + 
                            "</Tag>" + 
                            "<Tag name=\"kaka\" editable=\"false\" default=\"1\" values=\"1|2|3|4|5\" relyOn=\"yes\"/>" +
                        "</Tag>" + 
                        "<Tag name=\"description\" editable=\"false\" default=\"yes\" values=\"protected meeting|normal meeting\"></Tag>" + 
                    "</Classification>";
let wrapper: HTMLElement = document.getElementById('menu-wrapper');
let tree: TreeView = new TreeView(data, wrapper, true);
tree.on('expand', function(sender: HTMLElement, msg: string){
    console.log(sender);
    console.log(msg);
});

tree.on('select', function(sender: HTMLElement, msg: string){
    console.log(sender);
    console.log(msg);
});

tree.on('add', function(sender: HTMLElement, msg: string){
    console.log(sender);
    console.log(msg);
});

tree.on('delete', function(sender: HTMLElement, msg: string){
    console.log(sender);
    console.log(msg);
});

tree.on('modify', function(sender: HTMLElement, msg: string){
    console.log(sender);
    console.log(msg);
});