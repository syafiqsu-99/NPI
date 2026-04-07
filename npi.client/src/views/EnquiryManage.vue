<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="d-flex align-center justify-space-between bg-primary">
            <div class="text-h6 font-weight-bold text-white">
              <v-icon class="mr-2">mdi-file-document-outline</v-icon>
              Enquiry Details — {{ enquiry?.enquiry_no }}
            </div>
            <div class="d-flex ga-2">
              <v-btn variant="outlined" color="white" @click="downloadPDF" :loading="downloadingPDF">
                <v-icon start>mdi-download</v-icon>
                Download PDF
              </v-btn>

              <v-btn v-if="enquiry?.proj_id" variant="outlined" color="white"
                     @click="$router.push(`/projects/${enquiry.proj_id}/setup`)">
                <v-icon start>mdi-cog</v-icon>
                Manage Project
              </v-btn>

              <v-btn v-if="canStartProject" color="success" variant="flat"
                     @click="openStartProjectDialog">
                <v-icon start>mdi-rocket-launch</v-icon>
                Start Project
              </v-btn>

              <v-btn variant="text" color="white" @click="$router.back()">
                <v-icon start>mdi-arrow-left</v-icon>
                Back
              </v-btn>
            </div>
          </v-card-title>

          <v-card-text v-if="loading" class="text-center pa-8">
            <v-progress-circular indeterminate color="primary" size="64" />
          </v-card-text>

          <v-card-text v-else-if="enquiry" class="pa-6">

            <!-- Status + project link badge row -->
            <v-row class="mb-4">
              <v-col cols="12" class="d-flex align-center ga-3 flex-wrap">
                <v-chip :color="getStatusColor(enquiry.status)" size="large">
                  {{ enquiry.status }}
                </v-chip>
                <v-chip v-if="enquiry.proj_id" color="success" size="small" variant="tonal">
                  <v-icon start size="14">mdi-briefcase-check</v-icon>
                  Project: {{ enquiry.proj_no || `#${enquiry.proj_id}` }}
                </v-chip>
              </v-col>
            </v-row>

            <!-- Customer information -->
            <v-row>
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-account-box</v-icon>Customer Information
                </h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12" md="6"><strong>Company:</strong> {{ enquiry.customer_name || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Enquiry Date:</strong> {{ formatDate(enquiry.created_at) }}</v-col>
              <v-col cols="12" md="6"><strong>Submitted By:</strong> {{ enquiry.username }}</v-col>
            </v-row>

            <!-- NPI Category -->
            <v-row class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-tag</v-icon>NPI Category
                </h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12">
                <v-chip color="primary" size="large">{{ enquiry.npi_category }}</v-chip>
              </v-col>
            </v-row>

            <!-- ── Dynamic sections from field_values ─────────────────────
                 Each section renders using labels from the config store.
                 Falls back gracefully when field metadata has been deleted.
            -->
            <template v-for="(sectionValues, sectionKey) in enquiry.field_values"
                      :key="sectionKey">

              <v-row v-if="hasValues(sectionValues)" class="mt-4">
                <v-col cols="12">
                  <h3 class="text-h6 mb-3">
                    <v-icon class="mr-2">mdi-information</v-icon>
                    {{ getSectionLabel(sectionKey) }}
                  </h3>
                  <v-divider class="mb-4" />
                </v-col>
                <v-col v-for="(value, fieldKey) in sectionValues"
                       :key="fieldKey"
                       cols="12" md="6">
                  <span class="font-weight-medium">{{ getFieldLabel(sectionKey, fieldKey) }}:</span>
                  {{ value || 'N/A' }}
                </v-col>
              </v-row>
            </template>

            <!-- ── Customer Reference — always preserved ──────────────── -->
            <v-row v-if="enquiry.CustomerRef" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-file-tree</v-icon>Customer Reference
                </h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12" md="6">
                <strong>Mould Ownership:</strong>
                {{ enquiry.CustomerRef.mould_ownership || 'N/A' }}
              </v-col>
            </v-row>

            <!-- Files -->
            <v-row v-if="enquiry.Files?.length" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-paperclip</v-icon>Attached Files
                </h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12">
                <v-list>
                  <v-list-item v-for="file in enquiry.Files" :key="file.file_id">
                    <v-list-item-title>{{ file.file_name }}</v-list-item-title>
                    <template #append>
                      <v-btn icon size="small" variant="text">
                        <v-icon>mdi-download</v-icon>
                      </v-btn>
                    </template>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>

          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Start Project dialog (unchanged from original) -->
    <v-dialog v-model="showStartProjectDialog" max-width="640" persistent>
      <v-card rounded="xl">
        <v-card-title class="bg-primary pa-5">
          <div class="d-flex align-center ga-2 text-white">
            <v-icon>mdi-rocket-launch</v-icon>
            <div>
              <div class="text-subtitle-1 font-weight-bold">Start NPI Project</div>
              <div class="text-caption" style="opacity:.8">{{ enquiry?.enquiry_no }}</div>
            </div>
          </div>
        </v-card-title>
        <v-card-text class="pa-6">
          <v-alert type="info" variant="tonal" density="compact" class="mb-5" rounded="lg">
            Review project details. These can be changed later in Project Setup.
          </v-alert>
          <v-row dense>
            <v-col cols="12">
              <v-text-field v-model="projectData.project_name" label="Project Name *"
                            variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" md="6" class="mt-4">
              <v-select v-model="projectData.priority"
                        :items="['Low','Medium','High','Critical']"
                        label="Priority *" variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" md="6" class="mt-4">
              <v-text-field v-model="projectData.expected_completion"
                            label="Expected Completion" type="date"
                            variant="outlined" density="comfortable" />
            </v-col>
            <v-col cols="12" class="mt-4">
              <v-textarea v-model="projectData.description" label="Project Description"
                          variant="outlined" density="comfortable" rows="3" />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="pa-5 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="showStartProjectDialog = false">Cancel</v-btn>
          <v-btn color="success" variant="flat" rounded="lg"
                 :loading="startingProject"
                 :disabled="!projectData.project_name?.trim()"
                 @click="startProject">
            <v-icon start>mdi-check</v-icon>
            Create Project
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar" :color="snackbarColor" location="bottom right" rounded="lg">
      {{ snackbarMessage }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
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
  import { useNpiFormConfigStore } from '@/stores/npiFormConfig'

  const route = useRoute()
  const router = useRouter()
  const enquiryStore = useEnquiryStore()
  const projectStore = useProjectStore()
  const authStore = useAuthStore()
  const configStore = useNpiFormConfigStore()

  const loading = ref(false)
  const downloadingPDF = ref(false)
  const enquiry = ref(null)

  const showStartProjectDialog = ref(false)
  const startingProject = ref(false)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const projectData = ref({
    project_name: '', priority: 'Medium', expected_completion: '', description: ''
  })

  // ── Computed ──────────────────────────────────────────────────────────────────

  const canStartProject = computed(() => {
    if (!enquiry.value) return false
    if (enquiry.value.proj_id) return false
    const s = enquiry.value.status
    const role = authStore.user?.role
    return (s === 'Submitted' || s === 'Approved') &&
      (role === 'NPI Team' || role === 'Admin')
  })

  // ── Section / field label helpers ─────────────────────────────────────────────

  function getSectionLabel(sectionKey) {
    return configStore.sections.find(s => s.section_key === sectionKey)?.section_label
      ?? sectionKey
  }

  function getFieldLabel(sectionKey, fieldKey) {
    const section = configStore.sections.find(s => s.section_key === sectionKey)
    return section?.fields?.find(f => f.field_key === fieldKey)?.field_label
      ?? fieldKey.replace(/_/g, ' ')
  }

  function hasValues(sectionValues) {
    return Object.values(sectionValues).some(v => v !== null && v !== '')
  }

  // ── Other helpers ─────────────────────────────────────────────────────────────

  function getStatusColor(status) {
    return {
      Draft: 'warning', Submitted: 'info', Approved: 'success',
      Rejected: 'error', Started: 'primary', 'In Progress': 'blue', Completed: 'green'
    }[status] ?? 'grey'
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    })
  }

  async function downloadPDF() {
    downloadingPDF.value = true
    try { await enquiryStore.downloadEnquiryPDF(route.params.id) }
    finally { downloadingPDF.value = false }
  }

  function openStartProjectDialog() {
    const e = enquiry.value
    const company = e?.field_values?.generalInfo?.company_name
      ?? e?.customer_name
      ?? ''
    const category = e?.npi_category ?? ''
    const reqDate = e?.field_values?.generalInfo?.estimated_required_date
      ?? e?.field_values?.sealInfo?.estimated_required_date
      ?? ''

    projectData.value = {
      project_name: [company, category].filter(Boolean).join(' - '),
      priority: 'Medium',
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

  // ── Lifecycle ─────────────────────────────────────────────────────────────────

  onMounted(async () => {
    loading.value = true
    await configStore.fetchConfig()     // ensure labels are available for display
    const result = await enquiryStore.fetchEnquiryById(route.params.id)
    if (result?.success) enquiry.value = result.data
    loading.value = false
  })
</script>
