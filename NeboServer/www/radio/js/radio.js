$.extend($im, {
    radio: {
        adfFreq: function(adf) {
            return "" + Math.floor(adf) + (Math.floor(adf * 10) % 10) + (Math.floor(adf * 100) % 10) + (Math.floor(adf * 1000) % 10) + "."  + (Math.round(adf * 10000) % 10)
        },
        bcdToMhz: function(nCom) {
            return "1" + (nCom >> 12) + ((nCom >> 8) & 15) + "." + ((nCom >> 4) & 15) + (nCom & 15);
        },
        navFreq: function(nav) {
            return parseFloat(Math.round(nav * 100)/100).toFixed(2);
        },
        onCommData: function(id, data) {
            $im.display["nav"+id+"act"].setValue($im.radio.navFreq(data["nav"+id+"act"]));
            $im.display["nav"+id+"stb"].setValue($im.radio.navFreq(data["nav"+id+"stb"]));
            $im.display["com"+id+"act"].setValue($im.radio.bcdToMhz(data["com"+id+"act"]));
            $im.display["com"+id+"stb"].setValue($im.radio.bcdToMhz(data["com"+id+"stb"]));
            $("#com"+id+"trs").toggleClass("on", !!data["com"+id+"transmit"]);
            $("#com"+id+"rcv").toggleClass("on", !!data["com"+id+"transmit"] || !!data.comRecieveAll);
            $("#nav"+id+"idt").toggleClass("on", !!data["nav"+id+"sound"]);
        },
        onData: function(data) {
            if ($im.display.adf !== undefined) {
                $im.display.adf.setValue($im.radio.adfFreq(data.adf1act));
                $("#adfident").toggleClass("on", !!data.adf1sound);
            }
            if ($im.display.xpdr !== undefined) {
                $im.display.xpdr.setValue($im.xpdr.code(data.transponder));
            }
            if (!!$im.display.dme) {
                $("#dmeswitch1").toggle(data.dmeselected != 2);
                $("#dmeswitch2").toggle(data.dmeselected == 2);
            }
            if (!!$im.display.com1act) {
                $im.radio.onCommData(1, data);
            }
            if (!!$im.display.com2act) {
                $im.radio.onCommData(2, data);
            }
        }
    },
    xpdr: {
        digitPos: 0,
        digits: [0, 0, 0, 0],
        digit: function (digit) {
            $im.xpdr.digits[$im.xpdr.digitPos] = digit;
            var transponderCode = ($im.xpdr.digits[0] << 12) + ($im.xpdr.digits[1] << 8) + ($im.xpdr.digits[2]  << 4) + $im.xpdr.digits[3];
            
            $im.sendEvent("XPNDR_SET", transponderCode)
            $im.xpdr.digitPos++;
            if ($im.xpdr.digitPos > 3) {
                $im.xpdr.digitPos = 0;
            }
        },
        set: function(transponder) {
            $im.xpdr.digits[0] = parseInt(transponder.charAt(0));
            $im.xpdr.digits[1] = parseInt(transponder.charAt(1));
            $im.xpdr.digits[2] = parseInt(transponder.charAt(2));
            $im.xpdr.digits[3] = parseInt(transponder.charAt(3));
            var transponderCode = ($im.xpdr.digits[0] << 12) + ($im.xpdr.digits[1] << 8) + ($im.xpdr.digits[2]  << 4) + $im.xpdr.digits[3];
            $im.sendEvent("XPNDR_SET", transponderCode)
        },
        code: function(transponder) {
            $im.xpdr.digits[0] = (transponder >> 12);
            $im.xpdr.digits[1] = ((transponder >> 8) & 15);
            $im.xpdr.digits[2] = ((transponder >> 4) & 15);
            $im.xpdr.digits[3] = transponder & 15;
            return "" + $im.xpdr.digits[0] + $im.xpdr.digits[1] + $im.xpdr.digits[2] + $im.xpdr.digits[3];
        }
    }
});
$im.loadScript("js/keypad.js", function() { return typeof $im.keypad; });

$().ready(function() { $im.start({subscriptions: { Radio: $im.radio.onData }}) });

