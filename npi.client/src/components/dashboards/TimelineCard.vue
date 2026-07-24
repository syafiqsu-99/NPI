<template>
  <v-card class="d-flex flex-column fill-height overflow-hidden">
    <v-card-title class="section-title d-flex align-center justify-space-between pa-3 pb-2 flex-shrink-0">
      <div class="d-flex align-center">
        <v-icon class="mr-2" size="18">mdi-timeline-outline</v-icon>
        Projects Timeline
      </div>
      <v-btn size="small" variant="outlined" @click="$emit('refresh')">
        <v-icon start>mdi-refresh</v-icon>Refresh
      </v-btn>
    </v-card-title>
    <v-divider class="flex-shrink-0" />

    <div class="timeline-wrapper flex-grow-1 d-flex flex-column position-relative" ref="timelineWrapper">
      <v-data-table v-if="projectTimeline.length"
                    :headers="tlHeaders"
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
            <span class="text-body-2 font-weight-medium text-truncate tl-proj-name" :title="item.proj_name">
              {{ item.proj_name }}
            </span>
            <v-chip :color="statusColor(item.status)" size="x-small" variant="tonal" class="mt-1" style="width:fit-content;">
              {{ item.status }}
            </v-chip>
          </div>
        </template>

        <template v-for="col in timelineWeeks" :key="col.value" #[`item.${col.value}`]>
          <div class="tl-empty-cell" :class="{ 'is-today': col.isToday }" />
        </template>
      </v-data-table>

      <div v-if="!projectTimeline.length" class="flex-grow-1 d-flex flex-column align-center justify-center text-medium-emphasis">
        <v-icon size="56">mdi-folder-open-outline</v-icon>
        <div class="mt-2">No projects available</div>
      </div>

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
</template>

<script setup>
  import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'
  import { useRouter } from 'vue-router'

  const props = defineProps({
    projects: { type: Array, required: true },
    myTasks: { type: Array, required: true }
  })
  defineEmits(['refresh'])

  const router = useRouter()
  const timelineWrapper = ref(null)
  const tlBarOverlay = ref(null)
  const tlOverlayLeft = ref(0)
  const tlOverlayWidth = ref(0)
  const tlRowHeight = ref(52)
  const tlHeaderHeight = ref(36)
  const TL_FIXED_COLS = 1

  const goToProject = id => router.push(`/projects/${id}/gantt`)

  function formatDate(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }
  function toMs(d) { return d ? new Date(d).getTime() : NaN }
  function statusColor(s) {
    return { Completed: 'success', 'In Progress': 'primary', 'On Hold': 'warning', Planning: 'grey', Cancelled: 'error' }[s] ?? 'grey'
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
      const we = new Date(ws); we.setDate(we.getDate() + 6); we.setHours(23, 59, 59, 999)
      return {
        title: ws.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' }),
        value: `tlw_${i}`, width: '72px', align: 'center', sortable: false, date: ws, endDate: we, isToday: today >= ws && today <= we,
      }
    })
  })

  const tlHeaders = computed(() => [
    { title: 'Project', value: 'proj_name', width: 220, sortable: false, fixed: true },
    ...timelineWeeks.value,
  ])

  const tlStart = computed(() => tlFirstMonday.value)
  const tlEnd = computed(() => { const e = new Date(tlFirstMonday.value); e.setDate(e.getDate() + 24 * 7); return e })
  const tlMs = computed(() => tlEnd.value - tlStart.value)

  const projectTimeline = computed(() =>
    props.projects.map(p => {
      const pt = props.myTasks.filter(t => t.proj_id === p.proj_id)
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

      const wrapperRect = wrapper.getBoundingClientRect()
      const scrollable = wrapper.querySelector('.v-table__wrapper')

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
    const tsMs = tlStart.value.getTime()
    const totMs = tlMs.value
    const barH = Math.max(8, Math.floor(tlRowHeight.value * 0.42))

    return projectTimeline.value.flatMap((p, idx) => {
      if (!p.project_start_date || !p.target_completion_date) return []
      const sMs = toMs(p.project_start_date)
      const eMs = toMs(p.target_completion_date) + 86400000

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
        style: { top: `${topOffset}px`, left: `${leftPct}%`, width: `${widthPct}%`, height: `${barH}px`, '--tl-progress': `${p.progress}%` },
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

  watch(projectTimeline, () => nextTick(measureTlOverlay))

  onMounted(() => { window.addEventListener('resize', measureTlOverlay); measureTlOverlay(); attachScrollSync() })
  onBeforeUnmount(() => { window.removeEventListener('resize', measureTlOverlay) })
</script>

<style scoped>
  .section-title {
    font-size: 0.9rem;
    font-weight: 600;
  }

  .timeline-table :deep(.v-table__wrapper) {
    overflow-x: auto;
    overflow-y: auto;
    height: 100%;
  }

  .timeline-table :deep(table) {
    width: 100%;
    table-layout: auto;
  }

  .timeline-table :deep(th), .timeline-table :deep(td) {
    white-space: nowrap;
    border-right: 1px solid rgba(0,0,0,0.05);
    padding: 0 8px !important;
  }

  .timeline-table :deep(th:first-child), .timeline-table :deep(td:first-child) {
    background: #ffffff !important;
    position: sticky;
    left: 0;
    z-index: 10;
    min-width: 220px;
    max-width: 220px;
    border-right: 2px solid #e0e0e0 !important;
  }

  .timeline-table :deep(th:first-child) {
    z-index: 11;
  }

  .timeline-table :deep(tbody tr) {
    cursor: pointer;
  }

  .timeline-table :deep(tbody tr:hover td) {
    background-color: rgba(0,0,0,0.025) !important;
  }

  .timeline-table :deep(tbody tr td) {
    height: 52px !important;
    vertical-align: middle;
  }

  .tl-proj-name {
    max-width: 200px;
    display: block;
  }

  .tl-empty-cell {
    height: 52px;
    width: 100%;
    display: block;
  }

    .tl-empty-cell.is-today {
      background-color: rgba(33,150,243,0.05);
      border-left: 2px solid rgba(33,150,243,0.35);
    }

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
      box-shadow: 0 2px 6px rgba(0,0,0,0.22);
      z-index: 20;
    }

  .tl-bar-planning {
    background: rgba(158,158,158,0.45);
    border: 1px solid rgba(158,158,158,0.75);
  }

  .tl-bar-in-progress {
    background: linear-gradient( to right, rgba(25,118,210,0.9) var(--tl-progress), rgba(25,118,210,0.25) var(--tl-progress) );
    border: 1px solid rgba(25,118,210,1);
  }

  .tl-bar-completed {
    background: rgba(76,175,80,0.85);
    border: 1px solid rgba(76,175,80,1);
  }

  .tl-bar-on-hold {
    background: rgba(255,152,0,0.85);
    border: 1px solid rgba(255,152,0,1);
  }

  .tl-bar-cancelled {
    background: rgba(244,67,54,0.6);
    border: 1px solid rgba(244,67,54,0.9);
  }

  .tl-bar-label {
    font-size: 10px;
    font-weight: 500;
    color: white;
    text-shadow: 0 1px 2px rgba(0,0,0,0.35);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    padding: 0 5px;
  }

  .tl-today-line {
    position: absolute;
    width: 2px;
    background: rgba(33,150,243,0.72);
    pointer-events: none;
    z-index: 10;
  }
</style>
