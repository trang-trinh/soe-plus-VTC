<script setup>
import { ref, inject, onMounted, onBeforeUnmount, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { encr } from "../../util/function.js";
import moment from "moment";

const cryoptojs = inject("cryptojs");
const first = ref(0);
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const emitter = inject("emitter");
const basedomainURL = fileURL;

const props = defineProps({
  isShow: Boolean,
  id: String,
  turn: Intl,
});

const rules_discuss = {
  discuss_project_content: {
    required,
    $errors: [
      {
        $property: "discuss_project_content",
        $validator: "required",
        $message: "Nội dung thảo luận không được để trống!",
      },
    ],
  },
};
const discussProject = ref({
  discuss_project_id: null,
  project_id: props.id,
  discuss_project_content: null,
  is_public: true,
  start_date: new Date(),
  end_date: null,
  is_order: null,
  members: [],
})
const v3$ = useVuelidate(rules_discuss, discussProject);

const opition = ref({
  type_chart: 1,
  PageNo: 0,
  PageSize: 20,
  sort: "created_date",
  ob: "DESC",
  totalRecords: 0,
});
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const listProjectMainDiscuss = ref([]);
const headerAddDiscuss = ref();
const displayDiscuss = ref(false);
const issaveDiscuss = ref(false);
const listDropdownMembers = ref([]);
const DiscussMembers = ref([]);
const submitted = ref(false);

const loadData = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_get_list_discuss",
            par: [
              { par: "project_id", va: props.id },
              // { par: "ob", va: opition.value.ob },
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
      data[0].forEach(function (t, i) {
        t.STT = i + 1;
        t.Thanhviens = t.Thanhviens
          ? JSON.parse(t.Thanhviens)
          : [];
        t.ThanhvienShows = [];
        if (t.Thanhviens.length > 3) {
          t.ThanhvienShows = t.Thanhviens.slice(0, 3);
        } else {
          t.ThanhvienShows = [...t.Thanhviens];
        }
      })
      listProjectMainDiscuss.value = data[0];
      opition.value.totalRecords = data[0].length;
      listDropdownMembers.value = data[1].map((x) => ({
        name: x.full_name,
        code: x.user_id,
        avatar: x.avatar,
        ten: x.last_name,
      }));
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}

const isAddDiscuss = ref(false);

const addDiscuss = (str) => {
  isAddDiscuss.value = true;
  submitted.value = false;
  discussProject.value = {
    discuss_project_id: -1,
    project_id: props.id,
    discuss_project_content: null,
    is_public: true,
    start_date: new Date(),
    end_date: null,
    is_order: listProjectMainDiscuss.value.length + 1,
    members: [],
  };
  issaveDiscuss.value = false;
  headerAddDiscuss.value = str;
  displayDiscuss.value = true;
}

const DelDiscuss = (model) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thảo luận dự án này không!",
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
        var listId = [];
        axios
          .delete(baseURL + "/api/ProjectMain/Delete_DiscussProjectMain", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: model != null ? [model.discuss_project_id] : listId,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thảo luận dự án thành công!");
              loadListDiscuss();
            } else {
              swal.fire({
                title: "Thông báo!",
                html: response.data.ms,
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

const editDiscuss = (model) => {
  isAddDiscuss.value = false;
  issaveDiscuss.value = false;
  headerAddDiscuss.value = "Sửa thảo luận";
  displayDiscuss.value = true;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "discuss_project_get_edit",
            par: [{ par: "discuss_project_id", va: model.discuss_project_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[0].forEach((element, i) => {
        element.Thanhviens = element.Thanhviens
          ? JSON.parse(element.Thanhviens)
          : [];
        element.members = [];
      });
      discussProject.value = data[0][0];
      discussProject.value.start_date = discussProject.value.start_date
        ? new Date(discussProject.value.start_date)
        : null;
      discussProject.value.end_date = discussProject.value.end_date
        ? new Date(discussProject.value.end_date)
        : null;
      if (discussProject.value.Thanhviens.length > 0) {
        discussProject.value.Thanhviens.forEach(function (t) {
          discussProject.value.members.push(t.user_id);
        })
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}

const saveDiscussProjectMain = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  discussProject.value.start_date = discussProject.value.start_date ? new Date(discussProject.value.start_date) : null;
  discussProject.value.end_date = discussProject.value.end_date ? new Date(discussProject.value.end_date) : null;
  let formData = new FormData();
  if (discussProject.value.members.length > 0) {
    discussProject.value.members.forEach((t) => {
      let member = {
        discuss_member_id: -1,
        discuss_project_id: null,
        user_id: t,
        status: true,
      };
      member.user_id = t;
      DiscussMembers.value.push(member);
    });
  }
  formData.append("discussProject", JSON.stringify(discussProject.value));
  formData.append("discussMember", JSON.stringify(DiscussMembers.value));
  axios
    .post(
      baseURL +
      "/api/ProjectMain/" + (isAddDiscuss.value ? 'Add_DiscussProjectMain' : 'Update_DiscussProjectMain'),
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật thảo luận dự án thành công!");
        loadData();
        displayDiscuss.value = false;
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: 'Error!',
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}

onMounted(() => {
  loadData();
  return {};
});
</script>
<template>
  <div class="tab-project-content h-full w-full col-md-12 p-0 m-0 flex">
    <div class="col-6 p-0 m-0 tab-project-content-left">
      <div class="row">
        <div class="col-12" style="border-bottom: 1px solid #aaa; font-weight: 600;padding: 10px;">
          <Button label="Tạo thảo luận" icon="pi pi-plus" class="mr-2" @click="addDiscuss('Thêm mới thảo luận')" />
        </div>
        <div class="col-12">
          <DataTable id="projectmain-thaoluan" v-model:first="first" :rowHover="true" :value="listProjectMainDiscuss"
            :paginator="true" :rows="opition.PageSize"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[1, 20, 30, 50, 100, 200]" :scrollable="true" scrollHeight="flex"
            :totalRecords="opition.totalRecords" :row-hover="true" dataKey="project_id"
            v-model:selection="selectedProjectMains" @page="onPage($event)" @sort="onSort($event)"
            @filter="onFilter($event)" :lazy="true" selectionMode="single">
            <Column field="STT" header="STT" class="align-items-center justify-content-center text-center font-bold"
              headerStyle="text-align:center;max-width:4rem" bodyStyle="text-align:center;max-width:4rem">
            </Column>
            <Column field="" header="Người tạo" class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px">
              <template #body="md">
                <Avatar v-tooltip.bottom="{
                  value:
                    md.data.full_name +
                    '<br/>' +
                    (md.data.tenChucVu || '') +
                    '<br/>' +
                    (md.data.tenToChuc || ''),
                  escape: true,
                }" v-bind:label="
  md.data.avatar ? '' : (md.data.last_name ?? '').substring(0, 1)
" v-bind:image="basedomainURL + md.data.avatar" style="
                                                                      background-color: #2196f3;
                                                                      color: #ffffff;
                                                                      width: 32px;
                                                                      height: 32px;
                                                                      font-size: 15px !important;
                                                                      margin-left: -10px;
                                                                  " :style="{
                                                                    background: bgColor[index % 7] + '!important',
                                                                  }" class="cursor-pointer" size="xlarge"
                  shape="circle" />
              </template>
            </Column>
            <Column field="" header="Nội dung thảo luận" headerStyle="max-width:auto;">
              <template #body="md">
                <div style="display: flex; align-items: center">
                  <span style="margin-left: 5px">{{
                    md.data.discuss_project_content
                  }}</span>
                </div>
              </template>
            </Column>
            <Column field="" header="" class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px">
              <template #body="data">
                <div v-if="data.data.is_public == true">
                  <span
                    style="border: #2196f3;background-color: #2196f3;color: #ffffff;padding: 5px;border-radius: 5px;">is
                    public</span>
                </div>
                <div v-if="data.data.is_public == false">
                  <AvatarGroup>
                    <div v-for="(value, index) in data.data.ThanhvienShows" :key="index">
                      <div>
                        <Avatar v-tooltip.bottom="{
                          value:
                            value.fullName +
                            '<br/>' +
                            (value.tenChucVu || '') +
                            '<br/>' +
                            (value.tenToChuc || ''),
                          escape: true,
                        }" v-bind:label="
  value.avatar ? '' : (value.ten ?? '').substring(0, 1)
" v-bind:image="basedomainURL + value.avatar" style="
                                                                      background-color: #2196f3;
                                                                      color: #ffffff;
                                                                      width: 32px;
                                                                      height: 32px;
                                                                      font-size: 15px !important;
                                                                      margin-left: -10px;
                                                                  " :style="{
                                                                    background: bgColor[index % 7] + '!important',
                                                                  }" class="cursor-pointer" size="xlarge"
                          shape="circle" />
                      </div>
                    </div>
                    <Avatar v-if="
                      data.data.Thanhviens.length - data.data.ThanhvienShows.length >
                      0
                    " :label="
  '+' +
  (data.data.Thanhviens.length -
    data.data.ThanhvienShows.length) +
  ''
" class="cursor-pointer" shape="circle" style="
                                                                  background-color: #e9e9e9 !important;
                                                                  color: #98a9bc;
                                                                  font-size: 14px !important;
                                                                  width: 32px;
                                                                  margin-left: -10px;
                                                                  height: 32px;
                                                              " />
                  </AvatarGroup>
                </div>
              </template>
            </Column>
            <Column class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;min-height:3.125rem;max-width:100px;"
              bodyStyle="text-align:center;max-width:100px;">
              <template #body="data">
                <div v-if="
                  store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  data.data.isEdit == true ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id == data.data.organization_id)
                ">
                  <Button @click="editDiscuss(data.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1" type="button" icon="pi pi-pencil"
                    v-tooltip="'Sửa'"></Button>
                  <Button @click="DelDiscuss(data.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1" type="button" icon="pi pi-trash"
                    v-tooltip="'Xóa'"></Button>
                </div>
              </template>
            </Column>
            <template #empty>
              <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                                        min-height: calc(100vh - 215px);
                                                        max-height: calc(100vh - 215px);
                                                        display: flex;
                                                        flex-direction: column;
                                                    " v-if="listProjectMainDiscuss != null">
                <img src="../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataTable>
        </div>
      </div>
    </div>
    <div class="col-6 p-0 m-0 tab-project-content-right">
      <div class="row" style="padding: 15px; font-size: 13px;">

      </div>
    </div>
  </div>

  <Dialog :header="headerAddDiscuss" v-model:visible="displayDiscuss" :style="{ width: '40vw' }" :closable="true"
    :maximizable="true">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">Nội dung thảo luận<span class="redsao"> (*) </span></label>
          <InputText v-model="discussProject.discuss_project_content" spellcheck="false" class="col-9 ip36 px-2"
            :class="{ 'p-invalid': v3$.discuss_project_content.$invalid && submitted }" />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-3 text-left"></div>
          <small v-if="
            (v3$.discuss_project_content.$invalid && submitted) ||
            v3$.discuss_project_content.$pending.$response
          " class="col-9 p-error p-0">
            <span class="col-12 p-0">{{
              v3$.discuss_project_content.required.$message
                .replace("Value", "Nội dung thảo luận")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12" style="display: flex; align-items: center">
          <label class="col-3 text-left p-0">Ngày bắt đầu</label>
          <div class="col-9" style="display: flex; padding: 0px; align-items: center">
            <Calendar :manualInput="true" :showIcon="true" :showTime="true" class="col-5 ip36 title-lable"
              style="margin-top: 5px; padding: 0px" id="time1" autocomplete="on" v-model="discussProject.start_date" />
            <div class="col-7" style="display: flex; padding: 0px; align-items: center">
              <label class="col-5 text-center">Ngày kết thúc</label>
              <Calendar :manualInput="true" :showTime="true" :showIcon="true" class="col-7 ip36 title-lable"
                style="margin-top: 5px; padding: 0px" id="time2" placeholder="dd/MM/yy HH:mm" autocomplete="on"
                v-model="discussProject.end_date" @date-select="CheckDate($event)" />
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">STT</label>
          <InputNumber v-model="discussProject.is_order" style="padding: 0px !important" class="col-9 ip36 px-2" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-3 text-left p-0">Public thảo luận</label>
          <div class="col-9" style="position: relative;">
            <InputSwitch class="col-12" style="position: absolute; top: 0px; left: 0px"
              v-model="discussProject.is_public" />
          </div>
        </div>
        <div class="field col-12 md:col-12" v-if="discussProject.is_public == false">
          <label class="col-3 text-left p-0">Người tham gia
            <span @click="OpenDialogTreeUser(false, 1)" class="choose-user"><i class="pi pi-user-plus"></i></span></label>
          <MultiSelect :filter="true" v-model="discussProject.members" :options="listDropdownMembers" optionValue="code"
            optionLabel="name" class="col-9 ip36 p-0" placeholder="Người tham gia" display="chip">
            <template #option="slotProps">
              <div class="country-item flex" style="align-items: center; margin-left: 10px">
                <Avatar v-bind:label="
                  slotProps.option.avatar
                    ? ''
                    : (slotProps.option.name ?? '').substring(0, 1)
                " v-bind:image="basedomainURL + slotProps.option.avatar" style="
                                                  background-color: #2196f3;
                                                  color: #ffffff;
                                                  width: 32px;
                                                  height: 32px;
                                                  font-size: 15px !important;
                                                  margin-left: -10px;
                                                " :style="{
                                                  background: bgColor[slotProps.index % 7] + '!important',
                                                }" class="cursor-pointer" size="xlarge" shape="circle" />
                <div class="pt-1" style="padding-left: 10px">
                  {{ slotProps.option.name }}
                </div>
              </div>
            </template>
          </MultiSelect>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialogProjectMain" class="p-button-text" />
      <Button label="Lưu" icon="pi pi-check" @click="saveDiscussProjectMain(!v3$.discuss_project_content.$invalid)" />
    </template>
  </Dialog>
</template>
<style scoped>
.tab-project-content {
  height: calc(100vh - 50px) !important;
  background-color: #f3f3f3;
}

.tab-project-content-left {
  background-color: #fff;
  margin: 5px 5px 0px 0px !important;
  height: 100%;
}

.tab-project-content-right {
  background-color: #fff;
  margin: 5px 0px 5px 5px !important;
  height: 100%;
}

#projectmain-thaoluan {
  max-height: calc(100vh - 110px);
  min-height: calc(100vh - 110px);
}
</style>