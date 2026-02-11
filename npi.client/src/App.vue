<template>
  <v-app>
    <NavBar />
    <v-main class="bg-grey-lighten-4">
      <router-view />
    </v-main>

    <v-snackbar v-model="snackbar.show"
                :color="snackbar.color"
                :timeout="3000">
      {{ snackbar.message }}

      <template #actions>
        <v-btn variant="text" @click="snackbar.show = false">
          Close
        </v-btn>
      </template>
    </v-snackbar>
  </v-app>
</template>

<script setup>
  import { ref, onMounted, provide } from 'vue'
  import NavBar from '@/components/NavBar.vue'

  const snackbar = ref({
    show: false,
    message: "",
    color: "success",
  });

  function showSnackbar(message, color = "success") {
    snackbar.value.message = message;
    snackbar.value.color = color;
    snackbar.value.show = true;
  }

  provide("showSnackbar", showSnackbar);
</script>
