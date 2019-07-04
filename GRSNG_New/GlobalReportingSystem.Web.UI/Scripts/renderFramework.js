function downloadChildren(id, parentPart, controllerMethod) {
    var parent = $("#" + parentPart).find("label").attr("path");
    if (!$("#" + parent + ">ul").is("*")) {
        $.ajax({
            type: "POST",
            url: controllerMethod,//"@Url.Action("DownloadChildren")",
            data: "{ 'id': " + id + ", 'parent':'" + parent + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            beforeSend: function () {
                $("label[path='" + parent + "']").append("<i class='fa fa-spinner fa-spin'/>");
            },
            error: function (request, status, error) {
            },
            success: function (msg) {
                if (IsJsonString(msg) && jQuery.parseJSON(msg).type == "Error") {
                    ShowMessage(jQuery.parseJSON(msg).type, jQuery.parseJSON(msg).text);
                } else {
                    $("#" + parent).append(msg);
                    $("label[path='" + parent + "']>i").remove();
                }
            }
        });
    }
}

function setDraggable(element) {

    if (element.attr("chosen") == "0") {
        element.attr("chosen", "1");
        element.css("font-weight", "bold");
    } else {
        element.attr("chosen", "0");
        element.css("font-weight", "");
    }
}