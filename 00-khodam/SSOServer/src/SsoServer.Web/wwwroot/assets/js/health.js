// SsoServer - live service status for the admin health page.
// Polls /Admin/HealthServices (cookie-authenticated) for the server-side checks
// (database, signing key, token pruning) and fetches the public endpoints
// (/health, discovery, JWKS, swagger) straight from the browser, so the green/red
// indicators reflect exactly what a client machine experiences.
// The list refreshes automatically every 30 seconds and on demand via the button.
// CSP note: served from /assets/js ('self') - no inline code anywhere.
(function () {
    'use strict';

    var REFRESH_SECONDS = 30;
    var STATUS_URL = '/Admin/HealthServices';
    var SERVER_KEYS = ['database', 'signing-key', 'token-pruning', 'active-directory'];

    var card = document.getElementById('service-status-card');
    if (!card) {
        return;
    }

    var summary = document.getElementById('health-summary');
    var updated = document.getElementById('health-updated');
    var countdown = document.getElementById('health-countdown');
    var refreshButton = document.getElementById('health-refresh');
    var rows = Array.prototype.slice.call(
        document.querySelectorAll('#service-list li[data-service]'));

    var results = {};
    var inFlight = false;
    var sessionExpired = false;
    var remaining = REFRESH_SECONDS;

    function faDigits(value) {
        try {
            return value.toLocaleString('fa-IR');
        } catch (error) {
            return String(value);
        }
    }

    function rowByKey(key) {
        for (var i = 0; i < rows.length; i++) {
            if (rows[i].getAttribute('data-service') === key) {
                return rows[i];
            }
        }
        return null;
    }

    function setPending(li) {
        var badge = li.querySelector('.status-badge');
        var detail = li.querySelector('.service-detail');
        if (badge) {
            badge.className = 'service-badge status-badge badge text-bg-secondary';
            badge.innerHTML = '<span class="spinner-border spinner-border-sm" aria-hidden="true"></span>';
        }
        if (detail) {
            detail.textContent = 'در حال بررسی…';
            detail.classList.remove('ltr-inline');
        }
    }

    function setBadge(key, status, detailText) {
        var li = rowByKey(key);
        if (!li) {
            return;
        }
        var map = {
            up: { cls: 'text-bg-success', label: 'فعال' },
            degraded: { cls: 'text-bg-warning', label: 'هشدار' },
            down: { cls: 'text-bg-danger', label: 'قطع' },
            // Optional integration switched off in settings — informational, never a failure.
            disabled: { cls: 'text-bg-secondary', label: 'غیرفعال' }
        };
        var item = map[status] || map.down;
        var badge = li.querySelector('.status-badge');
        if (badge) {
            badge.className = 'service-badge status-badge badge ' + item.cls;
            badge.textContent = '\u25CF ' + item.label;
        }
        results[key] = map[status] ? status : 'down';
    }

    function setDetail(key, detailText, ltr) {
        var li = rowByKey(key);
        if (!li) {
            return;
        }
        var detail = li.querySelector('.service-detail');
        if (detail) {
            detail.textContent = detailText;
            if (ltr) {
                detail.classList.add('ltr-inline');
            } else {
                detail.classList.remove('ltr-inline');
            }
        }
    }

    function loadServerStatus() {
        return fetch(STATUS_URL, { cache: 'no-store', redirect: 'error' })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('HTTP ' + response.status);
                }
                return response.json();
            })
            .then(function (payload) {
                (payload.services || []).forEach(function (service) {
                    setBadge(service.key, service.status, null);
                    setDetail(service.key, service.detail || '', false);
                });
            })
            .catch(function () {
                // Cookie auth redirects to the login page (fetch would follow it and
                // return HTML) - with redirect:'error' any failure here means the
                // admin session is gone or the server did not answer.
                sessionExpired = true;
                SERVER_KEYS.forEach(function (key) {
                    setBadge(key, 'down', null);
                    setDetail(key, 'وضعیت نامشخص — نشست منقضی شده یا سرور پاسخ نداد', false);
                });
            });
    }

    function checkUrl(li) {
        var key = li.getAttribute('data-service');
        var url = li.getAttribute('data-url');
        var started = performance.now();
        return fetch(url, { cache: 'no-store' })
            .then(function (response) {
                var elapsed = Math.max(1, Math.round(performance.now() - started));
                if (response.ok) {
                    setBadge(key, 'up', null);
                    setDetail(key, url + ' \u2014 HTTP ' + response.status + ' (' + elapsed + ' ms)', true);
                } else {
                    setBadge(key, 'down', null);
                    setDetail(key, url + ' \u2014 HTTP ' + response.status, true);
                }
            })
            .catch(function () {
                setBadge(key, 'down', null);
                setDetail(key, url + ' \u2014 سرویس پاسخ نداد', false);
            });
    }

    function updateSummary() {
        if (!summary) {
            return;
        }
        var down = [];
        var degraded = [];
        Object.keys(results).forEach(function (key) {
            if (results[key] === 'down') {
                down.push(key);
            } else if (results[key] === 'degraded') {
                degraded.push(key);
            }
        });

        if (sessionExpired) {
            summary.className = 'alert alert-danger py-2 mb-3';
            summary.innerHTML = 'نشست مدیریت شما منقضی شده است. لطفاً <a href="/Account/Login">دوباره وارد شوید</a>.';
            return;
        }
        if (down.length > 0) {
            summary.className = 'alert alert-danger py-2 mb-3';
            summary.innerHTML = faDigits(down.length) + ' سرویس دچار مشکل است و باید بررسی شود.';
            return;
        }
        if (degraded.length > 0) {
            summary.className = 'alert alert-warning py-2 mb-3';
            summary.innerHTML = 'همه سرویس‌ها فعال‌اند؛ ' + faDigits(degraded.length) + ' مورد نیازمند توجه است.';
            return;
        }
        summary.className = 'alert alert-success py-2 mb-3';
        summary.innerHTML = '\u2714 همه سرویس‌ها سالم هستند.';
    }

    function refresh() {
        if (inFlight) {
            return;
        }
        inFlight = true;
        sessionExpired = false;
        if (refreshButton) {
            refreshButton.disabled = true;
        }
        if (countdown) {
            countdown.textContent = 'در حال به‌روزرسانی…';
        }
        rows.forEach(setPending);

        var jobs = [loadServerStatus()].concat(
            rows
                .filter(function (li) { return li.getAttribute('data-kind') === 'url'; })
                .map(checkUrl));

        Promise.all(jobs).then(function () {
            updateSummary();
            if (updated) {
                updated.textContent = 'آخرین به‌روزرسانی: ' + new Date().toLocaleTimeString('fa-IR');
            }
            remaining = REFRESH_SECONDS;
        }).finally(function () {
            inFlight = false;
            if (refreshButton) {
                refreshButton.disabled = false;
            }
        });
    }

    if (refreshButton) {
        refreshButton.addEventListener('click', refresh);
    }

    window.setInterval(function () {
        if (inFlight) {
            return;
        }
        remaining -= 1;
        if (remaining <= 0) {
            refresh();
            return;
        }
        if (countdown) {
            countdown.textContent = 'به‌روزرسانی خودکار تا ' + faDigits(remaining) + ' ثانیه';
        }
    }, 1000);

    refresh();
})();
