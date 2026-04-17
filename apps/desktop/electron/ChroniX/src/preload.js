const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("api", {
  getProcesses: () => ipcRenderer.invoke("get-processes"),
  getStatus: () => ipcRenderer.invoke("get-status"),
});
