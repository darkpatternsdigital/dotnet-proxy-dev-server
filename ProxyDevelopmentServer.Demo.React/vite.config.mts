import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [react()],
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
