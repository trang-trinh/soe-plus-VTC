import { createStore } from "vuex";
import { useCookies } from "vue3-cookies";
const { cookies } = useCookies();
export const store = createStore({
    state: {
        islogin: false,
        darkTheme: localStorage.getItem("darkTheme") == "true",
        background: localStorage.getItem("background") ||
            "https://primefaces.org/primevue/img/window.6b46439b.jpg",
        user: {},
        token: null,
        news: {},
        langs: [],
        birthDay: {},
        userConnected: [],
        listModule: [],

    },
    getters: {
        islogin: (state) => state.islogin,
        darkTheme: (state) => state.darkTheme,
        user: (state) => state.user,
        token: (state) => state.token,
        background: (state) => state.background,
        news: (state) => state.news,
        langs: (state) => state.langs,
        birthDay_id: (state) => state.birthDay,
        userConnected: (state) => state.userConnected,
        listModule: (state) => state.listModule,

    },
    mutations: {
        setislogin(state, vl) {
            state.islogin = vl;
        },
        setdarkTheme(state, vl) {
            state.darkTheme = vl;
            localStorage.setItem("darkTheme", vl);
        },
        setbackground(state, vl) {
            state.background = vl;
            localStorage.setItem("background", vl);
        },
        setuser(state, vl) {
            state.user = vl;
        },
        setnews(state, vl) {
            state.news = vl;
        },
        settoken(state, vl) {
            state.token = vl;
        },
        gologout(state, router) {
            //localStorage.removeItem("tk");
            cookies.remove("tk");
            state.token = null;
            state.islogin = false;
            //state.user = null;
        },
        setlang(state, vl) {
            state.lang = vl;
        },
        setlangs(state, vl) {
            state.langs = vl;
        },
        setbirthDay_id(state, vl) {
            state.birthDay = vl;
        },
        setuserConnected(state, vl) {
            state.userConnected = vl;
        },
        setlistModule(state, vl) {
            state.listModule = vl;
        },


    },
});