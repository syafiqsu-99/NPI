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
// DEPARTMENT NAMES  (Departments table)
// ─────────────────────────────────────────────────────────────────────────────

export const DEPT_NAMES = Object.freeze({
  SALES: 'Sales',
  TECHNICAL: 'Technical',
  PURCHASER: 'Purchaser',
  QA: 'QA',
  PRODUCTION: 'Production',
  MANAGEMENT: 'Management',
})

/** Departments that are allowed to create/edit enquiries */
export const ENQUIRY_ALLOWED_DEPTS = Object.freeze([
  DEPT_NAMES.SALES,
])

/** Departments that carry task editing rights for their own dept tasks */
export const TASK_DEPT_NAMES = Object.freeze([
  DEPT_NAMES.SALES,
  DEPT_NAMES.TECHNICAL,
  DEPT_NAMES.PURCHASER,
  DEPT_NAMES.QA,
  DEPT_NAMES.PRODUCTION,
])


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

export const TASK_STATUSES = Object.freeze([
  'Not Started',
  'In Progress',
  'On Hold',
  'Completed',
  'Cancelled',
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

export const PROJECT_STATUSES = Object.freeze([
  'Planning',
  'Not Started',
  'In Progress',
  'On Hold',
  'Completed',
  'Cancelled',
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

export const PRIORITY_OPTIONS = Object.freeze([
  'Low',
  'Medium',
  'High',
  'Critical',
])

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

/** Map enquiry status → Vuetify colour name */
export const ENQUIRY_STATUS_COLORS = Object.freeze({
  Draft: 'warning',
  Submitted: 'info',
  Approved: 'success',
  Rejected: 'error',
  Started: 'primary',
  'In Progress': 'blue',
  Completed: 'green',
  Pending: 'orange',
  'In Review': 'blue',
})


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
// FILE TYPES
// ─────────────────────────────────────────────────────────────────────────────

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
// DEPARTMENT UI METADATA
// ─────────────────────────────────────────────────────────────────────────────

/** Map dept_name → Vuetify colour name (used in DepartmentManagement chips) */
export const DEPT_COLORS = Object.freeze({
  Sales: 'blue',
  Technical: 'green',
  Purchaser: 'orange',
  Purchasing: 'orange',
  QA: 'purple',
  Production: 'red',
  Others: 'grey',
})

/** Map dept_name → MDI icon name (used in DepartmentManagement list) */
export const DEPT_ICONS = Object.freeze({
  Sales: 'mdi-chart-line',
  Technical: 'mdi-cog',
  Purchaser: 'mdi-cart',
  Purchasing: 'mdi-cart',
  QA: 'mdi-check-decagram',
  Production: 'mdi-factory',
  Others: 'mdi-domain',
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
