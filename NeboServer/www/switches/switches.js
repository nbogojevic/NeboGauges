$im = $im || {};

$im.switches = {
    onLoad: function() {
        var self = $im.switches;
        $(".toggle").click(self.toggleClick);
        $(".white").click(self.toggleClick);
        $(".knob").click(self.toggleClick);
        $("#MASTER").click(self.masterClick);
        $("#MAGNETO").click(self.magnetoClick);
        $im.start({subscriptions: { Switches: self.readResult }});
    },

    magnetoClick: function(e) {
        var data = $(e.target).data();
        var left = e.offsetX < e.target.clientWidth/2;
        console.log(data);
        if (!data || !data.magneto) {
            $(e.target).data().magneto = 0;
        } 
        if (left && data.magneto > 0) {
            $im.sendEvent("MAGNETO_DECR", 1);
            data.magneto--;
        } else if (data.magneto < 4) {
            data.magneto++;
            if (data.magneto == 4) {
                $(".magneto-4").show();
                $(".ignition-key:not(.magneto-4)").hide();
                setTimeout(function() {
                    if (data.magneto == 4) {
                        data.magneto = 3;
                        $(".ignition-key:not(.magneto-3)").hide();
                        $(".magneto-3").show();
                    }
                }, 1000);
            }
            $im.sendEvent("MAGNETO_INCR", 1);
        } else {
            return;
        }
    },

    masterClick: function(e) {
        var data = $(e.target).data();
        var left = e.offsetX < e.target.clientWidth/2; 
        if (left) {
            if (data.master == 0) {
                $im.sendEvent("BATTERY").always( function () { $im.sendEvent("ALTERNATOR") });
            } else {
                $im.sendEvent("ALTERNATOR");
            }
        } else {
            $im.sendEvent("BATTERY");
        }
    },

    masterSet: function(value) {
        $('#MASTER').data().master = value;
        $('#MASTER').toggleClass('off', value == 0).toggleClass('battery', value == 1).toggleClass('alternator', value == 2);
    },

    toggleClick: function(e) {
        $im.switches.toggleSet(e.target, $(e.target).hasClass("off"))
        $im.sendEvent(e.target.id);
    },

    toggleSet: function(id, value) {
        $(id).toggleClass("on", !!value).toggleClass("off", !value);
    },

    readResult: function(switches_status) {
        var self = $im.switches;
        self.toggleSet("#NAV", (switches_status.lights & 0x0001));
        self.toggleSet("#BCN", (switches_status.lights & 0x0002));
        self.toggleSet("#LAND", (switches_status.lights & 0x0004));
        self.toggleSet("#TAXI", (switches_status.lights & 0x0008));
        self.toggleSet("#STROBE", (switches_status.lights & 0x0010));
        self.toggleSet("#PANEL", (switches_status.lights & 0x0020));
        self.toggleSet("#PITOT", switches_status.pitot);
        self.toggleSet("#PUMP", switches_status.pump);
        self.toggleSet("#AVIONICS", switches_status.avionics);
        if (switches_status.alternator) {
            self.masterSet(2);
        } else if (switches_status.battery) {
            self.masterSet(1);
        } else {
            self.masterSet(0);
        }
        if (switches_status.ignition) {
            $("#MAGNETO").show();
            var data = $("#MAGNETO").data();
            var isIgnition = data.magneto === 4;
            if (switches_status.magnetoLeft) {
                data.magneto = switches_status.magnetoRight ? 3 : 2;
            } else {
                data.magneto = switches_status.magnetoRight ? 1 : 0;
            }
            if (isIgnition) {
                $(".ignition-key:not(.magneto-" + data.magneto + ")").hide();
                $(".magneto-"+data.magneto).show();
            }
        }
    }
}

$().ready($im.switches.onLoad);

