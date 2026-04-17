document.addEventListener("DOMContentLoaded", async () => {
  const processes = await window.api.getProcesses();
  console.log(processes);

  const servConBtn = document.getElementById("servConBtn");
  const pre = document.getElementById("pre");

  servConBtn.addEventListener("click", async () => {
    const status = await window.api.getStatus();
    pre.textContent = JSON.stringify(status);
  });
});
