function renderTestSteps(tcId, controllerMethod) {
    if ($("tr[key='TC " + tcId + "']").length == 0) {
        $.ajax({
            type: "POST",
            url: controllerMethod,
            async: true,
            data: "{'id':" + tcId + "}",

            contentType: "application/json; charset=utf-8",
            dataType: "html",
            beforeSend: function () {
                $("#TC_" + tcId + " > td > a:first > i").addClass("fa-spin");
                //$("#TC_" + tcId + " > td > a > i").addClass("fa-spin");
            },
            error: function () {
                //debugger;
            },
            success: function (msg) {
                if (IsJsonString(msg) && jQuery.parseJSON(msg).type == "Error") {
                    ShowMessage(jQuery.parseJSON(msg).type, jQuery.parseJSON(msg).text);
                } else {
                    $('html, body').animate({
                        scrollTop: $("#TC_" + tcId).offset().top
                    }, 1000);

                    $("#TC_" + tcId + " > td > a:first > i").removeClass("fa-spin");
                    //$("#TC_" + tcId + " > td > a > i").removeClass("fa-spin");
                    $("#TC_" + tcId).after(msg.trim());
                }
            }
        });
    }
    else {
        var tags = $("tr[key='TC " + tcId + "']");

        if (tags[0].getAttribute("style") == null) {
            var tagsStepsAndSubSteps = $("tr[key*='TC " + tcId + "']");
            for (var i = 0; i < tagsStepsAndSubSteps.length; i++) {
                tagsStepsAndSubSteps[i].setAttribute("style", "display:none");
            }
        } else {
            for (var i = 0; i < tags.length; i++) {
                tags[i].removeAttribute("style");
            }
            $('html, body').animate({
                scrollTop: $("#TC_" + tcId).offset().top
            }, 1000);
        }
    }
}

function renderTestSuite(tsId, filter, controllerNameMethod, controllerMethod, srcPath) {
    var tsName = "";
    $.post(controllerNameMethod, { id: tsId })
.done(function (data) {
    tsName = data;
});

    $.ajax({
        type: "POST",
        url: controllerMethod,
        async: true,
        data: "{'id':" + tsId + ", 'filter':'" + filter + "'}",

        contentType: "application/json; charset=utf-8",
        dataType: "html",
        beforeSend: function () {
            $("#tc_container").append("<img align='middle' src='" + srcPath + "'/>");
        },
        error: function () { },
        success: function (msg) {
            if (IsJsonString(msg) && jQuery.parseJSON(msg).type == "Error") {
                ShowMessage(jQuery.parseJSON(msg).type, jQuery.parseJSON(msg).text);
            } else {
                //debugger;
                $("#tc_container").html(tsName);
                $("#ts_id").val(tsId);
                $("#testSuite").html(msg);
                if (location.href.indexOf("?testSuite=") == -1) {
                    history.pushState(null, null, "?testSuite=" + tsId);
                }
            }
        }
    });
}


function showscreenshot(screens, image, isAttachment, attachmentsPath) {
    if ($("#modal-form").length == 0) {
        $("#modal-form_disabled").attr('id', "modal-form");
    }
    if (image.toString() == "true") {
        if (isAttachment == 'false') {
            $("#titleAttachment").text('Screenshot');
            $("#modalBody")[0].innerHTML = "<a target='_blank' href=" + attachmentsPath + "/Screenshots/" + screens + "><img class='SSPicture' src=" + attachmentsPath + "/Screenshots/" + screens + " /></a>";
        }
        else
            if (isAttachment != 'false') {
                $("#titleAttachment").text('Attachment');
                $("#modalBody")[0].innerHTML = "<a target='_blank' href=" + attachmentsPath + "/Screenshots/" + screens.split('/')[1] + "><img class='SSPicture' src=" + attachmentsPath + "/Screenshots/" + screens.split('/')[1] + " /></a>";
            }

    } else if (image.toString() == "false") {
        $("#titleAttachment")[0].innerHTML = 'Attachment';

        $("#modalBody")[0].innerHTML = "<iframe class='attachmentIframe', src=" + attachmentsPath + "/Attachments/" + screens + " />";
        if (screens.search('.doc') != -1 || screens.search('.xls') != -1) {
            $("#modal-form").attr('id', "modal-form_disabled");
        }
    }
}

function reRunTestCase(id, name, controllerMethod) {
    $.ajax({
        type: "POST",
        url: controllerMethod,
        data: "{'tcId':'" + id + "', 'tcName':'" + name + "'}",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    beforeSend: function() {
        $("#restart-" + id).addClass("fa-spin");
    },
    error: function () {
        //debugger;
        $("#restart-" + id).removeClass("fa-spin");
        ShowMessage(msg.type, msg.text);
    },
    success: function (msg) {
       // debugger;
        $("#restart-" + id).removeClass("fa-spin");
        ShowMessage(msg.type, msg.text);
    }
});
}