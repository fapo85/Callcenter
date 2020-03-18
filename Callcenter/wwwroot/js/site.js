// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/Hub").build();
connection.start().then(function () {
    console.log("SignalR Connected");
}).catch(function (err) {
    return console.error(err.toString());
});
connection.on("marked", function (id) {
    console.log("SignalR - Marked: " + id);
    var element = document.getElementById(id);
    if (element && element != undefined && element != null && !element.classList.contains("marked")) {
        element.classList.add("other");
    }
});
connection.on("free", function (id) {
    console.log("SignalR - Free: " + id);
    var element = document.getElementById(id);
    if (element && element != undefined && element != null) {
        element.classList.remove("other");
    }
});
connection.on("delete", function (id) {
    console.log("SignalR - Delete: " + id);
    var element = document.getElementById(id);
    if (element && element != undefined && element != null) {
        element.parentElement.removeChild(element);
    }
});
connection.on("filldata", function (data) {
    console.log("SignalR - filldata: " + data.id);
    const id = document.getElementById('id')
    if (id && id != undefined && id != null) {
        id.value = data.id;
    }
    const phone = document.getElementById('phone')
    if (phone && phone != undefined && phone != null) {
        phone.value = data.phone;
    }
    const zip = document.getElementById('zip')
    if (zip && zip != undefined && zip != null) {
        zip.value = data.zip;
    }
    const radios = document.getElementsByName("request");
    if (radios && radios != undefined && radios != null) {
        for (var i = 0; i < radios.length; i++) {
            radios[i].checked = (radios[i].value == data.request);
        }
    }
});
function MarkItem(elmid) {
    const element = document.getElementById(elmid);
    if (element.classList.contains("other")) {
        const isConfirmed = confirm("Dieser Eintrag wird bereits bearbeitet. Bitte bestätigen Sie, dass sie diesen Eintrag übernehmen möchten.");
        if (!isConfirmed) {
            return;
        }
        if (element.classList.contains("other")) {
            element.classList.remove("other");
        }
    }
    if (!element.classList.contains("marked")) {
        Array.from(document.getElementsByClassName("marked")).forEach(elm => {
7
            connection.invoke("FreeEntry", elm.id);
            elm.childNodes.forEach(item => {
                item.childNodes.forEach(itm => {
                    if (itm.classList != undefined && itm.classList.contains("fa-times")) {
                        itm.classList.add("fa-user-edit");
                        itm.classList.remove("fa-times");
                    }
                });
            });
            elm.classList.remove("marked");
        });
        element.classList.add("marked");
        element.childNodes.forEach(item => {
            item.childNodes.forEach(itm => {
                if (itm.classList != undefined && itm.classList.contains("fa-user-edit")) {
                    itm.classList.add("fa-times");
                    itm.classList.remove("fa-user-edit");
                }
            });
        });
        connection.invoke("MarkEntry", elmid);
    } else {
        element.classList.remove("marked");
        element.childNodes.forEach(item => {
            item.childNodes.forEach(itm => {
                if (itm.classList != undefined && itm.classList.contains("fa-times")) {
                    itm.classList.add("fa-user-edit");
                    itm.classList.remove("fa-times");
                }
            });
        });
        connection.invoke("FreeEntry", elmid);
        document.getElementById('id').value = "";
        document.getElementById('phone').value = "";
        document.getElementById('zip').value = "";
        const radios = document.getElementsByName("request");
        for (var i = 0; i < radios.length; i++) {
            if (radios[i].checked) {
                radios[i].checked = false;
            }
        }
    }
}
function DelItem(elmid) {
    var element = document.getElementById(elmid);
    if (element.classList.contains("marked")) {
        window.location.href = "/Entry/Delete/" + elmid;
    } else {
        alert("Element " + elmid + " nicht als bearbeitet Markiert");
    }
}