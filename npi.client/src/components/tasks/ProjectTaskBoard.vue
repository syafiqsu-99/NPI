<template>
  <v-card elevation="2">
    <v-card-title class="bg-grey-lighten-3">
      <v-icon class="mr-2">mdi-folder</v-icon>
      {{ project.proj_name }}
    </v-card-title>

    <v-card-text>
      <v-row>
        <v-col v-for="status in statusOptions"
               :key="status"
               cols="12"
               md="2">
          <v-card variant="outlined" class="pa-2">
            <div class="text-subtitle-2 mb-2">
              {{ status }}
            </div>

            <TaskCard v-for="task in project.tasks.filter(t => t.status === status)"
                      :key="task.task_id"
                      :task="task"
                      :can-edit-task="canEditTask"
                      :status-options="statusOptions"
                      @update-status="$emit('update-status', task, $event)"
                      @open-upload="$emit('open-upload', task)"
                      @open-documents="$emit('open-documents', task)"
                      @open-details="$emit('open-details', task)" />
          </v-card>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script setup>
  import TaskCard from './TaskCard.vue'

  defineProps({
    project: Object,
    statusOptions: Array,
    canEditTask: Function
  })
</script>
