<template>
  <v-navigation-drawer v-if="authStore.isAuthenticated"
                       permanent
                       expand-on-hover
                       rail>
    <v-list>
      <v-list-item title="NPI Project Management"
                   subtitle="NPI"
                   prepend-icon="mdi-clipboard-text-outline" />
    </v-list>

    <v-divider></v-divider>

    <v-list nav>
      <v-list-item to="/" title="Dashboard" prepend-icon="mdi-view-dashboard" />
      <v-list-item to="/enquiries" title="Enquiries" prepend-icon="mdi-clipboard-list" />
      <v-list-item to="/projects" title="Projects" prepend-icon="mdi-folder-multiple" />
      <v-list-item to="/tasks" title="Tasks" prepend-icon="mdi-check-circle" />
      <v-list-item to="/files" title="Files" prepend-icon="mdi-file-document" />
      <v-list-item v-if="authStore.userRole === 'Admin'" to="/settings" title="Settings" prepend-icon="mdi-cog" />
    </v-list>

    <template #append>
      <v-menu v-model="notifMenu" :close-on-content-click="false" location="end">
        <template #activator="{ props }">
          <v-list-item v-bind="props" prepend-icon="mdi-bell" title="Notifications">
            <template #append>
              <v-badge v-if="unreadCount > 0" :content="unreadCount" color="error" />
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
            <v-list-item v-for="n in notifications" :key="n.notif_id"
                         :class="{ 'bg-blue-lighten-5': !n.is_read }"
                         @click="openNotif(n)">
              <template #prepend>
                <v-icon :color="typeColor(n.type)" size="small">{{ typeIcon(n.type) }}</v-icon>
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
                     prepend-icon="mdi-account-circle"
                     @click="$router.push('/profile')" />
        <v-list-item title="Logout"
                     prepend-icon="mdi-logout"
                     @click="handleLogout" />
      </v-list>
    </template>
  </v-navigation-drawer>
</template>

<script setup>
  import { ref, onMounted, onBeforeUnmount } from 'vue'
  import { useRouter } from 'vue-router'
  import { useAuthStore } from '@/stores/auth'
  import { api } from '@/utils/api'

  const router = useRouter()
  const authStore = useAuthStore()

  const notifMenu = ref(false)
  const notifications = ref([])
  const unreadCount = ref(0)

  let pollInterval = null

  async function loadNotifications() {
    if (!authStore.isAuthenticated) return
    try {
      const result = await api.get('/notification')
      notifications.value = result?.data ?? []
      unreadCount.value = notifications.value.filter(n => !n.is_read).length
    } catch { }
  }

  async function markAll() {
    try {
      await api.post('/notification/mark-all-read', {})
      notifications.value.forEach(n => { n.is_read = true })
      unreadCount.value = 0
    } catch (err) {
      console.error('Mark all read failed:', err)
    }
  }

  async function openNotif(notification) {
    if (!notification.is_read) {
      try {
        await api.patch(`/notification/${notification.notif_id}/read`, {})
        notification.is_read = true
        unreadCount.value = Math.max(0, unreadCount.value - 1)
      } catch { }
    }
    notifMenu.value = false
    if (notification.proj_id) {
      router.push(`/projects/${notification.proj_id}/gantt`)
    }
  }

  function typeColor(type) {
    const map = {
      handover: 'blue', stage_complete: 'success', overdue: 'error',
      planning_stuck: 'warning', project_launch: 'primary', fai_complete: 'success',
      date_revised: 'warning', file_milestone: 'info', task_assigned: 'primary',
      approval_stalled: 'error'
    }
    return map[type] ?? 'grey'
  }

  function typeIcon(type) {
    const map = {
      handover: 'mdi-hand-pointing-right', stage_complete: 'mdi-check-all',
      overdue: 'mdi-alert-circle', planning_stuck: 'mdi-clock-alert',
      project_launch: 'mdi-rocket-launch', fai_complete: 'mdi-flask-check',
      date_revised: 'mdi-calendar-edit', file_milestone: 'mdi-file-multiple',
      task_assigned: 'mdi-clipboard-account', approval_stalled: 'mdi-timer-alert'
    }
    return map[type] ?? 'mdi-bell'
  }

  function formatTimeAgo(dateStr) {
    if (!dateStr) return ''
    const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000)
    if (diff < 60) return 'just now'
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`
    if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`
    return `${Math.floor(diff / 86400)}d ago`
  }

  const handleLogout = async () => {
    await authStore.logout()
    router.push('/login')
  }

  onMounted(async () => {
    if (authStore.token) {
      await authStore.checkAuth()
    }
    await loadNotifications()
    pollInterval = setInterval(loadNotifications, 60_000)
  })

  onBeforeUnmount(() => {
    if (pollInterval) clearInterval(pollInterval)
  })
</script>
