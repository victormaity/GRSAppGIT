/*-------------------------------------------SUNDRAM----------------------------------------------------*/

var showFilesLaunch = function (list) {
    var paths = [], i = 0;
    list = list;
    if (list instanceof Array) {
        for (i = 0; i < list.length; i++) {
            if (list[i]) {
                var path = (list[i].Path) ? list[i].Path !== "com.thomsonreuters.cortellis.cucumber" ? list[i].Path : "" : "";
                var file = list[i].Name;
                paths.push({ name: file, path: path });
            }
        }
    } else {
        paths = list;
    }

    var tree = explodeTree(paths);
    var finalList = renderTree(tree);

    $('#features').html(finalList);
    $.jstree.create('#features');

}

var showTagsLaunch = function (list) {
    var tree = renderTagsTree(list);
    $('#tags').html(tree);
    $.jstree.create('#tags');
}

/*-----------------------------------------------------------------------------------------------*/

var showFiles = function (list) {
    var paths = [], i = 0;
    list = list.features;
    if (list instanceof Array) {
        for (i = 0; i < list.length; i++) {
            if (list[i]) {
                var path = (list[i].Path) ? list[i].Path !== "com.thomsonreuters.cortellis.cucumber" ? list[i].Path : "" : "";
                var file = list[i].Name;
                paths.push({ name: file, path: path });
            }
        }
    } else {
        paths = list;
    }

    var tree = explodeTree(paths);
    var finalList = renderTree(tree);

    $('#features').html(finalList);
    $.jstree.create('#features');

}

var showTags = function (list) {
    var tree = renderTagsTree(list.Tags);
    $('#tags').html(tree);
    $.jstree.create('#tags');
}

function explodeTree(data) {
    var tree = {};
    for (var i in data) {
        var parts = data[i].path.split('\\');

        var parent = tree;
        for (var j in parts) {
            if (!parent[parts[j]]) {
                parent[parts[j]] = {};
            }

            if (j == parts.length - 1) {
                parent[parts[j]][data[i].name] = {};
            }

            parent = parent[parts[j]];
        }
    }
    return tree;
}

function renderTree(tree) {
    var items = [];
    var jstreedata = '{"icon":"green glyphicon glyphicon-leaf"}';
    for (var i in tree) {
        if (i) {
            var icon = '';
            if (isEmpty(tree[i])) {
                icon = " data-jstree='" + jstreedata + "'";
            }
            items.push('<li' + icon + '>' + i + (!isEmpty(tree[i]) ? renderTree(tree[i]) : '') + '</li>');
        } else {
            var items1 = Object.keys(tree[i]);
            for (var j in items1) {
                icon = " data-jstree='" + jstreedata + "'";
                items.push('<li ' + icon + '>' + items1[j] + '</li>');
            }
        }
    }

    return '<ul>' + items.join('') + '</ul>';
}
function renderTagsTree(tree) {
    var items = [];
    var jstreedata = '{"icon":"green glyphicon glyphicon-cog"}';
    var icon = " data-jstree='" + jstreedata + "'";
    //debugger;
    for (var i in tree) {
        items.push('<li' + icon + '>' + tree[i] + '</li>');
    }

    return '<ul>' + items.join('') + '</ul>';
}

function isEmpty(obj) {
    return Object.keys(obj).length === 0;
}