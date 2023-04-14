<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength, integer } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, change_unsigned } from "../../../../util/function.js";
import moment from "moment";

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
      })
      listGroups.value = data[0];
      var listgroupby = groupBy(data[1], "request_form_sign_id");
      var arrNew = [];
      for (let k in listgroupby) {
        var requestGroup = [];
        listgroupby[k].forEach(function (r) {
          requestGroup.push(r);
        });
        arrNew.push({
          is_open: true,
          request_form_sign_id: k,
          group_name: data[1].filter(x => x.request_form_sign_id == k)[0].group_name,
          requestGroup: requestGroup,
        });
      }
      listGroupUsers.value = arrNew;
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
const isAdd = ref(false);
const openDialogAdd = () => {
  headerAdd.value = "Thêm mới nhóm duyệt";
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
        <Button @click="refreshData()" class="mr-2 p-button-outlined p-button-secondary" style="float: right;"
          icon="pi pi-refresh" v-tooltip="'Tải lại'" />
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
            <li v-for="(l, index) in listGroupUsers" style="list-style: none;">
              <div
                style="background-color: #33C9DC !important;height: 40px;color: #fff;display: flex;align-items: center;padding-left: 10px;">
                <i v-tooltip.bottom="'Show user'" class="i-hover-click" @click="goOpen(l)"
                  style="margin-right:10px;font-size: 17px;"
                  :class="(l.is_open ? 'pi pi-angle-down' : 'pi pi-angle-right')"></i>
                  <label style="font-weight: bold;margin-right: 5px;">Nhóm {{ index + 1 }}:</label>
                 {{ l.group_name }}
              </div>
              <ul class="user-list" v-if="l.is_open">
                <li v-for="(u, index) in l.requestGroup">
                  <div>
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
                                                                  }" class="cursor-pointer" size="xlarge"
                      shape="circle" />
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
              <span
                style="height: 35px; width: 35px; border: 1px solid; border-radius: 50%;display: flex;justify-content: center;align-items: center;background-color: #689F38;color: #fff;">{{
                  listGroupUsers.length }}</span>
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
                                      														" type="button" icon="pi pi-pencil" v-tooltip.top="'Sửa'"></Button>
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
                                                                                " v-if="listGroups.length == 0">
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
</style>