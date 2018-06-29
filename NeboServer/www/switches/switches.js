
function initPage() {
    $(".toggle").click(toggleClick);
    $(".white").click(toggleClick);
    $(".knob").click(toggleClick);
    $("#MASTER").click(masterClick);
    $("#MAGNETO").click(magnetoClick);
    $im.start({
        Switches: readResult
    });
}

var magneto_switch = 0;
function magnetoStopIgnition() {
    if (magneto_switch == 4) {
        magneto_switch = 3;
        $(".magneto-4").hide();
        $(".magneto-3").show();
    }
}
function magnetoClick(e) {
    var left = e.offsetX < e.target.clientWidth/2; 
    if (left && magneto_switch > 0) {
        $im.sendEvent("MAGNETO_DECR", 1);
        magneto_switch--;
    } else if (magneto_switch < 4) {
        magneto_switch++;
        if (magneto_switch == 4) {
            $(".magneto-4").show();
            $(".magneto-3").hide();
            setTimeout(magnetoStopIgnition, 1000);
        }
        $im.sendEvent("MAGNETO_INCR", 1);
    } else {
        return;
    }
}

var master_switch = 0;

function masterClick(e) {
    var left = e.offsetX < e.target.clientWidth/2; 
    if (left) {
        if (master_switch == 0) {
            $im.sendEvent("BATTERY").always( function () { $im.sendEvent("ALTERNATOR") });
        } else {
            $im.sendEvent("ALTERNATOR");
        }
    } else {
        $im.sendEvent("BATTERY");
    }
}

function masterSet(value) {
    master_switch = value;
    $('#MASTER').toggleClass('off', value == 0).toggleClass('battery', value == 1).toggleClass('alternator', value == 2);
}

function toggleClick(e) {
    toggleSet(e.target.id, $(e.target).hasClass("off"))
    $im.sendEvent(e.target.id);
}

function toggleSet(id, value) {
    $("#" + id).toggleClass("on", !!value).toggleClass("off", !value);
}

function toggleLights(switches_status) {
    toggleSet("NAV", (switches_status.lights & 0x0001));
    toggleSet("BCN", (switches_status.lights & 0x0002));
    toggleSet("LAND", (switches_status.lights & 0x0004));
    toggleSet("TAXI", (switches_status.lights & 0x0008));
    toggleSet("STROBE", (switches_status.lights & 0x0010));
    toggleSet("PANEL", (switches_status.lights & 0x0020));
    // document.getElementById('cabinlights').checked = definition.lights & 0x0200;
}

function readResult(switches_status) {
    toggleLights(switches_status);
    toggleSet("PITOT", switches_status.pitot);
    toggleSet("PUMP", switches_status.pump);
    toggleSet("AVIONICS", switches_status.avionics);
    if (switches_status.alternator) {
        masterSet(2);
    } else if (switches_status.battery) {
        masterSet(1);
    } else {
        masterSet(0);
    }
    if (switches_status.ignition) {
        $("#MAGNETO").show();
        var old_magneto = magneto_switch;
        if (switches_status.magnetoLeft) {
            magneto_switch = switches_status.magnetoRight ? 3 : 2;
        } else {
            magneto_switch = switches_status.magnetoRight ? 1 : 0;
        }
        if (old_magneto != 4) {
            $(".magneto-"+old_magneto).hide();
            $(".magneto-"+magneto_switch).show();
        }

    }
}

$().ready(initPage);

