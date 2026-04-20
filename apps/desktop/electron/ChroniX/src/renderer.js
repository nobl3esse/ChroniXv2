document.addEventListener("DOMContentLoaded", async () => {
  const processes = await window.api.getProcesses();
  console.log(processes);

  //Buttons holen
  const servConBtn = document.getElementById("servConBtn");
  const toggleTrackingBtn = document.getElementById("toggleTrackingBtn");
  const testBtn = document.getElementById("testBtn");

  const pre = document.getElementById("pre");

  servConBtn.addEventListener("click", async () => {
    const status = await window.api.getStatus();
    pre.textContent = JSON.stringify(status);
  });

  toggleTrackingBtn.addEventListener("click", async () => {
    const start = await window.api.startTracking();
    pre.textContent = JSON.stringify(start);
  });

  testBtn.addEventListener("click", async () => {
    const times = await window.api.getTimes();

    let result = "";
    Object.entries(times).forEach(([name, seconds]) => {
      const minutes = Math.floor(seconds / 60) % 60;
      const hours = Math.floor(seconds / 3600);
      const remainingSeconds = seconds % 60;

      result +=
        name +
        ", " +
        hours +
        "h " +
        minutes +
        "min " +
        remainingSeconds +
        "s " +
        "\n";
    });
    pre.textContent = result;
  });
});
