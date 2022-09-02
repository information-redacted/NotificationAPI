// NotificationAPIAnnotation: DO_NOT_APPEND_ENGINE_CALLBACK
window.notificationApi_Indicator.classList.add("{{themeName}}-flicker");

window.notificationApi_Banner.classList.remove("{{themeName}}-max");
window.notificationApi_Banner.classList.remove("{{themeName}}-flickerbg");

setTimeout(() => {
    window.notificationApi_Indicator.classList.remove("{{themeName}}-flicker");
    window.notificationApi_Indicator.style.visibility = "hidden";
    engine.call("notificationDoneCallback");
}, 2000);