// ─────────────────────────────────────────────────────────────────────────────
// SYSTEM ROLES  (Roles table)
// ─────────────────────────────────────────────────────────────────────────────

export const SYSTEM_ROLES = Object.freeze({
  ADMIN: 'Admin',
  MANAGER: 'Manager',
  MEMBER: 'Member',
})

/** Convenience array for v-select / filter lists */
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

/** Department codes permitted to create/edit enquiries. */
export const ENQUIRY_ALLOWED_DEPT_CODES = Object.freeze([
  DEPT_CODES.SALES,
])

/** Fallback colour for a department with no color_hex set. */
export const DEFAULT_DEPT_COLOR = '#9E9E9E'

/** Fallback icon for a department with no icon mapping. */
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

/** Ordered array used by the role-hierarchy comparator in authStore */
export const PROJECT_ROLE_HIERARCHY = Object.freeze([
  PROJECT_ROLES.TEAM_LEAD,
  PROJECT_ROLES.MEMBER,
  PROJECT_ROLES.VIEWER,
])

/** For v-select dropdowns in assignment dialogs */
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

/** Ordered list for v-select / filter dropdowns */
export const TASK_STATUSES = Object.freeze([
  TASK_STATUS.NOT_STARTED,
  TASK_STATUS.IN_PROGRESS,
  TASK_STATUS.ON_HOLD,
  TASK_STATUS.COMPLETED,
  TASK_STATUS.CANCELLED,
])

/** Statuses that mean a task is closed and no longer actionable */
export const TASK_CLOSED_STATUSES = Object.freeze([
  TASK_STATUS.COMPLETED,
  TASK_STATUS.CANCELLED,
])

/** Map task status → Vuetify colour name */
export const TASK_STATUS_COLORS = Object.freeze({
  'Not Started': 'grey',
  'In Progress': 'blue',
  'On Hold': 'orange',
  'Completed': 'green',
  'Cancelled': 'red',
})

/** Map task status → MDI icon name */
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

/** Ordered list for v-select / filter dropdowns */
export const PROJECT_STATUSES = Object.freeze([
  PROJECT_STATUS.PLANNING,
  PROJECT_STATUS.NOT_STARTED,
  PROJECT_STATUS.IN_PROGRESS,
  PROJECT_STATUS.ON_HOLD,
  PROJECT_STATUS.COMPLETED,
  PROJECT_STATUS.CANCELLED,
])

/** Status transitions that trigger a team notification email */
export const PROJECT_NOTIFY_STATUSES = Object.freeze([
  PROJECT_STATUS.IN_PROGRESS,
  PROJECT_STATUS.ON_HOLD,
  PROJECT_STATUS.COMPLETED,
  PROJECT_STATUS.CANCELLED,
])

/** Map project status → Vuetify colour name */
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

/** Ordered list for v-select / filter dropdowns */
export const PRIORITY_OPTIONS = Object.freeze([
  PRIORITY.LOW,
  PRIORITY.MEDIUM,
  PRIORITY.HIGH,
  PRIORITY.CRITICAL,
])

export const DEFAULT_PRIORITY = PRIORITY.MEDIUM

/** Map priority → Vuetify colour name */
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
})

/** Map enquiry status → Vuetify colour name */
export const ENQUIRY_STATUS_COLORS = Object.freeze({
  [ENQUIRY_STATUS.DRAFT]: 'warning',
  [ENQUIRY_STATUS.SUBMITTED]: 'info',
})


// ─────────────────────────────────────────────────────────────────────────────
// SHARED UI DEFAULTS
// ─────────────────────────────────────────────────────────────────────────────

/** Sentinel value for "no filter applied" in v-select filter dropdowns */
export const FILTER_ALL = 'All'

/** Default working days applied to a newly added task row */
export const DEFAULT_WORKING_DAYS = 5

/** Fallback stage id when a task row has no stage_id */
export const DEFAULT_STAGE_ID = '1.0'

/** Fallback Vuetify colour when a lookup map misses */
export const DEFAULT_COLOR = 'grey'

/** Snackbar auto-dismiss (ms) */
export const SNACKBAR_TIMEOUT = 3000

/** Delay before redirecting after a success message (ms) */
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

/** Map file extension → MDI icon name */
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

/** Map file extension → Vuetify colour name */
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

/** Extensions that are previewable inline in the file explorer */
export const PREVIEWABLE_TYPES = Object.freeze({
  pdf: ['pdf'],
  image: ['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'svg'],
  text: ['txt', 'csv', 'log', 'md'],
})

/** Extensions that require an external application to open */
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

/** Map system role_name → Vuetify colour name */
export const SYSTEM_ROLE_COLORS = Object.freeze({
  Admin: 'error',
  Manager: 'primary',
  'Team Lead': 'success',
  Member: 'info',
})

/** Map system role_name → MDI icon name */
export const SYSTEM_ROLE_ICONS = Object.freeze({
  Admin: 'mdi-shield-crown',
  Manager: 'mdi-account-tie',
  'Team Lead': 'mdi-account-star',
  Member: 'mdi-account',
})


// ─────────────────────────────────────────────────────────────────────────────
// NOTIFICATION TYPES
// ─────────────────────────────────────────────────────────────────────────────

/** Map notification type string → Vuetify colour name */
export const NOTIFICATION_COLORS = Object.freeze({
  handover: 'blue',
  stage_complete: 'success',
  overdue: 'error',
  planning_stuck: 'warning',
  project_launch: 'primary',
  fai_complete: 'success',
  date_revised: 'warning',
  file_milestone: 'info',
  task_assigned: 'primary',
  approval_stalled: 'error',
  project_active: 'primary',
  project_on_hold: 'warning',
  project_complete: 'success',
  project_cancelled: 'error',
})

/** Map notification type string → MDI icon name */
export const NOTIFICATION_ICONS = Object.freeze({
  handover: 'mdi-hand-pointing-right',
  stage_complete: 'mdi-check-all',
  overdue: 'mdi-alert-circle',
  planning_stuck: 'mdi-clock-alert',
  project_launch: 'mdi-rocket-launch',
  fai_complete: 'mdi-flask-check',
  date_revised: 'mdi-calendar-edit',
  file_milestone: 'mdi-file-multiple',
  task_assigned: 'mdi-clipboard-account',
  approval_stalled: 'mdi-timer-alert',
  project_active: 'mdi-play-circle',
  project_on_hold: 'mdi-pause-circle',
  project_complete: 'mdi-check-circle',
  project_cancelled: 'mdi-close-circle',
})
