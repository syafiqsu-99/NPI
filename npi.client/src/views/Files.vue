<template>
  <v-container fluid class="page-container pa-6 d-flex flex-column">
    <v-row class="mb-4">
      <v-col cols="12">
        <v-card class="page-header-card" elevation="2">
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
                            style="max-width: 220px" />

              <v-select v-model="sortBy"
                        :items="sortOptions"
                        label="Sort"
                        density="compact"
                        variant="outlined"
                        hide-details
                        style="max-width: 180px" />
            </div>
          </v-card-title>
          <v-card-text class="pa-0">
            <v-card class="page-card d-flex flex-column">
              <v-breadcrumbs :items="breadcrumbs" class="mb-3" />

              <v-progress-linear v-if="loading"
                                 indeterminate
                                 color="primary"
                                 height="2"
                                 class="flex-shrink-0" />
              <div v-else style="height: 2px;" class="flex-shrink-0" />

              <v-divider class="flex-shrink-0" />

              <v-card-text class="flex-grow-1 pa-0" style="min-height: 0; overflow: hidden;">

                <div v-if="!loading && tree.length === 0"
                     class="d-flex flex-column align-center justify-center fill-height"
                     style="height: 100%;">
                  <v-icon size="72" color="grey-lighten-1" class="mb-4">mdi-folder-open-outline</v-icon>
                  <div class="text-h6 text-grey-darken-1 mb-1">No projects found</div>
                  <div class="text-body-2 text-grey">Projects and their files will appear here once created.</div>
                </div>

                <div v-else-if="!loading && filteredTree.length === 0 && search"
                     class="d-flex flex-column align-center justify-center fill-height"
                     style="height: 100%;">
                  <v-icon size="72" color="grey-lighten-1" class="mb-4">mdi-file-search-outline</v-icon>
                  <div class="text-h6 text-grey-darken-1 mb-1">No results for "{{ search }}"</div>
                  <div class="text-body-2 text-grey">Try a different search term or clear the filter.</div>
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
                            style="height: 100%; overflow-y: auto;"
                            @update:opened="handleOpen"
                            @update:activated="updateBreadcrumb">
                  <template #prepend="{ item }">
                    <v-icon v-if="item.type === 'project'" color="amber-darken-2">mdi-folder</v-icon>
                    <v-icon v-else-if="item.type === 'task'" color="orange-lighten-1">mdi-folder-outline</v-icon>
                    <v-icon v-else color="blue-darken-1">mdi-file-document-outline</v-icon>
                  </template>

                  <template #append="{ item }">
                    <div v-if="item.type === 'file'" class="d-flex align-center ga-1">
                      <v-chip size="x-small" variant="tonal" color="secondary" class="mr-1">
                        v{{ item.file_version }}
                      </v-chip>
                      <span class="text-caption text-grey mr-2">{{ formatSize(item.file_size) }}</span>
                      <span class="text-caption text-grey mr-3">{{ formatDate(item.uploaded_at) }}</span>

                      <v-tooltip text="Preview" location="top">
                        <template #activator="{ props }">
                          <v-btn v-bind="props" icon size="x-small" variant="text" @click.stop="previewFile(item)">
                            <v-icon size="16">mdi-eye-outline</v-icon>
                          </v-btn>
                        </template>
                      </v-tooltip>

                      <v-tooltip text="Download" location="top">
                        <template #activator="{ props }">
                          <v-btn v-bind="props" icon size="x-small" variant="text" @click.stop="downloadFile(item.file_id)">
                            <v-icon size="16">mdi-download-outline</v-icon>
                          </v-btn>
                        </template>
                      </v-tooltip>

                      <v-tooltip text="Delete" location="top">
                        <template #activator="{ props }">
                          <v-btn v-bind="props" icon size="x-small" variant="text" color="error" @click.stop="deleteFile(item.file_id, item.project_id)">
                            <v-icon size="16">mdi-delete-outline</v-icon>
                          </v-btn>
                        </template>
                      </v-tooltip>
                    </div>

                    <v-chip v-else-if="item.children && item.children.length"
                            size="x-small"
                            variant="tonal"
                            color="primary">
                      {{ countFiles(item) }}
                    </v-chip>
                  </template>
                </v-treeview>

              </v-card-text>
            </v-card>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-dialog v-model="previewDialog" max-width="600" persistent>
      <v-card class="dialog-card">
        <v-card-title class="bg-primary text-white d-flex align-center justify-space-between">
          {{ previewFileName }}
        </v-card-title>

        <v-card-text>

          <iframe v-if="previewType === 'pdf'"
                  :src="previewUrl"
                  width="100%"
                  height="100%" />

          <v-img v-else-if="previewType === 'image'"
                 :src="previewUrl"
                 contain
                 height="100%" />

          <div v-else>
            Preview not available
          </div>

        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn text @click="previewDialog=false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { api } from '@/utils/api'

  const loading = ref(false)
  const tree = ref([])
  const search = ref('')
  const sortBy = ref('Name (A-Z)')
  const breadcrumbs = ref([])
  const previewDialog = ref(false)
  const previewUrl = ref('')
  const previewType = ref('')
  const previewFileName = ref('')
  const openedIds = ref([])

  const sortOptions = [
    'Name (A-Z)',
    'Name (Z-A)',
    'Newest',
    'Oldest',
    'Size (Large)',
    'Size (Small)'
  ]

  const loadProjects = async () => {
    loading.value = true
    try {
      const res = await api.get('/Project')
      const projects = res.data

      tree.value = projects.map(p => ({
        id: `project-${p.proj_id}`,
        name: p.proj_name || `Project ${p.proj_id}`,
        type: 'project',
        project_id: p.proj_id,
        children: [],
        loaded: false
      }))
      console.log('Projects loaded:', JSON.parse(JSON.stringify(tree.value)))
    } catch (e) {
      console.error('Failed to load projects:', e)
    } finally {
      loading.value = false
    }
  }

  const findNodeRecursively = (nodes, id) => {
    for (const node of nodes) {
      if (node.id === id) return node;
      if (node.children) {
        const found = findNodeRecursively(node.children, id);
        if (found) return found;
      }
    }
    return null;
  }

  const handleOpen = async (openIds) => {
    console.log('--- Tree Folder Expanded ---')
    console.log('Currently Opened IDs:', openIds)

    for (const id of openIds) {
      const node = findNodeRecursively(tree.value, id)

      if (node && node.type === 'project' && !node.loaded) {
        console.log(`Loading contents for project: ${node.name} (ID: ${node.project_id})`)
        loading.value = true // Display the progress bar at the top

        try {
          const res = await api.get(`/File/by-project/${node.project_id}`)
          console.log(`API Response for project ${node.project_id}:`, res.data)

          const files = res.data.data || []
          console.log(`Found ${files.length} files. Mapping folder structure...`)

          const taskMap = {}
          const standaloneFiles = []

          files.forEach(file => {
            const fileNode = {
              id: `file-${file.file_id}`,
              name: file.file_name || `Unknown File ${file.file_id}`,
              type: 'file',
              file_id: file.file_id,
              file_size: file.file_size,
              file_version: file.file_version,
              uploaded_at: file.uploaded_at,
              project_id: file.proj_id,
              content_type: file.file_type
            }

            if (file.task_id) {
              if (!taskMap[file.task_id]) {
                taskMap[file.task_id] = {
                  id: `task-${file.task_id}`,
                  name: file.task_name || `Task ${file.task_id}`,
                  type: 'task',
                  children: []
                }
              }
              taskMap[file.task_id].children.push(fileNode)
            } else {
              standaloneFiles.push(fileNode)
            }
          })

          console.log('Files mapped without a Task folder:', standaloneFiles)
          console.log('Task folders created:', Object.values(taskMap))

          // Append files and task folders safely to the reactive state
          node.children.push(...standaloneFiles)
          node.children.push(...Object.values(taskMap))
          node.loaded = true

          console.log('Node state successfully updated:', JSON.parse(JSON.stringify(node)))

        } catch (error) {
          console.error(`Error loading files for project ${node.project_id}:`, error)
        } finally {
          loading.value = false
        }
      }
    }
  }

  const filteredTree = computed(() => {
    // Check for null errors during the clone
    let clone = []
    try {
      clone = JSON.parse(JSON.stringify(tree.value))
    } catch (e) {
      console.error('Failed to clone tree:', e)
      return []
    }

    const applySort = (items) => {
      items.sort((a, b) => {
        const nameA = a.name || ''
        const nameB = b.name || ''

        if (sortBy.value === 'Name (A-Z)') return nameA.localeCompare(nameB)
        if (sortBy.value === 'Name (Z-A)') return nameB.localeCompare(nameA)

        if (sortBy.value === 'Newest') {
          const timeA = a.uploaded_at ? new Date(a.uploaded_at).getTime() : 0;
          const timeB = b.uploaded_at ? new Date(b.uploaded_at).getTime() : 0;
          return timeB - timeA;
        }
        if (sortBy.value === 'Oldest') {
          const timeA = a.uploaded_at ? new Date(a.uploaded_at).getTime() : 0;
          const timeB = b.uploaded_at ? new Date(b.uploaded_at).getTime() : 0;
          return timeA - timeB;
        }
        if (sortBy.value === 'Size (Large)') return (b.file_size || 0) - (a.file_size || 0)
        if (sortBy.value === 'Size (Small)') return (a.file_size || 0) - (b.file_size || 0)
        return 0
      })

      items.forEach(i => i.children && applySort(i.children))
    }

    const filterSearch = (items) => {
      const query = (search.value || '').toLowerCase()

      return items
        .map(item => {
          if (item.children) {
            item.children = filterSearch(item.children)
          }
          return item
        })
        .filter(item => {
          if (!query) return true // Keep everything if no search

          const itemName = (item.name || '').toLowerCase()
          return itemName.includes(query) || (item.children && item.children.length > 0)
        })
    }

    try {
      applySort(clone)
      const result = filterSearch(clone)
      console.log('filteredTree re-computed successfully.')
      return result
    } catch (e) {
      console.error('Error during sort/filter computation:', e)
      return clone
    }
  })

  const updateBreadcrumb = (activeIds) => {
    if (!activeIds.length) return
    breadcrumbs.value = activeIds.map(id => ({ title: id }))
  }

  const downloadFile = (id) => {
    window.open(`/api/File/download/${id}`, '_blank')
  }

  const deleteFile = async (id, projectId) => {
    if (!confirm('Delete file?')) return
    await api.delete(`/File/${id}`)
    await loadProjects()
  }

  const previewFile = (file) => {
    previewFileName.value = file.name
    previewUrl.value = `/api/File/download/${file.file_id}`

    if (file.content_type?.includes('pdf'))
      previewType.value = 'pdf'
    else if (file.content_type?.includes('image'))
      previewType.value = 'image'
    else
      previewType.value = 'other'

    previewDialog.value = true
  }

  const formatSize = (bytes) => {
    if (!bytes) return '0 B'
    const sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(1024))
    return (bytes / Math.pow(1024, i)).toFixed(1) + ' ' + sizes[i]
  }

  const formatDate = (d) => {
    if (!d) return ''
    return new Date(d).toLocaleDateString()
  }

  const countFiles = (node) => {
    if (!node.children) return 0
    return node.children.reduce((acc, child) => {
      if (child.type === 'file') return acc + 1
      return acc + countFiles(child)
    }, 0)
  }

  onMounted(loadProjects)
</script>

<style scoped>
  .upload-drop-zone {
    border: 2px dashed #bbb;
    border-radius: 12px;
    cursor: pointer;
    transition: .3s;
  }

    .upload-drop-zone:hover {
      border-color: #1976d2;
      background: #f5faff;
    }
</style>
