// NotificationAPIAnnotation: DO_NOT_APPEND_ENGINE_CALLBACK
notificationApi_Indicator.classList.add("flicker");

notificationApi_Banner.classList.remove("max");
notificationApi_Banner.classList.remove("flickerbg");

setTimeout(() => {
    notificationApi_Indicator.classList.remove("flicker");
    notificationApi_Indicator.style.visibility = "hidden";
    engine.call("notificationDoneCallback");
}, 2000);