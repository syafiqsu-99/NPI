<template>
  <v-card-text class="pa-4">
    <v-row>
      <v-col cols="12" sm="6" md="3">
        <v-card variant="tonal" color="grey">
          <v-card-text>
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h4 font-weight-bold">{{ taskStats?.total ?? 0 }}</div>
                <div class="text-caption">Total Tasks</div>
              </div>
              <v-icon size="48" color="grey-darken-2">mdi-clipboard-list</v-icon>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card variant="tonal" color="blue">
          <v-card-text>
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h4 font-weight-bold">{{ taskStats?.inProgress ?? 0 }}</div>
                <div class="text-caption">In Progress</div>
              </div>
              <v-icon size="48" color="blue-darken-2">mdi-play-circle</v-icon>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card variant="tonal" color="success">
          <v-card-text>
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h4 font-weight-bold">{{ taskStats?.completed ?? 0 }}</div>
                <div class="text-caption">Completed</div>
              </div>
              <v-icon size="48" color="success-darken-2">mdi-check-circle</v-icon>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card variant="tonal" color="warning">
          <v-card-text>
            <div class="d-flex align-center justify-space-between">
              <div>
                <div class="text-h4 font-weight-bold">{{ taskStats?.overdue ?? 0 }}</div>
                <div class="text-caption">Overdue</div>
              </div>
              <v-icon size="48" color="warning-darken-2">mdi-alert-circle</v-icon>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-card-text>
</template>
<script setup>
defineProps({
  taskStats: {
    type: Object,
    required: true,
    default: () => ({
      total: 0,
      inProgress: 0,
      completed: 0,
      overdue: 0
    })
  },
  refreshTasks: {
    type: Function,
    required: true
  }
})
</script>
