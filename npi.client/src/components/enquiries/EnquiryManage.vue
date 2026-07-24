<template>
  <v-container fluid class="page-container pa-4 pa-md-6">

    <v-card class="page-card" elevation="2" border>

      <v-card-title class="bg-primary text-white d-flex align-center justify-space-between pa-4">
        <div class="text-h6 font-weight-bold text-white">
          <v-icon class="mr-2">mdi-file-document-outline</v-icon>
          Enquiry Summary — {{ enquiry?.enquiry_no }}
        </div>
        <div class="d-flex ga-2 align-center">
          <v-btn v-if="canStartProject" color="success" variant="flat" density="comfortable"
                 @click="openStartProjectDialog">
            <v-icon start>mdi-rocket-launch</v-icon> Start Project
          </v-btn>

          <v-menu location="bottom end">
            <template #activator="{ props }">
              <v-btn icon variant="text" color="white" density="comfortable" v-bind="props">
                <v-icon>mdi-dots-vertical</v-icon>
              </v-btn>
            </template>
            <v-list density="compact" min-width="210">
              <v-list-item prepend-icon="mdi-download" title="Download PDF"
                           :disabled="downloadingPDF" @click="downloadPDF" />
              <v-list-item v-if="enquiry?.proj_id"
                           prepend-icon="mdi-cog" title="Manage project"
                           @click="$router.push(`/projects/${enquiry.proj_id}/setup`)" />
              <v-list-item v-if="enquiry?.proj_id"
                           prepend-icon="mdi-chart-gantt" title="View Gantt"
                           @click="$router.push(`/projects/${enquiry.proj_id}/gantt`)" />
              <template v-if="authStore.canReviewEnquiry(enquiry)">
                <v-divider class="my-1" />
                <v-list-item prepend-icon="mdi-file-document-edit" title="Request rework"
                             @click="openReviewDialog('NeedsRework')" />
                <v-list-item prepend-icon="mdi-close-circle" title="Mark not feasible"
                             base-color="error"
                             @click="openReviewDialog('NotFeasible')" />
              </template>
            </v-list>
          </v-menu>

          <v-btn variant="text" color="white" density="comfortable" @click="$router.back()">
            <v-icon start>mdi-arrow-left</v-icon> Back
          </v-btn>
        </div>
      </v-card-title>

      <!-- Loading -->
      <v-card-text v-if="loading" class="text-center pa-8">
        <v-progress-circular indeterminate color="primary" size="64" />
      </v-card-text>

      <v-card-text v-else-if="enquiry" class="pa-4">

        <!-- PDF Preview -->
        <div class="text-subtitle-1 font-weight-bold text-primary mb-2">
          <v-icon size="18" class="mr-1">mdi-file-pdf-box</v-icon> Enquiry Document
        </div>
        <v-sheet border rounded class="pdf-frame mb-5">
          <iframe v-if="pdfUrl" :src="pdfUrl" class="pdf-iframe" />
          <div v-else class="d-flex align-center justify-center fill-height">
            <v-progress-circular indeterminate color="primary" size="48" />
          </div>
        </v-sheet>

        <!-- Attached Files -->
        <div v-if="enquiry.files?.length" class="mb-5">
          <div class="text-subtitle-1 font-weight-bold text-primary mb-1">
            <v-icon size="18" class="mr-1">mdi-paperclip</v-icon> Attached Files
          </div>
          <v-list density="compact" class="pa-0 bg-transparent">
            <v-list-item v-for="file in enquiry.files" :key="file.file_id" class="px-0">
              <template #prepend>
                <v-icon size="small" class="mr-2">mdi-paperclip</v-icon>
              </template>
              <v-list-item-title class="text-body-2">{{ file.file_name }}</v-list-item-title>
              <v-list-item-subtitle v-if="file.file_size" class="text-caption">
                {{ formatSize(file.file_size) }}
              </v-list-item-subtitle>
              <template #append>
                <v-btn icon="mdi-download" size="x-small" variant="text" color="primary"
                       @click="downloadFile(file)" />
              </template>
            </v-list-item>
          </v-list>
        </div>

        <!-- Review History + Compare Revisions -->
        <v-row v-if="reviews.length || revisions.length > 1" dense>
          <v-col cols="12" md="6">
            <v-card variant="outlined" height="100%">
              <v-card-title class="text-subtitle-2 font-weight-bold py-2">
                <v-icon start size="18">mdi-history</v-icon> Review History
              </v-card-title>
              <v-divider />
              <v-card-text class="pa-3">
                <v-timeline v-if="reviews.length" density="compact" side="end" class="pt-0">
                  <v-timeline-item v-for="r in reviews" :key="r.review_id"
                                   size="x-small" :dot-color="decisionColor(r.decision)">
                    <div class="d-flex align-center flex-wrap">
                      <strong class="text-body-2">{{ reviewDecisionLabel(r.decision) }}</strong>
                      <v-chip size="x-small" variant="tonal" class="ml-2">Rev {{ r.revision_no }}</v-chip>
                    </div>
                    <div class="text-caption text-grey">
                      {{ r.reviewer_name }} · {{ formatDate(r.created_at) }}
                    </div>
                    <div v-if="r.remark" class="text-body-2 mt-1">{{ r.remark }}</div>
                  </v-timeline-item>
                </v-timeline>
                <div v-else class="text-caption text-grey py-2">No reviews yet.</div>
              </v-card-text>
            </v-card>
          </v-col>

          <v-col cols="12" md="6">
            <v-card variant="outlined" height="100%">
              <v-card-title class="text-subtitle-2 font-weight-bold py-2 d-flex align-center">
                <v-icon start size="18">mdi-compare</v-icon> Compare Revisions
                <v-spacer />
                <template v-if="revisions.length > 1">
                  <v-select v-model="revA" :items="revisionOptions" density="compact"
                            hide-details variant="outlined" style="max-width:88px" label="From" />
                  <v-select v-model="revB" :items="revisionOptions" density="compact"
                            hide-details variant="outlined" class="ml-2" style="max-width:88px" label="To" />
                </template>
              </v-card-title>
              <v-divider />
              <v-card-text class="pa-0">
                <div v-if="revisions.length > 1" class="compare-scroll">
                  <v-table density="compact">
                    <thead>
                      <tr>
                        <th class="text-caption">Field</th>
                        <th class="text-caption">Rev {{ revA }}</th>
                        <th class="text-caption">Rev {{ revB }}</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="row in diffRows" :key="row.key"
                          :class="{ 'bg-yellow-lighten-4': row.changed }">
                        <td class="text-caption">{{ row.label }}</td>
                        <td class="text-caption">{{ row.a || '—' }}</td>
                        <td class="text-caption">{{ row.b || '—' }}</td>
                      </tr>
                    </tbody>
                  </v-table>
                </div>
                <div v-else class="text-caption text-grey pa-3">
                  Only one revision so far — nothing to compare.
                </div>
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>

      </v-card-text>
    </v-card>

    <!-- Start Project Dialog -->
    <v-dialog v-model="showStartProjectDialog" max-width="500" scrollable>
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

    <v-dialog v-model="reviewDialog" max-width="520">
      <v-card>
        <v-card-title class="text-subtitle-1 font-weight-bold">
          {{ reviewDecision === 'NeedsRework' ? 'Request Rework' : 'Mark Not Feasible' }}
          — {{ enquiry?.enquiry_no }}
        </v-card-title>
        <v-divider />
        <v-card-text>
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
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="reviewDialog = false">Cancel</v-btn>
          <v-btn :color="reviewDecision === 'NeedsRework' ? 'warning' : 'error'"
                 variant="flat" :loading="submittingReview"
                 :disabled="!reviewRemark.trim()" @click="submitReview">Submit</v-btn>
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
  import { formatDate, formatSize } from '@/utils/formatters'

  const route = useRoute()
  const router = useRouter()
  const enquiryStore = useEnquiryStore()
  const projectStore = useProjectStore()
  const authStore = useAuthStore()

  const loading = ref(false)
  const downloadingPDF = ref(false)
  const enquiry = ref(null)
  const pdfUrl = ref(null)

  const reviews = ref([])
  const revisions = ref([])
  const reviewDialog = ref(false)
  const reviewDecision = ref('')
  const reviewRemark = ref('')
  const submittingReview = ref(false)
  const revA = ref(null)
  const revB = ref(null)

  const showStartProjectDialog = ref(false)
  const startingProject = ref(false)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const priorityOptions = PRIORITY_OPTIONS

  const projectData = ref({
    project_name: '', priority: DEFAULT_PRIORITY, expected_completion: '', description: ''
  })

  function decisionColor(d) {
    return { NeedsRework: 'warning', NotFeasible: 'error' }[d] || 'grey'
  }

  function reviewDecisionLabel(d) {
    return { NeedsRework: 'Needs rework', NotFeasible: 'Not feasible' }[d] || d
  }

  function downloadFile(file) {
    window.open(`/api/file/download/${file.file_id}`, '_blank')
  }

  async function loadPdf() {
    pdfUrl.value = null
    try {
      const res = await enquiryStore.getEnquiryPDFBlob(route.params.id)
      if (res?.success) pdfUrl.value = res.url
    } catch {
      showSnack('Could not load the enquiry preview', 'error')
    }
  }

  function openReviewDialog(decision) {
    reviewDecision.value = decision
    reviewRemark.value = ''
    reviewDialog.value = true
  }

  const revisionOptions = computed(() => revisions.value.map(r => r.revision_no))

  const diffRows = computed(() => {
    const a = revisions.value.find(r => r.revision_no === revA.value)
    const b = revisions.value.find(r => r.revision_no === revB.value)
    if (!a || !b) return []
    const flatten = fv => {
      const out = {}
      Object.entries(fv || {}).forEach(([sk, fields]) =>
        Object.entries(fields || {}).forEach(([fk, v]) => { out[`${sk}.${fk}`] = v }))
      return out
    }
    const fa = flatten(a.field_values), fb = flatten(b.field_values)
    const keys = [...new Set([...Object.keys(fa), ...Object.keys(fb)])]
    return keys.map(k => ({
      key: k,
      label: k.split('.').pop().replace(/_/g, ' '),
      a: fa[k], b: fb[k],
      changed: (fa[k] ?? '') !== (fb[k] ?? '')
    }))
  })

  async function submitReview() {
    if (!reviewDecision.value || !reviewRemark.value.trim()) return
    submittingReview.value = true
    try {
      const res = await enquiryStore.reviewEnquiry(
        route.params.id, reviewDecision.value, reviewRemark.value.trim())
      if (res?.success) {
        reviewDialog.value = false
        reviewRemark.value = ''
        reviewDecision.value = ''
        await loadReviewData()
        const r = await enquiryStore.fetchEnquiryById(route.params.id)
        if (r?.success) enquiry.value = r.data
        showSnack('Review submitted')
      } else {
        showSnack(res?.message || 'Failed', 'error')
      }
    } finally {
      submittingReview.value = false
    }
  }

  async function loadReviewData() {
    reviews.value = await enquiryStore.fetchReviews(route.params.id)
    revisions.value = await enquiryStore.fetchRevisions(route.params.id)
    if (revisions.value.length > 1) {
      revA.value = revisions.value[revisions.value.length - 2].revision_no
      revB.value = revisions.value[revisions.value.length - 1].revision_no
    }
  }

  const canStartProject = computed(() =>
    !!enquiry.value && authStore.canStartProject(enquiry.value)
  )

  function getFieldLabel(sectionKey, fieldKey) {
    const section = (enquiryStore.sections ?? []).find(s => s.section_key === sectionKey)
    return section?.fields?.find(f => f.field_key === fieldKey)?.field_label ?? fieldKey.replace(/_/g, ' ')
  }

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

  onMounted(async () => {
    loading.value = true
    await enquiryStore.fetchConfig()
    const result = await enquiryStore.fetchEnquiryById(route.params.id)
    if (result?.success) enquiry.value = result.data
    loading.value = false

    loadPdf()
    loadReviewData()
  })
</script>

<style scoped>
  .page-container {
    min-height: 100vh;
    background-color: #f5f6f8;
  }

  .page-card {
    border-radius: 8px;
  }

  .pdf-frame {
    height: 70vh;
    overflow: hidden;
    background-color: #fafafa;
  }

  .pdf-iframe {
    width: 100%;
    height: 100%;
    border: none;
    display: block;
  }

  .compare-scroll {
    max-height: 320px;
    overflow-y: auto;
  }
</style>
