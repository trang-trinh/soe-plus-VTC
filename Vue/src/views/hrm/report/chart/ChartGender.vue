<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../../util/function.js";
import moment from "moment";
import ChartDataLabels from "chartjs-plugin-datalabels";


const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");

//color
const bgColor = ref([
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
    "#B0DE09",
]);
const plugins = [ChartDataLabels];

//Khai báo biến
const store = inject("store");
var data_org = [];
const department_name = ref();
department_name.value = store.getters.user.organization_name;
const datalists = ref();
const router = inject("router");
const options = ref({
    IsNext: true,
    sort: "created_date",
    SearchText: null,
    PageNo: 0,
    PageSize: 20,
    loading: true,
    totalRecords: null,
    loadingP: true,
    pagenoP: 0,
    pagesizeP: 20,
    searchP: "",
    sortP: "created_date",
    department_id: store.getters.user.organization_id,
});
const genders = ref(
    [
        { text: "Nam", value: 1 },
        { text: "Nữ", value: 2 }
    ]
)
const tudiens = ref();
const isFirst = ref(true);
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
const first = ref(0);
const selectCapcha = ref();
selectCapcha.value = {};


const loadData = () => {
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    axios
        .post(
            baseURL + "/api/hrm/callProc",
            {
                str: encr(
                    JSON.stringify({
                        proc: "hrm_report_profile_organization_list1",
                        par: [
                            { par: "search", va: options.value.SearchText },
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "department_id", va: options.value.department_id },
                            { par: "gender", va: options.value.gender },
                            { par: "title_id", va: options.value.title_id },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if (data[0].length > 0) {
                data[0].forEach((item, index) => {
                    item.is_active = false;
                    const startDate = moment(item.recruitment_date || new Date());
                    const endDate = moment(new Date());
                    item.duration = moment.duration(endDate.diff(startDate));
                    item.diffyear = item.duration.years();
                    item.diffmonth = item.duration.months();
                });
                data[0] = groupBy(data[0], 'department_id');
                let arr = [];
                var count = 1;
                for (let pb in data[0]) {
                    // let data_ns_by_id = groupBy(data[0][pb], 'profile_code');
                    // let arr_ns = [];
                    // for (let ns in data_ns_by_id) {
                    //     arr_ns.push({ group_ns: ns, name_group_ns: data_ns_by_id[ns][0].profile_user_name, list_con: data_ns_by_id[ns] });
                    // };
                    // data_ns_by_id = arr_ns;
                    data[0][pb].forEach((item) => {
                        item.stt = count;
                        count++
                    })
                    arr.push({ group_pb: pb, name_group_pb: data[0][pb][0].organization_name, list_ns: data[0][pb] });
                }
                datalists.value = arr;
                options.totalRecords = arr.length;
            }
            else datalists.value = [];
            swal.close();
            options.value.loading = false;
        })
        .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
        });
};


const treedonvis = ref();
const initTudien = () => {
    axios
        .post(
            baseURL + "/api/hrm/callProc",
            {
                str: encr(JSON.stringify({
                    proc: "hrm_report_dictionary",
                    par: [{ par: "user_id", va: store.getters.user.user_id }],
                }), SecretKey, cryoptojs
                ).toString()
            },
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if (data[0].length > 0) {
                data_org = data[0];
                let obj = renderTreeDV(
                    data[0],
                    "organization_id",
                    "organization_name",
                    "phòng ban"
                );
                treedonvis.value = obj.arrtreeChils;
            }
            tudiens.value = data;
        })
        .catch((error) => { });
};
const datas = ref({
    labels: [],
    datasets: [
        {
            data: [],
            backgroundColor: [],
            hoverBackgroundColor: [],
        },
    ],
});
const tables = ref([]);
const lightOptions = ref({
    plugins: {
        legend: {
            display: true,
            labels: {
                color: "#495057",
            },
        },
        datalabels: {
            formatter: (val) => val,
            anchor: "center",
            align: "center",
            color: "white",
            labels: {
                title: {
                    font: {
                        //weight: "bold",
                        //size: 48,
                    },
                },
                value: {
                    color: "white",
                    font: {
                        //weight: "bold",
                        //size: 48,
                    },
                },
            },
        },
    },
});
const basicOptions = ref({
    plugins: {
        legend: {
            display: false,
            labels: {
                color: "#495057",
            },
        },
        datalabels: {
            formatter: (val) => val,
            anchor: "end",
            align: "end",
            color: "black",
            labels: {
                title: {
                    font: {
                        //weight: "bold",
                        //size: 48,
                    },
                },
                value: {
                    color: "black",
                    font: {
                        //weight: "bold",
                        //size: 48,
                    },
                },
            },
        },
    },
    scales: {
        x: {
            ticks: {
                color: "#495057",
            },
            grid: {
                color: "#ebedef",
            },
        },
        y: {
            ticks: {
                color: "#495057",
            },
            grid: {
                color: "#ebedef",
            },
        },
    },
});
const horizontalOptions = ref({
    indexAxis: "y",
    plugins: {
        legend: {
            display: false,
            labels: {
                color: "#495057",
            },
        },
        datalabels: {
            formatter: (val) => val,
            anchor: "end",
            align: "end",
            color: "black",
            labels: {
                title: {
                    font: {
                        //weight: "bold",
                        //size: 48,
                    },
                },
                value: {
                    color: "black",
                    font: {
                        //weight: "bold",
                        //size: 48,
                    },
                },
            },
        },
    },
    scales: {
        x: {
            ticks: {
                color: "#495057",
            },
            grid: {
                color: "#ebedef",
            },
        },
        y: {
            ticks: {
                color: "#495057",
            },
            grid: {
                color: "#ebedef",
            },
        },
    },
});

//Khai báo function
const renderTreeDV = (data, id, name, title) => {
    let arrChils = [];
    let arrtreeChils = [];
    data
        .filter((x) => x.parent_id == null)
        .forEach((m, i) => {
            m.IsOrder = i + 1;
            let om = { key: m[id], data: m };
            const rechildren = (mm, pid) => {
                let dts = data.filter((x) => x.parent_id == pid);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em) => {
                        let om1 = { key: em[id], data: em };
                        rechildren(om1, em[id]);
                        mm.children.push(om1);
                    });
                }
            };
            rechildren(om, m[id]);
            arrChils.push(om);
            //
            om = { key: m[id], data: m[id], label: m[name] };
            const retreechildren = (mm, pid) => {
                let dts = data.filter((x) => x.parent_id == pid);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em) => {
                        let om1 = { key: em[id], data: em[id], label: em[name] };
                        retreechildren(om1, em[id]);
                        mm.children.push(om1);
                    });
                }
            };
            retreechildren(om, m[id]);
            arrtreeChils.push(om);
        });
    return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
function groupBy(list, props) {
    return list.reduce((a, b) => {
        (a[b[props]] = a[b[props]] || []).push(b);
        return a;
    }, {});
}
onMounted(() => {
    //init
    loadData();
    initTudien();
});
</script>

<template>
    <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
        <div style="background-color: #fff; padding: 1rem;padding-left: 0;">
            <h3 class="module-title module-title-hidden mt-0 ml-3 mb-2">
                <i class="pi pi-chart-bar"></i> Biểu đồ theo giới tính
            </h3>
            <div class="w-full h-full format-center">
                <Chart id="chart1" type="bar" :data="datas" :options="basicOptions" :plugins="plugins" style="
                    width: calc(100vh - 0px) !important;
                    height: 40% !important;
                    vertical-align: middle; 
                    align-items: center;
                    display: flex;
              " />
            </div>
        </div>
    </div>
</template>
<style lang="scss" scoped></style>
