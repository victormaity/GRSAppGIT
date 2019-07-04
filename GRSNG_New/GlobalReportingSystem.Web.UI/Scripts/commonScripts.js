function IsJsonString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}

function ShowMessage(type, message) {
    if (message != null && message != "") {
        $.gritter.add({
            // (string | mandatory) the heading of the notification
            title: type,
            // (string | mandatory) the text inside the notification
            text: message,
            // (int | optional) the time you want it to be alive for before fading out (milliseconds)
            time: 120000
        });
    }
}

function openLink(link) {
    //window.open(link, '_blank');
    location.target = '_blank';
    location.href = link;
}