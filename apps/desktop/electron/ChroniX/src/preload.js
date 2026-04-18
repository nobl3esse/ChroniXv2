const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("api", {
  getProcesses: () => ipcRenderer.invoke("get-processes"),
  getStatus: () => ipcRenderer.invoke("get-status"),
  getForeground: () => ipcRenderer.invoke("get-foreground"),
  start: () => ipcRenderer.invoke("start-tracking"),
  getTimes: () => ipcRenderer.invoke("get-times"),
});
