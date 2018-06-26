/*
Nebo Dashboard by Nenad Bogojevic. Published under GPLv3 License.
*/
var $im = {
    apiServer: "",
    queryUrl: "",
    switching: 0,
    noUpdate: false,
    interval: 1000,
    debug: false,
    pauseOnEvent: true,
    wakeLockEnabled: false,
    
    start: function () {
        $im.noSleep = new NoSleep();

        var toggleEl = $(".disable-lock");
        if (toggleEl.length !== 0) {
            toggleEl.click(function() {
                if (!$im.wakeLockEnabled) {
                    noSleep.enable(); // keep the screen on!
                    $im.wakeLockEnabled = true;
                } else {
                    noSleep.disable(); // let the screen turn off.
                    $im.wakeLockEnabled = false;
                }
            });
        }

        $(document).ajaxError($im.stopPolling);
        $im.queryValues();
        $im.startPolling();
    },
    startPolling: function () {
        $im.pollFunction = setInterval($im.queryValues, $im.interval);
    },
    pausePolling: function () {
        if ($im.pollFunction) {
            clearInterval($im.pollFunction);
        }
    },
    queryValues: function () {
        if ($im.switching <= 0) {
            $im.switching = 0;
            $im.getJSON($im.queryUrl, (data) => {
                if ($im.switching <= 0) {
                    $im.noUpdate = true;
                    if (data !== undefined && $im.readResult) {
                        $im.readResult(data);
                    }
                    $im.last_state = data;
                    $im.noUpdate = false;
                }
            });
        }
    },
    endUpdate: function (data) {
        $im.switching--;
        if ($im.pauseOnEvent) {
            $im.startPolling();
        }
    },
    sendEvent: function (event, value) {
        return $im.chainEvent(event, value).always($im.endUpdate);
    },
    chainEvent: function (event, value) {
        if ($im.pauseOnEvent) {
            $im.pausePolling();
        }
        $im.switching++;
        return $.get($im.apiServer + "/_event/" + event + "/" + (value !== undefined ? value : 0));
    },
    getJSON: function (url, fn) {
        return $.getJSON($im.apiServer + url, fn);
    },
    updateAllowed: function () {
        return $im.debug || (!$im.noUpdate && $im.last_state !== undefined);
    },
    stopPolling: function (event, jqXHR, textStatus, errorThrown) {
        $im.onDisconnect(event, jqXHR, textStatus, errorThrown);
    },
    onDisconnect: function (event, jqXHR, textStatus, errorThrown) {
        if ($('#disconnectedSplash').length === 0) {
            $('body').append('<div id="disconnectedSplash" style="left:0; top:0; width:100%; height:100%; opacity: 0.5; font-family:monospace; font-size:xx-large; display: none; background-color: blue;color: white; position: absolute"><p>Disconnected from simulator</p><p id="disconnectedCause"></p><p>Retry attempt: <span id="disconnectedReconnectRetry">&nbsp;</span></p></div>');
        }
        if ($im.pollFunction) {
            clearInterval($im.pollFunction);
            $im.pollFunction = null;
            $im.pollRetry = 0;
            $("#disconnectedCause").text(errorThrown.message);
            $im.retryFunction = setInterval($im.retryConnection, 2000);
        }
    },

    retryConnection: function () {
        if (!$im.pollFunction) {
            if (!$im.noSplash) {
                $('#disconnectedSplash').show();
            }
            $('#disconnectedReconnectRetry').text(++$im.pollRetry);
            $im.getJSON("/_status", function (status) {
                if (status.connected) {
                    $('#disconnectedSplash').hide();
                    clearInterval($im.retryFunction);
                    $im.retryFunction = null;
                    if ($im.start) {
                        $im.start();
                    }
                }
            });
        }
    },
    setupDisplay: function (id, pattern, value, options) {
        options = options || {};
        var display = new SegmentDisplay(id);
        display.pattern         = pattern;
        display.displayAngle    = 0;
        display.digitHeight     = options.digitHeight || 19;
        display.digitWidth      = options.digitWidth || 10;
        display.digitDistance   = 2.5;
        display.segmentWidth    = options.segmentWidth || 1.6;
        display.segmentDistance = 0.3;
        display.segmentCount    = options.segmentCount || 7;
        display.cornerType      = 3;
        display.colorOn         = "#e90000";
        display.colorOff        = "#422620";
        display.draw();
        display.setValue(value);
        return display;
    },
    appendTemplate(template) {
        $("body").append(template);
    },
    template: function(url, fn) {
        return $.get(url, (template) => {
            fn(template);
        });

    },    
}

var fsx = $im;