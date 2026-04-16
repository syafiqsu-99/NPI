<template>
  <div class="module-root d-flex flex-column pa-3 ga-3">

    <!-- Row 1: Page title -->
    <div class="flex-shrink-0">
      <h2 class="text-h6 font-weight-bold">Enquiry Form</h2>
    </div>

    <!-- Row 2: Sub-tabs + loading indicator -->
    <v-card class="flex-shrink-0" elevation="1">
      <v-tabs v-model="tab" bg-color="grey-lighten-4" color="primary" density="compact">
        <v-tab value="categories">
          <v-icon start size="18">mdi-tag-multiple</v-icon>
          Categories
        </v-tab>
        <v-tab value="sections">
          <v-icon start size="18">mdi-view-list</v-icon>
          Form Sections
        </v-tab>
        <v-tab value="fields">
          <v-icon start size="18">mdi-form-textbox</v-icon>
          Fields
        </v-tab>
      </v-tabs>
      <v-progress-linear v-if="configStore.loading" indeterminate color="primary" height="2" />
    </v-card>

    <!-- Row 3: Tab content fills remaining height -->
    <v-window v-model="tab" class="flex-grow-1" style="min-height: 0;">

      <!-- ── CATEGORIES ──────────────────────────────────────────────────── -->
      <v-window-item value="categories">
        <div class="fill-height d-flex flex-column ga-3">
          <!-- Controls card -->
          <v-card class="flex-shrink-0" elevation="1">
            <v-card-text class="pa-3">
              <v-row dense align="center">
                <v-col cols="auto" class="ml-auto">
                  <v-btn color="primary" prepend-icon="mdi-plus" size="small" @click="openCatDialog()">
                    Add Category
                  </v-btn>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
          <!-- Data card -->
          <v-card class="flex-grow-1 d-flex flex-column" elevation="1" style="min-height: 0;">
            <v-data-table-virtual :headers="catHeaders"
                                  :items="configStore.categories"
                                  :loading="configStore.loading"
                                  density="comfortable"
                                  fixed-header
                                  height="300"
                                  class="npi-table flex-grow-1">

              <template #item.is_active="{ item }">
                <v-chip :color="item.is_active ? 'success' : 'grey'" size="small" variant="flat">
                  {{ item.is_active ? 'Active' : 'Inactive' }}
                </v-chip>
              </template>

              <template #item.actions="{ item }">
                <v-menu location="bottom end">
                  <template #activator="{ props }">
                    <v-btn icon="mdi-dots-vertical" variant="text" size="small" v-bind="props" />
                  </template>
                  <v-list density="compact" min-width="140">
                    <v-list-item @click="openCatDialog(item)">
                      <template #prepend>
                        <v-icon size="18">mdi-pencil</v-icon>
                      </template>
                      <v-list-item-title>Edit</v-list-item-title>
                    </v-list-item>
                    <v-divider />
                    <v-list-item @click="confirmDeleteCat(item)">
                      <template #prepend>
                        <v-icon size="18" color="error">mdi-delete</v-icon>
                      </template>
                      <v-list-item-title class="text-error">Delete</v-list-item-title>
                    </v-list-item>
                  </v-list>
                </v-menu>
              </template>
            </v-data-table-virtual>
          </v-card>
        </div>
      </v-window-item>

      <!-- ── SECTIONS ───────────────────────────────────────────────────── -->
      <v-window-item value="sections">
        <div class="fill-height d-flex flex-column ga-3">
          <!-- Controls card -->
          <v-card class="flex-shrink-0" elevation="1">
            <v-card-text class="pa-3">
              <v-row dense align="center">
                <v-col cols="12" sm="8">
                  <p class="text-caption text-grey mb-0">
                    Sections group related fields. The <strong>section_key</strong> is set at creation and cannot be changed.
                  </p>
                </v-col>
                <v-col cols="auto" class="ml-auto d-flex ga-2">
                  <v-btn variant="outlined" size="small" prepend-icon="mdi-sort" @click="openReorderDialog">
                    Reorder
                  </v-btn>
                  <v-btn color="primary" size="small" prepend-icon="mdi-plus" @click="openSectionDialog()">
                    Add Section
                  </v-btn>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
          <!-- Data card -->
          <v-card class="flex-grow-1 d-flex flex-column" elevation="1" style="min-height: 0;">
            <v-data-table-virtual :headers="sectionHeaders"
                                  :items="configStore.sections"
                                  :loading="configStore.loading"
                                  density="comfortable"
                                  fixed-header
                                  height="300"
                                  item-value="section_id"
                                  class="npi-table flex-grow-1">

              <template #item.section_key="{ item }">
                <v-chip size="small" color="primary" variant="tonal" label>
                  {{ item.section_key }}
                </v-chip>
              </template>

              <template #item.trigger_keywords="{ item }">
                <div class="d-flex flex-wrap ga-1">
                  <v-chip v-for="kw in splitKeywords(item.trigger_keywords)" :key="kw" size="x-small" variant="outlined">
                    {{ kw }}
                  </v-chip>
                  <span v-if="!item.trigger_keywords" class="text-caption text-grey">None</span>
                </div>
              </template>

              <template #item.fields="{ item }">
                <v-chip size="small" variant="tonal" color="info">
                  {{ item.fields?.length ?? 0 }} fields
                </v-chip>
              </template>

              <template #item.is_active="{ item }">
                <v-chip :color="item.is_active ? 'success' : 'grey'" size="small" variant="flat"
                        style="cursor:pointer" @click="toggleSection(item)">
                  {{ item.is_active ? 'Active' : 'Inactive' }}
                </v-chip>
              </template>

              <template #item.actions="{ item }">
                <v-menu location="bottom end">
                  <template #activator="{ props }">
                    <v-btn icon="mdi-dots-vertical" variant="text" size="small" v-bind="props" />
                  </template>
                  <v-list density="compact" min-width="160">
                    <v-list-item @click="openSectionDialog(item)">
                      <template #prepend>
                        <v-icon size="18">mdi-pencil</v-icon>
                      </template>
                      <v-list-item-title>Edit</v-list-item-title>
                    </v-list-item>
                    <v-list-item @click="openFieldDialog(null, item)">
                      <template #prepend>
                        <v-icon size="18" color="primary">mdi-form-textbox-plus</v-icon>
                      </template>
                      <v-list-item-title>Add Field</v-list-item-title>
                    </v-list-item>
                    <v-divider />
                    <v-list-item @click="confirmDeleteSection(item)">
                      <template #prepend>
                        <v-icon size="18" color="error">mdi-delete</v-icon>
                      </template>
                      <v-list-item-title class="text-error">Delete</v-list-item-title>
                    </v-list-item>
                  </v-list>
                </v-menu>
              </template>

              <template #expanded-row="{ item }">
                <tr>
                  <td :colspan="sectionHeaders.length" class="pa-0">
                    <v-data-table-virtual density="compact" class="bg-grey-lighten-5">
                      <thead>
                        <tr>
                          <th class="text-caption">Order</th>
                          <th class="text-caption">Key</th>
                          <th class="text-caption">Label</th>
                          <th class="text-caption">Type</th>
                          <th class="text-caption">Required</th>
                          <th class="text-caption">Active</th>
                          <th class="text-caption">Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr v-for="field in sortedFields(item)" :key="field.field_id">
                          <td>{{ field.display_order }}</td>
                          <td><v-chip size="x-small" variant="outlined">{{ field.field_key }}</v-chip></td>
                          <td>{{ field.field_label }}</td>
                          <td><v-chip size="x-small" color="secondary" variant="tonal">{{ field.field_type }}</v-chip></td>
                          <td>
                            <v-icon v-if="field.is_required" size="small" color="error">mdi-asterisk</v-icon>
                            <v-icon v-else size="small" color="grey">mdi-minus</v-icon>
                          </td>
                          <td>
                            <v-chip :color="field.is_active ? 'success' : 'grey'" size="x-small" variant="flat">
                              {{ field.is_active ? 'On' : 'Off' }}
                            </v-chip>
                          </td>
                          <td>
                            <v-btn icon="mdi-pencil" size="x-small" variant="text" @click="openFieldDialog(field, item)" />
                            <v-btn icon="mdi-delete" size="x-small" variant="text" color="error" @click="confirmDeleteField(field)" />
                          </td>
                        </tr>
                        <tr v-if="!item.fields?.length">
                          <td colspan="7" class="text-center pa-3 text-caption text-grey">
                            No fields yet —
                            <span class="text-primary" style="cursor:pointer" @click="openFieldDialog(null, item)">
                              add the first field
                            </span>
                          </td>
                        </tr>
                      </tbody>
                    </v-data-table-virtual>
                  </td>
                </tr>
              </template>

            </v-data-table-virtual>
          </v-card>
        </div>
      </v-window-item>

      <!-- ── FIELDS ─────────────────────────────────────────────────────── -->
      <v-window-item value="fields">
        <div class="fill-height d-flex flex-column ga-3">
          <!-- Controls card -->
          <v-card class="flex-shrink-0" elevation="1">
            <v-card-text class="pa-3">
              <v-row dense align="center">
                <v-col cols="12" sm="4">
                  <v-select v-model="fieldSectionFilter"
                            :items="sectionFilterItems"
                            item-title="label"
                            item-value="key"
                            label="Filter by section"
                            variant="outlined"
                            density="compact"
                            clearable
                            hide-details />
                </v-col>
                <v-col cols="auto" class="ml-auto">
                  <v-btn color="primary" prepend-icon="mdi-plus" size="small" @click="openFieldDialog()">
                    Add Field
                  </v-btn>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
          <!-- Data cards: one per section, scrollable container -->
          <div class="flex-grow-1 overflow-y-auto" style="min-height: 0;">
            <v-card v-for="section in configStore.sections" :key="section.section_id"
                    class="mb-3"
                    :class="{ 'd-none': fieldSectionFilter && fieldSectionFilter !== section.section_key }"
                    elevation="1">

              <v-card-title class="bg-grey-lighten-4 text-subtitle-1 pa-3">
                <v-chip size="small" color="primary" variant="tonal" class="mr-2">
                  {{ section.section_key }}
                </v-chip>
                {{ section.section_label }}
              </v-card-title>

              <v-data-table-virtual :headers="fieldHeaders"
                                    :items="section.fields || []"
                                    density="compact"
                                    hide-default-footer
                                    :items-per-page="-1"
                                    class="npi-table">

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
                  <v-menu location="bottom end">
                    <template #activator="{ props }">
                      <v-btn icon="mdi-dots-vertical" variant="text" size="small" v-bind="props" />
                    </template>
                    <v-list density="compact" min-width="140">
                      <v-list-item @click="openFieldDialog(item, section)">
                        <template #prepend>
                          <v-icon size="18">mdi-pencil</v-icon>
                        </template>
                        <v-list-item-title>Edit</v-list-item-title>
                      </v-list-item>
                      <v-divider />
                      <v-list-item @click="confirmDeleteField(item)">
                        <template #prepend>
                          <v-icon size="18" color="error">mdi-delete</v-icon>
                        </template>
                        <v-list-item-title class="text-error">Delete</v-list-item-title>
                      </v-list-item>
                    </v-list>
                  </v-menu>
                </template>
              </v-data-table-virtual>
            </v-card>
          </div>
        </div>
      </v-window-item>

    </v-window>

    <!-- ── CATEGORY DIALOG ─────────────────────────────────────────────── -->
    <v-dialog v-model="catDialog" max-width="500" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          {{ editCat ? 'Edit Category' : 'Add Category' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-text-field v-model="catForm.category_name" label="Category Name *"
                        variant="outlined" :rules="[required]" />
          <v-text-field v-model.number="catForm.display_order" label="Display Order"
                        type="number" variant="outlined" class="mt-2" />
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

    <!-- ── SECTION DIALOG ──────────────────────────────────────────────── -->
    <v-dialog v-model="sectionDialog" max-width="600" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          {{ editSection ? 'Edit Section' : 'Add Form Section' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field v-model="sectionForm.section_key"
                            label="Section Key *" variant="outlined"
                            :rules="[required, noSpaces]"
                            :readonly="!!editSection"
                            :hint="editSection ? 'Section key cannot be changed.' : 'e.g. generalInfo (camelCase)'"
                            persistent-hint />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model="sectionForm.section_label"
                            label="Display Label *" variant="outlined"
                            :rules="[required]"
                            hint="Shown as the section heading in the form." />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model="sectionForm.trigger_keywords"
                            label="Trigger Keywords" variant="outlined"
                            hint="Comma-separated keywords that activate this section."
                            persistent-hint />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model.number="sectionForm.display_order"
                            label="Display Order" type="number" variant="outlined" />
            </v-col>
            <v-col cols="12" md="6" class="d-flex align-center">
              <v-switch v-model="sectionForm.is_active" label="Active" color="success" hide-details />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="sectionDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="elevated" :loading="saving" @click="saveSection">
            {{ editSection ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── FIELD DIALOG ────────────────────────────────────────────────── -->
    <v-dialog v-model="fieldDialog" max-width="640" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">
          {{ editField ? 'Edit Field' : 'Add Field' }}
        </v-card-title>
        <v-card-text class="pt-4">
          <v-row>
            <v-col cols="12">
              <v-select v-model="fieldForm.section_id"
                        :items="configStore.sections"
                        item-title="section_label" item-value="section_id"
                        label="Section *" variant="outlined" :rules="[required]" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model="fieldForm.field_key"
                            label="Field Key *" variant="outlined"
                            :rules="[required, noSpaces]"
                            :readonly="!!editField"
                            hint="e.g. company_name (snake_case)" persistent-hint />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model="fieldForm.field_label"
                            label="Display Label *" variant="outlined" :rules="[required]" />
            </v-col>
            <v-col cols="12" md="6">
              <v-select v-model="fieldForm.field_type"
                        :items="fieldTypes" label="Field Type" variant="outlined"
                        @update:model-value="onFieldTypeChange" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model.number="fieldForm.display_order"
                            label="Display Order" type="number" variant="outlined" />
            </v-col>
            <v-col v-if="fieldForm.field_type === 'select'" cols="12">
              <v-combobox v-model="fieldForm.options"
                          label="Dropdown Options *"
                          multiple chips closable-chips variant="outlined"
                          hint="Type each option and press Enter." persistent-hint />
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

    <!-- ── REORDER DIALOG ─────────────────────────────────────────────── -->
    <v-dialog v-model="reorderDialog" max-width="500" persistent>
      <v-card>
        <v-card-title class="bg-primary text-white text-subtitle-1">Reorder Sections</v-card-title>
        <v-card-text class="pa-4">
          <p class="text-caption text-grey mb-4">Drag items to set the display order.</p>
          <v-list density="compact" class="border rounded">
            <v-list-item v-for="(sec, idx) in reorderList" :key="sec.section_id"
                         class="border-b" draggable="true"
                         style="cursor:grab"
                         @dragstart="onDragStart(idx)"
                         @dragover.prevent
                         @drop="onDrop(idx)">
              <template #prepend>
                <v-icon color="grey">mdi-drag</v-icon>
              </template>
              <v-list-item-title>
                <v-chip size="x-small" color="primary" variant="tonal" class="mr-2">{{ sec.section_key }}</v-chip>
                {{ sec.section_label }}
              </v-list-item-title>
              <template #append>
                <span class="text-caption text-grey">{{ idx + 1 }}</span>
              </template>
            </v-list-item>
          </v-list>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="reorderDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="elevated" :loading="saving" @click="saveReorder">
            Save Order
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── DELETE DIALOG ──────────────────────────────────────────────── -->
    <v-dialog v-model="deleteDialog" max-width="440">
      <v-card>
        <v-card-title class="bg-error text-white text-subtitle-1">
          <v-icon class="mr-2">mdi-alert</v-icon>
          Confirm Delete
        </v-card-title>
        <v-card-text class="pt-4">
          <v-alert type="warning" variant="tonal" class="mb-3">{{ deleteDialogMessage }}</v-alert>
          <p class="text-body-2">
            If this item has been used in any enquiry, it will be
            <strong>deactivated</strong> rather than deleted to preserve historical data.
          </p>
        </v-card-text>
        <v-card-actions class="pa-4">
          <v-spacer />
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="elevated" :loading="saving" @click="executeDelete">
            Confirm
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="snackbar" :color="snackbarColor" :timeout="3500">
      {{ snackbarMsg }}
      <template #actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>

  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useNpiFormConfigStore } from '@/stores/npiFormConfig'

  const configStore = useNpiFormConfigStore()

  const tab = ref('categories')
  const saving = ref(false)
  const snackbar = ref(false)
  const snackbarMsg = ref('')
  const snackbarColor = ref('success')

  // Validation rules
  const required = v => (!!v && String(v).trim() !== '') || 'This field is required.'
  const noSpaces = v => !/\s/.test(v) || 'No spaces allowed.'

  // Table headers
  const catHeaders = [
    { title: 'Order', key: 'display_order', width: '80px' },
    { title: 'Category Name', key: 'category_name' },
    { title: 'Status', key: 'is_active', width: '110px' },
    { title: '', key: 'actions', sortable: false, width: '60px' }
  ]

  const sectionHeaders = [
    { title: 'Order', key: 'display_order', width: '70px' },
    { title: 'Key', key: 'section_key', width: '160px' },
    { title: 'Label', key: 'section_label' },
    { title: 'Triggers', key: 'trigger_keywords' },
    { title: 'Fields', key: 'fields', width: '100px' },
    { title: 'Status', key: 'is_active', width: '110px' },
    { title: '', key: 'actions', sortable: false, width: '60px' }
  ]

  const fieldHeaders = [
    { title: 'Order', key: 'display_order', width: '70px' },
    { title: 'Key', key: 'field_key', width: '180px' },
    { title: 'Label', key: 'field_label' },
    { title: 'Type', key: 'field_type', width: '100px' },
    { title: 'Req.', key: 'is_required', width: '60px' },
    { title: 'Status', key: 'is_active', width: '80px' },
    { title: '', key: 'actions', sortable: false, width: '60px' }
  ]

  const fieldTypes = ['text', 'number', 'date', 'select', 'textarea']

  // ── Category state ────────────────────────────────────────────────────────────
  const catDialog = ref(false)
  const editCat = ref(null)
  const catForm = ref({ category_name: '', display_order: 0, is_active: true })

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
      showSnack(editCat.value ? 'Category updated.' : 'Category created.')
      catDialog.value = false
    } else {
      showSnack(result?.message || 'Operation failed.', 'error')
    }
  }

  // ── Section state ─────────────────────────────────────────────────────────────
  const sectionDialog = ref(false)
  const editSection = ref(null)
  const defaultSectionForm = () => ({
    section_key: '', section_label: '', trigger_keywords: '', display_order: 0, is_active: true
  })
  const sectionForm = ref(defaultSectionForm())

  function openSectionDialog(item = null) {
    editSection.value = item
    sectionForm.value = item
      ? {
        section_key: item.section_key, section_label: item.section_label,
        trigger_keywords: item.trigger_keywords ?? '', display_order: item.display_order, is_active: item.is_active
      }
      : defaultSectionForm()
    sectionDialog.value = true
  }

  async function saveSection() {
    saving.value = true
    let result
    if (editSection.value) {
      const { section_key, ...updateDto } = sectionForm.value
      result = await configStore.updateSection(editSection.value.section_id, updateDto)
    } else {
      result = await configStore.createSection(sectionForm.value)
    }
    saving.value = false
    if (result?.success) {
      showSnack(editSection.value ? 'Section updated.' : 'Section created.')
      sectionDialog.value = false
    } else {
      showSnack(result?.message || 'Operation failed.', 'error')
    }
  }

  async function toggleSection(item) {
    const result = await configStore.toggleSectionStatus(item.section_id)
    showSnack(result?.message || 'Status updated.', result?.success ? 'success' : 'error')
  }

  function splitKeywords(kws) {
    if (!kws) return []
    return kws.split(',').map(k => k.trim()).filter(Boolean)
  }

  function sortedFields(section) {
    return [...(section.fields || [])].sort((a, b) => a.display_order - b.display_order)
  }

  // ── Field state ───────────────────────────────────────────────────────────────
  const fieldDialog = ref(false)
  const editField = ref(null)
  const fieldSectionFilter = ref(null)

  const defaultFieldForm = () => ({
    section_id: null, field_key: '', field_label: '',
    field_type: 'text', options: [], is_required: false,
    is_active: true, display_order: 0
  })
  const fieldForm = ref(defaultFieldForm())

  const sectionFilterItems = computed(() =>
    configStore.sections.map(s => ({ label: s.section_label, key: s.section_key }))
  )

  function openFieldDialog(item = null, section = null) {
    editField.value = item
    fieldForm.value = item
      ? {
        section_id: item.section_id, field_key: item.field_key, field_label: item.field_label,
        field_type: item.field_type, options: item.options ?? [], is_required: item.is_required,
        is_active: item.is_active, display_order: item.display_order
      }
      : { ...defaultFieldForm(), section_id: section?.section_id ?? null }
    fieldDialog.value = true
  }

  function onFieldTypeChange(type) {
    if (type !== 'select') fieldForm.value.options = []
  }

  async function saveField() {
    saving.value = true
    const payload = { ...fieldForm.value }
    const result = editField.value
      ? await configStore.updateField(editField.value.field_id, payload)
      : await configStore.createField(payload)
    saving.value = false
    if (result?.success) {
      showSnack(editField.value ? 'Field updated.' : 'Field created.')
      fieldDialog.value = false
    } else {
      showSnack(result?.message || 'Operation failed.', 'error')
    }
  }

  // ── Reorder state ─────────────────────────────────────────────────────────────
  const reorderDialog = ref(false)
  const reorderList = ref([])
  let dragSrcIdx = null

  function openReorderDialog() {
    reorderList.value = [...configStore.sections].sort((a, b) => a.display_order - b.display_order)
    reorderDialog.value = true
  }
  function onDragStart(idx) { dragSrcIdx = idx }
  function onDrop(targetIdx) {
    if (dragSrcIdx === null || dragSrcIdx === targetIdx) return
    const items = [...reorderList.value]
    const [moved] = items.splice(dragSrcIdx, 1)
    items.splice(targetIdx, 0, moved)
    reorderList.value = items
    dragSrcIdx = null
  }
  async function saveReorder() {
    saving.value = true
    const ids = reorderList.value.map(s => s.section_id)
    const result = await configStore.reorderSections(ids)
    saving.value = false
    if (result?.success) {
      showSnack('Section order saved.')
      reorderDialog.value = false
    } else {
      showSnack(result?.message || 'Failed to save order.', 'error')
    }
  }

  // ── Delete state ──────────────────────────────────────────────────────────────
  const deleteDialog = ref(false)
  const deleteDialogMessage = ref('')
  let deleteTarget = null
  let deleteType = ''

  function confirmDeleteCat(item) {
    deleteTarget = item; deleteType = 'category'
    deleteDialogMessage.value = `Delete category "${item.category_name}"?`
    deleteDialog.value = true
  }
  function confirmDeleteSection(item) {
    deleteTarget = item; deleteType = 'section'
    deleteDialogMessage.value = `Delete section "${item.section_label}" (key: ${item.section_key})? This will also remove its ${item.fields?.length ?? 0} field(s).`
    deleteDialog.value = true
  }
  function confirmDeleteField(item) {
    deleteTarget = item; deleteType = 'field'
    deleteDialogMessage.value = `Delete field "${item.field_label}" (key: ${item.field_key})?`
    deleteDialog.value = true
  }
  async function executeDelete() {
    saving.value = true
    let result
    if (deleteType === 'category') result = await configStore.deleteCategory(deleteTarget.category_id)
    if (deleteType === 'section') result = await configStore.deleteSection(deleteTarget.section_id)
    if (deleteType === 'field') result = await configStore.deleteField(deleteTarget.field_id)
    saving.value = false
    deleteDialog.value = false
    showSnack(result?.message || 'Done.', result?.success ? 'success' : 'error')
  }

  function showSnack(msg, color = 'success') {
    snackbarMsg.value = msg; snackbarColor.value = color; snackbar.value = true
  }

  onMounted(() => configStore.fetchAllSections())
</script>

<style scoped>
  .module-root {
    height: 100%;
    overflow: hidden;
  }

  :deep(.v-window__container),
  :deep(.v-window-item) {
    height: 100% !important;
  }

  .fill-height {
    height: 100%;
  }

  .npi-table :deep(th) {
    font-weight: 600 !important;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.4px;
    background: #fafbfc !important;
  }

  .npi-table :deep(.v-table__wrapper) {
    height: 100%;
    overflow-y: auto;
  }
</style>
