<template>
  <v-container fluid fill-height class="d-flex flex-column">
    <v-row no-gutters align="center" justify="center">
      <v-col cols="12" class="text-center">
        <v-card class="mb-6" elevation="2">
          <v-card-title class="d-flex align-center justify-space-between">
            <div class="text-h6 font-weight-bold">
              <v-icon class="mr-2">mdi-file-document-edit-outline</v-icon>
              {{ isEdit ? 'Edit Enquiry' : 'New Sales Enquiry' }}
            </div>

            <div class="d-flex ga-2">
              <v-btn v-if="formData.status === 'Draft'"
                     variant="outlined"
                     color="secondary"
                     :loading="saving"
                     @click="saveDraft">
                <v-icon start>mdi-content-save</v-icon>
                Save Draft
              </v-btn>

              <v-btn color="primary"
                     :loading="saving"
                     @click="handleSubmit">
                <v-icon start>mdi-send</v-icon>
                {{ formData.status === 'Draft' ? 'Submit Enquiry' : 'Update & Submit' }}
              </v-btn>
            </div>
          </v-card-title>
        </v-card>
      </v-col>
    </v-row>

    <v-row no-gutters align="center" justify="center">
      <v-col cols="12" class="text-center">

        <!-- CUSTOMER INFORMATION SECTION - NEW -->
        <v-card class="mb-6" elevation="2">
          <v-card-title>
            <span class="text-h6">
              <v-icon class="mr-2">mdi-account-box</v-icon>
              Customer Information
            </span>
          </v-card-title>

          <v-card-text>
            <v-row>
              <v-col cols="12">
                <v-radio-group v-model="customerType" inline>
                  <v-radio label="Existing Customer" value="existing"></v-radio>
                  <v-radio label="New Customer" value="new"></v-radio>
                </v-radio-group>
              </v-col>

              <!-- Existing Customer Selection -->
              <v-col v-if="customerType === 'existing'" cols="12" md="6">
                <v-autocomplete v-model="formData.cust_id"
                                :items="customers"
                                item-title="comp_name"
                                item-value="cust_id"
                                label="Select Customer *"
                                :loading="loadingCustomers"
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

              <!-- Display Selected Customer Info -->
              <v-col v-if="customerType === 'existing' && selectedCustomerInfo" cols="12">
                <v-card variant="outlined" class="bg-grey-lighten-4">
                  <v-card-text>
                    <v-row dense>
                      <v-col cols="12" md="4">
                        <strong>Contact Person:</strong> {{ selectedCustomerInfo.contact_name || 'N/A' }}
                      </v-col>
                      <v-col cols="12" md="4">
                        <strong>Email:</strong> {{ selectedCustomerInfo.contact_email || 'N/A' }}
                      </v-col>
                      <v-col cols="12" md="4">
                        <strong>Phone:</strong> {{ selectedCustomerInfo.contact_phone || 'N/A' }}
                      </v-col>
                      <v-col cols="12">
                        <strong>Address:</strong> {{ selectedCustomerInfo.cust_addr || 'N/A' }}
                      </v-col>
                    </v-row>
                  </v-card-text>
                </v-card>
              </v-col>

              <!-- New Customer Form -->
              <template v-if="customerType === 'new'">
                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.comp_name"
                                label="Company Name *"
                                :rules="[v => !!v || 'Company name is required']"
                                prepend-inner-icon="mdi-domain"
                                required />
                </v-col>

                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.contact_name"
                                label="Contact Person *"
                                :rules="[v => !!v || 'Contact person is required']"
                                prepend-inner-icon="mdi-account"
                                required />
                </v-col>

                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.contact_email"
                                label="Email *"
                                type="email"
                                :rules="emailRules"
                                prepend-inner-icon="mdi-email"
                                required />
                </v-col>

                <v-col cols="12" md="6">
                  <v-text-field v-model="formData.new_customer.contact_phone"
                                label="Phone *"
                                :rules="[v => !!v || 'Phone is required']"
                                prepend-inner-icon="mdi-phone"
                                required />
                </v-col>

                <v-col cols="12">
                  <v-textarea v-model="formData.new_customer.cust_addr"
                              label="Address *"
                              :rules="[v => !!v || 'Address is required']"
                              prepend-inner-icon="mdi-map-marker"
                              rows="2"
                              required />
                </v-col>
              </template>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- NPI CATEGORY SECTION -->
        <v-card class="mb-6" elevation="2">
          <v-card-title>
            <span class="text-h6">1. NPI Category</span>
          </v-card-title>

          <v-card-text>
            <v-radio-group v-model="formData.npi_category" inline>
              <v-radio v-for="category in npiCategories"
                       :key="category"
                       :label="category"
                       :value="category" />
            </v-radio-group>
          </v-card-text>
        </v-card>

        <!-- GENERAL INFO SECTION -->
        <v-card v-if="showGeneralInfo" class="mb-6" elevation="2">
          <v-card-title>
            <span class="text-h6">2. General Information - Cap/ Lid</span>
          </v-card-title>

          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field label="Company Name"
                              v-model="formData.generalInfo.company_name"
                              required />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Estimated Quantity / Year"
                              type="number"
                              v-model.number="formData.generalInfo.estimated_qty_per_year" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Estimated Required Date"
                              type="date"
                              v-model="formData.generalInfo.estimated_required_date" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Color"
                              v-model="formData.generalInfo.color" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Material Used"
                              v-model="formData.generalInfo.material_used" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Weight (g)"
                              v-model="formData.generalInfo.weight_g" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select label="Neck Size (mm)"
                          :items="neckSizes"
                          v-model="formData.generalInfo.neck_size_mm" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select label="Shape"
                          :items="shapes"
                          v-model="formData.generalInfo.shape" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select label="Hot/ Cold Filling"
                          :items="filling"
                          v-model="formData.generalInfo.hot_cold_filling" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Quantity Required For First Submission"
                              type="number"
                              v-model="formData.generalInfo.qty_first_submission" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select label="Cap Matching With Bottle, Bottle Source By"
                          :items="capSources"
                          v-model="formData.generalInfo.cap_bottle_source" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Filling Content"
                              v-model="formData.generalInfo.filling_content" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select label="Capping Method"
                          :items="capMethod"
                          v-model="formData.generalInfo.capping_method" />
              </v-col>

              <v-col cols="12" md="6">
                <v-select label="Cap Seal"
                          :items="capSeals"
                          v-model="formData.generalInfo.cap_seal" />
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- SEAL INFO SECTION -->
        <v-card v-if="showSealInfo" class="mb-6" elevation="2">
          <v-card-title>
            <span class="text-h6">2. Seal/ Wadding/ Gasket</span>
          </v-card-title>

          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field label="Customer Name"
                              v-model="formData.sealInfo.customer_name"
                              required />
              </v-col>

              <v-col cols="12" md="6">
                <v-select label="Apply to which product?"
                          :items="products"
                          v-model="formData.sealInfo.apply_to_product" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Estimated Required Date"
                              type="date"
                              v-model="formData.sealInfo.estimated_required_date" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Reason Of Change"
                              v-model="formData.sealInfo.reason_of_change" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Quantity Required For First Submission"
                              type="number"
                              v-model="formData.sealInfo.qty_first_submission" />
              </v-col>

              <v-col cols="12" md="6">
                <v-text-field label="Others Requirements"
                              v-model="formData.sealInfo.other_requirements" />
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- CUSTOMER REFERENCE SECTION -->
        <v-card class="mb-6" elevation="2">
          <v-card-title>
            <span class="text-h6">3. Customer Reference</span>
          </v-card-title>

          <v-card-text>
            <v-row>
              <v-col cols="12" md="6">
                <v-select label="Mould Ownership"
                          :items="mouldOwnerships"
                          v-model="formData.customerRef.mould_ownership" />
              </v-col>

              <v-col cols="12" md="6">
                <v-file-input multiple
                              show-size
                              counter
                              accept=".pdf,.doc,.docx,.dwg,.step,.stp,.jpg,.png"
                              label="Customer Reference Files"
                              prepend-icon="mdi-upload"
                              @change="handleFileSelect" />
              </v-col>
            </v-row>

            <v-row v-if="uploadedFiles.length > 0">
              <v-col cols="12">
                <v-list>
                  <v-list-subheader>Uploaded Files</v-list-subheader>
                  <v-list-item v-for="file in uploadedFiles" :key="file.file_id">
                    <v-list-item-title>{{ file.file_name }}</v-list-item-title>
                    <template v-slot:append>
                      <v-chip size="small">{{ formatFileSize(file.file_size) }}</v-chip>
                    </template>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <v-alert v-if="errorMessage"
                 type="error"
                 variant="tonal"
                 class="mb-4">
          {{ errorMessage }}
        </v-alert>

        <v-alert v-if="successMessage"
                 type="success"
                 variant="tonal">
          {{ successMessage }}
        </v-alert>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
  import { ref, computed, onMounted, watch } from 'vue'
  import { useRouter, useRoute } from 'vue-router'
  import { useEnquiryStore } from '@/stores/enquiry'
  import { useCustomerStore } from '@/stores/customer'

  const router = useRouter()
  const route = useRoute()
  const enquiryStore = useEnquiryStore()
  const customerStore = useCustomerStore()

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

  const formData = ref({
    cust_id: null,
    new_customer: {
      comp_name: '',
      cust_addr: '',
      contact_name: '',
      contact_email: '',
      contact_phone: ''
    },
    npi_category: '',
    status: 'Draft',
    generalInfo: {
      company_name: '',
      estimated_qty_per_year: null,
      estimated_required_date: '',
      color: '',
      material_used: '',
      weight_g: null,
      neck_size_mm: '',
      shape: '',
      hot_cold_filling: '',
      qty_first_submission: null,
      cap_bottle_source: '',
      filling_content: '',
      capping_method: '',
      cap_seal: ''
    },
    sealInfo: {
      customer_name: '',
      apply_to_product: '',
      estimated_required_date: '',
      reason_of_change: '',
      qty_first_submission: null,
      other_requirements: ''
    },
    customerRef: {
      mould_ownership: ''
    }
  })

  const emailRules = [
    v => !!v || 'Email is required',
    v => /.+@.+\..+/.test(v) || 'Email must be valid'
  ]

  const npiCategories = [
    'New Design - Bottle/ Jar/ Tub/ Container',
    'Duplication - Bottle/ Jar/ Tub Container',
    'New Design - Cap/ Lid',
    'Duplication - Cap/ Lid',
    'Label',
    'Wadding/ Seal',
    'New Material/New Color to Existing Product'
  ]

  const neckSizes = ['A 32mm', 'AA 28mm', 'B 28mm', 'BS 38mm', 'BW 38mm', 'BX 38mm', 'C 28mm', 'CX 32mm', 'CY 28mm', 'CS 28mm', 'D 24mm', 'J 63mm', 'JN 90mm', 'W 110mm']
  const shapes = ['Round', 'Square', 'Hexagonal', 'Oval', 'Rectangular', 'Other']
  const filling = ['Hot', 'Cold']
  const capSources = ['Jebsen & Jessen - New', 'Jebsen & Jessen - Existing Bottle', 'Customer']
  const capMethod = ['Auto', 'Manual']
  const capSeals = ['N/A', 'Wadding', 'Seal', 'Gasket', 'Others']
  const products = ['New Cap', 'Existing Cap']
  const mouldOwnerships = ['Customer', 'J&J', 'N/A']

  const showGeneralInfo = computed(() => {
    const category = formData.value.npi_category.toLowerCase()
    return category.includes('bottle') || category.includes('jar') ||
      category.includes('tub') || category.includes('container') ||
      category.includes('cap') || category.includes('lid') ||
      category.includes('label') || category.includes('new material') ||
      category.includes('new color')
  })

  const showSealInfo = computed(() => {
    const category = formData.value.npi_category.toLowerCase()
    return category.includes('wadding') || category.includes('seal')
  })

  watch(customerType, (newType) => {
    if (newType === 'new') {
      formData.value.cust_id = null
      selectedCustomerInfo.value = null
    } else {
      formData.value.new_customer = {
        comp_name: '',
        cust_addr: '',
        contact_name: '',
        contact_email: '',
        contact_phone: ''
      }
    }
  })

  async function onCustomerSelect(custId) {
    if (custId) {
      const customer = customers.value.find(c => c.cust_id === custId)
      selectedCustomerInfo.value = customer
    } else {
      selectedCustomerInfo.value = null
    }
  }

  function handleFileSelect(event) {
    const files = Array.from(event.target.files)
    selectedFiles.value = [...selectedFiles.value, ...files]
  }

  function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
  }

  async function saveDraft() {
    if (!validateCustomer()) {
      errorMessage.value = 'Please select or enter customer information'
      return
    }

    saving.value = true
    errorMessage.value = ''
    successMessage.value = ''

    try {
      const enquiryData = buildEnquiryData()

      let result
      if (isEdit.value) {
        result = await enquiryStore.updateEnquiry(route.params.id, enquiryData)
      } else {
        result = await enquiryStore.createEnquiry(enquiryData)
      }

      if (result.success) {
        successMessage.value = 'Draft saved successfully!'

        if (selectedFiles.value.length > 0 && result.data.enquiry) {
          await uploadFiles(result.data.enquiry.enquiry_id)
        }

        setTimeout(() => {
          router.push('/enquiries')
        }, 1500)
      } else {
        errorMessage.value = result.message
      }
    } catch (err) {
      errorMessage.value = err.message || 'An error occurred while saving'
    } finally {
      saving.value = false
    }
  }

  async function handleSubmit() {
    if (!validateCustomer()) {
      errorMessage.value = 'Please select or enter customer information'
      return
    }

    if (!validateForm()) {
      errorMessage.value = 'Please fill in all required fields'
      return
    }

    saving.value = true
    errorMessage.value = ''
    successMessage.value = ''

    try {
      const enquiryData = buildEnquiryData()

      let result
      if (isEdit.value) {
        result = await enquiryStore.updateEnquiry(route.params.id, enquiryData)
        if (result.success) {
          if (selectedFiles.value.length > 0) {
            await uploadFiles(route.params.id)
          }
          result = await enquiryStore.submitEnquiry(route.params.id)
        }
      } else {
        result = await enquiryStore.createEnquiry(enquiryData)
        if (result.success && result.data.enquiry) {
          if (selectedFiles.value.length > 0) {
            await uploadFiles(result.data.enquiry.enquiry_id)
          }
          result = await enquiryStore.submitEnquiry(result.data.enquiry.enquiry_id)
        }
      }

      if (result.success) {
        successMessage.value = 'Enquiry submitted successfully!'
        setTimeout(() => {
          router.push('/enquiries')
        }, 1500)
      } else {
        errorMessage.value = result.message
      }
    } catch (err) {
      errorMessage.value = err.message
    } finally {
      saving.value = false
    }
  }

  function validateCustomer() {
    if (customerType.value === 'existing') {
      return !!formData.value.cust_id
    } else {
      return !!(formData.value.new_customer.comp_name &&
        formData.value.new_customer.contact_name &&
        formData.value.new_customer.contact_email &&
        formData.value.new_customer.contact_phone &&
        formData.value.new_customer.cust_addr)
    }
  }

  function buildEnquiryData() {
    const data = {
      npi_category: formData.value.npi_category
    }

    if (customerType.value === 'existing') {
      data.cust_id = formData.value.cust_id
    } else {
      data.new_customer = formData.value.new_customer
    }

    if (showGeneralInfo.value) {
      data.GeneralInfo = {
        company_name: formData.value.generalInfo.company_name,
        estimated_qty_per_year: formData.value.generalInfo.estimated_qty_per_year,
        estimated_required_date: formData.value.generalInfo.estimated_required_date || null,
        color: formData.value.generalInfo.color,
        material_used: formData.value.generalInfo.material_used,
        weight_g: formData.value.generalInfo.weight_g,
        neck_size_mm: formData.value.generalInfo.neck_size_mm,
        shape: formData.value.generalInfo.shape,
        hot_cold_filling: formData.value.generalInfo.hot_cold_filling,
        qty_first_submission: formData.value.generalInfo.qty_first_submission,
        cap_bottle_source: formData.value.generalInfo.cap_bottle_source,
        filling_content: formData.value.generalInfo.filling_content,
        capping_method: formData.value.generalInfo.capping_method,
        cap_seal: formData.value.generalInfo.cap_seal
      }
    }

    if (showSealInfo.value) {
      data.SealInfo = {
        customer_name: formData.value.sealInfo.customer_name,
        apply_to_product: formData.value.sealInfo.apply_to_product,
        estimated_required_date: formData.value.sealInfo.estimated_required_date || null,
        reason_of_change: formData.value.sealInfo.reason_of_change,
        qty_first_submission: formData.value.sealInfo.qty_first_submission,
        other_requirements: formData.value.sealInfo.other_requirements
      }
    }

    data.CustomerRef = {
      mould_ownership: formData.value.customerRef.mould_ownership
    }

    return data
  }

  function validateForm() {
    if (!formData.value.npi_category) return false
    if (showGeneralInfo.value && !formData.value.generalInfo.company_name) return false
    if (showSealInfo.value && !formData.value.sealInfo.customer_name) return false
    return true
  }

  async function uploadFiles(enquiryId) {
    for (const file of selectedFiles.value) {
      await enquiryStore.uploadFile(enquiryId, file)
    }
    selectedFiles.value = []
  }

  onMounted(async () => {
    loadingCustomers.value = true
    const customerResult = await customerStore.fetchCustomers()
    if (customerResult.success) {
      customers.value = customerResult.data
    }
    loadingCustomers.value = false

    if (route.params.id) {
      isEdit.value = true
      const result = await enquiryStore.fetchEnquiryById(route.params.id)
      if (result && result.success && result.data) {
        const enquiry = result.data

        if (enquiry.cust_id) {
          formData.value.cust_id = enquiry.cust_id
          customerType.value = 'existing'

          const customer = customers.value.find(c => c.cust_id === enquiry.cust_id)
          if (customer) {
            selectedCustomerInfo.value = customer
          }
        }

        formData.value.npi_category = enquiry.npi_category
        formData.value.status = enquiry.status

        if (enquiry.generalInfo) {
          formData.value.generalInfo = {
            company_name: enquiry.generalInfo.company_name || '',
            estimated_qty_per_year: enquiry.generalInfo.estimated_qty_per_year ?? null,
            estimated_required_date: enquiry.generalInfo.estimated_required_date || '',
            color: enquiry.generalInfo.color || '',
            material_used: enquiry.generalInfo.material_used || '',
            weight_g: enquiry.generalInfo.weight_g,
            neck_size_mm: enquiry.generalInfo.neck_size_mm || '',
            shape: enquiry.generalInfo.shape || '',
            hot_cold_filling: enquiry.generalInfo.hot_cold_filling || '',
            qty_first_submission: enquiry.generalInfo.qty_first_submission,
            cap_bottle_source: enquiry.generalInfo.cap_bottle_source || '',
            filling_content: enquiry.generalInfo.filling_content || '',
            capping_method: enquiry.generalInfo.capping_method || '',
            cap_seal: enquiry.generalInfo.cap_seal || ''
          }
        }

        if (enquiry.sealInfo) {
          formData.value.sealInfo = {
            customer_name: enquiry.sealInfo.customer_name || '',
            apply_to_product: enquiry.sealInfo.apply_to_product || '',
            estimated_required_date: enquiry.sealInfo.estimated_required_date || '',
            reason_of_change: enquiry.sealInfo.reason_of_change || '',
            qty_first_submission: enquiry.sealInfo.qty_first_submission ?? null,
            other_requirements: enquiry.sealInfo.other_requirements || ''
          }
        }

        if (enquiry.customerRef) {
          formData.value.customerRef.mould_ownership = enquiry.customerRef.mould_ownership || ''
        }

        if (Array.isArray(enquiry.files)) {
          uploadedFiles.value = enquiry.files
        }
      }
    }
  })
</script>
