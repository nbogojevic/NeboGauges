$im.maxSize = Math.min(Math.max(document.documentElement.clientWidth, window.innerWidth || 0), Math.max(document.documentElement.clientHeight, window.innerHeight || 0));

$im.queryUrl = "/_dashboard";
$im.interval = 100;

$().ready($im.start);
