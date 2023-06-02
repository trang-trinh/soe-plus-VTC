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
const addToArray = (temp, array, id, lv, od) => {
  var filter = array.filter((x) => x.parent_id === id);
  filter = filter.sort((a, b) => {
    return b[od] - a[od];
  });
  if (filter.length > 0) {
    var sp = "";
    for (var i = 0; i < lv; i++) {
      sp += "---";
    }
    lv++;
    filter.forEach((item) => {
      item.lv = lv;
      item.close = true;
      if (!item.ids) {
        item.ids = "";
        item.ids += "," + item.organization_id;
      }
      if (!item.newname) item.newname = sp + item.organization_name;
      temp.push(item);
      addToArray(temp, array, item.organization_id, lv);
    });
  }
};

//Declare
const options = ref({
  loading: true,
  filter_organization_id: store.getters.user.organization_id,
  user_filter_organization_id: store.getters.user.organization_id,
  user_filter_month_id: 1,
});
const datanewprofiles = ref([]);
const databirthdays = ref([]);
const dataphonebooks = ref([]);
const organizations = ref([]);
const dictionarys = ref([]);
const bgColor = ref([
  "#FF6633",
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
const selectedNodes = ref({});

// Total
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

// Gender
const genderColor = ref(["#EE7E79", "#83ECC6", "#84B7F9", "#F5CD7C"]);
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

//academic level
const academics = ref({
  labels: [],
  datasets: [
    {
      data: [],
      backgroundColor: [],
      hoverBackgroundColor: [],
    },
  ],
});
const renderAcademic = (chart, data) => {
  var temp = data;
  chart.datasets = [];
  var labels = [];
  var arr = [];
  if (temp != null && temp.length > 0) {
    labels = temp.map((item) => item["academic_level_name"] + " ");
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
const changeOrganization = () => {
  initAcademicLevel(true);
};
const goRouter = (name, params) => {
  if (name != null) {
    router.push({ name: name, params: params || {} });
  }
};

const opfilter = ref();
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};

const opfilter2 = ref();
const toggleFilter2 = (event) => {
  opfilter2.value.toggle(event);
};

const changeNewProfile = () => {
  initNewProfile(true);
};

const goProfile = (profile) => {
  router.push({
    name: "profileinfo",
    params: { id: profile.key_id },
    query: { id: profile.profile_id },
  });
};

//init
const initAcademicLevel = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  academics.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_home_academic_level",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              {
                par: "filter_organization_id",
                va: options.value.filter_organization_id,
              },
            ],
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
            renderAcademic(academics.value, tbs[0]);
          } else {
            renderAcademic(academics.value, []);
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
const initGender = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
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
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
const initOrganization = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
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
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
const initNote = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
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
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
const initNewProfile = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  datanewprofiles.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_home_profile_new",
            par: [
              {
                par: "user_filter_organization_id",
                va: options.value.user_filter_organization_id,
              },
              {
                par: "user_filter_month_id",
                va: options.value.user_filter_month_id,
              },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item, i) => {
              if (item["recruitment_date"] != null) {
                item["recruitment_date"] = moment(
                  new Date(item["recruitment_date"])
                ).format("DD/MM/YYYY");
              }
            });
            datanewprofiles.value = tbn[0];
          }
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
    });
};
const initBirthday = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "dashboard_birthday",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "myDate", va: new Date() },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbn = JSON.parse(data);
          if (tbn[0] != null && tbn[0].length > 0) {
            tbn[0].forEach((item, i) => {
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY"
                );
              }
            });
            datatodaybirthdays.value = tbn[0];
          }
          if (tbn[1] != null && tbn[1].length > 0) {
            tbn[1].forEach((item, i) => {
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY"
                );
              }
            });
            databirthdays.value = tbn[1];
          }
          if (tbn[3] != null && tbn[3].length > 0) {
            tbn[3].forEach((item, i) => {
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY"
                );
              }
            });
            dataphonebooks.value = tbn[3];
          }
        }
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const initDictionary = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_home_dictionary",
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
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 1) {
            var temp1 = [];
            addToArray(temp1, tbs[0], null, 0, "is_order");
            tbs[0] = temp1;
          } else if (tbs[0] != null && tbs[0].length > 0) {
            tbs[0][0].newname = tbs[0][0].organization_name;
          }
          //tbs[0].unshift({ organization_id: -1, newname: "Tất cả" });
          dictionarys.value = tbs;
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
onMounted(() => {
  initOrganization();
  initAcademicLevel();
  initGender();
  initNote();
  initNewProfile();
  initBirthday();
  initDictionary();
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
          <div
            class="card-header"
            :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }"
          >
            <Toolbar class="outline-none surface-0 border-none p-0">
              <template #start
                ><span :style="{ fontSize: '15px', fontWeight: 'bold' }"
                  >Thống kê nhân sự theo trình độ học vấn</span
                ></template
              >
              <template #end>
                <div class="ip36">
                  <Button
                    v-if="dictionarys[0] && dictionarys[0].length > 1"
                    @click="toggleFilter($event)"
                    type="button"
                    class="p-button-outlined p-button-secondary ip36"
                    aria:haspopup="true"
                    aria-controls="overlay_panel"
                    v-tooltip.top="'Lọc dữ liệu'"
                  >
                    <div>
                      <span><i class="pi pi-filter"></i></span>
                    </div>
                  </Button>
                  <OverlayPanel
                    :showCloseIcon="false"
                    ref="opfilter"
                    appendTo="body"
                    class="p-0 m-0"
                    id="overlay_panel"
                    style="width: 400px"
                  >
                    <div class="grid formgrid m-0">
                      <div
                        class="col-12 md:col-12 p-0"
                        :style="{
                          minHeight: 'unset',
                          maxheight: 'calc(100vh - 300px)',
                          overflow: 'auto',
                        }"
                      >
                        <div class="row">
                          <div class="col-12 md:col-12 p-0">
                            <div class="form-group">
                              <label>Đơn vị</label>
                              <Dropdown
                                :options="dictionarys[0]"
                                :filter="true"
                                :showClear="false"
                                :editable="false"
                                v-model="options.filter_organization_id"
                                @change="changeOrganization()"
                                optionLabel="newname"
                                optionValue="organization_id"
                                placeholder="Chọn đơn vị"
                                class="ip36"
                                :style="{
                                  whiteSpace: 'nowrap',
                                  overflow: 'hidden',
                                  textOverflow: 'ellipsis',
                                }"
                              />
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </OverlayPanel>
                </div>
              </template>
            </Toolbar>
          </div>
          <div
            class="card-body carousel-hidden-p-link"
            :style="{ height: '433px' }"
          >
            <div
              v-show="
                !options.loading &&
                academics.datasets != null &&
                academics.datasets[0] != null &&
                academics.datasets[0].data.length > 0
              "
              class="w-full h-full format-center"
            >
              <Chart
                id="chart32"
                type="bar"
                :data="academics"
                :options="basicOptions"
                :plugins="plugins"
                :style="{
                  width: '100%',
                  height: '100%',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }"
              />
            </div>
            <div
              v-show="
                !options.loading &&
                academics.datasets != null &&
                academics.datasets[0] != null &&
                academics.datasets[0].data.length === 0
              "
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
          <div class="card-body carousel-hidden-p-link" style="height: 378px">
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
                :style="{
                  width: '85%',
                  //height: '100%',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }"
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
                  <span class="description">{{ item.title }} :</span>
                  <span>{{ item.total_name }}</span>
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
              v-if="notes && notes.length > 0"
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
              v-else-if="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div
            class="card-header"
            :style="{ cursor: 'pointer', padding: '4px 4px 4px 1rem' }"
          >
            <Toolbar class="outline-none surface-0 border-none p-0">
              <template #start
                ><span :style="{ fontSize: '15px', fontWeight: 'bold' }"
                  >Nhân sự mới</span
                ></template
              >
              <template #end>
                <Button
                  @click="toggleFilter2($event)"
                  type="button"
                  class="p-button-outlined p-button-secondary ip36"
                  aria:haspopup="true"
                  aria-controls="overlay_panel2"
                  v-tooltip.top="'Lọc dữ liệu'"
                >
                  <div>
                    <span><i class="pi pi-filter"></i></span>
                  </div>
                </Button>
                <OverlayPanel
                  :showCloseIcon="false"
                  ref="opfilter2"
                  appendTo="body"
                  class="p-0 m-0"
                  id="overlay_panel2"
                  style="width: 400px"
                >
                  <div class="grid formgrid m-0">
                    <div
                      class="col-12 md:col-12 p-0"
                      :style="{
                        minHeight: 'unset',
                        maxheight: 'calc(100vh - 300px)',
                        overflow: 'auto',
                      }"
                    >
                      <div class="row">
                        <div class="col-12 md:col-12 p-0">
                          <div class="form-group">
                            <label>Đơn vị</label>
                            <Dropdown
                              :options="dictionarys[0]"
                              :filter="true"
                              :showClear="false"
                              :editable="false"
                              v-model="options.user_filter_organization_id"
                              @change="changeNewProfile()"
                              optionLabel="newname"
                              optionValue="organization_id"
                              placeholder="Chọn đơn vị"
                              class="ip36"
                              :style="{
                                whiteSpace: 'nowrap',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                              }"
                            />
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="form-group">
                            <label>Thời gian</label>
                            <Dropdown
                              :options="[
                                { value: 1, title: 'Tháng hiện tại' },
                                { value: 2, title: '1 Tháng trước' },
                                { value: 3, title: '2 Tháng trước' },
                              ]"
                              :filter="true"
                              :showClear="false"
                              :editable="false"
                              v-model="options.user_filter_month_id"
                              @change="changeNewProfile()"
                              optionLabel="title"
                              optionValue="value"
                              placeholder="Chọn thời gian"
                              class="ip36"
                              :style="{
                                whiteSpace: 'nowrap',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                              }"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </OverlayPanel>
              </template>
            </Toolbar>
          </div>
          <div
            class="d-lang-table card-body carousel-hidden-p-link p-0"
            style="height: 400px"
          >
            <DataTable
              v-if="datanewprofiles && datanewprofiles.length > 0"
              :value="datanewprofiles"
              :scrollable="true"
              :lazy="true"
              :rowHover="true"
              :showGridlines="false"
              dataKey="profile_id"
              scrollHeight="flex"
              filterDisplay="menu"
              filterMode="lenient"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              responsiveLayout="scroll"
              class="border-none padding-new header-none"
            >
              <Column field="note_name" header="" headerStyle="max-width:auto;">
                <template #body="slotProps">
                  <div class="flex">
                    <div class="mr-3">
                      <Avatar
                        v-bind:label="
                          slotProps.data.avatar
                            ? ''
                            : slotProps.data.profile_last_name.substring(0, 1)
                        "
                        v-bind:image="
                          slotProps.data.avatar
                            ? basedomainURL + slotProps.data.avatar
                            : basedomainURL + '/Portals/Image/noimg.jpg'
                        "
                        :key="slotProps.data.profile_id"
                        @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                        size="large"
                        shape="circle"
                        class="cursor-pointer"
                        :style="{
                          backgroundColor:
                            bgColors[slotProps.data.is_order % 7],
                          color: 'white',
                          width: '4rem',
                          height: '4rem',
                          fontSize: '1.3rem !important',
                        }"
                      />
                    </div>
                    <div class="format-center text-left">
                      <div>
                        <div class="mb-2">
                          <b>{{ slotProps.data.profile_user_name }}</b>
                        </div>
                        <div>{{ slotProps.data.department_name }}</div>
                        <div>
                          {{ slotProps.data.phone }}
                          <span
                            v-if="slotProps.data.phone && slotProps.data.email"
                            >|</span
                          >
                          {{ slotProps.data.email }}
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
              </Column>
              <Column
                field="note_name"
                header="Ngày vào"
                headerStyle="text-align:center;max-width:90px;width:90px;height:50px"
                bodyStyle="text-align:center;max-width:90px;width:90px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <span class="description">{{
                    slotProps.data.recruitment_date
                  }}</span>
                </template>
              </Column>
            </DataTable>
            <div
              v-else-if="datanewprofiles == null || datanewprofiles.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div
          class="card m-1 mb-3"
          @click="goRouter('birthday')"
          style="cursor: pointer"
        >
          <div class="card-header">
            <span>Sinh nhật trong tháng</span>
          </div>
          <div class="card-body" style="height: 80px">
            <div class="d-grid formgrid">
              <div class="col-3 md:col-3 p-0">
                <div class="format-grid-center">
                  <div
                    :style="{
                      width: '70px',
                      position: 'absolute',
                      top: '60%',
                      left: '15%',
                      transform: 'translate(-50%, -50%)',
                    }"
                  >
                    <img
                      :src="basedomainURL + '/Portals/birthday.png?v=1'"
                      style="
                        width: 100%;
                        height: 100%;
                        object-fit: contain;
                        border-radius: 3px;
                        transform: scale(2);
                      "
                    />
                  </div>
                </div>
              </div>
              <div class="col-9 md:col-9 p-0">
                <div class="d-grid formgrid">
                  <div class="col-12 md:col-12 p-0 pb-2 text-center">
                    <span
                      v-if="datatodaybirthdays && datatodaybirthdays.length > 0"
                      >Sinh nhật hôm nay</span
                    >
                    <span v-else>Sinh nhật sắp tới</span>
                  </div>
                  <div
                    v-if="datatodaybirthdays && datatodaybirthdays.length > 0"
                    class="col-12 md:col-12 p-0"
                  >
                    <div class="flex justify-content-center">
                      <AvatarGroup
                        v-if="
                          datatodaybirthdays && datatodaybirthdays.length > 0
                        "
                      >
                        <Avatar
                          v-for="(item, index) in datatodaybirthdays.slice(
                            0,
                            3
                          )"
                          v-bind:label="
                            item.avatar ? '' : item.last_name.substring(0, 1)
                          "
                          v-bind:image="
                            item.avatar
                              ? basedomainURL + item.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          v-tooltip.top="item.full_name"
                          :key="item.user_id"
                          style="border: 2px solid white; color: white"
                          @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                          size="large"
                          shape="circle"
                          class="cursor-pointer"
                          :style="{ backgroundColor: bgColor[index % 7] }"
                        />
                        <Avatar
                          v-if="
                            datatodaybirthdays && datatodaybirthdays.length > 3
                          "
                          v-bind:label="
                            '+' + (datatodaybirthdays.length - 3).toString()
                          "
                          shape="circle"
                          size="large"
                          style="background-color: #2196f3; color: #ffffff"
                          class="cursor-pointer"
                        />
                      </AvatarGroup>
                    </div>
                  </div>
                  <div v-else class="col-12 md:col-12 p-0">
                    <div class="flex justify-content-center">
                      <AvatarGroup
                        v-if="databirthdays && databirthdays.length > 0"
                      >
                        <Avatar
                          v-for="(item, index) in databirthdays.slice(0, 3)"
                          v-bind:label="
                            item.avatar ? '' : item.last_name.substring(0, 1)
                          "
                          v-bind:image="
                            item.avatar
                              ? basedomainURL + item.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          v-tooltip.top="item.full_name"
                          :key="item.user_id"
                          style="border: 2px solid white; color: white"
                          @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                          size="large"
                          shape="circle"
                          class="cursor-pointer"
                          :style="{ backgroundColor: bgColor[index % 7] }"
                        />
                        <Avatar
                          v-if="databirthdays && databirthdays.length > 3"
                          v-bind:label="
                            '+' + (databirthdays.length - 3).toString()
                          "
                          shape="circle"
                          size="large"
                          style="background-color: #2196f3; color: #ffffff"
                          class="cursor-pointer"
                        />
                      </AvatarGroup>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div
          class="card m-1 mb-3"
          @click="goRouter('hrm_contact')"
          style="cursor: pointer"
        >
          <div class="card-header">
            <span>Danh bạ</span>
          </div>
          <div class="card-body" style="height: 80px">
            <div class="d-grid formgrid">
              <div class="col-3 md:col-3 p-0">
                <div class="format-grid-center">
                  <div
                    :style="{
                      width: '70px',
                      position: 'absolute',
                      top: '70%',
                      left: '15%',
                      transform: 'translate(-50%, -50%)',
                    }"
                  >
                    <img
                      :src="basedomainURL + '/Portals/phonebook.png'"
                      style="
                        width: 100%;
                        height: 100%;
                        object-fit: contain;
                        border-radius: 3px;
                        transform: scale(1.5);
                      "
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div
          class="card m-1"
          @click="goRouter('hrm_contact')"
          style="cursor: pointer"
        >
          <div class="card-header">
            <span>Thông báo công ty</span>
          </div>
          <div class="card-body" style="height: 125px"></div>
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
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
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
::v-deep(.header-none) {
  thead {
    display: none !important;
  }
}
::v-deep(#chart32) {
  canvas {
    height: 100% !important;
  }
}
::v-deep(#chart1) {
  canvas {
    width: 90% !important;
    height: 90% !important;
    text-align: center !important;
    display: flex;
    justify-content: center;
  }
}
</style>
