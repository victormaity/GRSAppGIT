$(document).ready(function () {
    $.jstree.defaults.core.themes.variant = "small";
    $('#jstreeview').jstree({
        //'plugins': ["checkbox"],
        //"checkbox": {
        //    "visible": false,
            //"three_state": false,
            //"cascade": "down",
            //"whole_node": "false"
        //},
        'core': {
            //"data":
            //    function (obj, cl) {
            //        var data = getItems(obj);
            //        cl.call(this, data);
            //    }
            //"multiple" : false,
            "data": {
                'type': "POST",
                'url': function (node) {
                    var url = "";
                    var ipregex = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
                    var dateregex = "([0-9]{2}\/){2}([0-9]{4}) ([0-9]{2}:){2}[0-9]{2}";
                    if (node.id === "#") {
                        url = '../GetTestSuits';
                    }
                    else if (node.text.match(ipregex)) {
                        url = "../GetTestSuitsByKeyAndIp";
                    }
                    else if (node.text.match(dateregex)) {
                        url = "../GetTestSuitDetails";
                    }
                    else {
                        url = '../GetTestSuitsByKey';
                    }
                    return url;
                   
                },
                'data': function (node) {
                    var dateregex = "([0-9]{2}\/){2}([0-9]{4}) ([0-9]{2}:){2}[0-9]{2}";
                    if (node.id === "#") { //GetTestSuits
                        return "";
                    }
                    else if (node.parent === "#") { //GetTestSuitsByKey
                        var cycleId = $(location).attr("pathname").split("/")[4];
                        return "{'cycleId': '" + cycleId + "','key': '" + node.id + "'}";
                    }
                    else if (node.text.match(dateregex)) { //GetTestSuitDetails
                        return "{'id': '" + node.id + "'}";
                    }
                    else { //GetTestSuitsByKeyAndIp
                        var tsname = node.parent;
                        var ip = node.id.split('/')[1];
                        var cycleId = $(location).attr("pathname").split("/")[4];
                        return "{'cycleId': '"+cycleId+"','ip': '" + ip + "','tsname': '" + tsname + "'}";
                    }
                },
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    
                }
            }
        }
    })
    .bind("changed.jstree", function (e, data) {
        if (data.action === "select_node") {
            if (data.node.id.match("^[0-9]+$")) {
                if (!data.event.ctrlKey) {
                    //var tsSelected = 0;
                    //for (var i in data.selected) {
                    //    if (data.selected[i].match("^[0-9]+$")) {
                    //        tsSelected++;
                    //    }
                    //}
                    //if (tsSelected === 1) {
                    renderLegend(data.node.id);
                    renderTestSuite(data.node.id, '', "../GetTestSuiteName", "../GetTestSuite", "~/Content/assets/css/images/loading.gif");
                    //}
                }
            }
            else {
                $("#jstreeview").jstree("select_node", data.node.children_d, true);
            }
            if (data.node.parents) {
                $("#jstreeview").jstree("select_node", data.node.parents, true);
            }

            //else {
            //    $("#jstreeview").jstree("select_node", data.node.children);
            //}
        }
        else if (data.action === "deselect_node") {
            $("#jstreeview").jstree("deselect_node", data.node.children);
        }
    })
    ;
});


var selectAllTree = function () {
    $('#jstreeview').jstree("select_all");
}

var deselectAllTree = function () {
    $('#jstreeview').jstree("deselect_all");
}

var deleteSelectedTestSuits = function (cycleId) {
    //debugger;
    var selected = $('#jstreeview').jstree("get_top_selected", true); //tsnames
    var tsnames = [];
    var tsips = [];
    var tsids = [];
    for (var i in selected) {
        if (selected[i].children.length > 0) { //tsips
            //debugger;
            var childrenids = selected[i].children;
            for (var j in childrenids) {
                var name = childrenids[j].split("/")[0];
                var ip = childrenids[j].split("/")[1];
                var childnode = $("#jstreeview").jstree("get_node", $('li[id$="' + ip + '"][id^="' + name + '"]'));
                if (childnode.children.length > 0) {
                    for (var k in childnode.children) {
                        var tsnode = $("#jstreeview").jstree("get_node", $('li[id=' + childnode.children[k] + ']'));
                        if (tsnode.state.selected === true) {
                            tsids.push(childnode.children[k]);
                        }
                    }
                }
                else {
                    if (childnode.state.selected) {
                        tsips.push(childnode.id);
                    }
                }
            }
        }
        else {
            tsnames.push(selected[i].id);
        }
    }
    $.ajax({
        type: "POST",
        url: "../RemoveTestSuits",
        beforeSend: function () {
            $("#testsuiteloading").addClass("fa-spin");
        },
        data: '{"cycleId":' + cycleId + ', "tsIds":' + JSON.stringify(tsids) + ', "tsNames":' + JSON.stringify(tsnames) + ', "tsNamesAndIps":' + JSON.stringify(tsips) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $("#testsuiteloading").removeClass("fa-spin");
            if (msg.type == "Error") {
                ShowMessage(msg.type, msg.text);
            } else {
                window.location.href = location.href.split("?testSuite")[0];
            }
        }
    });
};

var migrateTestSuite = function (event) {
    event.preventDefault();
    var cycleId = $(location).attr("pathname").split("/")[4];
    var checkedIds = getSelectedTestSuitesIds(cycleId);

    if (checkedIds.length != 0) {
        var targetTC = $("select[name='targetTestCycle']").val();
        $.ajax({
            type: "POST",
            url: "../MigrateTestSuites",
            data: '{"cycleId":' + cycleId + ', "testSuitesIds": [' + checkedIds + '], "targetTestCycle":' + targetTC + '}',
            contentType: "application/json; charset=utf-8",
            beforeSend: function () {
                $('#modal-migrate').modal("hide");
            },
            success: function (msg) {
                if (msg.type == "Error") {
                    ShowMessage(msg.type, msg.text);
                } else {
                    window.location.href = location.href.split("?testSuite")[0];
                }
            }
        });
    }
    else {
        ShowMessage("Message", "Please select test sets to migrate!");
    }

    return false;
}

var mergeTestSuite = function (cycleId) {
    if ($('#jstreeview').jstree("get_top_selected", true).length == 1) {
        var checkedIds = getSelectedTestSuitesIds(cycleId);
        if (checkedIds.length > 1) {
            $.ajax({
                type: "POST",
                url: "../MergeTestSuites",
                data: '{ "testSuitesIdsToMerge": [' + checkedIds + ']}',
                contentType: "application/json; charset=utf-8",
                beforeSend: function () {
                    $("#testsuiteloading").addClass("fa-spin");
                },
                success: function (msg) {
                    if (msg.type == "Error") {
                        ShowMessage(msg.type, msg.text);
                        $("#testsuiteloading").removeClass("fa-spin");
                    } else {
                        window.location.href = location.href.split("?testSuite")[0];
                    }
                }
            });
        } else {
            ShowMessage("Message", "Please select two or more test sets to merge!");
        }
    }
    else {
        $("#testsuiteloading").removeClass("fa-spin");
        ShowMessage("Message", "You can merge elements only from one test set!");
    }
    return false;
};


var exportInQcSelectedTestSuits = function (cycleId) {
    //debugger;
    var selected = $('#jstreeview').jstree("get_top_selected", true); //tsnames
    var tsnames = [];
    var tsips = [];
    var tsids = [];
    for (var i in selected) {
        if (selected[i].children.length > 0) { //tsips
            var childrenids = selected[i].children;
            for (var j in childrenids) {
                var name = childrenids[j].split("/")[0];
                var ip = childrenids[j].split("/")[1];
                var childnode = $("#jstreeview").jstree("get_node", $('li[id$="' + ip + '"][id^="' + name + '"]'));
                if (childnode.children.length > 0) {
                    for (var k in childnode.children) {
                        var tsnode = $("#jstreeview").jstree("get_node", $('li[id=' + childnode.children[k] + ']'));
                        if (tsnode.state.selected === true) {
                            tsids.push(childnode.children[k]);
                        }
                    }
                }
                else {
                    if (childnode.state.selected) {
                        tsips.push(childnode.id);
                    }
                }
            }
        }
        else {
            tsnames.push(selected[i].id);
        }
    }
    $.ajax({
        type: "POST",
        url: "../../QcExport/QcExport",
        beforeSend: function () {
            $("#testsuiteloading").addClass("fa-spin");
        },
        /*  data: '{ tsIds: " ' + ids + ' "}',*/
        data: '{"cycleId":' + cycleId + ', "tsIds":' + JSON.stringify(tsids) + ', "tsNames":' + JSON.stringify(tsnames) + ', "tsNamesAndIps":' + JSON.stringify(tsips) + '}',
        traditional: true,
        /*  data: "{ vals: "+ids+" }",*/
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $("#testsuiteloading").removeClass("fa-spin");
            if (msg.type == "Error") {
                ShowMessage(msg.type, msg.text);
            } else {
                window.location.href = location.href.split("?testSuite")[0];
            }
        }
    });
};

var exportToExcelSelectedTestSuits = function (cycleId) {
    //debugger;
    var selected = $('#jstreeview').jstree("get_top_selected", true); //tsnames
    var tsnames = [];
    var tsips = [];
    var tsids = [];
    for (var i in selected) {
        if (selected[i].children.length > 0) { //tsips
            var childrenids = selected[i].children;
            for (var j in childrenids) {
                var name = childrenids[j].split("/")[0];
                var ip = childrenids[j].split("/")[1];
                var childnode = $("#jstreeview").jstree("get_node", $('li[id$="' + ip + '"][id^="' + name + '"]'));
                if (childnode.children.length > 0) {
                    for (var k in childnode.children) {
                        var tsnode = $("#jstreeview").jstree("get_node", $('li[id=' + childnode.children[k] + ']'));
                        if (tsnode.state.selected === true) {
                            tsids.push(childnode.children[k]);
                        }
                    }
                }
                else {
                    if (childnode.state.selected) {
                        tsips.push(childnode.id);
                    }
                }
            }
        }
        else {
            tsnames.push(selected[i].id);
        }
    }
    $.ajax({
        type: "POST",
        url: "../../ExportToExcel/ExportTestSets",
        beforeSend: function () {
            $("#testsuiteloading").addClass("fa-spin");
        },
        /*  data: '{ tsIds: " ' + ids + ' "}',*/
        data: '{"cycleId":' + cycleId + ', "tsIds":' + JSON.stringify(tsids) + ', "tsNames":' + JSON.stringify(tsnames) + ', "tsNamesAndIps":' + JSON.stringify(tsips) + '}',
        traditional: true,
        /*  data: "{ vals: "+ids+" }",*/
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $("#testsuiteloading").removeClass("fa-spin");
            if (msg.type == "Error") {
                ShowMessage(msg.type, msg.text);
            }
            else {
                ShowMessage("Message", "Report was generated");
                window.location.href = msg.text;
            }
        }
    });
};

function startAutoAnalyzeMultiple(cycleId, type) {
    var ids = getSelectedTestSuitesIds(cycleId);
    for (var i in ids) {
        $.ajax({
            type: "POST",
            url: "../StartAutoAnalyze",
            async: true,
            //data: "{'suiteId':" + selectedItems[i].getAttribute("id") + ", 'type':" + type + "}",
            data: "{'suiteIds':" + JSON.stringify(ids[i]) + ", 'type':'" + type + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            beforeSend: function () {
                $("li[id=" + ids[i]+"]").addClass("jstree-loading");
                ShowMessage("Message", "Auto analysis is started");
            },
            error: function () { },
            success: function (msg) {
                if (IsJsonString(msg) && jQuery.parseJSON(msg).type == "Error") {
                    ShowMessage(jQuery.parseJSON(msg).type, jQuery.parseJSON(msg).text);
                }
                else {
                    renderLegend($("#testLegend > table").attr("id"));
                }
                $("li[id=" + ids[i] + "]").removeClass("jstree-loading");
                ShowMessage("Message", "Auto analysis is done");
            }
        });
        //    })(i)
    }

}

var getSelectedTestSuitesIds = function (cycleId) {
    var selected = $('#jstreeview').jstree("get_top_selected", true); //tsnames
    var tsnames = [];
    var tsips = [];
    var tsids = [];
    for (var i in selected) {
        if (selected[i].children.length > 0) { //tsips
            var childrenids = selected[i].children;
            for (var j in childrenids) {
                var name = childrenids[j].split("/")[0];
                var ip = childrenids[j].split("/")[1];
                var childnode = $("#jstreeview").jstree("get_node", $('li[id$="' + ip + '"][id^="' + name + '"]'));
                if (childnode.children.length > 0) {
                    for (var k in childnode.children) {
                        var tsnode = $("#jstreeview").jstree("get_node", $('li[id=' + childnode.children[k] + ']'));
                        if (tsnode.state.selected === true) {
                            tsids.push(childnode.children[k]);
                        }
                    }
                }
                else {
                    if (childnode.state.selected) {
                        tsips.push(childnode.id);
                    }
                }
            }
        }
        else {
            tsnames.push(selected[i].id);
        }
    }
    var ids;
    $.ajax({
        type: "POST",
        url: "../GetSelectedTestSuitIds",
        async: false,
        beforeSend: function () {
            $("#testsuiteloading").addClass("fa-spin");
        },
        data: '{"cycleId":' + cycleId + ', "tsIds":' + JSON.stringify(tsids) + ', "tsNames":' + JSON.stringify(tsnames) + ', "tsNamesAndIps":' + JSON.stringify(tsips) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        error: function () { },
        success: function (msg) {
            $("#testsuiteloading").removeClass("fa-spin");
            if (msg.type == "Error") {
                ShowMessage(msg.type, msg.text);
            } else {
                ids = msg.text;
            }
        }
    });
    return ids;
}