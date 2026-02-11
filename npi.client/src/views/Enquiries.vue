<template>
  <v-container fluid fill-height class="d-flex flex-column">
    <v-row no-gutters align="center" justify="center">
      <v-col cols="12" class="text-center">
        <h2 class="font-weight-bold">
          <v-icon class="mr-2">mdi-clipboard-list-outline</v-icon>
          Sales Enquiries
        </h2>

        <v-btn color="primary" @click="$router.push('/enquiries/new')">
          <v-icon start>mdi-plus</v-icon>
          New Enquiry
        </v-btn>
      </v-col>
    </v-row>

    <v-row no-gutters class="flex-grow-1 flex-shrink-1" style="min-height: 0;">
      <v-col cols="12" class="pa-1">
        <v-card elevation="2">
          <v-card-text>
            <v-progress-linear v-if="enquiryStore.loading"
                               indeterminate
                               color="primary" />

            <v-data-table-virtual v-else
                                  :items="enquiryStore.enquiries"
                                  :headers="headers"
                                  item-key="enquiry_id">
              <template #item.status="{ item }">
                <v-chip :color="getStatusColor(item.status)"
                        size="small"
                        variant="tonal">
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
                <v-btn icon
                       variant="text"
                       @click="viewEnquiryPDF(item.enquiry_id)"
                       title="View PDF Preview">
                  <v-icon>mdi-eye</v-icon>
                </v-btn>

                <v-btn v-if="canManageProject(item)"
                       icon
                       variant="text"
                       color="primary"
                       @click="viewEnquiryDetail(item.enquiry_id)"
                       title="Manage Project">
                  <v-icon>mdi-clipboard-text</v-icon>
                </v-btn>

                <v-btn v-if="item.status === 'Draft'"
                       icon
                       variant="text"
                       @click="editEnquiry(item.enquiry_id)"
                       title="Edit">
                  <v-icon>mdi-pencil</v-icon>
                </v-btn>

                <v-btn v-if="item.status === 'Draft'"
                       icon
                       color="error"
                       variant="text"
                       @click="openDeleteDialog(item.enquiry_id)"
                       title="Delete">
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </template>
            </v-data-table-virtual>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-dialog v-model="showPdfDialog" max-width="900px" scrollable>
      <v-card>
        <v-card-title class="d-flex align-center justify-space-between bg-primary">
          <div class="text-h6 font-weight-bold">
            <v-icon class="mr-2">mdi-file-pdf-box</v-icon>
            Enquiry Preview
          </div>
          <div class="d-flex ga-2">
            <v-btn variant="text"
                   color="white"
                   @click="downloadCurrentPDF"
                   :loading="downloadingPDF">
              <v-icon start>mdi-download</v-icon>
              Download
            </v-btn>
            <v-btn variant="text"
                   color="white"
                   icon
                   @click="showPdfDialog = false">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </div>
        </v-card-title>

        <v-card-text class="pa-0" style="height: 80vh;">
          <iframe class="pdfViewer" v-if="pdfUrl"
                  :src="pdfUrl"
                  style="width: 100%; height: 100%; border: none;">
          </iframe>
          <div v-else class="d-flex align-center justify-center" style="height: 100%;">
            <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
          </div>
        </v-card-text>
      </v-card>
    </v-dialog>

    <v-dialog v-model="deleteDialog" max-width="400">
      <v-card>
        <v-card-title class="text-h6">Confirm Delete</v-card-title>
        <v-card-text>
          Are you sure you want to delete this enquiry? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="text" @click="confirmDelete">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

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
  import { useRouter } from 'vue-router'
  import { useEnquiryStore } from '@/stores/enquiry'
  import { useAuthStore } from '@/stores/auth'

  const router = useRouter()
  const enquiryStore = useEnquiryStore()
  const authStore = useAuthStore()

  const deleteDialog = ref(false)
  const deleteEnquiryId = ref(null)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  // PDF Preview
  const showPdfDialog = ref(false)
  const pdfUrl = ref(null)
  const currentEnquiryId = ref(null)
  const downloadingPDF = ref(false)

  const headers = [
    { title: 'Enquiry No', key: 'enquiry_no', sortable: true },
    { title: 'NPI Category', key: 'npi_category', sortable: true },
    { title: 'Status', key: 'status', sortable: true },
    { title: 'Created By', key: 'created_by', sortable: true },
    { title: 'Created Date', key: 'created_at', sortable: true },
    { title: 'Actions', key: 'actions', sortable: false, align: 'center' },
  ]

  const userRole = computed(() => authStore.user?.role)

  function canManageProject(enquiry) {
    // Show "Manage Project" button if:
    // 1. User is NPI Team or Admin
    // 2. Enquiry is Submitted or Approved (ready to start project)
    const isNpiOrAdmin = userRole.value === 'NPI Team' || userRole.value === 'Admin'
    const canStart = enquiry.status === 'Submitted' || enquiry.status === 'Approved'
    return isNpiOrAdmin && canStart
  }

  function getStatusColor(status) {
    const colors = {
      'Draft': 'warning',
      'Submitted': 'info',
      'Approved': 'success',
      'Rejected': 'error',
      'Pending': 'orange',
      'In Review': 'blue'
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

  async function viewEnquiryPDF(id) {
    currentEnquiryId.value = id
    showPdfDialog.value = true
    pdfUrl.value = null

    try {
      // Generate blob URL for PDF preview
      const response = await enquiryStore.getEnquiryPDFBlob(id)
      if (response.success) {
        pdfUrl.value = response.url
      } else {
        throw new Error(response.message)
      }
    } catch (error) {
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
    } catch (error) {
      snackbarMessage.value = 'Error downloading PDF'
      snackbarColor.value = 'error'
      snackbar.value = true
    } finally {
      downloadingPDF.value = false
    }
  }

  function viewEnquiryDetail(id) {
    router.push(`/enquiries/${id}/detail`)
  }

  function editEnquiry(id) {
    router.push(`/enquiries/${id}/edit`)
  }

  function openDeleteDialog(id) {
    deleteEnquiryId.value = id
    deleteDialog.value = true
  }

  async function confirmDelete() {
    deleteDialog.value = false

    const result = await enquiryStore.deleteEnquiry(deleteEnquiryId.value)

    if (result.success) {
      snackbarMessage.value = 'Enquiry deleted successfully'
      snackbarColor.value = 'success'
    } else {
      snackbarMessage.value = `Error: ${result.message}`
      snackbarColor.value = 'error'
    }

    snackbar.value = true
    deleteEnquiryId.value = null
  }

  onMounted(() => {
    enquiryStore.fetchMyEnquiries();
  })
</script>
