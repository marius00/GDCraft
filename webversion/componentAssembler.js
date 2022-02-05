
function ComponentCraftVm(data) {
    const self = this;
    this.items = ko.observable(data);
}

function ItemSumVm() {
    const self = this;
    this.sum = ko.observableArray([]);

}

var tmp = {
    "name": 'Glacier',
    "bitmap": 'tier1_relic_04.tex.png',
    "numRequired": 1,
    "numOwned": 0,
    "numCraftable": 0,
    "isComplete": false,
    "cost": []
};
var componentCraftVM = new ComponentCraftVm(tmp);
var itemSumVM = new ItemSumVm();

function updateTree() {

    // The tree framework is very fragile
    // It needs a new element each time.
    const n = Math.floor(Math.random() * 1000000);
    const rElem = `<div id='tree${n}'></div>`;
    $('#mytree').html(rElem);
    $(`#tree${n}`).html($('#assembledItem').find(">:first-child").clone());

    $(`tree${n}`).show(); // TODO: is this not a bug?
    $(`#tree${n}`).find('>:first-child').jstree({ "plugins": ['checkbox'] });
    $(`#tree${n}`).find('>:first-child').on('changed.jstree', function (e, data) {
        updateSum();
        if (data.action == 'select_node') {
            $('.jstree').jstree(true).close_node([data.node.id]);
        }
        else if (data.action == 'deselect_node') {
            $('.jstree').jstree(true).open_node([data.node.id]);
        }
    });
}

let recipes = {};

var lastRecord = '';
function updateView(record) {
    componentCraftVM.items(dataset[record]);
    itemSumVM.sum([]);
    updateTree();
    updateSum();
	console.log('Recipe updated to', dataset[record]);
	history.pushState(null, '', 'https://grimdawn.evilsoft.net/crafting/?q=' + record)
}


____callbackSetItemList();
function ____callbackSetItemList() {

	
    for (let key in dataset) {
        const entry = dataset[key];
        $('#relicSelect').append($(`<option value=${entry.record}>${entry.name}</option>`));
    }

    $('.chosen-select').chosen();
    

    $('#relicSelect').change(function () {
        $('#recipeSelect').val('').trigger('chosen:updated');
        $('#componentSelect').val('').trigger('chosen:updated');
        updateView($('#relicSelect').val());
    });

}

/*
let annoyingQuickfixTimeout = setInterval(() => {
    if ($('#tab-crafting').is(':visible')) {
        console.debug('Crafting tab activated');
        data.requestRecipeList('____callbackSetItemList');
        clearInterval(annoyingQuickfixTimeout);
    }
}, 500);
*/
// 


$(document).ready(function () {
    ko.applyBindings(componentCraftVM, document.getElementById('assembledItem'));
    ko.applyBindings(itemSumVM, document.getElementById('itemSum'));

});


function updateSum() {
    var ids = $('.jstree').jstree(true).get_selected();
    const nodes = $('.jstree').jstree(true).get_json('#', { flat: true });

    $('.jstree').on('changed.jstree',
        function (e, data) {

            // Close all selected nodes
            const selectedNodes = $('.jstree').jstree(true).get_selected();
            for (let idx = 0; idx < selectedNodes.length; idx++) {
                $('.jstree').jstree(true).close_node(selectedNodes[idx]);
            }
        });

    var unselected = nodes.filter(function (v) {
        return ids.indexOf(v.id) == -1;
    });



    var result = {};
    for (let i = 0; i < unselected.length; i++) {
        // if any has me as parent, then don't render me.

        var safe = true;
        for (let m = 0; m < unselected.length; m++) {
            if (unselected[m].parent == unselected[i].id) {
                safe = false;
                break;
            }
        }

        if (safe) {
            const obj = {
                name: unselected[i].data.itemname,
                count: unselected[i].data.num,
                icon: unselected[i].icon
            };

            if (result[obj.icon] === undefined) {
                result[obj.icon] = obj;
            } else {
                result[obj.icon].count += obj.count;
            }
        }
    }


    const values = Object.keys(result).map(function (key) {
        return result[key];
    });

    itemSumVM.sum(values);
}

$(document).ready(() => {
	let initialRecord = new URLSearchParams(window.location.search).get('q');
	if (initialRecord) {
		updateView(initialRecord);
	}
});