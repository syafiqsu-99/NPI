<template>
  <v-container fluid class="pa-0" style="height:100vh; overflow:hidden; display:flex; flex-direction:column;">
    <v-row no-gutters class="h-100">
      <v-col cols="12" class="h-100">
        <v-card elevation="2" class="h-100 d-flex flex-column">

          <!-- ── Compact Header ──────────────────────────────────────────── -->
          <v-card-title class="bg-primary d-flex align-center justify-space-between py-1 px-3 text-subtitle-1 font-weight-medium">
            <div>
              <v-icon class="mr-2" size="small">mdi-chart-gantt</v-icon>
              Project Gantt Chart — {{ project?.proj_no }}
            </div>
            <div>
              <v-btn variant="text" color="white" density="compact" class="mr-2" @click="refreshData">
                <v-icon start size="small">mdi-refresh</v-icon>
                Refresh
              </v-btn>
              <v-btn variant="text" color="white" density="compact" @click="$router.push('/projects')">
                <v-icon start size="small">mdi-arrow-left</v-icon>
                Back
              </v-btn>
            </div>
          </v-card-title>

          <v-card-text v-if="loading" class="text-center pa-8">
            <v-progress-circular indeterminate color="primary" size="64" />
          </v-card-text>

          <v-card-text v-else class="pa-0 d-flex flex-column flex-grow-1" style="min-height: 0;">

            <!-- ── Compact Project Summary ────────────────────────────────── -->
            <v-sheet class="pa-2 bg-grey-lighten-4 border-b">
              <v-row align="center" dense>
                <v-col cols="auto" class="mr-6">
                  <div class="text-caption text-grey-darken-1 lh-tight">Project Name</div>
                  <div class="text-body-2 font-weight-medium">{{ project?.proj_name }}</div>
                </v-col>
                <v-col cols="auto" class="mr-6">
                  <div class="text-caption text-grey-darken-1 lh-tight">Status</div>
                  <v-chip :color="PROJECT_STATUS_COLORS[project?.status] || 'grey'" size="x-small" variant="tonal" class="mt-1">
                    {{ project?.status }}
                  </v-chip>
                </v-col>
                <v-col cols="auto" class="mr-6">
                  <div class="text-caption text-grey-darken-1 lh-tight">Priority</div>
                  <v-chip :color="PRIORITY_COLORS[project?.priority] || 'grey'" size="x-small" variant="tonal" class="mt-1">
                    {{ project?.priority }}
                  </v-chip>
                </v-col>
                <v-col cols="auto" class="mr-6">
                  <div class="text-caption text-grey-darken-1 lh-tight">Start Date</div>
                  <div class="text-body-2">{{ formatDate(project?.project_start_date) }}</div>
                </v-col>
                <v-col cols="auto">
                  <div class="text-caption text-grey-darken-1 lh-tight">Target Completion</div>
                  <div class="text-body-2">{{ formatDate(project?.target_completion_date) }}</div>
                </v-col>
              </v-row>
            </v-sheet>

            <!-- ── Merged NPI Stage pipeline & Controls ───────────────────── -->
            <v-sheet class="pa-2 bg-white border-b">
              <div class="d-flex align-center ga-2 overflow-x-auto">
                <span class="text-caption text-grey mr-2 flex-shrink-0">Stages:</span>
                <template v-for="(stage, idx) in projectStages" :key="stage.id">
                  <div class="pipeline-chip flex-shrink-0"
                       :class="{ 'pipeline-chip--active': stage.id === currentStageId }"
                       :style="{ background: stage.bg, borderColor: stage.border, color: stage.textColor }">
                    <span class="font-weight-bold" style="font-size:11px">{{ stage.id }}</span>
                    <span style="font-size:10px; margin-left:4px">{{ stage.shortName }}</span>
                    <v-icon v-if="stage.status === 'completed'" size="x-small" class="ml-1" color="success">mdi-check-circle</v-icon>
                    <v-icon v-else-if="stage.status === 'active'" size="x-small" class="ml-1" color="primary">mdi-play-circle</v-icon>
                  </div>
                  <v-icon v-if="idx < projectStages.length - 1" size="small" color="grey-lighten-1" class="flex-shrink-0">mdi-arrow-right</v-icon>
                </template>

                <v-spacer />

                <v-btn-toggle v-model="viewMode" mandatory variant="outlined" density="compact" class="flex-shrink-0 bg-white">
                  <v-btn value="day" size="small">Day</v-btn>
                  <v-btn value="week" size="small">Week</v-btn>
                  <v-btn value="month" size="small">Month</v-btn>
                </v-btn-toggle>
                <v-btn size="small" variant="outlined" density="compact" class="flex-shrink-0 bg-white" @click="scrollToToday">
                  <v-icon start size="small">mdi-calendar-today</v-icon>
                  Today
                </v-btn>
                <v-checkbox v-model="showAllStages" label="Show all stages" hide-details density="compact" class="flex-shrink-0 ml-2" />
              </div>
            </v-sheet>

            <div class="gantt-wrapper flex-grow-1" ref="ganttWrapper">

              <v-data-table-virtual ref="ganttTable"
                                    :headers="ganttHeaders"
                                    :items="displayRows"
                                    item-value="rowId"
                                    :row-props="getRowProps"
                                    class="gantt-table"
                                    density="compact"
                                    fixed-header
                                    height="420">

                <!-- Task column -->
                <template #item.title="{ item }">
                  <div v-if="item.rowType === 'stage-header'" class="stage-header-cell d-flex align-center pr-2">
                    <v-chip :color="STAGE_COLORS[item.stage_id] || 'primary'" size="small" variant="tonal" class="mr-2 font-weight-bold">
                      {{ item.stage_id }}
                    </v-chip>
                    <span class="font-weight-medium" style="color: #37474F;">{{ item.stageName }}</span>
                  </div>
                  <div v-else class="d-flex align-center ga-1 py-1 pr-2">
                    <v-chip v-if="item.task_code" :color="STAGE_COLORS[item.stage_id] || 'primary'" size="x-small" variant="tonal" class="font-weight-bold flex-shrink-0">
                      {{ item.task_code }}
                    </v-chip>
                    <span class="text-body-2 font-weight-medium text-truncate" style="max-width:160px" :title="item.title">{{ item.title }}</span>
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

                <!-- Planned / Actual -->
                <template #item.type_dates="{ item }">
                  <div v-if="item.rowType === 'task'" class="d-flex flex-column py-1 pr-2" style="gap:3px; font-size:11px; line-height:1.4">
                    <div class="d-flex align-center" style="gap:3px; flex-wrap:nowrap">
                      <v-chip color="blue" size="x-small" variant="tonal" label style="min-width:18px; padding:0 4px; font-size:9px">P</v-chip>
                      <span class="text-blue-darken-1">{{ formatDateShort(item.planned_start_date) ?? '—' }}</span>
                      <span class="text-grey-darken-1" style="font-size:9px">›</span>
                      <span class="text-blue-darken-3">{{ formatDateShort(item.planned_end_date) ?? '—' }}</span>
                    </div>
                    <div class="d-flex align-center" style="gap:3px; flex-wrap:nowrap">
                      <v-chip color="green" size="x-small" variant="tonal" label style="min-width:18px; padding:0 4px; font-size:9px">A</v-chip>
                      <span class="text-green-darken-1">{{ formatDateShort(item.actual_start_date) ?? '—' }}</span>
                      <span class="text-grey-darken-1" style="font-size:9px">›</span>
                      <span class="text-green-darken-3">{{ formatDateShort(item.actual_end_date) ?? (item.actual_start_date ? 'Now' : '—') }}</span>
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
              </v-data-table-virtual>

              <!-- ── Bar Overlay (Clipped Area Container) ───────────────────────── -->
              <div class="gantt-clip-box" :style="clipBoxStyle">
                <div class="gantt-canvas" :style="canvasStyle">

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
              </div>

            </div><!-- /gantt-wrapper -->
            <!-- Compact Legend strip -->
            <v-sheet class="pa-1 px-3 d-flex align-center ga-4 border-t text-caption bg-grey-lighten-5">
              <span class="text-grey font-weight-medium">Legend:</span>
              <div class="d-flex align-center ga-1">
                <div class="legend-swatch legend-planned" />
                <span>Planned</span>
              </div>
              <div class="d-flex align-center ga-1">
                <div class="legend-swatch legend-actual" />
                <span>Actual</span>
              </div>
              <div class="d-flex align-center ga-1">
                <div style="width:2px;height:12px;background:rgba(33,150,243,0.7);border-radius:1px" />
                <span>Today</span>
              </div>
            </v-sheet>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Success Snackbar -->
    <v-snackbar v-model="snackbar" :color="snackbarColor">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted, nextTick, watch, onBeforeUnmount } from 'vue'
  import { useRoute } from 'vue-router'
  import { useSettingsStore } from '@/stores/setting'
  import { api } from '@/utils/api'
  import {
    STAGE_COLORS,
    STAGE_COLORS_HEX,
    STAGE_SHORT_NAMES,
    PROJECT_STATUS_COLORS,
    PRIORITY_COLORS,
    OPTIONAL_STAGE_FLAGS,
    TASK_STATUS,
    DEFAULT_COLOR,
  } from '@/utils/constants'
  import { formatDate, formatDateShort } from '@/utils/formatters'

  const route = useRoute()
  const settingsStore = useSettingsStore()

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

  // Measured DOM geometry for accurate clipping and canvas scroll
  const headerHeight = ref(36)
  const fixedColsWidth = ref(0)
  const timelineTotalWidth = ref(0)
  const scrollbarWidth = ref(0)
  const scrollbarHeight = ref(0)
  const scrollTop = ref(0)
  const scrollLeft = ref(0)

  function deriveStageFromCode(code) {
    if (!code) return '1.0'
    const m = code.match(/^(\d+)\.\d+$/)
    return m ? `${m[1]}.0` : '1.0'
  }
  function resolvedStageId(task) {
    return task.stage_id || deriveStageFromCode(task.task_code)
  }

  let scrollTarget = null;
  function handleScroll(e) {
    if (!e.target) return
    scrollTop.value = e.target.scrollTop
    scrollLeft.value = e.target.scrollLeft
  }

  function attachScrollSync() {
    nextTick(() => {
      scrollTarget = ganttWrapper.value?.querySelector('.v-table__wrapper')
      if (scrollTarget) {
        scrollTarget.addEventListener('scroll', handleScroll)
      }
    })
  }

  // ── Pipeline ──────────────────────────────────────────────────────────────────
  const projectStages = computed(() => {
    if (!project.value) return []
    return (settingsStore.stages ?? []).map(s => s.stage_id).filter(id => {
      if (settingsStore.isStageRequired(id)) return true
      const flagKey = OPTIONAL_STAGE_FLAGS[id]
      return flagKey ? !!project.value[flagKey] : false
    }).map(id => {
      const st = tasks.value.filter(t => resolvedStageId(t) === id)
      const allDone = st.length > 0 && st.every(t => t.status === TASK_STATUS.COMPLETED)
      const anyActive = st.some(t => t.status === TASK_STATUS.IN_PROGRESS)
      return {
        id, name: settingsStore.getStageName(id), shortName: STAGE_SHORT_NAMES[id],
        status: allDone ? 'completed' : anyActive ? 'active' : 'pending',
        bg: allDone ? 'rgba(76,175,80,0.1)' : anyActive ? `${STAGE_COLORS_HEX[id]}12` : 'transparent',
        border: allDone ? '#4CAF50' : anyActive ? STAGE_COLORS_HEX[id] : '#BDBDBD',
        textColor: allDone ? '#2E7D32' : anyActive ? STAGE_COLORS_HEX[id] : '#9E9E9E',
      }
    })
  })

  const currentStageId = computed(() => projectStages.value.find(s => s.status !== 'completed')?.id ?? projectStages.value.at(-1)?.id ?? '1.0')

  // ── Display rows ─────────────────────────────────────────────────────────────
  const displayRows = computed(() => {
    const rows = []
    const stageIds = projectStages.value.map(s => s.id)

    stageIds.forEach(stageId => {
      const stageTasks = tasks.value.filter(t => resolvedStageId(t) === stageId)
      if (!stageTasks.length) return
      if (!showAllStages.value && stageId !== currentStageId.value) return

      if (showAllStages.value) {
        rows.push({
          rowId: `stage_${stageId}_header`,
          rowType: 'stage-header',
          stage_id: stageId,
          stageName: settingsStore.getStageName(stageId),
          taskCount: stageTasks.length,
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

  const timelineColumns = computed(() => {
    const cols = [], start = new Date(timelineStart.value), end = new Date(timelineEnd.value)
    const today = new Date(); today.setHours(0, 0, 0, 0)

    if (viewMode.value === 'day') {
      let c = new Date(start)
      while (c <= end) {
        const d = new Date(c); d.setHours(0, 0, 0, 0)
        const dEnd = new Date(c); dEnd.setHours(23, 59, 59, 999)
        cols.push({ title: d.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }), value: `day_${d.getTime()}`, width: '52px', align: 'center', sortable: false, date: d, endDate: dEnd, isToday: d.getTime() === today.getTime() })
        c.setDate(c.getDate() + 1)
      }
    } else if (viewMode.value === 'week') {
      let c = new Date(start); c.setDate(c.getDate() - c.getDay() + 1)
      while (c <= end) {
        const ws = new Date(c); ws.setHours(0, 0, 0, 0)
        const we = new Date(c); we.setDate(we.getDate() + 6); we.setHours(23, 59, 59, 999)
        cols.push({ title: ws.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }), value: `week_${ws.getTime()}`, width: '80px', align: 'center', sortable: false, date: ws, endDate: we, isToday: today >= ws && today <= we })
        c.setDate(c.getDate() + 7)
      }
    } else {
      let c = new Date(start.getFullYear(), start.getMonth(), 1)
      while (c <= end) {
        const ms = new Date(c); ms.setHours(0, 0, 0, 0)
        const me = new Date(c.getFullYear(), c.getMonth() + 1, 0); me.setHours(23, 59, 59, 999)
        cols.push({ title: ms.toLocaleDateString('en-GB', { month: 'short', year: '2-digit' }), value: `month_${ms.getTime()}`, width: '100px', align: 'center', sortable: false, date: ms, endDate: me, isToday: today.getMonth() === ms.getMonth() && today.getFullYear() === ms.getFullYear() })
        c.setMonth(c.getMonth() + 1)
      }
    }
    return cols
  })

  const renderedStart = computed(() => timelineColumns.value.length ? timelineColumns.value[0].date.getTime() : Date.now())
  const renderedEnd = computed(() => timelineColumns.value.length ? timelineColumns.value[timelineColumns.value.length - 1].endDate.getTime() + 1 : Date.now() + 86400000)
  const renderedMs = computed(() => renderedEnd.value - renderedStart.value)

  // Number of fixed left columns
  const FIXED_COL_COUNT = 4

  const ganttHeaders = computed(() => [
    { title: 'Task', value: 'title', width: '240px', sortable: false, fixed: true },
    { title: 'Dept', value: 'dept_name', width: '90px', sortable: false, fixed: true },
    { title: 'Dur.', value: 'duration', width: '60px', sortable: false, fixed: true },
    { title: 'Planned / Actual', value: 'type_dates', width: '130px', sortable: false, fixed: true },
    ...timelineColumns.value
  ])

  // ── Overlay / Box Measurement ─────────────────────────────────────────────────
  function measureOverlay() {
    nextTick(() => {
      const wrapper = ganttWrapper.value
      if (!wrapper) return
      const scrollable = wrapper.querySelector('.v-table__wrapper')
      if (!scrollable) return

      const table = scrollable.querySelector('table')
      if (!table) return
      const thead = table.querySelector('thead')
      if (!thead) return

      const allTh = Array.from(thead.querySelectorAll('tr:last-child th'))
      if (allTh.length <= FIXED_COL_COUNT) return

      headerHeight.value = thead.getBoundingClientRect().height || 36

      const scrollableRect = scrollable.getBoundingClientRect()

      // Calculate start of Gantt area right after the Fixed Columns
      const lastFixedTh = allTh[FIXED_COL_COUNT - 1]
      fixedColsWidth.value = lastFixedTh.getBoundingClientRect().right - scrollableRect.left

      // Timeline span total size
      const firstDateTh = allTh[FIXED_COL_COUNT]
      const lastDateTh = allTh[allTh.length - 1]
      timelineTotalWidth.value = lastDateTh.getBoundingClientRect().right - firstDateTh.getBoundingClientRect().left

      // Prevent overlapping scrollbars
      scrollbarWidth.value = scrollable.offsetWidth - scrollable.clientWidth
      scrollbarHeight.value = scrollable.offsetHeight - scrollable.clientHeight

      scrollTop.value = scrollable.scrollTop
      scrollLeft.value = scrollable.scrollLeft
    })
  }

  // ── Clipping & Scroll Overlay Styles ──────────────────────────────────────────
  const clipBoxStyle = computed(() => ({
    position: 'absolute',
    top: `${headerHeight.value}px`,
    left: `${fixedColsWidth.value}px`,
    right: `${scrollbarWidth.value}px`,
    bottom: `${scrollbarHeight.value}px`,
    overflow: 'hidden',
    pointerEvents: 'none',
    zIndex: 5
  }))

  const canvasStyle = computed(() => ({
    position: 'absolute',
    top: '0px',
    left: '0px',
    width: `${timelineTotalWidth.value}px`,
    transform: `translate(-${scrollLeft.value}px, -${scrollTop.value}px)`,
    pointerEvents: 'none'
  }))

  // ── Bar layout ────────────────────────────────────────────────────────────────
  const barLayout = computed(() => {
    if (!timelineTotalWidth.value || !renderedMs.value) return []

    const bars = []
    const tsMs = renderedStart.value
    const totMs = renderedMs.value

    let currentTop = 0;

    displayRows.value.forEach((row) => {
      // Must exactly mirror CSS fixed dimensions
      const rowH = row.rowType === 'stage-header' ? 30 : 54;

      if (row.rowType === 'task') {
        const stageHex = STAGE_COLORS_HEX[row.stage_id] ?? '#1976D2'
        const statusSlug = (row.status ?? 'not-started').toLowerCase().replace(/\s+/g, '-')

        const makeBar = (type, startDate, endDate) => {
          if (!startDate) return

          // Handle missing 'actual_end_date' by falling back to now()
          let resolvedEndDate = endDate;
          if (type === 'actual' && !endDate) {
            resolvedEndDate = new Date();
          } else if (!resolvedEndDate) {
            return;
          }

          const sDate = new Date(startDate)
          const eDate = new Date(resolvedEndDate)

          if (isNaN(sDate.getTime()) || isNaN(eDate.getTime())) return

          sDate.setHours(0, 0, 0, 0)
          eDate.setHours(23, 59, 59, 999)

          const sMs = sDate.getTime()
          const eMs = eDate.getTime()

          if (eMs < sMs) return

          const leftPct = Math.max(0, (sMs - tsMs) / totMs) * 100
          const rightPct = Math.min(100, ((eMs - tsMs) / totMs) * 100)
          const widthPct = rightPct - leftPct

          if (widthPct <= 0 || leftPct >= 100) return

          // Calculate correct spacing offset vertically relative to currentTop
          const topOffset = type === 'planned' ? currentTop + 9 : currentTop + 31;

          bars.push({
            barId: `${row.task_id}_${type}`,
            title: row.title,
            task_code: row.task_code,
            type,
            start: startDate,
            end: resolvedEndDate,
            status: row.status,
            per_complete: row.per_complete,
            cssClass: `gantt-bar gantt-bar-${type}-${statusSlug}`,
            label: widthPct > 4 ? (row.task_code ? `${row.task_code} · ${row.title}` : row.title) : '',
            style: {
              top: `${topOffset}px`,
              left: `${leftPct}%`,
              width: `${widthPct}%`,
              height: `14px`,
              '--bar-progress': `${row.per_complete || 0}%`,
              '--bar-color': stageHex,
            }
          })
        }

        makeBar('planned', row.planned_start_date, row.planned_end_date)
        makeBar('actual', row.actual_start_date, row.actual_end_date)
      }

      currentTop += rowH;
    })

    return bars
  })

  // Today line dynamic overlay mapping
  const todayLineStyle = computed(() => {
    if (!timelineTotalWidth.value || !renderedMs.value) return { display: 'none' }
    const todayMs = new Date().setHours(0, 0, 0, 0)
    const pct = ((todayMs - renderedStart.value) / renderedMs.value) * 100
    if (pct < 0 || pct > 100) return { display: 'none' }
    return { display: 'block', left: `${pct}%`, top: '0', height: '100%' }
  })

  function getRowProps({ item }) {
    if (item.rowType === 'stage-header') return { class: 'gantt-row-stage-header' }
    if (item.rowType === 'task') return { class: 'gantt-row-task' }
    return {}
  }

  function scrollToToday() {
    const wrapper = ganttWrapper.value?.querySelector('.v-table__wrapper')
    if (!wrapper || !timelineTotalWidth.value || !renderedMs.value) return
    const todayMs = new Date().setHours(0, 0, 0, 0)
    const pct = (todayMs - renderedStart.value) / renderedMs.value
    wrapper.scrollLeft = Math.max(0, pct * timelineTotalWidth.value - wrapper.clientWidth / 2)
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
      showSnack('Failed to load project data', 'error')
    } finally { loading.value = false }
  }

  watch([viewMode, displayRows, showAllStages], () => nextTick(measureOverlay))

  let resizeObserver = null

  onMounted(async () => {
    try {
      const templateResult = await settingsStore.fetchTaskTemplates()
      if (!templateResult?.success) {
        showSnack('Warning: could not load stage definitions', 'warning')
      }
      await loadProjectData()
      attachScrollSync()

      window.addEventListener('resize', measureOverlay)

      resizeObserver = new ResizeObserver(() => measureOverlay())
      if (ganttWrapper.value) resizeObserver.observe(ganttWrapper.value)
    } catch (err) {
      console.error('Gantt mount error:', err)
      showSnack('Failed to initialise the Gantt view', 'error')
      loading.value = false
    }
  })

  onBeforeUnmount(() => {
    resizeObserver?.disconnect()
    window.removeEventListener('resize', measureOverlay)
    if (scrollTarget) scrollTarget.removeEventListener('scroll', handleScroll)
  })
</script>

<style scoped>
  .gantt-wrapper {
    position: relative;
    overflow: hidden;
    flex: 1;
    min-height: 0;
  }

  .gantt-table :deep(.v-table__wrapper) {
    overflow-x: auto;
    overflow-y: auto;
    height: 100%;
  }

  .gantt-table :deep(table) {
    table-layout: fixed;
  }

  .gantt-table :deep(th),
  .gantt-table :deep(td) {
    white-space: nowrap;
    border-right: 1px solid rgba(0,0,0,0.05);
    border-bottom: none !important;
    box-shadow: inset 0 -1px 0 rgba(0,0,0,0.08);
    padding: 0 6px !important;
    box-sizing: border-box !important;
  }

  /* ── Horizontally Fixed Data Columns ────────────────────────────────────── */

  /* Planned/Actual Column - Includes Boundary border */
  .gantt-table :deep(th:nth-child(4)),
  .gantt-table :deep(td:nth-child(4)) {
    border-right: 2px solid #CFD8DC !important;
  }

  /* Z-Index mapping for overlapping rows and headers */
  .gantt-table :deep(thead th) {
    z-index: 3 !important;
  }

  .gantt-table :deep(thead th:nth-child(1)),
  .gantt-table :deep(thead th:nth-child(2)),
  .gantt-table :deep(thead th:nth-child(3)),
  .gantt-table :deep(thead th:nth-child(4)) {
    z-index: 4 !important;
  }
  /* Corner intersection */

  /* Background blocks for overlapping sticky rows */
  .gantt-table :deep(tr.gantt-row-task td:nth-child(-n+4)) {
    background-color: #ffffff;
  }

  .gantt-table :deep(tr.gantt-row-stage-header td:nth-child(-n+4)) {
    background-color: #ECEFF1;
  }

  /* ── Row height lockdown mapped exactly to JavaScript offsets ───────────── */
  .gantt-row-task :deep(td) {
    height: 54px !important;
    max-height: 54px !important;
    vertical-align: middle;
  }

  .gantt-row-stage-header :deep(td) {
    background-color: #ECEFF1 !important;
    font-weight: 600;
    height: 30px !important;
    max-height: 30px !important;
  }

  .stage-header-cell {
    width: 100%;
    padding: 2px 0;
  }

  .gantt-empty-cell {
    height: 100%;
    width: 100%;
  }

    .gantt-empty-cell.is-stage-header {
      background-color: rgba(0,0,0,0.02);
    }

    .gantt-empty-cell.is-today {
      background-color: rgba(33,150,243,0.06);
      border-left: 2px solid rgba(33,150,243,0.35);
    }

  /* ── Overlay Masks ─────────────────────────────────────────────────────── */
  .gantt-clip-box {
    position: absolute;
    overflow: hidden;
    pointer-events: none;
    z-index: 5;
  }

  .gantt-canvas {
    position: absolute;
    pointer-events: none;
  }

  /* ── Bars ─────────────────────────────────────────────────────────────────── */
  .gantt-bar {
    position: absolute;
    border-radius: 3px;
    display: flex;
    align-items: center;
    overflow: hidden;
    pointer-events: auto;
    cursor: pointer;
    transition: filter 0.12s, box-shadow 0.12s;
    min-width: 3px;
  }

    .gantt-bar:hover {
      filter: brightness(1.12);
      box-shadow: 0 2px 5px rgba(0,0,0,0.25);
      z-index: 20;
    }

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

  .today-line {
    position: absolute;
    width: 2px;
    background: rgba(33,150,243,0.7);
    pointer-events: none;
    z-index: 10;
  }

  /* Component Layout Tooling */
  .pipeline-chip {
    display: inline-flex;
    align-items: center;
    padding: 2px 6px;
    border-radius: 12px;
    border: 1.5px solid;
    white-space: nowrap;
    transition: box-shadow 0.2s;
  }

  .pipeline-chip--active {
    box-shadow: 0 0 0 3px rgba(25,118,210,0.2);
  }

  .legend-swatch {
    width: 18px;
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
    line-height: 1.25;
  }

  .border-b {
    border-bottom: 1px solid rgba(0,0,0,0.12);
  }

  .border-t {
    border-top: 1px solid rgba(0,0,0,0.12);
  }
</style>
