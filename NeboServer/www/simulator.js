/*
Nebo Dashboard by Nenad Bogojevic. Published under GPLv3 License.
*/


var $im = {
    _wakeLockEnabled: false,
    _noSleepSetUp: false,

    continueReady: function() {
        $.holdReady(false);
    },
    initNoSleep: function () {
        if (!$im._noSleepSetUp) {
            $im.noSleep = new NoSleep();

            var toggleEl = $(".disable-lock");
            if (toggleEl.length !== 0) {
                $im._noSleepSetUp = true;
                toggleEl.click(function() {
                    if (!$im._wakeLockEnabled) {
                        $im.noSleep.enable(); // keep the screen on!
                        toggleEl.text("ENABLE SCREEN LOCK");
                        $im._wakeLockEnabled = true;
                    } else {
                        $im.noSleep.disable(); // let the screen turn off.
                        toggleEl.text("DISABLE SCREEN LOCK");
                        $im._wakeLockEnabled = false;
                    }
                });
            }    
        }
    },
    
    start: function (options) {
        options = options || {};

        if (!options.static) {        
            $im.initWebsocket(options);
        }
        $(".nebo-template").each(function () {
            var container = $(this);
            container.load(container.data().template, function (_, status) {
                if (status != "error") {
                    if (container.data().parameters) {
                        var contents = container.html();
                        var params = container.data().parameters;
                        for (var p in params) {
                            var contents = contents.replace(new RegExp("\{"+p+"}", "g"), params[p]);
                        }
                        container.html(contents);
                    }
                    container.find("canvas.segmentDisplay").each(function () {
                        $im.display = $im.display || {};
                        var id = $(this).attr("id");
                        $im.display[id] = $im.setupDisplay(id, $(this).data().segmentdisplay);
                    }); 
                }
            });
        });
        $("#gaugeSelector").load("/navigation.html", function() { $im.initNoSleep(); });

    },

    loadScript: function (script, check, init) {
        var onScriptLoad = function() {
            if (check() !== "undefined") {
                $.holdReady(false);
            }
            else {
                setTimeout(onScriptLoad, 10);
            }
        }
        $.holdReady(true);
        $.getScript(script, onScriptLoad); 
    },
    
    initWebsocket: function (options) {
        $im.subscriptions = options.subscriptions || $im.subscriptions || {};
        $im.websocket = new ReconnectingWebSocket("ws://"+location.host+"/_gauges", null, {reconnectInterval: 1000, maxReconnectInterval: 10000});
        $im.websocket.onopen = function (event) { 
            $('#disconnectedSplash').hide();
            for (var p in $im.subscriptions) {
                $im.websocket.send(p);
            }
        };
        $im.websocket.onmessage = function (event) { 
            var feedData = JSON.parse(event.data);
            var subscription = $im.subscriptions[feedData.feed] || $im.readResult || function (data) { console.log(data); };
            subscription(feedData.data, feedData.feed);
        };
        $im.websocket.onclose = function (event) {
            if ($('#disconnectedSplash').length === 0) {
                $('body').append('<div id="disconnectedSplash" style="left:0; top:0; width:100%; height:100%; opacity: 0.5; font-family:monospace; font-size:xx-large; display: none; background-color: blue;color: white; position: absolute"><p>Disconnected from simulator</p><p id="disconnectedCause"></p><p>Reconnecting...</p></div>');
            }
            $('#disconnectedSplash').show();
        }
    },
    sendEvent: function (event, value)  {
        return $.post("/_event/" + event + "/" + (value !== undefined ? value : 0));
    },
    setupDisplay: function (id, options) {
        options = options || {};
        var display = new SegmentDisplay(id);
        display.pattern         = options.pattern || "##:##";
        display.displayAngle    = options.displayAngle || 0;
        display.digitHeight     = options.digitHeight || 19;
        display.digitWidth      = options.digitWidth || 10;
        display.digitDistance   = options.digitDistance || 2.5;
        display.segmentWidth    = options.segmentWidth || 1.6;
        display.segmentDistance = options.segmentDistance || 0.3;
        display.segmentCount    = options.segmentCount || 7;
        display.cornerType      = options.cornetType || 3;
        display.colorOn         = options.colorOn || "#e90000";
        display.colorOff        = options.colorOff || "#422620";
        display.draw();
        display.setValue(options.value || "");
        return display;
    }
}

$im.loadScript("/js/NoSleep.min.js", function() { return typeof NoSleep; });
$im.loadScript("/js/reconnecting-websocket.js", function() { return typeof ReconnectingWebSocket; });
$im.loadScript("/js/segment-display.js", function() { return typeof SegmentDisplay; });

