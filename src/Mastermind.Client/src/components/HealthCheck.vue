<template>
	<section class="grid">
		<button @click="checkApiHealth">
			Test backend health
		</button>

		<pre v-if="result" class="output">{{ result }}</pre>
		<pre v-if="error" class="error">{{ error }}</pre>
	</section>
</template>

<script setup lang="ts">
	import { ref } from 'vue'
	import client from '@/services/httpClient'

	const result = ref<string>('')
	const error = ref<string>('')

	async function checkApiHealth(): Promise<void> {
		result.value = ''
		error.value = ''
		try {
			const res = await client.get<string>('/api/health', { responseType: 'text' })
			result.value = typeof res.data === 'string' ? res.data : JSON.stringify(res.data)
		} catch (e: unknown) {
			error.value = e instanceof Error ? e.message : String(e)
		}
	}
</script>