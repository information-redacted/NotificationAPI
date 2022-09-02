### Building NotificationAPI

#### Building the JavaScript Notification Engine
In order to build the Notification Engine, you must have the following system packages installed:
 - NodeJS >= 18
 - `npm` (Should come with NodeJS)
 - `browserify` must be globally available (`npm install -g browserify`)

To install the engine's JS dependencies, you can go into the `notification_engine/` directory and run `npm install`.

To build the engine independently, run `browserify notification_engine.js -o <output_filename>.js`. A variation of this is automatically run by the `BeforeBuild` MSBuild task (specifically: `npx -g browserify notification_engine.js -o ..\.build\notification_engine.js`).

#### Building the API
After having built the notification engine and placing the bundled file into the `.build/` directory, you can use MSBuild to build the mod. Please note that the bundle is automatically created by the `BeforeBuild` task as stated above, so you should be ready to go as long as you have the engine's dependencies installed.