<template>
  <v-container fluid class="pa-0 dashboard-root d-flex flex-column">

    <!-- KPI strip (Fixed Top) -->
    <div class="kpi-strip px-4 pt-4 pb-2 flex-shrink-0">
      <v-row dense>
        <v-col v-for="kpi in kpis" :key="kpi.title" cols="12" sm="6" md="3">
          <v-card class="kpi-card d-flex flex-column justify-center" variant="tonal" :color="kpi.color">
            <div class="text-caption text-uppercase text-medium-emphasis">{{ kpi.title }}</div>
            <div class="text-h4 font-weight-bold">{{ kpi.value }}</div>
          </v-card>
        </v-col>
      </v-row>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex-grow-1 d-flex align-center justify-center">
      <v-progress-circular indeterminate color="primary" size="64" />
    </div>

    <!-- Dashboard Body utilizing the extracted Components -->
    <div v-else class="dashboard-body px-4 pb-4 flex-grow-1 overflow-hidden d-flex flex-column">
      <v-row dense class="fill-height ma-0">

        <!-- ── LEFT COLUMN ─────────────────────────────────────────────────── -->
        <v-col cols="12" md="4" class="d-flex flex-column pr-md-2" style="gap:12px; height: 100%;">
          <CalendarCard :projects="projects" :my-tasks="myTasks" />
          <OverdueTasksCard :my-tasks="myTasks" />
        </v-col>

        <!-- ── RIGHT COLUMN ────────────────────────────────────────────────── -->
        <v-col cols="12" md="8" class="d-flex flex-column pl-md-2" style="height: 100%;">
          <TimelineCard :projects="projects" :my-tasks="myTasks" @refresh="loadDashboardData" />
        </v-col>

      </v-row>
    </div>

    <v-snackbar v-model="snackbar" :color="snackbarColor">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { api } from '@/utils/api'

  import CalendarCard from '@/components/dashboards/CalendarCard.vue'
  import OverdueTasksCard from '@/components/dashboards/OverdueTasksCard.vue'
  import TimelineCard from '@/components/dashboards/TimelineCard.vue'

  // ── State ─────────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const projects = ref([])
  const myTasks = ref([])
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  // ── KPIs ──────────────────────────────────────────────────────────────────────
  const kpis = computed(() => [
    { title: 'In Progress', value: projects.value.filter(p => p.status === 'In Progress').length, color: 'blue' },
    { title: 'On Hold', value: projects.value.filter(p => p.status === 'On Hold').length, color: 'orange' },
    { title: 'Completed', value: projects.value.filter(p => p.status === 'Completed').length, color: 'green' },
    { title: 'Total Projects', value: projects.value.length, color: 'primary' },
  ])

  // ── Data loading ──────────────────────────────────────────────────────────────
  async function loadDashboardData() {
    loading.value = true
    try {
      const [pr, tr] = await Promise.all([api.get('/project'), api.get('/task/my-tasks')])
      projects.value = pr?.success ? (pr.data ?? []) : (Array.isArray(pr) ? pr : [])
      myTasks.value = tr?.data ?? (Array.isArray(tr) ? tr : [])
    } catch (err) {
      console.error(err)
      snackbarMessage.value = 'Failed to load dashboard data'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      loading.value = false
    }
  }

  onMounted(async () => {
    await loadDashboardData()
  })
</script>

<style scoped>
  .dashboard-root {
    background: #f5f6f8;
    height: 100vh !important;
    overflow: hidden !important;
  }

  .kpi-card {
    height: 88px;
    border-radius: 12px;
    padding: 14px 16px;
  }
</style>
