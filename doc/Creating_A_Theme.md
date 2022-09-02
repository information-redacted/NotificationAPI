### Creating a NotificationAPI theme

NOTE: The following documentation is rough, in disarray, and is meant as a general note to anyone interested in making themes for this project. It may be updated to be proper in the future.

---

#### Important note on CSS:
Due to conflict between themes, CSS classes are **always** prefixed with the theme's name and a dash (e.g.: `payday-`).

In order to reference your namespaced class, you must use `{{themeName}}`, which is automatically replaced with the name of the theme by the notification engine (theme names are equal to the directory name they are saved in, and therefore cannot be hardcoded).

#### Special files

- `content.html`: This is the content that will be put into the notification div.
- `notification_start.js`: This is the callback function that will be executed when a notification is called with the selected theme.
- `notification_end.js`*1: This is the callback function that will be executed when a notification with this theme ends.
- `notification.css`: This is the stylesheet used for the notification.

*1: By default, a **required** custom engine callback (`engine.call("notificationDoneCallback");`) is appended. This callback to the engine is required in order to let the API know that the notification playback is over, and that it can send another one.

In some cases (e.g., when `setTimeout` is used to delay an action), this is not desired, and you may want to do the implementation of this callback yourself, to do so, please add the following comment somewhere in your code: `// NotificationAPIAnnotation: DO_NOT_APPEND_ENGINE_CALLBACK`.

#### Automatically-replaced variables
The following variables are **always automatically replaced in any "special file".

 - `{{themeName}}`: Replaced by the name of the current theme.
 - `{{themePath}}`: Replaced by the *relative* path of the current theme. (from the `CVR_HUD/default` directory).