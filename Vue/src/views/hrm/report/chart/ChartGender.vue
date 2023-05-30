<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../../util/function.js";
import { useRouter, useRoute } from "vue-router";
import moment from "moment";
import ChartDataLabels from "chartjs-plugin-datalabels";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const router = useRoute();
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
const colors = ref([
  "#CB4335",
  "#FF6600",
  "#FF9E01",
  "#FCD202",
  "#B0DE09",
  "#04D215",
  "#0D8ECF",
  "#0D52D1",
  "#2A0CD0",
  "#8A0CCF",
  "#CD0D74",
  "#754DEB",
  "#DDDDDD",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
  "#999999",
]);

const plugins = [ChartDataLabels];

//Khai báo biến
const types = ref([
  { type: 1, icon: "pi pi-chart-bar", title: "Biểu đồ cột" },
  { type: 2, icon: "pi pi-align-left", title: "Biểu đồ thanh ngang" },
  { type: 3, icon: "pi pi-chart-pie", title: "Biểu đồ tròn" },
  // { type: 4, icon: "pi pi-table", title: "Biểu đồ bảng" },
]);
const store = inject("store");
const options = ref({
  view:1,
  loading: true,
  department_id: store.getters.user.organization_id,
  is_link: null,
  departments: null,
});
const genders = ref(
  [
    { text: "Nam", value: 1 },
    { text: "Nữ", value: 2 }
  ]
)
const isFirst = ref(true);
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const opfilter = ref();
const isfilter = ref(false)
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};
const filter = (event) => {
  opfilter.value.toggle(event);
  isfilter.value = true;
  loadData();
};
const resetFilter = () => {
  options.value.departments = null;
  department_id.value= null;
};
const treedonvis = ref();
const data_lines = ref([
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự theo giới tính" },
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự theo loại nhân sự" },
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự theo chức vụ"},
  { labels: [], datasets: [{ data: [], backgroundColor: [], hoverBackgroundColor: [], },], text:"Biểu đồ nhân sự tham gia Đảng"},
]);
var department_id;
const loadData = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (options.value.departments != null && Object.keys(options.value.departments).length > 0) {
    var dep_ids = [];
    for (var key in options.value.departments) {
      if (options.value.departments[key]) {
        dep_ids.push(key);
      }
    }
    department_id = dep_ids.join(",");
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_report_chart1",
            par: [
            { par: "user_id", va: store.getters.user.user_id },
            { par: "department_id", va: department_id},
            { par: "is_link", va: options.value.is_link},
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
      if (data.length > 0) {
        // data[0].forEach((item) => {
        //   if (item.total) {
        //     item.total_name = item.total.toLocaleString("vi-vN", {
        //       style: "decimal",
        //       minimumFractionDigits: 0,
        //       maximumFractionDigits: 20,
        //     });
        //   }
        // })
        renderTotalName(data[0]);
        renderTotalName(data[1]);
        renderTotalName(data[2]);
        renderTotalName(data[3]);
        renderAcademic(data_lines.value[0], data[0], 0);
        renderAcademic(data_lines.value[1], data[1], 1);
        renderAcademic(data_lines.value[2], data[2], 2);
        renderAcademic(data_lines.value[3], data[3], 3);
        if (data[4].length > 0) {
              let obj = renderTreeDV(
                data[4],
                "organization_id",
                "organization_name",
                "phòng ban"
              );
              treedonvis.value = obj.arrtreeChils;
            }
      }
      else renderAcademic(data_lines.value, []);
      swal.close();
      options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};
const renderAcademic = (chart, data, type) => {
  var temp = data;
  chart.datasets = [];
  var labels = [];
  var arr = [];
  if (temp != null && temp.length > 0) {
    if (type == 0)
      labels = temp.map((item) => item["gender_name"] + " ")
    else if (type == 1)
      labels = temp.map((item) => item["personel_groups_name"] + " ")
    else if (type == 2)
      labels = temp.map((item) => item["position_name"] + " ")
    else 
      labels = temp.map((item) => item["partisan"] + " ")

    arr = temp.map((item) => item["total"]);
  }
  setTimeout(() => {
    //lightOptions.value.plugins.legend.display = true;
    chart.datasets.push({
      label: "",
      data: [],
      backgroundColor: [],
      hoverBackgroundColor: [],
    });
    chart.labels = labels;
    chart.datasets[0].data = arr;
    chart.datasets[0].backgroundColor = colors.value;
    chart.datasets[0].hoverBackgroundColor = colors.value;
  }, 100);
};
const basicOptions = ref({
  plugins: {
    legend: {
      display: false,
      labels: {
        color: "#495057",
      },
    },
    datalabels: {
      formatter: (val) =>
        val.toLocaleString("vi-vN", {
          style: "decimal",
          minimumFractionDigits: 0,
          maximumFractionDigits: 20,
        }),
      anchor: "center",
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
const lightOptions = ref({
  plugins: {
    legend: {
      display: true,
      labels: {
        color: "#495057",
      },
    },
    datalabels: {
      formatter: (val) =>
        val.toLocaleString("vi-vN", {
          style: "decimal",
          minimumFractionDigits: 0,
          maximumFractionDigits: 20,
        }) + " %",
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
const goBack = () => {
  history.back();
};
//Khai báo function
const renderTotalName = (data) => {
  data.forEach((item) => {
    if (item.total) {
      item.total_name = item.total.toLocaleString("vi-vN", {
        style: "decimal",
        minimumFractionDigits: 0,
        maximumFractionDigits: 20,
      });
    }
  })
}
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
onMounted(() => {
  if (router.fullPath != null)
      options.value.is_link = router.fullPath;
  //init
  loadData();
  // initTudien();
});
</script>

<template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <div style="background-color: #fff; padding: 1rem;padding-left: 0;">
      <!-- <div style="height: 36px; display: flex; align-items: center;">
        <Button
            label="Quay lại"
            icon="pi pi-arrow-left"
            class="p-button-outlined mr-2 ml-2 p-button-secondary"
            @click="goBack()"
            />
          <SelectButton
            v-model="options.view"
            :options="types"
            optionValue="type"
            optionLabel="type"
            dataKey="type"
            aria-labelledby="custom"
          >
            <template #option="slotProps">
              <i
                v-if="slotProps.option.icon != ''"
                :class="slotProps.option.icon"
                class="mr-2"
              ></i
              ><span>{{ slotProps.option.title }}</span>
            </template>
          </SelectButton>
      </div> -->
      <Toolbar class="w-full custoolbar">
          <template #start>
            <Button
              @click="toggleFilter($event)"
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              aria:haspopup="true"
              aria-controls="overlay_panel"
            >
              <div>
                <span class="mr-2"><i class="pi pi-filter"></i></span>
                <span class="mr-2">Chọn điều kiện lập báo cáo</span>
                <span><i class="pi pi-chevron-down"></i></span>
              </div>
            </Button>
            <OverlayPanel :showCloseIcon="false" ref="opfilter" appendTo="body" class="p-0 m-0 panel-filter" id="overlay_panel" style="width: 600px; z-index:1000">
              <div class="grid formgrid m-0">
                <div class="col-12 md:col-12 p-0" :style="{
                  minHeight: 'unset',
                  maxheight: 'calc(100vh - 300px)',
                  overflow: 'auto',
                }">
                  <div class="row">
                    <div class="col-12 md:col-12">
                      <div class="form-group">
                        <label>Chọn phòng ban/ Đơn vị</label>
                        <TreeSelect class="col-12 ip36 mt-2 p-0 text-left" style="max-width: calc(600px - 3rem);"  :options="treedonvis"
                          v-model="options.departments"  selectionMode="multiple" :metaKeySelection="false"
                          :showClear="true" :max-height="200" display="chip" placeholder="Chọn phòng ban/ Đơn vị">
                        </TreeSelect>
                      </div>
                    </div>        
                  </div>
                  <div class="col-12 md:col-12 p-0">
                    <Toolbar class="border-none surface-0 outline-none px-0 pb-0 w-full">
                      <template #start>
                        <Button @click="resetFilter()" class="p-button-outlined" label="Bỏ chọn"></Button>
                      </template>
                      <template #end>
                        <Button @click="filter($event)" label="Lọc"></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </div>
            </OverlayPanel>
        </template>

          <template #end>
            <Button
            label="Quay lại"
            icon="pi pi-arrow-left"
            class="p-button-outlined mr-2 p-button-secondary"
            @click="goBack()"
            />
            <SelectButton
            v-model="options.view"
            :options="types"
            optionValue="type"
            optionLabel="type"
            dataKey="type"
            aria-labelledby="custom"
          >
            <template #option="slotProps">
              <i
                v-if="slotProps.option.icon != ''"
                :class="slotProps.option.icon"
                class="mr-2"
              ></i
              ><span>{{ slotProps.option.title }}</span>
            </template>
          </SelectButton>
          </template>
        </Toolbar>
      <div v-if="options.view == 1" class="w-full h-full flex content-body" style="flex-flow:wrap">
        <div class="col-6 md:col-6" v-for="(item, index) in data_lines" :key="index">
          <div class="card m-1">
            <div class="card-header" :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }">
              <div style="text-align: center;" :style="{ fontSize: '15px', fontWeight: 'bold'}">{{ item.text }}</div>
            </div>
            <Chart id="chart32" type="bar" :data="item" :options="basicOptions" :plugins="plugins" class="w-full"
              :style="{
                width: '80% !important',
                display: 'flex',
                alignItems: 'center',
              }" />
          </div>
        </div>
      </div>
      <div v-if="options.view == 2" class="w-full h-full flex content-body" style="flex-flow:wrap">
        <div class="col-6 md:col-6" v-for="(item, index) in data_lines" :key="index">
          <div class="card m-1">
            <div class="card-header" :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }">
              <div style="text-align: center;" :style="{ fontSize: '15px', fontWeight: 'bold'}">{{ item.text }}</div>
            </div>
            <Chart id="chart32" type="bar" :data="item" :options="horizontalOptions" :plugins="plugins" class="w-full"
              :style="{
                width: '80% !important',
                display: 'flex',
                alignItems: 'center',
              }" />
          </div>
        </div>
      </div>
      <div v-if="options.view == 3" class="w-full h-full flex content-body" style="flex-flow:wrap">
        <div class="col-6 md:col-6" v-for="(item, index) in data_lines" :key="index">
          <div class="card m-1">
            <div class="card-header" :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }">
              <div style="text-align: center;" :style="{ fontSize: '15px', fontWeight: 'bold'}">{{ item.text }}</div>
            </div>
            <div class="format-center w-full">
              <Chart id="chart32" type="doughnut" :data="item" :options="lightOptions" :plugins="plugins"
              :style="{
                width: '40% !important',
                display: 'flex',
                alignItems: 'center',
              }" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
  .content-body{
    max-height: calc(100vh - 118px);
    min-height: calc(100vh - 118px);
    overflow:auto;
  }
</style>
<style lang="scss" scoped>
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label, .p-treeselect .p-multiselect-label {
    height: 100%;
    display: flex;
    align-items: center;
    padding:0px 0.5rem !important
  }
}
::v-deep(.p-treeselect) {
   .p-multiselect-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-treeselect-label,.p-treeselect-token{
    height: 100%;
  }
}
</style>
