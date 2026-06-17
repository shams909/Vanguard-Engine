/**
 * notification.js
 * Handles real-time notifications via SignalR with polling fallback.
 */
(function () {
    'use strict';

    const userId = document.querySelector('meta[name="user-id"]')?.content;
    if (!userId) return; // Not logged in – nothing to do

    const badge = document.getElementById('notif-badge');
    const list  = document.getElementById('notif-list');
    const bell  = document.getElementById('notif-bell');
    const dropdown = document.getElementById('notif-dropdown');

    let unreadCount = 0;

    /* ─── helpers ─────────────────────────────────────────── */
    function updateBadge(n) {
        unreadCount = n;
        if (!badge) return;
        badge.textContent = n > 0 ? (n > 99 ? '99+' : n) : '';
        badge.style.display = n > 0 ? 'flex' : 'none';
    }

    function prependNotification(notif) {
        if (!list) return;
        const empty = list.querySelector('.notif-empty');
        if (empty) empty.remove();

        const el = document.createElement('div');
        el.className = 'notif-item notif-unread';
        el.dataset.id = notif.id ?? notif.Id ?? '';
        const typeClass = (notif.type ?? notif.Type ?? 'Info').toLowerCase();
        el.innerHTML = `
            <span class="notif-type-dot notif-type-${typeClass}"></span>
            <div class="notif-body">
                <p class="notif-title">${escHtml(notif.title ?? notif.Title ?? 'Notification')}</p>
                <p class="notif-msg">${escHtml(notif.message ?? notif.Message ?? '')}</p>
                <p class="notif-time">${timeAgo(notif.createdAt ?? notif.CreatedAt)}</p>
            </div>`;
        el.addEventListener('click', () => markRead(el));
        list.prepend(el);
    }

    function markRead(el) {
        const id = el?.dataset?.id;
        if (!id) return;
        el.classList.remove('notif-unread');
        updateBadge(Math.max(0, unreadCount - 1));
        fetch(`/api/notifications/${id}/read`, { method: 'PATCH' }).catch(() => {});
    }

    function escHtml(s) {
        return String(s ?? '')
            .replace(/&/g,'&amp;').replace(/</g,'&lt;')
            .replace(/>/g,'&gt;').replace(/"/g,'&quot;');
    }

    function timeAgo(dt) {
        if (!dt) return '';
        const diff = Date.now() - new Date(dt).getTime();
        const m = Math.floor(diff / 60000);
        if (m < 1)  return 'just now';
        if (m < 60) return `${m}m ago`;
        const h = Math.floor(m / 60);
        if (h < 24) return `${h}h ago`;
        return `${Math.floor(h / 24)}d ago`;
    }

    /* ─── load initial notifications ──────────────────────── */
    async function loadInitial() {
        try {
            const res  = await fetch(`/api/notifications?userId=${encodeURIComponent(userId)}`);
            if (!res.ok) return;
            const data = await res.json();
            const items   = Array.isArray(data) ? data : (data.notifications ?? data.items ?? []);
            const unread  = typeof data.unreadCount === 'number' ? data.unreadCount : items.filter(n => !(n.isRead ?? n.IsRead)).length;
            updateBadge(unread);
            if (list) {
                list.innerHTML = items.length === 0
                    ? '<div class="notif-empty">No notifications yet</div>'
                    : '';
                items.forEach(n => prependNotification(n));
            }
        } catch (_) {}
    }

    /* ─── polling fallback ────────────────────────────────── */
    let pollingTimer = null;
    function startPolling() {
        if (pollingTimer) return;
        pollingTimer = setInterval(async () => {
            try {
                const res = await fetch(`/api/notifications/unread-count?userId=${encodeURIComponent(userId)}`);
                if (res.ok) {
                    const data = await res.json();
                    const n = typeof data === 'number' ? data : (data.count ?? 0);
                    updateBadge(n);
                }
            } catch (_) {}
        }, 30000); // every 30 seconds
    }

    /* ─── SignalR ─────────────────────────────────────────── */
    function startSignalR() {
        if (typeof signalR === 'undefined') {
            console.warn('[Notifications] SignalR not loaded – using polling only.');
            startPolling();
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/notificationHub')
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveNotification', (notif) => {
            prependNotification(notif);
            updateBadge(unreadCount + 1);
        });

        connection.start()
            .then(() => connection.invoke('Subscribe', userId))
            .catch(err => {
                console.warn('[Notifications] SignalR failed, falling back to polling.', err);
                startPolling();
            });

        connection.onclose(() => startPolling());
    }

    /* ─── bell toggle ─────────────────────────────────────── */
    if (bell && dropdown) {
        bell.addEventListener('click', (e) => {
            e.stopPropagation();
            const open = dropdown.classList.toggle('notif-open');
            if (open) {
                // Mark visible unread items as read
                dropdown.querySelectorAll('.notif-unread').forEach(el => markRead(el));
            }
        });
        document.addEventListener('click', () => dropdown.classList.remove('notif-open'));
        dropdown.addEventListener('click', e => e.stopPropagation());
    }

    /* ─── init ────────────────────────────────────────────── */
    loadInitial();
    startSignalR();
})();
