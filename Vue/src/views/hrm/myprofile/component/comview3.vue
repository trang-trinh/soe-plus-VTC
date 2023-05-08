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
  pageSize: 10000,
  total: 0,
  contract_id: null,
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
const typestatus = ref([
  { value: 0, title: "Chưa hiệu lực", bg_color: "#0078d4", text_color: "#fff" },
  { value: 1, title: "Đang hiệu lực", bg_color: "#5FC57B", text_color: "#fff" },
  { value: 2, title: "Hết hiệu lực", bg_color: "#DF5249", text_color: "#fff" },
  { value: 3, title: "Đã thanh lý", bg_color: "#F39C12", text_color: "#fff" },
]);

const contracts = ref([]);
const dictionarys = ref([]);

//Function
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
const isView = ref(false);
const contract = ref({});
const headerDialogContract = ref();
const displayDialogContract = ref(false);
const openViewDialogContract = (str) => {
  isView.value = true;
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
            proc: "hrm_contract_get",
            par: [{ par: "contract_id", va: options.value.contract_id }],
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
          contract.value = tbs[0][0];
          if (contract.value["profiles"] != null) {
            contract.value["profile"] = JSON.parse(
              contract.value["profiles"]
            )[0];
          }
          if (contract.value["sign_users"] != null) {
            contract.value["sign_user"] = JSON.parse(
              contract.value["sign_users"]
            )[0];
          }
          if (contract.value["start_date"] != null) {
            contract.value["start_date"] = new Date(
              contract.value["start_date"]
            );
          }
          if (contract.value["end_date"] != null) {
            contract.value["end_date"] = new Date(contract.value["end_date"]);
          }
          if (contract.value["sign_date"] != null) {
            contract.value["sign_date"] = new Date(contract.value["sign_date"]);
          }
          if (contract.value["professional_works"] != null) {
            contract.value["professional_works"] = contract.value[
              "professional_works"
            ]
              .split(",")
              .map((x) => parseInt(x));
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          contract.value["allowances"] = tbs[1];
          if (tbs[2] != null && tbs[2].length > 0) {
            var formalitys = tbs[2].filter((x) => x["is_type"] === 0);
            formalitys.forEach((x) => {
              if (x["allowance_formality_id"] == null) {
                x["allowance_formality_id"] = x["allowance_formality"];
              }
            });
            var wages = tbs[2].filter((x) => x["is_type"] === 1);
            wages.forEach((x) => {
              if (x["allowance_wage_id"] == null) {
                x["allowance_wage_id"] = x["allowance_wage"];
              }
            });
            contract.value["allowances"].forEach((allowance) => {
              if (allowance["start_date"] != null) {
                allowance["start_date"] = new Date(allowance["start_date"]);
              }
              allowance.formalitys = formalitys.filter(
                (x) => x["allowance_id"] === allowance["allowance_id"]
              );
              allowance.wages = wages.filter(
                (x) => x["allowance_id"] === allowance["allowance_id"]
              );
            });
          }
        } else {
          contract.value.allowances = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          contract.value["files"] = tbs[3];
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogContract.value = str;
      displayDialogContract.value = true;
      forceRerender(0);
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
const closeDialogContract = () => {
  displayDialogContract.value = false;
  forceRerender(0);
};
const selectRow3 = (event) => {
  if (event && event.data) {
    options.value.contract_id = event.data.contract_id;
    openViewDialogContract("Thông tin hợp đồng");
  }
};

//init
const initView3 = (ref) => {
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
            proc: "hrm_myprofile_get_3",
            par: [{ par: "profile_id", va: props.profile_id }],
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
              item["effect"] = "";
              if (item["sign_date"] != null) {
                item["sign_date"] = moment(new Date(item["sign_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["start_date"] != null) {
                item["start_date"] = moment(
                  new Date(item["start_date"])
                ).format("DD/MM/YYYY");
                item["effect"] += item["sign_date"];
              }
              if (item["end_date"] != null) {
                item["end_date"] = moment(new Date(item["end_date"])).format(
                  "DD/MM/YYYY"
                );
                item["effect"] += "<br/> đến <br/>" + item["sign_date"];
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
onMounted(() => {
  nextTick(() => {
    initView3(true);
  });
});
//page
const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  options.value.pageNo = event.page + 1;
  initView3(true);
};
</script>
<template>
  <div class="surface-100">
    <div class="d-lang-table">
      <DataTable
        @page="onPage($event)"
        @rowSelect="selectRow3"
        :value="contracts"
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
        <Column
          field="contract_code"
          header="Mã HĐ"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
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
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
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
            <span v-html="slotProps.data.start_date"></span>
          </template>
        </Column>
        <Column
          field="start_date"
          header="Ngày hết hạn"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span v-html="slotProps.data.end_date"></span>
          </template>
        </Column>
        <Column
          field="created_date"
          header="Ngày/Người lập"
          headerStyle="text-align:center;max-width:130px;height:50px"
          bodyStyle="text-align:center;max-width:130px;"
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
                :style="{
                  background: bgColor[slotProps.data.created_is_order % 7],
                  color: '#ffffff',
                  width: '2rem',
                  height: '2rem',
                  fontSize: '1rem',
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
            <Button
              :label="slotProps.data.status_name"
              class="p-button-outlined"
              :style="{
                borderColor: slotProps.data.bg_color,
                // backgroundColor: slotProps.data.bg_color,
                color: slotProps.data.bg_color,
                borderRadius: '15px',
                padding: '0.3rem 0.75rem !important',
              }"
            />
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            :style="{
              display: 'flex',
              width: '100%',
              height: 'calc(100vh - 303px)',
              backgroundColor: '#fff',
            }"
          >
            <div v-if="!options.loading && options.total == 0">
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
  <dialogcontract
    :key="componentKey['0']"
    :headerDialog="headerDialogContract"
    :displayDialog="displayDialogContract"
    :closeDialog="closeDialogContract"
    :isView="isView"
    :model="contract"
    :dictionarys="dictionarys"
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
