import { FILE_ICONS, FILE_ICON_COLORS, PREVIEWABLE_TYPES } from '@/utils/constants.js'


// ─────────────────────────────────────────────────────────────────────────────
// DATE & TIME
// ─────────────────────────────────────────────────────────────────────────────

export function formatDate(date) {
  if (!date) return 'N/A'

  if (typeof date === 'string') {
    const parts = date.split('T')[0].split('-')
    if (parts.length === 3) {
      const [y, m, d] = parts.map(Number)
      return new Date(y, m - 1, d).toLocaleDateString('en-GB', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      })
    }
  }

  return new Date(date).toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export function formatDateShort(date) {
  if (!date) return 'N/A'
  return new Date(date).toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
  })
}

/** Returns YYYY-MM-DD for binding to <input type="date">. UTC-based to avoid
 *  the off-by-one that local-time parsing causes for dates near midnight. */
export function formatDateForInput(date) {
  if (!date) return null
  const d = new Date(date)
  if (Number.isNaN(d.getTime())) return null
  return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, '0')}-${String(d.getUTCDate()).padStart(2, '0')}`
}

export function formatDateTime(date) {
  if (!date) return 'N/A'
  return new Date(date).toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatTimeAgo(dateStr) {
  if (!dateStr) return ''
  const diffSeconds = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000)
  if (diffSeconds < 60) return 'just now'
  if (diffSeconds < 3600) return `${Math.floor(diffSeconds / 60)}m ago`
  if (diffSeconds < 86400) return `${Math.floor(diffSeconds / 3600)}h ago`
  return `${Math.floor(diffSeconds / 86400)}d ago`
}


// ─────────────────────────────────────────────────────────────────────────────
// FILE SIZE
// ─────────────────────────────────────────────────────────────────────────────

export function formatSize(bytes) {
  if (!bytes) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(1024))
  return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${units[i]}`
}


// ─────────────────────────────────────────────────────────────────────────────
// FILE ICONS
// ─────────────────────────────────────────────────────────────────────────────

export function getFileIcon(filename) {
  const ext = filename?.split('.').pop()?.toLowerCase() ?? ''
  return FILE_ICONS[ext] ?? 'mdi-file-document'
}

export function getFileIconColor(filename) {
  const ext = filename?.split('.').pop()?.toLowerCase() ?? ''
  return FILE_ICON_COLORS[ext] ?? 'grey'
}

export function getPreviewType(filename) {
  const ext = filename?.split('.').pop()?.toLowerCase() ?? ''
  for (const [type, exts] of Object.entries(PREVIEWABLE_TYPES)) {
    if (exts.includes(ext)) return type
  }
  return null
}


// ─────────────────────────────────────────────────────────────────────────────
// USER / TEXT HELPERS
// ─────────────────────────────────────────────────────────────────────────────

export function getInitials(name) {
  if (!name) return '?'
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .substring(0, 2)
}

export function sanitizeFolderName(name) {
  if (!name) return ''
  return name
    .replace(/[ /]/g, '_')
    .replace(/[<>:"\\|?*\x00-\x1F]/g, '_')
}

export function getAvatarColor(name) {
  const colors = [
    'primary', 'secondary', 'success', 'info',
    'warning', 'purple', 'teal', 'indigo',
  ]
  if (!name) return 'grey'
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return colors[Math.abs(hash) % colors.length]
}
