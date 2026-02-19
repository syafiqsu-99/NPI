<template>
  <v-dialog :model-value="modelValue"
            @update:model-value="$emit('update:modelValue', $event)"
            max-width="800px">
    <v-card>
      <v-card-title class="bg-primary d-flex justify-space-between">
        <div>
          <v-icon class="mr-2">mdi-file-document-multiple</v-icon>
          Task Documents - {{ selectedTask?.title }}
        </div>
        <v-btn icon="mdi-close"
               variant="text"
               color="white"
               @click="$emit('update:modelValue', false)" />
      </v-card-title>

      <v-card-text class="pa-0">
        <v-list v-if="taskDocuments.length > 0">
          <v-list-item v-for="doc in taskDocuments"
                       :key="doc.file_id"
                       class="border-b">
            <template #prepend>
              <v-icon>mdi-file</v-icon>
            </template>

            <v-list-item-title>
              {{ doc.file_name }}
            </v-list-item-title>

            <template #append>
              <div class="d-flex ga-2">
                <v-btn icon="mdi-download"
                       size="small"
                       variant="text"
                       @click="$emit('download', doc)" />
                <v-btn icon="mdi-delete"
                       size="small"
                       variant="text"
                       color="error"
                       :disabled="!canEditTask(selectedTask)"
                       @click="$emit('delete', doc)" />
              </div>
            </template>
          </v-list-item>
        </v-list>

        <v-card-text v-else class="text-center pa-8">
          <v-icon size="48" color="grey">
            mdi-file-document-off
          </v-icon>
          <div class="text-caption text-grey mt-2">
            No documents uploaded yet
          </div>
        </v-card-text>
      </v-card-text>

      <v-card-actions class="pa-4">
        <v-btn color="primary"
               variant="text"
               prepend-icon="mdi-upload"
               :disabled="!canEditTask(selectedTask)"
               @click="$emit('upload-more')">
          Upload More
        </v-btn>
        <v-spacer />
        <v-btn variant="text"
               @click="$emit('update:modelValue', false)">
          Close
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
  defineProps({
    modelValue: Boolean,
    selectedTask: Object,
    taskDocuments: Array,
    canEditTask: Function
  })

  defineEmits([
    'update:modelValue',
    'download',
    'delete',
    'upload-more'
  ])
</script>

<style scoped>
  .border-b {
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
  }
</style>
