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

        // Surgical placement: align floating command center directly beneath the notification bell button
        if (bell && window.innerWidth > 480) {
            const rect = bell.getBoundingClientRect();
            const sbWidth = Math.min(440, window.innerWidth - 32);
            let leftPos = rect.left + (rect.width / 2) - (sbWidth / 2);
            
            // Ensure card does not breach right or left window edges
            if (leftPos + sbWidth > window.innerWidth - 20) {
                leftPos = window.innerWidth - sbWidth - 20;
            }
            leftPos = Math.max(20, leftPos);
            
            sidebar.style.top = (rect.bottom + 14) + 'px';
            sidebar.style.left = leftPos + 'px';
            sidebar.style.right = 'auto';
            
            // Lock transform origin directly under the horizontal center of the bell icon!
            const originX = (rect.left + (rect.width / 2)) - leftPos;
            sidebar.style.transformOrigin = `${originX}px top`;
        } else {
            sidebar.style.left = '';
            sidebar.style.right = '';
            sidebar.style.top = '';
            sidebar.style.transformOrigin = 'top center';
        }

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
            <input type="checkbox" class="notif-checkbox" />
            <div class="notif-icon-wrap notif-icon-${type}">${icon}</div>
            <div class="notif-body">
                <p class="notif-title">${escHtml(notif.title ?? notif.Title ?? 'Notification')}</p>
                <p class="notif-msg">${escHtml(notif.message ?? notif.Message ?? '')}</p>
                <p class="notif-time">⏱ ${timeAgo(notif.createdAt ?? notif.CreatedAt)}</p>
            </div>
            <div style="position:relative; margin-left:auto;">
                <button class="notif-more-btn" aria-label="More options">
                    <i data-lucide="more-vertical" style="width:16px;height:16px;"></i>
                </button>
                <div class="notif-dropdown">
                    <button class="notif-dropdown-item notif-mark-read-btn">Mark as read</button>
                    <button class="notif-dropdown-item danger notif-del-btn">Delete</button>
                </div>
            </div>`;

        const moreBtn = el.querySelector('.notif-more-btn');
        const dropdown = el.querySelector('.notif-dropdown');
        const checkbox = el.querySelector('.notif-checkbox');

        // Toggle dropdown
        moreBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            document.querySelectorAll('.notif-dropdown.show').forEach(d => {
                if (d !== dropdown) d.classList.remove('show');
            });
            dropdown.classList.toggle('show');
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!moreBtn.contains(e.target) && !dropdown.contains(e.target)) {
                dropdown.classList.remove('show');
            }
        });

        // Prevent checkbox click from triggering markRead
        checkbox.addEventListener('click', (e) => e.stopPropagation());

        // Actions
        el.querySelector('.notif-mark-read-btn').addEventListener('click', (e) => {
            e.stopPropagation();
            dropdown.classList.remove('show');
            markRead(el);
        });

        el.querySelector('.notif-del-btn').addEventListener('click', (e) => {
            e.stopPropagation();
            dropdown.classList.remove('show');
            deleteNotif(el);
        });

        el.addEventListener('click', () => markRead(el));
        
        return el;
    }

    function prependNotification(notif) {
        if (!list) return;
        const empty = list.querySelector('.notif-empty');
        if (empty) empty.remove();
        list.prepend(buildItem(notif));
        if (window.lucide) lucide.createIcons();
    }

    function markRead(el, updateBadgeCount = true) {
        if (!el || !el.classList.contains('notif-unread')) return;
        el.classList.remove('notif-unread');
        if (updateBadgeCount) updateBadge(unreadCount - 1);
        const id = el.dataset?.id;
        if (id) fetch(`/api/notifications/${id}/read`, { method: 'PATCH' }).catch(() => {});
    }

    function deleteNotif(el) {
        if (!el) return;
        const id = el.dataset?.id;
        if (el.classList.contains('notif-unread')) {
            updateBadge(unreadCount - 1);
        }
        
        // Add animation class
        el.classList.add('removing');
        
        setTimeout(() => {
            el.remove();
            if (list && list.querySelectorAll('.notif-item').length === 0) {
                list.innerHTML = `
                    <div class="notif-empty">
                        <div class="notif-empty-icon">🔕</div>
                        <div class="notif-empty-text">All caught up!</div>
                        <div class="notif-empty-sub">No notifications yet.</div>
                    </div>`;
            }
        }, 300); // Matches CSS animation duration

        if (id) fetch(`/api/notifications/${id}`, { method: 'DELETE' }).catch(() => {});
    }

    const clearAllBtn = document.getElementById('notif-clear-all');
    if (clearAllBtn) {
        clearAllBtn.addEventListener('click', () => {
            const items = list?.querySelectorAll('.notif-item');
            if (!items || items.length === 0) return;
            
            // Get selected ones
            const checkedBoxes = Array.from(list.querySelectorAll('.notif-checkbox:checked'));
            const selectedItems = checkedBoxes.map(cb => cb.closest('.notif-item'));
            
            if (selectedItems.length === 0) {
                // If nothing selected, maybe warn them? We will just do nothing.
                alert("Please select at least one notification to delete.");
                return;
            }
            
            const ids = selectedItems.map(el => el.dataset.id).filter(id => id);
            
            selectedItems.forEach(el => {
                if (el.classList.contains('notif-unread')) {
                    updateBadge(unreadCount - 1);
                }
                el.classList.add('removing');
            });
            
            setTimeout(() => {
                selectedItems.forEach(el => el.remove());
                
                if (list.querySelectorAll('.notif-item').length === 0) {
                    list.innerHTML = `
                        <div class="notif-empty">
                            <div class="notif-empty-icon">🔕</div>
                            <div class="notif-empty-text">All caught up!</div>
                            <div class="notif-empty-sub">No notifications yet.</div>
                        </div>`;
                }
            }, 300);
            
            if (ids.length > 0) {
                fetch(`/api/notifications/bulk-delete`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(ids)
                }).catch(() => {});
            }
        });
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
                if (window.lucide) lucide.createIcons();
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
