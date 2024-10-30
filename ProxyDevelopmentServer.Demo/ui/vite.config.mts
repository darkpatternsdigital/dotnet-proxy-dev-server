import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [
		react(),
	],
	build: {
		outDir: './dist',
		emptyOutDir: true,
		assetsInlineLimit: 0,
	},
	resolve: {},
	server: {
		hmr: {
			path: '/.vite/hmr',
		},
	},
});