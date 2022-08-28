let notificationApi_AnimCount = 0;
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

    __notificationApiInternal_hudAreaElem.appendChild(__notificationApiInternal_notificationDivElem);
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
    notificationApi_AnimCount = 0;
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
    let divElem = __notificationApiInternal_notificationDivElem.cloneNode();
    let currentParent = __notificationApiInternal_notificationDivElem.parentNode;
    
    switch (whereInHud) {
        // TODO: Force CSS-based hack to toss it to the top/bottom
        
        case "top_right": {
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);
            
            let newParent = document.getElementsByClassName("hud-area-right-bottom")[0]; // TODO: Hack, missing right-top wrapper.
            newParent.appendChild(divElem);
            break;
        }
        case "top_left": {
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);
            
            let newParent = document.getElementsByClassName("hud-wrapper-left-top")[0];
            newParent.appendChild(divElem);
            break;
        }
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
        }
        case "native_notification_ui": {
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);

            let newParent = document.getElementsByClassName("hud-message")[0];
            newParent.appendChild(divElem);
            break;
        }
        case "custom": {
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);

            let customPositionParent = document.getElementById("notificationApi_NotificationPosition");
            if (customPositionParent === null) {
                customPositionParent = document.createElement("div");
                customPositionParent.id = "notificationApi_NotificationPosition";
            }
            
            customPositionParent.style.display = "block";
            customPositionParent.style.position = "absolute";

            customPositionParent.style.top = customTop;
            customPositionParent.style.bottom = customBottom;
            customPositionParent.style.left = customLeft;
            customPositionParent.style.right = customRight;

            customPositionParent.style.height = customHeight;
            customPositionParent.style.width = customWidth;

            let newParent = document.getElementsByTagName("body")[0];
            newParent.appendChild(divElem);
            customPositionParent.appendChild(customPositionParent);
            break;
        }
        case "custom_user": {
            if (!notificationApi_UserPreferences["customPosition"]["enabled"]){
                return;
            }
            currentParent.removeChild(__notificationApiInternal_notificationDivElem);
            
            let customPositionParent = document.getElementById("notificationApi_NotificationPosition");
            if (customPositionParent === null) {
                customPositionParent = document.createElement("div");
                customPositionParent.id = "notificationApi_NotificationPosition";
            }

            customPositionParent.style.display = "block";
            customPositionParent.style.position = "absolute";
            
            customPositionParent.style.top = `${notificationApi_UserPreferences["customPosition"]["top"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            customPositionParent.style.bottom = `${notificationApi_UserPreferences["customPosition"]["bottom"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            customPositionParent.style.left = `${notificationApi_UserPreferences["customPosition"]["left"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            customPositionParent.style.right = `${notificationApi_UserPreferences["customPosition"]["right"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            
            customPositionParent.style.height = `${notificationApi_UserPreferences["customPosition"]["height"]}px`;
            customPositionParent.style.width = `${notificationApi_UserPreferences["customPosition"]["width"]}px`;
            
            let newParent = document.getElementsByTagName("body")[0];
            customPositionParent.appendChild(divElem);
            newParent.appendChild(customPositionParent);
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