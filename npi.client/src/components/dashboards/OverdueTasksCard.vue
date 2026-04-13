<template>
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
      <v-list-item v-for="t in overdueTasks" :key="t.task_id"
                   @click="goToProject(t.proj_id)" style="cursor:pointer">
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
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  myTasks: { type: Array, required: true }
})

const router = useRouter()

const goToProject = id => router.push(`/projects/${id}/gantt`)

function formatDate(d) {
  if (!d) return 'N/A'
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
}

const overdueTasks = computed(() => {
  const today = new Date(); today.setHours(0, 0, 0, 0)
  return props.myTasks
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
</script>

<style scoped>
  .section-title {
    font-size: 0.9rem;
    font-weight: 600;
  }
</style>
