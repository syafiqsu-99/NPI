<template>
  <v-dialog :model-value="modelValue"
            @update:model-value="$emit('update:modelValue', $event)"
            max-width="400px">
    <v-card>
      <v-card-title class="bg-error">
        Confirm Delete
      </v-card-title>

      <v-card-text class="pt-4">
        Are you sure you want to delete
        "{{ docToDelete?.file_name }}"?
        This action cannot be undone.
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn variant="text"
               @click="$emit('update:modelValue', false)">
          Cancel
        </v-btn>
        <v-btn color="error"
               variant="elevated"
               :loading="deleting"
               @click="$emit('confirm')">
          Delete
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
  defineProps({
    modelValue: Boolean,
    docToDelete: Object,
    deleting: Boolean
  })

  defineEmits(['update:modelValue', 'confirm'])
</script>
