const { app, BrowserWindow, Menu, Tray, nativeImage } = require("electron");
const path = require("node:path");

let tray;

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (require("electron-squirrel-startup")) {
  app.quit();
}

const createWindow = () => {
  // For image paths use path.join
  const iconPath = path.join(__dirname, "../assets/icons/hourglass.ico");
  const appIcon = nativeImage.createFromPath(iconPath);
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    icon: appIcon,
    width: 800,
    height: 600,
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
    },
    titleBarStyle: "hidden",
    titleBarOverlay: {
      color: "#2c2d32",
      symbolColor: "#fff",
      height: 40,
    },
  });

  // and load the index.html of the app.
  mainWindow.loadFile(path.join(__dirname, "index.html"));

  // Open the DevTools.
  mainWindow.webContents.openDevTools();
};

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(() => {
  createWindow();

  const tryIconPath = path.join(__dirname, "../assets/icons/hourglass.ico");
  tray = new Tray(tryIconPath);
  const contextMenu = Menu.buildFromTemplate([
    { label: "Item1", type: "radio" },
    { label: "Item2", type: "radio" },
    { label: "Item3", type: "radio", checked: true },
    { label: "Item4", type: "radio" },
  ]);

  tray.setToolTip("This is my application.");
  tray.setContextMenu(contextMenu);

  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and import them here.
