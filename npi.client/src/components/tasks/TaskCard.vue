<template>
  <v-card class="mb-2" variant="tonal">
    <v-card-title class="text-body-2">
      {{ task.title }}
    </v-card-title>

    <v-card-actions class="justify-space-between">
      <v-menu>
        <template #activator="{ props }">
          <v-chip v-bind="props"
                  size="small"
                  :disabled="!canEditTask(task)">
            {{ task.status }}
          </v-chip>
        </template>

        <v-list density="compact">
          <v-list-item v-for="status in statusOptions"
                       :key="status"
                       @click="$emit('update-status', status)">
            {{ status }}
          </v-list-item>
        </v-list>
      </v-menu>

      <div class="d-flex ga-1">
        <v-btn icon="mdi-upload"
               size="x-small"
               :disabled="!canEditTask(task)"
               @click="$emit('open-upload')" />
        <v-btn icon="mdi-file-document"
               size="x-small"
               @click="$emit('open-documents')" />
        <v-btn icon="mdi-eye"
               size="x-small"
               @click="$emit('open-details')" />
      </div>
    </v-card-actions>
  </v-card>
</template>

<script setup>
  defineProps({
    task: Object,
    canEditTask: Function,
    statusOptions: Array
  })
</script>
