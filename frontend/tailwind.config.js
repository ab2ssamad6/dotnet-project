/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        brand: {
          50: "#edf9f7",
          100: "#d2f0eb",
          200: "#a6e1d9",
          300: "#71cac1",
          400: "#3fada4",
          500: "#1f918a",
          600: "#0f7a74",
          700: "#0c625e",
          800: "#0b4f4c",
          900: "#0b403e",
          950: "#042726",
        },
        ink: {
          50: "#f7f7f5",
          100: "#efeeeb",
          200: "#e3e1dc",
          300: "#cbc8c1",
          400: "#837f77",
          500: "#6f6c65",
          600: "#57544e",
          700: "#45423d",
          800: "#2e2c28",
          900: "#1d1b18",
          950: "#121110",
        },
        gold: {
          50: "#fdf8ec",
          100: "#faefd1",
          200: "#f4dda1",
          300: "#ebc468",
          400: "#e0aa3c",
          500: "#c98f26",
          600: "#a56f1e",
          700: "#7f551c",
          800: "#5d3f19",
          900: "#402c14",
        },
        shell: {
          DEFAULT: "#0e1c1a",
          raised: "#132523",
          hover: "#1a302d",
          border: "#22403c",
          muted: "#8fa6a1",
        },
        canvas: "#f5f4f1",
      },
      fontFamily: {
        sans: ["Inter", "system-ui", "Segoe UI", "Roboto", "Helvetica", "Arial", "sans-serif"],
      },
      borderRadius: {
        lg: "0.625rem",
        xl: "0.875rem",
        "2xl": "1.125rem",
        "3xl": "1.5rem",
      },
      boxShadow: {
        card: "0 1px 2px 0 rgb(29 27 24 / 0.04), 0 1px 3px 0 rgb(29 27 24 / 0.03)",
        raised: "0 2px 4px -2px rgb(29 27 24 / 0.10), 0 10px 24px -14px rgb(29 27 24 / 0.16)",
        pop: "0 24px 48px -24px rgb(29 27 24 / 0.32), 0 2px 6px -2px rgb(29 27 24 / 0.10)",
        ring: "0 0 0 1px rgb(29 27 24 / 0.06)",
      },
      backgroundImage: {
        "brand-gradient": "linear-gradient(135deg, #0f7a74 0%, #0b403e 100%)",
        "gold-gradient": "linear-gradient(135deg, #ebc468 0%, #c98f26 100%)",
        grain:
          "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='120' height='120'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='3'/%3E%3C/filter%3E%3Crect width='120' height='120' filter='url(%23n)' opacity='0.35'/%3E%3C/svg%3E\")",
      },
      keyframes: {
        "fade-in": {
          "0%": { opacity: "0" },
          "100%": { opacity: "1" },
        },
        "fade-up": {
          "0%": { opacity: "0", transform: "translateY(6px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
        shimmer: {
          "100%": { transform: "translateX(100%)" },
        },
        halo: {
          "0%, 100%": { opacity: "0.45", transform: "scale(1)" },
          "50%": { opacity: "0.9", transform: "scale(1.06)" },
        },
      },
      animation: {
        "fade-in": "fade-in 0.2s ease-out",
        "fade-up": "fade-up 0.28s cubic-bezier(0.16, 1, 0.3, 1)",
        halo: "halo 2.4s ease-in-out infinite",
      },
    },
  },
  plugins: [],
};
