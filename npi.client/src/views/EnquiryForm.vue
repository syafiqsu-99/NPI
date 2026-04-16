<template>
  <div class="page-root d-flex flex-column">

    <!-- Fixed header -->
    <v-sheet class="header-strip px-6 py-3 flex-shrink-0 d-flex align-center justify-space-between" elevation="1">
      <div class="text-h6 font-weight-bold d-flex align-center">
        <v-icon class="mr-2" color="primary">mdi-file-document-edit-outline</v-icon>
        {{ isEdit ? 'Edit Enquiry' : 'New Sales Enquiry' }}
      </div>
      <div class="d-flex" style="gap:12px">
        <v-btn v-if="formData.status === 'Draft'"
               variant="outlined" color="secondary"
               :loading="saving" @click="saveDraft">
          <v-icon start>mdi-content-save</v-icon>
          Save Draft
        </v-btn>
        <v-btn color="primary" :loading="saving" @click="handleSubmit">
          <v-icon start>mdi-send</v-icon>
          {{ formData.status === 'Draft' ? 'Submit Enquiry' : 'Update & Submit' }}
        </v-btn>
      </div>
    </v-sheet>

    <!-- Scrollable body -->
    <div class="flex-grow-1 overflow-y-auto form-body pa-6">
      <v-container fluid class="pa-0 mx-auto" style="max-width:1000px">

        <!-- ── Customer information ─────────────────────────────────────── -->
        <v-card class="mb-5 form-card" elevation="0" border>
          <v-card-title class="bg-grey-lighten-4 text-subtitle-1 font-weight-bold pa-3">
            <v-icon start size="20">mdi-account-box</v-icon>
            Customer Information
          </v-card-title>
          <v-divider />
          <v-card-text class="pa-4">
            <v-row dense>
              <v-col cols="12" class="mb-2">
                <v-radio-group v-model="customerType" inline hide-details>
                  <v-radio label="Existing Customer" value="existing" color="primary" density="compact" />
                  <v-radio label="New Customer" value="new" color="primary" density="compact" />
                </v-radio-group>
              </v-col>

              <!-- Existing customer autocomplete -->
              <v-col v-if="customerType === 'existing'" cols="12" md="6">
                <v-autocomplete v-model="formData.cust_id"
                                :items="customers"
                                item-title="comp_name"
                                item-value="cust_id"
                                label="Select Customer *"
                                :loading="loadingCustomers"
                                density="compact" variant="outlined"
                                clearable
                                @update:model-value="onCustomerSelect">
                  <template #item="{ props, item }">
                    <v-list-item v-bind="props">
                      <template #prepend>
                        <v-avatar color="primary" size="small">
                          <v-icon>mdi-domain</v-icon>
                        </v-avatar>
                      </template>
                      <v-list-item-title>{{ item.raw.comp_name }}</v-list-item-title>
                      <v-list-item-subtitle>{{ item.raw.contact_name }}</v-list-item-subtitle>
                    </v-list-item>
                  </template>
                </v-autocomplete>
              </v-col>

              <!-- Selected customer details -->
              <v-col v-if="customerType === 'existing' && selectedCustomerInfo" cols="12">
                <v-card variant="tonal" color="primary" class="mt-2">
                  <v-card-text class="py-3 px-4 text-body-2">
                    <v-row dense>
                      <v-col cols="12" md="4">
                        <strong>Contact:</strong> {{ selectedCustomerInfo.contact_name || 'N/A' }}
                      </v-col>
                      <v-col cols="12" md="4">
                        <strong>Email:</strong> {{ selectedCustomerInfo.contact_email || 'N/A' }}
                      </v-col>
                      <v-col cols="12" md="4">
                        <strong>Phone:</strong> {{ selectedCustomerInfo.contact_phone || 'N/A' }}
                      </v-col>
                    </v-row>
                  </v-card-text>
                </v-card>
              </v-col>

              <!-- New customer fields -->
              <template v-if="customerType === 'new'">
                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.comp_name" label="Company Name *"
                                prepend-inner-icon="mdi-domain" density="compact" variant="outlined"
                                :rules="[v => !!v || 'Required']" />
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.contact_name" label="Contact Person *"
                                prepend-inner-icon="mdi-account" density="compact" variant="outlined"
                                :rules="[v => !!v || 'Required']" />
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.contact_email" label="Email *"
                                type="email" prepend-inner-icon="mdi-email"
                                density="compact" variant="outlined" :rules="emailRules" />
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.contact_phone" label="Phone *"
                                prepend-inner-icon="mdi-phone" density="compact" variant="outlined"
                                :rules="[v => !!v || 'Required']" />
                </v-col>
                <v-col cols="12">
                  <v-textarea v-model="formData.new_customer.cust_addr" label="Address *"
                              prepend-inner-icon="mdi-map-marker" rows="2"
                              density="compact" variant="outlined"
                              :rules="[v => !!v || 'Required']" />
                </v-col>
              </template>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- ── NPI Category ─────────────────────────────────────────────── -->
        <v-card class="mb-5 form-card" elevation="0" border>
          <v-card-title class="bg-grey-lighten-4 text-subtitle-1 font-weight-bold pa-3 d-flex align-center">
            1. NPI Category
            <v-progress-circular v-if="configStore.loading" indeterminate size="18" class="ml-3" color="primary" />
          </v-card-title>
          <v-divider />
          <v-card-text class="pa-4">
            <v-radio-group v-model="formData.npi_category" hide-details>
              <v-row dense>
                <v-col cols="12" sm="6" md="4"
                       v-for="cat in configStore.categories"
                       :key="cat.category_id">
                  <v-radio :label="cat.category_name" :value="cat.category_name"
                           color="primary" density="compact" />
                </v-col>
              </v-row>
            </v-radio-group>
          </v-card-text>
        </v-card>

        <!-- ── Dynamic sections — rendered entirely from metadata ──────── -->
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

                    <!-- Custom Field for "Others" selection -->
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

        <!-- ── Customer Reference — always shown, always preserved ────── -->
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
                      <v-chip size="x-small" variant="tonal">{{ formatFileSize(file.file_size) }}</v-chip>
                    </template>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- Error / success banners -->
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
  import { useCustomerStore } from '@/stores/customer'
  import { useNpiFormConfigStore } from '@/stores/npiFormConfig'
  import { api } from '@/utils/api'

  const router = useRouter()
  const route = useRoute()
  const enquiryStore = useEnquiryStore()
  const customerStore = useCustomerStore()
  const configStore = useNpiFormConfigStore()

  const isEdit = ref(false)
  const saving = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')
  const selectedFiles = ref([])
  const uploadedFiles = ref([])
  const customerType = ref('existing')
  const customers = ref([])
  const loadingCustomers = ref(false)
  const selectedCustomerInfo = ref(null)

  const emailRules = [
    v => !!v || 'Email is required',
    v => /.+@.+\..+/.test(v) || 'Email must be valid'
  ]

  // Storage for text specified when 'Others' is selected in a dropdown
  const customOthers = ref({})

  // ── Reactive form data — mirrors EnquiryCreateDto exactly ────────────────────
  const formData = ref({
    cust_id: null,
    new_customer: { comp_name: '', cust_addr: '', contact_name: '', contact_email: '', contact_phone: '' },
    npi_category: '',
    status: 'Draft',
    field_values: {},
    CustomerRef: { mould_ownership: '' }
  })

  // ── Compute active sections based on chosen category ─────────────────────────
  const activeSections = computed(() => {
    if (!formData.value.npi_category) return []
    const cat = formData.value.npi_category.toLowerCase()
    return configStore.sections.filter(s => {
      if (!s.trigger_keywords) return false
      return s.trigger_keywords.split(',').some(kw => cat.includes(kw.trim()))
    })
  })

  // ── Initialise field_values entries whenever sections load or change ──────────
  watch(() => configStore.sections, (sections) => {
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

  // Clear customer-specific state when type switches
  watch(customerType, newType => {
    if (newType === 'new') {
      formData.value.cust_id = null
      selectedCustomerInfo.value = null
    } else {
      formData.value.new_customer = {
        comp_name: '', cust_addr: '', contact_name: '', contact_email: '', contact_phone: ''
      }
    }
  })

  // ── Helpers ───────────────────────────────────────────────────────────────────

  function onCustomerSelect(custId) {
    selectedCustomerInfo.value = custId
      ? customers.value.find(c => c.cust_id === custId) ?? null
      : null
  }

  function handleFileSelect(event) {
    selectedFiles.value = [...selectedFiles.value, ...Array.from(event.target.files)]
  }

  function formatFileSize(bytes) {
    if (!bytes) return '0 B'
    const k = 1024, sizes = ['B', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return (bytes / Math.pow(k, i)).toFixed(1) + ' ' + sizes[i]
  }

  function validateCustomer() {
    if (customerType.value === 'existing') return !!formData.value.cust_id
    const nc = formData.value.new_customer
    return !!(nc.comp_name && nc.contact_name && nc.contact_email && nc.contact_phone && nc.cust_addr)
  }

  function validateRequiredFields() {
    if (!formData.value.npi_category) return false
    for (const section of activeSections.value) {
      for (const field of (section.fields || [])) {
        const val = formData.value.field_values[section.section_key]?.[field.field_key]

        if (field.is_required) {
          // Check the primary field
          if (!val || String(val).trim() === '') return false

          // If "Others" is selected on a REQUIRED field, the custom specification must also be filled
          if (field.field_type === 'select' && val === 'Others') {
            const customVal = customOthers.value[`${section.section_key}_${field.field_key}`]
            if (!customVal || String(customVal).trim() === '') return false
          }
        }
      }
    }
    return true
  }

  // Builds the payload that matches EnquiryCreateDto exactly
  function buildPayload() {
    // Clone to ensure we don't accidentally mutate proxy state on the active UI
    const payloadFieldValues = JSON.parse(JSON.stringify(formData.value.field_values))

    for (const section of activeSections.value) {
      for (const field of (section.fields || [])) {
        if (field.field_type === 'select') {
          const currentVal = payloadFieldValues[section.section_key]?.[field.field_key]

          // Re-map actual value over 'Others' before submitting to database
          if (currentVal === 'Others') {
            const customVal = customOthers.value[`${section.section_key}_${field.field_key}`]
            if (customVal && customVal.trim() !== '') {
              payloadFieldValues[section.section_key][field.field_key] = customVal.trim()
            } else {
              // If it's an optional field and they left the custom input blank, save as blank instead of "Others"
              payloadFieldValues[section.section_key][field.field_key] = ''
            }
          }
        }
      }
    }

    const payload = {
      npi_category: formData.value.npi_category,
      field_values: payloadFieldValues,
      // Pass backups across various casing conventions to prevent missing IIS binding overrides
      FieldValues: payloadFieldValues,
      fieldValues: payloadFieldValues,
      CustomerRef: { mould_ownership: formData.value.CustomerRef.mould_ownership }
    }
    if (customerType.value === 'existing') {
      payload.cust_id = formData.value.cust_id
    } else {
      payload.new_customer = formData.value.new_customer
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

    for (const file of selectedFiles.value) {
      const fd = new FormData()
      fd.append('file', file)
      fd.append('enquiry_id', enquiryId)
      fd.append('proj_id', 0)
      fd.append('customer_name', compName)

      try {
        await api.uploadFile(`/file/upload-single`, fd)
      } catch (e) {
        console.error('File upload failed', e)
      }
    }
    selectedFiles.value = []
  }

  // ── Save draft ────────────────────────────────────────────────────────────────

  async function saveDraft() {
    if (!validateCustomer()) {
      errorMessage.value = 'Please select or provide customer information.'
      return
    }
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
        setTimeout(() => router.push('/enquiries'), 1500)
      } else {
        errorMessage.value = result.message
      }
    } catch (err) {
      errorMessage.value = err.message || 'An error occurred.'
    } finally {
      saving.value = false
    }
  }

  // ── Submit ────────────────────────────────────────────────────────────────────

  async function handleSubmit() {
    if (!validateCustomer()) {
      errorMessage.value = 'Please select or provide customer information.'
      return
    }
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
        setTimeout(() => router.push('/enquiries'), 1500)
      } else {
        errorMessage.value = result.message
      }
    } catch (err) {
      errorMessage.value = err.message
    } finally {
      saving.value = false
    }
  }

  // ── Mount: load config, customers, and existing data (edit mode) ──────────────

  onMounted(async () => {
    loadingCustomers.value = true
    const [, customerResult] = await Promise.all([
      configStore.fetchConfig(),
      customerStore.fetchCustomers()
    ])
    if (customerResult?.success) customers.value = customerResult.data
    loadingCustomers.value = false

    if (route.params.id) {
      isEdit.value = true
      const result = await enquiryStore.fetchEnquiryById(route.params.id)
      if (result?.success && result.data) {
        const enq = result.data

        // Restore customer selection
        if (enq.cust_id) {
          formData.value.cust_id = enq.cust_id
          customerType.value = 'existing'
          selectedCustomerInfo.value = customers.value.find(c => c.cust_id === enq.cust_id) ?? null
        }

        formData.value.npi_category = enq.npi_category
        formData.value.status = enq.status

        if (enq.field_values) {
          Object.entries(enq.field_values).forEach(([sectionKey, fields]) => {
            if (!formData.value.field_values[sectionKey]) {
              formData.value.field_values[sectionKey] = {}
            }
            Object.assign(formData.value.field_values[sectionKey], fields)

            // Extract "Others" texts inversely for Edit Draft flow
            const configSection = configStore.sections.find(s => s.section_key === sectionKey)
            if (configSection) {
              Object.entries(fields).forEach(([fieldKey, val]) => {
                const configField = configSection.fields?.find(f => f.field_key === fieldKey)
                if (configField && configField.field_type === 'select') {
                  const isStandardOption = configField.options?.includes(val)
                  // It was a customized typed choice
                  if (val && !isStandardOption && val !== 'Others') {
                    customOthers.value[`${sectionKey}_${fieldKey}`] = val
                    formData.value.field_values[sectionKey][fieldKey] = 'Others'
                  }
                }
              })
            }
          })
        }

        // Restore customer reference
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
