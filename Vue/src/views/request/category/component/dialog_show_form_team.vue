<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength, integer } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, change_unsigned } from "../../../../util/function.js";
//import moment from "moment";
//import treeuser from "../../../../components/user/treeuser.vue";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const options = ref({
  IsNext: true,
  sort: "modified_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
  IsType: 0,
  SearchTextUser: "",
  filter_type: 0,
  sdate: null,
  edate: null,
  loctitle: "Lọc",
  type_view: 2,
  type_group_view: null,
  filter_date: null,
  filter_duan: null,
  filter_taskgroup: null,
  searchTeamUse: "",
});
const headerSelectTeam = ref();
const displayDialogSelectTeam = ref();
const bgColor = ref([
  "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
const props = defineProps({
  dataForm: Object,
  isSave: Boolean,
  key: Number,
  id: String,
  headerDialog: String,
  displayDialog: Boolean,
  listTeamUses: Object,
  closeDialog1: Function,
});
const listTeams = ref([]);
const listTeamUses = ref(props.listTeamUses);
const selectedTeamDatas = ref();
const loadDataTeam = (rf) => {
  axios
    .post(
      baseUrlCheck + "/api/request/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "request_ca_team_list",
            par: [
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "user_id", va: store.getters.user.user_id, },
              { par: "status", va: null },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        element.checked = false;
      });
      listTeams.value = data.filter(x => props.listTeamUses.findIndex(y => y.request_team_id == x.request_team_id) < 0);
      options.value.loading = false;
      displayDialogSelectTeam.value = true;
      headerSelectTeam.value = "Chọn Team sử dụng";
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const selectTeams = () => {
  loadDataTeam(true);
}
const ChoseTeam = () => {
  let arr = [];
  selectedTeamDatas.value.forEach((d) => {
    d.IsSLA = 1;
    d.IsMail = true;
    d.IsNoty = true;
    d.IsSkip = false;
    d.IsChangeQT = false;
    d.request_team_name_en = change_unsigned(d.request_team_name);
    listTeamUses.value.push(d);
  })
  displayDialogSelectTeam.value = false;
  emitter.emit('listTeamUses', listTeamUses.value);
}
const listSearchTeamUse = () => {
  if (keySearch.value.trim() != "") {
    return listTeamUses.value.filter(x => x.request_team_name_en.includes(change_unsigned(keySearch.value)));
  }
  return listTeamUses.value;
};
const keySearch = ref("");
const searchTeamUse = () => {
  if (options.value.searchTeamUse == null || options.value.searchTeamUse.trim() == "") {
    keySearch.value = "";
  }
  else {
    keySearch.value = options.value.searchTeamUse;
  }
}
const loadDataTeamUse = (rf) => {
  axios
    .post(
      baseUrlCheck + "/api/request/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "request_ca_form_team_get_list",
            par: [
              { par: "request_form_id", va: props.id ? props.id : null },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        element.checked = false;
      });
      listTeamUses.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const closeDialogSelectTeam = () => {
  displayDialogSelectTeam.value = false;
}
const delTeamUse = (model) => {
  if (!model.request_form_team_id) {
    listTeamUses.value.splice(model, 1);
    listTeamUses.value.forEach((e, i) => {
      e.STT = i + 1;
    })
  } else {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá thiết lập team swue dụng này không!",
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
            .delete(baseURL + "/api/request_ca_form/delete_request_ca_form_team", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: [model.request_form_team_id],
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá thiết lập team sử dụng thành công!");
                listTeamUses.value.splice(model, 1);
                listTeamUses.value.forEach((e, i) => {
                  e.STT = i + 1;
                })
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
                  text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  }
}
const saveData = () => {
	let formData = new FormData();
	formData.append("request_form", JSON.stringify(props.dataForm));
  formData.append("request_ca_from_team", JSON.stringify(listTeamUses.value));
	axios
		.post(
			baseURL +
			"/api/request_ca_form/update_request_ca_from_team",
			formData,
			config,
		)
		.then((response) => {
			if (response.data.err != "1") {
				swal.close();
				toast.success("Cập nhật team sử dụng cho đề xuất thành công!");
				props.closeDialog1();
			}
		})
		.catch((res) => {
      debugger
			swal.close();
			swal.fire({
				title: "Error!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		});
}
onMounted(() => {
  //loadDataTeamUse(true);
  return {};
});
</script>
<template>
  <Dialog :header="props.headerDialog" :visible="props.displayDialog" :style="{ width: '70vw' }" :showCloseIcon="true"
    position="top" @update:visible="props.closeDialog1()" modal>
    <form @submit.prevent="">
      <div class="grid formgrid m-0">
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
          <div class="col-6 text-left flex p-0" style="align-items:center;">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText type="text" spellcheck="false" v-model="options.searchTeamUse" placeholder="Tìm kiếm"
                @keyup.enter="searchTeamUse()" style="min-width:30rem;" />
            </span>
          </div>
          <div class="col-6 text-left flex p-0" style="align-items:center;justify-content: end;">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <Button @click="selectTeams('Chọn team')" label="Chọn team" icon="pi pi-users" class="mr-2" />
            </span>
          </div>
        </div>
        <div class="field col-12 md:col-12 algn-items-center p-0">
          <DataTable class="table-ca-request" :value="listSearchTeamUse()" :paginator="false" :scrollable="true"
            scrollHeight="flex" :lazy="true" dataKey="request_team_id" :rowHover="true">
            <Column field="STT" header="STT"
              headerStyle="text-align:center;max-width:5rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;max-width:5rem;border-left:none;border-right:none;"
              class="align-items-center justify-content-center text-center" />
            <Column field="request_team_name" header="Tên team" headerStyle="text-align:left;height:50px"
              bodyStyle="text-align:left">
            </Column>
            <Column field="" header="Số giờ xử lý"
              headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;max-width:10rem;border-left:none;border-right:none;"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputNumber v-model="data.data.IsSLA" style="padding: 0px !important;text-align: center !important;"
                  class="col-9 ip36 px-2" />
              </template>
            </Column>
            <Column field="" header="Cho phép đổi quy trình khi chạy"
              headerStyle="text-align:center;max-width:20rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;max-width:20rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputSwitch style="position: absolute;" v-model="data.data.IsChangeQT" />
              </template>
            </Column>
            <Column field="" header="Cho phép chuyển vượt cấp"
              headerStyle="text-align:center;max-width:20rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;max-width:20rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputSwitch style="position: absolute;" v-model="data.data.IsSkip" />
              </template>
            </Column>
            <Column field="" header="Thông báo Noty"
              headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;max-width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputSwitch style="position: absolute;" v-model="data.data.IsNoty" />
              </template>
            </Column>
            <Column field="" header="Thông báo Mail"
              headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;max-width:10rem;border-left:none;border-right:none;position:relative"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <InputSwitch style="position: absolute;" v-model="data.data.IsMail" />
              </template>
            </Column>
            <Column field="" header="Chức năng"
              headerStyle="text-align:center;height:50px;max-width:10rem;border-left:none;border-right:none;"
              bodyStyle="max-width:10rem;border-left:none;border-right:none;"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <div>
                  <Button class="
                          p-button-rounded
                          p-button-danger
                          p-button-outlined
                          mx-1
                        " type="button" icon="pi pi-trash" @click="delTeamUse(data.data)" v-tooltip.top="'Xóa'"></Button>
                </div>
              </template>
            </Column>`
            <template #empty>
              <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                  display: flex;
                  flex-direction: column;
                " v-if="listTeamUses.length == 0">
                <img src="../../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Đề xuất chưa có team</h3>
              </div>
            </template>
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Đóng" icon="pi pi-times" @click="props.closeDialog1" class="p-button-outlined" />
      <Button label="Lưu" v-if="props.isSave" icon="pi pi-check" @click="saveData()" autofocus />
    </template>
  </Dialog>

  <Dialog :header="headerSelectTeam" v-model:visible="displayDialogSelectTeam" :style="{ width: '40vw' }" :closable="true"
    position="top" :modal="true">
    <form @submit.prevent="">
      <div class="grid formgrid m-0">
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
          <div class="col-6 text-left flex p-0" style="align-items:center;">
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText type="text" spellcheck="false" placeholder="Tìm kiếm" style="min-width:20rem;" />
            </span>
          </div>
        </div>
        <div class="field col-12 md:col-12 algn-items-center p-0">
          <DataTable class="table-ca-request" :value="listTeams" :paginator="false" :scrollable="true" scrollHeight="flex"
            :lazy="true" dataKey="request_team_id" :rowHover="true" v-model:selection="selectedTeamDatas">
            <Column selectionMode="multiple" headerStyle="text-align:center;max-width:4rem;"
              bodyStyle="text-align:center;max-width:4rem;" class="align-items-center justify-content-center text-center">
            </Column>
            <Column field="request_team_name" header="Tên team" headerStyle="text-align:left;height:50px"
              bodyStyle="text-align:left">
            </Column>
            <Column field="" header="Trạng thái"
              headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
              bodyStyle="text-align:center;max-width:10rem;border-left:none;border-right:none;"
              class="align-items-center justify-content-center text-center">
              <template #body="data">
                <span :style="(data.data.status
                  ? 'background-color: #2196f3 ;border: 1px solid #2196f3;'
                  : 'background-color: red ;border: 1px solid red' + ';')"
                  style="padding: 5px; color: #fff;border-radius: 5px;">{{ data.data.status ? "Kích hoạt" : "Khóa"
                  }}</span>
              </template>
            </Column>
            <template #empty>
              <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                  display: flex;
                  flex-direction: column;
                " v-if="listTeams.length == 0">
                <img src="../../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialogSelectTeam()" class="p-button-outlined" />
      <Button label="Chọn" icon="pi pi-check" @click="ChoseTeam()" autofocus />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
::v-deep(.p-datatable-tbody) {
  .p-inputtext {
    text-align: center;
  }
}
</style>