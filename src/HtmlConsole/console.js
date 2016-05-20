function updateConsole() {
    var body = $("body");
    var timestamp = parseInt(body.attr("data-console-timestamp"))
    $.get("updates?t=" + timestamp).done(function (updates) {
        for (var i = 0; i < updates.length; i++) {
            var id = updates[i].id;
            var text = updates[i].text;
            var div = $("#" + id);
            if (div.length == 0)
                body.append($("<div>" + text + "</div>").attr("id", id));
            else
                div.html(text);
            //var xmlDoc = $(xml);
            //var updates = $.map(xmlDoc.find("updates").find("u"), function (u) {
            //    var ju = $(u);
            //    return {
            //        time: parseInt(ju.attr("time")),
            //        text: ju.attr("text")
            //    }
            //});
            //for (var i = 0; i < updates.length; i++)
            //    if (updates[i].time > time) {
            //        $("body").append($("<div>" + updates[i].text + "</div>"));
            //        time = updates[i].time;
            //    }
        }
        body.attr("data-console-timestamp", timestamp + updates.length);
        setTimeout(updateConsole, 1000);
    }).fail(function (e) {
        console.log(e);
        setTimeout(updateConsole, 1000);
    });
}

$(function () {
    updateConsole();
})
