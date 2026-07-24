// ─────────────────────────────────────────────────────────────────────────────
// SYSTEM ROLES  (Roles table)
// ─────────────────────────────────────────────────────────────────────────────

export const SYSTEM_ROLES = Object.freeze({
  ADMIN: 'Admin',
  MANAGER: 'Manager',
  MEMBER: 'Member',
})

export const SYSTEM_ROLE_NAMES = Object.freeze([
  SYSTEM_ROLES.ADMIN,
  SYSTEM_ROLES.MANAGER,
  SYSTEM_ROLES.MEMBER,
])

// ─────────────────────────────────────────────────────────────────────────────
// DEPARTMENT CODES  (Departments.dept_code — stable identifier)
// ─────────────────────────────────────────────────────────────────────────────

export const DEPT_CODES = Object.freeze({
  SALES: 'SLS',
  TECHNICAL: 'TEC',
  PURCHASER: 'PUR',
  PRODUCTION: 'PRD',
  QA: 'QA',
  MANAGEMENT: 'MGT',
})

export const ENQUIRY_ALLOWED_DEPT_CODES = Object.freeze([
  DEPT_CODES.SALES,
])

export const DEFAULT_DEPT_COLOR = '#9E9E9E'

export const DEFAULT_DEPT_ICON = 'mdi-domain'

export const DEPT_ICON_HINTS = Object.freeze({
  [DEPT_CODES.SALES]: 'mdi-chart-line',
  [DEPT_CODES.TECHNICAL]: 'mdi-cog',
  [DEPT_CODES.PURCHASER]: 'mdi-cart',
  [DEPT_CODES.PRODUCTION]: 'mdi-factory',
  [DEPT_CODES.QA]: 'mdi-check-decagram',
  [DEPT_CODES.MANAGEMENT]: 'mdi-domain',
})


// ─────────────────────────────────────────────────────────────────────────────
// PROJECT ROLES  (ProjectRoles table — per-project assignment)
// ─────────────────────────────────────────────────────────────────────────────

export const PROJECT_ROLES = Object.freeze({
  TEAM_LEAD: 'Team Lead',
  MEMBER: 'Member',
  VIEWER: 'Viewer',
})

export const PROJECT_ROLE_HIERARCHY = Object.freeze([
  PROJECT_ROLES.TEAM_LEAD,
  PROJECT_ROLES.MEMBER,
  PROJECT_ROLES.VIEWER,
])

export const PROJECT_ROLE_OPTIONS = Object.freeze([
  PROJECT_ROLES.TEAM_LEAD,
  PROJECT_ROLES.MEMBER,
  PROJECT_ROLES.VIEWER,
])


// ─────────────────────────────────────────────────────────────────────────────
// TASK STATUSES
// ─────────────────────────────────────────────────────────────────────────────

export const TASK_STATUS = Object.freeze({
  NOT_STARTED: 'Not Started',
  IN_PROGRESS: 'In Progress',
  ON_HOLD: 'On Hold',
  COMPLETED: 'Completed',
  CANCELLED: 'Cancelled',
})

export const TASK_STATUSES = Object.freeze([
  TASK_STATUS.NOT_STARTED,
  TASK_STATUS.IN_PROGRESS,
  TASK_STATUS.ON_HOLD,
  TASK_STATUS.COMPLETED,
  TASK_STATUS.CANCELLED,
])

export const TASK_CLOSED_STATUSES = Object.freeze([
  TASK_STATUS.COMPLETED,
  TASK_STATUS.CANCELLED,
])

export const TASK_STATUS_COLORS = Object.freeze({
  'Not Started': 'grey',
  'In Progress': 'blue',
  'On Hold': 'orange',
  'Completed': 'green',
  'Cancelled': 'red',
})

export const TASK_STATUS_ICONS = Object.freeze({
  'Not Started': 'mdi-circle-outline',
  'In Progress': 'mdi-play-circle',
  'On Hold': 'mdi-pause-circle',
  'Completed': 'mdi-check-circle',
  'Cancelled': 'mdi-close-circle',
})


// ─────────────────────────────────────────────────────────────────────────────
// PROJECT STATUSES
// ─────────────────────────────────────────────────────────────────────────────

export const PROJECT_STATUS = Object.freeze({
  PLANNING: 'Planning',
  NOT_STARTED: 'Not Started',
  IN_PROGRESS: 'In Progress',
  ON_HOLD: 'On Hold',
  COMPLETED: 'Completed',
  CANCELLED: 'Cancelled',
})

export const PROJECT_STATUSES = Object.freeze([
  PROJECT_STATUS.PLANNING,
  PROJECT_STATUS.NOT_STARTED,
  PROJECT_STATUS.IN_PROGRESS,
  PROJECT_STATUS.ON_HOLD,
  PROJECT_STATUS.COMPLETED,
  PROJECT_STATUS.CANCELLED,
])

export const PROJECT_NOTIFY_STATUSES = Object.freeze([
  PROJECT_STATUS.IN_PROGRESS,
  PROJECT_STATUS.ON_HOLD,
  PROJECT_STATUS.COMPLETED,
  PROJECT_STATUS.CANCELLED,
])

export const PROJECT_STATUS_COLORS = Object.freeze({
  'Planning': 'info',
  'Not Started': 'grey',
  'In Progress': 'primary',
  'On Hold': 'warning',
  'Completed': 'success',
  'Cancelled': 'error',
})


// ─────────────────────────────────────────────────────────────────────────────
// PRIORITY
// ─────────────────────────────────────────────────────────────────────────────

export const PRIORITY = Object.freeze({
  LOW: 'Low',
  MEDIUM: 'Medium',
  HIGH: 'High',
  CRITICAL: 'Critical',
})

export const PRIORITY_OPTIONS = Object.freeze([
  PRIORITY.LOW,
  PRIORITY.MEDIUM,
  PRIORITY.HIGH,
  PRIORITY.CRITICAL,
])

export const DEFAULT_PRIORITY = PRIORITY.MEDIUM

export const PRIORITY_COLORS = Object.freeze({
  Low: 'grey',
  Medium: 'blue',
  High: 'orange',
  Critical: 'red',
})


// ─────────────────────────────────────────────────────────────────────────────
// ENQUIRY STATUSES
// ─────────────────────────────────────────────────────────────────────────────

export const ENQUIRY_STATUS = Object.freeze({
  DRAFT: 'Draft',
  SUBMITTED: 'Submitted',
  NEEDS_REWORK: 'Needs Rework',
  NOT_FEASIBLE: 'Not Feasible',
})

export const ENQUIRY_STATUS_COLORS = Object.freeze({
  [ENQUIRY_STATUS.DRAFT]: 'warning',
  [ENQUIRY_STATUS.SUBMITTED]: 'success',
  [ENQUIRY_STATUS.NEEDS_REWORK]: 'orange',
  [ENQUIRY_STATUS.NOT_FEASIBLE]: 'error',
})


// ─────────────────────────────────────────────────────────────────────────────
// SHARED UI DEFAULTS
// ─────────────────────────────────────────────────────────────────────────────

export const FILTER_ALL = 'All'
export const DEFAULT_WORKING_DAYS = 5
export const DEFAULT_STAGE_ID = '1.0'
export const DEFAULT_COLOR = 'grey'
export const SNACKBAR_TIMEOUT = 3000
export const REDIRECT_DELAY_MS = 1500


// ─────────────────────────────────────────────────────────────────────────────
// NPI STAGES — colour maps
// ─────────────────────────────────────────────────────────────────────────────

export const STAGE_COLORS = Object.freeze({
  '0.0': 'blue-grey',
  '1.0': 'primary',
  '2.0': 'purple',
  '3.0': 'teal',
  '4.0': 'indigo',
  '5.0': 'deep-orange',
})

export const STAGE_COLORS_HEX = Object.freeze({
  '0.0': '#607D8B',
  '1.0': '#1976D2',
  '2.0': '#7B1FA2',
  '3.0': '#00796B',
  '4.0': '#303F9F',
  '5.0': '#E64A19',
})

export const STAGE_SHORT_NAMES = Object.freeze({
  '0.0': 'Enquiry',
  '1.0': 'Proj Start',
  '2.0': 'Pilot Mould',
  '3.0': 'Machine',
  '4.0': 'Prod Mould',
  '5.0': 'Trial JJ',
})

// ─────────────────────────────────────────────────────────────────────────────
// OPTIONAL STAGE → Projects table flag column
// ─────────────────────────────────────────────────────────────────────────────

export const OPTIONAL_STAGE_FLAGS = Object.freeze({
  '2.0': 'pilot_mould_required',
  '3.0': 'machine_purchase_required',
})

// ─────────────────────────────────────────────────────────────────────────────
// FILE TYPES
// ─────────────────────────────────────────────────────────────────────────────

export const PREVIEWABLE_EXTENSIONS = Object.freeze(['pdf', 'png', 'jpg', 'jpeg', 'gif', 'webp'])

export const FILE_ICONS = Object.freeze({
  pdf: 'mdi-file-pdf-box',
  doc: 'mdi-file-word',
  docx: 'mdi-file-word',
  xls: 'mdi-file-excel',
  xlsx: 'mdi-file-excel',
  png: 'mdi-file-image',
  jpg: 'mdi-file-image',
  jpeg: 'mdi-file-image',
})

export const FILE_ICON_COLORS = Object.freeze({
  pdf: 'red',
  doc: 'blue',
  docx: 'blue',
  xls: 'green',
  xlsx: 'green',
  png: 'purple',
  jpg: 'purple',
  jpeg: 'purple',
})

export const PREVIEWABLE_TYPES = Object.freeze({
  pdf: ['pdf'],
  image: ['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'svg'],
  text: ['txt', 'csv', 'log', 'md'],
})

export const EXTERNAL_APP_HINTS = Object.freeze({
  doc: { icon: 'mdi-microsoft-word', color: '#2B579A', app: 'Microsoft Word' },
  docx: { icon: 'mdi-microsoft-word', color: '#2B579A', app: 'Microsoft Word' },
  xls: { icon: 'mdi-microsoft-excel', color: '#217346', app: 'Microsoft Excel' },
  xlsx: { icon: 'mdi-microsoft-excel', color: '#217346', app: 'Microsoft Excel' },
  ppt: { icon: 'mdi-microsoft-powerpoint', color: '#D24726', app: 'PowerPoint' },
  pptx: { icon: 'mdi-microsoft-powerpoint', color: '#D24726', app: 'PowerPoint' },
  dwg: { icon: 'mdi-drawing', color: '#E34C26', app: 'AutoCAD' },
  step: { icon: 'mdi-cube-outline', color: '#607D8B', app: 'CAD software' },
  stp: { icon: 'mdi-cube-outline', color: '#607D8B', app: 'CAD software' },
})

// ─────────────────────────────────────────────────────────────────────────────
// SYSTEM ROLE UI METADATA
// ─────────────────────────────────────────────────────────────────────────────

export const SYSTEM_ROLE_COLORS = Object.freeze({
  Admin: 'error',
  Manager: 'primary',
  'Team Lead': 'success',
  Member: 'info',
})

export const SYSTEM_ROLE_ICONS = Object.freeze({
  Admin: 'mdi-shield-crown',
  Manager: 'mdi-account-tie',
  'Team Lead': 'mdi-account-star',
  Member: 'mdi-account',
})


// ─────────────────────────────────────────────────────────────────────────────
// NOTIFICATION TYPES
// ─────────────────────────────────────────────────────────────────────────────

export const NOTIFICATION_TYPES = Object.freeze({
  // Task-scoped
  HANDOVER: 'handover',
  TASK_ASSIGNED: 'task_assigned',
  TASK_COMMENT: 'task_comment',
  DATE_REVISED: 'date_revised',
  OVERDUE: 'overdue',
  DUE_SOON: 'due_soon',
  APPROVAL_STALLED: 'approval_stalled',

  // Project-scoped
  STAGE_COMPLETE: 'stage_complete',
  PROJECT_LAUNCH: 'project_launch',
  PLANNING_STUCK: 'planning_stuck',
  PROJECT_PLANNING: 'project_planning',
  PROJECT_ACTIVE: 'project_active',
  PROJECT_ON_HOLD: 'project_on_hold',
  PROJECT_COMPLETE: 'project_complete',
  PROJECT_CANCELLED: 'project_cancelled',
  PROJECT_STATUS_CHANGED: 'project_status_changed',

  // Enquiry-scoped
  ENQUIRY_REVIEW: 'enquiry_review',
  ENQUIRY_SUBMITTED: 'enquiry_submitted',
})

export const NOTIFICATION_COLORS = Object.freeze({
  handover: 'blue',
  task_assigned: 'primary',
  task_comment: 'indigo',
  date_revised: 'warning',
  overdue: 'error',
  due_soon: 'warning',
  approval_stalled: 'error',

  stage_complete: 'success',
  project_launch: 'primary',
  planning_stuck: 'warning',
  project_planning: 'info',
  project_active: 'primary',
  project_on_hold: 'warning',
  project_complete: 'success',
  project_cancelled: 'error',
  project_status_changed: 'info',

  enquiry_review: 'orange',
  enquiry_submitted: 'info',
})

export const NOTIFICATION_ICONS = Object.freeze({
  handover: 'mdi-hand-pointing-right',
  task_assigned: 'mdi-clipboard-account',
  task_comment: 'mdi-comment-text-outline',
  date_revised: 'mdi-calendar-edit',
  overdue: 'mdi-alert-circle',
  due_soon: 'mdi-clock-alert-outline',
  approval_stalled: 'mdi-timer-alert',

  stage_complete: 'mdi-check-all',
  project_launch: 'mdi-rocket-launch',
  planning_stuck: 'mdi-clock-alert',
  project_planning: 'mdi-clipboard-text-clock',
  project_active: 'mdi-play-circle',
  project_on_hold: 'mdi-pause-circle',
  project_complete: 'mdi-check-circle',
  project_cancelled: 'mdi-close-circle',
  project_status_changed: 'mdi-information',

  enquiry_review: 'mdi-file-document-edit',
  enquiry_submitted: 'mdi-file-send',
})
