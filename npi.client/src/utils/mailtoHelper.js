export function openMailClient({ to = '', cc = [], subject = '', body = '' }) {
  const encodedSubject = encodeURIComponent(subject)
  const encodedBody = encodeURIComponent(body).replace(/%0A/gi, '%0D%0A')

  const toList = (Array.isArray(to) ? to : [to])
    .filter(e => typeof e === 'string' && e.trim().length > 0)

  const ccList = cc.filter(e => typeof e === 'string' && e.trim().length > 0)
  const ccParam = ccList.length ? `&cc=${ccList.map(encodeURIComponent).join(',')}` : ''

  const uri = `mailto:${toList.map(encodeURIComponent).join(',')}?subject=${encodedSubject}${ccParam}&body=${encodedBody}`

  const a = document.createElement('a')
  a.href = uri
  a.rel = 'noopener'
  a.style.display = 'none'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
}

export function openProjectStatusEmail(project, newStatus, currentUser) {
  const timestamp = new Intl.DateTimeFormat('en-MY', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
  }).format(new Date())

  const changedByName = currentUser?.full_name ?? currentUser?.username ?? 'NPI Coordinator'

  const members = project.team_members ?? []
  const hasEmail = m => typeof m.email === 'string' && m.email.trim().length > 0

  const teamLeads = members.filter(m => m.role === 'Team Lead' && hasEmail(m))
  const regularMembers = members.filter(m => m.role === 'Member' && hasEmail(m))
  const viewers = members.filter(m => m.role === 'Viewer' && hasEmail(m))

  const toEmails = [...new Set([...teamLeads, ...regularMembers].map(m => m.email))]
  const ccEmails = [...new Set(viewers.map(m => m.email))].filter(e => !toEmails.includes(e))

  const subject = `[NPI] ${project.proj_no} — ${project.proj_name}: Status updated to "${newStatus}"`

  const body = [
    `Hi all,`,
    ``,
    `Sharing an update on the following NPI project:`,
    ``,
    `Project Name  : ${project.proj_name}`,
    `New Status    : ${newStatus}`,
    `Date/Time     : ${timestamp}`,
    ``,
    `Please review the current task list under your respective stages and flag`,
    `any blockers or pending items that may affect the timeline.`,
    ``,
    `Thank you,`,
    `${changedByName}`,
  ].join('\n')

  openMailClient({
    to: toEmails,
    cc: ccEmails,
    subject,
    body,
  })
}
