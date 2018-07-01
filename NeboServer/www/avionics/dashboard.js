$im.maxSize = Math.min(Math.max(document.documentElement.clientWidth, window.innerWidth || 0), Math.max(document.documentElement.clientHeight, window.innerHeight || 0));

$().ready(function () { $im.start({subscriptions: { Dashboard: null }}); });
