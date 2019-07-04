function enlargeObject(id) {
    if ($("#obj-" + id).attr("enlarged") == "true") {
        $("#obj-" + id).attr("enlarged", "false");
        $("#obj-" + id).attr("style", "max-width: 40px; max-height: 40px");
    } else {
        $("#obj-" + id).attr("enlarged", "true");
        $("#obj-" + id).attr("style", "max-width: 500px; max-height: 500px");
    }
}
