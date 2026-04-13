<template>
  <v-dialog v-model="isOpen" max-width="500" persistent>
    <v-card class="dialog-card">
      <v-card-text class="pa-6">
        <div class="d-flex align-center ga-4 mb-4">
          <div class="delete-icon-wrap">
            <v-icon color="error" size="26">mdi-folder-remove</v-icon>
          </div>
          <div>
            <div class="text-h6 font-weight-bold">Delete Project?</div>
            <div class="text-body-2 text-medium-emphasis">
              <strong>{{ project?.proj_no }}</strong> — {{ project?.proj_name }}
            </div>
          </div>
        </div>

        <v-alert type="error" variant="tonal" density="compact" class="mb-5">
          All tasks, team assignments, and stage data will be removed.
          Files on disk are <strong>not</strong> deleted.
        </v-alert>

        <div class="d-flex ga-2 justify-end">
          <v-btn variant="text" :disabled="loading" @click="isOpen = false">Cancel</v-btn>
          <v-btn color="error" variant="flat" rounded="lg" :loading="loading" @click="$emit('confirm')">
            <v-icon start>mdi-delete</v-icon>
            Delete Project
          </v-btn>
        </div>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: Boolean,
  project: Object,
  loading: Boolean
})
const emit = defineEmits(['update:modelValue', 'confirm'])

const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})
</script>

<style scoped>
  .delete-icon-wrap {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    background: rgba(var(--v-theme-error), 0.08);
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }
</style>
