import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path'

const backend = 'https://localhost:7267';

export default defineConfig({
	base: '/',
	root: './',
	build: {
		// write directly into the backend for a simple production serve
		outDir: '../Mastermind.Api/wwwroot',
		emptyOutDir: true,
		sourcemap: true,
	},
	server: {
		port: 3000,
		strictPort: true,
		proxy: {
			'/api': {
				target: backend,
				changeOrigin: true,
				secure: false,
			},
		},
	},
	plugins: [vue()],
	resolve: {
		alias: {
			'@': resolve(__dirname, 'src')
		}
	}
});
