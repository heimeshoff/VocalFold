import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  root: './dist',  // Use dist folder as root since Fable outputs there
  publicDir: path.resolve(__dirname, 'public'),
  build: {
    outDir: path.resolve(__dirname, 'dist-build'),  // Output to a different directory for production build
    emptyOutDir: true,  // Clean the build output directory
    sourcemap: false,
    rollupOptions: {
      input: path.resolve(__dirname, 'dist/index.html'),
    }
  },
  server: {
    port: 5173,
    open: false,
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
