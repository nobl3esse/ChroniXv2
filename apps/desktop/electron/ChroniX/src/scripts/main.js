const trackingBtn = document.getElementById("trackingBtn");
const statusBtn = document.getElementById("statusBtn");
const output = document.getElementById("output");

statusBtn.addEventListener("click", async () => {
  const data = await window.backendApi.getStatus();
  output.textContent = JSON.stringify(data, null, 2);
});

trackingBtn.addEventListener("click", async () => {
  const data = await window.backendApi.startTracking();
  output.textContent = JSON.stringify(data, null, 2);
});
