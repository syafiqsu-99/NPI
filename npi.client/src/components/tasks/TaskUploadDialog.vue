<template>
  <v-dialog :model-value="modelValue"
            @update:model-value="$emit('update:modelValue', $event)"
            max-width="600px"
            persistent>
    <v-card>
      <v-card-title class="bg-primary">
        <v-icon class="mr-2">mdi-file-upload</v-icon>
        Upload Documents - {{ selectedTask?.title }}
      </v-card-title>

      <v-card-text class="pt-4">
        <v-file-input :model-value="uploadFiles"
                      @update:model-value="$emit('update:uploadFiles', $event)"
                      label="Select Files"
                      multiple
                      chips
                      show-size
                      prepend-icon="mdi-paperclip"
                      variant="outlined"
                      accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg">
          <template #selection="{ fileNames }">
            <v-chip v-for="(fileName, index) in fileNames"
                    :key="fileName"
                    size="small"
                    label
                    closable
                    @click:close="$emit('remove-file', index)"
                    class="mr-2 mb-2">
              {{ fileName }}
            </v-chip>
          </template>
        </v-file-input>

        <v-textarea :model-value="uploadDescription"
                    @update:model-value="$emit('update:uploadDescription', $event)"
                    label="Description (Optional)"
                    variant="outlined"
                    rows="3"
                    class="mt-4" />

        <v-alert type="info" variant="tonal" class="mt-4">
          <div class="text-caption">
            Allowed file types: PDF, Word, Excel, Images
          </div>
          <div class="text-caption">
            Maximum file size: 10MB per file
          </div>
        </v-alert>
      </v-card-text>

      <v-card-actions class="pa-4">
        <v-spacer />
        <v-btn variant="text" @click="$emit('close')">
          Cancel
        </v-btn>
        <v-btn color="primary"
               variant="elevated"
               :loading="uploading"
               :disabled="!uploadFiles || uploadFiles.length === 0"
               @click="$emit('upload')">
          Upload
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
  defineProps({
    modelValue: Boolean,
    selectedTask: Object,
    uploadFiles: Array,
    uploadDescription: String,
    uploading: Boolean
  })

  defineEmits([
    'update:modelValue',
    'update:uploadFiles',
    'update:uploadDescription',
    'remove-file',
    'upload',
    'close'
  ])
</script>
