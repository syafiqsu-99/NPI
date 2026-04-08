<template>
  <!-- Strict 100vh Root Container -->
  <v-container fluid class="page-container pa-4 pa-md-6 d-flex flex-column">

    <!-- Main Expanding Card -->
    <v-card class="page-card flex-grow-1 d-flex flex-column overflow-hidden" elevation="2" border>

      <!-- Fixed Header Strip -->
      <v-card-title class="bg-primary text-white d-flex align-center justify-space-between pa-4 flex-shrink-0">
        <div class="text-h6 font-weight-bold d-flex align-center">
          <v-icon start>mdi-clipboard-list-outline</v-icon>
          Sales Enquiries
        </div>
        <v-btn color="white" variant="elevated" class="text-primary font-weight-bold" @click="$router.push('/enquiries/new')">
          <v-icon start>mdi-plus</v-icon>
          New Enquiry
        </v-btn>
      </v-card-title>

      <!-- Legend/Filter Strip (Fixed) -->
      <v-card-text class="pa-3 flex-shrink-0 bg-grey-lighten-4">
        <div class="d-flex align-center ga-4 flex-wrap">
          <span class="text-caption text-medium-emphasis font-weight-medium">
            Project:
          </span>
          <div class="d-flex align-center ga-1">
            <v-icon size="16" color="success">mdi-briefcase-check</v-icon>
            <span class="text-caption">Exists</span>
          </div>
          <div class="d-flex align-center ga-1">
            <v-icon size="16" color="grey">mdi-briefcase-outline</v-icon>
            <span class="text-caption">Not started</span>
          </div>
        </div>
      </v-card-text>

      <v-divider class="flex-shrink-0" />

      <!-- Loading Bar -->
      <v-progress-linear v-if="enquiryStore.loading" indeterminate color="primary" class="flex-shrink-0" />

      <!-- Scrollable Virtual Table Area -->
      <div class="table-wrapper flex-grow-1 position-relative overflow-hidden d-flex flex-column" v-if="!enquiryStore.loading">
        <v-data-table-virtual :items="enquiryStore.enquiries"
                              :headers="headers"
                              item-key="enquiry_id"
                              fixed-header
                              height="100%"
                              class="enquiry-table flex-grow-1">

          <!-- ── Project indicator ── -->
          <template #item.project_indicator="{ item }">
            <div class="d-flex justify-center">
              <v-tooltip :text="item.proj_id ? `Project exists${item.proj_no ? ': ' + item.proj_no : ''}` : 'No project yet'"
                         location="top">
                <template #activator="{ props }">
                  <v-icon v-bind="props"
                          :color="item.proj_id ? 'success' : 'grey-lighten-1'"
                          size="20">
                    {{ item.proj_id ? 'mdi-briefcase-check' : 'mdi-briefcase-outline' }}
                  </v-icon>
                </template>
              </v-tooltip>
            </div>
          </template>

          <template #item.enquiry_no="{ item }">
            <span class="font-weight-medium text-primary">{{ item.enquiry_no }}</span>
          </template>

          <template #item.status="{ item }">
            <v-chip :color="getStatusColor(item.status)" size="small" variant="tonal" class="font-weight-medium">
              {{ item.status }}
            </v-chip>
          </template>

          <!-- Display User Name instead of ID -->
          <template #item.created_by="{ item }">
            {{ item.username || item.created_by }}
          </template>

          <template #item.created_at="{ item }">
            {{ formatDate(item.created_at) }}
          </template>

          <template #item.actions="{ item }">
            <div class="d-flex justify-center ga-1">
              <!-- View PDF -->
              <v-btn icon size="small" variant="text" color="grey-darken-2" @click="viewEnquiryPDF(item.enquiry_id)" title="View PDF Preview">
                <v-icon size="18">mdi-eye</v-icon>
              </v-btn>

              <!-- Go to Gantt if project exists -->
              <v-btn v-if="item.proj_id" icon size="small" variant="text" color="success" @click="$router.push(`/projects/${item.proj_id}/gantt`)" title="View Project Gantt">
                <v-icon size="18">mdi-chart-gantt</v-icon>
              </v-btn>

              <!-- Start project (Manager/Admin, no project yet, right status) -->
              <v-btn v-if="canManageProject(item) && !item.proj_id" icon size="small" variant="text" color="primary" @click="viewEnquiryDetail(item.enquiry_id)" title="Start Project">
                <v-icon size="18">mdi-rocket-launch</v-icon>
              </v-btn>

              <!-- Manage existing project setup -->
              <v-btn v-if="canManageProject(item) && item.proj_id" icon size="small" variant="text" color="primary" @click="$router.push(`/projects/${item.proj_id}/setup`)" title="Manage Project Setup">
                <v-icon size="18">mdi-cog</v-icon>
              </v-btn>

              <!-- Edit (Draft only) -->
              <v-btn v-if="item.status === 'Draft'" icon size="small" variant="text" color="info" @click="editEnquiry(item.enquiry_id)" title="Edit">
                <v-icon size="18">mdi-pencil</v-icon>
              </v-btn>

              <!-- Delete: Draft OR Admin -->
              <v-btn v-if="canDelete(item)" icon size="small" variant="text" color="error" @click="openDeleteDialog(item)" title="Delete Enquiry">
                <v-icon size="18">mdi-delete</v-icon>
              </v-btn>
            </div>
          </template>
        </v-data-table-virtual>
      </div>

    </v-card>

    <!-- ── PDF Dialog ── -->
    <v-dialog v-model="showPdfDialog" max-width="800" persistent scrollable>
      <v-card class="dialog-card">
        <v-card-title class="d-flex align-center justify-space-between bg-primary">
          <div class="text-h6 font-weight-bold text-white">
            <v-icon start>mdi-file-pdf-box</v-icon>
            Enquiry Preview
          </div>
          <div class="d-flex ga-2">
            <v-btn variant="elevated" color="white" class="text-primary font-weight-bold" @click="downloadCurrentPDF" :loading="downloadingPDF">
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
          <div v-else class="d-flex align-center justify-center fill-height">
            <v-progress-circular indeterminate color="primary" size="64" />
          </div>
        </v-card-text>
      </v-card>
    </v-dialog>

    <!-- ── Delete Confirmation Dialog ── -->
    <v-dialog v-model="deleteDialog" max-width="500" persistent>
      <v-card class="dialog-card rounded-lg">
        <v-card-text class="pa-6">
          <div class="d-flex align-start ga-4 mb-3">
            <div class="delete-icon-wrap">
              <v-icon color="error" size="28">mdi-delete-alert</v-icon>
            </div>
            <div>
              <div class="text-h6 font-weight-bold mb-1">Delete Enquiry?</div>
              <div class="text-body-2 text-medium-emphasis">
                <strong>{{ deleteTarget?.enquiry_no }}</strong> will be permanently removed. This action cannot be undone.
              </div>
            </div>
          </div>

          <!-- Extra warning if the enquiry already has a linked project -->
          <v-alert v-if="deleteTarget?.proj_id" type="warning" variant="tonal" density="compact" class="mt-4 mb-2">
            This enquiry has a linked project. Deleting the enquiry will <strong>not</strong> delete the project.
          </v-alert>

          <div class="d-flex ga-2 justify-end mt-6">
            <v-btn variant="text" color="grey-darken-1" @click="deleteDialog = false">Cancel</v-btn>
            <v-btn color="error" variant="flat" :loading="deleting" @click="confirmDelete">
              <v-icon start>mdi-delete</v-icon>
              Confirm Delete
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
  const deleteTarget = ref(null)
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
    const isAdmin = userRole.value === 'Manager' || userRole.value === 'Admin'
    const canStart = enquiry.status === 'Submitted' || enquiry.status === 'Approved' || enquiry.status === 'Started'
    return isAdmin && canStart
  }

  function canDelete(enquiry) {
    if (userRole.value === 'Admin' || userRole.value === 'Manager') return true
    return enquiry.status === 'Draft'
  }

  function getStatusColor(status) {
    const colors = {
      'Draft': 'warning', 'Submitted': 'info', 'Approved': 'success',
      'Rejected': 'error', 'Started': 'primary', 'Pending': 'orange', 'In Review': 'blue',
      'Completed': 'success'
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
  /* Strict 100vh Layout Constraints */
  .page-container {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
  }

  .page-card {
    border-radius: 8px;
  }

  /* Ensure the wrapper scales and hides overflow so internal scroll works */
  .table-wrapper {
    height: 100%;
  }

  .enquiry-table :deep(.v-table__wrapper) {
    height: 100% !important;
    overflow-y: auto;
  }

  .enquiry-table :deep(tbody tr) {
    transition: background-color 0.2s ease;
  }

  .enquiry-table :deep(tbody tr:hover) {
    background-color: rgba(0, 0, 0, 0.03);
  }

  .enquiry-table :deep(th) {
    font-size: 11px !important;
    font-weight: 700 !important;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    color: rgba(0, 0, 0, 0.6) !important;
    background-color: #ffffff !important;
    border-bottom: 2px solid #e0e0e0 !important;
  }

  .delete-icon-wrap {
    width: 56px;
    height: 56px;
    border-radius: 12px;
    background: rgba(var(--v-theme-error), 0.1);
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }
</style>
