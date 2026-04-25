document.addEventListener("DOMContentLoaded", async () => {
  const processes = await window.api.getProcesses();
  // console.log(processes);
  const whitelist = await window.api.getWhitelist();
  renderWhitelist(whitelist);

  //Variablen
  let isTracking = false;

  //Buttons holen
  const servConBtn = document.getElementById("servConBtn");
  const toggleTrackingBtn = document.getElementById("toggleTrackingBtn");
  const testBtn = document.getElementById("testBtn");
  const whitelistBtn = document.getElementById("whitelistBtn");
  const addProcessBtn = document.getElementById("addProcessBtn");

  //Dropdowns holen
  const processDropdown = document.getElementById("processDropdown");

  //Anzeigefenster holen
  const pre = document.getElementById("pre");

  servConBtn.addEventListener("click", async () => {
    const status = await window.api.getStatus();
    pre.textContent = JSON.stringify(status);
  });

  toggleTrackingBtn.addEventListener("click", async () => {
    if (isTracking) {
      const stop = await window.api.stopTracking();
      pre.textContent = JSON.stringify(stop);
    } else {
      const start = await window.api.startTracking();
      pre.textContent = JSON.stringify(start);
    }

    //wenn true auf false setzen, wenn false auf true setzen
    isTracking = !isTracking;
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

  whitelistBtn.addEventListener("click", async () => {});

  function renderWhitelist(items) {
    //ul holen
    const whitelistList = document.getElementById("whitelistList");
    whitelistList.innerHTML = "";

    items.forEach((name) => {
      let li = document.createElement("li");
      li.classList = "whitelistLi";
      li.textContent = name;

      let removeBtn = document.createElement("button");
      removeBtn.classList = "removeBtn";
      removeBtn.textContent = "X";
      removeBtn.addEventListener("click", async () => {
        const result = await window.api.removeWhitelist(name);
        if (result.success) {
          pre.textContent =
            "<System> " + name + " wurde aus der Liste entfernt.";
        } else {
          pre.textContent = "<System> " + name + " war nicht in der Liste.";
        }
        renderWhitelist(result.whitelist);
      });

      li.appendChild(removeBtn);
      whitelistList.appendChild(li);
    });
  }

  processes.forEach((name) => {
    let option = document.createElement("option");
    option.textContent = name;
    option.value = name;
    processDropdown.appendChild(option);
  });

  addProcessBtn.addEventListener("click", async () => {
    let name = processDropdown.value;
    if (name == "" || name == null) {
      pre.textContent = "<System> Du musst einen Prozess ausgewählt haben.";
      return;
    }

    const result = await window.api.addWhitelist(name);
    if (result.success) {
      pre.textContent = "<System> " + name + " wurde der Liste hinzugefügt.";
    } else {
      pre.textContent = "<System> " + name + " ist bereits in der Liste.";
    }
    renderWhitelist(result.whitelist);
    processDropdown.value = "";
  });
});
