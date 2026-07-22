<template>
  <div class="page-root d-flex flex-column">

    <!-- Header -->
    <v-sheet class="header-strip px-6 py-3 flex-shrink-0 d-flex align-center justify-space-between" elevation="1">
      <div class="text-h6 font-weight-bold d-flex align-center">
        <v-icon class="mr-2" color="primary">mdi-file-document-edit-outline</v-icon>
        {{ isEdit ? 'Edit Enquiry' : 'New Sales Enquiry' }}
      </div>
      <div class="d-flex" style="gap:12px">
        <v-btn v-if="formData.status === ENQUIRY_STATUS.DRAFT"
               variant="outlined" color="secondary"
               :loading="saving" @click="saveDraft">
          <v-icon start>mdi-content-save</v-icon>
          Save Draft
        </v-btn>
        <v-btn color="primary" :loading="saving" @click="handleSubmit">
          <v-icon start>mdi-send</v-icon>
          {{ formData.status === ENQUIRY_STATUS.DRAFT ? 'Submit Enquiry' : 'Update & Submit' }}
        </v-btn>
        <v-btn variant="text" density="comfortable" @click="$router.back()">
          <v-icon start>mdi-arrow-left</v-icon> Back
        </v-btn>
      </div>
    </v-sheet>

    <!-- Scrollable Body -->
    <div class="flex-grow-1 overflow-y-auto form-body pa-6">
      <v-container fluid class="pa-0 mx-auto" style="max-width:1000px">

        <!-- 1. NPI Category -->
        <v-card class="mb-5 form-card" elevation="0" border>
          <v-card-title class="bg-grey-lighten-4 text-subtitle-1 font-weight-bold pa-3 d-flex align-center">
            1. NPI Category
            <v-progress-circular v-if="enquiryStore.loading" indeterminate size="18" class="ml-3" color="primary" />
          </v-card-title>
          <v-divider />
          <v-card-text class="pa-4">
            <v-radio-group v-model="formData.form_category" hide-details>
              <v-row dense>
                <v-col cols="12" sm="6" md="4"
                       v-for="cat in enquiryStore.categories"
                       :key="cat.category_id">
                  <v-radio :label="cat.category_name" :value="cat.category_name"
                           color="primary" density="compact" />
                </v-col>
              </v-row>
            </v-radio-group>
          </v-card-text>
        </v-card>

        <!-- Dynamic Sections -->
        <template v-for="(section, sIdx) in activeSections" :key="section.section_id">
          <v-card class="mb-5 form-card" elevation="0" border>
            <v-card-title class="bg-grey-lighten-4 text-subtitle-1 font-weight-bold pa-3">
              {{ sIdx + 2 }}. {{ section.section_label }}
            </v-card-title>
            <v-divider />
            <v-card-text class="pa-4">
              <v-row dense>
                <v-col v-for="field in section.fields"
                       :key="field.field_id"
                       cols="12" md="6">

                  <!-- Text / Number -->
                  <v-text-field v-if="field.field_type === 'text' || field.field_type === 'number'"
                                v-model="formData.field_values[section.section_key][field.field_key]"
                                :label="field.field_label + (field.is_required ? ' *' : '')"
                                :type="field.field_type"
                                density="compact" variant="outlined"
                                hide-details="auto" class="mb-2"
                                :rules="field.is_required ? [v => !!v || 'Required'] : []" />

                  <!-- Date -->
                  <v-text-field v-else-if="field.field_type === 'date'"
                                v-model="formData.field_values[section.section_key][field.field_key]"
                                :label="field.field_label + (field.is_required ? ' *' : '')"
                                type="date"
                                density="compact" variant="outlined"
                                hide-details="auto" class="mb-2"
                                :rules="field.is_required ? [v => !!v || 'Required'] : []" />

                  <!-- Dropdown -->
                  <div v-else-if="field.field_type === 'select'">
                    <v-select v-model="formData.field_values[section.section_key][field.field_key]"
                              :label="field.field_label + (field.is_required ? ' *' : '')"
                              :items="field.options ?? []"
                              density="compact" variant="outlined"
                              hide-details="auto" class="mb-2"
                              :rules="field.is_required ? [v => !!v || 'Required'] : []" />

                    <!-- Custom Field for 'Others' -->
                    <v-expand-transition>
                      <v-text-field v-if="formData.field_values[section.section_key][field.field_key] === 'Others'"
                                    v-model="customOthers[`${section.section_key}_${field.field_key}`]"
                                    :label="field.is_required ? 'Please specify *' : 'Please specify'"
                                    density="compact" variant="outlined"
                                    hide-details="auto" class="mb-2 mt-1"
                                    :rules="field.is_required ? [v => !!v || 'Required'] : []" />
                    </v-expand-transition>
                  </div>

                  <!-- Textarea -->
                  <v-textarea v-else-if="field.field_type === 'textarea'"
                              v-model="formData.field_values[section.section_key][field.field_key]"
                              :label="field.field_label + (field.is_required ? ' *' : '')"
                              rows="3"
                              density="compact" variant="outlined"
                              hide-details="auto" class="mb-2"
                              :rules="field.is_required ? [v => !!v || 'Required'] : []" />
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
        </template>

        <!-- Customer Reference -->
        <v-card class="mb-5 form-card" elevation="0" border>
          <v-card-title class="bg-grey-lighten-4 text-subtitle-1 font-weight-bold pa-3">
            {{ activeSections.length + 2 }}. Customer Reference
          </v-card-title>
          <v-divider />
          <v-card-text class="pa-4">
            <v-row dense>
              <v-col cols="12" md="6">
                <v-select v-model="formData.CustomerRef.mould_ownership"
                          label="Mould Ownership"
                          :items="['Customer', 'J&J', 'N/A']"
                          density="compact" variant="outlined" />
              </v-col>
              <v-col cols="12" md="6">
                <v-file-input multiple show-size counter
                              accept=".pdf,.doc,.docx,.dwg,.step,.stp,.jpg,.png"
                              label="Customer Reference Files"
                              prepend-inner-icon="mdi-upload" prepend-icon=""
                              density="compact" variant="outlined"
                              @change="handleFileSelect" />
              </v-col>
            </v-row>

            <!-- Previously uploaded files -->
            <v-row v-if="uploadedFiles.length > 0" dense>
              <v-col cols="12">
                <v-list density="compact" class="bg-grey-lighten-4 rounded mt-2">
                  <v-list-subheader class="text-caption font-weight-bold">Previously Uploaded</v-list-subheader>
                  <v-list-item v-for="file in uploadedFiles" :key="file.file_id">
                    <template #prepend>
                      <v-icon size="small">mdi-file-document-outline</v-icon>
                    </template>
                    <v-list-item-title class="text-body-2">{{ file.file_name }}</v-list-item-title>
                    <template #append>
                      <v-chip size="x-small" variant="tonal">{{ formatSize(file.file_size) }}</v-chip>
                    </template>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- Messages -->
        <v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-4" density="compact">
          {{ errorMessage }}
        </v-alert>
        <v-alert v-if="successMessage" type="success" variant="tonal" class="mb-4" density="compact">
          {{ successMessage }}
        </v-alert>

      </v-container>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted, watch } from 'vue'
  import { useRouter, useRoute } from 'vue-router'
  import { useEnquiryStore } from '@/stores/enquiry'
  import { api } from '@/utils/api'
  import { ENQUIRY_STATUS, REDIRECT_DELAY_MS } from '@/utils/constants'
  import { formatSize } from '@/utils/formatters'

  const router = useRouter()
  const route = useRoute()
  const enquiryStore = useEnquiryStore()

  const isEdit = ref(false)
  const saving = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')
  const selectedFiles = ref([])
  const uploadedFiles = ref([])
  const customOthers = ref({})

  const formData = ref({
    cust_id: null,
    form_category: '',
    status: ENQUIRY_STATUS.DRAFT,
    field_values: {},
    CustomerRef: { mould_ownership: '' }
  })

  // Compute sections based on category
  const activeSections = computed(() => {
    if (!formData.value.form_category) return []
    const cat = formData.value.form_category.toLowerCase()
    return (enquiryStore.sections ?? []).filter(s => {
      if (!s.trigger_keywords) return false
      return s.trigger_keywords.split(',').some(kw => cat.includes(kw.trim()))
    })
  })

  // Initialize field values
  watch(() => enquiryStore.sections, (sections) => {
    sections.forEach(s => {
      if (!formData.value.field_values[s.section_key]) {
        formData.value.field_values[s.section_key] = {}
      }
      s.fields?.forEach(f => {
        if (formData.value.field_values[s.section_key][f.field_key] === undefined) {
          formData.value.field_values[s.section_key][f.field_key] = ''
        }
      })
    })
  }, { immediate: true })

  function handleFileSelect(event) {
    selectedFiles.value = [...selectedFiles.value, ...Array.from(event.target.files)]
  }

  function validateRequiredFields() {
    if (!formData.value.form_category) return false
    for (const section of activeSections.value) {
      for (const field of (section.fields || [])) {
        const val = formData.value.field_values[section.section_key]?.[field.field_key]
        if (field.is_required) {
          if (!val || String(val).trim() === '') return false
          if (field.field_type === 'select' && val === 'Others') {
            const customVal = customOthers.value[`${section.section_key}_${field.field_key}`]
            if (!customVal || String(customVal).trim() === '') return false
          }
        }
      }
    }
    return true
  }

  function buildPayload() {
    const payloadFieldValues = JSON.parse(JSON.stringify(formData.value.field_values))
    let compName = 'TBD'

    for (const section of activeSections.value) {
      for (const field of (section.fields || [])) {
        const currentVal = payloadFieldValues[section.section_key]?.[field.field_key]

        if (field.field_type === 'select' && currentVal === 'Others') {
          const customVal = customOthers.value[`${section.section_key}_${field.field_key}`]
          payloadFieldValues[section.section_key][field.field_key] = (customVal && customVal.trim() !== '') ? customVal.trim() : ''
        }

        if ((field.field_key === 'company_name' || field.field_key === 'customer_name') && currentVal) {
          compName = String(currentVal).trim()
        }
      }
    }

    const payload = {
      form_category: formData.value.form_category,
      field_values: payloadFieldValues,
      CustomerRef: { mould_ownership: formData.value.CustomerRef.mould_ownership }
    }

    if (formData.value.cust_id) {
      payload.cust_id = formData.value.cust_id
    } else {
      payload.new_customer = {
        comp_name: compName,
        cust_addr: null,
        contact_name: null,
        contact_email: null,
        contact_phone: null
      }
    }

    return payload
  }

  async function uploadPendingFiles(enquiryId) {
    if (!selectedFiles.value.length) return

    let compName = 'Unknown'
    if (customerType.value === 'existing' && selectedCustomerInfo.value) {
      compName = selectedCustomerInfo.value.comp_name
    } else if (customerType.value === 'new') {
      compName = formData.value.new_customer.comp_name
    }

    const failed = []

    for (const file of selectedFiles.value) {
      const fd = new FormData()
      fd.append('file', file)

      try {
        await api.uploadFile(
          `/enquiry/${enquiryId}/upload?comp_name=${encodeURIComponent(compName)}`,
          fd
        )
      } catch (e) {
        console.error('File upload failed', file.name, e)
        failed.push(file.name)
      }
    }

    selectedFiles.value = []

    if (failed.length) {
      errorMessage.value =
        `Enquiry saved, but these files failed to upload: ${failed.join(', ')}`
    }
  }

  async function saveDraft() {
    saving.value = true
    errorMessage.value = ''
    successMessage.value = ''
    try {
      const result = isEdit.value
        ? await enquiryStore.updateEnquiry(route.params.id, buildPayload())
        : await enquiryStore.createEnquiry(buildPayload())

      if (result.success) {
        const id = isEdit.value ? route.params.id : result.data?.enquiry?.enquiry_id
        if (id) await uploadPendingFiles(id)
        successMessage.value = 'Draft saved.'
        setTimeout(() => router.push('/enquiries'), REDIRECT_DELAY_MS)
      } else {
        errorMessage.value = result.message
      }
    } catch (err) {
      errorMessage.value = err.message || 'An error occurred.'
    } finally {
      saving.value = false
    }
  }

  async function handleSubmit() {
    if (!validateRequiredFields()) {
      errorMessage.value = 'Please fill in all required fields.'
      return
    }
    saving.value = true
    errorMessage.value = ''
    successMessage.value = ''
    try {
      let result = isEdit.value
        ? await enquiryStore.updateEnquiry(route.params.id, buildPayload())
        : await enquiryStore.createEnquiry(buildPayload())

      if (result.success) {
        const id = isEdit.value ? route.params.id : result.data?.enquiry?.enquiry_id
        if (id) await uploadPendingFiles(id)
        result = await enquiryStore.submitEnquiry(id)
      }

      if (result.success) {
        successMessage.value = 'Enquiry submitted successfully.'
        setTimeout(() => router.push('/enquiries'), REDIRECT_DELAY_MS)
      } else {
        errorMessage.value = result.message
      }
    } catch (err) {
      errorMessage.value = err.message
    } finally {
      saving.value = false
    }
  }

  onMounted(async () => {
    await enquiryStore.fetchConfig()

    // Restore existing data in edit mode
    if (route.params.id) {
      isEdit.value = true
      const result = await enquiryStore.fetchEnquiryById(route.params.id)

      if (result?.success && result.data) {
        const enq = result.data

        if (enq.cust_id) formData.value.cust_id = enq.cust_id
        formData.value.form_category = enq.form_category
        formData.value.status = enq.status

        if (enq.field_values) {
          Object.entries(enq.field_values).forEach(([sectionKey, fields]) => {
            if (!formData.value.field_values[sectionKey]) {
              formData.value.field_values[sectionKey] = {}
            }
            Object.assign(formData.value.field_values[sectionKey], fields)

            const configSection = (enquiryStore.sections ?? []).find(s => s.section_key === sectionKey)
            if (configSection) {
              Object.entries(fields).forEach(([fieldKey, val]) => {
                const configField = configSection.fields?.find(f => f.field_key === fieldKey)
                if (configField && configField.field_type === 'select') {
                  const isStandardOption = configField.options?.includes(val)
                  if (val && !isStandardOption && val !== 'Others') {
                    customOthers.value[`${sectionKey}_${fieldKey}`] = val
                    formData.value.field_values[sectionKey][fieldKey] = 'Others'
                  }
                }
              })
            }
          })
        }

        if (enq.CustomerRef) {
          formData.value.CustomerRef.mould_ownership = enq.CustomerRef.mould_ownership ?? ''
        }

        if (Array.isArray(enq.Files)) uploadedFiles.value = enq.Files
      }
    }
  })
</script>

<style scoped>
  .page-root {
    height: 100vh !important;
    overflow: hidden !important;
    background-color: #f5f6f8;
  }

  .header-strip {
    background: white;
    border-bottom: 1px solid #e0e0e0;
    z-index: 10;
  }

  .form-body {
    overflow-y: auto;
  }

  .form-card {
    border-radius: 8px;
    background-color: #ffffff;
  }
</style>
