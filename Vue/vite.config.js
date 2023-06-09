import { fileURLToPath, URL } from "url";

import { defineConfig } from "vite";

import vue from "@vitejs/plugin-vue";
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },

  // Local test
  // define: {
  //   baseURL: JSON.stringify("http://localhost:8080/"),
  //   fileURL: JSON.stringify("http://localhost:8080/"),
  //   socketURL: JSON.stringify("https://socket2.soe.vn/"),
  //   SecretKey: JSON.stringify("1012198815021989"),
  //   isDev: true,
  // },
  // define: {
  //   baseURL: JSON.stringify("https://localhost:44393/"),
  //   fileURL: JSON.stringify("https://localhost:44393/"),
  //   socketURL: JSON.stringify("https://socket2.soe.vn/"),
  //   SecretKey: JSON.stringify("1012198815021989"),
  //   isDev: true,
  // },
  // Của BHBQP
  // define: {
  //     baseURL: JSON.stringify("http://172.16.102.211:8080/"),
  //     fileURL: JSON.stringify("http://172.16.102.211:8080/"),
  //     socketURL: JSON.stringify("http://172.16.102.211:3333/"),
  //     SecretKey: JSON.stringify("1012198815021989"),
  //     isDev: true,
  // },
  // define: {s
  //     baseURL: JSON.stringify("http://192.168.100.9:8080/"),
  //     fileURL: JSON.stringify("http://192.168.100.9:8080/"),
  //     socketURL: JSON.stringify("http://192.168.100.9:3333/"),
  //     SecretKey: JSON.stringify("1012198815021989"),
  //     isDev: true,
  // },
  // define: {
  //     baseURL: JSON.stringify("http://api.qldh.bhxh/"),
  //     fileURL: JSON.stringify("http://api.qldh.bhxh/"),
  //     socketURL: JSON.stringify("http://192.168.100.9:3333/"),
  //     SecretKey: JSON.stringify("1012198815021989"),
  //     isDev: true,Tìm
  // },
  define: {
    baseURL: JSON.stringify("https://apidev.soe.vn/"),
    fileURL: JSON.stringify("https://apidev.soe.vn/"),
    socketURL: JSON.stringify("https://socket2.soe.vn/"),
    SecretKey: JSON.stringify("1012198815021989"),
    isDev: true,
  },
  server: {
    host: true,
    //port 3000 localhost
    port: 3000,
  },
  configureWebpack: {
    devtool: "source-map",
  },
});
