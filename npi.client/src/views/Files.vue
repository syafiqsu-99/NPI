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
        <div class="d-flex ga-3">
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
      </v-card-title>

      <!-- Breadcrumbs -->
      <v-card-text class="px-4 d-flex align-center flex-shrink-0 py-2"
                   style="flex: 0 0 24px; overflow: hidden;">
        <v-breadcrumbs :items="breadcrumbs"
                       class="pa-0 w-100"
                       style="white-space: nowrap; flex-wrap: nowrap; overflow: hidden; text-overflow: ellipsis;" />
      </v-card-text>

      <!-- Loading bar -->
      <div style="flex: 0 0 2px; height: 2px;">
        <v-progress-linear v-if="loading" indeterminate color="primary" height="2" />
      </div>
      <v-divider style="flex: 0 0 1px;" />

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

                <!-- Preview — available to everyone -->
                <v-tooltip text="Preview" location="top">
                  <template #activator="{ props }">
                    <v-btn v-bind="props" icon size="x-small" variant="text"
                           :color="previewItem?.id === item.id ? 'primary' : ''"
                           @click.stop="handlePreview(item)">
                      <v-icon size="16">mdi-eye-outline</v-icon>
                    </v-btn>
                  </template>
                </v-tooltip>

                <!-- Download — available to everyone -->
                <v-tooltip text="Download" location="top">
                  <template #activator="{ props }">
                    <v-btn v-bind="props" icon size="x-small" variant="text"
                           @click.stop="downloadFile(item)">
                      <v-icon size="16">mdi-download-outline</v-icon>
                    </v-btn>
                  </template>
                </v-tooltip>

                <!-- Delete — Admin / Manager only -->
                <v-tooltip text="Delete" location="top" v-if="canManageFiles && item.can_edit">
                  <template #activator="{ props }">
                    <v-btn v-bind="props" icon size="x-small" variant="text" color="error"
                           @click.stop="deleteFile(item)">
                      <v-icon size="16">mdi-delete-outline</v-icon>
                    </v-btn>
                  </template>
                </v-tooltip>
              </div>

              <!-- Folder badge -->
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

            <!-- Panel header -->
            <div class="d-flex align-center px-4 py-2 bg-grey-lighten-4"
                 style="border-bottom:1px solid #e0e0e0; flex-shrink:0;">
              <v-icon class="mr-2" color="primary" size="18">{{ previewIconFor(previewItem) }}</v-icon>
              <span class="text-body-2 font-weight-medium text-truncate flex-grow-1"
                    :title="previewItem.name">{{ previewItem.name }}</span>
              <v-btn icon size="x-small" variant="text" class="ml-2" @click="closePreview">
                <v-icon size="18">mdi-close</v-icon>
              </v-btn>
            </div>

            <!-- Panel content -->
            <div class="flex-grow-1" style="overflow:hidden; position:relative;">

              <!-- Loading -->
              <div v-if="previewLoading"
                   class="d-flex align-center justify-center fill-height">
                <v-progress-circular indeterminate color="primary" />
              </div>

              <!--
                PDF — blob URL fed into iframe so the browser renders it inline.
                Using a blob URL (rather than the raw API URL) forces the browser's
                built-in PDF viewer instead of triggering a download.
              -->
              <iframe v-else-if="previewType === 'pdf'"
                      :src="previewBlobUrl"
                      width="100%" height="100%"
                      style="border:none; display:block;" />

              <!--
                Image — blob URL fed into <img>.
                Blob URLs bypass any Content-Disposition: attachment header the
                server might return, so the image always renders inline.
              -->
              <div v-else-if="previewType === 'image'"
                   class="d-flex align-center justify-center fill-height pa-4"
                   style="background:#f0f0f0;">
                <img :src="previewBlobUrl"
                     :alt="previewItem.name"
                     style="max-width:100%; max-height:100%; object-fit:contain;
                            border-radius:4px; box-shadow:0 2px 8px rgba(0,0,0,0.15);" />
              </div>

              <!-- Plain text / CSV -->
              <div v-else-if="previewType === 'text'"
                   style="height:100%; overflow:auto; padding:16px;">
                <pre style="font-size:12px; font-family:monospace; white-space:pre-wrap;
                            word-break:break-all; margin:0;">{{ previewTextContent }}</pre>
              </div>

            </div>
          </div>
        </transition>

      </v-card-text>
    </v-card>

    <!--
      External software snackbar.
      Layout: icon + text block stacked, then action buttons on a second row.
      This prevents long filenames from squeezing the buttons off-screen.
    -->
    <v-snackbar v-model="externalPrompt.show"
                location="bottom center"
                :timeout="6000"
                color="surface"
                elevation="4"
                min-width="320"
                max-width="480">
      <div class="d-flex flex-column ga-2">

        <!-- Top row: icon + text -->
        <div class="d-flex align-start ga-3">
          <v-icon :color="externalPrompt.iconColor" size="22" class="mt-1 flex-shrink-0">
            {{ externalPrompt.icon }}
          </v-icon>
          <div style="min-width:0;">
            <div class="text-body-2 font-weight-medium"
                 style="word-break:break-word;">{{ externalPrompt.title }}</div>
            <div class="text-caption text-grey">{{ externalPrompt.subtitle }}</div>
          </div>
        </div>

        <!-- Bottom row: actions flush-right -->
        <div class="d-flex justify-end align-center ga-2">
          <v-btn size="small" color="primary" variant="tonal"
                 @click="downloadFile(externalPrompt.item); externalPrompt.show = false">
            <v-icon start size="14">mdi-download-outline</v-icon>
            Download to open
          </v-btn>
          <v-btn size="small" icon variant="text"
                 @click="externalPrompt.show = false">
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

  const authStore = useAuthStore()

  const canManageFiles = computed(() =>
    ['Admin', 'Manager'].includes(authStore.user?.role_name ?? '')
  )

  const loading = ref(false)
  const rawTree = ref([])
  const search = ref('')
  const sortBy = ref('Name (A-Z)')
  const breadcrumbs = ref([{ title: 'Files' }])
  const openedIds = ref([])

  const previewItem = ref(null)
  const previewType = ref('')
  const previewBlobUrl = ref('')   // revokable blob URL for PDF and image
  const previewTextContent = ref('')
  const previewLoading = ref(false)

  const externalPrompt = ref({
    show: false, item: null, title: '', subtitle: '', icon: 'mdi-application', iconColor: 'primary'
  })

  const sortOptions = [
    'Name (A-Z)', 'Name (Z-A)', 'Newest', 'Oldest', 'Size (Large)', 'Size (Small)'
  ]

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

  // ── Load ──────────────────────────────────────────────────────────────────────

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

  // ── Preview ───────────────────────────────────────────────────────────────────

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

  // Revoke the current blob URL to free memory before replacing it
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

    // Reset panel state
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
        // Fetch as arraybuffer so we control the blob MIME type.
        // This ensures the browser renders PDF/image inline rather than downloading.
        const res = await api.get(fileApiPath(item), { responseType: 'arraybuffer' })
        const buffer = res?.data ?? res

        // Derive MIME: prefer the DB content_type, fall back to extension mapping
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

  // Minimal MIME fallback for common previewable extensions
  function mimeForExt(ext) {
    const map = {
      pdf: 'application/pdf',
      png: 'image/png',
      jpg: 'image/jpeg', jpeg: 'image/jpeg',
      gif: 'image/gif',
      webp: 'image/webp',
      bmp: 'image/bmp',
      svg: 'image/svg+xml',
      txt: 'text/plain',
      csv: 'text/csv',
      md: 'text/plain',
      log: 'text/plain',
    }
    return map[ext] ?? 'application/octet-stream'
  }

  // Free blob URL when the component is destroyed
  onBeforeUnmount(() => { revokeBlobUrl() })

  // ── Search Auto-Expand ────────────────────────────────────────────────────────

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

  // ── Filter + Sort ─────────────────────────────────────────────────────────────

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

  // ── Actions ───────────────────────────────────────────────────────────────────

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

  // ── Utilities ─────────────────────────────────────────────────────────────────

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

  onMounted(() => { loadStructure() })
</script>

<style scoped>
  .page-container {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
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
