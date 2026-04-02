<template>
  <v-container fluid class="page-container pa-6 d-flex flex-column">
    <v-row class="mb-4">
      <v-col cols="12">
        <v-card class="page-header-card" elevation="2">
          <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
            <div class="page-title">
              <v-icon class="mr-2">mdi-clipboard-list-outline</v-icon>
              Sales Enquiries
            </div>

            <div class="d-flex align-center ga-2">
              <v-btn color="primary" variant="flat" @click="$router.push('/enquiries/new')">
                <v-icon start>mdi-plus</v-icon>
                New Enquiry
              </v-btn>
            </div>
          </v-card-title>
          <v-card-text class="pa-0">
            <v-card class="page-card d-flex flex-column">
              <v-card-text class="pb-2">
                <div class="d-flex align-center ga-4 flex-wrap">
                  <span class="text-caption text-medium-emphasis font-weight-medium">
                    Project:
                  </span>

                  <div class="d-flex align-center ga-1">
                    <v-icon size="15" color="success">mdi-briefcase-check</v-icon>
                    <span class="text-caption">Exists</span>
                  </div>

                  <div class="d-flex align-center ga-1">
                    <v-icon size="15" color="grey-lighten-1">mdi-briefcase-outline</v-icon>
                    <span class="text-caption">Not started</span>
                  </div>
                </div>
              </v-card-text>

              <v-divider />

              <!-- Loading -->
              <v-progress-linear v-if="enquiryStore.loading"
                                  indeterminate
                                  color="primary" />

              <!-- Table -->
              <v-card-text class="pa-0 flex-grow-1">
                <v-data-table-virtual v-if="!enquiryStore.loading"
                                      :items="enquiryStore.enquiries"
                                      :headers="headers"
                                      item-key="enquiry_id"
                                      fixed-header
                                      height="500"
                                      class="enquiry-table">
                  <!-- ── Project indicator ── -->
                  <template #item.project_indicator="{ item }">
                    <div class="d-flex justify-center">
                      <v-tooltip :text="item.proj_id ? `Project exists${item.proj_no ? ': ' + item.proj_no : ''}` : 'No project yet'"
                                  location="top">
                        <template #activator="{ props }">
                          <v-icon v-bind="props"
                                  :color="item.proj_id ? 'success' : 'grey-lighten-2'"
                                  size="19">
                            {{ item.proj_id ? 'mdi-briefcase-check' : 'mdi-briefcase-outline' }}
                          </v-icon>
                        </template>
                      </v-tooltip>
                    </div>
                  </template>

                  <template #item.enquiry_no="{ item }">
                    <span class="font-weight-medium">{{ item.enquiry_no }}</span>
                  </template>

                  <template #item.status="{ item }">
                    <v-chip :color="getStatusColor(item.status)" size="small" variant="tonal">
                      {{ item.status }}
                    </v-chip>
                  </template>

                  <template #item.created_by="{ item }">
                    {{ item.created_by || 'Unknown' }}
                  </template>

                  <template #item.created_at="{ item }">
                    {{ formatDate(item.created_at) }}
                  </template>

                  <template #item.actions="{ item }">
                    <!-- View PDF -->
                    <v-btn icon size="small" variant="text"
                            @click="viewEnquiryPDF(item.enquiry_id)"
                            title="View PDF Preview">
                      <v-icon size="18">mdi-eye</v-icon>
                    </v-btn>

                    <!-- Go to Gantt if project exists -->
                    <v-btn v-if="item.proj_id"
                            icon size="small" variant="text" color="success"
                            @click="$router.push(`/projects/${item.proj_id}/gantt`)"
                            title="View Project Gantt">
                      <v-icon size="18">mdi-chart-gantt</v-icon>
                    </v-btn>

                    <!-- Start project (NPI/Admin, no project yet, right status) -->
                    <v-btn v-if="canManageProject(item) && !item.proj_id"
                            icon size="small" variant="text" color="primary"
                            @click="viewEnquiryDetail(item.enquiry_id)"
                            title="Start Project">
                      <v-icon size="18">mdi-rocket-launch</v-icon>
                    </v-btn>

                    <!-- Manage existing project setup -->
                    <v-btn v-if="canManageProject(item) && item.proj_id"
                            icon size="small" variant="text" color="primary"
                            @click="$router.push(`/projects/${item.proj_id}/setup`)"
                            title="Manage Project Setup">
                      <v-icon size="18">mdi-cog</v-icon>
                    </v-btn>

                    <!-- Edit (Draft only) -->
                    <v-btn v-if="item.status === 'Draft'"
                            icon size="small" variant="text"
                            @click="editEnquiry(item.enquiry_id)"
                            title="Edit">
                      <v-icon size="18">mdi-pencil</v-icon>
                    </v-btn>

                    <!-- Delete: Draft OR Admin -->
                    <v-btn v-if="canDelete(item)"
                            icon size="small" variant="text" color="error"
                            @click="openDeleteDialog(item)"
                            title="Delete Enquiry">
                      <v-icon size="18">mdi-delete</v-icon>
                    </v-btn>
                  </template>
                </v-data-table-virtual>
              </v-card-text>
            </v-card>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>


    <!-- ── PDF Dialog ── -->
    <v-dialog v-model="showPdfDialog" max-width="600" persistent scrollable>
      <v-card class="dialog-card">
        <v-card-title class="d-flex align-center justify-space-between bg-primary">
          <div class="text-h6 font-weight-bold text-white">
            <v-icon class="mr-2">mdi-file-pdf-box</v-icon>
            Enquiry Preview
          </div>
          <div class="d-flex ga-2">
            <v-btn variant="text" color="white" @click="downloadCurrentPDF" :loading="downloadingPDF">
              <v-icon start>mdi-download</v-icon>
              Download
            </v-btn>
            <v-btn variant="text" color="white" icon @click="showPdfDialog = false">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </div>
        </v-card-title>
        <v-card-text class="pa-0" style="height: 80vh;">
          <iframe v-if="pdfUrl" :src="pdfUrl" style="width: 100%; height: 100%; border: none;" />
          <div v-else class="d-flex align-center justify-center" style="height: 100%;">
            <v-progress-circular indeterminate color="primary" size="64" />
          </div>
        </v-card-text>
      </v-card>
    </v-dialog>

    <!-- ── Delete Confirmation Dialog ── -->
    <v-dialog v-model="deleteDialog" max-width="600" persistent>
      <v-card class="dialog-card">
        <v-card-text class="pa-6">
          <div class="d-flex align-center ga-4 mb-3">
            <div class="delete-icon-wrap">
              <v-icon color="error" size="26">mdi-delete-alert</v-icon>
            </div>
            <div>
              <div class="text-subtitle-1 font-weight-bold">Delete Enquiry?</div>
              <div class="text-body-2 text-medium-emphasis">
                <strong>{{ deleteTarget?.enquiry_no }}</strong> will be permanently removed.
                This cannot be undone.
              </div>
            </div>
          </div>

          <!-- Extra warning if the enquiry already has a linked project -->
          <v-alert v-if="deleteTarget?.proj_id"
                   type="warning" variant="tonal" density="compact" class="mb-2">
            This enquiry has a linked project. Deleting the enquiry will
            <strong>not</strong> delete the project.
          </v-alert>

          <div class="d-flex ga-2 justify-end mt-4">
            <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
            <v-btn color="error" variant="flat" rounded="lg"
                   :loading="deleting" @click="confirmDelete">
              <v-icon start>mdi-delete</v-icon>
              Delete
            </v-btn>
          </div>
        </v-card-text>
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
  import { useRouter } from 'vue-router'
  import { useEnquiryStore } from '@/stores/enquiry'
  import { useAuthStore } from '@/stores/auth'

  const router = useRouter()
  const enquiryStore = useEnquiryStore()
  const authStore = useAuthStore()

  const deleteDialog = ref(false)
  const deleteTarget = ref(null)   // full enquiry object (need enquiry_no + proj_id for the dialog)
  const deleting = ref(false)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const showPdfDialog = ref(false)
  const pdfUrl = ref(null)
  const currentEnquiryId = ref(null)
  const downloadingPDF = ref(false)

  const headers = [
    { title: '', key: 'project_indicator', sortable: false, align: 'center', width: '52px' },
    { title: 'Enquiry No', key: 'enquiry_no', sortable: true },
    { title: 'NPI Category', key: 'npi_category', sortable: true },
    { title: 'Status', key: 'status', sortable: true },
    { title: 'Created By', key: 'created_by', sortable: true },
    { title: 'Created Date', key: 'created_at', sortable: true },
    { title: 'Actions', key: 'actions', sortable: false, align: 'center', width: '190px' },
  ]

  const userRole = computed(() => authStore.user?.role)

  function canManageProject(enquiry) {
    const isNpiOrAdmin = userRole.value === 'NPI Team' || userRole.value === 'Admin'
    const canStart = enquiry.status === 'Submitted' || enquiry.status === 'Approved' || enquiry.status === 'Started'
    return isNpiOrAdmin && canStart
  }

  // Draft → anyone; non-Draft → Admin only
  function canDelete(enquiry) {
    if (userRole.value === 'Admin') return true
    return enquiry.status === 'Draft'
  }

  function getStatusColor(status) {
    const colors = {
      'Draft': 'warning', 'Submitted': 'info', 'Approved': 'success',
      'Rejected': 'error', 'Started': 'primary', 'Pending': 'orange', 'In Review': 'blue'
    }
    return colors[status] || 'grey'
  }

  function formatDate(date) {
    if (!date) return 'N/A'
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    })
  }

  async function viewEnquiryPDF(id) {
    currentEnquiryId.value = id
    showPdfDialog.value = true
    pdfUrl.value = null
    try {
      const response = await enquiryStore.getEnquiryPDFBlob(id)
      if (response.success) { pdfUrl.value = response.url }
      else throw new Error(response.message)
    } catch {
      snackbarMessage.value = 'Error loading PDF preview'
      snackbarColor.value = 'error'
      snackbar.value = true
      showPdfDialog.value = false
    }
  }

  async function downloadCurrentPDF() {
    if (!currentEnquiryId.value) return
    downloadingPDF.value = true
    try {
      const result = await enquiryStore.downloadEnquiryPDF(currentEnquiryId.value)
      if (!result.success) {
        snackbarMessage.value = 'Failed to download PDF'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } catch {
      snackbarMessage.value = 'Error downloading PDF'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally { downloadingPDF.value = false }
  }

  function viewEnquiryDetail(id) { router.push(`/enquiries/${id}/detail`) }
  function editEnquiry(id) { router.push(`/enquiries/${id}/edit`) }

  function openDeleteDialog(enquiry) {
    deleteTarget.value = enquiry
    deleteDialog.value = true
  }

  async function confirmDelete() {
    if (!deleteTarget.value) return
    deleting.value = true
    const result = await enquiryStore.deleteEnquiry(deleteTarget.value.enquiry_id)
    deleting.value = false
    deleteDialog.value = false
    snackbarMessage.value = result.success ? 'Enquiry deleted successfully' : `Error: ${result.message}`
    snackbarColor.value = result.success ? 'success' : 'error'
    snackbar.value = true
    deleteTarget.value = null
  }

  onMounted(() => { enquiryStore.fetchEnquiries() })
</script>

<style scoped>
  .dashboard-container {
    height: 100vh;
    overflow: hidden;
    background: #f7f9fb;
  }

  .dashboard-card {
    border-radius: 16px;
    display: flex;
    flex-direction: column;
  }

  .section-title {
    font-weight: 600;
    font-size: 1rem;
  }

  .enquiry-table :deep(.v-table__wrapper) {
    overflow-y: auto;
  }

  .enquiry-table :deep(tbody tr:hover) {
    background-color: rgba(0, 0, 0, 0.04);
  }

  .enquiry-table :deep(th) {
    font-size: 11px !important;
    font-weight: 700 !important;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    color: rgba(0,0,0,0.5) !important;
  }

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
