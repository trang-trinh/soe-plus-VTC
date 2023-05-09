<script setup>
import { onMounted, ref, inject, nextTick } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
import dialogcontract from "../../contract/component/dialogcontract.vue";

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

//Get arguments
const props = defineProps({
  profile_id: String,
});

//Declare
const options = ref({
  loading: true,
  pageNo: 1,
  pageSize: 25,
  total: 0,
  training_emps: {},
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

//Function
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
const statusTrannings = ref([
  { name: "Lên kế hoạch", code: 1 },
  { name: "Đang thực hiện", code: 2 },
  { name: "Đã hoàn thành", code: 3 },
  { name: "Tạm dừng", code: 4 },
  { name: "Đã hủy", code: 5 },
]);
const typeTrannings = ref([
  { name: "Cấp lãnh đạo", code: 1 },
  { name: "Quản lý", code: 2 },
  { name: "Nhân viên", code: 3 },
]);
const formTrannings = ref([
  { name: "Bắt buộc", code: 1 },
  { name: "Đăng ký", code: 2 },
  { name: "Cả hai", code: 3 },
]);
const trannings = ref([]);
const tranning = ref({});
const headerDialogTranning = ref();
const displayDialogTranning = ref(false);
const openViewDialogTranning = (str) => {
  headerDialogTranning.value = str;
  displayDialogTranning.value = true;
  forceRerender(0);
};
const closeDialogTranning = () => {
  displayDialogTranning.value = false;
  forceRerender(0);
};
const selectRow7 = (event) => {
  if (event && event.data) {
    options.value.training_emps = event.data;
    openViewDialogTranning("Thông tin khóa đào tạo");
  }
};

//init
const initView7 = (rf) => {
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
            proc: "hrm_myprofile_get_7",
            par: [
              { par: "profile_id", va: props.profile_id },
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
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((element) => {
            if (element["li_user_verify"]) {
              element["li_user_verify"] = JSON.parse(element["li_user_verify"]);
            } else {
              element["li_user_verify"] = [];
            }
            if (element["start_date"] != null) {
              element["start_date"] = moment(
                new Date(element["start_date"])
              ).format("DD/MM/YYYY");
            }
            if (element["end_date"] != null) {
              element["end_date"] = moment(
                new Date(element["end_date"])
              ).format("DD/MM/YYYY");
            }
          });
          trannings.value = tbs[0];
          if (tbs[1] != null && tbs[1].length > 0) {
            options.value.total = tbs[1][0].total;
          }
        } else {
          trannings.value = [];
          options.value.total = 0;
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
onMounted(() => {
  nextTick(() => {
    initView7(true);
  });
});
//page
const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  options.value.pageNo = event.page + 1;
  initView7(true);
};
</script>
<template>
  <div class="surface-100">
    <div class="d-lang-table">
      <DataTable
        @page="onPage($event)"
        @rowSelect="selectRow7"
        :value="trannings"
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
        dataKey="training_emps_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
      >
        <Column
          field="training_emps_code"
          header="Mã số"
          headerStyle="text-align:center;max-width:80px;height:50px"
          bodyStyle="text-align:center;max-width:80px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="training_emps_name"
          header="Tên khóa đào tạo"
          headerStyle="text-align:center;max-width:250px;height:50px"
          bodyStyle="text-align:center;max-width:250px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="form_training_name"
          header="Hình thức"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div>
              {{
                slotProps.data.form_training == 1
                  ? "Bắt buộc"
                  : slotProps.data.form_training == 2
                  ? "Đăng ký"
                  : "Cả hai"
              }}
            </div>
          </template>
        </Column>
        <Column
          field="start_date"
          header="Từ ngày"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="end_date"
          header="Đến ngày"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="li_user_verify"
          header="Giảng viên"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <AvatarGroup
              v-if="
                slotProps.data.li_user_verify &&
                slotProps.data.li_user_verify.length > 0
              "
            >
              <Avatar
                v-for="(item, index) in slotProps.data.li_user_verify.slice(
                  0,
                  3
                )"
                v-bind:label="item.avatar ? '' : item.last_name.substring(0, 1)"
                v-bind:image="
                  item.avatar
                    ? basedomainURL + item.avatar
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
                v-tooltip.top="item.full_name"
                :key="item.user_id"
                style="color: white"
                @click="onTaskUserFilter(item)"
                @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                size="large"
                shape="circle"
                class="cursor-pointer"
                :style="{ backgroundColor: bgColor[index % 7] }"
              />
              <Avatar
                v-if="
                  slotProps.data.li_user_verify &&
                  slotProps.data.li_user_verify.length > 3
                "
                v-bind:label="
                  '+' + (slotProps.data.li_user_verify.length - 3).toString()
                "
                shape="circle"
                size="large"
                style="background-color: #2196f3; color: #ffffff"
                class="cursor-pointer"
              />
            </AvatarGroup>
          </template>
        </Column>
        <Column
          field="count_emps"
          header="Học viên"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <div>
              {{ data.data.count_emps ? data.data.count_emps : "0" }}
            </div>
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:11rem;height:50px"
          bodyStyle="text-align:center;max-width:11rem"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <Button
              :label="
                slotProps.data.status == 1
                  ? 'Lên kế hoạch'
                  : slotProps.data.status == 2
                  ? 'Đang thực hiện'
                  : slotProps.data.status == 3
                  ? 'Đã hoàn thành'
                  : slotProps.data.status == 4
                  ? 'Tạm dừng'
                  : 'Đã hủy'
              "
              :class="
                slotProps.data.status == 1
                  ? 'bg-blue-500'
                  : slotProps.data.status == 2
                  ? 'bg-yellow-500'
                  : slotProps.data.status == 3
                  ? 'bg-green-500'
                  : slotProps.data.status == 4
                  ? 'bg-orange-500'
                  : 'bg-pink-500'
              "
              class="px-2 w-10rem"
            />
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 332px);
              background-color: #fff;
            "
          >
            <div v-if="!options.loading && options.total === 0">
              <img
                src="../../../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
  <!--Dialog-->
  <dialogtraining
    :key="componentKey['0']"
    :headerDialog="headerDialogTranning"
    :displayBasic="displayDialogTranning"
    :training_emps="options.training_emps"
    :checkadd="false"
    :closeDialog="closeDialogTranning"
    :view="true"
  />
</template>
<style scoped>
@import url(../../contract/component/stylehrm.css);
.d-lang-table {
  height: calc(100vh - 220px);
  overflow-y: auto;
  background-color: #fff;
}
.box-info .card {
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
.box-info .card-header {
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
.box-info .card-body {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
}
</style>
<style lang="scss" scoped>
::v-deep(.border-radius) {
  img {
    border-radius: 50%;
  }
}
</style>
<style lang="scss" scoped>
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
::v-deep(.p-timeline) {
  background-color: #fff;
  padding: 1rem;
  .p-timeline-event .p-timeline-event-opposite {
    display: none !important;
  }
  .p-timeline-event:nth-child(even) {
    flex-direction: row;
  }
  .p-card-body {
    padding: 0;
  }
}
</style>
