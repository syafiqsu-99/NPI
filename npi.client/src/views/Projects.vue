<template>
  <v-container fluid fill-height class="d-flex flex-column">
    <v-row no-gutters align="center" justify="center">
      <v-col cols="12" class="text-center">
        <h2 class="font-weight-bold">
          <v-icon class="mr-2">mdi-folder-multiple</v-icon>
          NPI Projects
        </h2>
      </v-col>
    </v-row>

    <!-- Filter/Status Tabs -->
    <v-row no-gutters class="mb-4">
      <v-col cols="12">
        <v-card elevation="2">
          <v-tabs v-model="statusFilter" bg-color="primary">
            <v-tab value="all">
              <v-icon start>mdi-view-grid</v-icon>
              All Projects
            </v-tab>
            <v-tab value="Planning">
              <v-icon start>mdi-clipboard-text-outline</v-icon>
              Planning
            </v-tab>
            <v-tab value="In Progress">
              <v-icon start>mdi-progress-clock</v-icon>
              In Progress
            </v-tab>
            <v-tab value="On Hold">
              <v-icon start>mdi-pause-circle</v-icon>
              On Hold
            </v-tab>
            <v-tab value="Completed">
              <v-icon start>mdi-check-circle</v-icon>
              Completed
            </v-tab>
          </v-tabs>

          <v-window v-model="statusFilter">
            <v-window-item :value="statusFilter">
              <v-card elevation="2">
                <v-card-title class="d-flex align-center justify-space-between">
                  <span>Projects List</span>
                  <v-text-field v-model="search"
                                prepend-inner-icon="mdi-magnify"
                                label="Search projects..."
                                single-line
                                hide-details
                                density="compact"
                                variant="outlined"
                                class="ma-2"
                                style="max-width: 300px;" />
                </v-card-title>

                <v-card-text>
                  <v-progress-linear v-if="projectStore.loading"
                                     indeterminate
                                     color="primary" />

                  <v-data-table v-else
                                :items="filteredProjects"
                                :headers="headers"
                                :search="search"
                                item-key="proj_id"
                                class="elevation-1">
                    <template #item.proj_no="{ item }">
                      <v-chip color="primary" size="small" variant="outlined">
                        {{ item.proj_no }}
                      </v-chip>
                    </template>

                    <template #item.priority="{ item }">
                      <v-chip :color="getPriorityColor(item.priority)"
                              size="small"
                              variant="tonal">
                        {{ item.priority }}
                      </v-chip>
                    </template>

                    <template #item.status="{ item }">
                      <v-chip :color="getStatusColor(item.status)"
                              size="small"
                              variant="tonal">
                        {{ item.status }}
                      </v-chip>
                    </template>

                    <template #item.project_start_date="{ item }">
                      {{ formatDate(item.project_start_date) }}
                    </template>

                    <template #item.target_completion_date="{ item }">
                      {{ formatDate(item.target_completion_date) }}
                    </template>

                    <template #item.actions="{ item }">
                      <!-- Setup/Manage Button (for Planning & In Progress) -->
                      <v-btn v-if="canManageProject(item)"
                             icon
                             variant="text"
                             color="primary"
                             @click="manageProject(item.proj_id)"
                             title="Manage Project">
                        <v-icon>mdi-cog</v-icon>
                      </v-btn>

                      <!-- View Gantt Chart -->
                      <v-btn v-if="item.status === 'In Progress' || item.status === 'Completed'"
                             icon
                             variant="text"
                             color="success"
                             @click="viewGantt(item.proj_id)"
                             title="View Gantt Chart">
                        <v-icon>mdi-chart-gantt</v-icon>
                      </v-btn>

                      <!-- View Details -->
                      <v-btn icon
                             variant="text"
                             @click="viewProject(item.proj_id)"
                             title="View Details">
                        <v-icon>mdi-eye</v-icon>
                      </v-btn>
                    </template>

                    <template #no-data>
                      <v-alert type="info" variant="tonal" class="ma-4">
                        No projects found. Start a project from an approved enquiry.
                      </v-alert>
                    </template>
                  </v-data-table>
                </v-card-text>
              </v-card>
            </v-window-item>
          </v-window>
        </v-card>
      </v-col>
    </v-row>

    <!-- Project Detail Dialog -->
    <v-dialog v-model="showDetailDialog" max-width="800">
      <v-card v-if="selectedProject">
        <v-card-title class="bg-primary">
          <v-icon class="mr-2">mdi-information</v-icon>
          {{ selectedProject.proj_no }} - {{ selectedProject.proj_name }}
        </v-card-title>

        <v-card-text class="pa-6">
          <v-row>
            <v-col cols="12" md="6">
              <strong>Customer:</strong> {{ selectedProject.customer_name || 'N/A' }}
            </v-col>
            <v-col cols="12" md="6">
              <strong>Status:</strong>
              <v-chip :color="getStatusColor(selectedProject.status)"
                      size="small"
                      variant="tonal"
                      class="ml-2">
                {{ selectedProject.status }}
              </v-chip>
            </v-col>
            <v-col cols="12" md="6">
              <strong>Priority:</strong>
              <v-chip :color="getPriorityColor(selectedProject.priority)"
                      size="small"
                      variant="tonal"
                      class="ml-2">
                {{ selectedProject.priority }}
              </v-chip>
            </v-col>
            <v-col cols="12" md="6">
              <strong>Start Date:</strong> {{ formatDate(selectedProject.project_start_date) }}
            </v-col>
            <v-col cols="12" md="6">
              <strong>Target Completion:</strong> {{ formatDate(selectedProject.target_completion_date) }}
            </v-col>
            <v-col cols="12">
              <strong>Description:</strong>
              <p class="mt-2">{{ selectedProject.description || 'No description available' }}</p>
            </v-col>
          </v-row>
        </v-card-text>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="showDetailDialog = false">Close</v-btn>
          <v-btn v-if="canManageProject(selectedProject)"
                 color="primary"
                 @click="manageProject(selectedProject.proj_id)">
            Manage Project
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Snackbar -->
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
  import { useProjectStore } from '@/stores/project'
  import { useAuthStore } from '@/stores/auth'

  const router = useRouter()
  const projectStore = useProjectStore()
  const authStore = useAuthStore()

  const statusFilter = ref('all')
  const search = ref('')
  const showDetailDialog = ref(false)
  const selectedProject = ref(null)
  const snackbar = ref(false)
  const snackbarMessage = ref('')
  const snackbarColor = ref('success')

  const headers = [
    { title: 'Project No', key: 'proj_no', sortable: true },
    { title: 'Project Name', key: 'proj_name', sortable: true },
    { title: 'Customer', key: 'customer_name', sortable: true },
    { title: 'Priority', key: 'priority', sortable: true },
    { title: 'Status', key: 'status', sortable: true },
    { title: 'Start Date', key: 'project_start_date', sortable: true },
    { title: 'Target Date', key: 'target_completion_date', sortable: true },
    { title: 'Actions', key: 'actions', sortable: false, align: 'center', width: '150' }
  ]

  const userRole = computed(() => authStore.user?.role)

  const filteredProjects = computed(() => {
    let projects = projectStore.projects || []

    if (statusFilter.value !== 'all') {
      projects = projects.filter(p => p.status === statusFilter.value)
    }

    return projects
  })

  function canManageProject(project) {
    // NPI Team and Admin can manage projects in Planning or In Progress status
    const isNpiOrAdmin = userRole.value === 'NPI Team' || userRole.value === 'Admin'
    const canManage = project.status === 'Planning' || project.status === 'In Progress'
    return isNpiOrAdmin && canManage
  }

  function getStatusColor(status) {
    const colors = {
      'Planning': 'info',
      'In Progress': 'primary',
      'On Hold': 'warning',
      'Completed': 'success',
      'Cancelled': 'error'
    }
    return colors[status] || 'grey'
  }

  function getPriorityColor(priority) {
    const colors = {
      'Low': 'success',
      'Medium': 'info',
      'High': 'warning',
      'Critical': 'error'
    }
    return colors[priority] || 'grey'
  }

  function formatDate(date) {
    if (!date) return 'N/A'

    // Handle DateOnly format from C# (YYYY-MM-DD)
    if (typeof date === 'string') {
      const parts = date.split('-')
      if (parts.length === 3) {
        const dateObj = new Date(parts[0], parts[1] - 1, parts[2])
        return dateObj.toLocaleDateString('en-GB', {
          day: '2-digit',
          month: 'short',
          year: 'numeric'
        })
      }
    }

    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    })
  }

  function manageProject(projId) {
    router.push(`/projects/${projId}/setup`)
  }

  function viewGantt(projId) {
    router.push(`/projects/${projId}/gantt`)
  }

  function viewProject(projId) {
    const project = projectStore.projects.find(p => p.proj_id === projId)
    if (project) {
      selectedProject.value = project
      showDetailDialog.value = true
    }
  }

  onMounted(async () => {
    const result = await projectStore.fetchProjects()

    if (!result.success) {
      snackbarMessage.value = result.message || 'Failed to load projects'
      snackbarColor.value = 'error'
      snackbar.value = true
    }
  })
</script>

<style scoped>
  .v-data-table {
    background-color: transparent;
  }
</style>
