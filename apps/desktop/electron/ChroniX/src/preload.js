const { contextBridge } = require("electron");

contextBridge.exposeInMainWorld("backendApi", {
  getStatus: async () => {
    const res = await fetch("http://localhost:5000/api/status");
    return res.json();
  },

  startTracking: async () => {
    const res = await fetch("http://localhost:5000/api/start-tracking", {
      method: "POST",
    });
    return res.json();
  },

  getStats: async () => {
    const res = await fetch("http://localhost:5000/api/stats");
    return res.json();
  },
});
