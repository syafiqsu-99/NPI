<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="d-flex align-center justify-space-between bg-primary">
            <div class="text-h6 font-weight-bold">
              <v-icon class="mr-2">mdi-file-document-outline</v-icon>
              Enquiry Details - {{ enquiry?.enquiry_no }}
            </div>
            <div class="d-flex ga-2">
              <v-btn variant="outlined"
                     color="white"
                     @click="downloadPDF"
                     :loading="downloadingPDF">
                <v-icon start>mdi-download</v-icon>
                Download PDF
              </v-btn>

              <v-btn v-if="canStartProject"
                     color="success"
                     @click="showStartProjectDialog = true">
                <v-icon start>mdi-rocket-launch</v-icon>
                Start Project
              </v-btn>

              <v-btn variant="text"
                     color="white"
                     @click="$router.back()">
                <v-icon start>mdi-arrow-left</v-icon>
                Back
              </v-btn>
            </div>
          </v-card-title>

          <v-card-text v-if="loading" class="text-center pa-8">
            <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
          </v-card-text>

          <v-card-text v-else-if="enquiry" class="pa-6">
            <!-- Status Badge -->
            <v-row class="mb-4">
              <v-col cols="12">
                <v-chip :color="getStatusColor(enquiry.status)" size="large" class="mb-2">
                  {{ enquiry.status }}
                </v-chip>
              </v-col>
            </v-row>

            <!-- Customer Information -->
            <v-row>
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-account-box</v-icon>
                  Customer Information
                </h3>
                <v-divider class="mb-4"></v-divider>
              </v-col>

              <v-col cols="12" md="6">
                <strong>Company Name:</strong> {{ enquiry.customer_name || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Enquiry Date:</strong> {{ formatDate(enquiry.created_at) }}
              </v-col>
            </v-row>

            <!-- NPI Category -->
            <v-row class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-tag</v-icon>
                  NPI Category
                </h3>
                <v-divider class="mb-4"></v-divider>
              </v-col>

              <v-col cols="12">
                <v-chip color="primary" size="large">
                  {{ enquiry.npi_category }}
                </v-chip>
              </v-col>
            </v-row>

            <!-- General Information -->
            <v-row v-if="enquiry.generalInfo" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-information</v-icon>
                  General Information
                </h3>
                <v-divider class="mb-4"></v-divider>
              </v-col>

              <v-col cols="12" md="6">
                <strong>Company Name:</strong> {{ enquiry.generalInfo.company_name || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Estimated Qty/Year:</strong> {{ enquiry.generalInfo.estimated_qty_per_year || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Required Date:</strong> {{ enquiry.generalInfo.estimated_required_date || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Color:</strong> {{ enquiry.generalInfo.color || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Material:</strong> {{ enquiry.generalInfo.material_used || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Weight (g):</strong> {{ enquiry.generalInfo.weight_g || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Neck Size:</strong> {{ enquiry.generalInfo.neck_size_mm || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Shape:</strong> {{ enquiry.generalInfo.shape || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Filling:</strong> {{ enquiry.generalInfo.hot_cold_filling || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>First Submission Qty:</strong> {{ enquiry.generalInfo.qty_first_submission || 'N/A' }}
              </v-col>
            </v-row>

            <!-- Seal Information -->
            <v-row v-if="enquiry.sealInfo" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-seal</v-icon>
                  Seal Information
                </h3>
                <v-divider class="mb-4"></v-divider>
              </v-col>

              <v-col cols="12" md="6">
                <strong>Customer Name:</strong> {{ enquiry.sealInfo.customer_name || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Apply to Product:</strong> {{ enquiry.sealInfo.apply_to_product || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Required Date:</strong> {{ enquiry.sealInfo.estimated_required_date || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>Reason of Change:</strong> {{ enquiry.sealInfo.reason_of_change || 'N/A' }}
              </v-col>

              <v-col cols="12" md="6">
                <strong>First Submission Qty:</strong> {{ enquiry.sealInfo.qty_first_submission || 'N/A' }}
              </v-col>

              <v-col cols="12">
                <strong>Other Requirements:</strong> {{ enquiry.sealInfo.other_requirements || 'N/A' }}
              </v-col>
            </v-row>

            <!-- Customer Reference -->
            <v-row v-if="enquiry.customerRef" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-file-tree</v-icon>
                  Customer Reference
                </h3>
                <v-divider class="mb-4"></v-divider>
              </v-col>

              <v-col cols="12" md="6">
                <strong>Mould Ownership:</strong> {{ enquiry.customerRef.mould_ownership || 'N/A' }}
              </v-col>
            </v-row>

            <!-- Files -->
            <v-row v-if="enquiry.files && enquiry.files.length > 0" class="mt-4">
              <v-col cols="12">
                <h3 class="text-h6 mb-3">
                  <v-icon class="mr-2">mdi-paperclip</v-icon>
                  Attached Files
                </h3>
                <v-divider class="mb-4"></v-divider>
              </v-col>

              <v-col cols="12">
                <v-list>
                  <v-list-item v-for="file in enquiry.files" :key="file.file_id">
                    <v-list-item-title>{{ file.file_name }}</v-list-item-title>
                    <template v-slot:append>
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

    <!-- Start Project Dialog -->
    <v-dialog v-model="showStartProjectDialog" max-width="800" persistent>
      <v-card>
        <v-card-title class="bg-primary">
          <v-icon class="mr-2">mdi-rocket-launch</v-icon>
          Start Project - {{ enquiry?.enquiry_no }}
        </v-card-title>

        <v-card-text class="pa-6">
          <v-alert type="info" variant="tonal" class="mb-4">
            Starting a project will create the project structure, default tasks, and folder structure for file management.
          </v-alert>

          <v-row>
            <v-col cols="12">
              <v-text-field v-model="projectData.project_name"
                            label="Project Name *"
                            :rules="[v => !!v || 'Project name is required']"
                            required />
            </v-col>

            <v-col cols="12" md="6">
              <v-select v-model="projectData.priority"
                        :items="['Low', 'Medium', 'High', 'Critical']"
                        label="Priority *"
                        required />
            </v-col>

            <v-col cols="12" md="6">
              <v-text-field v-model="projectData.expected_completion"
                            label="Expected Completion Date"
                            type="date" />
            </v-col>

            <v-col cols="12">
              <v-textarea v-model="projectData.description"
                          label="Project Description"
                          rows="3" />
            </v-col>
          </v-row>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="showStartProjectDialog = false">
            Cancel
          </v-btn>
          <v-btn color="success"
                 :loading="startingProject"
                 @click="startProject">
            <v-icon start>mdi-check</v-icon>
            Create Project
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Success/Error Snackbar -->
    <v-snackbar v-model="snackbar" :color="snackbarColor">
      {{ snackbarMessage }}
      <template v-slot:actions>
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
  
    if (enquiry.value.status !== 'Submitted' && enquiry.value.status !== 'Approved') {
      return false
    }
  
    const userRole = authStore.user?.role
    return userRole === 'NPI Team' || userRole === 'Admin'
  })

  function getStatusColor(status) {
    const colors = {
      'Draft': 'warning',
      'Submitted': 'info',
      'Approved': 'success',
      'Rejected': 'error',
      'In Progress': 'blue',
      'Completed': 'green'
    }
    return colors[status] || 'grey'
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  async function startProject() {
    if (!projectData.value.project_name) {
      snackbarMessage.value = 'Please enter a project name'
      snackbarColor.value = 'error'
      snackbar.value = true
      return
    }

    startingProject.value = true
    try {
      const result = await projectStore.createProjectFromEnquiry(
        route.params.id,
        projectData.value
      )

      if (result?.success && result?.data?.proj_id) {
        snackbarMessage.value = 'Project created successfully!'
        snackbarColor.value = 'success'
        snackbar.value = true

        showStartProjectDialog.value = false

        setTimeout(() => {
          router.push(`/projects/${result.data.proj_id}/setup`)
        }, 1500)
      } else {
        snackbarMessage.value = result?.message || 'Failed to create project'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch (error) {
      console.error('Create project error:', error)
      snackbarMessage.value = error?.message || 'Error creating project'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      startingProject.value = false
    }
  }

  onMounted(async () => {
    loading.value = true
    const result = await enquiryStore.fetchEnquiryById(route.params.id)

    if (result && result.success) {
      enquiry.value = result.data

      // Pre-fill project name
      if (enquiry.value.generalInfo?.company_name) {
        projectData.value.project_name = `${enquiry.value.generalInfo.company_name} - ${enquiry.value.npi_category}`
      } else if (enquiry.value.customer_name) {
        projectData.value.project_name = `${enquiry.value.customer_name} - ${enquiry.value.npi_category}`
      }

      // Pre-fill expected completion from enquiry
      if (enquiry.value.generalInfo?.estimated_required_date) {
        projectData.value.expected_completion = enquiry.value.generalInfo.estimated_required_date
      }
    }

    loading.value = false
  })
</script>
