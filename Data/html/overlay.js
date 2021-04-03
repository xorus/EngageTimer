const REDRAWS_PER_SECOND = 10;

let state = {
    reset: true,
    CountingDown: false,
    InCombat: false,
    CombatStart: new Date(),
    CombatEnd: new Date(),
    // CountDownValue: 0,
    CountDownEnd: new Date(),
    TimeDelta: 0 // lag compensation
};

// needed in case the streaming machine and the game machine don't have perfectly synced clocks
const clockSyncedCurrentDate = () => {
    const date = new Date();
    date.setMilliseconds(date.getMilliseconds() - state.TimeDelta);
    return date;
}

const connect = () => {
    let loc = window.location, new_uri;
    if (loc.protocol === "https:") {
        new_uri = "wss:";
    } else {
        new_uri = "ws:";
    }
    new_uri += "//" + loc.host;
    new_uri += loc.pathname + "ws";

    let ws = new WebSocket(new_uri);
    ws.addEventListener('open', () => {
        console.log("connected")
    });
    ws.addEventListener('message', (data) => {
        const msg = JSON.parse(data.data);

        state.TimeDelta = new Date() - new Date(msg.Now)
        state.InCombat = msg.InCombat;
        state.CountingDown = msg.CountingDown;
        state.CombatStart = new Date(msg.CombatStart);
        state.CombatEnd = new Date(msg.CombatEnd);
        // state.CountDownValue = msg.CountDownValue;
        state.CountDownEnd = new Date();
        state.CountDownEnd.setMilliseconds(state.CountDownEnd.getMilliseconds() + Math.floor(msg.CountDownValue * 1000));

        if (state.InCombat) {
            state.reset = false;
        } else if (state.CountingDown) {
            state.reset = true;
        }

        // console.log(state);
    })
    ws.addEventListener('close', () => setTimeout(() => connect(), 10000))
};

connect();

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
    sepEl.innerText = state.TimeDelta;
    let countDownDiff = state.CountDownEnd - new Date();
    if (state.CountingDown && countDownDiff > 0) {
        minEl.style.visibility = 'hidden';
        sepEl.innerText = '-';
        secEl.innerText = String(Math.floor(countDownDiff / 1000) % 60).padStart(2, '0')
        secEl.innerText += "." + String(Math.floor(countDownDiff / 100) % 10)
    } else {
        let diff;
        if (state.reset) {
            diff = 0;
        } else if (state.InCombat) {
            diff = clockSyncedCurrentDate() - state.CombatStart;
        } else {
            diff = state.CombatEnd - state.CombatStart;
        }
        minEl.style.visibility = 'visible';
        sepEl.innerText = ':';
        minEl.innerText = String(Math.floor(diff / 1000 / 60)).padStart(2, '0')
        secEl.innerText = String(Math.floor(diff / 1000) % 60).padStart(2, '0')
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
