notificationApi_Indicator.style.visibility = "visible";
notificationApi_Indicator.classList.add("flicker");

let documentRoot = document.querySelector(":root");
documentRoot.style.setProperty("--custom-notification-color", notificationApi_currentColor)

setTimeout(() => {
    notificationApi_Indicator.classList.remove("flicker");
    notificationApi_Banner.classList.add("max");
    notificationApi_Banner.classList.add("flickerbg");
}, 2000);