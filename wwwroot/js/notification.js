/**
 * notification.js
 * Slide-in sidebar notification panel with SignalR + polling fallback.
 */
(function () {
    'use strict';

    const userId = document.querySelector('meta[name="user-id"]')?.content;
    if (!userId) return;

    /* ─── Elements ──────────────────────────────────────────── */
    const bell      = document.getElementById('notif-bell');
    const badge     = document.getElementById('notif-badge');
    const sidebar   = document.getElementById('notif-sidebar');
    const overlay   = document.getElementById('notif-overlay');
    const list      = document.getElementById('notif-list');
    const closeBtn  = document.getElementById('notif-close');
    const markAllBtn= document.getElementById('notif-mark-all');

    let unreadCount = 0;
    let isOpen      = false;

    /* ─── Sidebar open / close ──────────────────────────────── */
    function openSidebar() {
        if (!sidebar || !overlay) return;
        isOpen = true;
        overlay.classList.add('open');
        requestAnimationFrame(() => overlay.classList.add('visible'));
        sidebar.classList.add('open');
        bell?.classList.add('active');
        document.body.style.overflow = 'hidden'; // prevent background scroll
        // Re-init icons inside sidebar (X button)
        if (window.lucide) lucide.createIcons();
    }

    function closeSidebar() {
        if (!sidebar || !overlay) return;
        isOpen = false;
        sidebar.classList.remove('open');
        overlay.classList.remove('visible');
        bell?.classList.remove('active');
        document.body.style.overflow = '';
        setTimeout(() => overlay.classList.remove('open'), 280);
    }

    if (bell)     bell.addEventListener('click', () => isOpen ? closeSidebar() : openSidebar());
    if (closeBtn) closeBtn.addEventListener('click', closeSidebar);
    if (overlay)  overlay.addEventListener('click', closeSidebar);

    // ESC key to close
    document.addEventListener('keydown', e => { if (e.key === 'Escape' && isOpen) closeSidebar(); });

    /* ─── Badge ─────────────────────────────────────────────── */
    function updateBadge(n) {
        unreadCount = Math.max(0, n);
        if (!badge) return;
        badge.textContent = unreadCount > 99 ? '99+' : unreadCount;
        badge.style.display = unreadCount > 0 ? 'flex' : 'none';
    }

    /* ─── Mark all read ─────────────────────────────────────── */
    if (markAllBtn) {
        markAllBtn.addEventListener('click', () => {
            list?.querySelectorAll('.notif-item.notif-unread').forEach(el => markRead(el, false));
            updateBadge(0);
        });
    }

    /* ─── Notification item ─────────────────────────────────── */
    const TYPE_ICONS = {
        info:     '💬',
        warning:  '⚠️',
        critical: '🚨',
    };

    function buildItem(notif) {
        const el = document.createElement('div');
        el.className = 'notif-item notif-unread';
        el.dataset.id = notif.id ?? notif.Id ?? '';

        const raw  = (notif.type ?? notif.Type ?? 'info').toLowerCase();
        const type = ['info','warning','critical'].includes(raw) ? raw : 'info';
        const icon = TYPE_ICONS[type] ?? '💬';

        el.innerHTML = `
            <div class="notif-icon-wrap notif-icon-${type}">${icon}</div>
            <div class="notif-body">
                <p class="notif-title">${escHtml(notif.title ?? notif.Title ?? 'Notification')}</p>
                <p class="notif-msg">${escHtml(notif.message ?? notif.Message ?? '')}</p>
                <p class="notif-time">⏱ ${timeAgo(notif.createdAt ?? notif.CreatedAt)}</p>
            </div>`;

        el.addEventListener('click', () => markRead(el));
        return el;
    }

    function prependNotification(notif) {
        if (!list) return;
        const empty = list.querySelector('.notif-empty');
        if (empty) empty.remove();
        list.prepend(buildItem(notif));
    }

    function markRead(el, updateBadgeCount = true) {
        if (!el || !el.classList.contains('notif-unread')) return;
        el.classList.remove('notif-unread');
        if (updateBadgeCount) updateBadge(unreadCount - 1);
        const id = el.dataset?.id;
        if (id) fetch(`/api/notifications/${id}/read`, { method: 'PATCH' }).catch(() => {});
    }

    /* ─── Helpers ───────────────────────────────────────────── */
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

    /* ─── Load initial ──────────────────────────────────────── */
    async function loadInitial() {
        try {
            const res  = await fetch(`/api/notifications?userId=${encodeURIComponent(userId)}`);
            if (!res.ok) return;
            const data  = await res.json();
            const items  = Array.isArray(data) ? data : (data.notifications ?? []);
            const unread = typeof data.unreadCount === 'number'
                ? data.unreadCount
                : items.filter(n => !(n.isRead ?? n.IsRead)).length;

            updateBadge(unread);

            if (!list) return;
            if (items.length === 0) {
                list.innerHTML = `
                    <div class="notif-empty">
                        <div class="notif-empty-icon">🔕</div>
                        <div class="notif-empty-text">All caught up!</div>
                        <div class="notif-empty-sub">No notifications yet.</div>
                    </div>`;
            } else {
                list.innerHTML = '';
                items.forEach(n => list.appendChild(buildItem(n)));
                // mark already-read ones
                list.querySelectorAll('.notif-item').forEach((el, i) => {
                    const n = items[i];
                    if (n?.isRead ?? n?.IsRead) el.classList.remove('notif-unread');
                });
            }
        } catch (_) {}
    }

    /* ─── Polling fallback ──────────────────────────────────── */
    let pollingTimer = null;
    function startPolling() {
        if (pollingTimer) return;
        pollingTimer = setInterval(async () => {
            try {
                const res = await fetch(`/api/notifications/unread-count?userId=${encodeURIComponent(userId)}`);
                if (res.ok) {
                    const data = await res.json();
                    updateBadge(typeof data === 'number' ? data : (data.count ?? 0));
                }
            } catch (_) {}
        }, 30000);
    }

    /* ─── SignalR ───────────────────────────────────────────── */
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
            // Subtle bell shake
            bell?.animate([
                { transform: 'rotate(-15deg)' }, { transform: 'rotate(15deg)' },
                { transform: 'rotate(-10deg)' }, { transform: 'rotate(10deg)' },
                { transform: 'rotate(0deg)' }
            ], { duration: 500, easing: 'ease-in-out' });
        });

        connection.start()
            .then(() => connection.invoke('Subscribe', userId))
            .catch(err => {
                console.warn('[Notifications] SignalR failed, falling back to polling.', err);
                startPolling();
            });

        connection.onclose(() => startPolling());
    }

    /* ─── Init ──────────────────────────────────────────────── */
    loadInitial();
    startSignalR();
})();
