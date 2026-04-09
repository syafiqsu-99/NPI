<template>
  <v-container fluid class="page-container pa-6 d-flex flex-column">
    <v-card class="page-header-card flex-grow-1 d-flex flex-column overflow-hidden" elevation="2">

      <!-- Header -->
      <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
        <div class="page-title">
          <v-icon class="mr-2">mdi-file-document</v-icon>
          Files
        </div>
        <div class="d-flex ga-3">
          <v-text-field v-model="search"
                        label="Search"
                        density="compact"
                        clearable
                        prepend-inner-icon="mdi-magnify"
                        variant="outlined"
                        hide-details
                        style="max-width:220px" />
          <v-select v-model="sortBy"
                    :items="sortOptions"
                    label="Sort"
                    density="compact"
                    variant="outlined"
                    hide-details
                    style="max-width:180px" />
        </div>
      </v-card-title>

      <!-- Breadcrumb + loading -->
      <v-card-text class="py-2 px-4 flex-shrink-0">
        <v-breadcrumbs :items="breadcrumbs" class="pa-0" />
      </v-card-text>

      <v-progress-linear v-if="loading" indeterminate color="primary" height="2" />
      <v-divider />

      <!-- Tree -->
      <v-card-text class="flex-grow-1 pa-0" style="min-height:0; overflow:hidden;">

        <div v-if="!loading && filteredTree.length === 0 && !search"
             class="d-flex flex-column align-center justify-center fill-height pa-8">
          <v-icon size="72" color="grey-lighten-1">mdi-folder-open-outline</v-icon>
          <div class="text-h6 text-grey-darken-1 mt-4">No files found</div>
        </div>

        <div v-else-if="!loading && filteredTree.length === 0 && search"
             class="d-flex flex-column align-center justify-center fill-height pa-8">
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
                    style="height:100%; overflow-y:auto;"
                    @update:opened="handleOpen"
                    @update:activated="updateBreadcrumb">

          <template #prepend="{ item }">
            <v-icon v-if="item.type === 'root-projects'" color="primary">mdi-folder-multiple</v-icon>
            <v-icon v-else-if="item.type === 'root-customers'" color="teal">mdi-account-group</v-icon>
            <v-icon v-else-if="item.type === 'project'" color="amber-darken-2">mdi-folder</v-icon>
            <v-icon v-else-if="item.type === 'customer'" color="teal-darken-1">mdi-folder-account</v-icon>
            <v-icon v-else-if="item.type === 'department'" color="blue-darken-1">mdi-folder-open</v-icon>
            <v-icon v-else color="grey-darken-1">mdi-file-document-outline</v-icon>
          </template>

          <template #append="{ item }">
            <!-- File actions -->
            <div v-if="item.type === 'file'" class="d-flex align-center ga-1">
              <v-chip size="x-small" variant="tonal" color="secondary" class="mr-1">
                v{{ item.file_version }}
              </v-chip>
              <span class="text-caption text-grey mr-2">{{ formatSize(item.file_size) }}</span>
              <span class="text-caption text-grey mr-3">{{ formatDate(item.uploaded_at) }}</span>

              <v-tooltip text="Preview" location="top">
                <template #activator="{ props }">
                  <v-btn v-bind="props" icon size="x-small" variant="text"
                         @click.stop="previewFile(item)">
                    <v-icon size="16">mdi-eye-outline</v-icon>
                  </v-btn>
                </template>
              </v-tooltip>

              <v-tooltip text="Download" location="top">
                <template #activator="{ props }">
                  <v-btn v-bind="props" icon size="x-small" variant="text"
                         @click.stop="downloadFile(item.file_id)">
                    <v-icon size="16">mdi-download-outline</v-icon>
                  </v-btn>
                </template>
              </v-tooltip>

              <v-tooltip text="Delete" location="top">
                <template #activator="{ props }">
                  <v-btn v-bind="props" icon size="x-small" variant="text" color="error"
                         @click.stop="deleteFile(item.file_id, item.project_id)">
                    <v-icon size="16">mdi-delete-outline</v-icon>
                  </v-btn>
                </template>
              </v-tooltip>
            </div>

            <!-- Folder file count badge -->
            <v-chip v-else-if="item.children?.length"
                    size="x-small" variant="tonal" color="primary">
              {{ countFiles(item) }}
            </v-chip>
          </template>
        </v-treeview>
      </v-card-text>
    </v-card>

    <!-- Preview dialog -->
    <v-dialog v-model="previewDialog" max-width="700" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
          {{ previewFileName }}
          <v-btn icon="mdi-close" variant="text" color="white"
                 @click="previewDialog = false" />
        </v-card-title>
        <v-card-text style="height:70vh;">
          <iframe v-if="previewType === 'pdf'" :src="previewUrl"
                  width="100%" height="100%" style="border:none;" />
          <v-img v-else-if="previewType === 'image'" :src="previewUrl" contain height="100%" />
          <div v-else class="d-flex align-center justify-center fill-height text-grey">
            Preview not available for this file type.
          </div>
        </v-card-text>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { api } from '@/utils/api'

  const loading = ref(false)
  const rawProjects = ref([])       // project nodes (lazy-loaded)
  const rawCustomers = ref([])      // customer file nodes (eager-loaded)
  const search = ref('')
  const sortBy = ref('Name (A-Z)')
  const breadcrumbs = ref([{ title: 'Files' }])
  const openedIds = ref([])

  const previewDialog = ref(false)
  const previewUrl = ref('')
  const previewType = ref('')
  const previewFileName = ref('')

  const sortOptions = [
    'Name (A-Z)', 'Name (Z-A)', 'Newest', 'Oldest', 'Size (Large)', 'Size (Small)'
  ]

  // ── Root tree structure ─────────────────────────────────────────────────────

  const tree = computed(() => [
    {
      id: 'root-projects',
      name: 'Projects',
      type: 'root-projects',
      children: rawProjects.value
    },
    {
      id: 'root-customers',
      name: 'Customers',
      type: 'root-customers',
      children: rawCustomers.value
    }
  ])

  // ── Load projects (lazy — files fetched when node opened) ───────────────────

  async function loadProjects() {
    try {
      const res = await api.get('/project')
      const projects = res?.data ?? (Array.isArray(res) ? res : [])
      rawProjects.value = projects.map(p => ({
        id: `project-${p.proj_id}`,
        name: p.proj_name,
        type: 'project',
        project_id: p.proj_id,
        children: [],
        loaded: false
      }))
    } catch (err) {
      console.error('Failed to load projects:', err)
    }
  }

  // ── Load customer files (eager — small dataset) ─────────────────────────────

  async function loadCustomerFiles() {
    try {
      const res = await api.get('/file/by-customers')
      const files = res?.data ?? []

      // Group by customer name (dept_name carries customer name from API)
      const customerMap = new Map()
      for (const file of files) {
        const custName = file.dept_name || 'Unknown Customer'
        if (!customerMap.has(custName)) {
          customerMap.set(custName, {
            id: `customer-${custName}`,
            name: custName,
            type: 'customer',
            children: []
          })
        }
        customerMap.get(custName).children.push(buildFileNode(file))
      }

      rawCustomers.value = [...customerMap.values()]
        .sort((a, b) => a.name.localeCompare(b.name))
    } catch (err) {
      console.error('Failed to load customer files:', err)
    }
  }

  // ── Lazy-load project files when project node is expanded ───────────────────

  async function handleOpen(openIds) {
    for (const id of openIds) {
      const projectNode = rawProjects.value.find(p => p.id === id)
      if (!projectNode || projectNode.loaded) continue

      try {
        const res = await api.get(`/file/by-project/${projectNode.project_id}`)
        const files = res?.data ?? []

        // Group by department
        const deptMap = new Map()
        for (const file of files) {
          const deptKey = file.dept_name?.trim() || 'General'
          if (!deptMap.has(deptKey)) {
            deptMap.set(deptKey, {
              id: `dept-${projectNode.project_id}-${deptKey}`,
              name: deptKey,
              type: 'department',
              project_id: projectNode.project_id,
              children: []
            })
          }
          deptMap.get(deptKey).children.push(buildFileNode(file))
        }

        projectNode.children = [...deptMap.values()]
          .sort((a, b) => a.name.localeCompare(b.name))
          .map(dept => ({
            ...dept,
            children: dept.children.sort(
              (a, b) => new Date(b.uploaded_at) - new Date(a.uploaded_at)
            )
          }))

        projectNode.loaded = true
      } catch (err) {
        console.error(`Failed to load files for project ${projectNode.project_id}:`, err)
      }
    }
  }

  function buildFileNode(file) {
    return {
      id: `file-${file.file_id}`,
      name: file.file_name,
      type: 'file',
      file_id: file.file_id,
      file_size: file.file_size,
      file_version: file.file_version,
      uploaded_at: file.uploaded_at,
      project_id: file.proj_id,
      content_type: file.file_type
    }
  }

  // ── Filter + sort ───────────────────────────────────────────────────────────

  const filteredTree = computed(() => {
    const q = search.value?.toLowerCase() ?? ''

    function applySort(items) {
      items.sort((a, b) => {
        if (a.type !== 'file' && b.type !== 'file') return a.name.localeCompare(b.name)
        if (a.type !== 'file') return -1
        if (b.type !== 'file') return 1
        switch (sortBy.value) {
          case 'Name (A-Z)': return a.name.localeCompare(b.name)
          case 'Name (Z-A)': return b.name.localeCompare(a.name)
          case 'Newest': return new Date(b.uploaded_at) - new Date(a.uploaded_at)
          case 'Oldest': return new Date(a.uploaded_at) - new Date(b.uploaded_at)
          case 'Size (Large)': return (b.file_size || 0) - (a.file_size || 0)
          case 'Size (Small)': return (a.file_size || 0) - (b.file_size || 0)
          default: return 0
        }
      })
      items.forEach(i => i.children?.length && applySort(i.children))
    }

    function filterSearch(items) {
      if (!q) return items
      return items
        .map(item => {
          const matchSelf = item.name.toLowerCase().includes(q)
          const filteredChildren = item.children ? filterSearch(item.children) : []
          if (matchSelf || filteredChildren.length)
            return { ...item, children: filteredChildren }
          return null
        })
        .filter(Boolean)
    }

    const clone = JSON.parse(JSON.stringify(tree.value))
    applySort(clone)
    return q ? filterSearch(clone) : clone
  })

  // ── Actions ─────────────────────────────────────────────────────────────────

  function updateBreadcrumb(activeIds) {
    breadcrumbs.value = activeIds.length
      ? [{ title: 'Files' }, ...activeIds.map(id => ({ title: id }))]
      : [{ title: 'Files' }]
  }

  function downloadFile(id) {
    window.open(`/api/file/download/${id}`, '_blank')
  }

  async function deleteFile(id) {
    if (!confirm('Delete this file? This cannot be undone.')) return
    try {
      await api.delete(`/file/${id}`)
      // Reset lazy-load state so files refresh on next open
      rawProjects.value.forEach(p => { p.loaded = false; p.children = [] })
      await loadProjects()
      await loadCustomerFiles()
    } catch (err) {
      console.error('Delete failed:', err)
    }
  }

  function previewFile(file) {
    previewFileName.value = file.name
    previewUrl.value = `/api/file/download/${file.file_id}`
    previewType.value = file.content_type?.includes('pdf') ? 'pdf'
      : file.content_type?.includes('image') ? 'image'
        : 'other'
    previewDialog.value = true
  }

  // ── Utilities ────────────────────────────────────────────────────────────────

  function formatSize(bytes) {
    if (!bytes) return '0 B'
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(1024))
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${sizes[i]}`
  }

  function formatDate(d) {
    if (!d) return 'N/A'
    return new Date(d).toLocaleDateString('en-GB', {
      day: '2-digit', month: 'short', year: 'numeric'
    })
  }

  function countFiles(node) {
    if (!node.children) return 0
    return node.children.reduce((acc, child) =>
      acc + (child.type === 'file' ? 1 : countFiles(child)), 0)
  }

  // ── Lifecycle ────────────────────────────────────────────────────────────────

  onMounted(async () => {
    loading.value = true
    await Promise.all([loadProjects(), loadCustomerFiles()])
    loading.value = false
  })
</script>

<style scoped>
  .page-container {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
  }
</style>
