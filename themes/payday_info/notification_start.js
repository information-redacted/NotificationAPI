window.notificationApi_Indicator.style.visibility = "visible";
window.notificationApi_Indicator.classList.add("{{themeName}}-flicker");

let documentRoot = document.querySelector(":root");
documentRoot.style.setProperty("--custom-notification-color", window.notificationApi_currentColor)

setTimeout(() => {
    window.notificationApi_Indicator.classList.remove("flicker");
    window.notificationApi_Banner.classList.add("{{themeName}}-max");
    window.notificationApi_Banner.classList.add("{{themeName}}-flickerbg");
}, 2000);