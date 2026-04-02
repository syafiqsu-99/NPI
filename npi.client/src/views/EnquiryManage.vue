<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="d-flex align-center justify-space-between bg-primary">
            <div class="text-h6 font-weight-bold text-white">
              <v-icon class="mr-2">mdi-file-document-outline</v-icon>
              Enquiry Details - {{ enquiry?.enquiry_no }}
            </div>
            <div class="d-flex ga-2">
              <v-btn variant="outlined" color="white"
                     @click="downloadPDF" :loading="downloadingPDF">
                <v-icon start>mdi-download</v-icon>
                Download PDF
              </v-btn>

              <!-- If project already exists → go to setup -->
              <v-btn v-if="enquiry?.proj_id"
                     variant="outlined" color="white"
                     @click="$router.push(`/projects/${enquiry.proj_id}/setup`)">
                <v-icon start>mdi-cog</v-icon>
                Manage Project
              </v-btn>

              <!-- Start project (no project yet, right role + status) -->
              <v-btn v-if="canStartProject"
                     color="success" variant="flat"
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
            <!-- Status / project link badges -->
            <v-row class="mb-4">
              <v-col cols="12" class="d-flex align-center ga-3 flex-wrap">
                <v-chip :color="getStatusColor(enquiry.status)" size="large">
                  {{ enquiry.status }}
                </v-chip>
                <v-chip v-if="enquiry.proj_id" color="success" size="small" variant="tonal">
                  <v-icon start size="14">mdi-briefcase-check</v-icon>
                  Project linked: {{ enquiry.proj_no || `#${enquiry.proj_id}` }}
                </v-chip>
              </v-col>
            </v-row>

            <!-- Customer Information -->
            <v-row>
              <v-col cols="12">
                <h3 class="text-h6 mb-3"><v-icon class="mr-2">mdi-account-box</v-icon>Customer Information</h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12" md="6"><strong>Company Name:</strong> {{ enquiry.customer_name || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Enquiry Date:</strong> {{ formatDate(enquiry.created_at) }}</v-col>
            </v-row>

            <!-- NPI Category -->
            <v-row class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3"><v-icon class="mr-2">mdi-tag</v-icon>NPI Category</h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12">
                <v-chip color="primary" size="large">{{ enquiry.npi_category }}</v-chip>
              </v-col>
            </v-row>

            <!-- General Information -->
            <v-row v-if="enquiry.generalInfo" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3"><v-icon class="mr-2">mdi-information</v-icon>General Information</h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12" md="6"><strong>Company Name:</strong> {{ enquiry.generalInfo.company_name || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Estimated Qty/Year:</strong> {{ enquiry.generalInfo.estimated_qty_per_year || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Required Date:</strong> {{ enquiry.generalInfo.estimated_required_date || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Color:</strong> {{ enquiry.generalInfo.color || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Material:</strong> {{ enquiry.generalInfo.material_used || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Weight (g):</strong> {{ enquiry.generalInfo.weight_g || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Neck Size:</strong> {{ enquiry.generalInfo.neck_size_mm || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Shape:</strong> {{ enquiry.generalInfo.shape || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Filling:</strong> {{ enquiry.generalInfo.hot_cold_filling || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>First Submission Qty:</strong> {{ enquiry.generalInfo.qty_first_submission || 'N/A' }}</v-col>
            </v-row>

            <!-- Seal Information -->
            <v-row v-if="enquiry.sealInfo" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3"><v-icon class="mr-2">mdi-seal</v-icon>Seal Information</h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12" md="6"><strong>Customer Name:</strong> {{ enquiry.sealInfo.customer_name || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Apply to Product:</strong> {{ enquiry.sealInfo.apply_to_product || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Required Date:</strong> {{ enquiry.sealInfo.estimated_required_date || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>Reason of Change:</strong> {{ enquiry.sealInfo.reason_of_change || 'N/A' }}</v-col>
              <v-col cols="12" md="6"><strong>First Submission Qty:</strong> {{ enquiry.sealInfo.qty_first_submission || 'N/A' }}</v-col>
              <v-col cols="12"><strong>Other Requirements:</strong> {{ enquiry.sealInfo.other_requirements || 'N/A' }}</v-col>
            </v-row>

            <!-- Customer Reference -->
            <v-row v-if="enquiry.customerRef" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3"><v-icon class="mr-2">mdi-file-tree</v-icon>Customer Reference</h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12" md="6"><strong>Mould Ownership:</strong> {{ enquiry.customerRef.mould_ownership || 'N/A' }}</v-col>
            </v-row>

            <!-- Files -->
            <v-row v-if="enquiry.files && enquiry.files.length > 0" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3"><v-icon class="mr-2">mdi-paperclip</v-icon>Attached Files</h3>
                <v-divider class="mb-4" />
              </v-col>
              <v-col cols="12">
                <v-list>
                  <v-list-item v-for="file in enquiry.files" :key="file.file_id">
                    <v-list-item-title>{{ file.file_name }}</v-list-item-title>
                    <template #append>
                      <v-btn icon size="small" variant="text"><v-icon>mdi-download</v-icon></v-btn>
                    </template>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- ── Start Project Dialog ─────────────────────────────────────────── -->
    <v-dialog v-model="showStartProjectDialog" max-width="640" persistent>
      <v-card rounded="xl">
        <v-card-title class="bg-primary pa-5">
          <div class="d-flex align-center ga-2 text-white">
            <v-icon>mdi-rocket-launch</v-icon>
            <div>
              <div class="text-subtitle-1 font-weight-bold">Start NPI Project</div>
              <div class="text-caption" style="opacity:0.8">{{ enquiry?.enquiry_no }}</div>
            </div>
          </div>
        </v-card-title>

        <v-card-text class="pa-6">
          <v-alert type="info" variant="tonal" density="compact" class="mb-5" rounded="lg">
            Review and edit the project details below. These can also be changed later in Project Setup.
          </v-alert>

          <v-row dense>
            <v-col cols="12">
              <v-text-field v-model="projectData.project_name"
                            label="Project Name *"
                            variant="outlined"
                            density="comfortable"
                            hint="Becomes the root folder name and Gantt title"
                            persistent-hint />
            </v-col>

            <v-col cols="12" md="6" class="mt-4">
              <v-select v-model="projectData.priority"
                        :items="['Low', 'Medium', 'High', 'Critical']"
                        label="Priority *"
                        variant="outlined"
                        density="comfortable" />
            </v-col>

            <v-col cols="12" md="6" class="mt-4">
              <v-text-field v-model="projectData.expected_completion"
                            label="Expected Completion Date"
                            type="date"
                            variant="outlined"
                            density="comfortable" />
            </v-col>

            <v-col cols="12" class="mt-4">
              <v-textarea v-model="projectData.description"
                          label="Project Description"
                          variant="outlined"
                          density="comfortable"
                          rows="3" />
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

  const projectData = ref({
    project_name: '',
    priority: 'Medium',
    expected_completion: '',
    description: ''
  })

  const canStartProject = computed(() => {
    if (!enquiry.value) return false
    if (enquiry.value.proj_id) return false
    const s = enquiry.value.status
    if (s !== 'Submitted' && s !== 'Approved') return false
    const role = authStore.user?.role
    return role === 'NPI Team' || role === 'Admin'
  })

  function getStatusColor(status) {
    return {
      'Draft': 'warning', 'Submitted': 'info', 'Approved': 'success',
      'Rejected': 'error', 'Started': 'primary', 'In Progress': 'blue', 'Completed': 'green'
    }[status] || 'grey'
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
    const company = e?.generalInfo?.company_name || e?.customer_name || ''
    const category = e?.npi_category || ''
    projectData.value = {
      project_name: [company, category].filter(Boolean).join(' - '),
      priority: 'Medium',
      description: '',
      expected_completion: e?.generalInfo?.estimated_required_date || ''
    }
    showStartProjectDialog.value = true
  }

  async function startProject() {
    if (!projectData.value.project_name?.trim()) {
      showSnack('Please enter a project name', 'error'); return
    }

    startingProject.value = true
    try {
      const result = await projectStore.createProjectFromEnquiry(route.params.id, {
        project_name: projectData.value.project_name.trim(),
        priority: projectData.value.priority,
        description: projectData.value.description || '',
        expected_completion: projectData.value.expected_completion || null,
      })

      if (result?.success && result?.data?.proj_id) {
        showSnack('Project created successfully!', 'success')
        showStartProjectDialog.value = false
        setTimeout(() => router.push(`/projects/${result.data.proj_id}/setup`), 800)
      } else {
        showSnack(result?.message || 'Failed to create project', 'error')
      }
    } catch (err) {
      showSnack(err?.message || 'Error creating project', 'error')
    } finally {
      startingProject.value = false
    }
  }

  function showSnack(msg, color = 'success') {
    snackbarMessage.value = msg; snackbarColor.value = color; snackbar.value = true
  }

  onMounted(async () => {
    loading.value = true
    const result = await enquiryStore.fetchEnquiryById(route.params.id)
    if (result?.success) enquiry.value = result.data
    loading.value = false
  })
</script>
