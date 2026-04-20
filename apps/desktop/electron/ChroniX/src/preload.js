const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("api", {
  getProcesses: () => ipcRenderer.invoke("get-processes"),
  getStatus: () => ipcRenderer.invoke("get-status"),
  getForeground: () => ipcRenderer.invoke("get-foreground"),
  startTracking: () => ipcRenderer.invoke("start-tracking"),
  stopTracking: () => ipcRenderer.invoke("stop-tracking"),
  getTimes: () => ipcRenderer.invoke("get-times"),
});
