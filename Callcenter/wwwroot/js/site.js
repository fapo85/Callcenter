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
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", "/Entry/Free/" + element.id, true);
            xhttp.send();
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
        var xhttp = new XMLHttpRequest();
        xhttp.open("GET", "/Entry/Mark/" + elmid, true);
        xhttp.onload = function () {
            if (xhttp.status >= 200 && xhttp.status < 400) {
                var data = JSON.parse(xhttp.responseText);
                document.getElementById('id').value = data.id;
                document.getElementById('phone').value = data.phone;
                document.getElementById('zip').value = data.zip;
            } else {
                console.log("Error Get");
            }
        };
        xhttp.onerror = function () {
            console.log("Error Get");
        };
        xhttp.send();
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
        var xhttp = new XMLHttpRequest();
        xhttp.open("GET", "/Entry/Free/" + elmid, true);
        xhttp.send();
        document.getElementById('id').value = "";
        document.getElementById('phone').value = "";
        document.getElementById('zip').value = "";
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