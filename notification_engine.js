let notificationApi_MaxAnimCount = 0;
let notificationApi_Indicator = null;
let notificationApi_Banner = null;
let notificationApi_Themes = {};
let notificationApi_currentTheme = null;
let notificationApi_currentColor = null;

let notificationApi_UserPreferences = {
    "customPosition": {
        "enabled": false,
        "type": "percent",
        "top": 50,
        "bottom": 0,
        "left": 0,
        "right": 0,
        "height": 300,
        "width": 300
    }
};

let __notificationApiInternal_headElem = null;
let __notificationApiInternal_bodyElem = null;
let __notificationApiInternal_hudAreaElem = null;
let __notificationApiInternal_hudMessageElem = null;
let __notificationApiInternal_notificationDivElem = null;
let __notificationApiInternal_notificationPositionElement = null;
let __notificationApiInternal_textElem = null;
let __notificationApiInternal_userDataPath = null;
let __notificationApiInternal_hudAreaClass = "hud-area-right-bottom"; // TODO: Create setter from within C#, possibly turn into a user-pref, then theme-property if user-pref is "theme-selected"

// notificationApi_Init: Initializes the notification element.
function notificationApi_Init() {
    __notificationApiInternal_headElem = document.getElementsByTagName("head")[0];
    __notificationApiInternal_bodyElem = document.getElementsByTagName("body")[0];
    __notificationApiInternal_hudAreaElem = document.getElementsByClassName(__notificationApiInternal_hudAreaClass)[0];
    __notificationApiInternal_hudMessageElem = document.getElementsByClassName("hud-message")[0];

    __notificationApiInternal_notificationDivElem = document.createElement("div");
    __notificationApiInternal_notificationDivElem.setAttribute("id", "notificationApi_NotificationDiv")

    __notificationApiInternal_notificationPositionElement = document.getElementById("notificationApi_NotificationPosition");
    if (__notificationApiInternal_notificationPositionElement === null) {
        __notificationApiInternal_notificationPositionElement = document.createElement("div");
        __notificationApiInternal_notificationPositionElement.id = "notificationApi_NotificationPosition";
    }

    __notificationApiInternal_notificationPositionElement.appendChild(__notificationApiInternal_notificationDivElem);
    __notificationApiInternal_bodyElem.appendChild(__notificationApiInternal_notificationPositionElement);
    if (notificationApi_UserPreferences["customPosition"]["enabled"]) {
        __notificationApiInternal_moveElement("custom_user");
    }
    
    console.log("[NotificationAPI]: Notification element initialized.")
}

function notificationApi_Notify(notificationText, 
                                maxAnimCount = 3, 
                                animSecs = 8, 
                                color = "#ffef00", 
                                theme = "payday") {
    console.log(`[NotificationAPI]: Notify was called;
      Content:\t\t"${notificationText}".
      Iterations:\t${maxAnimCount}.
      Duration:\t\t${animSecs}.
      Color:\t\t${color}.
      Theme:\t\t${theme}.`);
    
    notificationApi_currentTheme = theme;
    notificationApi_currentColor = color;
    notificationApi_MaxAnimCount = maxAnimCount;

    if (notificationApi_UserPreferences["customPosition"]["enabled"]) {
        __notificationApiInternal_moveElement("custom_user");
    }

    __notificationApiInternal_notificationDivElem.innerHTML = notificationApi_Themes[theme]["content"];
    
    notificationApi_Indicator = document.getElementById("indicator");
    notificationApi_Banner = document.getElementById("banner");
    

    let _textField = document.getElementById("notificationApi_NotificationField");
    let textField = _textField.cloneNode(true);
    _textField.parentNode.replaceChild(textField, _textField);

    textField.setAttribute("style", "animation: rightToLeft " + animSecs + "s linear " + maxAnimCount + ";")
    textField.addEventListener("animationend", notificationApi_EndNotification, false);
    
    textField.textContent = "";
    textField.textContent = notificationText;

    notificationApi_Themes[notificationApi_currentTheme]["jsNotifyCallback"]();
}

function notificationApi_EndNotification() {
    console.log("[NotificationAPI]: EndNotification was called.")
    let textField = document.getElementById("notificationApi_NotificationField");
    textField.textContent = "";
    
    notificationApi_Themes[notificationApi_currentTheme]["jsEndNotificationCallback"]();
}

function notificationApi_LoadTheme(name, themeContent, styleSheetFile, notifyCallback = "", endNotificationCallback = "") {
    notificationApi_Themes[name] = {};
    
    notificationApi_Themes[name]["path"] = __notificationApiInternal_userDataPath + `themes/${name}/`;
    notificationApi_Themes[name]["content"] = themeContent.replaceAll("{{themePath}}", notificationApi_Themes[name]["path"]);
    notificationApi_Themes[name]["jsNotifyCallback"] = new Function (notifyCallback);
    notificationApi_Themes[name]["jsEndNotificationCallback"] = new Function(endNotificationCallback);

    __notificationApiInternal_headElem.appendChild(document.createComment(`The following element is managed by NotificationAPI for the "${name}" theme.`));
    let linkElem = document.createElement("link");
    linkElem.setAttribute("rel", "stylesheet");
    linkElem.setAttribute("href", `${notificationApi_Themes[name]["path"]}${styleSheetFile}`);

    __notificationApiInternal_headElem.appendChild(linkElem);
}

function __notificationApiInternal_setUserDataPath(userDataPath) {
    __notificationApiInternal_userDataPath = userDataPath;
}

function __notificationApiInternal_moveElement(whereInHud, customTop, customBottom, customLeft, customRight, customHeight, customWidth) {
    let divElem = __notificationApiInternal_notificationDivElem // .cloneNode();
    
    switch (whereInHud) {
        case "top_right": {
            __notificationApiInternal_notificationPositionElement.style.display = "block";
            __notificationApiInternal_notificationPositionElement.style.position = "absolute";

            __notificationApiInternal_notificationPositionElement.style.top = "0%";
            __notificationApiInternal_notificationPositionElement.style.bottom = "0%";
            __notificationApiInternal_notificationPositionElement.style.left = "88%";
            __notificationApiInternal_notificationPositionElement.style.right = "0%";
            break;
        }
        
        case "top_left": {
            __notificationApiInternal_notificationPositionElement.style.top = "0%";
            __notificationApiInternal_notificationPositionElement.style.bottom = "0%";
            __notificationApiInternal_notificationPositionElement.style.left = "0%";
            __notificationApiInternal_notificationPositionElement.style.right = "20%";
            break;
        }/**
        case "bottom_right": {
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);

            let newParent = document.getElementsByClassName("hud-wrapper-right-bottom")[0];
            newParent.appendChild(divElem);
            break;
        }
        case "bottom_left": {
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);

            let newParent = document.getElementsByClassName("hud-wrapper-left-bottom")[0];
            newParent.appendChild(divElem);
            break;
        }*/
        // TODO: Dynamic offsetting from that element i guess?
        case "native_notification_ui": {
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);

            let newParent = document.getElementsByClassName("hud-message")[0];
            newParent.appendChild(divElem);
            break;
        }
        case "custom": {
            __notificationApiInternal_notificationPositionElement.style.display = "block";
            __notificationApiInternal_notificationPositionElement.style.position = "absolute";

            __notificationApiInternal_notificationPositionElement.style.top = customTop;
            __notificationApiInternal_notificationPositionElement.style.bottom = customBottom;
            __notificationApiInternal_notificationPositionElement.style.left = customLeft;
            __notificationApiInternal_notificationPositionElement.style.right = customRight;

            __notificationApiInternal_notificationPositionElement.style.height = customHeight;
            __notificationApiInternal_notificationPositionElement.style.width = customWidth;
            break;
        }
        case "custom_user": {
            if (!notificationApi_UserPreferences["customPosition"]["enabled"]){
                return;
            }

            __notificationApiInternal_notificationPositionElement.style.display = "block";
            __notificationApiInternal_notificationPositionElement.style.position = "absolute";

            __notificationApiInternal_notificationPositionElement.style.top = `${notificationApi_UserPreferences["customPosition"]["top"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            __notificationApiInternal_notificationPositionElement.style.bottom = `${notificationApi_UserPreferences["customPosition"]["bottom"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            __notificationApiInternal_notificationPositionElement.style.left = `${notificationApi_UserPreferences["customPosition"]["left"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            __notificationApiInternal_notificationPositionElement.style.right = `${notificationApi_UserPreferences["customPosition"]["right"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;

            __notificationApiInternal_notificationPositionElement.style.height = `${notificationApi_UserPreferences["customPosition"]["height"]}px`;
            __notificationApiInternal_notificationPositionElement.style.width = `${notificationApi_UserPreferences["customPosition"]["width"]}px`;
            break;
        }
    }
    
    if (whereInHud === "native_notification_ui") {
        __notificationApiInternal_hideNativeNotificationUI(true);
        __notificationApiInternal_hudMessageElem.style.opacity = 1;
        divElem.style.position = "relative";
        divElem.style.top = "50%";
    } else {
        __notificationApiInternal_hideNativeNotificationUI(false);
        __notificationApiInternal_hudMessageElem.removeAttribute("style");
        divElem.removeAttribute("style");
    }
    
    __notificationApiInternal_notificationDivElem = divElem;
}

function __notificationApiInternal_hideNativeNotificationUI(hideText) {
    let authoritativeIndication = __notificationApiInternal_hudMessageElem.getElementsByClassName("authorative")[0];
    let headline = __notificationApiInternal_hudMessageElem.getElementsByClassName("headline")[0];
    let message = __notificationApiInternal_hudMessageElem.getElementsByClassName("message")[0];
    
    if (hideText) {
        authoritativeIndication.style.display = "none";
        headline.style.display = "none";
        message.style.display = "none";
    } else {
        authoritativeIndication.removeAttribute("style");
        headline.removeAttribute("style");
        message.removeAttribute("style");
    }
}

function __notificationApiInternal_CustomPositionPreferencesUpdated(newCustomPositionPref) {
    console.log(`[NotificationAPI]: Custom position preferences updated:`);
    notificationApi_UserPreferences["customPosition"] = JSON.parse(newCustomPositionPref);
    console.log(notificationApi_UserPreferences);
    if (notificationApi_UserPreferences["customPosition"]["enabled"]) {
        __notificationApiInternal_moveElement("custom_user");
    }
}

function __notificationApiInternal_initEngineDefs() {
    engine.on("notificationApi_Init", notificationApi_Init);
    engine.on("notificationApi_LoadTheme", notificationApi_LoadTheme)
    engine.on("notificationApi_Notify", notificationApi_Notify);
    engine.on("notificationApi_EndNotification", notificationApi_EndNotification);
    engine.on("__notificationApiInternal_setUserDataPath", __notificationApiInternal_setUserDataPath)
    engine.on("__notificationApiInternal_CustomPositionPreferencesUpdated", __notificationApiInternal_CustomPositionPreferencesUpdated)
    notificationApi_Init();

    engine.call("notificationThemeInitCallback");
}

document.addEventListener("load", __notificationApiInternal_initEngineDefs);