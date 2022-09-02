/**
 Copyright 2022 [information redacted]

 Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

window.__notificationApiInternal_dep_rework = null;
window.__notificationApiInternal_dep_classPrefix = null;

window.notificationApi_MaxAnimCount = 0;
window.notificationApi_Indicator = null;
window.notificationApi_Banner = null;
window.notificationApi_Themes = {};
window.notificationApi_currentTheme = null;
window.notificationApi_currentColor = null;

window.notificationApi_UserPreferences = {
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

window.__notificationApiInternal_headElem = null;
window.__notificationApiInternal_bodyElem = null;
window.__notificationApiInternal_hudAreaElem = null;
window.__notificationApiInternal_hudMessageElem = null;
window.__notificationApiInternal_notificationDivElem = null;
window.__notificationApiInternal_notificationPositionElement = null;
window.__notificationApiInternal_textElem = null;
window.__notificationApiInternal_userDataPath = null;
window.__notificationApiInternal_hudAreaClass = "hud-area-right-bottom"; // TODO: Create setter from within C#, possibly turn into a user-pref, then theme-property if user-pref is "theme-selected"

// notificationApi_Init: Initializes the notification element.
function notificationApi_Init() {

    window.__notificationApiInternal_dep_rework = require("rework");
    window.__notificationApiInternal_dep_classPrefix = require("rework-class-prefix");
    
    window.__notificationApiInternal_headElem = document.getElementsByTagName("head")[0];
    window.__notificationApiInternal_bodyElem = document.getElementsByTagName("body")[0];
    window.__notificationApiInternal_hudAreaElem = document.getElementsByClassName(__notificationApiInternal_hudAreaClass)[0];
    window.__notificationApiInternal_hudMessageElem = document.getElementsByClassName("hud-message")[0];

    window.__notificationApiInternal_notificationDivElem = document.createElement("div");
    window.__notificationApiInternal_notificationDivElem.setAttribute("id", "notificationApi_NotificationDiv")

    window.__notificationApiInternal_notificationPositionElement = document.getElementById("notificationApi_NotificationPosition");
    if (window.__notificationApiInternal_notificationPositionElement === null) {
        window.__notificationApiInternal_notificationPositionElement = document.createElement("div");
        window.__notificationApiInternal_notificationPositionElement.id = "notificationApi_NotificationPosition";
    }

    window.__notificationApiInternal_notificationPositionElement.appendChild(__notificationApiInternal_notificationDivElem);
    window.__notificationApiInternal_bodyElem.appendChild(__notificationApiInternal_notificationPositionElement);
    if (window.notificationApi_UserPreferences["customPosition"]["enabled"]) {
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
    
    window.notificationApi_currentTheme = theme;
    window.notificationApi_currentColor = color;
    window.notificationApi_MaxAnimCount = maxAnimCount;

    if (window.notificationApi_UserPreferences["customPosition"]["enabled"]) {
        __notificationApiInternal_moveElement("custom_user");
    }

    window.__notificationApiInternal_notificationDivElem.innerHTML = notificationApi_Themes[theme]["content"];
    
    window.notificationApi_Indicator = document.getElementById("indicator");
    window.notificationApi_Banner = document.getElementById("banner");
    

    let _textField = document.getElementById("notificationApi_NotificationField");
    let textField = _textField.cloneNode(true);
    _textField.parentNode.replaceChild(textField, _textField);

    textField.setAttribute("style", "animation: rightToLeft " + animSecs + "s linear " + maxAnimCount + ";")
    textField.addEventListener("animationend", notificationApi_EndNotification, false);
    
    textField.textContent = "";
    textField.textContent = notificationText;

    window.notificationApi_Themes[window.notificationApi_currentTheme]["jsNotifyCallback"]();
}

function notificationApi_EndNotification() {
    console.log("[NotificationAPI]: EndNotification was called.")
    let textField = document.getElementById("notificationApi_NotificationField");
    textField.textContent = "";
    
    window.notificationApi_Themes[window.notificationApi_currentTheme]["jsEndNotificationCallback"]();
}

function notificationApi_LoadTheme(name, themeContent, styleSheet, notifyCallback = "", endNotificationCallback = "") {
    window.notificationApi_Themes[name] = {};
    
    window.notificationApi_Themes[name]["path"] = __notificationApiInternal_userDataPath + `themes/${name}/`;
    window.notificationApi_Themes[name]["content"] = themeContent.replaceAll("{{themePath}}", notificationApi_Themes[name]["path"]).replaceAll("{{themeName}}", name);
    window.notificationApi_Themes[name]["jsNotifyCallback"] = new Function (notifyCallback.replaceAll("{{themeName}}", name));
    window.notificationApi_Themes[name]["jsEndNotificationCallback"] = new Function(endNotificationCallback.replaceAll("{{themeName}}", name));

    window.__notificationApiInternal_headElem.appendChild(document.createComment(`The following element is managed by NotificationAPI for the "${name}" theme.`));
    styleSheet = window.__notificationApiInternal_dep_rework(styleSheet).use(__notificationApiInternal_dep_classPrefix(`${name}-`)).toString();
    let styleElem = document.createElement("style")
    styleElem.textContent = styleSheet;

    window.__notificationApiInternal_headElem.appendChild(styleElem);
}

function __notificationApiInternal_setUserDataPath(userDataPath) {
    window.__notificationApiInternal_userDataPath = userDataPath;
}

function __notificationApiInternal_moveElement(whereInHud, customTop, customBottom, customLeft, customRight, customHeight, customWidth) {
    let divElem = window.__notificationApiInternal_notificationDivElem // .cloneNode();
    
    switch (whereInHud) {
        case "top_right": {
            window.__notificationApiInternal_notificationPositionElement.style.display = "block";
            window.__notificationApiInternal_notificationPositionElement.style.position = "absolute";

            window.__notificationApiInternal_notificationPositionElement.style.top = "0%";
            window.__notificationApiInternal_notificationPositionElement.style.bottom = "0%";
            window.__notificationApiInternal_notificationPositionElement.style.left = "88%";
            window.__notificationApiInternal_notificationPositionElement.style.right = "0%";
            break;
        }
        
        case "top_left": {
            window.__notificationApiInternal_notificationPositionElement.style.top = "0%";
            window.__notificationApiInternal_notificationPositionElement.style.bottom = "0%";
            window.__notificationApiInternal_notificationPositionElement.style.left = "0%";
            window.__notificationApiInternal_notificationPositionElement.style.right = "20%";
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
            window.__notificationApiInternal_notificationPositionElement.style.display = "block";
            window.__notificationApiInternal_notificationPositionElement.style.position = "absolute";

            window.__notificationApiInternal_notificationPositionElement.style.top = customTop;
            window.__notificationApiInternal_notificationPositionElement.style.bottom = customBottom;
            window.__notificationApiInternal_notificationPositionElement.style.left = customLeft;
            window.__notificationApiInternal_notificationPositionElement.style.right = customRight;

            window.__notificationApiInternal_notificationPositionElement.style.height = customHeight;
            window.__notificationApiInternal_notificationPositionElement.style.width = customWidth;
            break;
        }
        case "custom_user": {
            if (!window.notificationApi_UserPreferences["customPosition"]["enabled"]){
                return;
            }

            window.__notificationApiInternal_notificationPositionElement.style.display = "block";
            window.__notificationApiInternal_notificationPositionElement.style.position = "absolute";

            window.__notificationApiInternal_notificationPositionElement.style.top = `${notificationApi_UserPreferences["customPosition"]["top"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            window.__notificationApiInternal_notificationPositionElement.style.bottom = `${notificationApi_UserPreferences["customPosition"]["bottom"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            window.__notificationApiInternal_notificationPositionElement.style.left = `${notificationApi_UserPreferences["customPosition"]["left"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;
            window.__notificationApiInternal_notificationPositionElement.style.right = `${notificationApi_UserPreferences["customPosition"]["right"]}${notificationApi_UserPreferences["customPosition"]["type"] === "percent" ? "%" : "px"}`;

            window.__notificationApiInternal_notificationPositionElement.style.height = `${notificationApi_UserPreferences["customPosition"]["height"]}px`;
            window.__notificationApiInternal_notificationPositionElement.style.width = `${notificationApi_UserPreferences["customPosition"]["width"]}px`;
            break;
        }
    }
    
    if (whereInHud === "native_notification_ui") {
        __notificationApiInternal_hideNativeNotificationUI(true);
        window.__notificationApiInternal_hudMessageElem.style.opacity = 1;
        divElem.style.position = "relative";
        divElem.style.top = "50%";
    } else {
        __notificationApiInternal_hideNativeNotificationUI(false);
        window.__notificationApiInternal_hudMessageElem.removeAttribute("style");
        divElem.removeAttribute("style");
    }

    window.__notificationApiInternal_notificationDivElem = divElem;
}

function __notificationApiInternal_hideNativeNotificationUI(hideText) {
    let authoritativeIndication = window.__notificationApiInternal_hudMessageElem.getElementsByClassName("authorative")[0];
    let headline = window.__notificationApiInternal_hudMessageElem.getElementsByClassName("headline")[0];
    let message = window.__notificationApiInternal_hudMessageElem.getElementsByClassName("message")[0];
    
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
    window.notificationApi_UserPreferences["customPosition"] = JSON.parse(newCustomPositionPref);
    
    if (window.notificationApi_UserPreferences["customPosition"]["enabled"]) {
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