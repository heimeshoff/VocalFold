/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./public/index.html",
    "./src/**/*.{js,jsx,ts,tsx,fs}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Company branding colors
        primary: {
          DEFAULT: '#25abfe',   // Primary brand color (blue)
          50: '#e6f5ff',
          100: '#b8e2ff',
          200: '#8acfff',
          300: '#5cbcff',
          400: '#25abfe',        // Main primary
          500: '#0c95e8',
          600: '#0a7bc2',
          700: '#08619c',
          800: '#064776',
          900: '#042d50',
        },
        secondary: {
          DEFAULT: '#ff8b00',   // Secondary brand color (orange)
          50: '#fff3e0',
          100: '#ffd9a3',
          200: '#ffc266',
          300: '#ffab29',
          400: '#ff8b00',        // Main secondary
          500: '#e67a00',
          600: '#cc6b00',
          700: '#b35c00',
          800: '#994d00',
          900: '#803e00',
        },
        // UI colors (dark theme)
        background: {
          dark: '#1a1a1a',      // DarkBackground
          card: '#2a2a2a',      // CardBackground
          sidebar: '#232323',   // SidebarBackground
        },
        text: {
          primary: '#FFFFFF',   // PrimaryText
          secondary: '#B0B0B0', // SecondaryText
        }
      }
    }
  },
  plugins: [],
}
