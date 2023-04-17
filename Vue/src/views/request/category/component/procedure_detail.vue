<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength, integer } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, change_unsigned } from "../../../../util/function.js";
import moment from "moment";
import treeuser from "../../../../components/user/treeuser.vue";

//Khai báo
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const toast = useToast();
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const listDropdownTypeProcess = ref([
  { value: 0, text: "Một trong nhiều" },
  { value: 1, text: "Duyệt lần lượt" },
  { value: 2, text: "Duyệt ngẫu nhiên" },
])
const props = defineProps({
  isShow: Boolean,
  id: String,
  headerShowProcedure: String,
});
const listGroups = ref([]);
const listGroupUsers = ref([]);
const countThanhVien = ref();
const options = ref({
  loading: true,
  SearchText: ''
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const loadData = (rf) => {
  axios
    .post(
      baseUrlCheck + "/api/request/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "request_ca_form_sign_get_list",
            par: [
              { par: "request_form_id", va: props.id },
              { par: "search", va: options.value.SearchText },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[0].forEach((element) => {
        element.type_process_name = listDropdownTypeProcess.value.filter(
          (x) => x.value == element.type_process,
        )[0].text;
        element.listGroupUsers = element.listGroupUsers ? JSON.parse(element.listGroupUsers) : [];
      })
      listGroups.value = data[0];
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}
const goOpen = (model) => {
  model.is_open = !model.is_open;
}
const changeStatus = (model) => {
  let formData = new FormData();
  formData.append("request_form_sign", JSON.stringify(model));
  axios
    .post(
      baseURL +
      "/api/request_ca_form/change_status_request_form_sign",
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật dữ liệu thành công!");
        // props.closeDialog();
        // props.reloadData();
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const edit_request_form_sign = (model) => {
  headerAdd.value = "Cập nhật nhóm duyệt";
  request_form_sign.value = model;
  displayAdd.value = true;
  isAdd.value = false;
}
const closeDialog = () => {
  displayAdd.value = false;
}
const headerAdd = ref();
const displayAdd = ref(false);
const request_form_sign = ref({
  request_form_sign_id: "",
  request_team_id: null,
  request_form_id: props.id,
  group_name: null,
  type_process: 0,
  is_skip_offline: null,
  is_type_group: null,
  is_skip_group: null,
  is_order: listGroups.value.length + 1,
  status: null,
});
const rules = {
  group_name: {
    required,
    maxLength: maxLength(500),
    $errors: [
      {
        $property: "group_name",
        $validator: "required",
        $message: "Tên nhóm không được để trống!",
      },
    ],
  },
};
const v$ = useVuelidate(rules, request_form_sign);
const listTypeApproved = ref([
  { type: "Duyệt 1 trong nhiều", value: 0 },
  { type: "Duyệt lần lượt", value: 1 },
  { type: "Duyệt ngẫu nhiên", value: 2 },
]);
const listTypeGroup = ref([
  { type: "Bình thường", value: 0 },
  { type: "Trưởng bộ phận duyệt", value: 1 },
])
const listDropdownTypeUser = ref([
  { value: 0, text: 'Người duyệt' },
  { value: 3, text: 'Người theo dõi' },
])
const isAdd = ref(false);
const openDialogAdd = () => {
  headerAdd.value = "Thêm mới nhóm duyệt";
  request_form_sign.value = {
    request_form_sign_id: "-1",
    request_team_id: null,
    request_form_id: props.id,
    group_name: null,
    type_process: 0,
    is_skip_offline: null,
    is_type_group: null,
    is_skip_group: null,
    is_order: listGroups.value.length + 1,
    status: true,
  };
  displayAdd.value = true;
  isAdd.value = true;
}
const groupBy = (list, props) => {
  return list.reduce((a, b) => {
    (a[b[props]] = a[b[props]] || []).push(b);
    return a;
  }, {});
};
const submitted = ref(false);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  let formData = new FormData();
  formData.append("request_form_sign", JSON.stringify(request_form_sign.value));
  axios
    .post(
      baseURL +
      "/api/request_ca_form/" +
      (isAdd.value == true ? "add_request_ca_form_sign" : "update_request_ca_form_sign"),
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật nhóm duyệt thành công!");
        displayAdd.value = false;
        loadData(true);
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const saveDataSelect = () => {
  let formData = new FormData();
  formData.append("request_form_sign_user", JSON.stringify(listSelectUsers.value));
  formData.append("request_form_sign_id", JSON.stringify(selectRequestFormSignID.value));
  axios
    .post(
      baseURL +
      "/api/request_ca_form/update_request_ca_form_sign_user",
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật nhân sự nhóm duyệt thành công!");
        displaySelectUser.value = false;
        loadData(true);
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const headerSelectUser = ref();
const displaySelectUser = ref(false);
const listSelectUsers = ref([]);
const selectRequestFormSignID = ref();
const selectUser = (model) => {
  axios
    .post(
      baseUrlCheck + "/api/request/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "request_ca_form_sign_user_get_list",
            par: [
              { par: "request_form_sign_id", va: model.request_form_sign_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      selectRequestFormSignID.value = model.request_form_sign_id;
      listSelectUsers.value = data[0];
      headerSelectUser.value = "Cập nhật nhân sự nhóm duyệt";
      displaySelectUser.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}
const closeDialogSelectUser = () => {
  {
    displaySelectUser.value = false;
  }
}
const displayDialogUser = ref(false);
const selectedUser = ref([]);
const headerDialogUser = ref();
const is_one = ref(false);
const is_type = ref();
const indexSelect = ref();
const OpenDialogTreeUser = (one, index) => {
  indexSelect.value = index;
  selectedUser.value = [];
  if (listSelectUsers.value.length > 0) {
    listSelectUsers.value.forEach((e) => {
      let u = { user_id: e.user_id };
      selectedUser.value.push(u);
    })
  }
  headerDialogUser.value = "Chọn người duyệt";
  displayDialogUser.value = true;
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceTreeUser = () => {
  if (selectedUser.value.length > 0) {
    selectedUser.value.forEach(function (u, j) {
      let user = { request_form_sign_user_id: "-1", request_form_sign_id: null, user_id: u.user_id, full_name: u.full_name, avatar: u.avatar, last_name: u.last_name, created_by: null, created_date: null, created_ip: null, created_token_id: null, STT: listSelectUsers.value.length + 1, status: true };
      listSelectUsers.value.push(user);
    })
  }
  displayDialogUser.value = false;
};
const del_request_form_sign = (model) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm duyệt này không!",
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
        axios
          .delete(
            baseURL + "/api/request_ca_form/delete_request_ca_form_sign",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: [model.request_form_sign_id],
            },
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm duyệt thành công!");
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
}
const del_request_form_sign_user = (model, idx) => {
  if (model.request_form_sign_user_id == "-1") {
    listSelectUsers.value.splice(idx, 1);
  } else {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá nhân sự nhóm duyệt này không!",
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
          axios
            .delete(
              baseURL + "/api/request_ca_form/delete_request_ca_form_sign_user",
              {
                headers: { Authorization: `Bearer ${store.getters.token}` },
                data: [model.request_form_sign_user_id],
              },
            )
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá nhân sự nhóm duyệt thành công!");
                listSelectUsers.value.splice(idx, 1);
              } else {
                swal.fire({
                  title: "Thông báo!",
                  text: response.data.ms,
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            })
            .catch((error) => {
              swal.close();
              if (error.status === 401) {
                swal.fire({
                  title: "Thông báo!",
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  }
}
const refreshData = () => {
  options.value = {
    loading: true,
    SearchText: ''
  };
  loadData(true);
}
onMounted(() => {
  loadData(true);
  return {
    listGroups,
    listGroupUsers,
    options,
    loadData,
  };
});
</script>
<template>
  <div class="overflow-hidden h-full w-full col-md-12 p-0 m-0" style="padding-top: 10px !important;">
    <div class="col-md-12 flex">
      <div class="col-4 p-0 m-0">
        <Button label="Thêm mới" icon="pi pi-plus" @click="openDialogAdd()" autofocus />
      </div>
      <div class="col-4 p-0 m-0">
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText type="text" spellcheck="false" v-model="options.SearchText" placeholder="Tìm kiếm"
            @keyup.enter="loadData(true)" style="min-width:30rem;" />
        </span>
      </div>
      <div class="col-4 p-0 m-0">
        <Button @click="refreshData()" label="Tải lại" class="mr-2 p-button-outlined p-button-secondary" style="float: right;"
          icon="pi pi-refresh" autofocus/>
      </div>
    </div>
    <div class="col-md-12 flex"
      style="justify-content: center;background-color: #fff;border-bottom: 1px solid #aaa;margin-top: 10px;">
      <h3>{{ props.headerShowProcedure }}</h3>
    </div>
    <div class="col-md-12 flex">
      <div class="col-3"
        style="background-color: #fff;min-height: calc(100vh - 100px);max-height: calc(100vh - 100px);padding: 0px;">
        <div class="col-12" style="background-color: #3498db;color: #fff;display: flex;align-items: center;height: 40px;">
          Quy trình nhóm duyệt
        </div>
        <div class="col-12 p-0">
          <ul style="padding: 0px;margin: 0px;">
            <li v-for="(l, index) in listGroups" style="list-style: none;border-bottom: 1px solid #fff;">
              <div
                style="background-color: #33C9DC !important;height: 40px;color: #fff;display: flex;align-items: center;padding-left: 10px;">
                <i v-tooltip.bottom="'Show user'" class="i-hover-click" @click="goOpen(l)"
                  style="margin-right:10px;font-size: 17px;"
                  :class="(l.is_open ? 'pi pi-angle-down' : 'pi pi-angle-right')"></i>
                <label style="font-weight: bold;margin-right: 5px;">Nhóm {{ index + 1 }}:</label>
                {{ l.group_name }}
              </div>
              <ul class="user-list" v-if="l.is_open">
                <li v-for="(u, index) in l.listGroupUsers">
                  <div style="display: flex;align-items: center;">
                    <span style="margin: 0px;margin-right: 10px;font-weight: bold;font-size: 14px;">{{ index + 1 }}</span>
                    <Avatar v-tooltip.bottom="{
                      value:
                        u.full_name +
                        '<br/>' +
                        (u.tenChucVu || '') +
                        '<br/>' +
                        (u.tenToChuc || ''),
                      escape: true,
                    }" v-bind:label="
  u.avatar
    ? ''
    : (u.last_name ?? '').substring(0, 1)
" v-bind:image="basedomainURL + u.avatar" style="
                                                                                                      background-color: #2196f3;
                                                                                                      color: #ffffff;
                                                                                                      width: 2.5rem;
                                                                                                      height: 2.5rem;
                                                                                                    font-size: 15px !important;
                                                                                                  " :style="{
                                                                                                    background: bgColor[index] + '!important',
                                                                                                  }"
                      class="cursor-pointer" size="xlarge" shape="circle" />
                    <span style="font-size: 14px;">{{ u.full_name }}</span>
                  </div>
                </li>
              </ul>
            </li>
          </ul>
        </div>
      </div>
      <div class="col-9"
        style="background-color: #fff;margin-left: 5px;min-height: calc(100vh - 100px);max-height: calc(100vh - 100px);">
        <DataTable class="table-ca-request" :value="listGroups" :paginator="false" :scrollable="true"
          scrollDirection="both" scrollHeight="flex" :lazy="true" dataKey="request_form_sign_id" :rowHover="true">
          <Column field="group_name" header="Nhóm phê duyệt"
            headerStyle="width:15rem;height:50px;border-left:none;border-right:none;"
            bodyStyle="height:50px;;width:15rem;border-left:none;border-right:none;position:relative">
          </Column>
          <Column field="type_process_name" header="Kiểu duyệt"
            headerStyle="text-align:center;width:11rem;height:50px;border-left:none;border-right:none;"
            bodyStyle="text-align:center;height:50px;;width:11rem;border-left:none;border-right:none;position:relative"
            class="align-items-center justify-content-center text-center">
          </Column>
          <Column field="" header="Ngày tạo"
            headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
            bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
            class="align-items-center justify-content-center text-center">
            <template #body="data">
              <div style="
                                                                                              background-color: #fff8ee;
                                                                                              padding: 10px 20px;
                                                                                              border-radius: 5px;
                                                                                            ">
                <span style="color: #ffab2b; font-size: 13px; font-weight: bold">{{ moment(new
                  Date(data.data.created_date)).format("DD/MM/YYYY") }}
                </span>
              </div>
            </template>
          </Column>
          <Column field="" header="Thành viên"
            headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
            bodyStyle="text-align:center;height:50px;;width:15rem;border-left:none;border-right:none;position:relative"
            class="align-items-center justify-content-center text-center">
            <template #body="data">
              <span class="span-thanh-vien" @click="selectUser(data.data)"
                style="height: 35px; width: 35px; border: 1px solid; border-radius: 50%;display: flex;justify-content: center;align-items: center;background-color: #689F38;color: #fff;">{{
                  data.data.listGroupUsers.length }}</span>
            </template>
          </Column>
          <Column field="is_required" header="Trạng thái"
            headerStyle="text-align:center;width:7rem;height:50px;border-left:none;border-right:none;"
            bodyStyle="text-align:center;height:50px;;width:7rem;border-left:none;border-right:none;position:relative"
            class="align-items-center justify-content-center text-center">
            <template #body="data">
              <Checkbox disabled :binary="true" v-model="data.data.status" @click="changeStatus(data.data)" />
            </template>
          </Column>
          <Column field="" header=""
            headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
            bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
            class="align-items-center justify-content-center text-center">
            <template #body="Tem">
              <div v-if="
                store.state.user.is_super == true || store.state.user.user_id == Tem.data.created_by ||
                (store.state.user.role_id == 'admin' && store.state.user.organization_id == Tem.data.organization_id)
              ">
                <Button @click="edit_request_form_sign(Tem.data)" class="
                                                                      															p-button-rounded
                                                                      															p-button-secondary
                                                                      															p-button-outlined
                                                                      															mx-1
                                                                      														" type="button" icon="pi pi-pencil"
                  v-tooltip.top="'Sửa'"></Button>
                <Button class="
                                                                      															p-button-rounded
                                                                      															p-button-danger
                                                                      															p-button-outlined
                                                                      															mx-1
                                                                      														" type="button" icon="pi pi-trash"
                  @click="del_request_form_sign(Tem.data)" v-tooltip.top="'Xóa'"></Button>
              </div>
            </template>
          </Column>
          <template #empty>
            <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                                                                                                  display: flex;
                                                                                                                  flex-direction: column;
                                                                                                                "
              v-if="listGroups.length == 0">
              <img src="../../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
        </DataTable>
      </div>
    </div>
  </div>

  <Dialog :header="headerAdd" v-model:visible="displayAdd" :style="{ width: '45vw', 'z-index': '1000', }"
    :showCloseIcon="true" :modal="true">
    <form @submit.prevent="">
      <div class="grid formgrid m-0">
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
          <div class="col-3 text-left flex p-0" style="align-items:center;">
            Tên nhóm <span class="redsao pl-1"> (*)</span>
          </div>
          <Textarea v-model="request_form_sign.group_name" spellcheck="false" class="col-9 ip36 p-2" autoResize autofocus
            rows="1" :class="{ 'p-invalid': v$.group_name.$invalid && submitted, }" />
        </div>
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
          <div class="col-3 text-left flex p-0" style="align-items:center;">
            Kiểu duyệt
          </div>
          <Dropdown :options="listTypeApproved" :filter="true" :showClear="true" :editable="false"
            v-model="request_form_sign.type_process" optionLabel="type" optionValue="value" placeholder="Chọn kiểu duyệt"
            class="col-9 ip36">
          </Dropdown>
        </div>
        <div class="col-12 field md:col-12 flex p-0" style="margin-bottom: 0px;">
          <div class="field col-6 md:col-6 p-0 align-items-center flex">
            <div class="col-6 text-left p-0" style="align-items:center;">
              Loại duyệt
            </div>
            <div class="col-6 text-left p-0" style="align-items:center;">
              <Dropdown :options="listTypeGroup" :filter="true" :showClear="true" :editable="false"
                v-model="request_form_sign.is_type_group" optionLabel="type" optionValue="value"
                placeholder="Chọn kiểu duyệt" class="col-12 ip36">
              </Dropdown>
            </div>
          </div>
          <div class="field col-6 md:col-6 p-0 align-items-center flex">
            <div class="col-6 text-center p-0">Thứ tự</div>
            <InputNumber v-model="request_form_sign.is_order" :disabled="true" class="col-6 ip36 p-0" />
          </div>
        </div>
        <div class="col-12 field md:col-12 flex p-0" style="margin-bottom: 0px;">
          <div class="field col-6 md:col-6 p-0 align-items-center flex">
            <div class="col-6 text-left p-0" style="align-items:center;">
              Skip nhân sự Offline
            </div>
            <div class="col-6 text-left p-0" style="position: relative;">
              <InputSwitch class="col-12" style="position: absolute; top: 0px; left: 0px"
                v-model="request_form_sign.is_skip_offline" />
            </div>
          </div>
          <div class="field col-6 md:col-6 p-0 align-items-center flex" style="position: relative;">
            <div class="col-6 text-center p-0">Kích hoạt</div>
            <div class="col-6 text-left p-0" style="position: relative;">
              <InputSwitch class="col-12" style="position: absolute; top: 0px; left: 0px"
                v-model="request_form_sign.status" />
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog()" class="p-button-outlined" />

      <Button label="Lưu" icon="pi pi-check" @click="saveData(!v$.$invalid)" autofocus />
    </template>
  </Dialog>

  <Dialog :header="headerSelectUser" v-model:visible="displaySelectUser" :style="{ width: '50vw', 'z-index': '1000', }"
    :showCloseIcon="true" :modal="true">
    <form @submit.prevent="">
      <div class="grid formgrid m-0">
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
          <Button label="Chọn nhân sự" icon="pi pi-users" @click="OpenDialogTreeUser()" autofocus />
        </div>
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
          <DataTable class="table-ca-request-form-sign-user" :value="listSelectUsers" :paginator="false"
            :scrollable="true" scrollDirection="both" scrollHeight="flex" :lazy="true" dataKey="request_form_sign_user_id"
            :rowHover="true">
            <Column field="" header="Nhân sự" headerStyle="width:15rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="height:50px;;width:15rem;border-left:none;border-right:none;position:relative">
              <template #body="data">
                <div style="display: flex;align-items: center;">
                  <Avatar v-tooltip.bottom="{
                    value:
                      data.data.full_name +
                      '<br/>' +
                      (data.data.tenChucVu || '') +
                      '<br/>' +
                      (data.data.tenToChuc || ''),
                    escape: true,
                  }" v-bind:label="
  data.data.avatar
    ? ''
    : (data.data.last_name ?? '').substring(0, 1)
" v-bind:image="basedomainURL + data.data.avatar" style="
                                                                                                      background-color: #2196f3;
                                                                                                      color: #ffffff;
                                                                                                      width: 2.5rem;
                                                                                                      height: 2.5rem;
                                                                                                    font-size: 15px !important;
                                                                                                  " :style="{
                                                                                                    background: bgColor[data.index] + '!important',
                                                                                                  }"
                    class="cursor-pointer" size="xlarge" shape="circle" />
                  <span style="font-size: 14px;margin-left: 10px;">{{ data.data.full_name }}</span>
                </div>
              </template>
            </Column>
            <Column field="IsSLA" header="Số giờ tối đa"
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputNumber v-model="data.data.IsSLA" style="text-align: center;" class="col-12 ip36 p-0" />
              </template>
            </Column>
            <Column field="IsType" header="Vai trò"
              headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:15rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Dropdown :options="listDropdownTypeUser" :filter="true" :showClear="true" :editable="false"
                  v-model="data.data.IsType" optionLabel="text" optionValue="value" placeholder="Chọn kiểu duyệt"
                  class="col-12 ip36">
                </Dropdown>
              </template>
            </Column>
            <Column field="status" header="Trạng thái"
              headerStyle="text-align:center;width:7rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:7rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <Checkbox disabled :binary="true" v-model="data.data.status"/>
              </template>
            </Column>
            <Column field="" header=""
              headerStyle="text-align:center;width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;height:50px;;width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="Tem">
                <div v-if="
                  store.state.user.is_super == true || store.state.user.user_id == Tem.data.created_by ||
                  (store.state.user.role_id == 'admin' && store.state.user.organization_id == Tem.data.organization_id)
                ">
                  <Button class="
                                                                      															p-button-rounded
                                                                      															p-button-danger
                                                                      															p-button-outlined
                                                                      															mx-1
                                                                      														" type="button" icon="pi pi-trash"
                    @click="del_request_form_sign_user(Tem.data, Tem.index)" v-tooltip.top="'Xóa'"></Button>
                </div>
              </template>
            </Column>
            <template #empty>
              <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                                                                                                  display: flex;
                                                                                                                  flex-direction: column;
                                                                                                                "
                v-if="listSelectUsers.length == 0">
                <img src="../../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialogSelectUser()" class="p-button-outlined" />
      <Button label="Lưu" icon="pi pi-check" @click="saveDataSelect()" autofocus />
    </template>
  </Dialog>
  <treeuser v-if="displayDialogUser === true" :headerDialog="headerDialogUser" :displayDialog="displayDialogUser"
    :one="is_one" :selected="selectedUser" :closeDialog="closeDialogUser" :choiceUser="choiceTreeUser" />
</template>
<style lang="scss" scoped>
.i-hover-click:hover {
  cursor: pointer;
}

.user-list {
  padding: 10px 20px;
  margin: 0px;
}

.user-list li {
  display: flex;
  align-items: center;
  list-style: none;
  padding: 10px;
  border: 1px solid gainsboro;
  border-radius: 40px;
  height: 40px;
  background-color: gainsboro;
  font-size: 14px;
  margin-bottom: 10px;
}

.user-list li span {
  margin-left: 10px;
}

::v-deep(.table-ca-request) {
  .p-datatable-emptymessage {
    justify-content: center;
  }
}

::v-deep(.table-ca-request) {
  .span-thanh-vien:hover {
    cursor: pointer;
  }
}

::v-deep(.table-ca-request-form-sign-user) {
  .p-inputnumber-input {
    text-align: center;
  }

  .p-datatable-emptymessage {
    justify-content: center;
  }
}
</style>