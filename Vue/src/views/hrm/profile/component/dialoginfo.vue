<script setup>
import { onMounted, inject, ref } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../../util/function";
import moment from "moment";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  profile_id: String,
  isType: Number,
  initData: Function,
});
const display = ref(props.displayDialog);
//Declare
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const submitted = ref(false);
const model = ref({});
const datas = ref([]);
const data_copys = ref([]);
const places = ref([]);
const dictionarys = ref([]);
const marital_status = ref([
  { value: 0, text: "Độc thân" },
  { value: 1, text: "Kết hôn" },
  { value: 2, text: "Ly hôn" },
]);
const status = ref([
  { value: 0, title: "Chờ duyệt", bg_color: "#eee", text_color: "#000" },
  { value: 1, title: "Đã duyệt", bg_color: "#F2FBE6", text_color: "#000" },
]);

//Function
const save1 = (isEdit) => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var obj = JSON.parse(JSON.stringify(model.value));
  if (obj["select_place_register_permanent"] != null) {
    obj["place_register_permanent"] =
      Object.keys(obj["select_place_register_permanent"])[0] == -1
        ? null
        : Object.keys(obj["select_place_register_permanent"])[0];
  }
  if (obj["identity_date_issue"] != null) {
    obj["identity_date_issue"] = moment(obj["identity_date_issue"]).format(
      "YYYY-MM-DDTHH:mm:ssZZ"
    );
  }
  if (obj["identity_papers_outdate"] != null) {
    obj["identity_papers_outdate"] = moment(
      obj["identity_papers_outdate"]
    ).format("YYYY-MM-DDTHH:mm:ssZZ");
  }
  let formData = new FormData();
  formData.append("isEdit", isEdit);
  formData.append("model", JSON.stringify(obj));
  axios
    .put(baseURL + "/api/hrm_profile/update_profile_edit", formData, config)
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success("Cập nhật thành công!");
      props.closeDialog();
      // props.initData(true);
    })
    .catch((error) => {
      swal.close();
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
  if (submitted.value) submitted.value = true;
};
const save2 = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let formData = new FormData();
  datas.value.forEach((x) => {
    if (x["identification_date_issue"] != null) {
      x["identification_date_issue"] = moment(
        x["identification_date_issue"]
      ).format("YYYY-MM-DDTHH:mm:ssZZ");
    }
    if (x["start_date"] != null) {
      x["start_date"] = moment(x["start_date"]).format("YYYY-MM-DDTHH:mm:ssZZ");
    }
    if (x["end_date"] != null) {
      x["end_date"] = moment(x["end_date"]).format("YYYY-MM-DDTHH:mm:ssZZ");
    }
  });
  formData.append("profile_id", props.profile_id);
  formData.append("relative", JSON.stringify(datas.value));
  axios
    .put(
      baseURL + "/api/hrm_profile/update_profile_relative_late",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success("Cập nhật thành công!");
      props.closeDialog();
      props.initData(true);
    })
    .catch((error) => {
      swal.close();
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
  if (submitted.value) submitted.value = true;
};
const saveModel = (isEdit) => {
  if (props.isType === 1) {
    save1(isEdit);
  } else if (props.isType === 2) {
    save2();
  }
};
const addRow = (type) => {
  datas.value.push({});
};
const deleteRow = (type, idx) => {
  datas.value.splice(idx, 1);
};
const goEdit = (item) => {
  datas.value.forEach((x) => {
    x["active"] = false;
  });
  item["active"] = true;
  model.value = item;
};
const deleteEdit = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá hồ sơ này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        var ids = [];
        if (item != null) {
          ids = [item["key_id"]];
        }
        axios
          .delete(baseURL + "/api/hrm_profile/delete_profile_edit", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            swal.close();
            if (response.data.err === "1" || response.data.err === "2") {
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            toast.success("Xoá thành công!");
            initView1(true);
          })
          .catch((error) => {
            swal.close();
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
      }
    });
};

//init
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
      if (props.isType === 1) {
        initView1(true);
      } else if (props.isType === 2) {
        initView2(true);
      }
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
const initDictionary1 = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  dictionarys.value = [];
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
    });
};
const initView1 = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_edit_list",
            par: [{ par: "profile_id", va: props.profile_id }],
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
          model.value = tbs[0][0];
          if (model.value["identity_date_issue"] != null) {
            model.value["identity_date_issue"] = new Date(
              model.value["identity_date_issue"]
            );
          }
          if (model.value["identity_papers_outdate"] != null) {
            model.value["identity_papers_outdate"] = new Date(
              model.value["identity_papers_outdate"]
            );
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((item) => {
            item["select_place_register_permanent"] = {};
            item["select_place_register_permanent"][
              item["place_register_permanent"] || -1
            ] = true;
            if (item["status"] != null) {
              var idx = status.value.findIndex(
                (x) => x["value"] === item["status"]
              );
              if (idx !== -1) {
                item["bg_color"] = status.value[idx]["bg_color"];
                item["text_color"] = status.value[idx]["text_color"];
              }
            }
            if (item["key_id"] === model.value["key_id"]) {
              item["active"] = true;
            }
            if (item["created_date"] != null) {
              item["created_date"] = moment(
                new Date(item["created_date"])
              ).format("DD/MM/YYYY");
            }
            if (item["identity_date_issue"] != null) {
              item["identity_date_issue"] = new Date(
                item["identity_date_issue"]
              );
            }
            if (item["identity_papers_outdate"] != null) {
              item["identity_papers_outdate"] = new Date(
                item["identity_papers_outdate"]
              );
            }
          });
          datas.value = tbs[1];
          data_copys.value = JSON.parse(JSON.stringify(tbs[1]));
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
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
const initDictionary2 = () => {
  dictionarys.value = [];
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
      initView2(true);
    });
};
const initView2 = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_relative_late",
            par: [{ par: "profile_id", va: props.profile_id }],
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
          tbs[0].forEach((x) => {
            if (x["identification_date_issue"] != null) {
              x["identification_date_issue"] = new Date(
                x["identification_date_issue"]
              );
            }
            if (x["start_date"] != null) {
              x["start_date"] = new Date(x["start_date"]);
            }
            if (x["end_date"] != null) {
              x["end_date"] = new Date(x["end_date"]);
            }
          });
          datas.value = tbs[0];
        }
      }
      swal.close();
    })
    .catch((error) => {
      swal.close();
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
  if (props.isType === 1) {
    initDictionary1();
  } else if (props.isType === 2) {
    initDictionary2();
  }
};
onMounted(() => {
  if (props.displayDialog) {
    initData();
  }
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="display"
    :style="{ width: '72vw' }"
    :maximizable="true"
    :closable="true"
    style="z-index: 9000"
  >
    <form @submit.prevent="" name="submitform">
      <Toolbar
        v-if="props.isType === 1"
        class="outline-none surface-0 border-none pt-0"
      >
        <template #start>
          <ul class="flex p-0 m-0" :style="{ listStyle: 'none' }">
            <li
              v-for="(item, index) in datas"
              @click="goEdit(item)"
              :style="{
                padding: '0.5rem',
                marginRight: '0.5rem',
                border: item.bg_color,
                borderRadius: '3px',
                display: 'inline-block',
                background: item.bg_color,
                color: item.text_color,
                cursor: 'pointer',
              }"
              :class="{ hightlight: item.active }"
              class="li-custom relative"
            >
              <div class="format-center">
                <Avatar
                  v-bind:label="
                    item.avatar ? '' : (item.last_name ?? '').substring(0, 1)
                  "
                  v-bind:image="
                    item.avatar
                      ? basedomainURL + item.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  :style="{
                    background: bgColor[index % 7],
                    color: '#ffffff',
                    width: '3rem',
                    height: '3rem',
                  }"
                  class="mr-2 text-avatar"
                  size="xlarge"
                  shape="circle"
                  v-tooltip.top="item.full_name"
                />
                <div>
                  <h3 class="pb-1 m-0 text-center">
                    Lần thay đổi {{ index + 1 }}
                  </h3>
                  <span>Ngày: {{ item.created_date }}</span>
                </div>
              </div>
              <div
                class="li-custom-hover p-button-rounded absolute top-0 right-0 cursor-pointer"
              >
                <Button
                  style="width: 1.5rem; height: 1.5rem"
                  icon="pi pi-times"
                  @click="
                    deleteEdit(item);
                    $event.stopPropagation();
                  "
                />
              </div>
            </li>
          </ul>
        </template>
        <template #end> </template>
      </Toolbar>
      <div v-if="props.isType === 1" class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <h3 class="m-0">1. Thông tin chung</h3>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Nơi đăng ký HKTT</label>
            <TreeSelect
              :options="places"
              :showClear="true"
              :max-height="200"
              v-model="model.select_place_register_permanent"
              placeholder="Chọn nơi đăng ký"
              optionLabel="name"
              optionValue="place_id"
              class="ip36"
            >
            </TreeSelect>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="row">
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Loại giấy tờ</label>
                <Dropdown
                  :showClear="true"
                  :options="dictionarys[0]"
                  optionLabel="identity_papers_name"
                  optionValue="identity_papers_id"
                  placeholder="Chọn loại"
                  class="ip36"
                  v-model="model.identity_papers_id"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Số</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.identity_papers_code"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Nơi cấp</label>
                <Dropdown
                  :options="dictionarys[17]"
                  :showClear="true"
                  :filter="true"
                  v-model="model.identity_place_id"
                  placeholder="Chọn nơi cấp"
                  optionLabel="identity_place_name"
                  optionValue="identity_place_id"
                  class="ip36"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Ngày cấp</label>
                <Calendar
                  class="ip36"
                  id="icon"
                  v-model="model.identity_date_issue"
                  :showIcon="true"
                  placeholder="dd/mm/yyyy"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Ngày hết hạn</label>
                <Calendar
                  class="ip36"
                  id="icon"
                  v-model="model.identity_papers_outdate"
                  :showIcon="true"
                  placeholder="dd/mm/yyyy"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Quốc tịch</label>
                <Dropdown
                  :showClear="true"
                  :options="dictionarys[1]"
                  optionLabel="nationality_name"
                  optionValue="nationality_id"
                  placeholder="Chọn quốc tịch"
                  class="ip36"
                  v-model="model.nationality_id"
                  :filter="true"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Tình trạng hôn nhân</label>
                <Dropdown
                  :showClear="true"
                  :options="marital_status"
                  optionLabel="text"
                  optionValue="value"
                  placeholder="Chọn trạng thái"
                  class="ip36"
                  v-model="model.marital_status"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Mã số thuế</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.tax_code"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Ngân hàng</label>
                <Dropdown
                  :showClear="true"
                  :options="dictionarys[4]"
                  optionLabel="bank_name"
                  optionValue="bank_id"
                  placeholder="Chọn ngân hàng"
                  class="ip36"
                  v-model="model.bank_id"
                  :filter="true"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Số tài khoản</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.bank_number"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Tên tài khoản</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.bank_account"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Biển số xe máy</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.bike_code"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Biển số xe ô tô</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.car_code"
                  maxLength="50"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <h3 class="m-0">2. Liên hệ</h3>
          </div>
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="row">
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Số điện thoại</label>
                <InputMask
                  v-model="model.phone"
                  mask="9999999999"
                  placeholder="__________"
                  class="ip36"
                />
              </div>
            </div>
            <div class="col-6 md:col-6">
              <div class="form-group">
                <label>Email</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.email"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Thường trú</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.place_permanent"
                  maxLength="500"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Chỗ ở hiện nay</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.place_residence"
                  maxLength="500"
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label class="m-0">Khi cần báo tin cho:</label>
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Họ và tên</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.involved_name"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Số điện thoại</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.involved_phone"
                  maxLength="50"
                />
              </div>
            </div>
            <div class="col-4 md:col-4">
              <div class="form-group">
                <label>Mối quan hệ</label>
                <Dropdown
                  :showClear="true"
                  :options="dictionarys[11]"
                  optionLabel="relationship_name"
                  optionValue="relationship_id"
                  placeholder="Chọn quan hệ"
                  v-model="model.relationship_id"
                  class="ip36"
                  style="
                    white-space: nowrap;
                    overflow: hidden;
                    text-overflow: ellipsis;
                  "
                />
              </div>
            </div>
            <div class="col-12 md:col-12">
              <div class="form-group">
                <label>Địa chỉ</label>
                <InputText
                  spellcheck="false"
                  class="ip36"
                  v-model="model.involved_place"
                  maxLength="500"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
      <div v-if="props.isType === 2" class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <Toolbar class="w-full custoolbar font-bold">
            <template #start></template>
            <template #end>
              <a
                @click="
                  addRow(2);
                  $event.stopPropagation();
                "
                class="hover"
                v-tooltip.top="'Thêm mới'"
              >
                <i
                  class="pi pi-plus-circle"
                  data-v-62364173=""
                  style="font-size: 18px"
                ></i>
              </a>
            </template>
          </Toolbar>
        </div>
        <div class="col-12 md:col-12">
          <div style="min-height: 250px">
            <DataTable
              :value="datas"
              :scrollable="true"
              :lazy="true"
              :rowHover="true"
              :showGridlines="true"
              scrollDirection="both"
              class="empty-full p-datatable"
            >
              <Column
                header=""
                headerStyle="text-align:center;width:50px"
                bodyStyle="text-align:center;width:50px"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <a
                    @click="deleteRow(2, slotProps.index)"
                    class="hover"
                    v-tooltip.top="'Xóa'"
                  >
                    <i class="pi pi-times-circle" style="font-size: 18px"></i>
                  </a>
                </template>
              </Column>
              <Column
                field="relationship_id"
                header="Quan hệ gia đình"
                headerStyle="text-align:center;width:170px;height:50px"
                bodyStyle="text-align:center;width:170px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="form-group m-0">
                    <Dropdown
                      :showClear="true"
                      :options="[
                        { value: 1, title: 'Về bản thân' },
                        { value: 2, title: 'Về bên vợ' },
                      ]"
                      optionLabel="title"
                      optionValue="value"
                      placeholder="Chọn quan hệ"
                      v-model="slotProps.data.is_type"
                      class="ip36"
                      style="
                        white-space: nowrap;
                        overflow: hidden;
                        text-overflow: ellipsis;
                      "
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="relative_name"
                header="Họ tên"
                headerStyle="text-align:center;width:180px;height:50px"
                bodyStyle="text-align:center;width:180px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.relative_name"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="25"
                  />
                </template>
              </Column>
              <Column
                field="relationship_id"
                header="Quan hệ"
                headerStyle="text-align:center;width:170px;height:50px"
                bodyStyle="text-align:center;width:170px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="form-group m-0">
                    <Dropdown
                      :showClear="true"
                      :options="dictionarys[11]"
                      optionLabel="relationship_name"
                      optionValue="relationship_id"
                      placeholder="Chọn quan hệ"
                      v-model="slotProps.data.relationship_id"
                      class="ip36"
                      style="
                        white-space: nowrap;
                        overflow: hidden;
                        text-overflow: ellipsis;
                      "
                    />
                  </div>
                </template>
              </Column>
              <Column
                field="birthday"
                header="Năm sinh"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.birthday"
                    :showIcon="false"
                    class="ip36"
                    inputMask="9999"
                    view="year"
                    dateFormat="yy"
                    placeholder="yyyy"
                  />
                </template>
              </Column>
              <Column
                field="phone"
                header="SĐT"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputMask
                    v-model="slotProps.data.phone"
                    mask="9999999999"
                    placeholder="__________"
                    class="ip36"
                  />
                </template>
              </Column>
              <Column
                field="tax_code"
                header="Mã số thuế"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.tax_code"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="50"
                  />
                </template>
              </Column>
              <Column
                field="identification_citizen"
                header="CCCD/Hộ chiếu"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.identification_citizen"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="50"
                  />
                </template>
              </Column>
              <Column
                field="identification_date_issue"
                header="Ngày cấp"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.identification_date_issue"
                    :showIcon="false"
                    class="ip36"
                    placeholder="dd/mm/yyyy"
                  />
                </template>
              </Column>
              <Column
                field="identification_place_issue"
                header="Nơi cấp"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.identification_place_issue"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                    maxLength="250"
                  />
                </template>
              </Column>
              <Column
                field="is_dependent"
                header="Phụ thuộc"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <div class="form-group m-0">
                    <Dropdown
                      :options="[
                        { value: 1, title: 'Có phụ thuộc' },
                        { value: 0, title: 'Không phụ thuộc' },
                      ]"
                      :filter="false"
                      :showClear="true"
                      :editable="false"
                      v-model="slotProps.data.is_dependent"
                      optionLabel="title"
                      optionValue="value"
                      placeholder="Chọn trạng thái"
                      class="ip36"
                      style="
                        white-space: nowrap;
                        overflow: hidden;
                        text-overflow: ellipsis;
                      "
                    >
                      <template #option="slotProps">
                        <div class="country-item flex align-items-center">
                          <div class="pt-1 pl-2">
                            {{ slotProps.option.title }}
                          </div>
                        </div>
                      </template>
                    </Dropdown>
                  </div>
                </template>
              </Column>
              <Column
                field="start_date"
                header="Từ ngày"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.start_date"
                    :showIcon="false"
                    class="ip36"
                    placeholder="dd/mm/yyyy"
                  />
                </template>
              </Column>
              <Column
                field="end_date"
                header="Đến ngày"
                headerStyle="text-align:center;width:120px;height:50px"
                bodyStyle="text-align:center;width:120px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <Calendar
                    v-model="slotProps.data.end_date"
                    :showIcon="false"
                    class="ip36"
                    placeholder="dd/mm/yyyy"
                  />
                </template>
              </Column>
              <Column
                field="info"
                header="Thông tin cơ bản"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.info"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                  />
                </template>
              </Column>
              <Column
                field="note"
                header="Ghi chú"
                headerStyle="text-align:center;width:150px;height:50px"
                bodyStyle="text-align:center;width:150px;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="slotProps">
                  <InputText
                    v-model="slotProps.data.note"
                    spellcheck="false"
                    type="text"
                    class="ip36"
                  />
                </template>
              </Column>
              <template #empty>
                <div
                  class="align-items-center justify-content-center p-4 text-center m-auto"
                  style="display: flex; width: 100%; min-height: 200px"
                ></div>
              </template>
            </DataTable>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button
        v-if="props.isType === 1"
        label="Cập nhật"
        icon="pi pi-save"
        @click="saveModel(true)"
      />
      <Button
        label="Lưu thay đổi"
        icon="pi pi-check"
        @click="saveModel(false)"
      />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
.li-custom:hover {
  background-color: #e9ecef !important;
  color: #495057 !important;
}
.hightlight {
  background-color: #e5f2fc !important;
  color: #000 !important;
}
.li-custom-hover {
  display: none;
}
.li-custom:hover .li-custom-hover {
  display: block;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable) {
  table {
    border-collapse: collapse;
    min-width: 100%;
    table-layout: fixed;
  }
}
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label,
  .p-treeselect .p-treeselect-label {
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
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
::v-deep(.text-right) {
  input {
    text-align: right;
  }
}
::v-deep(.limit-width) {
  .p-multiselect-label {
    white-space: normal !important;
  }
}
::v-deep(.empty-full) {
  .p-datatable-emptymessage td {
    width: 100% !important;
  }
}
</style>
