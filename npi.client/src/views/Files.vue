<template>
  <v-container fluid class="page-container pa-6 d-flex flex-column">
    <v-card class="page-header-card flex-grow-1 d-flex flex-column overflow-hidden" elevation="2">

      <!-- Header -->
      <v-card-title class="bg-primary text-white d-flex align-center justify-space-between flex-shrink-0"
                    style="flex: 0 0 auto;">
        <div class="page-title">
          <v-icon class="mr-2">mdi-file-document</v-icon>
          Files
        </div>
        <!-- Search / sort shown only on Files tab -->
        <div v-if="activeTab === 'files'" class="d-flex ga-3">
          <v-text-field v-model="search"
                        label="Search files/folders"
                        density="compact"
                        clearable
                        prepend-inner-icon="mdi-magnify"
                        variant="outlined"
                        hide-details
                        style="min-width:400px" />
          <v-select v-model="sortBy"
                    :items="sortOptions"
                    label="Sort"
                    density="compact"
                    variant="outlined"
                    hide-details
                    style="max-width:180px" />
        </div>
        <!-- Customer search shown on customers tab -->
        <div v-if="activeTab === 'customers'" class="d-flex ga-3">
          <v-text-field v-model="customerSearch"
                        label="Search customers…"
                        density="compact"
                        clearable
                        prepend-inner-icon="mdi-magnify"
                        variant="outlined"
                        hide-details
                        style="min-width:300px" />
          <v-btn v-if="canManageFiles"
                 color="white" variant="outlined"
                 prepend-icon="mdi-plus"
                 @click="openCustomerDialog()">
            Add Customer
          </v-btn>
        </div>
      </v-card-title>

      <!-- Sub-tabs -->
      <v-tabs v-model="activeTab" bg-color="grey-lighten-4" color="primary" density="compact"
              class="flex-shrink-0">
        <v-tab value="files">
          <v-icon start size="18">mdi-folder-multiple</v-icon>
          File Explorer
        </v-tab>
        <v-tab value="customers" v-if="canManageFiles">
          <v-icon start size="18">mdi-domain</v-icon>
          Customers
        </v-tab>
      </v-tabs>

      <v-divider class="flex-shrink-0" />

      <!-- Loading bar -->
      <div style="flex: 0 0 2px; height: 2px;">
        <v-progress-linear v-if="loading || customersLoading" indeterminate color="primary" height="2" />
      </div>

      <!-- Tab windows -->
      <v-window v-model="activeTab" :key="activeTab" class="flex-grow-1 d-flex flex-column" style="min-height:0;">

        <!-- ── FILE EXPLORER TAB ──────────────────────────────────────────── -->
        <v-window-item value="files" class="fill-height d-flex flex-column">

          <!-- Breadcrumbs -->
          <v-card-text class="px-4 d-flex align-center flex-shrink-0 py-2"
                       style="flex: 0 0 24px; overflow: hidden;">
            <v-breadcrumbs :items="breadcrumbs"
                           class="pa-0 w-100"
                           style="white-space: nowrap; flex-wrap: nowrap; overflow: hidden; text-overflow: ellipsis;" />
          </v-card-text>

          <!-- Tree + Preview side-by-side -->
          <v-card-text class="flex-grow-1 pa-0 w-100 d-flex" style="min-height: 0; overflow: hidden;">

            <!-- Tree panel -->
            <div :style="previewItem ? 'width:45%; min-width:0;' : 'width:100%;'"
                 style="overflow-y: auto; transition: width 0.25s ease; border-right: 1px solid #e0e0e0;">

              <div v-if="!loading && filteredTree.length === 0 && !search"
                   class="d-flex flex-column align-center mt-12">
                <v-icon size="72" color="grey-lighten-1">mdi-folder-open-outline</v-icon>
                <div class="text-h6 text-grey-darken-1 mt-4">No files found</div>
              </div>

              <div v-else-if="!loading && filteredTree.length === 0 && search"
                   class="d-flex flex-column align-center mt-12">
                <v-icon size="72" color="grey-lighten-1">mdi-file-search-outline</v-icon>
                <div class="text-h6 text-grey-darken-1 mt-4">No results for "{{ search }}"</div>
                <v-btn variant="tonal" class="mt-4" @click="search = ''">Clear search</v-btn>
              </div>

              <v-treeview v-else
                          :items="filteredTree"
                          item-title="name"
                          item-value="id"
                          item-children="children"
                          open-on-click
                          activatable
                          v-model:opened="openedIds"
                          class="pt-2 pl-2"
                          @update:activated="updateBreadcrumb">

                <template #prepend="{ item }">
                  <v-icon v-if="item.type === 'root-projects'" color="primary">mdi-folder-multiple</v-icon>
                  <v-icon v-else-if="item.type === 'root-customers'" color="teal">mdi-account-group</v-icon>
                  <v-icon v-else-if="item.type === 'project'" color="amber-darken-2">mdi-folder</v-icon>
                  <v-icon v-else-if="item.type === 'customer'" color="teal-darken-1">mdi-folder-account</v-icon>
                  <v-icon v-else-if="item.type === 'folder'" color="blue-darken-1">mdi-folder-open</v-icon>
                  <v-icon v-else color="grey-darken-1">mdi-file-document-outline</v-icon>
                </template>

                <template #append="{ item }">
                  <div v-if="item.type === 'file'" class="d-flex align-center ga-1 pr-4">
                    <v-chip v-if="item.file_version" size="x-small" variant="tonal" color="secondary" class="mr-1">
                      v{{ item.file_version }}
                    </v-chip>
                    <span class="text-caption text-grey mr-2">{{ formatSize(item.size) }}</span>
                    <span class="text-caption text-grey mr-3">{{ formatDate(item.uploaded_at) }}</span>
                    <v-tooltip text="Preview" location="top">
                      <template #activator="{ props }">
                        <v-btn v-bind="props" icon size="x-small" variant="text"
                               :color="previewItem?.id === item.id ? 'primary' : ''"
                               @click.stop="handlePreview(item)">
                          <v-icon size="16">mdi-eye-outline</v-icon>
                        </v-btn>
                      </template>
                    </v-tooltip>
                    <v-tooltip text="Download" location="top">
                      <template #activator="{ props }">
                        <v-btn v-bind="props" icon size="x-small" variant="text"
                               @click.stop="downloadFile(item)">
                          <v-icon size="16">mdi-download-outline</v-icon>
                        </v-btn>
                      </template>
                    </v-tooltip>
                    <v-tooltip text="Delete" location="top" v-if="canManageFiles && item.can_edit">
                      <template #activator="{ props }">
                        <v-btn v-bind="props" icon size="x-small" variant="text" color="error"
                               @click.stop="deleteFile(item)">
                          <v-icon size="16">mdi-delete-outline</v-icon>
                        </v-btn>
                      </template>
                    </v-tooltip>
                  </div>
                  <div v-else-if="item.children?.length" class="pr-4">
                    <v-chip size="x-small" variant="tonal" color="primary">
                      {{ countFiles(item) }}
                    </v-chip>
                  </div>
                </template>
              </v-treeview>
            </div>

            <!-- Inline Preview Panel -->
            <transition name="slide-preview">
              <div v-if="previewItem"
                   class="d-flex flex-column"
                   style="width:55%; min-width:0; overflow:hidden; background:#fafafa;">
                <div class="d-flex align-center px-4 py-2 bg-grey-lighten-4"
                     style="border-bottom:1px solid #e0e0e0; flex-shrink:0;">
                  <v-icon class="mr-2" color="primary" size="18">{{ previewIconFor(previewItem) }}</v-icon>
                  <span class="text-body-2 font-weight-medium text-truncate flex-grow-1"
                        :title="previewItem.name">{{ previewItem.name }}</span>
                  <v-btn icon size="x-small" variant="text" class="ml-2" @click="closePreview">
                    <v-icon size="18">mdi-close</v-icon>
                  </v-btn>
                </div>
                <div class="flex-grow-1" style="overflow:hidden; position:relative;">
                  <div v-if="previewLoading" class="d-flex align-center justify-center fill-height">
                    <v-progress-circular indeterminate color="primary" />
                  </div>
                  <iframe v-else-if="previewType === 'pdf'"
                          :src="previewBlobUrl" width="100%" height="100%"
                          style="border:none; display:block;" />
                  <div v-else-if="previewType === 'image'"
                       class="d-flex align-center justify-center fill-height pa-4"
                       style="background:#f0f0f0;">
                    <img :src="previewBlobUrl" :alt="previewItem.name"
                         style="max-width:100%; max-height:100%; object-fit:contain;
                                border-radius:4px; box-shadow:0 2px 8px rgba(0,0,0,0.15);" />
                  </div>
                  <div v-else-if="previewType === 'text'"
                       style="height:100%; overflow:auto; padding:16px;">
                    <pre style="font-size:12px; font-family:monospace; white-space:pre-wrap;
                                word-break:break-all; margin:0;">{{ previewTextContent }}</pre>
                  </div>
                </div>
              </div>
            </transition>
          </v-card-text>
        </v-window-item>

        <!-- ── CUSTOMERS TAB ──────────────────────────────────────────────── -->
        <v-window-item value="customers" v-show="canManageFiles" class="fill-height d-flex flex-column"
                       style="min-height:0; overflow:hidden;">
          <v-card-text class="flex-grow-1 pa-0 d-flex flex-column" style="min-height:0; overflow:hidden;">

            <v-data-table-virtual :headers="customerHeaders"
                                  :items="filteredCustomers"
                                  :loading="customersLoading"
                                  density="comfortable"
                                  fixed-header
                                  height="100%"
                                  item-key="cust_id"
                                  class="customers-table flex-grow-1">

              <!-- Company name -->
              <template #item.comp_name="{ item }">
                <div class="d-flex align-center ga-2 py-1">
                  <v-avatar color="teal" size="30">
                    <span class="text-white" style="font-size:10px; font-weight:700;">
                      {{ getInitials(item.comp_name) }}
                    </span>
                  </v-avatar>
                  <span class="font-weight-medium text-body-2">{{ item.comp_name }}</span>
                </div>
              </template>

              <!-- Status chip -->
              <template #item.is_active="{ item }">
                <v-chip :color="item.is_active ? 'success' : 'grey'" size="small" variant="flat">
                  {{ item.is_active ? 'Active' : 'Inactive' }}
                </v-chip>
              </template>

              <!-- Created date -->
              <template #item.created_at="{ item }">
                <span class="text-caption">{{ formatDate(item.created_at) }}</span>
              </template>

              <!-- Actions — visible to Admin/Manager only -->
              <template #item.actions="{ item }">
                <div v-if="canManageFiles" class="d-flex ga-1">
                  <v-tooltip text="Edit" location="top">
                    <template #activator="{ props }">
                      <v-btn v-bind="props" icon size="small" variant="text"
                             @click="openCustomerDialog(item)">
                        <v-icon size="16">mdi-pencil</v-icon>
                      </v-btn>
                    </template>
                  </v-tooltip>
                  <v-tooltip text="Delete" location="top">
                    <template #activator="{ props }">
                      <v-btn v-bind="props" icon size="small" variant="text" color="error"
                             @click="confirmDeleteCustomer(item)">
                        <v-icon size="16">mdi-delete-outline</v-icon>
                      </v-btn>
                    </template>
                  </v-tooltip>
                </div>
              </template>

              <template #no-data>
                <div class="d-flex flex-column align-center justify-center pa-10 text-grey">
                  <v-icon size="56" color="grey-lighten-1">mdi-domain</v-icon>
                  <div class="mt-2 text-body-2">No customers found</div>
                </div>
              </template>

            </v-data-table-virtual>
          </v-card-text>
        </v-window-item>

      </v-window>
    </v-card>

    <!-- ── Customer Create/Edit Dialog ──────────────────────────────────── -->
    <v-dialog v-model="customerDialog" max-width="600" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          {{ editingCustomer ? 'Edit Customer' : 'Add Customer' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-form ref="customerFormRef">
            <v-row dense>
              <v-col cols="12">
                <v-text-field v-model="customerForm.comp_name"
                              label="Company Name *"
                              variant="outlined" density="comfortable"
                              :rules="[v => !!v || 'Required']" />
              </v-col>
              <v-col cols="12">
                <v-textarea v-model="customerForm.cust_addr"
                            label="Address"
                            variant="outlined" density="comfortable" rows="2" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="customerForm.contact_name"
                              label="Contact Person"
                              variant="outlined" density="comfortable" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="customerForm.contact_email"
                              label="Contact Email"
                              type="email"
                              variant="outlined" density="comfortable" />
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field v-model="customerForm.contact_phone"
                              label="Contact Phone"
                              variant="outlined" density="comfortable" />
              </v-col>
              <v-col cols="12" md="6" class="d-flex align-center">
                <v-switch v-model="customerForm.is_active"
                          label="Active" color="success" hide-details density="compact" />
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="customerDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="elevated"
                 :loading="customerSaving"
                 @click="saveCustomer">
            {{ editingCustomer ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Customer Delete Confirm ───────────────────────────────────────── -->
    <v-dialog v-model="customerDeleteDialog" max-width="460">
      <v-card>
        <v-card-title class="bg-error text-white text-subtitle-1">
          <v-icon class="mr-2">mdi-alert</v-icon>
          Delete Customer
        </v-card-title>
        <v-card-text class="pt-4">
          Delete <strong>{{ customerDeleteTarget?.comp_name }}</strong>?
          This will not remove associated files or projects.
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="customerDeleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="customerSaving"
                 @click="doDeleteCustomer">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- External software snackbar (file explorer) -->
    <v-snackbar v-model="externalPrompt.show"
                location="bottom center"
                :timeout="6000"
                color="surface"
                elevation="4"
                min-width="320"
                max-width="480">
      <div class="d-flex flex-column ga-2">
        <div class="d-flex align-start ga-3">
          <v-icon :color="externalPrompt.iconColor" size="22" class="mt-1 flex-shrink-0">
            {{ externalPrompt.icon }}
          </v-icon>
          <div style="min-width:0;">
            <div class="text-body-2 font-weight-medium" style="word-break:break-word;">
              {{ externalPrompt.title }}
            </div>
            <div class="text-caption text-grey">{{ externalPrompt.subtitle }}</div>
          </div>
        </div>
        <div class="d-flex justify-end align-center ga-2">
          <v-btn size="small" color="primary" variant="tonal"
                 @click="downloadFile(externalPrompt.item); externalPrompt.show = false">
            <v-icon start size="14">mdi-download-outline</v-icon>
            Download to open
          </v-btn>
          <v-btn size="small" icon variant="text" @click="externalPrompt.show = false">
            <v-icon size="16">mdi-close</v-icon>
          </v-btn>
        </div>
      </div>
    </v-snackbar>
  </v-container>
</template>

<script setup>
  import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue'
  import { api } from '@/utils/api'
  import { useAuthStore } from '@/stores/auth'
  import { useCustomerStore } from '@/stores/customer'

  const authStore = useAuthStore()
  const customerStore = useCustomerStore()

  // ── Access control ────────────────────────────────────────────────────────────
  const canManageFiles = computed(() => authStore.isAdmin || authStore.isManager)

  // ── Tab state ─────────────────────────────────────────────────────────────────
  const activeTab = ref('files')

  // ── File explorer state (unchanged from original) ─────────────────────────────
  const loading = ref(false)
  const rawTree = ref([])
  const search = ref('')
  const sortBy = ref('Name (A-Z)')
  const breadcrumbs = ref([{ title: 'Files' }])
  const openedIds = ref([])

  const previewItem = ref(null)
  const previewType = ref('')
  const previewBlobUrl = ref('')
  const previewTextContent = ref('')
  const previewLoading = ref(false)

  const externalPrompt = ref({
    show: false, item: null, title: '', subtitle: '', icon: 'mdi-application', iconColor: 'primary'
  })

  const sortOptions = ['Name (A-Z)', 'Name (Z-A)', 'Newest', 'Oldest', 'Size (Large)', 'Size (Small)']

  const PREVIEWABLE_TYPES = {
    pdf: ['pdf'],
    image: ['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'svg'],
    text: ['txt', 'csv', 'log', 'md']
  }

  const EXTERNAL_HINTS = {
    doc: { icon: 'mdi-microsoft-word', color: '#2B579A', app: 'Microsoft Word' },
    docx: { icon: 'mdi-microsoft-word', color: '#2B579A', app: 'Microsoft Word' },
    xls: { icon: 'mdi-microsoft-excel', color: '#217346', app: 'Microsoft Excel' },
    xlsx: { icon: 'mdi-microsoft-excel', color: '#217346', app: 'Microsoft Excel' },
    ppt: { icon: 'mdi-microsoft-powerpoint', color: '#D24726', app: 'PowerPoint' },
    pptx: { icon: 'mdi-microsoft-powerpoint', color: '#D24726', app: 'PowerPoint' },
    dwg: { icon: 'mdi-drawing', color: '#E34C26', app: 'AutoCAD' },
    step: { icon: 'mdi-cube-outline', color: '#607D8B', app: 'CAD software' },
    stp: { icon: 'mdi-cube-outline', color: '#607D8B', app: 'CAD software' },
  }

  // ── Customers tab state ───────────────────────────────────────────────────────
  const customersLoading = ref(false)
  const customerSearch = ref('')
  const customerDialog = ref(false)
  const customerDeleteDialog = ref(false)
  const customerDeleteTarget = ref(null)
  const customerSaving = ref(false)
  const editingCustomer = ref(null)
  const customerFormRef = ref(null)

  const customerForm = ref({
    comp_name: '',
    cust_addr: '',
    contact_name: '',
    contact_email: '',
    contact_phone: '',
    is_active: true
  })

  const customerHeaders = [
    { title: 'Company', value: 'comp_name', width: '25%' },
    { title: 'Contact Person', value: 'contact_name', width: '16%' },
    { title: 'Email', value: 'contact_email', width: '20%' },
    { title: 'Phone', value: 'contact_phone', width: '14%' },
    { title: 'Status', value: 'is_active', width: '10%' },
    { title: 'Created', value: 'created_at', width: '11%' },
    { title: '', value: 'actions', width: '4%', sortable: false }
  ]

  const filteredCustomers = computed(() => {
    const q = customerSearch.value?.toLowerCase() ?? ''
    const list = customerStore.customers ?? []
    if (!q) return list
    return list.filter(c =>
      c.comp_name?.toLowerCase().includes(q) ||
      c.contact_name?.toLowerCase().includes(q) ||
      c.contact_email?.toLowerCase().includes(q)
    )
  })

  // ── Customer CRUD ─────────────────────────────────────────────────────────────
  function openCustomerDialog(customer = null) {
    editingCustomer.value = customer
    customerForm.value = customer
      ? {
        comp_name: customer.comp_name ?? '',
        cust_addr: customer.cust_addr ?? '',
        contact_name: customer.contact_name ?? '',
        contact_email: customer.contact_email ?? '',
        contact_phone: customer.contact_phone ?? '',
        is_active: customer.is_active ?? true
      }
      : { comp_name: '', cust_addr: '', contact_name: '', contact_email: '', contact_phone: '', is_active: true }
    customerDialog.value = true
  }

  async function saveCustomer() {
    const validation = await customerFormRef.value?.validate()
    if (!validation?.valid) return

    customerSaving.value = true
    try {
      let result
      if (editingCustomer.value) {
        result = await customerStore.updateCustomer(editingCustomer.value.cust_id, customerForm.value)
      } else {
        result = await customerStore.createCustomer(customerForm.value)
      }

      if (result?.success) {
        await customerStore.fetchCustomers()
        customerDialog.value = false
      }
    } finally {
      customerSaving.value = false
    }
  }

  function confirmDeleteCustomer(customer) {
    customerDeleteTarget.value = customer
    customerDeleteDialog.value = true
  }

  async function doDeleteCustomer() {
    if (!customerDeleteTarget.value) return
    customerSaving.value = true
    try {
      await customerStore.updateCustomer(customerDeleteTarget.value.cust_id, {
        ...customerDeleteTarget.value,
        is_active: false
      })
      await customerStore.fetchCustomers()
      customerDeleteDialog.value = false
    } finally {
      customerSaving.value = false
      customerDeleteTarget.value = null
    }
  }

  // Load customers when tab activates
  watch(activeTab, async (tab) => {
    if (tab === 'customers' && (!customerStore.customers || customerStore.customers.length === 0)) {
      customersLoading.value = true
      try {
        await customerStore.fetchCustomers()
      } catch (e) {
        console.error('Failed to load customers:', e)
      } finally {
        customersLoading.value = false
      }
    }
  })

  // ── File explorer helpers (all unchanged from original) ───────────────────────
  async function loadStructure() {
    try {
      loading.value = true
      const res = await api.get('/file/directory-structure')
      const payload = res?.data ?? res
      rawTree.value = Array.isArray(payload)
        ? payload
        : (Array.isArray(payload?.data) ? payload.data : [])
    } catch (err) {
      console.error('Failed to load physical structure:', err)
      rawTree.value = []
    } finally {
      loading.value = false
    }
  }

  function getFileExt(item) {
    return (item.name || '').split('.').pop().toLowerCase()
  }

  function resolvePreviewType(ext) {
    for (const [type, exts] of Object.entries(PREVIEWABLE_TYPES)) {
      if (exts.includes(ext)) return type
    }
    return null
  }

  function fileApiPath(item) {
    return item.file_id
      ? `/api/file/download/${item.file_id}`
      : `/api/file/download-physical?path=${encodeURIComponent(item.path)}`
  }

  function revokeBlobUrl() {
    if (previewBlobUrl.value) {
      URL.revokeObjectURL(previewBlobUrl.value)
      previewBlobUrl.value = ''
    }
  }

  async function handlePreview(item) {
    const ext = getFileExt(item)
    const type = resolvePreviewType(ext)

    if (previewItem.value?.id === item.id) { closePreview(); return }

    if (!type) {
      const hint = EXTERNAL_HINTS[ext] ?? { icon: 'mdi-file-outline', color: 'grey', app: 'an external application' }
      externalPrompt.value = {
        show: true, item,
        title: `"${item.name}" cannot be previewed`,
        subtitle: `This file requires ${hint.app} to open.`,
        icon: hint.icon, iconColor: hint.color
      }
      return
    }

    revokeBlobUrl()
    previewItem.value = item
    previewType.value = type
    previewTextContent.value = ''
    previewLoading.value = true

    try {
      if (type === 'text') {
        const res = await api.get(fileApiPath(item), { responseType: 'text' })
        previewTextContent.value = res?.data ?? res
      } else {
        const res = await api.get(fileApiPath(item), { responseType: 'arraybuffer' })
        const buffer = res?.data ?? res
        const mime = item.content_type || mimeForExt(ext)
        const blob = new Blob([buffer], { type: mime })
        previewBlobUrl.value = URL.createObjectURL(blob)
      }
    } catch (err) {
      console.error('Preview failed:', err)
      previewTextContent.value = 'Failed to load file content.'
      previewType.value = 'text'
    } finally {
      previewLoading.value = false
    }
  }

  function closePreview() {
    revokeBlobUrl()
    previewItem.value = null
    previewType.value = ''
    previewTextContent.value = ''
  }

  function previewIconFor(item) {
    const type = resolvePreviewType(getFileExt(item))
    if (type === 'pdf') return 'mdi-file-pdf-box'
    if (type === 'image') return 'mdi-image-outline'
    if (type === 'text') return 'mdi-file-document-outline'
    return 'mdi-file-outline'
  }

  function mimeForExt(ext) {
    const map = {
      pdf: 'application/pdf',
      png: 'image/png', jpg: 'image/jpeg', jpeg: 'image/jpeg',
      gif: 'image/gif', webp: 'image/webp', bmp: 'image/bmp', svg: 'image/svg+xml',
      txt: 'text/plain', csv: 'text/csv', md: 'text/plain', log: 'text/plain',
    }
    return map[ext] ?? 'application/octet-stream'
  }

  onBeforeUnmount(() => { revokeBlobUrl() })

  watch(search, (val) => {
    if (!val) return
    const lowerSearch = val.toLowerCase()
    const expanded = new Set(openedIds.value || [])

    function findMatches(nodes, pathIds) {
      if (!nodes) return false
      let hasMatch = false
      for (const node of nodes) {
        const currentPath = [...pathIds, node.id]
        let nodeMatches = (node.name || '').toLowerCase().includes(lowerSearch)
        if (node.children?.length && findMatches(node.children, currentPath)) nodeMatches = true
        if (nodeMatches) { hasMatch = true; currentPath.forEach(id => expanded.add(id)) }
      }
      return hasMatch
    }

    findMatches(rawTree.value || [], [])
    openedIds.value = Array.from(expanded)
  })

  const filteredTree = computed(() => {
    const q = search.value?.toLowerCase() ?? ''

    function applySort(items) {
      if (!Array.isArray(items)) return
      items.sort((a, b) => {
        const nameA = a.name || '', nameB = b.name || ''
        if (a.type !== 'file' && b.type !== 'file') return nameA.localeCompare(nameB)
        if (a.type !== 'file') return -1
        if (b.type !== 'file') return 1
        switch (sortBy.value) {
          case 'Name (A-Z)': return nameA.localeCompare(nameB)
          case 'Name (Z-A)': return nameB.localeCompare(nameA)
          case 'Newest': return new Date(b.uploaded_at || 0) - new Date(a.uploaded_at || 0)
          case 'Oldest': return new Date(a.uploaded_at || 0) - new Date(b.uploaded_at || 0)
          case 'Size (Large)': return (b.size || 0) - (a.size || 0)
          case 'Size (Small)': return (a.size || 0) - (b.size || 0)
          default: return 0
        }
      })
      items.forEach(i => { if (i.children?.length) applySort(i.children) })
    }

    function filterSearch(items) {
      if (!q) return items
      return items.map(item => {
        const matchSelf = (item.name || '').toLowerCase().includes(q)
        const filteredChildren = item.children ? filterSearch(item.children) : []
        return (matchSelf || filteredChildren.length > 0)
          ? { ...item, children: filteredChildren }
          : null
      }).filter(Boolean)
    }

    const clone = JSON.parse(JSON.stringify(rawTree.value || []))
    applySort(clone)
    return q ? filterSearch(clone) : clone
  })

  function updateBreadcrumb(activeIds) {
    if (!activeIds?.length) { breadcrumbs.value = [{ title: 'Files' }]; return }
    const targetId = activeIds[0]

    function findNodePath(nodes, id, currentPath = []) {
      for (const node of nodes) {
        const path = [...currentPath, node]
        if (node.id === id) return path
        if (node.children?.length) {
          const found = findNodePath(node.children, id, path)
          if (found) return found
        }
      }
      return null
    }

    const pathNodes = findNodePath(rawTree.value, targetId)
    breadcrumbs.value = pathNodes
      ? [{ title: 'Files' }, ...pathNodes.map(n => ({ title: n.name }))]
      : [{ title: 'Files' }, { title: String(targetId).split(/[/\\]/).pop() }]
  }

  function downloadFile(item) {
    if (item.file_id) {
      window.open(`/api/file/download/${item.file_id}`, '_blank')
    } else {
      window.open(`/api/file/download-physical?path=${encodeURIComponent(item.path)}`, '_blank')
    }
  }

  async function deleteFile(item) {
    if (!confirm('Delete this file? This cannot be undone.')) return
    if (previewItem.value?.id === item.id) closePreview()
    try {
      if (item.file_id) {
        await api.delete(`/file/${item.file_id}`)
      } else {
        await api.delete(`/file/delete-physical?path=${encodeURIComponent(item.path)}`)
      }
      await loadStructure()
    } catch (err) {
      console.error('Delete failed:', err)
    }
  }

  function formatSize(bytes) {
    if (!bytes) return '0 B'
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(1024))
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${sizes[i]}`
  }

  function formatDate(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }

  function countFiles(node) {
    if (!node?.children) return 0
    return node.children.reduce((acc, child) =>
      acc + (child.type === 'file' ? 1 : countFiles(child)), 0)
  }

  function getInitials(name) {
    if (!name) return '?'
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
  }

  onMounted(() => { loadStructure() })
</script>

<style scoped>
  .page-container {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
  }

  /* Customers table — fixed header, scrollable body */
  .customers-table :deep(th) {
    font-weight: 600 !important;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    background: #fafbfc !important;
    color: rgba(0,0,0,0.55) !important;
  }

  .customers-table :deep(.v-table__wrapper) {
    height: 100% !important;
    overflow-y: auto;
  }

  .customers-table :deep(tbody tr:hover td) {
    background-color: rgba(0,0,0,0.025) !important;
  }

  /* Window items need height */
  :deep(.v-window__container),
  :deep(.v-window-item) {
    height: 100% !important;
  }

  .fill-height {
    height: 100%;
  }

  .slide-preview-enter-active,
  .slide-preview-leave-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
  }

  .slide-preview-enter-from,
  .slide-preview-leave-to {
    opacity: 0;
    transform: translateX(16px);
  }
</style>
