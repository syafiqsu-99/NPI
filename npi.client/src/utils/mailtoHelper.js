/**
 * Opens the OS-registered default mail client with a pre-filled draft.
 * Works with Outlook, Thunderbird, Apple Mail, or any client registered
 * as the mailto: handler — no hardcoding.
 *
 * @param {Object} opts
 * @param {string}   opts.to       - Primary recipient email
 * @param {string[]} opts.cc       - Array of CC addresses (filtered automatically)
 * @param {string}   opts.subject  - Subject line
 * @param {string}   opts.body     - Body text — use \n for line breaks
 */
export function openMailClient({ to = '', cc = [], subject = '', body = '' }) {
  // RFC 2368: mailto body must use CRLF for line breaks.
  // encodeURIComponent converts \n → %0A; we then normalise to %0D%0A.
  const encodedSubject = encodeURIComponent(subject)
  const encodedBody = encodeURIComponent(body).replace(/%0A/gi, '%0D%0A')

  const ccList = cc.filter(e => typeof e === 'string' && e.trim().length > 0)
  const ccParam = ccList.length ? `&cc=${ccList.map(encodeURIComponent).join(',')}` : ''

  const uri = `mailto:${encodeURIComponent(to)}?subject=${encodedSubject}${ccParam}&body=${encodedBody}`

  // A programmatic <a> click is treated as a trusted user gesture by all
  // major browsers, which is required to trigger a registered protocol handler.
  // window.location.href is unreliable in Chromium for mailto: since ~2022.
  const a = document.createElement('a')
  a.href = uri
  a.rel = 'noopener'
  a.style.display = 'none'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
}

/**
 * Builds a project status-change email and immediately opens the default
 * mail client — uses the logged-in user's email from authStore as the
 * sender context, and team member emails from Users.email for CC.
 *
 * @param {Object} project    - Project object with proj_no, proj_name, team_members[]
 * @param {string} newStatus  - The status just applied
 * @param {Object} currentUser - authStore.currentUser ({ full_name, email, username })
 */
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

  const changedByName = currentUser?.full_name ?? currentUser?.username ?? 'System'
  const senderEmail = currentUser?.email ?? ''   // from Users.email via /auth/me

  // Collect all team member emails from Users.email (populated via TeamMemberDto.email)
  const allTeamEmails = (project.team_members ?? [])
    .map(m => m.email)
    .filter(e => typeof e === 'string' && e.trim().length > 0)

  // "To" = Team Lead's email, or first member as fallback
  const teamLead = project.team_members?.find(m => m.role === 'Team Lead')
  const primaryRecipient = teamLead?.email ?? allTeamEmails[0] ?? ''

  const ccSet = new Set(allTeamEmails.filter(e => e !== primaryRecipient))
  if (senderEmail && senderEmail !== primaryRecipient) ccSet.add(senderEmail)
  const ccEmails = [...ccSet]

  const subject = `[NPI] ${project.proj_no} — Status Updated to: ${newStatus}`

  const body = [
    `PROJECT STATUS NOTIFICATION`,
    `${'─'.repeat(40)}`,
    ``,
    `Project No   : ${project.proj_no}`,
    `Project Name : ${project.proj_name}`,
    `New Status   : ${newStatus}`,
    `Changed By   : ${changedByName}`,
    `Timestamp    : ${timestamp}`,
    ``,
    `${'─'.repeat(40)}`,
    `Please log in to the NPI system to review`,
    `any outstanding tasks or approvals.`,
    ``,
    `This is a system-generated notification.`,
  ].join('\n')

  openMailClient({
    to: primaryRecipient,
    cc: ccEmails,
    subject,
    body,
  })
}
