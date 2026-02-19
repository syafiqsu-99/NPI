<template>
  <v-dialog :model-value="modelValue"
            @update:model-value="$emit('update:modelValue', $event)"
            max-width="900px">
    <v-card v-if="selectedTask">
      <v-card-title class="bg-primary d-flex justify-space-between">
        {{ selectedTask.title }}
        <v-btn icon="mdi-close"
               variant="text"
               color="white"
               @click="$emit('update:modelValue', false)" />
      </v-card-title>

      <v-card-text class="pt-4">
        <v-row>
          <v-col cols="12" md="6">
            <v-card variant="outlined">
              <v-card-title>Task Information</v-card-title>
              <v-card-text>
                <div><strong>Status:</strong> {{ selectedTask.status }}</div>
                <div><strong>Priority:</strong> {{ selectedTask.priority }}</div>
                <div><strong>Department:</strong> {{ selectedTask.dept_name }}</div>
                <div><strong>Duration:</strong> {{ selectedTask.duration }} days</div>
                <v-progress-linear :model-value="selectedTask.per_complete || 0"
                                   height="20">
                  <strong>{{ selectedTask.per_complete || 0 }}%</strong>
                </v-progress-linear>
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="$emit('update:modelValue', false)">
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
    projects: Array
  })

  defineEmits(['update:modelValue'])
</script>
