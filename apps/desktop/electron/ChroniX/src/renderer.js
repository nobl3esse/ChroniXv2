document.addEventListener("DOMContentLoaded", async () => {
  // ============ TOAST SYSTEM ============
  function showToast(message, type = "info") {
    const container = document.getElementById("toast-container");
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    container.appendChild(toast);

    requestAnimationFrame(() => toast.classList.add("show"));

    setTimeout(() => {
      toast.classList.remove("show");
      setTimeout(() => toast.remove(), 300);
    }, 3000);
  }

  // ============ TAB SWITCHING ============
  const tabTitles = {
    times: "Zeiten",
    whitelist: "Whitelist",
    server: "Serververbindung",
    login: "Login/Token",
  };

  function switchTab(id) {
    document
      .querySelectorAll(".tab-content")
      .forEach((s) => s.classList.remove("active"));
    document
      .querySelectorAll(".menu-left button[data-tab]")
      .forEach((b) => b.classList.remove("active"));

    const section = document.getElementById(`section-${id}`);
    const button = document.querySelector(`[data-tab="${id}"]`);
    if (section) section.classList.add("active");
    if (button) button.classList.add("active");

    document.getElementById("sectionTitle").textContent = tabTitles[id] || "";

    // Tab-spezifische Aktionen beim Öffnen
    if (id === "times") loadTimes();
    if (id === "server") checkServerStatus();
  }

  document.querySelectorAll(".menu-left button[data-tab]").forEach((btn) => {
    btn.addEventListener("click", () => switchTab(btn.dataset.tab));
  });

  // Default-Tab visuell markieren
  document.querySelector('[data-tab="times"]').classList.add("active");

  // ============ TRACKING TOGGLE ============
  const trackingCheckbox = document.getElementById("tracking");
  const trackingStatus = document.getElementById("trackingStatus");

  // Beim App-Start: echten Tracking-Status vom Backend holen
  async function syncTrackingState() {
    const result = await window.api.getIsTracking();

    if (result?.error) {
      showToast(result.error, "error");
      return;
    }

    if (result.isTracking) {
      trackingCheckbox.checked = true;
      trackingStatus.textContent = "Aktiv";
    } else {
      trackingCheckbox.checked = false;
      trackingStatus.textContent = "Pausiert";
    }
  }

  trackingCheckbox.addEventListener("change", async (e) => {
    if (e.target.checked) {
      trackingStatus.textContent = "Aktiv";
      const result = await window.api.startTracking();
      if (result?.error) {
        showToast(result.error, "error");
        e.target.checked = false;
        trackingStatus.textContent = "Pausiert";
      } else {
        showToast("Tracking gestartet", "success");
      }
    } else {
      trackingStatus.textContent = "Pausiert";
      const result = await window.api.stopTracking();
      if (result?.error) {
        showToast(result.error, "error");
      } else {
        showToast("Tracking gestoppt", "info");
      }
    }
  });

  // ============ ZEITEN-TAB ============
  function formatTime(seconds) {
    const h = Math.floor(seconds / 3600);
    const m = Math.floor(seconds / 60) % 60;
    const s = seconds % 60;
    return `${h}h ${m}min ${s}s`;
  }

  async function loadTimes() {
    const times = await window.api.getTimes();
    const list = document.getElementById("timesList");
    const empty = document.getElementById("timesEmpty");
    list.innerHTML = "";

    if (times?.error) {
      empty.textContent = times.error;
      empty.style.display = "block";
      return;
    }

    const entries = Object.entries(times);
    if (entries.length === 0) {
      empty.style.display = "block";
      return;
    }

    empty.style.display = "none";
    entries
      .sort((a, b) => b[1] - a[1])
      .forEach(([name, seconds]) => {
        const li = document.createElement("li");
        li.innerHTML = `<span class="name">${name}</span><span class="time">${formatTime(seconds)}</span>`;
        list.appendChild(li);
      });
  }

  document.getElementById("refreshTimesBtn").addEventListener("click", () => {
    loadTimes();
    showToast("Zeiten aktualisiert", "info");
  });

  // ============ SERVER-TAB ============
  async function checkServerStatus() {
    const statusEl = document.getElementById("serverStatus");
    statusEl.textContent = "Wird geprüft...";
    statusEl.className = "server-status";

    const status = await window.api.getStatus();
    if (status?.error) {
      statusEl.textContent = `❌ ${status.error}`;
      statusEl.classList.add("error");
    } else {
      statusEl.textContent = `✅ Verbunden (${status.message})`;
      statusEl.classList.add("ok");
    }
  }

  document
    .getElementById("checkStatusBtn")
    .addEventListener("click", checkServerStatus);

  // ============ WHITELIST-TAB ============
  function renderWhitelist(items) {
    const list = document.getElementById("whitelistList");
    list.innerHTML = "";

    if (!items || items.length === 0) return;

    items.forEach((name) => {
      const li = document.createElement("li");
      li.className = "whitelistLi";
      li.textContent = name;

      const removeBtn = document.createElement("button");
      removeBtn.className = "removeBtn";
      removeBtn.textContent = "✕";
      removeBtn.title = "Entfernen";
      removeBtn.addEventListener("click", async () => {
        const result = await window.api.removeWhitelist(name);
        if (result.success) {
          showToast(`${name} entfernt`, "success");
        } else {
          showToast(`${name} war nicht in der Liste`, "error");
        }
        renderWhitelist(result.whitelist);
      });

      li.appendChild(removeBtn);
      list.appendChild(li);
    });
  }

  document
    .getElementById("addProcessBtn")
    .addEventListener("click", async () => {
      const dropdown = document.getElementById("processDropdown");
      const name = dropdown.value;

      if (!name) {
        showToast("Bitte einen Prozess auswählen", "error");
        return;
      }

      const result = await window.api.addWhitelist(name);
      if (result.success) {
        showToast(`${name} hinzugefügt`, "success");
      } else {
        showToast(`${name} ist bereits in der Liste`, "error");
      }
      renderWhitelist(result.whitelist);
      dropdown.value = "";
    });

  // ============ INITIAL LOAD ============
  const processes = await window.api.getProcesses();
  const dropdown = document.getElementById("processDropdown");

  if (Array.isArray(processes)) {
    processes.forEach((name) => {
      const option = document.createElement("option");
      option.textContent = name;
      option.value = name;
      dropdown.appendChild(option);
    });
  }

  const whitelist = await window.api.getWhitelist();
  if (Array.isArray(whitelist)) {
    renderWhitelist(whitelist);
  }

  // Default-Tab Daten laden
  loadTimes();
  // Tracking-Toggle mit echtem Backend-Status synchronisieren
  syncTrackingState();
});
