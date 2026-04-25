const {
  app,
  BrowserWindow,
  Menu,
  Tray,
  nativeImage,
  ipcMain,
} = require("electron");
const path = require("node:path");
const fs = require("fs");

const myProcessName = path.parse(process.execPath).name;

ipcMain.handle("get-processes", async () => {
  try {
    const response = await fetch("http://localhost:5000/processes");
    const data = await response.json();

    const filtered = data.filter(
      (name) => name.toLowerCase() !== myProcessName.toLowerCase(),
    );
    return filtered;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("get-status", async () => {
  try {
    const response = await fetch("http://localhost:5000/status");
    const data = await response.json();
    return data;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("get-is-tracking", async () => {
  try {
    const response = await fetch("http://localhost:5000/isTracking");
    const data = await response.json();
    return data;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("get-foreground", async () => {
  try {
    const response = await fetch("http://localhost:5000/foreground");
    const data = await response.json();
    return data;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("start-tracking", async () => {
  try {
    const response = await fetch("http://localhost:5000/start");
    const message = "Tracking gestartet.";
    return message;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("stop-tracking", async () => {
  try {
    const response = await fetch("http://localhost:5000/stop");
    const message = "Tracking gestoppt.";
    return message;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("get-times", async () => {
  try {
    const response = await fetch("http://localhost:5000/times");
    const data = await response.json();
    return data;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("get-whitelist", async () => {
  try {
    const response = await fetch("http://localhost:5000/whitelist");
    const data = await response.json();
    return data;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("add-whitelist", async (event, name) => {
  try {
    const response = await fetch("http://localhost:5000/whitelist", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ processName: name }),
    });
    const data = await response.json();
    // console.log("ADD response:", data);
    return data;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

ipcMain.handle("remove-whitelist", async (event, name) => {
  try {
    const response = await fetch(
      `http://localhost:5000/whitelist?name=${name}`,
      {
        method: "DELETE",
      },
    );
    const data = await response.json();
    // console.log("DELETE response:", data);
    return data;
  } catch (error) {
    return { error: "C# API nicht erreichbar" };
  }
});

let tray;
let mainWindow;
let watchers = [];

if (require("electron-squirrel-startup")) {
  app.quit();
}

function createWindow() {
  const iconPath = path.join(__dirname, "../assets/icons/hourglass.ico");
  const appIcon = nativeImage.createFromPath(iconPath);

  mainWindow = new BrowserWindow({
    icon: appIcon,
    width: 800,
    height: 600,
    resizable: false,
    title: "ChroniX",
    titleBarStyle: "hidden",
    titleBarOverlay: {
      color: "#2c2d32",
      symbolColor: "#fff",
      height: 30,
    },
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  mainWindow.loadFile(path.join(__dirname, "index.html"));
  mainWindow.webContents.openDevTools();

  mainWindow.on("closed", () => {
    mainWindow = null;
  });
}

function reloadWindow() {
  if (mainWindow && !mainWindow.isDestroyed()) {
    mainWindow.webContents.reload();
  }
}

function setupWatchers() {
  const filesToWatch = [
    path.join(__dirname, "index.html"),
    path.join(__dirname, "index.css"),
    path.join(__dirname, "renderer.js"),
    path.join(__dirname, "preload.js"),
  ];

  for (const watcher of watchers) {
    watcher.close();
  }

  watchers = [];

  for (const file of filesToWatch) {
    try {
      const watcher = fs.watch(file, (eventType) => {
        if (eventType === "change") {
          reloadWindow();
        }
      });

      watchers.push(watcher);
    } catch (error) {
      console.error(`Watcher konnte nicht gestartet werden: ${file}`, error);
    }
  }
}

app.whenReady().then(() => {
  createWindow();
  setupWatchers();

  const trayIconPath = path.join(__dirname, "../assets/icons/hourglass.ico");
  tray = new Tray(trayIconPath);

  const contextMenu = Menu.buildFromTemplate([
    {
      label: "Fenster zeigen",
      click: () => {
        if (!mainWindow || mainWindow.isDestroyed()) {
          createWindow();
        } else {
          mainWindow.show();
          mainWindow.focus();
        }
      },
    },
    {
      label: "Neu laden",
      click: () => {
        reloadWindow();
      },
    },
    { type: "separator" },
    {
      label: "Beenden",
      click: () => {
        app.quit();
      },
    },
  ]);

  tray.setToolTip("ChroniX");
  tray.setContextMenu(contextMenu);

  tray.on("double-click", () => {
    if (!mainWindow || mainWindow.isDestroyed()) {
      createWindow();
    } else {
      mainWindow.show();
      mainWindow.focus();
    }
  });

  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on("before-quit", () => {
  for (const watcher of watchers) {
    watcher.close();
  }
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});
