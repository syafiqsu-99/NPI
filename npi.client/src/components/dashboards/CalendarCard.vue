<template>
  <v-card class="d-flex flex-column flex-grow-1 overflow-hidden">
    <v-card-title class="section-title d-flex align-center pa-3 pb-2 flex-shrink-0">
      <v-icon class="mr-2" size="18">mdi-calendar</v-icon>
      Project Calendar
    </v-card-title>
    <v-divider class="flex-shrink-0" />
    <v-sheet height="56" class="flex-shrink-0">
      <v-toolbar flat density="compact">
        <v-btn class="me-2" size="small" variant="outlined" @click="setToday">Today</v-btn>
        <v-btn icon size="small" variant="text" @click="prev">
          <v-icon size="18">mdi-chevron-left</v-icon>
        </v-btn>
        <v-btn icon size="small" variant="text" @click="next">
          <v-icon size="18">mdi-chevron-right</v-icon>
        </v-btn>
        <v-toolbar-title v-if="calendarRef">{{ calendarTitle }}</v-toolbar-title>
      </v-toolbar>
    </v-sheet>

    <v-sheet class="calendar-host flex-grow-1 position-relative">
      <v-calendar ref="calendarRef"
                  v-model="calendarFocus"
                  :events="calendarEvents"
                  view-mode="month"
                  color="primary"
                  class="dashboard-calendar compact-calendar"
                  :event-height="14"
                  :event-margin-bottom="2">
        <template #day="{ date, day, present }">
          <div class="compact-day-bg" :class="{ 'is-today': isTodayCheck(date, day, present) }" />
        </template>
        <template #day-label="{ date, day, present }">
          <div class="compact-day-label" :class="{ 'is-today-text': isTodayCheck(date, day, present) }">
            {{ getDayNumber(date, day) }}
          </div>
        </template>

        <template #event="{ event }">
          <div class="compact-event-pill"
               :style="{ '--evt-color': event.color }"
               :title="`${event.name} (${event.status} • ${event.progress}%)`">
            <span class="compact-event-dot" />
            <span class="compact-event-text">{{ event.name }}</span>
          </div>
        </template>

        <template #event-more="{ event }">
          <div class="compact-more-events">+{{ event.length || event }} more</div>
        </template>
      </v-calendar>
    </v-sheet>
  </v-card>
</template>

<script setup>
  import { ref, computed } from 'vue'

  const props = defineProps({
    projects: { type: Array, required: true },
    myTasks: { type: Array, required: true }
  })

  const calendarRef = ref(null)
  const calendarFocus = ref('')

  function statusHex(s) {
    return {
      Completed: '#4CAF50', 'In Progress': '#1976D2',
      'On Hold': '#FF9800', Planning: '#9E9E9E', Cancelled: '#F44336'
    }[s] ?? '#9E9E9E'
  }

  const calendarTitle = computed(() => {
    const base = calendarFocus.value ? new Date(calendarFocus.value) : new Date()
    return base.toLocaleDateString('en-GB', { month: 'long', year: 'numeric' })
  })

  const calendarEvents = computed(() => {
    return props.projects
      .filter(p => p.project_start_date && p.target_completion_date)
      .map(p => {
        const pt = props.myTasks.filter(t => t.proj_id === p.proj_id)
        const progress = pt.length
          ? Math.round(pt.reduce((s, t) => s + (t.per_complete || 0), 0) / pt.length)
          : 0
        return {
          name: p.proj_name,
          start: new Date(p.project_start_date),
          end: new Date(p.target_completion_date),
          color: statusHex(p.status),
          proj_id: p.proj_id,
          status: p.status,
          progress,
          timed: false,
        }
      })
  })

  function setToday() { calendarFocus.value = new Date() }
  function prev() {
    const c = calendarFocus.value ? new Date(calendarFocus.value) : new Date()
    c.setMonth(c.getMonth() - 1); calendarFocus.value = c
  }
  function next() {
    const c = calendarFocus.value ? new Date(calendarFocus.value) : new Date()
    c.setMonth(c.getMonth() + 1); calendarFocus.value = c
  }

  function isTodayCheck(date, dayObj, present) {
    if (present !== undefined) return present;
    if (dayObj && dayObj.isToday !== undefined) return dayObj.isToday;
    const d = new Date(date || (dayObj && dayObj.date));
    const today = new Date();
    return d.toDateString() === today.toDateString();
  }

  function getDayNumber(date, dayObj) {
    if (dayObj && dayObj.day) return dayObj.day;
    const d = new Date(date || (dayObj && dayObj.date));
    return d.getDate();
  }
</script>

<style scoped>
  .section-title {
    font-size: 0.9rem;
    font-weight: 600;
  }

  .calendar-host {
    height: 100%;
    min-height: 0;
    overflow: hidden !important;
    display: flex;
    flex-direction: column;
  }

  .compact-calendar {
    height: 100%;
    min-height: 0 !important;
    width: 100%;
  }

    .compact-calendar :deep(.v-calendar-month__days) {
      flex: 1 1 auto;
    }

  .compact-day-bg {
    width: 100%;
    height: 100%;
    position: absolute;
    top: 0;
    left: 0;
    pointer-events: none;
  }

    .compact-day-bg.is-today {
      background-color: rgba(25, 118, 210, 0.08);
      border-top: 2px solid rgb(25, 118, 210);
    }

  .compact-day-label {
    font-size: 11px;
    font-weight: 500;
    color: rgba(0,0,0,0.7);
    text-align: right;
    padding: 2px 5px;
    line-height: 1;
  }

    .compact-day-label.is-today-text {
      color: rgb(25, 118, 210);
      font-weight: 700;
    }

  .compact-event-pill {
    display: flex;
    align-items: center;
    gap: 3px;
    width: 100%;
    height: 100%;
    padding: 0 4px;
    border-radius: 2px;
    background: color-mix(in srgb, var(--evt-color) 16%, white);
    border-left: 2px solid var(--evt-color);
    box-sizing: border-box;
  }

  .compact-event-dot {
    width: 4px;
    height: 4px;
    border-radius: 50%;
    background: var(--evt-color);
    flex-shrink: 0;
  }

  .compact-event-text {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    font-size: 8.5px;
    line-height: 1;
    font-weight: 500;
  }

  .compact-more-events {
    font-size: 8.5px;
    line-height: 1;
    color: rgba(0,0,0,0.6);
    padding: 1px 4px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    font-weight: 500;
  }
</style>
