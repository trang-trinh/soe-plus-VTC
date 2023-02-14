<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import { useRoute } from "vue-router";
import moment from "moment";
import format from "date-fns/fp/format/index.js";

const route = useRoute();
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

//Declare
const views = ref([
  { view: 1, title: "Sơ yếu lý lịch", icon: "fa-regular fa-address-card" },
  { view: 2, title: "Công việc", icon: "fa-solid fa-list-check" },
  { view: 3, title: "Hợp đồng", icon: "fa-solid fa-file-contract" },
  { view: 4, title: "Chấm công", icon: "fa-solid fa-building-circle-check" },
  { view: 5, title: "Phiếu lương", icon: "a-solid fa-money-check-dollar" },
  { view: 6, title: "Bảo hiểm", icon: "fa-solid fa-file-shield" },
  { view: 7, title: "Phép năm", icon: "fa-regular fa-calendar-days" },
  { view: 8, title: "Đào tạo", icon: "fa-solid fa-person-chalkboard" },
  { view: 9, title: "Quyết định", icon: "fa-solid fa-envelope-open" },
  { view: 10, title: "Tệp số hóa", icon: "fa-solid fa-paperclip" },
  { view: 11, title: null, icon: "fa-solid fa-ellipsis" },
]);
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  view: 1,
  profile_id: null,
});
const selectedNodes = ref([]);

//data view 1
const profile = ref({});
const places = ref([]);
const marital_status = ref([
  { value: 0, text: "Độc thân" },
  { value: 1, text: "Kết hôn" },
  { value: 2, text: "Ly hôn" },
]);
const dependents = ref([
  { value: 1, title: "Có phụ thuộc" },
  { value: 0, title: "Không phụ thuộc" },
]);
const forms = ref([
  { value: 0, title: "Dự bị" },
  { value: 1, title: "Chính thức" },
  { value: 1, title: "Điều chuyển" },
]);
const dictionarys = ref([]);
const datachilds = ref([]);

//data view 3
const contracts = ref([]);
const typestatus = ref([
  { value: 0, title: "Chưa hiệu lực", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Đang hiệu lực", bg_color: "#2196f3", text_color: "#fff" },
  { value: 2, title: "Hết hiệu lực", bg_color: "red", text_color: "#fff" },
  { value: 3, title: "Đã thanh lý", bg_color: "#ff8b4e", text_color: "#fff" },
]);

//filter
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};
const goBack = () => {
  router.back();
};

//Function
const changeView = (view) => {
  options.value.view = view;
  switch (view) {
    case 1:
      initView1(true);
      break;
    case 3:
      initView3(true);
      break;
    default:
      break;
  }
};

//init Dictionary
const initPlace = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_places_list",
            par: [
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 100 },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      renderPlace(response);
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const renderPlace = (response) => {
  let list1 = [];
  let list2 = [];
  let list3 = [];
  let d1 = JSON.parse(response.data.data)[0];
  d1.forEach((element, i) => {
    let c = {
      key: element.place_id,
      data: element.place_id,
      label: element.name,
      children: null,
    };
    if (d1[i].children) {
      list2 = JSON.parse(d1[i].children);
      if (list2 != null) {
        list2.forEach((element, i) => {
          element.label = element.data.name;
          element.data = parseInt(element.data.place_id);
          element.key = element.data;
          //đổi is_order
          if (list2[i].children != null && list2[i].children.length > 0) {
            // list3 = list2[i].children;
            // list2[i].children = list3;
            list2[i].children.forEach((element, i) => {
              element.label = element.data.name;
              element.data = parseInt(element.data.place_id);
              element.key = element.data;
            });
          }
        });
      }
      c.children = list2;
    }
    list1.push(c);
  });
  places.value = list1;
};
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_dictionary",
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
          dictionarys.value = tbs;
        }
      }
    })
    .then(() => {
      initPlace();
    })
    .then(() => {
      initView1(true);
    });
};

//Init data
const initView1 = (rf) => {
  datachilds.value = [];
  options.value.loading = true;
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
            proc: "hrm_profile_detail_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va: options.value["profile_id"] },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          profile.value = tbs[0][0];
          profile.value["gender"] =
            profile.value["gender"] == 1
              ? "Nam"
              : profile.value["gender"] == 2
              ? "Nữ"
              : "";
          var idx = places.value.findIndex(
            (x) => x["place_id"] === profile.value["birthplace_id"]
          );
          if (idx !== -1) {
            profile.value["select_birthplace"] =
              places.value[idx]["place_name"];
          }
          var idx = places.value.findIndex(
            (x) => x["place_id"] === profile.value["birthplace_origin_id"]
          );
          if (idx !== -1) {
            profile.value["select_birthplace_origin"] =
              places.value[idx]["place_name"];
          }
          var idx = places.value.findIndex(
            (x) => x["place_id"] === profile.value["place_register_permanent"]
          );
          if (idx !== -1) {
            profile.value["select_place_register_permanent"] =
              places.value[idx]["place_name"];
          }
          if (profile.value["recruitment_date"] != null) {
            profile.value["recruitment_date"] = moment(
              new Date(profile.value["recruitment_date"])
            ).format("DD/MM/YYYY");
          }
          if (profile.value["birthday"] != null) {
            profile.value["birthday"] = moment(
              new Date(profile.value["birthday"])
            ).format("DD/MM/YYYY");
          }
          if (profile.value["identity_date_issue"] != null) {
            profile.value["identity_date_issue"] = moment(
              new Date(profile.value["identity_date_issue"])
            ).format("DD/MM/YYYY");
          }
          //
          var idx = dictionarys.value[0].findIndex(
            (x) =>
              x["identity_papers_id"] === profile.value["identity_papers_id"]
          );
          if (profile.value["identity_papers_id"] != null && idx != -1) {
            profile.value["identity_papers_name"] =
              dictionarys.value[0][idx]["identity_papers_name"];
          }
          //
          var idx = dictionarys.value[17].findIndex(
            (x) => x["identity_place_id"] === profile.value["identity_place_id"]
          );
          if (profile.value["identity_place_id"] != null && idx != -1) {
            profile.value["identity_place_name"] =
              dictionarys.value[17][idx]["identity_place_name"];
          }
          //
          var idx = dictionarys.value[1].findIndex(
            (x) => x["nationality_id"] === profile.value["nationality_id"]
          );
          if (profile.value["nationality_id"] != null && idx != -1) {
            profile.value["nationality_name"] =
              dictionarys.value[1][idx]["nationality_name"];
          }
          //
          var idx = marital_status.value.findIndex(
            (x) => x["value"] === profile.value["marital_status"]
          );
          if (profile.value["value"] != null && idx != -1) {
            profile.value["marital_status"] = marital_status.value[idx]["text"];
          }
          //
          var idx = dictionarys.value[2].findIndex(
            (x) => x["ethnic_id"] === profile.value["ethnic_id"]
          );
          if (profile.value["ethnic_id"] != null && idx != -1) {
            profile.value["ethnic_name"] =
              dictionarys.value[2][idx]["ethnic_name"];
          }
          //
          var idx = dictionarys.value[3].findIndex(
            (x) => x["religion_id"] === profile.value["religion_id"]
          );
          if (profile.value["religion_id"] != null && idx != -1) {
            profile.value["religion_name"] =
              dictionarys.value[3][idx]["religion_name"];
          }
          //
          var idx = dictionarys.value[4].findIndex(
            (x) => x["bank_id"] === profile.value["bank_id"]
          );
          if (profile.value["bank_id"] != null && idx != -1) {
            profile.value["bank_name"] = dictionarys.value[4][idx]["bank_name"];
          }
          //
          var idx = dictionarys.value[5].findIndex(
            (x) => x["cultural_level_id"] === profile.value["cultural_level_id"]
          );
          if (profile.value["cultural_level_id"] != null && idx != -1) {
            profile.value["cultural_level_name"] =
              dictionarys.value[5][idx]["cultural_level_name"];
          }
          //
          var idx = dictionarys.value[6].findIndex(
            (x) => x["academic_level_id"] === profile.value["academic_level_id"]
          );
          if (profile.value["academic_level_id"] != null && idx != -1) {
            profile.value["academic_level_name"] =
              dictionarys.value[6][idx]["academic_level_name"];
          }
          //
          var idx = dictionarys.value[7].findIndex(
            (x) => x["specialization_id"] === profile.value["specialization_id"]
          );
          if (profile.value["specialization_id"] != null && idx != -1) {
            profile.value["specialization_name"] =
              dictionarys.value[7][idx]["specialization_name"];
          }
          //
          var idx = dictionarys.value[14].findIndex(
            (x) =>
              x["management_state_id"] === profile.value["management_state_id"]
          );
          if (profile.value["management_state_id"] != null && idx != -1) {
            profile.value["management_state_name"] =
              dictionarys.value[14][idx]["management_state_name"];
          }
          //
          var idx = dictionarys.value[8].findIndex(
            (x) =>
              x["political_theory_id"] === profile.value["political_theory_id"]
          );
          if (profile.value["political_theory_id"] != null && idx != -1) {
            profile.value["political_theory_name"] =
              dictionarys.value[8][idx]["political_theory_name"];
          }
          //
          var idx = dictionarys.value[9].findIndex(
            (x) => x["language_level_id"] === profile.value["language_level_id"]
          );
          if (profile.value["language_level_id"] != null && idx != -1) {
            profile.value["language_level_name"] =
              dictionarys.value[9][idx]["language_level_name"];
          }
          //
          var idx = dictionarys.value[10].findIndex(
            (x) =>
              x["informatic_level_id"] === profile.value["informatic_level_id"]
          );
          if (profile.value["informatic_level_id"] != null && idx != -1) {
            profile.value["informatic_level_name"] =
              dictionarys.value[10][idx]["informatic_level_name"];
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((x) => {
            if (x["identification_date_issue"] != null) {
              x["identification_date_issue"] = moment(
                new Date(x["identification_date_issue"])
              ).format("DD/MM/YYYY");
            }
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format(
                "DD/MM/YYYY"
              );
            }
            //
            var idx = dictionarys.value[11].findIndex(
              (a) => a["relationship_id"] === x["relationship_id"]
            );
            if (x["relationship_id"] != null && idx != -1) {
              x["relationship_name"] =
                dictionarys.value[11][idx]["relationship_name"];
            }
            //
            var idx = dependents.value.findIndex(
              (a) => a["value"] === x["is_dependent"]
            );
            if (x["is_dependent"] != null && idx != -1) {
              x["dependent_name"] = dependents.value[idx]["title"];
            }
          });
          datachilds.value[1] = tbs[1];
        } else {
          datachilds.value[1] = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          tbs[2].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format("MM/YYYY");
            }
            if (x["certificate_start_date"] != null) {
              x["certificate_start_date"] = moment(
                new Date(x["certificate_start_date"])
              ).format("DD/MM/YYYY");
            }
            if (x["certificate_end_date"] != null) {
              x["certificate_end_date"] = moment(
                new Date(x["certificate_end_date"])
              ).format("DD/MM/YYYY");
            }
            //
            var idx = dictionarys.value[18].findIndex(
              (a) => a["specialization_id"] === x["specialized"]
            );
            if (x["specialized"] != null && idx != -1) {
              x["specialization_name"] =
                dictionarys.value[18][idx]["specialization_name"];
            }
            //
            var idx = dictionarys.value[12].findIndex(
              (a) => a["form_traning_id"] === x["form_traning_id"]
            );
            if (x["form_traning_id"] != null && idx != -1) {
              x["form_traning_name"] =
                dictionarys.value[12][idx]["form_traning_name"];
            }
            //
            var idx = dictionarys.value[13].findIndex(
              (a) => a["certificate_id"] === x["certificate_id"]
            );
            if (x["certificate_id"] != null && idx != -1) {
              x["certificate_name"] =
                dictionarys.value[13][idx]["certificate_name"];
            }
          });
          datachilds.value[2] = tbs[2];
        } else {
          datachilds.value[2] = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          tbs[3].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format(
                "DD/MM/YYYY"
              );
            }
            //
            var idx = forms.value.findIndex((a) => a["value"] === x["form"]);
            if (x["form"] != null && idx != -1) {
              x["form"] = forms.value[idx]["title"];
            }
          });
          datachilds.value[3] = tbs[3];
        } else {
          datachilds.value[3] = [];
        }
        if (tbs[4] != null && tbs[4].length > 0) {
          tbs[4].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format("MM/YYYY");
            }
          });
          datachilds.value[4] = tbs[4];
        } else {
          datachilds.value[4] = [];
        }
        if (tbs[5] != null && tbs[5].length > 0) {
          profile.value["files"] = tbs[5];
        } else {
          profile.value["files"] = [];
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
const initView3 = (rf) => {
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
            proc: "hrm_contract_by_user",
            par: [
              { par: "profile_id", va: options.value["profile_id"] },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
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
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            data[0].forEach((item, i) => {
              item["STT"] = i + 1;
              var idx = typestatus.value.findIndex(
                (x) => x["value"] === item["status"]
              );
              if (idx != -1) {
                item["status_name"] = typestatus.value[idx]["title"];
                item["bg_color"] = typestatus.value[idx]["bg_color"];
                item["text_color"] = typestatus.value[idx]["text_color"];
              } else {
                item["status_name"] = "Chưa xác định";
                item["bg_color"] = "#bbbbbb";
                item["text_color"] = "#fff";
              }
              if (item["sign_date"] != null) {
                item["sign_date"] = moment(new Date(item["sign_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["start_date"] != null) {
                item["start_date"] = moment(
                  new Date(item["start_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["end_date"] != null) {
                item["end_date"] = moment(new Date(item["end_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["created_date"] != null) {
                item["created_date"] = moment(
                  new Date(item["created_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["liquidation_date"] != null) {
                item["liquidation_date"] = moment(
                  new Date(item["liquidation_date"])
                ).format("DD/MM/YYYY");
              }
            });
            contracts.value = data[0];
            if (data[1] != null && data[1].length > 0) {
              options.value.total = data[1][0].total;
            }
          } else {
            contracts.value = [];
            options.value.total = 0;
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
const initData = () => {
  if (options.value.view === 1) {
      initDictionary();
    } else if (options.value.view === 2) {
    } else if (options.value.view === 3) {
      initView3(true);
    }
}
onMounted(() => {
  if (route.params.id != null) {
    options.value["profile_id"] = route.params.id;
    initData();
  } else {
    router.back();
    return;
  }
});
//page
const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  options.value.pageNo = event.page + 1;
  initData();
};
</script>
<template>
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <ul class="flex p-0 m-0" style="list-style: none">
          <li>
            <Button
              @click="goBack()"
              type="button"
              label="Quay lại"
              icon="pi pi-arrow-left"
              class="p-button"
              style="
                background-color: #5bc0de !important;
                border: 1px solid #5bc0de !important;
              "
            />
          </li>
        </ul>
      </template>
      <template #end> </template>
    </Toolbar>
    <Toolbar class="outline-none surface-0 border-none pt-0">
      <template #start>
        <div style="height: 36px; display: flex; align-items: center">
          <SelectButton
            :options="views"
            v-model="options.view"
            @change="changeView(options.view)"
            optionValue="view"
            optionLabel="view"
            dataKey="view"
            aria-labelledby="custom"
          >
            <template #option="slotProps">
              <span
                v-if="slotProps.option.icon != null"
                :class="{ 'mr-2': slotProps.option.title != null }"
              >
                <font-awesome-icon :icon="slotProps.option.icon" />
              </span>
              <span> {{ slotProps.option.title }}</span>
            </template>
          </SelectButton>
        </div>
      </template>
      <template #end> </template>
    </Toolbar>
    <div class="d-lang-table" style="border-top: solid 1px rgba(0, 0, 0, 0.1)">
      <div class="grid formgrid m-0">
        <div class="row">
          <div class="col-9 md:col-9 p-3 pr-0">
            <div class="d-lang-table-1">
              <div v-show="options.view === 1" class="f-full">
                <div class="row">
                  <div class="col-12 md:col-12 p-0">
                    <!-- 1. Thông tin chung -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <span>1. Thông tin chung</span>
                        </template>
                        <div class="col-12 md:col-12 p-0">
                          <div class="row">
                            <div class="col-3 md:col-3 format-center">
                              <div class="form-group">
                                <div
                                  class="inputanh2 relative mb-2"
                                  style="margin: 0 auto"
                                >
                                  <img
                                    id="avatar"
                                    v-bind:src="
                                      profile.avatar
                                        ? basedomainURL + profile.avatar
                                        : basedomainURL +
                                          '/Portals/Image/noimg.jpg'
                                    "
                                  />
                                </div>
                                <label class="text-center">Ảnh đại diện</label>
                              </div>
                            </div>
                            <div class="col-9 md:col-9 p-0">
                              <div class="row">
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label>
                                      Mã nhân sự:
                                      <span class="description">{{
                                        profile.profile_id
                                      }}</span>
                                    </label>
                                  </div>
                                </div>
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label
                                      >Mã chấm công:
                                      <span class="description">{{
                                        profile.check_in_id
                                      }}</span></label
                                    >
                                  </div>
                                </div>
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label
                                      >Mã quản lý cấp trên:
                                      <span class="description">{{
                                        profile.superior_id
                                      }}</span></label
                                    >
                                  </div>
                                </div>
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label
                                      >Ngày tuyển dụng:
                                      <span class="description">{{
                                        profile.recruitment_date
                                      }}</span></label
                                    >
                                  </div>
                                </div>
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label
                                      >Họ và tên:
                                      <span class="description">{{
                                        profile.profile_user_name
                                      }}</span></label
                                    >
                                  </div>
                                </div>
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label
                                      >Tên gọi khác:
                                      <span class="description">{{
                                        profile.profile_nick_name
                                      }}</span></label
                                    >
                                  </div>
                                </div>
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label
                                      >Ngày sinh:
                                      <span class="description">{{
                                        profile.birthday
                                      }}</span></label
                                    >
                                  </div>
                                </div>
                                <div class="col-6 md:col-6">
                                  <div class="form-group">
                                    <label
                                      >Giới tính:
                                      <span class="description">{{
                                        profile.gender
                                      }}</span></label
                                    >
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label
                              >Nơi sinh:
                              <span class="description">{{
                                profile.select_birthplace
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label
                              >Quê quán:
                              <span class="description">{{
                                profile.select_birthplace_origin
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label
                              >Nơi đăng ký HKTT:
                              <span class="description">{{
                                profile.select_place_register_permanent
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="row">
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Loại giấy tờ:
                                  <span class="discription">{{
                                    profile.identity_papers_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Số:
                                  <span class="description">{{
                                    profile.identity_papers_code
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Ngày cấp:
                                  <span class="description">{{
                                    profile.identity_date_issue
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Nơi cấp:
                                  <span class="discription">{{
                                    profile.identity_papers_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Quốc tịch:
                                  <span class="discription">{{
                                    profile.nationality_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Tình trạng hôn nhân:
                                  <span class="discription">{{
                                    profile.marital_status
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Dân tộc:
                                  <span class="discription">{{
                                    profile.ethnic_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Tôn giáo:
                                  <span class="discription">{{
                                    profile.religion_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Mã số thuế:
                                  <span class="description">{{
                                    profile.tax_code
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Ngân hàng:
                                  <span class="discription">{{
                                    profile.bank_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Số tài khoản:
                                  <span class="description">{{
                                    profile.bank_number
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Tên tài khoản:
                                  <span class="description">{{
                                    profile.bank_account
                                  }}</span></label
                                >
                              </div>
                            </div>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 2. Trình độ học vấn -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <!-- <i class="pi pi-book mr-2"></i> -->
                          <span>2. Trình độ học vấn</span>
                        </template>
                        <div class="col-12 md:col-12">
                          <div class="row">
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Trình độ phổ thông:
                                  <span class="discription">{{
                                    profile.cultural_level_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Trình độ học vấn cao nhất:
                                  <span class="discription">{{
                                    profile.academic_level_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Chuyên ngành học:
                                  <span class="discription">{{
                                    profile.specialization_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Quản lý nhà nước:
                                  <span class="discription">{{
                                    profile.management_state_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Lý luận chính trị:
                                  <span class="discription">{{
                                    profile.political_theory_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Ngoại ngữ:
                                  <span class="discription">{{
                                    profile.language_level_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-4 md:col-4">
                              <div class="form-group">
                                <label
                                  >Tin học:
                                  <span class="discription">{{
                                    profile.informatic_level_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 3. Thông tin liên hệ -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <!-- <i class="pi pi-info-circle mr-2"></i> -->
                          <span>3. Thông tin liên hệ</span>
                        </template>
                        <div class="col-12 md:col-12">
                          <div class="row">
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Số điện thoại:
                                  <span class="description">{{
                                    profile.phone
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Email:
                                  <span class="description">{{
                                    profile.email
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-12 md:col-12">
                              <div class="form-group">
                                <label
                                  >Thường trú:
                                  <span class="description">{{
                                    profile.place_permanent
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-12 md:col-12">
                              <div class="form-group">
                                <label
                                  >Chỗ ở hiện nay:
                                  <span class="description">{{
                                    profile.place_residence
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-12 md:col-12">
                              <div class="form-group">
                                <label class="m-0">Khi cần báo tin cho:</label>
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Họ và tên:
                                  <span class="description">{{
                                    profile.involved_name
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Số điện thoại:
                                  <span class="description">{{
                                    profile.involved_phone
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-12 md:col-12">
                              <div class="form-group">
                                <label
                                  >Địa chỉ:
                                  <span class="description">{{
                                    profile.involved_place
                                  }}</span></label
                                >
                              </div>
                            </div>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 4. Thông tin gia đình, người phụ thuộc -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <Toolbar class="w-full custoolbar p-0 font-bold">
                            <template #start>
                              <!-- <i class="pi pi-users mr-2"></i> -->
                              <span
                                >4. Thông tin gia đình, người phụ thuộc</span
                              ></template
                            >
                          </Toolbar>
                        </template>
                        <div class="col-12 md:col-12 p-0">
                          <div style="min-height: 250px">
                            <DataTable
                              :value="datachilds[1]"
                              :scrollable="true"
                              :lazy="true"
                              :rowHover="true"
                              :showGridlines="true"
                              scrollDirection="both"
                              style="display: grid"
                            >
                              <Column
                                field="relative_name"
                                header="Họ tên"
                                headerStyle="text-align:center;width:180px;height:50px"
                                bodyStyle="text-align:center;width:180px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.relative_name
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="relationship_id"
                                header="Quan hệ"
                                headerStyle="text-align:center;width:170px;height:50px"
                                bodyStyle="text-align:center;width:170px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <div class="form-group m-0">
                                    <span>{{
                                      slotProps.data.relationship_name
                                    }}</span>
                                  </div>
                                </template>
                              </Column>
                              <Column
                                field="identification_date_issue"
                                header="Năm sinh"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.identification_date_issue
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="phone"
                                header="SĐT"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.phone }}</span>
                                </template>
                              </Column>
                              <Column
                                field="tax_code"
                                header="Mã số thuế"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.tax_code }}</span>
                                </template>
                              </Column>
                              <Column
                                field="identification_citizen"
                                header="CCCD/Hộ chiếu"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.identification_citizen
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="identification_date_issue"
                                header="Ngày cấp"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  {{ slotProps.data.identification_date_issue }}
                                </template>
                              </Column>
                              <Column
                                field="identification_place_issue"
                                header="Nơi cấp"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  {{
                                    slotProps.data.identification_place_issue
                                  }}
                                </template>
                              </Column>
                              <Column
                                field="is_dependent"
                                header="Phụ thuộc"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <div class="form-group m-0">
                                    <span>{{
                                      slotProps.data.dependent_name
                                    }}</span>
                                  </div>
                                </template>
                              </Column>
                              <Column
                                field="start_date"
                                header="Từ ngày"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.start_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="end_date"
                                header="Đến ngày"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.end_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="info"
                                header="Thông tin cơ bản"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.info }}</span>
                                </template>
                              </Column>
                              <Column
                                field="note"
                                header="Ghi chú"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.note }}</span>
                                </template>
                              </Column>
                              <template #empty>
                                <div
                                  class="
                                    align-items-center
                                    justify-content-center
                                    p-4
                                    text-center
                                    m-auto
                                  "
                                  style="
                                    display: flex;
                                    width: 100%;
                                    min-height: 200px;
                                  "
                                ></div>
                              </template>
                            </DataTable>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 5. Quá trình đào tạo, bồi dưỡng về chuyên môn, nghiệp vụ, lý luận chính trị, ngoại ngữ, tin học -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <Toolbar class="w-full custoolbar p-0 font-bold">
                            <template #start>
                              <!-- <i class="pi pi-replay mr-2"></i> -->
                              <span
                                >5. Quá trình đào tạo, bồi dưỡng về chuyên môn,
                                nghiệp vụ, lý luận chính trị, ngoại ngữ, tin
                                học</span
                              ></template
                            >
                          </Toolbar>
                        </template>
                        <div class="col-12 md:col-12 p-0">
                          <div style="min-height: 250px">
                            <DataTable
                              :value="datachilds[2]"
                              :scrollable="true"
                              :lazy="true"
                              :rowHover="true"
                              :showGridlines="true"
                              scrollDirection="both"
                              style="display: grid"
                            >
                              <Column
                                field="university_name"
                                header="Tên trường"
                                headerStyle="text-align:center;width:180px;height:50px"
                                bodyStyle="text-align:center;width:180px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.university_name
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="specialized"
                                header="Chuyên ngành"
                                headerStyle="text-align:center;width:170px;height:50px"
                                bodyStyle="text-align:center;width:170px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.specialization_name
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="start_date"
                                header="Từ tháng, năm"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.start_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="end_date"
                                header="Đến tháng, năm"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.end_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="form_traning_id"
                                header="Hình thức đào tạo"
                                headerStyle="text-align:center;width:170px;height:50px"
                                bodyStyle="text-align:center;width:170px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.form_traning_name
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="certificate_id"
                                header="Văn bằng, chứng chỉ"
                                headerStyle="text-align:center;width:170px;height:50px"
                                bodyStyle="text-align:center;width:170px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.certificate_name
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="certificate_start_date"
                                header="Ngày hiệu lực"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.certificate_start_date
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="certificate_end_date"
                                header="Ngày hết hiệu lực"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.certificate_end_date
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="certificate_key_code"
                                header="Số hiệu"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.certificate_key_code
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="certificate_version"
                                header="phiên bản"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.certificate_version
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="certificate_release_time"
                                header="Lần phát hành"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.certificate_release_time
                                  }}</span>
                                </template>
                              </Column>
                              <template #empty>
                                <div
                                  class="
                                    align-items-center
                                    justify-content-center
                                    p-4
                                    text-center
                                    m-auto
                                  "
                                  style="
                                    display: flex;
                                    width: 100%;
                                    min-height: 200px;
                                  "
                                ></div>
                              </template>
                            </DataTable>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 6. Lịch sử Đảng viên -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <Toolbar class="w-full custoolbar p-0 font-bold">
                            <template #start>
                              <!-- <i class="pi pi-replay mr-2"></i> -->
                              <span>6. Lịch sử Đảng viên</span></template
                            >
                          </Toolbar>
                        </template>
                        <div class="col-12 md:col-12 p-0">
                          <div style="min-height: 250px">
                            <DataTable
                              :value="datachilds[3]"
                              :scrollable="true"
                              :lazy="true"
                              :rowHover="true"
                              :showGridlines="true"
                              scrollDirection="both"
                              style="display: grid"
                            >
                              <Column
                                field="card_number"
                                header="Số thẻ"
                                headerStyle="text-align:center;width:180px;height:50px"
                                bodyStyle="text-align:center;width:180px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.card_number }}</span>
                                </template>
                              </Column>
                              <Column
                                field="form"
                                header="Hình thức"
                                headerStyle="text-align:center;width:170px;height:50px"
                                bodyStyle="text-align:center;width:170px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.form }}</span>
                                </template>
                              </Column>
                              <Column
                                field="start_date"
                                header="Từ ngày"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.start_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="end_date"
                                header="Đến ngày"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.end_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="admission_place"
                                header="Nơi kết nạp"
                                headerStyle="text-align:center;width:180px;height:50px"
                                bodyStyle="text-align:center;width:180px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.admission_place
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="transfer_place"
                                header="Nơi điều chuyển"
                                headerStyle="text-align:center;width:180px;height:50px"
                                bodyStyle="text-align:center;width:180px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.transfer_place
                                  }}</span>
                                </template>
                              </Column>
                              <template #empty>
                                <div
                                  class="
                                    align-items-center
                                    justify-content-center
                                    p-4
                                    text-center
                                    m-auto
                                  "
                                  style="
                                    display: flex;
                                    width: 100%;
                                    min-height: 200px;
                                  "
                                ></div>
                              </template>
                            </DataTable>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 7. Lịch sử tham gia quân đội -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <!-- <i class="pi pi-chart-line mr-2"></i> -->
                          <span>7. Lịch sử tham gia quân đội</span>
                        </template>
                        <div class="col-12 md:col-12">
                          <div class="row">
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Ngày nhập ngũ:
                                  <span class="description">{{
                                    profile.military_start_date
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Ngày xuất ngũ:
                                  <span class="description">{{
                                    profile.military_end_date
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Quân hàm cao nhất:
                                  <span class="description">{{
                                    profile.military_rank
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Danh hiệu cao nhất:
                                  <span class="description">{{
                                    profile.military_title
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Sở trường công tác:
                                  <span class="description">{{
                                    profile.military_forte
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Sức khỏe:
                                  <span class="description">{{
                                    profile.military_health
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Khen thưởng:
                                  <span class="description">{{
                                    profile.military_reward
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Kỷ luật:
                                  <span class="description">{{
                                    profile.military_discipline
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Thương binh hạng:
                                  <span class="description">{{
                                    profile.military_veterans_rank
                                  }}</span></label
                                >
                              </div>
                            </div>
                            <div class="col-6 md:col-6">
                              <div class="form-group">
                                <label
                                  >Con gia đình chính sách:
                                  <span class="description">{{
                                    profile.military_policy_family
                                  }}</span></label
                                >
                              </div>
                            </div>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 8. Kinh nghiệm làm việc -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <Toolbar class="w-full custoolbar p-0 font-bold">
                            <template #start>
                              <span>8. Kinh nghiệm làm việc</span></template
                            >
                          </Toolbar>
                        </template>
                        <div class="col-12 md:col-12 p-0">
                          <div style="min-height: 250px">
                            <DataTable
                              :value="datachilds[4]"
                              :scrollable="true"
                              :lazy="true"
                              :rowHover="true"
                              :showGridlines="true"
                              scrollDirection="both"
                              style="display: grid"
                            >
                              <Column
                                field="start_date"
                                header="Từ tháng, năm"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.start_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="end_date"
                                header="Đến tháng, năm"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.end_date }}</span>
                                </template>
                              </Column>
                              <Column
                                field="company"
                                header="Công ty, đơn vị"
                                headerStyle="text-align:center;width:180px;height:50px"
                                bodyStyle="text-align:center;width:180px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.company }}</span>
                                </template>
                              </Column>
                              <Column
                                field="role"
                                header="Vị trí"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.role }}</span>
                                </template>
                              </Column>
                              <Column
                                field="reference_name"
                                header="Người tham chiếu"
                                headerStyle="text-align:center;width:150px;height:50px"
                                bodyStyle="text-align:center;width:150px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.reference_name
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="reference_phone"
                                header="SĐT"
                                headerStyle="text-align:center;width:120px;height:50px"
                                bodyStyle="text-align:center;width:120px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{
                                    slotProps.data.reference_phone
                                  }}</span>
                                </template>
                              </Column>
                              <Column
                                field="description"
                                header="Mô tả công việc"
                                headerStyle="text-align:center;width:200px;height:50px"
                                bodyStyle="text-align:center;width:200px;"
                                class="
                                  align-items-center
                                  justify-content-center
                                  text-center
                                "
                              >
                                <template #body="slotProps">
                                  <span>{{ slotProps.data.description }}</span>
                                </template>
                              </Column>
                              <template #empty>
                                <div
                                  class="
                                    align-items-center
                                    justify-content-center
                                    p-4
                                    text-center
                                    m-auto
                                  "
                                  style="
                                    display: flex;
                                    width: 100%;
                                    min-height: 200px;
                                  "
                                ></div>
                              </template>
                            </DataTable>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- Đặc điểm lịch sử bản thân -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <!-- <i class="pi pi-chart-line mr-2"></i> -->
                          <span>9. Đặc điểm lịch sử bản thân</span>
                        </template>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label
                              >Thông tin 1:
                              <span class="description">{{
                                profile.biography_first
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label
                              >Thông tin 2:
                              <span class="description">{{
                                profile.biography_second
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label
                              >Thông tin 3:
                              <span class="description">{{
                                profile.biography_third
                              }}</span></label
                            >
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                    <!-- 10.	Đính kèm khác (file số hóa liên quan) -->
                    <Accordion class="w-full" :activeIndex="0">
                      <AccordionTab>
                        <template #header>
                          <!-- <i class="pi pi-chart-line mr-2"></i> -->
                          <span>
                            10. Đính kèm khác (file số hóa liên quan)</span
                          >
                        </template>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label>Tải file lên </label>
                            <div
                              v-if="
                                profile.files != null &&
                                profile.files.length > 0
                              "
                            >
                              <DataView
                                :lazy="true"
                                :value="profile.files"
                                :rowHover="true"
                                :scrollable="true"
                                class="
                                  w-full
                                  h-full
                                  ptable
                                  p-datatable-sm
                                  flex flex-column
                                "
                                layout="list"
                                responsiveLayout="scroll"
                              >
                                <template #list="slotProps">
                                  <div class="w-full">
                                    <Toolbar class="w-full">
                                      <template #start>
                                        <div
                                          @click="goFile(slotProps.data)"
                                          class="flex align-items-center"
                                        >
                                          <img
                                            class="mr-2"
                                            :src="
                                              basedomainURL +
                                              '/Portals/Image/file/' +
                                              slotProps.data.file_type +
                                              '.png'
                                            "
                                            style="object-fit: contain"
                                            width="40"
                                            height="40"
                                          />
                                          <span style="line-height: 1.5">
                                            {{ slotProps.data.file_name }}</span
                                          >
                                        </div>
                                      </template>
                                    </Toolbar>
                                  </div>
                                </template>
                              </DataView>
                            </div>
                          </div>
                        </div>
                      </AccordionTab>
                    </Accordion>
                  </div>
                  <div class="col-12 md:col-12 p-0 mt-2">
                    <div class="form-group">
                      <label
                        >Ghi chú:
                        <span class="description">{{
                          profile.note
                        }}</span></label
                      >
                    </div>
                  </div>
                </div>
              </div>
              <div v-show="options.view === 2" class="f-full">Công việc</div>
              <div v-show="options.view === 3" class="f-full">
                <div class="d-lang-table">
      <DataTable
        @page="onPage($event)"
        :value="datas"
        :paginator="true"
        :rows="options.pageSize"
        :rowsPerPageOptions="[25, 50, 100, 200]"
        :totalRecords="options.total"
        :scrollable="true"
        :lazy="true"
        :rowHover="true"
        :showGridlines="false"
        :globalFilterFields="['type_contract_name']"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="contract_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
      >
        <!-- <Column
          field="STT"
          header="STT"
          headerStyle="text-align:center;max-width:75px;height:50px"
          bodyStyle="text-align:center;max-width:75px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column> -->
        <Column
          field="contract_no"
          header="Mã HĐ"
          headerStyle="text-align:center;max-width:80px;height:50px"
          bodyStyle="text-align:center;max-width:80px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="department_name"
          header="Phòng ban"
          headerStyle="height:50px;max-width:auto;"
        >
          <template #body="slotProps">
            {{ slotProps.data.department_name }}
          </template>
        </Column>
        <Column
          field="type_contract_name"
          header="Loại hợp đồng"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.data.type_contract_name }}
          </template>
        </Column>
        <Column
          field="sign_date"
          header="Ngày ký"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.sign_date }}</span>
          </template>
        </Column>
        <Column
          field="start_date"
          header="Ngày hiệu lực"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.start_date }}</span>
          </template>
        </Column>
        <Column
          field="end_date"
          header="Ngày hết hạn"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.end_date }}</span>
          </template>
        </Column>
        <Column
          field="sign_user_name"
          header="Người ký"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.data.sign_user_name }}
          </template>
        </Column>
        <Column
          field="created_date"
          header="Ngày/Người lập"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span class="mr-2">{{ slotProps.data.created_date }}</span>
            <div>
              <Avatar
                v-bind:label="
                  slotProps.data.avatar
                    ? ''
                    : slotProps.data.full_name.substring(0, 1)
                "
                v-bind:image="
                  slotProps.data.avatar
                    ? basedomainURL + slotProps.data.avatar
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
                style="
                  background-color: #2196f3;
                  color: #ffffff;
                  width: 2rem;
                  height: 2rem;
                  font-size: 1rem !important;
                "
                :style="{
                  background: bgColor[slotProps.data.created_is_order % 7],
                }"
                class="text-avatar"
                size="xlarge"
                shape="circle"
                v-tooltip.top="slotProps.data.full_name"
              />
            </div>
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:140px;height:50px"
          bodyStyle="text-align:center;max-width:140px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div
              class="m-2"
              aria:haspopup="true"
              aria-controls="overlay_panel_status"
            >
              <Button
                :label="slotProps.data.status_name"
                :style="{
                  border: slotProps.data.bg_color,
                  backgroundColor: slotProps.data.bg_color,
                  color: slotProps.data.text_color,
                }"
              />
            </div>
          </template>
        </Column>
        <template #empty>
          <div
            class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            "
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 326px);
              background-color: #fff;
            "
          >
            <div v-if="!options.loading && options.total == 0">
              <img src="../../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
              </div>
              <div v-show="options.view === 4" class="f-full">Chấm công</div>
              <div v-show="options.view === 5" class="f-full">Phiếu lương</div>
              <div v-show="options.view === 6" class="f-full">Bảo hiểm</div>
              <div v-show="options.view === 7" class="f-full">Phép năm</div>
              <div v-show="options.view === 8" class="f-full">Đào tạo</div>
              <div v-show="options.view === 9" class="f-full">Quyết định</div>
              <div v-show="options.view === 10" class="f-full">Tệp số hóa</div>
              <div v-show="options.view === 11" class="f-full">
                Sơ yếu lí lịch
              </div>
            </div>
          </div>
          <div
            class="col-3 md:col-3"
            style="border-left: solid 1px rgba(0, 0, 0, 0.1)"
          >
            <div></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
.d-lang-table {
  height: calc(100vh - 180px) !important;
  background-color: #fff;
  overflow: hidden;
}
.d-lang-table-1 {
  height: calc(100vh - 195px) !important;
  overflow-x: hidden;
  overflow-y: auto;
}
.icon-star {
  color: #f4b400 !important;
}
</style>
<style lang="scss" scoped>
</style>