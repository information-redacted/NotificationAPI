let notificationApi_AnimCount = 0;
let notificationApi_MaxAnimCount = 0;
let notificationApi_Indicator = null;
let notificationApi_Banner = null;

let __notificationApiInternal_headElem = null;
let __notificationApiInternal_bodyElem = null;
let __notificationApiInternal_hudAreaElem = null;
let __notificationApiInternal_hudMessageElem = null;
let __notificationApiInternal_notificationDivElem = null;
let __notificationApiInternal_textElem = null;

// notificationApi_Init: Initializes the notification element.
function notificationApi_Init() {
    __notificationApiInternal_headElem = document.getElementsByTagName("head")[0];
    __notificationApiInternal_bodyElem = document.getElementsByTagName("body")[0];
    __notificationApiInternal_hudAreaElem = document.getElementsByClassName("hud-area-right-bottom")[0];
    __notificationApiInternal_hudMessageElem = document.getElementsByClassName("hud-message")[0];

    let linkElem = document.createElement("link");
    linkElem.setAttribute("rel", "stylesheet");
    linkElem.setAttribute("href", "{{userDataPath}}notification.css");

    __notificationApiInternal_headElem.appendChild(linkElem);

    __notificationApiInternal_notificationDivElem = document.createElement("div");
    __notificationApiInternal_notificationDivElem.setAttribute("id", "notificationApi_NotificationDiv")
    __notificationApiInternal_notificationDivElem.innerHTML = `{{htmlTemplate}}`;

    __notificationApiInternal_hudAreaElem.appendChild(__notificationApiInternal_notificationDivElem);
    console.log("[NotificationAPI]: Notification element initialized.")
}

function notificationApi_Notify(notificationText, maxAnimCount = 3, animSecs = 8) {
    console.log("[NotificationAPI]: Notify was called;\n  Notification contents: " + notificationText + ".\n  Iterations: " + maxAnimCount + ".\n  Duration: " + animSecs + " seconds.");
    notificationApi_AnimCount = 0;
    notificationApi_MaxAnimCount = maxAnimCount;
    if (notificationApi_Indicator == null) {
        notificationApi_Indicator = document.getElementById("indicator");
        notificationApi_Banner = document.getElementById("banner");
    }

    let _textField = document.getElementById("notificationApi_NotificationField");
    let textField = _textField.cloneNode(true);
    _textField.parentNode.replaceChild(textField, _textField);

    textField.setAttribute("style", "animation: rightToLeft " + animSecs + "s linear " + maxAnimCount + ";")
    textField.addEventListener("animationend", notificationApi_EndNotification, false);

    textField.textContent = "";
    textField.textContent = notificationText;

    notificationApi_Indicator.style.visibility = "visible";
    notificationApi_Indicator.classList.add("flicker");
    setTimeout(() => {
        notificationApi_Indicator.classList.remove("flicker");
        notificationApi_Banner.classList.add("max");
        notificationApi_Banner.classList.add("flickerbg");
    }, 2000);
}

function notificationApi_EndNotification() {
    console.log("[NotificationAPI]: EndNotification was called.")
    let textField = document.getElementById("notificationApi_NotificationField");
    textField.textContent = "";

    notificationApi_Indicator.classList.add("flicker");

    notificationApi_Banner.classList.remove("max");
    notificationApi_Banner.classList.remove("flickerbg");

    setTimeout(() => {
        notificationApi_Indicator.classList.remove("flicker");
        notificationApi_Indicator.style.visibility = "hidden";
        engine.call("notificationDoneCallback");
    }, 2000);
}

function __notificationApiInternal_initEngineDefs() {
    engine.on("notificationApi_Init", notificationApi_Init);
    engine.on("notificationApi_Notify", notificationApi_Notify);
    engine.on("notificationApi_EndNotification", notificationApi_EndNotification);
    notificationApi_Init();
}

document.addEventListener("load", __notificationApiInternal_initEngineDefs);