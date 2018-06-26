$im = $im || {};

$im.keypad = {

    show: function(event) {
        if ($("#keypad").length == 0) {
            $im.template("templates/keypad.html", function (template) { $im.appendTemplate(template); }).then(function () { 
                $im.keypad.display = $im.setupDisplay("keypad-enterfrequency", "###.###", "123.455")
                $im.keypad.type = $im.setupDisplay("keypad-frequencytype", "#####", "NAV1", {segmentCount: 14})
                $im.keypad.doShow(event) 
            }); 
        } else {
            $im.keypad.doShow(event);
        } 
    },
    doShow: function(event) {
        $im.keypad.updateEvent = event;
        $im.keypad.type.colorOff = "#000";
        $im.keypad.value = "";
        $im.keypad.mode = event.substring(0, 3).toUpperCase();
        $(".keypad-key").prop('disabled', false);
        if ($im.keypad.mode == "COM") {
            $im.keypad.name = event.substring(0, 4).toUpperCase();
            $im.keypad.display.pattern = "###.##";
            $im.keypad.value = "1";
        } else if ($im.keypad.mode == "NAV") {
            $im.keypad.name = event.substring(0, 4).toUpperCase();
            $im.keypad.display.pattern = "###.##";        
            $im.keypad.value = "1";
        } else if ($im.keypad.mode == "ADF") {
            $im.keypad.name = event.substring(0, 3).toUpperCase();
            $im.keypad.display.pattern = "####.#";
        } else if ($im.keypad.mode == "XPN") {
            $im.keypad.name = "XPNDR";
            $im.keypad.display.pattern = "####";
            $(".keypad-no-xpdr").prop('disabled', true);
        }
        $im.keypad.type.setValue($im.keypad.name);
        $im.keypad.display.setValue($im.keypad.value);
        $("#keypad").show();
        $(".keypad-enter").prop('disabled', true);
    },

    keyPressed: function(key) {
        if ($im.keypad.mode == "COM") {
            if ($im.keypad.value.length == 1 && (key < 1 || key > 3)) {
                return;
            }
            if ($im.keypad.value.length == 2) {
                if ($im.keypad.value.charAt(1) == '3' && key > 6) {
                    return;
                }
                if ($im.keypad.value.charAt(1) == '1' && key < 8) {
                    return;
                }
            }
            if ($im.keypad.value.length == 3) {
                $im.keypad.value += ".";
            }
            if ($im.keypad.value.length == 5) {
                if (key != 5 && key != 0 && key != 2 && key != 7) {
                    return;
                }

                $(".keypad-enter").prop('disabled', false);
            }
            if ($im.keypad.value.length > 5) {
                return;
            }
        }
        if ($im.keypad.mode == "NAV") {
            if ($im.keypad.value.length == 1 && key > 1) {
                return;
            }
            if ($im.keypad.value.length == 2) {
                if ($im.keypad.value.charAt(1) == '1' && key > 7) {
                    return;
                }
                if ($im.keypad.value.charAt(1) == '0' && key < 8) {
                    return;
                }
            }
            if ($im.keypad.value.length == 3) {
                $im.keypad.value += ".";
            }
            if ($im.keypad.value.length > 5) {
                return;
            }
            if ($im.keypad.value.length == 5) {
                if (key != 5 && key != 0) {
                    return;
                }
                $(".keypad-enter").prop('disabled', false);
            }
        }
        if ($im.keypad.mode == "XPN") {
            if ($im.keypad.value.length > 3) {
                return;
            }
            if (key > 7) {
                return;
            }
            if ($im.keypad.value.length == 3) {
                $(".keypad-enter").prop('disabled', false);
            }
        }
        if ($im.keypad.mode == "ADF") {
            if ($im.keypad.value.length == 0 && key > 1) {
                return;
            }
            if ($im.keypad.value.length == 1) {
                if ($im.keypad.value.charAt(0) == '0') {
                    if (key === 0) {
                        return;
                    }
                } else if ($im.keypad.value.charAt(0) == '1') {
                    if (key > 7) {
                        return;
                    }
                }
            }
            if ($im.keypad.value.length == 2) {
                if ($im.keypad.value.startsWith("17")) {
                    if (key > 5) {
                        return;
                    }
                }
                if ($im.keypad.value.startsWith("01")) {
                    if (key < 9) {
                        return;
                    }
                }
            }
            if ($im.keypad.value.length == 4) {
                $im.keypad.value += ".";
            }
            if ($im.keypad.value.length == 5) {
                $(".keypad-enter").prop('disabled', false);
            }
            if ($im.keypad.value.length > 5) {
                return;
            }
        }
        $im.keypad.value += key;
        $im.keypad.display.setValue($im.keypad.value);
    },

    clearPressed: function() {
        $im.keypad.value = "";
        $im.keypad.display.setValue($im.keypad.value);
    },

    closePressed: function () {
        $("#keypad").hide();
    },

    enterPressed: function() {
        var value = 0;
        if ($im.keypad.mode == "ADF") {
            value = (parseInt($im.keypad.value.charAt(0)) << 28) +
                    (parseInt($im.keypad.value.charAt(1)) << 24) +
                    (parseInt($im.keypad.value.charAt(2)) << 20) +
                    (parseInt($im.keypad.value.charAt(3)) << 16) +
                    (parseInt($im.keypad.value.charAt(5)) << 12);
        }
        else if ($im.keypad.mode == "XPN") {
            value = (parseInt($im.keypad.value.charAt(0)) << 12) +
                    (parseInt($im.keypad.value.charAt(1)) << 8) +
                    (parseInt($im.keypad.value.charAt(2)) << 4) +
                    (parseInt($im.keypad.value.charAt(3))); 

        } else if ($im.keypad.mode == "COM" || $im.keypad.mode == "NAV") {
            var frq  = parseFloat($im.keypad.value).toFixed(2);
            // make sure you have 00.00 format !!! and cut 1xx from BCD value.
            var n1 = frq.substr(0, frq.indexOf("."));
            n1 = n1.substr(-2);
            var n2 = frq.substr(frq.indexOf(".")+1,2);
            value = (parseInt(n1.charCodeAt(0) - 0x30 )<<12) + (parseInt(n1.charCodeAt(1) - 0x30 )<<8) + (parseInt(n2.charCodeAt(0) - 0x30 )<<4) + parseInt(n2.charCodeAt(1) - 0x30 );
        }
        $im.sendEvent($im.keypad.updateEvent, value)
        $("#keypad").hide();
    }
};
