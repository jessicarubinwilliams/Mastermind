<template>
	<main class="app-container">
		<h1>Mastermind</h1>

		<HealthCheck />
	</main>
</template>

<script setup lang="ts">
	import { ref } from 'vue'
	import HealthCheck from '@/components/HealthCheck.vue'
</script>