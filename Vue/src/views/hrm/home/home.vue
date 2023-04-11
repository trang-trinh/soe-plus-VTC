<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
import ChartDataLabels from "chartjs-plugin-datalabels";

const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const base_url = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;
const plugins = [ChartDataLabels];

//Declare
const bgColor = ref([
  "#0078d4",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#B0DE09",
]);
const bgColors = ref([
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
const genderColor = ref(["#EE7E79", "#83ECC6", "#84B7F9", "#F5CD7C"]);
const options = ref([]);
const organizations = ref([]);
const yearOlds = ref({
  labels: [],
  datasets: [
    {
      data: [],
      backgroundColor: [],
      hoverBackgroundColor: [],
    },
  ],
});
const genders = ref([]);
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
const notes = ref([]);
const renderGender = (chart, data) => {
  var temp = data;
  chart.datasets = [];
  var labels = [];
  var arr = [];
  if (temp != null && temp.length > 0) {
    labels = temp.map((item) => item["title"] + " ");
    arr = temp.map((item) => item["avg"]);
  }
  setTimeout(() => {
    lightOptions.value.plugins.legend.display = true;
    chart.datasets.push({
      label: "",
      data: [],
      backgroundColor: [],
      hoverBackgroundColor: [],
    });
    chart.labels = labels;
    chart.datasets[0].data = arr;
    chart.datasets[0].backgroundColor = genderColor.value;
    chart.datasets[0].hoverBackgroundColor = genderColor.value;
  }, 100);
};
const animateNumber = (
  finalNumber,
  duration = 5000,
  startNumber = 0,
  callback
) => {
  let currentNumber = startNumber;
  const interval = window.setInterval(updateNumber, 17);
  function updateNumber() {
    if (currentNumber >= finalNumber) {
      clearInterval(interval);
    } else {
      let inc = Math.ceil(finalNumber / (duration / 17));
      if (currentNumber + inc > finalNumber) {
        currentNumber = finalNumber;
        clearInterval(interval);
      } else {
        currentNumber += inc;
      }
      callback(currentNumber);
    }
  }
};

//init
const initGender = () => {
  yearOlds.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_home_genders",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            tbs[0].forEach((item) => {
              if (item.total) {
                item.total_name = item.total.toLocaleString("vi-vN", {
                  style: "decimal",
                  minimumFractionDigits: 0,
                  maximumFractionDigits: 20,
                });
              }
            });
            renderGender(yearOlds.value, tbs[0]);
          } else {
            renderGender(yearOlds.value, []);
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            tbs[1].forEach((item) => {
              if (item.total) {
                item.total_name = item.total.toLocaleString("vi-vN", {
                  style: "decimal",
                  minimumFractionDigits: 0,
                  maximumFractionDigits: 20,
                });
              }
            });
            genders.value = tbs[1].reverse();
          }
        }
      }
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
const initOrganization = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_home_organizations",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            organizations.value = tbs[0];

            tbs[0].forEach((item, index) => {
              animateNumber(item.total, 3000, 0, function (number) {
                const formattedNumber = number.toLocaleString("vi-vN", {
                  style: "decimal",
                  minimumFractionDigits: 0,
                  maximumFractionDigits: 20,
                });
                document.getElementById("counter" + index).innerText =
                  formattedNumber;
              });
            });
          } else {
            organizations.value = [];
          }
        }
      }
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
const initNote = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_home_notes",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            notes.value = tbs[0];
            tbs[0].forEach((item) => {
              if (item.members != null) {
                item.members = JSON.parse(item.members);
                item.members.forEach((mb) => {
                  if (!mb.position_name) {
                    mb.position_name = "";
                  } else {
                    mb.position_name =
                      " </br> <span class='text-sm'>" +
                      mb.position_name +
                      "</span>";
                  }
                  if (!mb.department_name) {
                    mb.department_name = "";
                  } else {
                    mb.department_name =
                      " </br> <span class='text-sm'>" +
                      mb.department_name +
                      "</span>";
                  }
                });
              }
            });
          } else {
            notes.value = [];
          }
        }
      }
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
onMounted(() => {
  initOrganization();
  initGender();
  initNote();
});
</script>
<template>
  <div class="surface-100 dashboard">
    <div class="d-grid formgrid m-1">
      <div class="col-12 md:col-12 test">
        <template v-for="(item, index) in organizations">
          <div
            class="zoom"
            :style="{
              backgroundColor: bgColor[index],
              color: '#fff',
              height: '80px !important',
            }"
            @click="goRouter('calendarenact')"
          >
            <div
              class="card-body h-full"
              :style="{ height: '80px !important' }"
            >
              <div class="format-grid-center h-full">
                <h4 class="m-0">{{ item.short_name }}</h4>
                <h1
                  class="my-2"
                  :style="{ wordBreak: 'break-all', fontSize: '30px' }"
                >
                  <span :id="'counter' + index">{{ item.total }}</span>
                </h1>
              </div>
            </div>
          </div>
        </template>
      </div>
      <div class="col-8 md:col-8">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 415px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>Thống kê nhân sự theo độ tuổi</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 360px">
            <div
              v-show="
                !options.loading &&
                yearOlds.datasets != null &&
                yearOlds.datasets[0] != null &&
                yearOlds.datasets[0].data.length > 0
              "
              class="w-full h-full format-center"
            >
              <Chart
                id="chart1"
                type="doughnut"
                :data="yearOlds"
                :options="lightOptions"
                :plugins="plugins"
                class="w-full"
                :style="{ width: '80% !important' }"
              />
            </div>
            <div
              v-show="
                !options.loading &&
                yearOlds.datasets != null &&
                yearOlds.datasets[0] != null &&
                yearOlds.datasets[0].data.length === 0
              "
              class="w-full h-full format-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
          <div class="format-center justify-content-between p-3">
            <template v-for="(item, index) in genders">
              <div>
                <h1 class="m-0">
                  <span class="description">{{ item.title }} :</span> <span>{{ item.total_name }}</span>
                </h1>
              </div>
            </template>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>Hệ thống cảnh báo</span>
          </div>
          <div
            class="d-lang-table card-body carousel-hidden-p-link p-0"
            style="height: 400px"
          >
            <DataTable
              :value="notes"
              :scrollable="true"
              :lazy="true"
              :rowHover="true"
              :showGridlines="false"
              dataKey="receipt_id"
              scrollHeight="flex"
              filterDisplay="menu"
              filterMode="lenient"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              responsiveLayout="scroll"
              class="border-none padding-new"
            >
              <Column
                field="note_name"
                header="Nội dung"
                headerStyle="max-width:auto;"
              >
              </Column>
              <Column
                field="members"
                header="Nhân sự"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="flex" :style="{ justifyContent: 'right' }">
                    <AvatarGroup
                      v-if="
                        slotProps.data.members &&
                        slotProps.data.members.length > 0
                      "
                    >
                      <Avatar
                        v-for="(item, index) in slotProps.data.members.slice(
                          0,
                          3
                        )"
                        v-bind:label="
                          item.avatar
                            ? ''
                            : item.profile_user_name.substring(0, 1)
                        "
                        v-bind:image="
                          item.avatar
                            ? basedomainURL + item.avatar
                            : basedomainURL + '/Portals/Image/noimg.jpg'
                        "
                        v-tooltip.top="{
                          value:
                            item.profile_user_name +
                            item.position_name +
                            item.department_name,
                          escape: true,
                        }"
                        :key="item.user_id"
                        @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                        size="large"
                        shape="circle"
                        class="cursor-pointer"
                        :style="{
                          backgroundColor: bgColors[index % 7],
                          color: 'white',
                          width: '2.3rem',
                          height: '2.3rem',
                          fontSize: '10px !important',
                        }"
                      />
                      <Avatar
                        v-if="
                          slotProps.data.members &&
                          slotProps.data.members.length > 3
                        "
                        v-bind:label="
                          '+' + (slotProps.data.members.length - 3).toString()
                        "
                        shape="circle"
                        size="large"
                        :style="{
                          backgroundColor: '#2196f3',
                          color: '#ffffff',
                        }"
                        class="cursor-pointer"
                      />
                    </AvatarGroup>
                  </div>
                </template>
              </Column>
            </DataTable>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 400px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 400px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
@import url(../../calendar/component/stylecalendar.css);
.test {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  grid-auto-rows: minmax(80px, auto);
  column-gap: 15px;
  row-gap: 15px;
}
.dashboard {
  overflow: auto;
  max-height: calc(100vh - 50px);
}
.d-grid {
  display: flex;
  flex-wrap: wrap;
}
.d-lang-table {
  height: calc(100vh - 190px);
  overflow-y: auto;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.format-grid-center {
  display: grid;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.form-group {
  display: grid;
  margin-bottom: 1rem;
}
.form-group > label {
  margin-bottom: 0.5rem;
}
.ip36 {
  width: 100%;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}
.p-lichip {
  float: left;
}
.p-multiselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}

.dashboard .card {
  border: none;
  border-radius: 0;
  position: relative;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
  -ms-flex-direction: column;
  flex-direction: column;
  min-width: 0;
  word-wrap: break-word;
  background-color: #fff;
  background-clip: border-box;
}
.dashboard .card-header {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
  border-bottom: solid 1px rgba(0, 0, 0, 0.1);
  font-size: 15px;
  font-weight: bold;
  color: #005a9e;
}
.dashboard .card-body {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
}
.zoom {
  cursor: pointer;
  border-radius: 0.25rem;
  /* box-shadow: 0 2px 4px rgb(0 0 0 / 23%); */
  transition: transform 0.3s !important;
}
.zoom:hover {
  transform: scale(0.9) !important;
  /* box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important; */
  cursor: pointer !important;
}
.grid-item {
  display: grid;
}
.flex-item {
  display: flex;
}
.limit-line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.description {
  color: #aaa;
  font-size: 12px;
}
.carousel-item {
  cursor: pointer;
  padding-bottom: 0.5rem !important;
}
.row-item {
  cursor: pointer;
  padding: 0.5rem !important;
}
.carousel-item:hover,
.row-item:hover {
  background-color: aliceblue;
}
span.online {
  position: absolute;
  display: block;
  width: 14px;
  height: 14px;
  background-color: rgb(98, 203, 0);
  border-radius: 50%;
  right: 0;
  bottom: 0;
  border: 2px solid #fff;
}
.scroll-outer {
  visibility: hidden;
}
.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-left .p-column-header-content {
    justify-content: left !important;
  }
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }
  .p-datatable-thead .justify-content-right .p-column-header-content {
    justify-content: right !important;
  }
}
::v-deep(.border-none) {
  .p-datatable-table tr th,
  .p-datatable-table tr td {
    border: none;
  }
}
::v-deep(.padding-new) {
  .p-datatable-table tr td {
    padding: 0.5rem 1rem !important;
  }
}
</style>
