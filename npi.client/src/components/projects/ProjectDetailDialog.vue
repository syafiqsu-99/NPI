<template>
  <v-dialog v-model="isOpen" max-width="900" persistent>
    <v-card class="dialog-card d-flex flex-column" max-height="90vh">

      <v-card-title class="bg-primary text-white d-flex align-center justify-space-between flex-shrink-0">
        <span class="d-flex align-center gap-2">
          <v-icon class="mr-2">mdi-information</v-icon>
          {{ project?.proj_no }} — {{ project?.proj_name }}
        </span>
        <v-btn icon="mdi-close" variant="text" color="white" @click="isOpen = false" />
      </v-card-title>

      <v-tabs v-model="activeTab" bg-color="surface" class="px-4 flex-shrink-0 border-b">
        <v-tab value="overview" class="font-weight-bold">
          <v-icon start>mdi-view-dashboard</v-icon>Overview
        </v-tab>
        <v-tab value="revisions" class="font-weight-bold">
          <v-icon start>mdi-history</v-icon>Revision History
        </v-tab>
      </v-tabs>

      <v-card-text class="pa-0 flex-grow-1 d-flex flex-column" style="min-height: 0; overflow: hidden;">
        <v-window v-model="activeTab" class="custom-window flex-grow-1 d-flex flex-column" style="min-height: 0;">

          <v-window-item value="overview">
            <div class="pa-3">
              <v-card variant="outlined" class="mb-3 pa-2">
                <div class="text-caption font-weight-bold mb-1">NPI STAGE PIPELINE</div>
                <div class="d-flex align-center flex-wrap ga-1">
                  <template v-for="(stage, idx) in projectStages" :key="stage.id">
                    <div class="pipeline-node"
                         :style="{ borderColor: getStageBorderColor(stage), background: getStageBgColor(stage) }">
                      <div class="text-caption font-weight-bold">{{ stage.id }}</div>
                      <div style="font-size:10px">{{ stage.shortName }}</div>
                    </div>
                    <v-icon v-if="idx < projectStages.length - 1" size="x-small">mdi-arrow-right</v-icon>
                  </template>
                </div>
              </v-card>

              <v-row dense class="ma-0">
                <v-col cols="12" md="6">
                  <div class="text-caption font-weight-bold mb-2">PROJECT DETAILS</div>
                  <div class="text-caption d-flex flex-column ga-1">
                    <div><strong>Customer:</strong> {{ project?.customer_name || 'N/A' }}</div>
                    <div>
                      <strong>Status:</strong>
                      <v-chip size="x-small" :color="getStatusColor(project?.status)" variant="tonal">
                        {{ project?.status }}
                      </v-chip>
                    </div>
                    <div>
                      <strong>Priority:</strong>
                      <v-chip size="x-small" :color="getPriorityColor(project?.priority)" variant="tonal">
                        {{ project?.priority }}
                      </v-chip>
                    </div>
                    <div><strong>Start:</strong> {{ formatDate(project?.project_start_date) }}</div>
                    <div><strong>Target:</strong> {{ formatDate(project?.target_completion_date) }}</div>
                    <div>
                      <strong>Stages:</strong>
                      <div class="d-flex flex-wrap ga-1 mt-1">
                        <v-chip v-if="project?.pilot_mould_required" size="x-small">Pilot</v-chip>
                        <v-chip v-if="project?.machine_purchase_required" size="x-small">Machine</v-chip>
                        <span v-if="!project?.pilot_mould_required && !project?.machine_purchase_required">None</span>
                      </div>
                    </div>
                    <div>
                      <strong>Description:</strong>
                      <div class="text-grey">{{ project?.description || 'No description' }}</div>
                    </div>
                  </div>
                </v-col>

                <v-col cols="12" md="6">
                  <div class="text-caption font-weight-bold mb-2">TEAM MEMBERS</div>
                  <v-list density="compact" class="pa-0 bg-transparent">
                    <v-list-item v-for="member in teamMembers" :key="member.user_id" class="pa-1">
                      <template #prepend>
                        <v-avatar size="28" color="primary">
                          <span class="text-white text-caption">
                            {{ getInitials(member.full_name || member.user_name) }}
                          </span>
                        </v-avatar>
                      </template>
                      <div class="text-caption">
                        <div class="font-weight-medium">{{ member.full_name || member.user_name }}</div>
                        <div class="d-flex flex-wrap ga-1">
                          <v-chip size="x-small">{{ member.dept_name || 'N/A' }}</v-chip>
                          <v-chip size="x-small">{{ member.role || 'Member' }}</v-chip>
                        </div>
                      </div>
                    </v-list-item>
                  </v-list>
                </v-col>
              </v-row>
            </div>
          </v-window-item>

          <v-window-item value="revisions">
            <div class="pa-6 fill-height overflow-y-auto">
              <v-timeline v-if="revisions && revisions.length > 0" align="start" side="end">
                <v-timeline-item v-for="(rev, idx) in revisions" :key="rev.revision_id"
                                 :dot-color="getRevisionColor(idx)" size="small">
                  <div class="mb-2">
                    <strong class="text-body-2">Revision #{{ rev.revision_number }}</strong>
                    <v-chip size="x-small" variant="text" color="grey" class="ml-2">
                      {{ formatDateTime(rev.revision_date) }}
                    </v-chip>
                  </div>
                  <div class="text-caption mb-2">
                    <div class="mb-1"><strong>Updated by:</strong> {{ rev.revised_by_name || 'System' }}</div>
                    <div class="mb-1">
                      <strong>Reason:</strong>
                      <span class="text-grey">{{ rev.revision_notes || 'No reason provided' }}</span>
                    </div>
                  </div>
                  <v-card v-if="rev.previous_target_date || rev.new_target_date"
                          variant="outlined" size="small" class="mb-3 pa-2">
                    <div class="text-caption">
                      <div v-if="rev.previous_target_date" class="mb-1">
                        <strong>Previous Target:</strong> {{ formatDate(rev.previous_target_date) }}
                      </div>
                      <div v-if="rev.new_target_date">
                        <strong>New Target:</strong> {{ formatDate(rev.new_target_date) }}
                      </div>
                    </div>
                  </v-card>
                  <v-expand-transition>
                    <div v-if="showTaskRevisions[rev.revision_id]"
                         class="bg-grey-lighten-4 pa-2 rounded mt-2">
                      <div class="text-caption font-weight-bold mb-2">Task Changes:</div>
                      <v-list v-if="rev.task_revisions?.length" density="compact" class="pa-0">
                        <v-list-item v-for="tr in rev.task_revisions" :key="tr.task_id"
                                     class="text-caption pa-1">
                          <template #prepend>
                            <v-icon size="x-small" color="warning">mdi-pencil</v-icon>
                          </template>
                          <v-list-item-title class="text-caption">
                            {{ tr.task_title || `Task ${tr.task_id}` }}
                          </v-list-item-title>
                          <v-list-item-subtitle class="text-caption mt-1">
                            {{ formatDate(tr.old_start_date) }} → {{ formatDate(tr.old_end_date) }}
                            <v-icon size="x-small" class="mx-1">mdi-arrow-right</v-icon>
                            {{ formatDate(tr.new_start_date) }} → {{ formatDate(tr.new_end_date) }}
                          </v-list-item-subtitle>
                        </v-list-item>
                      </v-list>
                      <div v-else class="text-caption text-grey">No task changes in this revision</div>
                    </div>
                  </v-expand-transition>
                  <v-btn v-if="rev.task_revisions?.length"
                         size="x-small" variant="text" color="primary" class="mt-1"
                         @click="toggleTaskRevisions(rev.revision_id)">
                    {{ showTaskRevisions[rev.revision_id] ? 'Hide' : 'Show' }} Task Changes
                  </v-btn>
                </v-timeline-item>
              </v-timeline>
              <v-alert v-else type="info" variant="tonal" class="ma-4">
                No revision history available
              </v-alert>
            </div>
          </v-window-item>
        </v-window>
      </v-card-text>

      <v-card-actions class="pa-4 d-flex justify-end ga-2 flex-shrink-0 border-t">
        <v-btn variant="text" @click="isOpen = false">Close</v-btn>
        <v-btn v-if="canManage" color="primary" variant="elevated"
               @click="$emit('manage', project.proj_id)">
          Manage Project
        </v-btn>
        <v-btn v-if="project?.status === 'In Progress' || project?.status === 'Completed'"
               color="success" variant="elevated"
               @click="$emit('gantt', project.proj_id)">
          View Gantt Chart
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
  import { ref, computed, watch } from 'vue'
  import { useAuthStore } from '@/stores/auth'
  import { useSettingsStore } from '@/stores/setting'
  import { PROJECT_STATUS_COLORS, PRIORITY_COLORS, STAGE_COLORS_HEX, STAGE_SHORT_NAMES, OPTIONAL_STAGE_FLAGS } from '@/utils/constants'
  import { formatDate, formatDateTime, getInitials } from '@/utils/formatters'

  const props = defineProps({
    modelValue: Boolean,
    project:     Object,
    teamMembers: Array,
    revisions:   Array,
  })
  const settingsStore = useSettingsStore()
  const emit = defineEmits(['update:modelValue', 'manage', 'gantt'])

  const isOpen = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val)
  })

  const activeTab = ref('overview')
  const showTaskRevisions = ref({})
  const authStore = useAuthStore()

  const canManage = computed(() => {
    if (!props.project) return false
    return authStore.canManageProject(props.project)
  })

  watch(() => props.revisions, (vals) => {
    if (!vals) return
    vals.forEach(rev => {
      if (showTaskRevisions.value[rev.revision_id] === undefined) {
        showTaskRevisions.value[rev.revision_id] = false
      }
    })
  }, { immediate: true })

  function toggleTaskRevisions(id) {
    showTaskRevisions.value[id] = !showTaskRevisions.value[id]
  }

  const projectStages = computed(() => {
    if (!props.project) return []
    return settingsStore.stages
      .map(s => s.stage_id)
      .filter(id => {
        if (settingsStore.stagesById[id]?.is_required) return true
        const flagKey = OPTIONAL_STAGE_FLAGS[id]
        return flagKey ? !!props.project[flagKey] : false
      })
      .map(id => {
        const progress = props.project.stage_progress?.[id]
        let status = 'pending'
        if (progress?.completed) status = 'completed'
        else if (progress?.in_progress) status = 'active'
        return {
          id,
          name: settingsStore.getStageName(id),
          shortName: STAGE_SHORT_NAMES[id] || id,
          status,
          color: STAGE_COLORS_HEX[id] || '#9E9E9E',
        }
      })
  })

  function getStageBorderColor(stage) {
    if (stage.status === 'completed') return '#4CAF50'
    if (stage.status === 'active') return stage.color
    return '#BDBDBD'
  }

  function getStageBgColor(stage) {
    if (stage.status === 'completed') return 'rgba(76,175,80,0.1)'
    if (stage.status === 'active') return `${stage.color}18`
    return 'rgba(0,0,0,0.03)'
  }

  function getStatusColor(status) { return PROJECT_STATUS_COLORS[status] || 'grey' }
  function getPriorityColor(p) { return PRIORITY_COLORS[p] || 'grey' }
  function getRevisionColor(index) {
    return ['primary', 'success', 'warning', 'error', 'info'][index % 5]
  }
</script>

<style scoped>
  .custom-window :deep(.v-window__container) {
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    min-height: 0;
  }

  .custom-window :deep(.v-window-item) {
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    min-height: 0;
  }

  .pipeline-node {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 6px 10px;
    border-radius: 8px;
    border: 2px solid;
    min-width: 64px;
    text-align: center;
    cursor: default;
    transition: all 0.2s;
  }
</style>
