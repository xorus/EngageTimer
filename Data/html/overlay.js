const TIME_BEFORE_COMBAT_DONE = 5000; // ms
const TIME_BEFORE_RESYNC = 15000; // ms
const REDRAWS_PER_SECOND = 10;


document.addEventListener("onOverlayStateUpdate", function (e) {
    // console.log("coucou", e)
});

let started = false;
let startTime = null;
let lastUpdate = null;
let lastDuration = null;
let lastSync = 0;

let inCombat = true;

let timerEl = document.getElementById("timer");
let minEl = document.createElement("span");
let sepEl = document.createElement("span");
let secEl = document.createElement("span");

minEl.innerText = "00";
sepEl.innerText = ":";
secEl.innerText = "00";

timerEl.appendChild(minEl);
timerEl.appendChild(sepEl);
sepEl.classList.add("sep");
timerEl.appendChild(secEl);

function draw() {
    if (new Date() - lastUpdate > TIME_BEFORE_COMBAT_DONE) {
        startTime = new Date() - (lastDuration * 1000)
    }

    if (startTime !== null) {
        let elapsed = new Date() - startTime;
        minEl.innerText = String(Math.floor(elapsed / 1000 / 60)).padStart(2, '0')
        secEl.innerText = String(Math.floor(elapsed / 1000) % 60).padStart(2, '0')
    }
}

const fpsInterval = 1000 / REDRAWS_PER_SECOND;
let lastDrawTime = 0;

function update() {
    requestAnimationFrame(function () {
        update();
    })

    timeSinceLastDraw = Date.now() - lastDrawTime;

    if (timeSinceLastDraw > fpsInterval) {
        draw();
        lastDrawTime = Date.now();
    }
}
update();


addOverlayListener('CombatData', (data) => {
    inCombat = data.isActive;

    //if (!started) {
    if (inCombat) {
        let duration = parseInt(data.Encounter.DURATION);

        lastUpdate = new Date();
        lastDuration = duration;

        if (new Date() - lastSync > TIME_BEFORE_RESYNC) {
            startTime = new Date() - (duration * 1000)
            lastSync = new Date();
        }
    } else {
        lastSync = 0;
    }

    // console.log(data)
});


// addOverlayListener('LogLine', (data) => {
//     console.log(data)
// });

startOverlayEvents();