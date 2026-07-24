<template>
  <v-navigation-drawer v-if="authStore.isAuthenticated" permanent expand-on-hover rail>
    <v-list>
      <v-list-item title="NPI Project Management"
                   subtitle="NPI"
                   prepend-icon="mdi-clipboard-text-outline" />
    </v-list>

    <v-divider />

    <v-list nav>
      <v-list-item to="/" title="Dashboard" prepend-icon="mdi-view-dashboard" />
      <v-list-item to="/enquiries" title="Enquiries" prepend-icon="mdi-clipboard-list" />
      <v-list-item to="/projects" title="Projects" prepend-icon="mdi-folder-multiple" />
      <v-list-item to="/tasks" title="Tasks" prepend-icon="mdi-check-circle" />
      <v-list-item to="/files" title="Files" prepend-icon="mdi-file-document" />
      <v-list-item to="/settings" title="Settings" prepend-icon="mdi-cog" />
    </v-list>

    <template #append>
      <v-menu v-model="notifMenu" :close-on-content-click="false" location="end">
        <template #activator="{ props }">
          <v-list-item v-bind="props" title="Notifications">
            <template #prepend>
              <v-badge :model-value="unreadCount > 0" :content="unreadCount" color="error">
                <v-icon>mdi-bell</v-icon>
              </v-badge>
            </template>
          </v-list-item>
        </template>

        <v-card min-width="340" max-width="400">
          <v-card-title class="d-flex align-center justify-space-between py-2 px-4">
            <span class="text-subtitle-2">Notifications</span>
            <v-btn v-if="unreadCount > 0" variant="text" size="small" @click="markAll">
              Mark all read
            </v-btn>
          </v-card-title>
          <v-divider />
          <v-list density="compact" style="max-height: 420px; overflow-y: auto;">
            <v-list-item v-if="notifications.length === 0">
              <v-list-item-title class="text-caption text-grey">No new notifications</v-list-item-title>
            </v-list-item>
            <v-list-item v-for="n in notifications"
                         :key="n.notif_id"
                         :class="{ 'bg-blue-lighten-5': !n.is_read }"
                         @click="openNotif(n)">
              <template #prepend>
                <v-icon :color="NOTIFICATION_COLORS[n.type] ?? 'grey'" size="small">
                  {{ NOTIFICATION_ICONS[n.type] ?? 'mdi-bell' }}
                </v-icon>
              </template>
              <v-list-item-title class="text-body-2">{{ n.title }}</v-list-item-title>
              <v-list-item-subtitle class="text-caption">
                {{ formatTimeAgo(n.created_at) }}
              </v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-card>
      </v-menu>

      <v-divider />

      <v-list>
        <v-list-item :title="authStore.currentUser?.full_name"
                     :subtitle="authStore.userDepartment"
                     prepend-icon="mdi-account-circle" />
        <v-list-item title="Logout" prepend-icon="mdi-logout" @click="handleLogout" />
      </v-list>
    </template>
  </v-navigation-drawer>
</template>

<script setup>
  import { ref, watch, onMounted, onBeforeUnmount } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/auth'
  import { api } from '@/utils/api'
  import { resolveNotificationRoute } from '@/utils/notifications'
  import { NOTIFICATION_COLORS, NOTIFICATION_ICONS } from '@/utils/constants'
  import { formatTimeAgo } from '@/utils/formatters'
  import * as signalR from '@microsoft/signalr'

  const router = useRouter()
  const authStore = useAuthStore()

  const notifMenu = ref(false)
  const notifications = ref([])
  const unreadCount = ref(0)

  let pollInterval = null
  let hubConnection = null

  async function loadNotifications() {
    if (!authStore.isAuthenticated) return
    try {
      const result = await api.get('/notification')
      notifications.value = result?.data ?? []
      unreadCount.value = notifications.value.filter(n => !n.is_read).length
    } catch { /* ignore polling errors */ }
  }

  async function markAll() {
    try {
      await api.post('/notification/mark-all-read', {})
      notifications.value.forEach(n => { n.is_read = true })
      unreadCount.value = 0
    } catch { /* ignore */ }
  }

  async function openNotif(notification) {
    if (!notification.is_read) {
      try {
        await api.patch(`/notification/${notification.notif_id}/read`, {})
        notification.is_read = true
        unreadCount.value = Math.max(0, unreadCount.value - 1)
      } catch { /* ignore */ }
    }
    notifMenu.value = false

    const target = resolveNotificationRoute(notification)
    if (target) router.push(target)
  }

  async function connectSignalR() {
    if (!authStore.token) return

    if (hubConnection) {
      await hubConnection.stop().catch(() => { })
      hubConnection = null
    }

    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => authStore.token,
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (ctx) =>
          Math.min(2 ** ctx.previousRetryCount * 1000, 30000),
      })
      .configureLogging(
        import.meta.env.DEV ? signalR.LogLevel.Information : signalR.LogLevel.Warning
      )
      .build()

    hubConnection.on('NewNotification', payload => {
      unreadCount.value += 1
      notifications.value.unshift({
        notif_id: payload.notif_id ?? Date.now(),
        type: payload.type ?? 'info',
        title: payload.title ?? 'New notification',
        body: payload.body ?? '',
        is_read: false,
        proj_id: payload.proj_id ?? null,
        task_id: payload.task_id ?? null,
        enquiry_id: payload.enquiry_id ?? null,
        created_at: payload.created_at ?? new Date().toISOString(),
      })
      if (notifications.value.length > 50) {
        notifications.value = notifications.value.slice(0, 50)
      }
    })

    hubConnection.onreconnected(() => loadNotifications())

    hubConnection.onclose(() => {
      if (!pollInterval) pollInterval = setInterval(loadNotifications, 30_000)
    })

    try {
      await hubConnection.start()
      if (pollInterval) { clearInterval(pollInterval); pollInterval = null }
    } catch (err) {
      console.warn('[SignalR] Initial connection failed — relying on polling:', err?.message ?? err)
    }
  }

  async function joinProjectGroup(projectId) {
    if (hubConnection?.state === signalR.HubConnectionState.Connected) {
      await hubConnection.invoke('JoinProjectGroup', projectId).catch(() => { })
    }
  }

  defineExpose({ joinProjectGroup })

  async function handleLogout() {
    if (hubConnection) {
      await hubConnection.stop().catch(() => { })
      hubConnection = null
    }
    await authStore.logout()
    router.push('/login')
  }

  watch(
    () => authStore.currentUser?.user_id,
    async (newId, oldId) => {
      if (newId === oldId) return

      if (!newId) {
        if (hubConnection) {
          await hubConnection.stop().catch(() => { })
          hubConnection = null
        }
        notifications.value = []
        unreadCount.value = 0
        return
      }

      await loadNotifications()
      await connectSignalR()
    }
  )

  onMounted(async () => {
    if (authStore.token) await authStore.checkAuth()
    await loadNotifications()
    await connectSignalR()
    pollInterval = setInterval(loadNotifications, 60 * 1000)
  })

  onBeforeUnmount(async () => {
    if (pollInterval) clearInterval(pollInterval)
    if (hubConnection) {
      await hubConnection.stop().catch(() => { })
      hubConnection = null
    }
  })
</script>
