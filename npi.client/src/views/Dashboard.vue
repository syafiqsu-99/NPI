<template>
  <v-container fluid fill-height class="d-flex flex-column">
    <v-row no-gutters align="center" justify="center">
      <v-col cols="12" class="text-center">
        <h2 class="font-weight-bold">
          <v-icon class="mr-2">mdi-clipboard-list-outline</v-icon>
          Dashboard
        </h2>
      </v-col>
    </v-row>

    <!-- KPI ROW -->
    <v-row no-gutters>
      <v-col v-for="kpi in dashboard.kpis" :key="kpi.title" cols="3">
        <v-card class="fill-height ms-1" :color="kpi.color" variant="tonal">
          <v-card-title class="text-caption">
            {{ kpi.title }}
          </v-card-title>
          <v-card-text class="text-h4 font-weight-bold">
            {{ kpi.value }}
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- MAIN CONTENT -->
    <v-row no-gutters>
      <!-- LEFT COLUMN -->
      <v-col cols="4" class="fill-height d-flex flex-column">
        <!-- CALENDAR -->
        <v-card class="flex-grow-1 ma-1 d-flex flex-column">
          <v-sheet class="d-flex" tile>
            <v-btn class="ma-2"
                   variant="text"
                   icon
                   @click="$refs.calendar.prev()">
              <v-icon>mdi-chevron-left</v-icon>
            </v-btn>
            <v-spacer></v-spacer>
            <v-card-title>Project Calendar</v-card-title>
            <v-spacer></v-spacer>
            <v-btn class="ma-2"
                   variant="text"
                   icon
                   @click="$refs.calendar.next()">
              <v-icon>mdi-chevron-right</v-icon>
            </v-btn>
          </v-sheet>

            <v-sheet class="flex-grow-1" height="450">
              <v-calendar ref="calendar"
                          type="month"
                          :event-height="20"
                          :event-margin-bottom="0"
                          :weekdays="[0, 1, 2, 3, 4, 5, 6]"
                          :events="dashboard.calendarEvents"
                          :event-color="e => e.color"
                          class="fill-height"/>
                          <!--@click:event="onCalendarClick" />-->
            </v-sheet>
        </v-card>

        <!-- OVERDUE TASKS -->
        <v-card class="flex-grow-1 ma-1">
          <v-card-title>Overdue Tasks</v-card-title>
          <v-divider />

          <v-list density="compact">
            <v-list-item v-for="t in dashboard.overdueTasks"
                         :key="t.task_id">
              <v-list-item-title>
                {{ t.title }}
              </v-list-item-title>
              <v-list-item-subtitle>
                {{ t.proj_name }} · Due {{ formatDate(t.end_date) }}
              </v-list-item-subtitle>

              <template #append>
                <v-chip color="red" size="small">
                  {{ t.days_overdue }}d
                </v-chip>
              </template>
            </v-list-item>
          </v-list>
        </v-card>
      </v-col>

      <!-- RIGHT COLUMN -->
      <v-col cols="8" class="fill-height">
        <v-card class="fill-height">
          <v-card-title>Project Timeline (Gantt)</v-card-title>
          <v-divider />

          <v-data-table-virtual fixed-header
                                :headers="dashboard.headersTimeline"
                                :items="dashboard.timelineItems"
                                height="100%"
                                class="gantt-table fill-height"
                                @click:row="(_, row) => goToProject(row.item.proj_id)">
            <template #item.proj_name="{ item }">
              <v-chip :color="dashboard.projectStatusColor(item.status)"
                      variant="tonal"
                      size="small">
                {{ item.proj_name }}
              </v-chip>
            </template>

            <template v-for="h in dashboard.additionalDateHeaders"
                      :key="h.value"
                      #[`item.${h.value}`]="{ item }">
              <v-tooltip v-if="item[h.value]" location="top">
                <template #activator="{ props }">
                  <div v-bind="props"
                       class="gantt-cell"
                       :class="item[h.value].state" />
                </template>
                {{ item[h.value].milestone_name }}
              </v-tooltip>
            </template>
          </v-data-table-virtual>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
  import { ref, computed } from 'vue'
  import { useRouter } from 'vue-router'
  import { useDashboardStore } from '@/stores/dashboard'

  const router = useRouter()
  const dashboard = useDashboardStore()

  const goToProject = projId => {
    router.push(`/projects/${projId}`)
  }

  const onCalendarClick = ({ event }) => {
    router.push(`/projects/${event.proj_id}`)
  }

  const formatDate = d =>
    new Date(d).toLocaleDateString()
</script>

<style scoped>
  .gantt-table {
    table-layout: fixed;
    height: 100%;
  }

  .v-data-table-virtual {
    height: calc(100% - 48px);
  }

  .gantt-cell {
    height: 18px;
    border-radius: 3px;
  }

  .on-track {
    background-color: #4caf50;
  }

  .delayed {
    background-color: #f44336;
  }

  :deep(th:first-child),
  :deep(td:first-child) {
    position: sticky;
    left: 0;
    background: white;
    z-index: 3;
  }
</style>
