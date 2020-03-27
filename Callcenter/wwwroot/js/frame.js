
function ValidateAll() {
    if (!validatePhone()) {
        ShowErrorMsg("Telefonnummer ist nicht korrekt", 2000);
        return true;
    }
    if (!validateZip()) {
        ShowErrorMsg("Postleitzahl ist nicht korrekt", 2000);
        return true;
    }
    if (!validateAuswahl()) {
        ShowErrorMsg("Auswahl ist nicht korrekt", 2000);
        return true;
    }
    return false;
}
function validatePhone() {
    const phone = document.getElementById("phone");
    if (phone && phone != undefined && phone != null) {
        var rgx = new RegExp("^(0049\\d{5,}|0[1-9]\\d{4,}|\\+49\\d{5,})$");
        if (phone.value.match(rgx)) {
            phone.classList.remove("invalid");
            return true;
        } else {
            phone.classList.add("invalid");
        }
    }
    return false;
}
function validateZip() {
    const zip = document.getElementById("zip");
    if (zip && zip != undefined && zip != null) {
        var rgx = new RegExp("^\\d{5}$");
        if (zip.value.match(rgx)) {
            zip.classList.remove("invalid");
            return true;
        } else {
            zip.classList.add("invalid");
        }
    }
    return false;
}
function validateAuswahl() {
    const radios = document.getElementsByName("request");
    var ok = false;
    if (zip && zip != undefined && zip != null) {
        for (var i = 0; i < radios.length; i++) {
            if (radios[i].checked) {
                ok = true;
            }
        }
        for (var i = 0; i < radios.length; i++) {
            if (ok) {
                radios[i].classList.remove("invalid");
            } else {
                radios[i].classList.add("invalid");
            }
        }
    }
    return ok;
}
function ShowErrorMsg(msg, time) {
    console.error("Fehler: " + msg);
    const requestFinish = document.getElementById('requestFinish');
    const requestForm = document.getElementById('requestForm');
    const ErrorWindow = document.getElementById('ErrorWindow');
    if (requestFinish && requestFinish != undefined && requestFinish != null && requestForm && requestForm != undefined && requestForm != null && ErrorWindow && ErrorWindow != undefined && ErrorWindow != null) {
        document.getElementById('ErrorText').innerHTML = msg;
        requestFinish.classList.add("non-visible");
        requestForm.classList.add("non-visible");
        ErrorWindow.classList.remove("non-visible");
        setTimeout(function () {
            ErrorWindow.classList.add("non-visible");
            requestForm.classList.remove("non-visible");
        }, time);
    }
}