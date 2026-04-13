import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    name: 'Dashboard',
    component: () => import('@/views/Dashboard.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/projects',
    name: 'Projects',
    component: () => import('@/views/Projects.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/projects/:id/setup',
    name: 'ProjectSetup',
    component: () => import('@/views/ProjectSetup.vue'),
    meta: { requiresAuth: true, requiresRole: ['Admin', 'Manager'] }
  },
  {
    path: '/projects/:id/gantt',
    name: 'ProjectGantt',
    component: () => import('@/views/ProjectGantt.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/tasks',
    name: 'Tasks',
    component: () => import('@/views/Tasks.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/files',
    name: 'Files',
    component: () => import('@/views/Files.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/enquiries',
    name: 'Enquiries',
    component: () => import('@/views/Enquiries.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/enquiries/new',
    name: 'NewEnquiry',
    component: () => import('@/views/EnquiryForm.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/enquiries/:id/edit',
    name: 'EditEnquiry',
    component: () => import('@/views/EnquiryForm.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/enquiries/:id/detail',
    name: 'EnquiryManage',
    component: () => import('@/views/EnquiryManage.vue'),
    meta: { requiresRole: ['Admin', 'Manager'] }
  },
  {
    path: '/settings',
    name: 'Settings',
    component: () => import('@/views/Settings.vue'),
    meta: { requiresAuth: true, requiresRole: ['Admin', 'Manager'] }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  if (authStore.token && !authStore.user) {
    await authStore.checkAuth()
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }

  if (to.path === '/login' && authStore.isAuthenticated) {
    next('/')
    return
  }

  if (to.meta.requiresRole) {
    const userRole = authStore.userRole
    if (!to.meta.requiresRole.includes(userRole)) {
      next('/')
      return
    }
  }

  next()
})

export default router
