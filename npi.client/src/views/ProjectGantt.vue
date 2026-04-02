<template>
  <v-container fluid class="pa-0">
    <v-row no-gutters>
      <v-col cols="12">
        <v-card elevation="2">

          <!-- ── Header ──────────────────────────────────────────────────── -->
          <v-card-title class="bg-primary d-flex align-center justify-space-between">
            <div>
              <v-icon class="mr-2">mdi-chart-gantt</v-icon>
              Project Gantt Chart — {{ project?.proj_no }}
            </div>
            <div>
              <v-btn variant="text" color="white" class="mr-2" @click="refreshData">
                <v-icon start>mdi-refresh</v-icon>
                Refresh
              </v-btn>
              <v-btn variant="text" color="white" @click="$router.push('/projects')">
                <v-icon start>mdi-arrow-left</v-icon>
                Back
              </v-btn>
            </div>
          </v-card-title>

          <v-card-text v-if="loading" class="text-center pa-8">
            <v-progress-circular indeterminate color="primary" size="64" />
          </v-card-text>

          <v-card-text v-else class="pa-0">

            <!-- ── Project summary ────────────────────────────────────────── -->
            <v-sheet class="pa-4 bg-grey-lighten-4">
              <v-row align="center">
                <v-col cols="12" md="3">
                  <div class="text-caption text-grey-darken-1">Project Name</div>
                  <div class="text-subtitle-1 font-weight-medium">{{ project?.proj_name }}</div>
                </v-col>
                <v-col cols="12" md="2">
                  <div class="text-caption text-grey-darken-1">Status</div>
                  <v-chip :color="getStatusColor(project?.status)" size="small" variant="tonal">
                    {{ project?.status }}
                  </v-chip>
                </v-col>
                <v-col cols="12" md="2">
                  <div class="text-caption text-grey-darken-1">Priority</div>
                  <v-chip :color="getPriorityColor(project?.priority)" size="small" variant="tonal">
                    {{ project?.priority }}
                  </v-chip>
                </v-col>
                <v-col cols="12" md="2">
                  <div class="text-caption text-grey-darken-1">Start Date</div>
                  <div class="text-subtitle-1">{{ formatDate(project?.project_start_date) }}</div>
                </v-col>
                <v-col cols="12" md="3">
                  <div class="text-caption text-grey-darken-1">Target Completion</div>
                  <div class="text-subtitle-1">{{ formatDate(project?.target_completion_date) }}</div>
                </v-col>
              </v-row>
            </v-sheet>

            <!-- ── NPI Stage pipeline ─────────────────────────────────────── -->
            <v-sheet class="pa-3 bg-white border-b">
              <div class="d-flex align-center ga-1 overflow-x-auto">
                <span class="text-caption text-grey mr-2">Stages:</span>
                <template v-for="(stage, idx) in projectStages" :key="stage.id">
                  <div class="pipeline-chip"
                       :class="{ 'pipeline-chip--active': stage.id === currentStageId }"
                       :style="{ background: stage.bg, borderColor: stage.border, color: stage.textColor }">
                    <span class="font-weight-bold" style="font-size:11px">{{ stage.id }}</span>
                    <span style="font-size:10px; margin-left:4px">{{ stage.shortName }}</span>
                    <v-icon v-if="stage.status === 'completed'" size="x-small" class="ml-1" color="success">mdi-check-circle</v-icon>
                    <v-icon v-else-if="stage.status === 'active'" size="x-small" class="ml-1" color="primary">mdi-play-circle</v-icon>
                  </div>
                  <v-icon v-if="idx < projectStages.length - 1" size="small" color="grey-lighten-1">mdi-arrow-right</v-icon>
                </template>
                <v-spacer />
                <div class="text-caption text-grey">
                  Current stage: <strong>{{ currentStageName }}</strong> ·
                  Done: <strong>{{ completedTasks }}/{{ tasks.length }}</strong> ·
                  Progress: <strong>{{ overallProgress }}%</strong>
                </div>
              </div>
            </v-sheet>

            <!-- ── Controls ──────────────────────────────────────────────── -->
            <v-sheet class="pa-3 border-b">
              <v-row align="center">
                <v-col cols="auto">
                  <v-btn-toggle v-model="viewMode" mandatory variant="outlined" density="compact">
                    <v-btn value="day" size="small">Day</v-btn>
                    <v-btn value="week" size="small">Week</v-btn>
                    <v-btn value="month" size="small">Month</v-btn>
                  </v-btn-toggle>
                </v-col>
                <v-col cols="auto">
                  <v-btn size="small" variant="outlined" @click="scrollToToday">
                    <v-icon start>mdi-calendar-today</v-icon>
                    Today
                  </v-btn>
                </v-col>
                <v-col cols="auto">
                  <v-checkbox v-model="showAllStages" label="Show all stages" hide-details density="compact" />
                </v-col>
              </v-row>
            </v-sheet>

            <!-- ══════════════════════════════════════════════════════════════
                 GANTT TABLE
                 One row per task. Fixed left columns:
                   title | dept | duration | progress | planned dates | actual dates
                 Date header columns are empty — all bars drawn on overlay.
                 Each task row has two stacked bars (planned top, actual bottom).
            ══════════════════════════════════════════════════════════════ -->
            <div class="gantt-wrapper" ref="ganttWrapper">

              <v-data-table ref="ganttTable"
                            :headers="ganttHeaders"
                            :items="displayRows"
                            item-value="rowId"
                            :row-props="getRowProps"
                            class="gantt-table"
                            density="compact"
                            fixed-header
                            height="600"
                            hide-default-footer
                            :items-per-page="-1">

                <!-- Task column -->
                <template #item.title="{ item }">
                  <div v-if="item.rowType === 'stage-header'" class="stage-header-cell d-flex align-center">
                    <v-chip :color="getStageColor(item.stage_id)" size="small" variant="tonal" class="mr-2 font-weight-bold">
                      {{ item.stage_id }}
                    </v-chip>
                    <span class="font-weight-medium">{{ item.stageName }}</span>
                    <v-spacer />
                    <span class="text-caption text-grey mr-2">{{ item.completedCount }}/{{ item.taskCount }} done</span>
                    <v-progress-linear :model-value="item.stageProgress"
                                       :color="getStageColor(item.stage_id)"
                                       height="4" rounded style="max-width:60px" />
                  </div>
                  <div v-else class="d-flex align-center ga-1 py-1">
                    <v-chip v-if="item.task_code"
                            :color="getStageColor(item.stage_id)"
                            size="x-small" variant="tonal" class="font-weight-bold flex-shrink-0">
                      {{ item.task_code }}
                    </v-chip>
                    <span class="text-body-2 font-weight-medium text-truncate">{{ item.title }}</span>
                  </div>
                </template>

                <!-- Department -->
                <template #item.dept_name="{ item }">
                  <v-chip v-if="item.rowType === 'task'" size="x-small" variant="outlined">
                    {{ item.dept_name || 'N/A' }}
                  </v-chip>
                </template>

                <!-- Duration -->
                <template #item.duration="{ item }">
                  <span v-if="item.rowType === 'task'" class="text-body-2">
                    {{ item.duration ?? '—' }} d
                  </span>
                </template>

                <!-- Progress — editable via popup slider -->
                <template #item.per_complete="{ item }">
                  <div v-if="item.rowType === 'task'" class="d-flex align-center" style="min-width:110px">
                    <v-progress-linear :model-value="item.per_complete || 0"
                                       :color="getProgressColor(item.per_complete)"
                                       height="6" rounded class="mr-2" style="width:55px" />
                    <v-menu>
                      <template #activator="{ props }">
                        <span v-bind="props" class="text-caption cursor-pointer text-decoration-underline">
                          {{ item.per_complete || 0 }}%
                        </span>
                      </template>
                      <v-card min-width="200" class="pa-2">
                        <div class="text-caption mb-2 px-2">Update Progress</div>
                        <v-slider :model-value="item.per_complete || 0"
                                  @update:model-value="val => updateTaskProgress(item, val)"
                                  :min="0" :max="100" :step="5"
                                  thumb-label show-ticks="always"
                                  class="px-2" />
                      </v-card>
                    </v-menu>
                  </div>
                </template>

                <!-- Planned / Actual — type chip + date range stacked in one cell -->
                <template #item.type_dates="{ item }">
                  <div v-if="item.rowType === 'task'" class="d-flex flex-column py-1" style="gap:3px; font-size:11px; line-height:1.4">
                    <div class="d-flex align-center" style="gap:3px; flex-wrap:nowrap">
                      <v-chip color="blue" size="x-small" variant="tonal" label style="min-width:18px; padding:0 4px; font-size:9px">P</v-chip>
                      <span class="text-blue-darken-1">{{ fmtShort(item.planned_start_date) ?? '—' }}</span>
                      <span class="text-grey-darken-1" style="font-size:9px">›</span>
                      <span class="text-blue-darken-3">{{ fmtShort(item.planned_end_date) ?? '—' }}</span>
                    </div>
                    <div class="d-flex align-center" style="gap:3px; flex-wrap:nowrap">
                      <v-chip color="green" size="x-small" variant="tonal" label style="min-width:18px; padding:0 4px; font-size:9px">A</v-chip>
                      <span class="text-green-darken-1">{{ fmtShort(item.actual_start_date) ?? '—' }}</span>
                      <span class="text-grey-darken-1" style="font-size:9px">›</span>
                      <span class="text-green-darken-3">{{ fmtShort(item.actual_end_date) ?? '—' }}</span>
                    </div>
                  </div>
                </template>

                <!-- Date cells — empty; all bars on overlay -->
                <template v-for="col in timelineColumns"
                          :key="col.value"
                          #[`item.${col.value}`]="{ item }">
                  <div class="gantt-empty-cell"
                       :class="{ 'is-today': col.isToday, 'is-stage-header': item.rowType === 'stage-header' }" />
                </template>

              </v-data-table>

              <!-- ── Bar overlay ─────────────────────────────────────────────
                   Absolutely positioned over the date columns.
                   Each task row has TWO bars stacked vertically:
                     top ~38% of row height  → planned bar
                     top ~62% of row height  → actual bar
              ──────────────────────────────────────────────────────────── -->
              <div class="gantt-bar-overlay" ref="barOverlay" :style="overlayStyle">

                <template v-for="bar in barLayout" :key="bar.barId">
                  <v-tooltip location="top">
                    <template #activator="{ props }">
                      <div v-bind="props" :class="bar.cssClass" :style="bar.style">
                        <span v-if="bar.label" class="gantt-bar-label">{{ bar.label }}</span>
                      </div>
                    </template>
                    <div class="tooltip-content">
                      <div class="font-weight-bold mb-1">{{ bar.task_code ? bar.task_code + ' · ' : '' }}{{ bar.title }}</div>
                      <div>
                        <span class="bar-type-dot" :style="{ background: bar.type === 'planned' ? '#64B5F6' : '#81C784' }" />
                        {{ bar.type === 'planned' ? 'Planned' : 'Actual' }}
                      </div>
                      <div>Start: {{ formatDate(bar.start) }}</div>
                      <div>End: {{ formatDate(bar.end) }}</div>
                      <div>Progress: {{ bar.per_complete || 0 }}%</div>
                      <div>Status: {{ bar.status }}</div>
                    </div>
                  </v-tooltip>
                </template>

                <!-- Today vertical line -->
                <div class="today-line" :style="todayLineStyle" />
              </div>

            </div><!-- /gantt-wrapper -->
            <!-- Legend strip -->
            <v-sheet class="pa-3 d-flex align-center ga-4 border-t text-caption">
              <span class="text-grey">Legend:</span>
              <div class="d-flex align-center ga-1">
                <div class="legend-swatch legend-planned" />
                <span>Planned</span>
              </div>
              <div class="d-flex align-center ga-1">
                <div class="legend-swatch legend-actual" />
                <span>Actual</span>
              </div>
              <div class="d-flex align-center ga-1">
                <div style="width:2px;height:14px;background:rgba(33,150,243,0.7);border-radius:1px" />
                <span>Today</span>
              </div>
            </v-sheet>

          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-snackbar v-model="snackbar" :color="snackbarColor">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted, nextTick, watch } from 'vue'
  import { useRoute } from 'vue-router'
  import { NPI_STAGES } from '@/stores/stageTemplate'
  import { api } from '@/utils/api'

  const route = useRoute()

  // ── State ─────────────────────────────────────────────────────────────────────
  const loading = ref(false)
  const project = ref(null)
  const tasks = ref([])
  const viewMode = ref('week')
  const showAllStages = ref(false)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const ganttWrapper = ref(null)
  const barOverlay = ref(null)

  // Measured DOM geometry
  const overlayLeft = ref(0)
  const overlayWidth = ref(0)
  const rowHeight = ref(54)   // tall enough for two stacked bars per task
  const headerHeight = ref(36)

  // ── Stage constants ───────────────────────────────────────────────────────────
  const STAGE_COLORS_HEX = {
    '0.0': '#607D8B', '1.0': '#1976D2', '2.0': '#7B1FA2',
    '3.0': '#00796B', '4.0': '#303F9F', '5.0': '#E64A19'
  }
  const STAGE_SHORT = {
    '0.0': 'Enquiry', '1.0': 'Proj Start', '2.0': 'Pilot Mould',
    '3.0': 'Machine', '4.0': 'Prod Mould', '5.0': 'Trial JJ'
  }

  function getStageColor(sid) {
    return { '0.0': 'blue-grey', '1.0': 'primary', '2.0': 'purple', '3.0': 'teal', '4.0': 'indigo', '5.0': 'deep-orange' }[sid] ?? 'grey'
  }
  function deriveStageFromCode(code) {
    if (!code) return '1.0'
    const m = code.match(/^(\d+)\.\d+$/)
    return m ? `${m[1]}.0` : '1.0'
  }
  function resolvedStageId(task) {
    return task.stage_id || deriveStageFromCode(task.task_code)
  }

  // ── Pipeline ──────────────────────────────────────────────────────────────────
  const projectStages = computed(() => {
    if (!project.value) return []
    return Object.keys(NPI_STAGES).filter(id => {
      if (NPI_STAGES[id].required) return true
      if (id === '2.0') return !!project.value.pilot_mould_required
      if (id === '3.0') return !!project.value.machine_purchase_required
      return false
    }).map(id => {
      const st = tasks.value.filter(t => resolvedStageId(t) === id)
      const allDone = st.length > 0 && st.every(t => t.status === 'Completed')
      const anyActive = st.some(t => t.status === 'In Progress')
      return {
        id, name: NPI_STAGES[id].name, shortName: STAGE_SHORT[id],
        status: allDone ? 'completed' : anyActive ? 'active' : 'pending',
        bg: allDone ? 'rgba(76,175,80,0.1)' : anyActive ? `${STAGE_COLORS_HEX[id]}12` : 'transparent',
        border: allDone ? '#4CAF50' : anyActive ? STAGE_COLORS_HEX[id] : '#BDBDBD',
        textColor: allDone ? '#2E7D32' : anyActive ? STAGE_COLORS_HEX[id] : '#9E9E9E',
      }
    })
  })

  const currentStageId = computed(() => projectStages.value.find(s => s.status !== 'completed')?.id ?? projectStages.value.at(-1)?.id ?? '1.0')
  const currentStageName = computed(() => NPI_STAGES[currentStageId.value]?.name ?? currentStageId.value)
  const completedTasks = computed(() => tasks.value.filter(t => t.status === 'Completed').length)
  const overallProgress = computed(() => {
    if (!tasks.value.length) return 0
    return Math.round(tasks.value.reduce((s, t) => s + (t.per_complete || 0), 0) / tasks.value.length)
  })

  // ── Display rows — ONE ROW PER TASK ──────────────────────────────────────────
  const displayRows = computed(() => {
    const rows = []
    const stageIds = projectStages.value.map(s => s.id)

    stageIds.forEach(stageId => {
      const stageTasks = tasks.value.filter(t => resolvedStageId(t) === stageId)
      if (!stageTasks.length) return
      if (!showAllStages.value && stageId !== currentStageId.value) return

      if (showAllStages.value) {
        const done = stageTasks.filter(t => t.status === 'Completed').length
        rows.push({
          rowId: `stage_${stageId}_header`,
          rowType: 'stage-header',
          stage_id: stageId,
          stageName: NPI_STAGES[stageId]?.name ?? stageId,
          taskCount: stageTasks.length,
          completedCount: done,
          stageProgress: Math.round((done / stageTasks.length) * 100),
        })
      }

      stageTasks.forEach(task => {
        rows.push({
          ...task,
          rowId: `task_${task.task_id}`,
          rowType: 'task',
          stage_id: stageId,
        })
      })
    })

    return rows
  })

  // ── Timeline ──────────────────────────────────────────────────────────────────
  const timelineStart = computed(() => {
    const vis = showAllStages.value ? tasks.value : tasks.value.filter(t => resolvedStageId(t) === currentStageId.value)
    const dates = vis.map(t => new Date(t.planned_start_date)).filter(d => !isNaN(d))
    if (!dates.length) return new Date()
    const min = new Date(Math.min(...dates)); min.setDate(min.getDate() - 3); return min
  })
  const timelineEnd = computed(() => {
    const vis = showAllStages.value ? tasks.value : tasks.value.filter(t => resolvedStageId(t) === currentStageId.value)
    const dates = vis.map(t => new Date(t.planned_end_date)).filter(d => !isNaN(d))
    if (!dates.length) { const e = new Date(); e.setMonth(e.getMonth() + 1); return e }
    const max = new Date(Math.max(...dates)); max.setDate(max.getDate() + 3); return max
  })
  const timelineMs = computed(() => timelineEnd.value - timelineStart.value)

  const timelineColumns = computed(() => {
    const cols = [], start = new Date(timelineStart.value), end = new Date(timelineEnd.value)
    const today = new Date(); today.setHours(0, 0, 0, 0)

    if (viewMode.value === 'day') {
      let c = new Date(start)
      while (c <= end) {
        const d = new Date(c); d.setHours(0, 0, 0, 0)
        cols.push({ title: d.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }), value: `day_${d.getTime()}`, width: '52px', align: 'center', sortable: false, date: d, isToday: d.getTime() === today.getTime() })
        c.setDate(c.getDate() + 1)
      }
    } else if (viewMode.value === 'week') {
      let c = new Date(start); c.setDate(c.getDate() - c.getDay() + 1)
      while (c <= end) {
        const ws = new Date(c), we = new Date(c); we.setDate(we.getDate() + 6)
        cols.push({ title: ws.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }), value: `week_${ws.getTime()}`, width: '80px', align: 'center', sortable: false, date: ws, endDate: we, isToday: today >= ws && today <= we })
        c.setDate(c.getDate() + 7)
      }
    } else {
      let c = new Date(start.getFullYear(), start.getMonth(), 1)
      while (c <= end) {
        const ms = new Date(c), me = new Date(c.getFullYear(), c.getMonth() + 1, 0)
        cols.push({ title: ms.toLocaleDateString('en-GB', { month: 'short', year: '2-digit' }), value: `month_${ms.getTime()}`, width: '100px', align: 'center', sortable: false, date: ms, endDate: me, isToday: today.getMonth() === ms.getMonth() && today.getFullYear() === ms.getFullYear() })
        c.setMonth(c.getMonth() + 1)
      }
    }
    return cols
  })

  // Fixed left columns: title | dept_name | duration | per_complete | type_dates
  const FIXED_COL_COUNT = 5

  const ganttHeaders = computed(() => [
    { title: 'Task', value: 'title', width: '240px', sortable: false, fixed: true },
    { title: 'Dept', value: 'dept_name', width: '90px', sortable: false },
    { title: 'Dur.', value: 'duration', width: '60px', sortable: false },
    { title: 'Progress', value: 'per_complete', width: '120px', sortable: false },
    { title: 'Planned / Actual', value: 'type_dates', width: '130px', sortable: false },
    ...timelineColumns.value
  ])

  // ── Overlay measurement ───────────────────────────────────────────────────────
  function measureOverlay() {
    nextTick(() => {
      const wrapper = ganttWrapper.value
      if (!wrapper) return
      const table = wrapper.querySelector('table')
      if (!table) return

      const allTh = Array.from(table.querySelectorAll('thead tr th'))
      if (allTh.length <= FIXED_COL_COUNT) return

      const scrollable = wrapper.querySelector('.v-table__wrapper')
      const wrapperRect = wrapper.getBoundingClientRect()
      const firstDateTh = allTh[FIXED_COL_COUNT]
      const lastDateTh = allTh[allTh.length - 1]

      overlayLeft.value = firstDateTh.getBoundingClientRect().left - wrapperRect.left + (scrollable?.scrollLeft ?? 0)
      overlayWidth.value = lastDateTh.getBoundingClientRect().right - firstDateTh.getBoundingClientRect().left

      const firstTr = table.querySelector('tbody tr')
      if (firstTr) rowHeight.value = firstTr.getBoundingClientRect().height || 40

      const headerTr = table.querySelector('thead tr')
      if (headerTr) headerHeight.value = headerTr.getBoundingClientRect().height || 36
    })
  }

  // ── Bar layout — TWO BARS PER TASK ROW (planned upper, actual lower) ─────────
  const barLayout = computed(() => {
    if (!overlayWidth.value || !timelineMs.value) return []

    const bars = []
    const tsMs = timelineStart.value.getTime()
    const totMs = timelineMs.value
    // Each task row is tall: planned bar in top ~45%, actual bar in bottom ~45%
    const barH = Math.max(8, Math.floor(rowHeight.value * 0.38))

    displayRows.value.forEach((row, idx) => {
      if (row.rowType !== 'task') return

      const stageHex = STAGE_COLORS_HEX[row.stage_id] ?? '#1976D2'
      const statusSlug = (row.status ?? 'not-started').toLowerCase().replace(/\s+/g, '-')
      const rowTopPx = idx * rowHeight.value

      function makeBar(type, startDate, endDate) {
        if (!startDate || !endDate) return
        const sMs = new Date(startDate).getTime()
        const eMs = new Date(endDate).getTime()
        if (isNaN(sMs) || isNaN(eMs) || eMs <= sMs) return

        const leftPct = Math.max(0, (sMs - tsMs) / totMs) * 100
        const widthPct = Math.min(100 - leftPct, ((eMs - sMs) / totMs) * 100)
        if (widthPct <= 0) return

        // Planned: top 7% of row; Actual: top 52% of row
        const topOffset = type === 'planned'
          ? rowTopPx + Math.floor(rowHeight.value * 0.07)
          : rowTopPx + Math.floor(rowHeight.value * 0.52)

        bars.push({
          barId: `${row.task_id}_${type}`,
          title: row.title,
          task_code: row.task_code,
          type,
          start: startDate,
          end: endDate,
          status: row.status,
          per_complete: row.per_complete,
          cssClass: `gantt-bar gantt-bar-${type}-${statusSlug}`,
          label: widthPct > 9 ? (row.task_code ? `${row.task_code} · ${row.title}` : row.title) : '',
          style: {
            top: `${topOffset}px`,
            left: `${leftPct}%`,
            width: `${widthPct}%`,
            height: `${barH}px`,
            '--bar-progress': `${row.per_complete || 0}%`,
            '--bar-color': stageHex,
          }
        })
      }

      makeBar('planned', row.planned_start_date, row.planned_end_date)
      makeBar('actual', row.actual_start_date, row.actual_end_date)
    })

    return bars
  })

  // Today line
  const todayLineStyle = computed(() => {
    if (!overlayWidth.value || !timelineMs.value) return { display: 'none' }
    const pct = ((new Date().setHours(0, 0, 0, 0) - timelineStart.value.getTime()) / timelineMs.value) * 100
    if (pct < 0 || pct > 100) return { display: 'none' }
    return { display: 'block', left: `${pct}%`, top: '0', height: '100%' }
  })

  const overlayStyle = computed(() => ({
    position: 'absolute',
    top: `${headerHeight.value}px`,
    left: `${overlayLeft.value}px`,
    width: `${overlayWidth.value}px`,
    pointerEvents: 'none',
    overflow: 'visible',
    zIndex: 5,
  }))

  // ── Row props ─────────────────────────────────────────────────────────────────
  function getRowProps({ item }) {
    if (item.rowType === 'stage-header') return { class: 'gantt-row-stage-header' }
    if (item.rowType === 'task') return { class: 'gantt-row-task' }
    return {}
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────
  function getStatusColor(s) { return { Planning: 'grey', 'Not Started': 'grey', 'In Progress': 'blue', 'On Hold': 'orange', Completed: 'green', Cancelled: 'red' }[s] ?? 'grey' }
  function getPriorityColor(p) { return { Low: 'grey', Medium: 'blue', High: 'orange', Critical: 'red' }[p] ?? 'grey' }
  function getProgressColor(v) { if (v >= 100) return 'success'; if (v >= 50) return 'primary'; if (v > 0) return 'warning'; return 'grey' }

  function formatDate(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }
  function fmtShort(d) {
    if (!d) return null
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })
  }

  // ── API ───────────────────────────────────────────────────────────────────────
  async function updateTaskProgress(task, progress) {
    try {
      const result = await api.put(`/task/${task.task_id}/progress`, { per_complete: progress })
      if (result?.success) {
        const t = tasks.value.find(t => t.task_id === task.task_id)
        if (t) {
          t.per_complete = progress
          if (progress === 100) t.status = 'Completed'
          else if (progress > 0 && progress < 100 && t.status === 'Not Started') t.status = 'In Progress'
        }
        showSnack('Progress updated', 'success')
      }
    } catch { showSnack('Failed to update progress', 'error') }
  }

  function scrollToToday() {
    const wrapper = ganttWrapper.value?.querySelector('.v-table__wrapper')
    if (!wrapper || !overlayWidth.value || !timelineMs.value) return
    const pct = (new Date().setHours(0, 0, 0, 0) - timelineStart.value.getTime()) / timelineMs.value
    wrapper.scrollLeft = Math.max(0, pct * overlayWidth.value - wrapper.clientWidth / 2)
    showSnack('Scrolled to today', 'info')
  }

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg; snackbarColor.value = color; snackbar.value = true
  }
  async function refreshData() { await loadProjectData() }

  async function loadProjectData() {
    loading.value = true
    try {
      const pr = await api.get(`/project/${route.params.id}`)
      if (pr?.success && pr?.data) project.value = pr.data

      const tr = await api.get(`/project/${route.params.id}/tasks`)
      tasks.value = tr?.success ? (tr.data || []) : (Array.isArray(tr) ? tr : [])

      await nextTick()
      measureOverlay()
    } catch (err) {
      console.error(err)
      showSnack('Failed to load project data', 'error')
    } finally { loading.value = false }
  }

  watch([viewMode, displayRows, showAllStages], () => nextTick(measureOverlay))
  if (typeof window !== 'undefined') window.addEventListener('resize', measureOverlay)

  function attachScrollSync() {
    nextTick(() => {
      const tw = ganttWrapper.value?.querySelector('.v-table__wrapper')
      const ov = barOverlay.value
      if (!tw || !ov) return
      tw.addEventListener('scroll', () => {
        ov.style.transform = `translateX(-${tw.scrollLeft}px)`
        ov.style.top = `${headerHeight.value - tw.scrollTop}px`
      })
    })
  }

  onMounted(async () => { await loadProjectData(); attachScrollSync() })
</script>

<style scoped>
  .gantt-wrapper {
    position: relative;
    overflow: hidden;
  }

  /* Table base */
  .gantt-table :deep(.v-table__wrapper) {
    overflow-x: auto;
    overflow-y: auto;
  }

  .gantt-table :deep(th),
  .gantt-table :deep(td) {
    white-space: nowrap;
    border-right: 1px solid rgba(0,0,0,0.05);
    padding: 0 6px !important;
  }

  /* Task rows — tall to accommodate two stacked bars (planned + actual) */
  .gantt-row-task :deep(td) {
    height: 54px !important;
    vertical-align: middle;
  }

  /* Stage header rows */
  .gantt-row-stage-header :deep(td) {
    background-color: rgba(0,0,0,0.035) !important;
    border-top: 2px solid rgba(0,0,0,0.10) !important;
    font-weight: 600;
    height: 30px !important;
  }

  .stage-header-cell {
    width: 100%;
    padding: 2px 0;
  }

  /* Empty date cell */
  .gantt-empty-cell {
    height: 54px;
    width: 100%;
  }

    .gantt-empty-cell.is-stage-header {
      height: 30px;
      background-color: rgba(0,0,0,0.02);
    }

    .gantt-empty-cell.is-today {
      background-color: rgba(33,150,243,0.06);
      border-left: 2px solid rgba(33,150,243,0.35);
    }

  /* ── Overlay ─────────────────────────────────────────────────────────────── */
  .gantt-bar-overlay {
    pointer-events: none;
    position: absolute;
    overflow: visible;
  }

  /* ── Bars ─────────────────────────────────────────────────────────────────── */
  .gantt-bar {
    position: absolute;
    border-radius: 3px;
    display: flex;
    align-items: center;
    overflow: hidden;
    pointer-events: all;
    cursor: pointer;
    transition: filter 0.12s, box-shadow 0.12s;
    min-width: 3px;
  }

    .gantt-bar:hover {
      filter: brightness(1.12);
      box-shadow: 0 2px 5px rgba(0,0,0,0.25);
      z-index: 20;
    }

  /* Planned (upper bar) — lighter/semi-transparent */
  .gantt-bar-planned-not-started {
    background: rgba(158,158,158,0.38);
    border: 1px solid rgba(158,158,158,0.65);
  }

  .gantt-bar-planned-in-progress {
    background: linear-gradient(to right, color-mix(in srgb, var(--bar-color) 78%, transparent) var(--bar-progress), color-mix(in srgb, var(--bar-color) 22%, transparent) var(--bar-progress));
    border: 1px solid var(--bar-color);
  }

  .gantt-bar-planned-completed {
    background: rgba(76,175,80,0.48);
    border: 1px solid rgba(76,175,80,0.82);
  }

  .gantt-bar-planned-on-hold {
    background: rgba(255,152,0,0.48);
    border: 1px solid rgba(255,152,0,0.82);
  }

  .gantt-bar-planned-cancelled {
    background: rgba(244,67,54,0.38);
    border: 1px solid rgba(244,67,54,0.70);
  }

  /* Actual (lower bar) — more opaque / solid */
  .gantt-bar-actual-not-started {
    background: rgba(158,158,158,0.22);
    border: 1.5px dashed rgba(158,158,158,0.65);
  }

  .gantt-bar-actual-in-progress {
    background: linear-gradient(to right, color-mix(in srgb, var(--bar-color) 95%, black 5%) var(--bar-progress), color-mix(in srgb, var(--bar-color) 28%, transparent) var(--bar-progress));
    border: 1.5px solid var(--bar-color);
  }

  .gantt-bar-actual-completed {
    background: rgba(76,175,80,0.88);
    border: 1.5px solid rgba(76,175,80,1);
  }

  .gantt-bar-actual-on-hold {
    background: rgba(255,152,0,0.88);
    border: 1.5px solid rgba(255,152,0,1);
  }

  .gantt-bar-actual-cancelled {
    background: rgba(244,67,54,0.72);
    border: 1.5px solid rgba(244,67,54,1);
  }

  .gantt-bar-label {
    font-size: 10px;
    font-weight: 500;
    color: white;
    text-shadow: 0 1px 2px rgba(0,0,0,0.4);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    padding: 0 4px;
  }

  /* Today line */
  .today-line {
    position: absolute;
    width: 2px;
    background: rgba(33,150,243,0.7);
    pointer-events: none;
    z-index: 10;
  }

  /* Pipeline */
  .pipeline-chip {
    display: inline-flex;
    align-items: center;
    padding: 3px 8px;
    border-radius: 12px;
    border: 1.5px solid;
    white-space: nowrap;
    transition: box-shadow 0.2s;
  }

  .pipeline-chip--active {
    box-shadow: 0 0 0 3px rgba(25,118,210,0.2);
  }

  /* Legend */
  .legend-swatch {
    width: 22px;
    height: 8px;
    border-radius: 2px;
  }

  .legend-planned {
    background: rgba(25,118,210,0.45);
    border: 1px solid rgba(25,118,210,0.75);
  }

  .legend-actual {
    background: rgba(76,175,80,0.88);
    border: 1px solid rgba(76,175,80,1);
  }

  /* Tooltip */
  .tooltip-content {
    font-size: 12px;
    line-height: 1.6;
  }

  .bar-type-dot {
    display: inline-block;
    width: 8px;
    height: 8px;
    border-radius: 50%;
    margin-right: 4px;
  }

  .lh-tight {
    line-height: 1.35;
  }

  .border-b {
    border-bottom: 1px solid rgba(0,0,0,0.12);
  }

  .border-t {
    border-top: 1px solid rgba(0,0,0,0.12);
  }

  .cursor-pointer {
    cursor: pointer;
  }

  .text-truncate {
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 180px;
    display: inline-block;
  }
</style>
