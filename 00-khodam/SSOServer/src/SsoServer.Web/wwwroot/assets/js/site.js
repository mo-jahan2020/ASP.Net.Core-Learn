// SsoServer - minimal, dependency-free UI helpers (offline, no CDN).
(function () {
    'use strict';

    // Confirmation for destructive forms (delete/revoke/disable).
    document.addEventListener('submit', function (event) {
        var form = event.target;
        if (form && form.hasAttribute('data-confirm')) {
            var message = form.getAttribute('data-confirm') || 'آیا مطمئن هستید؟';
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        }
    });

    // Copy-to-clipboard for one-time secrets.
    document.addEventListener('click', function (event) {
        var button = event.target.closest('[data-copy]');
        if (!button) {
            return;
        }
        var source = document.querySelector(button.getAttribute('data-copy'));
        if (!source) {
            return;
        }
        var text = source.innerText || source.textContent;
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(function () {
                var original = button.innerText;
                button.innerText = 'کپی شد';
                window.setTimeout(function () { button.innerText = original; }, 1500);
            });
        }
    });

    // "Select all / clear all" toggle for a group of checkboxes.
    // The toggle carries a CSS selector in data-toggle-checks pointing at the group
    // container; only checkboxes that have a name (i.e. are submitted) are toggled,
    // so the toggle itself is never affected.
    document.addEventListener('click', function (event) {
        var toggle = event.target.closest('[data-toggle-checks]');
        if (!toggle) {
            return;
        }
        var scope = document.querySelector(toggle.getAttribute('data-toggle-checks') || '');
        if (!scope) {
            return;
        }
        var boxes = Array.prototype.slice.call(scope.querySelectorAll('input[type="checkbox"][name]'));
        if (boxes.length === 0) {
            return;
        }
        var check = boxes.some(function (box) { return !box.checked; });
        boxes.forEach(function (box) { box.checked = check; });
        toggle.checked = check;
    });

    // ============ مرتب‌سازی گرید با کلیک روی سرستون ============
    // جدول‌های دارای data-sort-url: هر سرستون با data-key با کلیک،
    // صفحه را با sortKey/sortDesc جدید بارگذاری می‌کند (فیلترها و شماره صفحه حفظ می‌شوند).
    document.addEventListener('click', function (event) {
        var th = event.target.closest('th.sortable');
        if (!th) {
            return;
        }
        var table = th.closest('table[data-sort-url]');
        if (!table) {
            return;
        }

        var key = th.getAttribute('data-key');
        var sortUrl = table.getAttribute('data-sort-url');
        if (!key || !sortUrl) {
            return;
        }

        // اگر همان ستون است جهت برعکس شود؛ در غیر این صورت صعودی از همان ستون.
        var currentKey = table.getAttribute('data-sort-key') || '';
        var currentDesc = table.getAttribute('data-sort-desc') === 'true';
        var desc;
        if (key === currentKey) {
            desc = !currentDesc;
        } else if (!currentKey) {
            // اولین کلیک روی هر ستونی وقتی هنوز مرتب‌سازی انتخاب نشده: نزولی
            // (جدیدترین/بزرگترین اول)، مثل رفتار رایج گریدها.
            desc = true;
        } else {
            desc = false;
        }

        var url = sortUrl +
            '?sortKey=' + encodeURIComponent(key) +
            '&sortDesc=' + (desc ? 'true' : 'false');

        var extra = [
            { name: 'page', value: table.getAttribute('data-page') },
            { name: 'eventType', value: table.getAttribute('data-eventtype') },
            { name: 'userName', value: table.getAttribute('data-username') }
        ];
        extra.forEach(function (item) {
            if (item.value) {
                url += '&' + item.name + '=' + encodeURIComponent(item.value);
            }
        });

        window.location.href = url;
    });

    // ============ باز کردن جزئیات نشست با کلیک روی سطر جدول ============
    // جدول‌های دارای سطرهای row-clickable: هر سطر data-detail-url دارد؛
    // کلیک روی سطر (نه روی دکمه/لینک داخل آن) صفحه را با selected=id بارگذاری می‌کند.
    document.addEventListener('click', function (event) {
        var row = event.target.closest('tr.row-clickable');
        if (!row) {
            return;
        }
        // اگر کلیک روی دکمه یا لینک داخل سطر بود، رفتار پیش‌فرض همان حفظ شود.
        if (event.target.closest('a, button, input, select, textarea, form')) {
            return;
        }
        var url = row.getAttribute('data-detail-url');
        if (url) {
            window.location.href = url;
        }
    });
})();
