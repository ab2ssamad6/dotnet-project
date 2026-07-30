var _a;
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'node:path';
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
        },
    },
    server: {
        port: 5173,
        host: true,
        proxy: {
            '/api': {
                target: (_a = process.env.VITE_PROXY_TARGET) !== null && _a !== void 0 ? _a : 'http://localhost:8080',
                changeOrigin: true,
            },
        },
    },
    preview: {
        port: 3000,
        host: true,
    },
    test: {
        globals: true,
        environment: 'jsdom',
        setupFiles: ['./src/test/setup.ts'],
        css: false,
    },
});
