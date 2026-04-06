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

    <!-- Dashboard Body (Expands to fill remaining height) -->
    <div v-else class="dashboard-body px-4 pb-4 flex-grow-1 overflow-hidden d-flex flex-column">
      <v-row dense class="fill-height ma-0">

        <!-- ── LEFT COLUMN ─────────────────────────────────────────────────── -->
        <v-col cols="12" md="4" class="d-flex flex-column pr-md-2" style="gap:12px; height: 100%;">

          <!-- Calendar card (Flex-grow to fill available column space) -->
          <v-card class="d-flex flex-column flex-grow-1 overflow-hidden">
            <v-card-title class="section-title d-flex align-center pa-3 pb-2 flex-shrink-0">
              <v-icon class="mr-2" size="18">mdi-calendar</v-icon>
              Project Calendar
            </v-card-title>

            <v-divider class="flex-shrink-0" />

            <!-- Toolbar -->
            <v-sheet height="56" class="flex-shrink-0">
              <v-toolbar flat density="compact">
                <v-btn class="me-2" size="small" variant="outlined" @click="setToday">
                  Today
                </v-btn>
                <v-btn icon size="small" variant="text" @click="prev">
                  <v-icon size="18">mdi-chevron-left</v-icon>
                </v-btn>
                <v-btn icon size="small" variant="text" @click="next">
                  <v-icon size="18">mdi-chevron-right</v-icon>
                </v-btn>
                <v-toolbar-title v-if="calendarRef">
                  {{ calendarTitle }}
                </v-toolbar-title>
              </v-toolbar>
            </v-sheet>

            <!-- Calendar wrapper scales internally -->
            <v-sheet class="flex-grow-1 overflow-y-auto position-relative">
              <v-calendar ref="calendarRef"
                          v-model="calendarFocus"
                          :events="calendarEvents"
                          :type="calendarType"
                          :event-color="getEventColor"
                          color="primary"
                          @click:event="showEvent" />
            </v-sheet>

            <!-- Tooltip Menu -->
            <v-menu v-model="selectedOpen"
                    :activator="selectedElement"
                    :close-on-content-click="false"
                    location="end">
              <v-card min-width="320" flat>
                <v-toolbar :color="selectedEvent.color" density="compact">
                  <v-toolbar-title>{{ selectedEvent.name }}</v-toolbar-title>
                </v-toolbar>
                <v-card-text class="text-body-2">
                  <div><strong>Status:</strong> {{ selectedEvent.status }}</div>
                  <div><strong>Progress:</strong> {{ selectedEvent.progress }}%</div>
                  <div>
                    <strong>Duration:</strong>
                    {{ formatDate(selectedEvent.start) }} → {{ formatDate(selectedEvent.end) }}
                  </div>
                </v-card-text>
                <v-card-actions>
                  <v-spacer />
                  <v-btn size="small" variant="text" @click="goToProject(selectedEvent.proj_id)">Open</v-btn>
                  <v-btn size="small" variant="text" @click="selectedOpen = false">Close</v-btn>
                </v-card-actions>
              </v-card>
            </v-menu>
          </v-card>

          <!-- Overdue tasks card (Fixed max height, internal scroll) -->
          <v-card class="d-flex flex-column flex-shrink-0" style="max-height: 30vh;">
            <v-card-title class="section-title d-flex align-center justify-space-between pa-3 pb-2 flex-shrink-0">
              <div class="d-flex align-center">
                <v-icon class="mr-2" size="18">mdi-alert-circle-outline</v-icon>
                Overdue Tasks
              </div>
              <v-chip v-if="overdueTasks.length" size="small" color="error">
                {{ overdueTasks.length }}
              </v-chip>
            </v-card-title>
            <v-divider class="flex-shrink-0" />

            <v-list v-if="overdueTasks.length" density="compact" class="flex-grow-1 overflow-y-auto pa-0">
              <v-list-item v-for="t in overdueTasks"
                           :key="t.task_id"
                           @click="goToProject(t.proj_id)"
                           style="cursor:pointer">
                <v-list-item-title class="text-body-2">{{ t.title }}</v-list-item-title>
                <v-list-item-subtitle class="text-caption">
                  {{ t.proj_name }} · Due {{ formatDate(t.planned_end_date) }}
                </v-list-item-subtitle>
                <template #append>
                  <v-chip size="small" color="error" variant="tonal">{{ t.days_overdue }}d</v-chip>
                </template>
              </v-list-item>
            </v-list>

            <div v-else class="flex-grow-1 d-flex flex-column align-center justify-center text-medium-emphasis py-4">
              <v-icon size="40">mdi-check-circle-outline</v-icon>
              <div class="mt-2 text-caption">No overdue tasks</div>
            </div>
          </v-card>

        </v-col>

        <!-- ── RIGHT COLUMN — Timeline ──────────────────────────────────────── -->
        <v-col cols="12" md="8" class="d-flex flex-column pl-md-2" style="height: 100%;">
          <v-card class="d-flex flex-column fill-height overflow-hidden">

            <v-card-title class="section-title d-flex align-center justify-space-between pa-3 pb-2 flex-shrink-0">
              <div class="d-flex align-center">
                <v-icon class="mr-2" size="18">mdi-timeline-outline</v-icon>
                Projects Timeline
              </div>
              <v-btn size="small" variant="outlined" @click="loadDashboardData">
                <v-icon start>mdi-refresh</v-icon>Refresh
              </v-btn>
            </v-card-title>
            <v-divider class="flex-shrink-0" />

            <!-- Timeline dynamically fits remaining space -->
            <div class="timeline-wrapper flex-grow-1 d-flex flex-column position-relative" ref="timelineWrapper">

              <v-data-table v-if="projectTimeline.length"
                            :headers="timelineHeaders"
                            :items="projectTimeline"
                            item-value="proj_id"
                            class="timeline-table flex-grow-1"
                            density="compact"
                            fixed-header
                            height="100%"
                            hide-default-footer
                            :items-per-page="-1"
                            @click:row="(_, row) => goToProject(row.item.proj_id)">

                <template #item.proj_name="{ item }">
                  <div class="d-flex flex-column py-1">
                    <span class="text-body-2 font-weight-medium text-truncate">
                      {{ item.proj_name }}
                    </span>
                    <div class="d-flex align-center mt-1" style="gap:4px">
                      <v-chip :color="statusColor(item.status)" size="x-small" variant="tonal">
                        {{ item.status }}
                      </v-chip>
                      <v-chip v-if="item.priority" :color="priorityColor(item.priority)" size="x-small" variant="tonal">
                        {{ item.priority }}
                      </v-chip>
                    </div>
                  </div>
                </template>

                <template #item.progress="{ item }">
                  <div class="d-flex align-center" style="gap:6px; min-width:90px">
                    <v-progress-linear :model-value="item.progress"
                                       :color="progressColor(item.progress)"
                                       height="8"
                                       rounded
                                       style="width:52px; flex-shrink:0" />
                    <span class="text-caption" style="white-space:nowrap">{{ item.progress }}%</span>
                  </div>
                </template>

                <template v-for="col in timelineWeeks" :key="col.value" #[`item.${col.value}`]="{}">
                  <div class="tl-empty-cell" :class="{ 'is-today': col.isToday }" />
                </template>

              </v-data-table>

              <div v-if="!projectTimeline.length" class="flex-grow-1 d-flex flex-column align-center justify-center text-medium-emphasis">
                <v-icon size="56">mdi-folder-open-outline</v-icon>
                <div class="mt-2">No projects available</div>
              </div>

              <!-- Bar overlay -->
              <div class="tl-bar-overlay" ref="tlBarOverlay" :style="tlOverlayStyle">
                <template v-for="bar in tlBarLayout" :key="bar.proj_id">
                  <v-tooltip location="top">
                    <template #activator="{ props }">
                      <div v-bind="props" :class="bar.cssClass" :style="bar.style" @click.stop="goToProject(bar.proj_id)">
                        <span v-if="bar.label" class="tl-bar-label">{{ bar.label }}</span>
                      </div>
                    </template>
                    <div style="font-size:12px; line-height:1.6">
                      <strong>{{ bar.proj_name }}</strong><br>
                      {{ bar.status }} · {{ bar.progress }}%<br>
                      {{ formatDate(bar.start) }} → {{ formatDate(bar.end) }}
                    </div>
                  </v-tooltip>
                </template>
                <div class="tl-today-line" :style="tlTodayLineStyle" />
              </div>

            </div>
          </v-card>
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
  // [SCRIPT CONTENT REMAINS EXACTLY THE SAME]
  // (Ensure all your previous JS is retained here)
  import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'
  import { useRouter } from 'vue-router'
  import { api } from '@/utils/api'

  const router = useRouter()

  const loading = ref(false)
  const projects = ref([])
  const myTasks = ref([])
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const calendarRef = ref(null)
  const calendarFocus = ref('')
  const calendarType = ref('month')
  const selectedEvent = ref({})
  const selectedOpen = ref(false)
  const selectedElement = ref(null)

  const timelineWrapper = ref(null)
  const tlBarOverlay = ref(null)
  const tlOverlayLeft = ref(0)
  const tlOverlayWidth = ref(0)
  const tlRowHeight = ref(52)
  const tlHeaderHeight = ref(36)
  const TL_FIXED_COLS = 2

  const goToProject = id => router.push(`/projects/${id}/gantt`)

  function formatDate(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }
  function toMs(d) { return d ? new Date(d).getTime() : NaN }

  function statusColor(s) { return { Completed: 'success', 'In Progress': 'primary', 'On Hold': 'warning', Planning: 'grey', Cancelled: 'error' }[s] ?? 'grey' }
  function priorityColor(p) { return { Low: 'grey', Medium: 'blue', High: 'orange', Critical: 'red' }[p] ?? 'grey' }
  function progressColor(v) { if (v >= 100) return 'success'; if (v >= 50) return 'primary'; if (v > 0) return 'warning'; return 'grey' }
  function statusHex(s) { return { Completed: '#4CAF50', 'In Progress': '#1976D2', 'On Hold': '#FF9800', Planning: '#9E9E9E', Cancelled: '#F44336' }[s] ?? '#9E9E9E' }

  const kpis = computed(() => [
    { title: 'In Progress', value: projects.value.filter(p => p.status === 'In Progress').length, color: 'blue' },
    { title: 'On Hold', value: projects.value.filter(p => p.status === 'On Hold').length, color: 'orange' },
    { title: 'Completed', value: projects.value.filter(p => p.status === 'Completed').length, color: 'green' },
    { title: 'Total Projects', value: projects.value.length, color: 'primary' },
  ])

  const overdueTasks = computed(() => {
    const today = new Date(); today.setHours(0, 0, 0, 0)
    return myTasks.value
      .filter(t => {
        if (['Completed', 'Cancelled'].includes(t.status)) return false
        if (!t.planned_end_date) return false
        const end = new Date(t.planned_end_date); end.setHours(0, 0, 0, 0)
        return end < today
      })
      .map(t => {
        const end = new Date(t.planned_end_date); end.setHours(0, 0, 0, 0)
        return { ...t, days_overdue: Math.ceil((today - end) / 86400000) }
      })
      .sort((a, b) => b.days_overdue - a.days_overdue)
      .slice(0, 15)
  })

  const calendarTitle = computed(() => calendarRef.value?.title ?? '')
  const calendarEvents = computed(() =>
    projectTimeline.value.filter(p => p.project_start_date && p.target_completion_date).map(p => ({
      name: p.proj_name, start: new Date(p.project_start_date), end: new Date(p.target_completion_date),
      color: statusHex(p.status), proj_id: p.proj_id, status: p.status, progress: p.progress, timed: false,
    }))
  )

  function getEventColor(event) { return event.color }
  function setToday() { calendarFocus.value = '' }
  function prev() { calendarRef.value?.prev() }
  function next() { calendarRef.value?.next() }

  function showEvent(nativeEvent, { event }) {
    const open = () => {
      selectedEvent.value = event
      selectedElement.value = nativeEvent.target
      requestAnimationFrame(() => requestAnimationFrame(() => (selectedOpen.value = true)))
    }
    if (selectedOpen.value) {
      selectedOpen.value = false
      requestAnimationFrame(() => requestAnimationFrame(open))
    } else open()
    nativeEvent.stopPropagation()
  }

  const tlFirstMonday = computed(() => {
    const d = new Date(); d.setHours(0, 0, 0, 0)
    d.setDate(d.getDate() - ((d.getDay() + 6) % 7) - 28)
    return d
  })

  const timelineWeeks = computed(() => {
    const today = new Date(); today.setHours(0, 0, 0, 0)
    return Array.from({ length: 24 }, (_, i) => {
      const ws = new Date(tlFirstMonday.value); ws.setDate(ws.getDate() + i * 7)
      const we = new Date(ws); we.setDate(we.getDate() + 6)
      return {
        title: ws.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }),
        value: `tlw_${i}`, width: '72px', align: 'center', sortable: false,
        date: ws, endDate: we, isToday: today >= ws && today <= we,
      }
    })
  })

  const timelineHeaders = computed(() => [
    { title: 'Project', value: 'proj_name', width: '220px', sortable: false, fixed: true },
    { title: 'Progress', value: 'progress', width: '110px', sortable: false, fixed: true },
    ...timelineWeeks.value,
  ])

  const tlStart = computed(() => tlFirstMonday.value)
  const tlEnd = computed(() => { const e = new Date(tlFirstMonday.value); e.setDate(e.getDate() + 24 * 7); return e })
  const tlMs = computed(() => tlEnd.value - tlStart.value)

  const projectTimeline = computed(() =>
    projects.value.map(p => {
      const pt = myTasks.value.filter(t => t.proj_id === p.proj_id)
      const progress = pt.length ? Math.round(pt.reduce((s, t) => s + (t.per_complete || 0), 0) / pt.length) : 0
      return { ...p, progress }
    })
  )

  function measureTlOverlay() {
    nextTick(() => {
      const wrapper = timelineWrapper.value
      if (!wrapper) return
      const table = wrapper.querySelector('table')
      if (!table) return

      const allTh = Array.from(table.querySelectorAll('thead tr th'))
      if (allTh.length <= TL_FIXED_COLS) return

      const scrollable = wrapper.querySelector('.v-table__wrapper')
      const wrapperRect = wrapper.getBoundingClientRect()
      const firstDateTh = allTh[TL_FIXED_COLS]
      const lastDateTh = allTh[allTh.length - 1]

      tlOverlayLeft.value = firstDateTh.getBoundingClientRect().left - wrapperRect.left + (scrollable?.scrollLeft ?? 0)
      tlOverlayWidth.value = lastDateTh.getBoundingClientRect().right - firstDateTh.getBoundingClientRect().left

      const firstTr = table.querySelector('tbody tr')
      if (firstTr) tlRowHeight.value = firstTr.getBoundingClientRect().height || 52

      const headerTr = table.querySelector('thead tr')
      if (headerTr) tlHeaderHeight.value = headerTr.getBoundingClientRect().height || 36
    })
  }

  const tlBarLayout = computed(() => {
    if (!tlOverlayWidth.value || !tlMs.value) return []
    const tsMs = tlStart.value.getTime(), totMs = tlMs.value, barH = Math.max(8, Math.floor(tlRowHeight.value * 0.42))
    return projectTimeline.value.flatMap((p, idx) => {
      if (!p.project_start_date || !p.target_completion_date) return []
      const sMs = toMs(p.project_start_date), eMs = toMs(p.target_completion_date) + 86400000
      if (isNaN(sMs) || isNaN(eMs) || eMs <= sMs) return []

      const leftPct = Math.max(0, (sMs - tsMs) / totMs) * 100
      const widthPct = Math.min(100 - leftPct, ((eMs - sMs) / totMs) * 100)
      if (widthPct <= 0) return []

      const topOffset = idx * tlRowHeight.value + Math.floor(tlRowHeight.value * 0.28)
      const slug = (p.status ?? 'planning').toLowerCase().replace(/\s+/g, '-')

      return [{
        proj_id: p.proj_id, proj_name: p.proj_name, status: p.status, start: p.project_start_date,
        end: p.target_completion_date, progress: p.progress, cssClass: `tl-bar tl-bar-${slug}`,
        label: widthPct > 5 ? p.proj_name : '',
        style: { top: `${topOffset}px`, left: `${leftPct}%`, width: `${widthPct}%`, height: `${barH}px`, '--tl-progress': `${p.progress}%` }
      }]
    })
  })

  const tlOverlayStyle = computed(() => ({
    position: 'absolute', top: `${tlHeaderHeight.value}px`, left: `${tlOverlayLeft.value}px`,
    width: `${tlOverlayWidth.value}px`, pointerEvents: 'none', overflow: 'visible', zIndex: 5,
  }))

  const tlTodayLineStyle = computed(() => {
    if (!tlOverlayWidth.value || !tlMs.value) return { display: 'none' }
    const pct = ((new Date().setHours(0, 0, 0, 0) - tlStart.value.getTime()) / tlMs.value) * 100
    if (pct < 0 || pct > 100) return { display: 'none' }
    return { display: 'block', left: `${pct}%`, top: '0', height: '100%' }
  })

  function attachScrollSync() {
    nextTick(() => {
      const tw = timelineWrapper.value?.querySelector('.v-table__wrapper')
      const ov = tlBarOverlay.value
      if (!tw || !ov) return
      tw.addEventListener('scroll', () => {
        ov.style.left = `${tlOverlayLeft.value - tw.scrollLeft}px`
        ov.style.top = `${tlHeaderHeight.value - tw.scrollTop}px`
      })
    })
  }

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

  watch(projectTimeline, () => nextTick(measureTlOverlay))
  if (typeof window !== 'undefined') window.addEventListener('resize', measureTlOverlay)

  onMounted(async () => {
    window.addEventListener('resize', measureTlOverlay)
    await loadDashboardData()
    measureTlOverlay()
    attachScrollSync()
  })

  onBeforeUnmount(() => {
    window.removeEventListener('resize', measureTlOverlay)
  })
</script>

<style scoped>
  /* 1. Strict 100vh Root without scrolling */
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

  .section-title {
    font-size: 0.9rem;
    font-weight: 600;
  }

  /* Timeline Layout Cleanup */
  .timeline-table :deep(.v-table__wrapper) {
    overflow-x: auto;
    overflow-y: auto;
    height: 100%;
  }

  .timeline-table :deep(th),
  .timeline-table :deep(td) {
    white-space: nowrap;
    border-right: 1px solid rgba(0, 0, 0, 0.05);
    padding: 0 8px !important;
  }

  .timeline-table :deep(tbody tr) {
    cursor: pointer;
  }

  .timeline-table :deep(tbody tr:hover td) {
    background-color: rgba(0, 0, 0, 0.025) !important;
  }

  /* Sticky Headers for Project/Progress Fixed Columns */
  .timeline-table :deep(th:nth-child(1)),
  .timeline-table :deep(td:nth-child(1)),
  .timeline-table :deep(th:nth-child(2)),
  .timeline-table :deep(td:nth-child(2)) {
    background: #ffffff !important;
    position: sticky;
    z-index: 15;
  }

  .timeline-table :deep(th) {
    z-index: 16;
  }

  .timeline-table :deep(tbody tr td) {
    height: 52px !important;
    vertical-align: middle;
  }

  /* Empty Gantt Track Grid */
  .tl-empty-cell {
    height: 52px;
    width: 100%;
  }

    .tl-empty-cell.is-today {
      background-color: rgba(33, 150, 243, 0.05);
      border-left: 2px solid rgba(33, 150, 243, 0.35);
    }

  /* Overlay & Bars */
  .tl-bar-overlay {
    pointer-events: none;
    position: absolute;
    overflow: visible;
  }

  .tl-bar {
    position: absolute;
    border-radius: 4px;
    display: flex;
    align-items: center;
    overflow: hidden;
    pointer-events: all;
    cursor: pointer;
    transition: filter 0.12s, box-shadow 0.12s;
    min-width: 4px;
  }

    .tl-bar:hover {
      filter: brightness(1.1);
      box-shadow: 0 2px 6px rgba(0, 0, 0, 0.22);
      z-index: 20;
    }

  .tl-bar-planning {
    background: rgba(158, 158, 158, 0.45);
    border: 1px solid rgba(158, 158, 158, 0.75);
  }

  .tl-bar-in-progress {
    background: linear-gradient( to right, rgba(25, 118, 210, 0.9) var(--tl-progress), rgba(25, 118, 210, 0.25) var(--tl-progress) );
    border: 1px solid rgba(25, 118, 210, 1);
  }

  .tl-bar-completed {
    background: rgba(76, 175, 80, 0.85);
    border: 1px solid rgba(76, 175, 80, 1);
  }

  .tl-bar-on-hold {
    background: rgba(255, 152, 0, 0.85);
    border: 1px solid rgba(255, 152, 0, 1);
  }

  .tl-bar-cancelled {
    background: rgba(244, 67, 54, 0.6);
    border: 1px solid rgba(244, 67, 54, 0.9);
  }

  .tl-bar-label {
    font-size: 10px;
    font-weight: 500;
    color: white;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.35);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    padding: 0 5px;
  }

  /* Today vertical line */
  .tl-today-line {
    position: absolute;
    width: 2px;
    background: rgba(33, 150, 243, 0.72);
    pointer-events: none;
    z-index: 10;
  }
</style>
