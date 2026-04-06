<template>
  <v-container fluid class="pa-6">
    <v-tabs v-model="tab" bg-color="grey-lighten-3">
      <v-tab value="categories">NPI Categories</v-tab>
      <v-tab value="fields">Form Fields</v-tab>
    </v-tabs>

    <v-window v-model="tab" class="mt-4">

      <!-- ── CATEGORIES ─────────────────────────────────────────────────── -->
      <v-window-item value="categories">
        <div class="d-flex justify-space-between align-center mb-4">
          <h3 class="text-h6">NPI Categories</h3>
          <v-btn color="primary" prepend-icon="mdi-plus" @click="openCatDialog()">
            Add Category
          </v-btn>
        </div>

        <v-card>
          <v-data-table :headers="catHeaders" :items="configStore.categories"
                        :loading="configStore.loading" density="comfortable">
            <template #item.is_active="{ item }">
              <v-chip :color="item.is_active ? 'success' : 'grey'" size="small" variant="flat">
                {{ item.is_active ? 'Active' : 'Inactive' }}
              </v-chip>
            </template>
            <template #item.actions="{ item }">
              <v-btn icon="mdi-pencil" size="small" variant="text"
                     @click="openCatDialog(item)" />
              <v-btn icon="mdi-delete" size="small" variant="text" color="error"
                     @click="deleteCategory(item)" />
            </template>
          </v-data-table>
        </v-card>
      </v-window-item>

      <!-- ── FIELDS ─────────────────────────────────────────────────────── -->
      <v-window-item value="fields">
        <div class="d-flex justify-space-between align-center mb-4">
          <h3 class="text-h6">Form Fields</h3>
          <v-btn color="primary" prepend-icon="mdi-plus" @click="openFieldDialog()">
            Add Field
          </v-btn>
        </div>

        <!-- Group by section for clarity -->
        <v-card v-for="section in configStore.sections" :key="section.section_id" class="mb-4">
          <v-card-title class="bg-grey-lighten-3 text-subtitle-1">
            <v-chip size="small" color="primary" variant="tonal" class="mr-2">
              {{ section.section_key }}
            </v-chip>
            {{ section.section_label }}
          </v-card-title>
          <v-data-table :headers="fieldHeaders" :items="section.fields"
                        density="compact" hide-default-footer :items-per-page="-1">
            <template #item.is_required="{ item }">
              <v-icon :color="item.is_required ? 'error' : 'grey'" size="small">
                {{ item.is_required ? 'mdi-asterisk' : 'mdi-minus' }}
              </v-icon>
            </template>
            <template #item.is_active="{ item }">
              <v-chip :color="item.is_active ? 'success' : 'grey'" size="x-small" variant="flat">
                {{ item.is_active ? 'Active' : 'Off' }}
              </v-chip>
            </template>
            <template #item.actions="{ item }">
              <v-btn icon="mdi-pencil" size="small" variant="text"
                     @click="openFieldDialog(item)" />
              <v-btn icon="mdi-delete" size="small" variant="text" color="error"
                     @click="deleteField(item)" />
            </template>
          </v-data-table>
        </v-card>
      </v-window-item>
    </v-window>

    <!-- Category Dialog -->
    <v-dialog v-model="catDialog" max-width="500" persistent>
      <v-card>
        <v-card-title class="bg-primary">
          {{ editCat ? 'Edit Category' : 'Add Category' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-text-field v-model="catForm.category_name" label="Category Name *"
                        variant="outlined" :rules="[v => !!v || 'Required']" />
          <v-text-field v-model.number="catForm.display_order" label="Display Order"
                        type="number" variant="outlined" />
          <v-switch v-model="catForm.is_active" label="Active" color="success" hide-details />
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="catDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="elevated" :loading="saving" @click="saveCategory">
            {{ editCat ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Field Dialog -->
    <v-dialog v-model="fieldDialog" max-width="600" persistent>
      <v-card>
        <v-card-title class="bg-primary">
          {{ editField ? 'Edit Field' : 'Add Field' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-row>
            <v-col cols="12">
              <v-select v-model="fieldForm.section_id" label="Section *"
                        :items="sectionItems" item-title="title" item-value="value"
                        variant="outlined" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model="fieldForm.field_key" label="Field Key *"
                            variant="outlined" hint="e.g. company_name"
                            :rules="[v => !!v || 'Required']" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model="fieldForm.field_label" label="Field Label *"
                            variant="outlined" :rules="[v => !!v || 'Required']" />
            </v-col>
            <v-col cols="12" md="6">
              <v-select v-model="fieldForm.field_type" label="Field Type"
                        :items="['text','number','date','select']" variant="outlined" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model.number="fieldForm.display_order" label="Display Order"
                            type="number" variant="outlined" />
            </v-col>
            <v-col v-if="fieldForm.field_type === 'select'" cols="12">
              <v-combobox v-model="fieldForm.options" label="Options (press Enter to add)"
                          multiple chips closable-chips variant="outlined"
                          hint="Type each option and press Enter" persistent-hint />
            </v-col>
            <v-col cols="12" class="d-flex ga-4">
              <v-switch v-model="fieldForm.is_required" label="Required" color="error" hide-details />
              <v-switch v-model="fieldForm.is_active" label="Active" color="success" hide-details />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="fieldDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="elevated" :loading="saving" @click="saveField">
            {{ editField ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar" :color="snackbarColor" :timeout="3000">
      {{ snackbarMsg }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>import { ref, computed, onMounted } from 'vue'
import { useNpiFormConfigStore } from '@/stores/npiFormConfig'

const configStore = useNpiFormConfigStore()
const tab = ref('categories')
const saving = ref(false)
const snackbar = ref(false)
const snackbarMsg = ref('')
const snackbarColor = ref('success')

// Category state
const catDialog = ref(false)
const editCat = ref(null)
const catForm = ref({ category_name: '', display_order: 0, is_active: true })

const catHeaders = [
  { title: 'Order', value: 'display_order', width: '80px' },
  { title: 'Category Name', value: 'category_name' },
  { title: 'Status', value: 'is_active', width: '100px' },
  { title: 'Actions', value: 'actions', sortable: false, width: '100px' }
]

function openCatDialog(item = null) {
  editCat.value = item
  catForm.value = item
    ? { category_name: item.category_name, display_order: item.display_order, is_active: item.is_active }
    : { category_name: '', display_order: 0, is_active: true }
  catDialog.value = true
}

async function saveCategory() {
  saving.value = true
  const result = editCat.value
    ? await configStore.updateCategory(editCat.value.category_id, catForm.value)
    : await configStore.createCategory(catForm.value)
  saving.value = false
  if (result?.success) {
    showSnack(editCat.value ? 'Category updated' : 'Category created', 'success')
    catDialog.value = false
  } else {
    showSnack(result?.message || 'Operation failed', 'error')
  }
}

async function deleteCategory(item) {
  if (!confirm(`Deactivate "${item.category_name}"?`)) return
  const result = await configStore.deleteCategory(item.category_id)
  showSnack(result?.success ? 'Category deactivated' : result?.message, result?.success ? 'success' : 'error')
}

// Field state
const fieldDialog = ref(false)
const editField = ref(null)
const defaultFieldForm = () => ({
  section_id: null, field_key: '', field_label: '',
  field_type: 'text', options: [], is_required: false,
  is_active: true, display_order: 0
})
const fieldForm = ref(defaultFieldForm())

const fieldHeaders = [
  { title: 'Order', value: 'display_order', width: '70px' },
  { title: 'Key', value: 'field_key', width: '160px' },
  { title: 'Label', value: 'field_label' },
  { title: 'Type', value: 'field_type', width: '80px' },
  { title: 'Req.', value: 'is_required', width: '60px' },
  { title: 'Status', value: 'is_active', width: '80px' },
  { title: 'Actions', value: 'actions', sortable: false, width: '100px' }
]

const sectionItems = computed(() =>
  configStore.sections.map(s => ({ title: s.section_label, value: s.section_id }))
)

function openFieldDialog(item = null) {
  editField.value = item
  fieldForm.value = item
    ? { section_id: item.section_id, field_key: item.field_key, field_label: item.field_label,
        field_type: item.field_type, options: item.options ?? [], is_required: item.is_required,
        is_active: item.is_active, display_order: item.display_order }
    : defaultFieldForm()
  fieldDialog.value = true
}

async function saveField() {
  saving.value = true
  const result = editField.value
    ? await configStore.updateField(editField.value.field_id, fieldForm.value)
    : await configStore.createField(fieldForm.value)
  saving.value = false
  if (result?.success) {
    showSnack(editField.value ? 'Field updated' : 'Field created', 'success')
    fieldDialog.value = false
  } else {
    showSnack(result?.message || 'Operation failed', 'error')
  }
}

async function deleteField(item) {
  if (!confirm(`Deactivate field "${item.field_label}"?`)) return
  const result = await configStore.deleteField(item.field_id)
  showSnack(result?.success ? 'Field deactivated' : result?.message, result?.success ? 'success' : 'error')
}

function showSnack(msg, color = 'success') {
  snackbarMsg.value = msg; snackbarColor.value = color; snackbar.value = true
}

onMounted(() => configStore.fetchConfig())</script>
