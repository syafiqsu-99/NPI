<template>
  <v-container fluid class="page-container pa-4 pa-md-6 d-flex flex-column">

    <v-card class="page-card flex-grow-1 d-flex flex-column overflow-hidden" elevation="2" border>

      <!-- Header Actions -->
      <v-card-title class="bg-primary text-white d-flex align-center justify-space-between pa-4 flex-shrink-0">
        <div class="text-h6 font-weight-bold text-white">
          <v-icon class="mr-2">mdi-file-document-outline</v-icon>
          Enquiry Summary — {{ enquiry?.enquiry_no }}
        </div>
        <div class="d-flex ga-2">
          <v-btn variant="outlined" color="white" density="comfortable" @click="downloadPDF" :loading="downloadingPDF">
            <v-icon start>mdi-download</v-icon> Download PDF
          </v-btn>
          <v-btn v-if="enquiry?.proj_id" variant="outlined" color="white" density="comfortable"
                 @click="$router.push(`/projects/${enquiry.proj_id}/setup`)">
            <v-icon start>mdi-cog</v-icon> Manage Project
          </v-btn>
          <v-btn v-if="canStartProject" color="success" variant="flat" density="comfortable"
                 @click="openStartProjectDialog">
            <v-icon start>mdi-rocket-launch</v-icon> Start Project
          </v-btn>
          <v-btn variant="text" color="white" density="comfortable" @click="$router.back()">
            <v-icon start>mdi-arrow-left</v-icon> Back
          </v-btn>
        </div>
      </v-card-title>

      <!-- Loading State -->
      <v-card-text v-if="loading" class="text-center pa-8">
        <v-progress-circular indeterminate color="primary" size="64" />
      </v-card-text>

      <!-- Main Content -->
      <v-card-text v-else-if="enquiry" class="pa-4">

        <!-- General Summary -->
        <v-row class="mb-2" dense>
          <v-col cols="12" md="12" class="text-body-1">
            <v-icon size="small" color="primary" class="mr-1">mdi-domain</v-icon>
            <strong>Company:</strong> {{ enquiry.customer_name || 'N/A' }}
          </v-col>
          <v-col cols="12" md="12" class="text-body-1">
            <v-icon size="small" color="primary" class="mr-1">mdi-tag</v-icon>
            <strong>Category:</strong> {{ enquiry.form_category }}
          </v-col>
          <v-col cols="12" md="12" class="text-body-1">
            <v-icon size="small" color="primary" class="mr-1">mdi-calendar</v-icon>
            <strong>Date:</strong> {{ formatDate(enquiry.created_at) }}
          </v-col>
        </v-row>

        <v-divider class="my-3" />

        <!-- Dynamic Sections from field_values -->
        <template v-for="(sectionValues, sectionKey) in enquiry.field_values" :key="sectionKey">
          <v-row v-if="hasValues(sectionValues)" dense class="mb-4">
            <v-col cols="12" class="pb-1">
              <div class="text-subtitle-1 font-weight-bold text-primary">
                {{ getSectionLabel(sectionKey) }}
              </div>
            </v-col>
            <v-col v-for="(value, fieldKey) in sectionValues" :key="fieldKey" cols="12" sm="6" md="4" class="text-body-2 py-1">
              <span class="font-weight-medium text-grey-darken-2">{{ getFieldLabel(sectionKey, fieldKey) }}:</span>
              <span class="ml-1">{{ value || 'N/A' }}</span>
            </v-col>
          </v-row>
        </template>

        <!-- Customer Reference & Attached Files -->
        <v-row dense class="mt-2">
          <v-col cols="12" md="6" v-if="enquiry.CustomerRef">
            <div class="text-subtitle-1 font-weight-bold text-primary mb-1">Customer Reference</div>
            <div class="text-body-2">
              <span class="font-weight-medium text-grey-darken-2">Mould Ownership:</span>
              <span class="ml-1">{{ enquiry.CustomerRef.mould_ownership || 'N/A' }}</span>
            </div>
          </v-col>

          <v-col cols="12" md="6" v-if="enquiry.Files?.length">
            <div class="text-subtitle-1 font-weight-bold text-primary mb-1">Attached Files</div>
            <v-list density="compact" class="pa-0 bg-transparent">
              <v-list-item v-for="file in enquiry.Files" :key="file.file_id" class="px-0">
                <template #prepend>
                  <v-icon size="small" class="mr-2">mdi-paperclip</v-icon>
                </template>
                <v-list-item-title class="text-body-2">{{ file.file_name }}</v-list-item-title>
                <template #append>
                  <v-btn icon="mdi-download" size="x-small" variant="text" color="primary" />
                </template>
              </v-list-item>
            </v-list>
          </v-col>
        </v-row>

      </v-card-text>
    </v-card>

    <!-- Compacted Start Project Dialog -->
    <v-dialog v-model="showStartProjectDialog" max-width="500" persistent scrollable>
      <v-card rounded="xl">
        <v-card-title class="bg-primary py-3 px-4">
          <div class="d-flex align-center ga-2 text-white">
            <v-icon>mdi-rocket-launch</v-icon>
            <div>
              <div class="text-subtitle-1 font-weight-bold">Start NPI Project</div>
              <div class="text-caption" style="opacity:.8">{{ enquiry?.enquiry_no }}</div>
            </div>
          </div>
        </v-card-title>

        <v-card-text class="pa-4">
          <v-row dense>
            <v-col cols="12">
              <v-text-field v-model="projectData.project_name" label="Project Name *"
                            variant="outlined" density="compact" hide-details="auto" class="mb-3" />
            </v-col>
            <v-col cols="12" sm="6">
              <v-select v-model="projectData.priority" :items="priorityOptions"
                        label="Priority *" variant="outlined" density="compact" hide-details="auto" class="mb-3" />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field v-model="projectData.expected_completion" label="Expected Completion" type="date"
                            variant="outlined" density="compact" hide-details="auto" class="mb-3" />
            </v-col>
            <v-col cols="12">
              <v-textarea v-model="projectData.description" label="Project Description"
                          variant="outlined" density="compact" hide-details="auto" rows="2" />
            </v-col>
          </v-row>
        </v-card-text>

        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" density="comfortable" @click="showStartProjectDialog = false">Cancel</v-btn>
          <v-btn color="success" variant="flat" rounded="lg" density="comfortable"
                 :loading="startingProject" :disabled="!projectData.project_name?.trim()" @click="startProject">
            Create Project
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Global Snackbar -->
    <v-snackbar v-model="snackbar" :color="snackbarColor" location="bottom right" rounded="lg">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" density="compact" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useEnquiryStore } from '@/stores/enquiry'
  import { useProjectStore } from '@/stores/project'
  import { useAuthStore } from '@/stores/auth'
  import {
    ENQUIRY_STATUS_COLORS,
    PRIORITY_OPTIONS,
    DEFAULT_PRIORITY,
    DEFAULT_COLOR,
  } from '@/utils/constants'
  import { formatDate } from '@/utils/formatters'

  const route = useRoute()
  const router = useRouter()
  const enquiryStore = useEnquiryStore()
  const projectStore = useProjectStore()
  const authStore = useAuthStore()

  const loading = ref(false)
  const downloadingPDF = ref(false)
  const enquiry = ref(null)

  const showStartProjectDialog = ref(false)
  const startingProject = ref(false)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const priorityOptions = PRIORITY_OPTIONS

  const projectData = ref({
    project_name: '', priority: DEFAULT_PRIORITY, expected_completion: '', description: ''
  })

  // Start project conditional
  const canStartProject = computed(() =>
    !!enquiry.value && authStore.canStartProject(enquiry.value)
  )

  // Formatting & Mapping Helpers
  function getSectionLabel(sectionKey) {
    return (enquiryStore.sections ?? []).find(s => s.section_key === sectionKey)?.section_label ?? sectionKey
  }

  function getFieldLabel(sectionKey, fieldKey) {
    const section = (enquiryStore.sections ?? []).find(s => s.section_key === sectionKey)
    return section?.fields?.find(f => f.field_key === fieldKey)?.field_label ?? fieldKey.replace(/_/g, ' ')
  }

  function hasValues(sectionValues) {
    return Object.values(sectionValues).some(v => v !== null && v !== '')
  }

  // API Call Actions
  async function downloadPDF() {
    downloadingPDF.value = true
    try { await enquiryStore.downloadEnquiryPDF(route.params.id) }
    finally { downloadingPDF.value = false }
  }

  function openStartProjectDialog() {
    const e = enquiry.value
    const company = e?.field_values?.generalInfo?.company_name ?? e?.customer_name ?? ''
    const reqDate = e?.field_values?.generalInfo?.estimated_required_date ?? ''

    projectData.value = {
      project_name: [company, e?.form_category].filter(Boolean).join(' - '),
      priority: DEFAULT_PRIORITY,
      description: '',
      expected_completion: reqDate
    }
    showStartProjectDialog.value = true
  }

  async function startProject() {
    if (!projectData.value.project_name?.trim()) {
      showSnack('Please enter a project name.', 'error'); return
    }
    startingProject.value = true
    try {
      const result = await projectStore.createProjectFromEnquiry(route.params.id, {
        project_name: projectData.value.project_name.trim(),
        priority: projectData.value.priority,
        description: projectData.value.description || '',
        expected_completion: projectData.value.expected_completion || null
      })
      if (result?.success && result?.data?.proj_id) {
        showSnack('Project created successfully!', 'success')
        showStartProjectDialog.value = false
        setTimeout(() => router.push(`/projects/${result.data.proj_id}/setup`), 800)
      } else {
        showSnack(result?.message || 'Failed to create project.', 'error')
      }
    } catch (err) {
      showSnack(err?.message || 'Error creating project.', 'error')
    } finally {
      startingProject.value = false
    }
  }

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg; snackbarColor.value = color; snackbar.value = true
  }

  // Initialization
  onMounted(async () => {
    loading.value = true
    await enquiryStore.fetchConfig()
    const result = await enquiryStore.fetchEnquiryById(route.params.id)
    if (result?.success) enquiry.value = result.data
    loading.value = false
  })
</script>

<style scoped>
  .page-container {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
  }

  .page-card {
    border-radius: 8px;
  }
</style>
