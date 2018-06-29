
$im.radio = {
    adfFreq: function(adf) {
        return "" + Math.floor(adf) + (Math.floor(adf * 10) % 10) + (Math.floor(adf * 100) % 10) + (Math.floor(adf * 1000) % 10) + "."  + (Math.round(adf * 10000) % 10)
    },
    configuration: {
        adf: false,
        dme: false,
        xpdr: false,
        comm1: false,
        comm2: false
    },
    bcdToMhz: function(nCom) {
        return "1" + (nCom >> 12) + ((nCom >> 8) & 15) + "." + ((nCom >> 4) & 15) + (nCom & 15);
    },
    
    navFreq: function(nav) {
        return parseFloat(Math.round(nav * 100)/100).toFixed(2);
    },
    onCommData: function(id, data) {
        $im.radio["nav"+id+"act"].setValue($im.radio.navFreq(data["nav"+id+"act"]));
        $im.radio["nav"+id+"stb"].setValue($im.radio.navFreq(data["nav"+id+"stb"]));
        $im.radio["com"+id+"act"].setValue($im.radio.bcdToMhz(data["com"+id+"act"]));
        $im.radio["com"+id+"stb"].setValue($im.radio.bcdToMhz(data["com"+id+"stb"]));
        $("#com"+id+"trs").toggleClass("on", !!data["com"+id+"transmit"]);
        $("#com"+id+"rcv").toggleClass("on", !!data["com"+id+"transmit"] || !!data.comRecieveAll);
        $("#nav"+id+"idt").toggleClass("on", !!data["nav"+id+"sound"]);
    },
    onData: function(data) {
        if ($im.radio.adf !== undefined) {
            $im.radio.adf.setValue($im.radio.adfFreq(data.adf1act));
            $("#adfident").toggleClass("on", !!data.adf1sound);
        }
        if ($im.radio.xpdr !== undefined) {
            $im.radio.xpdr.setValue($im.xpdrBox.code(data.transponder));
        }
        if (!!$im.radio.configuration.dme) {
            $("#dmeswitch1").toggle(data.dmeselected != 2);
            $("#dmeswitch2").toggle(data.dmeselected == 2);
        }
        if (!!$im.radio.configuration.comm1) {
            $im.radio.onCommData(1, data);
        }
        if (!!$im.radio.configuration.comm2) {
            $im.radio.onCommData(2, data);
        }
    },
    setupComm(id, template) {
        $im.appendTemplate(template.replace(/{id}/g, id));
        $im.radio["com"+id+"act"] = $im.setupDisplay("com"+id+"act", "###.##", "123.45");
        $im.radio["com"+id+"stb"] = $im.setupDisplay("com"+id+"stb", "###.##", "123.45");
        $im.radio["nav"+id+"act"] = $im.setupDisplay("nav"+id+"act", "###.##", "123.45");
        $im.radio["nav"+id+"stb"] = $im.setupDisplay("nav"+id+"stb", "###.##", "123.45");   
    },

    loadAdf: function() {
        return $im.template("templates/adf.html", (template) => {
            $im.appendTemplate(template);
            $im.radio["adf"] = $im.setupDisplay("adf", "####.#", "0123.4")
        });

    },
    loadDme: function() {
        return $im.template("templates/dme.html", (template) => {
            $im.appendTemplate(template);
            $im.radio["dmedist"] = $im.setupDisplay("dmedist", "###.###", "---.-NM", {segmentCount: 14, digitHeight: 22})
            $im.radio["dmespeed"] = $im.setupDisplay("dmespeed", "#####", "---KT", {segmentCount: 14, digitHeight: 19, segmentWidth: 1.6, digitWidth: 11})
        });

    },
    loadXpdr: function() {
        return $im.template("templates/xpdr.html", (template) => {
            $im.appendTemplate(template);
            $im.radio["xpdr"] = $im.setupDisplay("xpdr", "####", "1234");
        });

    },
    loadComm: function() {
        return $im.template("templates/comm.html", (template) => {
            if ($im.radio.configuration.comm1) {
                $im.radio.setupComm("1", template);
            }
            if ($im.radio.configuration.comm2) {
                $im.radio.setupComm("2", template);
            }
        });
    },
    onLoad: function() {
        var loadPromise = $.Deferred();
        loadPromise.resolve("");
        if ($im.radio.configuration.comm1 || $im.radio.configuration.comm1) {
            loadPromise = loadPromise.then($im.radio.loadComm);
        }
        if ($im.radio.configuration.adf) {
            loadPromise = loadPromise.then($im.radio.loadAdf);
        }
        if ($im.radio.configuration.dme) { 
            loadPromise = loadPromise.then($im.radio.loadDme);
        }
        if ($im.radio.configuration.xpdr) { 
            loadPromise = loadPromise.then($im.radio.loadXpdr);
        }
        loadPromise.then(function() { $im.start({ Radio: $im.radio.onData }); });
    }
};

$().ready($im.radio.onLoad);

$im.xpdrBox = {
    digitPos: 0,
    digits: [0, 0, 0, 0],
    digit: function (digit) {
        $im.xpdrBox.digits[$im.xpdrBox.digitPos] = digit;
        var transponderCode = ($im.xpdrBox.digits[0] << 12) + ($im.xpdrBox.digits[1] << 8) + ($im.xpdrBox.digits[2]  << 4) + $im.xpdrBox.digits[3];
        
        $im.sendEvent("XPNDR_SET", transponderCode)
        $im.xpdrBox.digitPos++;
        if ($im.xpdrBox.digitPos > 3) {
            $im.xpdrBox.digitPos = 0;
        }
    },
    set: function(transponder) {
        $im.xpdrBox.digits[0] = parseInt(transponder.charAt(0));
        $im.xpdrBox.digits[1] = parseInt(transponder.charAt(1));
        $im.xpdrBox.digits[2] = parseInt(transponder.charAt(2));
        $im.xpdrBox.digits[3] = parseInt(transponder.charAt(3));
        var transponderCode = ($im.xpdrBox.digits[0] << 12) + ($im.xpdrBox.digits[1] << 8) + ($im.xpdrBox.digits[2]  << 4) + $im.xpdrBox.digits[3];
        $im.sendEvent("XPNDR_SET", transponderCode)
    },
    code: function(transponder) {
        $im.xpdrBox.digits[0] = (transponder >> 12);
        $im.xpdrBox.digits[1] = ((transponder >> 8) & 15);
        $im.xpdrBox.digits[2] = ((transponder >> 4) & 15);
        $im.xpdrBox.digits[3] = transponder & 15;
        return "" + $im.xpdrBox.digits[0] + $im.xpdrBox.digits[1] + $im.xpdrBox.digits[2] + $im.xpdrBox.digits[3];
    }
}
