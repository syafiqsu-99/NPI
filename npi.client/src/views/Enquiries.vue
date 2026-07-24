<template>
  <v-container fluid class="page-container pa-4 pa-md-6 d-flex flex-column">
    <v-card class="page-card flex-grow-1 d-flex flex-column overflow-hidden" elevation="2" border>

      <v-card-title class="bg-primary text-white d-flex align-center justify-space-between pa-4 flex-shrink-0">
        <div class="text-h6 font-weight-bold d-flex align-center">
          <v-icon start>mdi-clipboard-list-outline</v-icon>
          Sales Enquiries
        </div>
        <v-btn v-if="authStore.canCreateEnquiry"
               color="white" variant="elevated"
               class="text-primary font-weight-bold"
               @click="$router.push('/enquiries/new')">
          <v-icon start>mdi-plus</v-icon>
          New Enquiry
        </v-btn>
      </v-card-title>

      <v-card-text class="pa-3 flex-shrink-0 bg-grey-lighten-4">
        <div class="d-flex align-center ga-4 flex-wrap">
          <span class="text-caption text-medium-emphasis font-weight-medium">Project:</span>
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

      <v-progress-linear v-if="enquiryStore.loading" indeterminate color="primary" class="flex-shrink-0" />

      <div class="table-wrapper flex-grow-1 position-relative overflow-hidden d-flex flex-column"
           v-if="!enquiryStore.loading">
        <v-data-table-virtual :items="enquiryStore.enquiries"
                              :headers="headers"
                              item-key="enquiry_id"
                              fixed-header
                              height="100%"
                              class="enquiry-table clickable-rows flex-grow-1"
                              :row-props="({ item }) => ({
                                class: item.enquiry_id === highlightId ? 'row-highlight' : ''
                              })"
                              @click:row="(_, { item }) => viewEnquiryPDF(item)">

          <template #item.project_indicator="{ item }">
            <div class="d-flex justify-center">
              <v-tooltip :text="item.proj_id
                  ? `Project exists${item.proj_no ? ': ' + item.proj_no : ''}`
                  : 'No project yet'"
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
            <v-tooltip v-if="item.latest_review_remark" location="top" max-width="360">
              <template #activator="{ props }">
                <v-chip v-bind="props" :color="getStatusColor(item.status)" size="small"
                        variant="tonal" class="font-weight-medium">
                  <v-icon start size="14">mdi-comment-alert-outline</v-icon>
                  {{ item.status }}
                </v-chip>
              </template>
              <div class="text-caption font-weight-bold mb-1">
                {{ reviewDecisionLabel(item.latest_review_decision) }}
              </div>
              <div class="text-body-2">{{ item.latest_review_remark }}</div>
            </v-tooltip>
            <v-chip v-else :color="getStatusColor(item.status)" size="small"
                    variant="tonal" class="font-weight-medium">
              {{ item.status }}
            </v-chip>
          </template>

          <template #item.created_by="{ item }">
            {{ item.username || item.created_by }}
          </template>

          <template #item.created_at="{ item }">
            {{ formatDate(item.created_at) }}
          </template>

          <template #item.actions="{ item }">
            <div class="d-flex justify-center ga-1" @click.stop>
              <v-btn v-if="item.proj_id"
                     icon size="small" variant="text" color="success"
                     title="View Project Gantt"
                     @click="$router.push(`/projects/${item.proj_id}/gantt`)">
                <v-icon size="18">mdi-chart-gantt</v-icon>
              </v-btn>

              <v-btn v-if="authStore.canStartProject(item)"
                     icon size="small" variant="text" color="primary"
                     title="Start Project"
                     @click="viewEnquiryDetail(item.enquiry_id)">
                <v-icon size="18">mdi-rocket-launch</v-icon>
              </v-btn>

              <v-btn v-if="authStore.canManageProject(item)"
                     icon size="small" variant="text" color="primary"
                     title="Manage Project Setup"
                     @click="$router.push(`/projects/${item.proj_id}/setup`)">
                <v-icon size="18">mdi-cog</v-icon>
              </v-btn>

              <v-btn v-if="authStore.canEditEnquiry(item)"
                     icon size="small" variant="text" color="info"
                     title="Edit"
                     @click="editEnquiry(item.enquiry_id)">
                <v-icon size="18">mdi-pencil</v-icon>
              </v-btn>

              <v-btn v-if="authStore.canDeleteEnquiry(item)"
                     icon size="small" variant="text" color="error"
                     title="Delete Enquiry"
                     @click="openDeleteDialog(item)">
                <v-icon size="18">mdi-delete</v-icon>
              </v-btn>
            </div>
          </template>
        </v-data-table-virtual>
      </div>

    </v-card>

    <!-- PDF Dialog -->
    <v-dialog v-model="showPdfDialog" max-width="900" scrollable>
      <v-card class="dialog-card">
        <v-card-title class="d-flex align-center justify-space-between bg-primary">
          <div class="text-h6 font-weight-bold text-white">
            <v-icon start>mdi-file-pdf-box</v-icon>
            {{ pdfEnquiry?.enquiry_no || 'Enquiry Preview' }}
            <v-chip v-if="pdfEnquiry" size="small" variant="flat" class="ml-2"
                    :color="getStatusColor(pdfEnquiry.status)">
              {{ pdfEnquiry.status }}
            </v-chip>
          </div>
          <div class="d-flex ga-2">
            <v-btn variant="elevated" color="white" class="text-primary font-weight-bold"
                   :loading="downloadingPDF"
                   @click="downloadCurrentPDF">
              <v-icon start>mdi-download</v-icon>
              Download
            </v-btn>
            <v-btn variant="text" color="white" icon @click="showPdfDialog = false">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </div>
        </v-card-title>

        <v-alert v-if="pdfEnquiry?.latest_review_remark" type="warning" variant="tonal"
                 density="compact" rounded="0" class="text-body-2">
          <strong>{{ reviewDecisionLabel(pdfEnquiry.latest_review_decision) }}:</strong>
          {{ pdfEnquiry.latest_review_remark }}
        </v-alert>

        <v-card-text class="pa-0" style="height: 72vh;">
          <iframe v-if="pdfUrl" :src="pdfUrl" style="width: 100%; height: 100%; border: none;" />
          <div v-else class="d-flex align-center justify-center fill-height">
            <v-progress-circular indeterminate color="primary" size="64" />
          </div>
        </v-card-text>

        <v-divider />

        <v-card-actions class="pa-3">
          <template v-if="authStore.canReviewEnquiry(pdfEnquiry)">
            <v-btn color="warning" variant="tonal"
                   @click="openReviewDialog('NeedsRework')">
              <v-icon start>mdi-file-document-edit</v-icon>
              Request Rework
            </v-btn>
            <v-btn color="error" variant="tonal" class="ml-2"
                   @click="openReviewDialog('NotFeasible')">
              <v-icon start>mdi-close-circle</v-icon>
              Not Feasible
            </v-btn>
          </template>

          <v-spacer />

          <v-btn v-if="authStore.canStartProject(pdfEnquiry)"
                 color="primary" variant="elevated"
                 @click="startProjectFromPdf">
            <v-icon start>mdi-rocket-launch</v-icon>
            Start Project
          </v-btn>
          <v-btn variant="text" @click="showPdfDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Review Dialog -->
    <v-dialog v-model="reviewDialog" max-width="520">
      <v-card class="dialog-card">
        <v-card-title class="text-subtitle-1 font-weight-bold pa-4">
          {{ reviewDecision === 'NeedsRework' ? 'Request Rework' : 'Mark Not Feasible' }}
        </v-card-title>
        <v-divider />
        <v-card-text class="pa-4">
          <v-alert :type="reviewDecision === 'NeedsRework' ? 'warning' : 'error'"
                   variant="tonal" density="compact" class="mb-4">
            {{
 reviewDecision === 'NeedsRework'
              ? 'The enquiry goes back to Sales for changes.'
              : 'The enquiry is rejected and cannot be launched as a project.'
            }}
          </v-alert>
          <v-textarea v-model="reviewRemark"
                      :label="reviewDecision === 'NeedsRework'
                        ? 'What needs to change? (required)'
                        : 'Reason (required)'"
                      variant="outlined" rows="4" auto-grow
                      maxlength="2000" counter autofocus />
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="reviewDialog = false">Cancel</v-btn>
          <v-btn :color="reviewDecision === 'NeedsRework' ? 'warning' : 'error'"
                 variant="flat" :loading="submittingReview"
                 :disabled="!reviewRemark.trim()"
                 @click="submitReview">
            Submit
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="500">
      <v-card class="dialog-card rounded-lg">
        <v-card-text class="pa-6">
          <div class="d-flex align-start ga-4 mb-3">
            <div class="delete-icon-wrap">
              <v-icon color="error" size="28">mdi-delete-alert</v-icon>
            </div>
            <div>
              <div class="text-h6 font-weight-bold mb-1">Delete Enquiry?</div>
              <div class="text-body-2 text-medium-emphasis">
                <strong>{{ deleteTarget?.enquiry_no }}</strong> will be permanently removed.
                This action cannot be undone.
              </div>
            </div>
          </div>

          <v-alert v-if="deleteTarget?.proj_id" type="warning" variant="tonal"
                   density="compact" class="mt-4 mb-2">
            This enquiry has a linked project. Deleting the enquiry will
            <strong>not</strong> delete the project.
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
  import { ref, onMounted, nextTick } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useEnquiryStore } from '@/stores/enquiry'
  import { useAuthStore } from '@/stores/auth'
  import { ENQUIRY_STATUS_COLORS, DEFAULT_COLOR } from '@/utils/constants'
  import { formatDate } from '@/utils/formatters'

  const route = useRoute()
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
  const pdfEnquiry = ref(null)
  const downloadingPDF = ref(false)

  const reviewDialog = ref(false)
  const reviewDecision = ref('')
  const reviewRemark = ref('')
  const submittingReview = ref(false)

  const headers = [
    { title: '', key: 'project_indicator', sortable: false, align: 'center', width: '52px' },
    { title: 'Enquiry No', key: 'enquiry_no', sortable: true },
    { title: 'NPI Category', key: 'form_category', sortable: true },
    { title: 'Status', key: 'status', sortable: true },
    { title: 'Created By', key: 'created_by', sortable: true },
    { title: 'Created Date', key: 'created_at', sortable: true },
    { title: 'Actions', key: 'actions', sortable: false, align: 'center', width: '190px' },
  ]

  const highlightId = ref(null)

  function applyHighlight() {
    const id = Number(route.query.highlight)
    if (!id) return
    highlightId.value = id
    nextTick(() => {
      document.querySelector('.row-highlight')?.scrollIntoView({
        behavior: 'smooth', block: 'center',
      })
    })
    setTimeout(() => { highlightId.value = null }, 3000)
  }

  function getStatusColor(status) {
    return ENQUIRY_STATUS_COLORS[status] ?? DEFAULT_COLOR
  }

  function reviewDecisionLabel(decision) {
    return {
      NeedsRework: 'Needs rework',
      NotFeasible: 'Not feasible',
    }[decision] ?? 'Latest review'
  }

  async function viewEnquiryPDF(item) {
    const id = typeof item === 'object' ? item.enquiry_id : item
    currentEnquiryId.value = id
    pdfEnquiry.value = typeof item === 'object' ? item : null
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
    } finally {
      downloadingPDF.value = false
    }
  }

  function startProjectFromPdf() {
    showPdfDialog.value = false
    viewEnquiryDetail(currentEnquiryId.value)
  }

  function openReviewDialog(decision) {
    reviewDecision.value = decision
    reviewRemark.value = ''
    reviewDialog.value = true
  }

  async function submitReview() {
    if (!currentEnquiryId.value || !reviewRemark.value.trim()) return
    submittingReview.value = true
    try {
      const res = await enquiryStore.reviewEnquiry(
        currentEnquiryId.value, reviewDecision.value, reviewRemark.value.trim())

      if (res?.success) {
        reviewDialog.value = false
        showPdfDialog.value = false
        snackbarMessage.value = res.message || 'Review submitted'
        snackbarColor.value = 'success'
        snackbar.value = true
        await enquiryStore.fetchEnquiries()
      } else {
        snackbarMessage.value = res?.message || 'Failed to submit review'
        snackbarColor.value = 'error'
        snackbar.value = true
      }
    } finally {
      submittingReview.value = false
    }
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
    snackbarMessage.value = result.success
      ? 'Enquiry deleted successfully'
      : `Error: ${result.message}`
    snackbarColor.value = result.success ? 'success' : 'error'
    snackbar.value = true
    deleteTarget.value = null
  }

  onMounted(async () => {
    try {
      await enquiryStore.fetchEnquiries()

      const projIds = [...new Set(
        (enquiryStore.enquiries ?? [])
          .filter(e => e.proj_id)
          .map(e => e.proj_id)
      )]
      await Promise.all(projIds.map(id => authStore.fetchProjectRole(id)))
    } catch (err) {
      console.error('Enquiries mount error:', err)
    }
    applyHighlight()
  })
</script>

<style scoped>
  @keyframes row-flash {
    0% {
      background-color: rgba(25, 118, 210, 0.28);
    }

    100% {
      background-color: transparent;
    }
  }

  .row-highlight {
    animation: row-flash 2.5s ease-out;
  }

  .page-container {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
  }

  .page-card {
    border-radius: 8px;
  }

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

  .clickable-rows :deep(tbody tr) {
    cursor: pointer;
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
